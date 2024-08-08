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
﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Interneuron.Common.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using SynapseStudioWeb.DataService;
using SynapseStudioWeb.DataService.APIModel;
using SynapseStudioWeb.Helpers;
using SynapseStudioWeb.Models;

namespace SynapseStudioWeb.Controllers
{
    public partial class FormularyController : Controller
    {
        private const string SUCCESS_IMPORT_MSG = "Successfully Imported the records.";

        [HttpPost]
        [Route("Formulary/LoadDMDList")]
        public async Task<JsonResult> LoadDMDList(string searchTxt)
        {
            string token = HttpContext.Session.GetString("access_token");

            var dmdResponseTask = TerminologyAPIService.SearchDMDNamesGetWithAllLevelNodes(searchTxt, token);// TerminologyAPIService.SearchDMDWithAllDescendents(searchTxt, token);

            var dmdResponse = await dmdResponseTask;

            if (dmdResponse.StatusCode != DataService.APIModel.StatusCode.Success)
            {
                var errors = "Error getting the DMD data.";

                if (dmdResponse.ErrorMessages.IsCollectionValid())
                    errors = errors + string.Join('\n', dmdResponse.ErrorMessages);

                _toastNotification.AddErrorToastMessage(errors);

                return Json(null);
            }

            var dmds = dmdResponse.Data;

            if (dmds == null || !dmds.Data.IsCollectionValid()) return Json(null);

            var orderedDataList = dmds.Data.OrderBy(rec => rec.Name).ToList();

            var formularyList = FillFormularyTreeModel(orderedDataList);

            return Json(formularyList);
        }

        [HttpPost]
        [Route("Formulary/LoadSyncPendingDMDList")]
        public async Task<JsonResult> LoadSyncPendingDMDList(string searchTxt)
        {
            string token = HttpContext.Session.GetString("access_token");

            var dmdResponseTask = TerminologyAPIService.SearchDMDSyncLog(searchTxt, token);

            var dmdResponse = await dmdResponseTask;

            if (dmdResponse.StatusCode != DataService.APIModel.StatusCode.Success)
            {
                var errors = "Error getting the DMD data.";

                if (dmdResponse.ErrorMessages.IsCollectionValid())
                    errors = errors + string.Join('\n', dmdResponse.ErrorMessages);

                _toastNotification.AddErrorToastMessage(errors);

                return Json(null);
            }

            var dmds = dmdResponse.Data;

            if (dmds == null || !dmds.Data.IsCollectionValid()) return Json(null);

            var orderedDataList = dmds.Data.OrderBy(rec => rec.Name).ToList();

            var formularyList = FillFormularyTreeModel(orderedDataList);

            return Json(formularyList);
        }

        private List<FormularyTreeModel> FillFormularyTreeModel(List<DMDSearchResultWithTree> nodes)
        {
            var formularyNodes = new List<FormularyTreeModel>();

            foreach (DMDSearchResultWithTree node in nodes)
            {
                var formularyNode = new FormularyTreeModel();
                formularyNode.Data["Level"] = node.Level;
                formularyNode.Key = node.Code;
                formularyNode.Title = node.Name;

                if (node.Children.IsCollectionValid())
                    formularyNode.Children = FillFormularyTreeModel(node.Children);

                formularyNodes.Add(formularyNode);
            }
            return formularyNodes;
        }

        private async Task<bool> HasRunningBackgroundTask(string token, List<string> taskNames = null)
        {
            taskNames = taskNames ?? new List<string> { "dmdfileupload", "importdmdtoformulary" };
            var getTaskByNameResponse = await TerminologyAPIService.GetTaskByNames(taskNames, token);

            var hasRunningTasks = getTaskByNameResponse?.Data?.Any(rec => rec.StatusCd == 1 || rec.StatusCd == 2);

            return hasRunningTasks == true;
        }

        private async Task<bool> IsRecordsUpdateInProgress(string token)
        {
            var hasAnyUpdateInProgessResponse = await TerminologyAPIService.HasAnyUpdateInProgess(token);

            var hasAnyUpdateInProgess = (hasAnyUpdateInProgessResponse != null && hasAnyUpdateInProgessResponse.Data == true);

            return hasAnyUpdateInProgess == true;
        }

