 //Interneuron synapse

//Copyright(C) 2024 Interneuron Limited

//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.

//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.

//See the
//GNU General Public License for more details.

//You should have received a copy of the GNU General Public License
//along with this program.If not, see<http://www.gnu.org/licenses/>.
﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SynapseStudioWeb.DataService;
using SynapseStudioWeb.DataService.APIModel;
using NToastNotify;
using Microsoft.AspNetCore.Mvc.Rendering;
using Interneuron.Common.Extensions;
using SynapseStudioWeb.Helpers;
using SynapseStudioWeb.Models.MedicinalMgmt;
using AutoMapper;
using SynapseStudioWeb.AppCode;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SynapseStudioWeb.Controllers
{
    public partial class FormularyController : Controller
    {
        private const string UNKNOWN_SAVE_STATUS_MSG = "Unknown error saving the status.";
        private const string STATUS_SUCCESS_MSG = "Successfully changed the record status to {0} in the system.";
        private const string UNKNOWN_GET_DATA_MSG = "Unknown error getting the data from the data store.";
        private readonly IMapper _mapper;
        private readonly IToastNotification _toastNotification;
        //private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _provider;
        private readonly IHttpContextAccessor _contextAccessor;
        private FormularyLookupLoaderService _formularyLookupService;
        private readonly ILogger<FormularyController> _logger;

        public FormularyController(IMapper mapper, IToastNotification toastNotification, IServiceProvider provider,
            IConfiguration configuration, ILogger<FormularyController> logger, IHttpContextAccessor httpContextAccessor)
        {
            _mapper = mapper;
            _toastNotification = toastNotification;
            //_hostingEnvironment = hostingEnvironment;
            _configuration = configuration;

            _provider = provider;
            _contextAccessor = httpContextAccessor;// _provider.GetService<IHttpContextAccessor>();

            _formularyLookupService = new FormularyLookupLoaderService(_contextAccessor, _configuration);
            _logger = logger;

            //commenting - as it is resulting in session expire
            //Task.Run(async () => await LoadLookupData());
        }

        private async Task LoadLookupData()
        {
            var isLookupExists = HttpContext.Session.GetObject<bool>(SynapseSession.IsFormularyLookupExists);

            if (!isLookupExists)
                await _formularyLookupService.LoadLookUps();
        }


        public async Task<IActionResult> FormularyList()
        {
            await LoadLookupData();
            string token = HttpContext.Session.GetString("access_token");

            var recordStatusDictionary = HttpContext.Session.GetObject<Dictionary<string, string>>(SynapseSession.FormularyRecStatusLkpKey);
            var formularyStatusDictionary = HttpContext.Session.GetObject<Dictionary<string, string>>(SynapseSession.FormularyStatusLkpKey);

            if (!formularyStatusDictionary.IsCollectionValid() || !recordStatusDictionary.IsCollectionValid())
            {
                var recordStatusLkpTask = TerminologyAPIService.GetRecordStatusLookup(token);

                var formularyStatusLkpTask = TerminologyAPIService.GetFormularyStatusLookup(token);

                await Task.WhenAll(recordStatusLkpTask, formularyStatusLkpTask);

                var recordStatusLkp = await recordStatusLkpTask;

                if (recordStatusLkp.IsCollectionValid())
                {
                    recordStatusDictionary = recordStatusLkp.Where(rec => rec.Recordstatus == 1).ToDictionary(k => k.Cd.ToString(), v => v.Desc);

                    HttpContext.Session.SetObject(SynapseSession.FormularyRecStatusLkpKey, recordStatusDictionary);
                }

                var formularyStatusLkp = await formularyStatusLkpTask;

                if (formularyStatusLkp.IsCollectionValid())
                {
                    formularyStatusDictionary = formularyStatusLkp.Where(rec => rec.Recordstatus == 1).ToDictionary(k => k.Cd.ToString(), v => v.Desc);

                    HttpContext.Session.SetObject(SynapseSession.FormularyStatusLkpKey, formularyStatusDictionary);
                }
            }

            var vm = new FormularyListFilterModel();

            var statusItems = new List<SelectListItem>();
            if (recordStatusDictionary != null)
            {
                recordStatusDictionary.Keys.Each(K =>
                {
                    statusItems.Add(new SelectListItem() { Value = $"Rec|{K}", Text = recordStatusDictionary[K], Group = new SelectListGroup() { Name = "RecordStatus" } });
                });
            }
            if (formularyStatusDictionary != null)
            {
                formularyStatusDictionary.Keys.Each(K =>
                {
                    statusItems.Add(new SelectListItem() { Value = $"Form|{K}", Text = formularyStatusDictionary[K], Group = new SelectListGroup() { Name = "FormularyStatus" } });
                });
            }
            var possibleFlags = FormularyListSearchCriteria.GetPossibleFlags();

            if (possibleFlags.IsCollectionValid())
            {
                possibleFlags.Keys.Each(K =>
                {
                    statusItems.Add(new SelectListItem() { Value = $"Flags|{K}", Text = possibleFlags[K], Group = new SelectListGroup() { Name = "Flags" } });
                });
            }
            vm.FilterStatuses = statusItems.FindAll(i => i.Value == "Rec|001" || i.Value == "Rec|002" || i.Value == "Rec|003" || i.Value == "Form|001" || i.Value == "Form|002" || (i.Group != null && i.Group.Name == "Flags"));

            vm.SelectedFilterStatuses = new List<string>();

            //var isNew = Request.Query["isnew"];//to be removed
            //if(isNew == "1")
            //{
            //    return View("FormularyList_New", vm);
            //}
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateFormularyStatusInBulk([FromBody] UpdateFormularyStatusAPIRequest request)
        {
            string token = HttpContext.Session.GetString("access_token");
            dynamic response = new { status = "", errors = new List<string>() };

            if (request == null || !request.RequestData.IsCollectionValid())
            {
                const string INVALID_INPUT = "No records passed as input.";

                response.errors.Add(INVALID_INPUT);
                return (Json(response));
            }

            if (await HasRunningBackgroundTask(token))
            {
                const string IMPORT_IN_PROGRESS_MSG = "DM+D data import is in progress. Please try after some time.";
                _toastNotification.AddErrorToastMessage(IMPORT_IN_PROGRESS_MSG);
                response = new { status = "", errors = new List<string>() { IMPORT_IN_PROGRESS_MSG } };
                return (Json(response));
            }

            if (await IsRecordsUpdateInProgress(token))
            {
                const string UPDATE_IN_PROGRESS_MSG = "Update is in progress. Please try after some time.";
                _toastNotification.AddErrorToastMessage(UPDATE_IN_PROGRESS_MSG);
                response = new { status = "", errors = new List<string>() { UPDATE_IN_PROGRESS_MSG } };
                return (Json(response));
            }

            var fvIds = request.RequestData.Select(rec => rec.FormularyVersionId)?.Distinct().ToList();
            var newChangedStatus = request.RequestData[0].RecordStatusCode;

            (bool isValidStatusChange, string errorMsg) = await ValidateFormularyStatusChange(fvIds, newChangedStatus);

            const string UNABLE_TO_VALIDATE = "Unable to validate the records. Unknown Error.";

            if (!isValidStatusChange)
            {
                errorMsg = errorMsg.IsEmpty() ? $"{UNABLE_TO_VALIDATE}|" : errorMsg;
                var msg = string.Join(". ", errorMsg.Split("|"));
                _toastNotification.AddErrorToastMessage(msg);
                response = new { status = "", errors = errorMsg.Split("|") };
                return Json(response);
            }

            var lockResponse = await TerminologyAPIService.GetHeaderRecordsLock(fvIds, token);

            //Need to introduce lock here before
            if (lockResponse == null || !lockResponse.Data)
            {
                var errorMsgForLock = $"This formulary details {string.Join(", ", fvIds)} cannot be saved as it has already been modified and a new version of it exists in the system. Please take latest version and try updating.";
                response = new { status = "", errors = new string[] { errorMsgForLock } };
                return Json(response);
            }

            var updateResponse = await UpdateFormularyStatusInBulkHandler(response, request, newChangedStatus);

            //release lock here
            await TerminologyAPIService.TryReleaseHeaderRecordsLock(fvIds, token);

            return updateResponse;

            #region old code ref only
            /*
            var resultResponse = await TerminologyAPIService.UpdateFormularyStatus(request, token);

            if (resultResponse == null)
            {
                _toastNotification.AddErrorToastMessage(UNKNOWN_SAVE_STATUS_MSG);
                response.errors.Add(UNKNOWN_SAVE_STATUS_MSG);
                response = new { status = "", errors = new List<string>() { UNKNOWN_SAVE_STATUS_MSG } };
                return (Json(response));
            }

            if (resultResponse.StatusCode != DataService.APIModel.StatusCode.Success)
            {
                string errors = "Error updating the status.";

                if (resultResponse.ErrorMessages.IsCollectionValid())
                    errors += string.Join(". ", resultResponse.ErrorMessages);

                _toastNotification.AddErrorToastMessage(errors);
                response = new { status = "", errors = new List<string>() { errors } };
                return Json(response);
            }

            if (resultResponse.StatusCode == DataService.APIModel.StatusCode.Success)
            {
                if (resultResponse.Data.Status != null && resultResponse.Data.Status.ErrorMessages.IsCollectionValid())
                {
                    var error = string.Join(". ", resultResponse.Data.Status.ErrorMessages);

                    _toastNotification.AddInfoToastMessage(error);
                    response = new { status = "", errors = new List<string>() { error } };
                    return Json(response);
                }
            }

            var recordStatuses = await _formularyLookupService.GetRecordStatusLookup();//Let it throw the error if not exist

            var statusName = recordStatuses?[newChangedStatus];
            _toastNotification.AddSuccessToastMessage(STATUS_SUCCESS_MSG.ToFormat(statusName));
            response = new { status = "success", errors = new List<string>() {} };
            return Json(response);
            */
            #endregion
        }

        private async Task<JsonResult> UpdateFormularyStatusInBulkHandler(dynamic response, UpdateFormularyStatusAPIRequest request, string newChangedStatus)
        {
            string token = HttpContext.Session.GetString("access_token");

            var uniqueIds = request.RequestData;

            var batchSize = 500;
            var batchedRequests = new List<UpdateFormularyStatusAPIRequest>();

            for (var reqIndex = 0; reqIndex < uniqueIds.Count; reqIndex += batchSize)
            {
                var batches = uniqueIds.Skip(reqIndex).Take(batchSize).ToList();
                var apiRequest = new UpdateFormularyStatusAPIRequest { RequestData = batches };
                batchedRequests.Add(apiRequest);
            }

            var updateStatusTasks = new List<Task<TerminologyAPIResponse<UpdateFormularyStatusAPIResponse>>>();

            foreach (var batchedRequest in batchedRequests)
            {
                var responseTask = TerminologyAPIService.UpdateFormularyStatus(batchedRequest, token);
                updateStatusTasks.Add(responseTask);
            }

            await Task.WhenAll(updateStatusTasks);

            var allErrors = new List<string>();
            var hasNoResponse = false;

            foreach (var updateStatusTask in updateStatusTasks)
            {
                var resultResponseOfUpdate = await updateStatusTask;

                if (resultResponseOfUpdate == null)
                    hasNoResponse = true;

                if (resultResponseOfUpdate.StatusCode != DataService.APIModel.StatusCode.Success)
                {
                    if (resultResponseOfUpdate.ErrorMessages.IsCollectionValid())
                        allErrors.Add(string.Join(". ", resultResponseOfUpdate.ErrorMessages));
                }

                if (resultResponseOfUpdate.StatusCode == DataService.APIModel.StatusCode.Success)
                {
                    if (resultResponseOfUpdate.Data.Status != null && resultResponseOfUpdate.Data.Status.ErrorMessages.IsCollectionValid())
                    {
                        allErrors.Add(string.Join(". ", resultResponseOfUpdate.Data.Status.ErrorMessages));
                    }
                }
            }

            if (hasNoResponse)
            {
                _toastNotification.AddErrorToastMessage(UNKNOWN_SAVE_STATUS_MSG);
                response.errors.Add(UNKNOWN_SAVE_STATUS_MSG);
                response = new { status = "", errors = new List<string>() { UNKNOWN_SAVE_STATUS_MSG } };
                return (Json(response));
            }
            if (allErrors.IsCollectionValid())
            {
                var respErrorMsg = string.Join(". ", allErrors);
                _toastNotification.AddErrorToastMessage(respErrorMsg);
                response = new { status = "", errors = allErrors };
                return Json(response);
            }

            var recordStatuses = await _formularyLookupService.GetRecordStatusLookup();//Let it throw the error if not exist

            var statusName = recordStatuses?[newChangedStatus];
            _toastNotification.AddSuccessToastMessage(STATUS_SUCCESS_MSG.ToFormat(statusName));
            response = new { status = "success", errors = new List<string>() { } };
            return Json(response);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateFormularyStatus(string formularyVersionId, string reason, string status)
        {
            string token = HttpContext.Session.GetString("access_token");

            UpdateFormularyStatusAPIRequest request = new UpdateFormularyStatusAPIRequest();

            if (await HasRunningBackgroundTask(token))
            {
                const string IMPORT_IN_PROGRESS_MSG = "DM+D data import is in progress. Please try after some time.";
                _toastNotification.AddErrorToastMessage(IMPORT_IN_PROGRESS_MSG);
                return (Json(null));
            }

            if (await IsRecordsUpdateInProgress(token))
            {
                const string UPDATE_IN_PROGRESS_MSG = "Update is in progress. Please try after some time.";
                _toastNotification.AddErrorToastMessage(UPDATE_IN_PROGRESS_MSG);
                return (Json(null));
            }

            request.RequestData = new List<UpdateFormularyStatusAPRequestData>()
            {
                new UpdateFormularyStatusAPRequestData
                {
                    FormularyVersionId = formularyVersionId,
                    RecordStatusCode =status,
                    RecordStatusCodeChangeMsg = reason
                }
            };

            (bool isValidStatusChange, string errorMsg) = await ValidateFormularyStatusChange(new List<string> { formularyVersionId }, status);

            const string UnableToValidate = "Unable to validate the records. Unknown Error.";

            if (!isValidStatusChange)
            {
                errorMsg = errorMsg.IsEmpty() ? $"{UnableToValidate}|" : errorMsg;
                _toastNotification.AddErrorToastMessage(string.Join(". ", errorMsg.Split("|")));
                return (Json(null));
            }

            var resultResponse = await TerminologyAPIService.UpdateFormularyStatus(request, token);

            if (resultResponse == null)
            {
                _toastNotification.AddErrorToastMessage(UNKNOWN_SAVE_STATUS_MSG);
                return (Json(null));
            }

            if (resultResponse.StatusCode != DataService.APIModel.StatusCode.Success)
            {
                string errors = "Error updating the status.";

                if (resultResponse.ErrorMessages.IsCollectionValid())
                    errors += string.Join('\n', resultResponse.ErrorMessages.ToArray());

                _toastNotification.AddErrorToastMessage(errors);

                return Json(null);
            }

            if (resultResponse.StatusCode == DataService.APIModel.StatusCode.Success)
            {
                if (resultResponse.Data.Status != null && resultResponse.Data.Status.ErrorMessages.IsCollectionValid())
                {
                    var errors = string.Join('\n', resultResponse.Data.Status.ErrorMessages);

                    _toastNotification.AddInfoToastMessage(errors);
                    return Json(null);
                }
            }

            var recordStatuses = await _formularyLookupService.GetRecordStatusLookup();//Let it throw the error if not exist

            var statusName = recordStatuses?[status];
            _toastNotification.AddSuccessToastMessage(STATUS_SUCCESS_MSG.ToFormat(statusName));

            return Json(new List<string> { "success" });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateFormularyStatus_New([FromBody] UpdateFormularyStatusAPRequestData updateFormularyStatusAPRequestData)
        {
            string token = HttpContext.Session.GetString("access_token");

            UpdateFormularyStatusAPIRequest request = new UpdateFormularyStatusAPIRequest();

            if (await HasRunningBackgroundTask(token))
            {
                const string IMPORT_IN_PROGRESS_MSG = "DM+D data import is in progress. Please try after some time.";
                _toastNotification.AddErrorToastMessage(IMPORT_IN_PROGRESS_MSG);
                return (Json(null));
            }

            if (await IsRecordsUpdateInProgress(token))
            {
                const string UPDATE_IN_PROGRESS_MSG = "Update is in progress. Please try after some time.";
                _toastNotification.AddErrorToastMessage(UPDATE_IN_PROGRESS_MSG);
                return (Json(null));
            }

            request.RequestData = new List<UpdateFormularyStatusAPRequestData>()
            {
                new UpdateFormularyStatusAPRequestData
                {
                    FormularyVersionId = updateFormularyStatusAPRequestData.FormularyVersionId,
                    RecordStatusCode = updateFormularyStatusAPRequestData.RecordStatusCode,
                    RecordStatusCodeChangeMsg = updateFormularyStatusAPRequestData.RecordStatusCodeChangeMsg
                }
            };

            (bool isValidStatusChange, string errorMsg) = await ValidateFormularyStatusChange(new List<string> { updateFormularyStatusAPRequestData.FormularyVersionId }, updateFormularyStatusAPRequestData.RecordStatusCode);

            const string UnableToValidate = "Unable to validate the records. Unknown Error.";

            if (!isValidStatusChange)
            {
                errorMsg = errorMsg.IsEmpty() ? $"{UnableToValidate}|" : errorMsg;
                _toastNotification.AddErrorToastMessage(string.Join(". ", errorMsg.Split("|")));
                return (Json(null));
            }

            var lockResponse = await TerminologyAPIService.GetHeaderRecordsLock(new List<string> { updateFormularyStatusAPRequestData.FormularyVersionId }, token);

            //Need to introduce lock here before
            if (lockResponse == null || !lockResponse.Data)
            {
                var errorMsgForLock = $"This formulary details ${updateFormularyStatusAPRequestData.FormularyVersionId} cannot be saved as it has already been modified and a new version of it exists in the system. Please take latest version and try updating.";
                _toastNotification.AddErrorToastMessage(errorMsgForLock);
                return (Json(null));
            }

            var resultResponse = await TerminologyAPIService.UpdateFormularyStatus(request, token);

            //release lock here
            await TerminologyAPIService.TryReleaseHeaderRecordsLock(new List<string> { updateFormularyStatusAPRequestData.FormularyVersionId }, token);

            if (resultResponse == null)
            {
                _toastNotification.AddErrorToastMessage(UNKNOWN_SAVE_STATUS_MSG);
                return (Json(null));
            }

            if (resultResponse.StatusCode != DataService.APIModel.StatusCode.Success)
            {
                string errors = "Error updating the status.";

                if (resultResponse.ErrorMessages.IsCollectionValid())
                    errors += string.Join('\n', resultResponse.ErrorMessages.ToArray());

                _toastNotification.AddErrorToastMessage(errors);

                return Json(null);
            }

            if (resultResponse.StatusCode == DataService.APIModel.StatusCode.Success)
            {
                if (resultResponse.Data.Status != null && resultResponse.Data.Status.ErrorMessages.IsCollectionValid())
                {
                    var errors = string.Join('\n', resultResponse.Data.Status.ErrorMessages);

                    _toastNotification.AddInfoToastMessage(errors);
                    return Json(null);
                }
            }

            var recordStatuses = await _formularyLookupService.GetRecordStatusLookup();//Let it throw the error if not exist

            var statusName = recordStatuses?[updateFormularyStatusAPRequestData.RecordStatusCode];
            _toastNotification.AddSuccessToastMessage(STATUS_SUCCESS_MSG.ToFormat(statusName));

            return Json(new List<string> { "success" });
        }

        [HttpPost]
        public async Task<IActionResult> BulkUpdateFormularyStatus(UpdateFormularyStatusAPIRequest request)
        {
            string token = HttpContext.Session.GetString("access_token");

            if (request == null || request.RequestData.IsCollectionValid())
                return Json("No records selected.");

            var resultResponse = await TerminologyAPIService.BulkUpdateFormularyStatus(request, token);

            if (resultResponse == null)
            {
                //_toastNotification.AddErrorToastMessage(UNKNOWN_SAVE_STATUS_MSG);
                return (Json(null));
            }

            if (resultResponse.StatusCode != DataService.APIModel.StatusCode.Success)
            {
                string errors = "";

                if (resultResponse.ErrorMessages.IsCollectionValid())
                    errors += string.Join('\n', resultResponse.ErrorMessages.ToArray());

                //_toastNotification.AddErrorToastMessage(errors);

                return Json(errors);
            }

            if (resultResponse.StatusCode == DataService.APIModel.StatusCode.Success)
            {
                if (resultResponse.Data.Status != null && resultResponse.Data.Status.ErrorMessages.IsCollectionValid())
                {
                    var errors = string.Join('\n', resultResponse.Data.Status.ErrorMessages);

                    //_toastNotification.AddInfoToastMessage(errors);
                    return Json(errors);
                }
            }

            return Json(new List<string> { "success" });
        }
    }
}