        [HttpPost]
        [Route("Formulary/ImportMeds")]
        public async Task<IActionResult> ImportMeds(List<string> meds)
        {
            if (!meds.IsCollectionValid())
            {
                _toastNotification.AddErrorToastMessage("No input");
                return Json(null);
            }
            string token = HttpContext.Session.GetString("access_token");

            if (await HasRunningBackgroundTask(token))
            {
                const string IMPORT_IN_PROGRESS_MSG = "DM+D data import is in progress. Please try after some time.";
                _toastNotification.AddErrorToastMessage(IMPORT_IN_PROGRESS_MSG);
                return Json(null);
            }
            if (await IsRecordsUpdateInProgress(token))
            {
                const string UPDATE_IN_PROGRESS_MSG = "Update is in progress. Please try after some time.";
                _toastNotification.AddErrorToastMessage(UPDATE_IN_PROGRESS_MSG);
                return Json(null);
            }

            //MMC-477
            var jObj = JObject.FromObject(new { seq = 1, stepname = "initializeforimport", codes = string.Join("|", meds) });
            JArray arr = new() { jObj };
            var request = new BackgroundTaskAPIModel { Name = "importdmdtoformulary", Status = "initializeforimport", StatusCd = 1, Detail = arr.ToString() };
            var resultResponse = await TerminologyAPIService.CreateTerminologyBGTask(request, token);

            if (resultResponse == null)
            {
                _toastNotification.AddErrorToastMessage(UNKNOWN_SAVE_STATUS_MSG);
                return Json(null);
            }

            if (resultResponse.StatusCode != DataService.APIModel.StatusCode.Success)
            {
                string errors = "Error Importing the data.";

                if (resultResponse.ErrorMessages.IsCollectionValid())
                    errors += string.Join('\n', resultResponse.ErrorMessages);
                _toastNotification.AddErrorToastMessage(errors);
                return Json(null);
            }

            const string SUCCESS_BG_IMPORTMSG = "This is a long running process. Please wait...";
            _toastNotification.AddSuccessToastMessage(SUCCESS_BG_IMPORTMSG);

            return Json(resultResponse.Data.TaskId);//Just to indicate the client

            /*
             * This part has been moved to the background service now
            var resultResponse = await TerminologyAPIService.ImportMeds(meds, token);

            if (resultResponse == null)
            {
                _toastNotification.AddErrorToastMessage(UNKNOWN_SAVE_STATUS_MSG);
                return Json(null);
            }

            if (resultResponse.StatusCode != DataService.APIModel.StatusCode.Success)
            {
                string errors = "Error Importing the data.";

                if (resultResponse.ErrorMessages.IsCollectionValid())
                    errors += string.Join('\n', resultResponse.ErrorMessages);
                _toastNotification.AddErrorToastMessage(errors);
                return Json(null);
            }

            if (resultResponse.StatusCode == DataService.APIModel.StatusCode.Success)
            {
                if (resultResponse.Data.Status != null && resultResponse.Data.Status.ErrorMessages.IsCollectionValid())
                {
                    var errors = string.Join('\n', resultResponse.Data.Status.ErrorMessages);
                    _toastNotification.AddWarningToastMessage(errors);
                    return Json(null);
                }
                else
                {
                    var postProcessImportResponse = await TerminologyAPIService.InvokePostImportProcess(token, meds);

                    if (postProcessImportResponse.StatusCode != DataService.APIModel.StatusCode.Success)
                    {
                        string errors = "Error Completing post import process of the data.";

                        if (postProcessImportResponse.ErrorMessages.IsCollectionValid())
                            errors += string.Join('\n', postProcessImportResponse.ErrorMessages);
                        _toastNotification.AddErrorToastMessage(errors);
                        return Json(null);
                    }
                }
            }

            _toastNotification.AddSuccessToastMessage(SUCCESS_IMPORT_MSG);

            return Json(new List<string> { "Success" });//Just to indicate the client
            */
        }

        [HttpGet]
        [Route("Formulary/GetLatestFormulariesHeaderOnly")]
        public async Task<JsonResult> GetLatestFormulariesHeaderOnly()
        {
            string token = HttpContext.Session.GetString("access_token");

            var response = await TerminologyAPIService.GetLatestFormulariesHeaderOnly(token);

            if (response.StatusCode != DataService.APIModel.StatusCode.Success)
            {
                var errors = "Error getting the DMD data.";

                if (response.ErrorMessages.IsCollectionValid())
                    errors = errors + string.Join('\n', response.ErrorMessages);

                _toastNotification.AddErrorToastMessage(errors);

                return Json(null);
            }

            var dmds = response.Data;

            if (dmds.IsCollectionValid())
            {
                var result = dmds.Distinct(x => x.code).ToHashSet();

                return (Json(result));
            }
            else
            {
                return Json(null);
            }
        }

        [HttpGet]
        [Route("Formulary/GetDMDVersion")]
        public async Task<JsonResult> GetDMDVersion()
        {
            string token = HttpContext.Session.GetString("access_token");

            var response = await TerminologyAPIService.GetDmdSnomedVersion(token);

            if (response.StatusCode != DataService.APIModel.StatusCode.Success)
            {
                var errors = "Error while getting the DMD SNOMED version data.";

                if (response.ErrorMessages.IsCollectionValid())
                    errors = errors + string.Join('\n', response.ErrorMessages);

                _toastNotification.AddErrorToastMessage(errors);

                return Json(null);
            }

            var dmdSnomedVersion = response.Data;

            if (dmdSnomedVersion.IsNotNull())
            {
                var result = dmdSnomedVersion.DmdVersion;

                return (Json(result));
            }
            else
            {
                return Json(null);
            }
        }

        [HttpGet]
        [Route("Formulary/GetDMDSNOMEDVersion")]
        public async Task<JsonResult> GetDMDSNOMEDVersion()
        {
            string token = HttpContext.Session.GetString("access_token");

            var response = await TerminologyAPIService.GetDmdSnomedVersion(token);

            if (response.StatusCode != DataService.APIModel.StatusCode.Success)
            {
                var errors = "Error while getting the DMD SNOMED version data.";

                if (response.ErrorMessages.IsCollectionValid())
                    errors = errors + string.Join('\n', response.ErrorMessages);

                _toastNotification.AddErrorToastMessage(errors);

                return Json(null);
            }

            var dmdSnomedVersion = response.Data;

            if (dmdSnomedVersion.IsNotNull())
            {
                var result = dmdSnomedVersion;

                return (Json(result));
            }
            else
            {
                return Json(null);
            }
        }



        [HttpGet]
        [Route("Formulary/GetHistoryOfFormularies")]
        public async Task<JsonResult> GetHistoryOfFormularies(HistoryOfFormulariesRequest request)
        {
            string token = HttpContext.Session.GetString("access_token");
            if (request == null)
                request = new HistoryOfFormulariesRequest() { PageNo = 1, PageSize = 10 };

            var requestParams = new List<KeyValuePair<string, string>>()
            { new KeyValuePair<string, string>("pageSize", request.PageSize.ToString()), new KeyValuePair<string, string>("pageNo", request.PageNo.ToString()), new KeyValuePair<string, string>("needTotalRecords", request.NeedTotalRecords.ToString()) };

            if (request.FilterParams.IsCollectionValid())
            {
                var filtersVal = new List<KeyValuePair<string, string>>();
                foreach (var filter in request.FilterParams)
                {
                    var item = Newtonsoft.Json.JsonConvert.DeserializeObject<KeyValuePair<string, string>>(filter);
                    filtersVal.Add(new KeyValuePair<string, string>(item.Key, item.Value));
                }
                requestParams.Add(new KeyValuePair<string, string>("filterParams", Newtonsoft.Json.JsonConvert.SerializeObject(filtersVal)));
            }

            var response = await TerminologyAPIService.GetHistoryOfFormularies(token, requestParams);

            if (response.StatusCode != DataService.APIModel.StatusCode.Success)
            {
                var errors = "Error while getting the history of formularies data.";

                if (response.ErrorMessages.IsCollectionValid())
                    errors = errors + string.Join('\n', response.ErrorMessages);

                _toastNotification.AddErrorToastMessage(errors);

                return Json(null);
            }

            var formulariesHistory = response.Data;

            if (formulariesHistory == null) return Json(null);

            var result = formulariesHistory;

            return (Json(result));
        }
    }
}
