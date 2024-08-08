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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AnyDiff;
using AnyDiff.Extensions;
using Interneuron.Common.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SynapseStudioWeb.AppCode.Constants;
using SynapseStudioWeb.Controllers.MedicationManagement.BulkEditMergeHandlers;
using SynapseStudioWeb.DataService;
using SynapseStudioWeb.DataService.APIModel;
using SynapseStudioWeb.Helpers;
using SynapseStudioWeb.Models.MedicationMgmt;
using SynapseStudioWeb.Models.MedicinalMgmt;
using System.Linq;

namespace SynapseStudioWeb.Controllers
{
    public partial class FormularyController : Controller
    {

        [HttpPost]
        [Route("Formulary/LoadFormularyDetails")]
        public async Task<IActionResult> LoadFormularyDetails(string formularyVersionId)
        {
            const string MMC_CONFIG_KEY = "MMC_Control_Source";

            string token = HttpContext.Session.GetString("access_token");

            var responseTask = TerminologyAPIService.GetFormularyDetailRuleBound(formularyVersionId, true, token);// TerminologyAPIService.GetFormularyDetail(formularyVersionId, token);

            var terminologyConfigTask = TerminologyAPIService.GetTerminologyConfiguration(MMC_CONFIG_KEY, token);

            await Task.WhenAll(responseTask, terminologyConfigTask);

            var response = await responseTask;

            var terminologyConfigResponse = await terminologyConfigTask;

            if ((response == null || response.Data == null || response.StatusCode == DataService.APIModel.StatusCode.Fail) || ((terminologyConfigResponse == null || terminologyConfigResponse.StatusCode == DataService.APIModel.StatusCode.Fail)))
            {
                const string ErrorResponse = "Error getting details for the selected record";
                var errors = "";
                if (response.ErrorMessages.IsCollectionValid())
                {
                    errors = string.Join('\n', ErrorResponse, response.ErrorMessages.ToArray());
                }
                if (terminologyConfigResponse != null && terminologyConfigResponse.ErrorMessages.IsCollectionValid())
                {
                    errors = string.Join('\n', ErrorResponse, terminologyConfigResponse.ErrorMessages.ToArray());
                }
                _toastNotification.AddErrorToastMessage(errors);

                return null;
            }

            var formulariesfromAPI = response.Data;

            await FillLookupsToViewBag(formulariesfromAPI.RecStatusCode);

            FormularyEditModel vm = GetFormularyEditModel(formulariesfromAPI);

            if (string.Compare(vm.CodeSystem, "Custom", true) != 0)
            {
                var identificationTypes = (await _formularyLookupService.GetIdentificationCodeTypes());

                if (identificationTypes.IsCollectionValid())
                {
                    ViewBag.IndentificationCodeTypes = identificationTypes.Where(x => x.Value != TerminologyConstants.PRIMARY_IDENTIFICATION_CODE_TYPE)?.ToDictionary(k => k.Key, v => v.Value).ToSelectList(TerminologyConstants.DEFAULT_DROPDOWN_TEXT);
                }
            }

            return PartialView(GetFormularyEditView(formulariesfromAPI.ProductType), vm);
        }

        [HttpPost]
        [Route("Formulary/LoadFormularyDetails_New")]
        public async Task<IActionResult> LoadFormularyDetails_New([FromBody] string formularyVersionId)
        {
            const string MMC_CONFIG_KEY = "MMC_Control_Source";

            string token = HttpContext.Session.GetString("access_token");

            var responseTask = TerminologyAPIService.GetFormularyDetailRuleBound(formularyVersionId, true, token);// TerminologyAPIService.GetFormularyDetail(formularyVersionId, token);

            var terminologyConfigTask = TerminologyAPIService.GetTerminologyConfiguration(MMC_CONFIG_KEY, token);

            await Task.WhenAll(responseTask, terminologyConfigTask);

            var response = await responseTask;

            var terminologyConfigResponse = await terminologyConfigTask;

            if ((response == null || response.Data == null || response.StatusCode == DataService.APIModel.StatusCode.Fail) || ((terminologyConfigResponse == null || terminologyConfigResponse.StatusCode == DataService.APIModel.StatusCode.Fail)))
            {
                const string ErrorResponse = "Error getting details for the selected record";
                var errors = "";
                if (response.ErrorMessages.IsCollectionValid())
                {
                    errors = string.Join('\n', ErrorResponse, response.ErrorMessages.ToArray());
                }
                if (terminologyConfigResponse.ErrorMessages.IsCollectionValid())
                {
                    errors = string.Join('\n', ErrorResponse, terminologyConfigResponse.ErrorMessages.ToArray());
                }
                _toastNotification.AddErrorToastMessage(errors);

                return null;
            }

            var formulariesfromAPI = response.Data;

            await FillLookupsToViewBag(formulariesfromAPI.RecStatusCode);

            FormularyEditModel vm = GetFormularyEditModel(formulariesfromAPI);

            if (string.Compare(vm.CodeSystem, "Custom", true) != 0)
            {
                var identificationTypes = (await _formularyLookupService.GetIdentificationCodeTypes());

                if (identificationTypes.IsCollectionValid())
                {
                    ViewBag.IndentificationCodeTypes = identificationTypes.Where(x => x.Value != TerminologyConstants.PRIMARY_IDENTIFICATION_CODE_TYPE)?.ToDictionary(k => k.Key, v => v.Value).ToSelectList(TerminologyConstants.DEFAULT_DROPDOWN_TEXT);
                }
            }

            return PartialView(GetFormularyEditView(formulariesfromAPI.ProductType), vm);
        }


        private string GetFormularyEditView(string productType)
        {
            if (productType.IsEmpty()) return null;

            if (string.Compare(productType, "vmp", true) == 0)
                return "_VMPFormularyEdit";
            if (string.Compare(productType, "amp", true) == 0)
                return "_AMPFormularyEdit";
            if (string.Compare(productType, "vtm", true) == 0)
                return "_VTMFormularyEdit";

            return null;
        }

        private string GetFormularyEditViewForBulkEdit(string productType)
        {
            if (productType.IsEmpty()) return null;

            if (string.Compare(productType, "vmp", true) == 0)
                return "_VMPFormularyBulkEdit";
            if (string.Compare(productType, "amp", true) == 0)
                return "_AMPFormularyBulkEdit";
            if (string.Compare(productType, "vtm", true) == 0)
                return "_VTMFormularyBulkEdit";

            return null;
        }

        private FormularyEditModel GetFormularyEditModel(FormularyHeaderAPIModel formulariesfromAPI)
        {
            var vmProvider = new FormularyEditViewModelProvider(this._mapper, this.HttpContext)
            {
                ApiModelProvider = () => formulariesfromAPI
            };
            return vmProvider.GetFormularyEditVM();
        }

        [HttpGet]
        [Route("Formulary/GetBulkEditSelectorPartial")]
        public IActionResult GetBulkEditSelectorPartial()
        {
            return PartialView("_FormularyBulkEditSelector");
        }

        [HttpPost]
        [Route("Formulary/GetProductTypeBulkEditPartialByPost")]
        public async Task<IActionResult> GetProductTypeBulkEditPartialByPost(string formularyVersionIds, string productType, string status)
        {
            return await GetProductTypeBulkEditPartial(formularyVersionIds, productType, status);
        }

        [HttpGet]
        [Route("Formulary/GetProductTypeBulkEditPartial")]
        public async Task<IActionResult> GetProductTypeBulkEditPartial(string formularyVersionIds, string productType, string status)
        {
            var formularyVersionIdList = JsonConvert.DeserializeObject<List<string>>(formularyVersionIds);

            var editPartial = GetFormularyEditViewForBulkEdit(productType);

            var vm = new BulkFormularyEditModel
            {
                FormularyVersionIds = formularyVersionIdList,
                ProductType = productType,
                ControlIdentifier = "Edit_Formulary",
                IsBulkEdit = true
            };

            vm.SerializedOriginalObj = JsonConvert.SerializeObject(vm);
            vm.Status = GetStatucCodeForStatus(status);
            vm.OriginalStatus = vm.Status;

            await FillLookupsToViewBag(vm.Status);

            var batchSize = 100;
            var batchedRequests = new List<List<string>>();

            for (var reqIndex = 0; reqIndex < formularyVersionIdList.Count; reqIndex += batchSize)
            {
                var batches = formularyVersionIdList.Skip(reqIndex).Take(batchSize);
                batchedRequests.Add(batches.ToList());
            }
            List<FormularyHeaderAPIModel> formularies = new();

            var getFormularyTasks = new List<Task<Dictionary<string, FormularyHeaderAPIModel>>>();
            foreach (var batch in batchedRequests)
            {
                var formulariesLkpTask = GetFormulariesAsLkp(batch);
                getFormularyTasks.Add(formulariesLkpTask);
            }

            await Task.WhenAll(getFormularyTasks);

            foreach (var getFormularyTask in getFormularyTasks)
            {
                var formulariesLkp = await getFormularyTask;
                if (formulariesLkp.IsCollectionValid())
                    formularies.AddRange(formulariesLkp.Values);
            }

            new BulkEditVMBuilder().Build(formularies, vm);

            return PartialView(editPartial, vm);
        }



        private string GetStatucCodeForStatus(string status)
        {
            switch (status.ToLower())
            {
                case "draft":
                    return TerminologyConstants.DRAFT_STATUS_CD;
                case "active":
                    return TerminologyConstants.ACTIVE_STATUS_CD;
                case "archived":
                    return TerminologyConstants.ARCHIEVED_STATUS_CD;
                case "inactive":
                    return TerminologyConstants.INACTIVE_STATUS_CD;
                case "deleted":
                    return TerminologyConstants.DELETED_STATUS_CD;
                case "ready for review":
                    return TerminologyConstants.READY_FOR_REVIEW_STATUS_CD;
                default: return status;
            }
        }

        #region old code - ref only
        //[HttpGet]
        //[Route("Formulary/GetProductTypeBulkEditPartial_old")]
        //public async Task<IActionResult> GetProductTypeBulkEditPartial_old(string formularyVersionIds, string productType, string status)
        //{
        //    var formularyVersionIdList = JsonConvert.DeserializeObject<List<string>>(formularyVersionIds);

        //    var editPartial = GetFormularyEditViewForBulkEdit(productType);

        //    var vm = new BulkFormularyEditModel
        //    {
        //        FormularyVersionIds = formularyVersionIdList,
        //        ProductType = productType,
        //        ControlIdentifier = "Edit_Formulary",
        //        IsBulkEdit = true
        //    };

        //    vm.SerializedOriginalObj = JsonConvert.SerializeObject(vm);

        //    string statusCode = "";

        //    switch (status.ToLower())
        //    {
        //        case "draft":
        //            statusCode = TerminologyConstants.DRAFT_STATUS_CD;
        //            break;
        //        case "active":
        //            statusCode = TerminologyConstants.ACTIVE_STATUS_CD;
        //            break;
        //        case "archived":
        //            statusCode = TerminologyConstants.ARCHIEVED_STATUS_CD;
        //            break;
        //        case "inactive":
        //            statusCode = TerminologyConstants.INACTIVE_STATUS_CD;
        //            break;
        //        case "deleted":
        //            statusCode = TerminologyConstants.DELETED_STATUS_CD;
        //            break;
        //        case "ready for review":
        //            statusCode = TerminologyConstants.READY_FOR_REVIEW_STATUS_CD;
        //            break;

        //    }

        //    await FillLookupsToViewBag(statusCode);

        //    string token = HttpContext.Session.GetString("access_token");

        //    var lclLicUseTask = TerminologyAPIService.GetLocalLicensedUse(formularyVersionIdList, token);

        //    var lclUnlicUseTask = TerminologyAPIService.GetLocalUnlicensedUse(formularyVersionIdList, token);

        //    var llclLicRouteTask = TerminologyAPIService.GetLocalLicensedRoute(formularyVersionIdList, token);

        //    var lclUnlicRouteTask = TerminologyAPIService.GetLocalUnlicensedRoute(formularyVersionIdList, token);

        //    var customWarningTask = TerminologyAPIService.GetCustomWarning(formularyVersionIdList, token);

        //    var reminderTask = TerminologyAPIService.GetReminder(formularyVersionIdList, token);

        //    var endorsementTask = TerminologyAPIService.GetEndorsement(formularyVersionIdList, token);

        //    var medusaPreparationInstructionTask = TerminologyAPIService.GetMedusaPreparationInstruction(formularyVersionIdList, token);

        //    var titrationTypeTask = TerminologyAPIService.GetTitrationType(formularyVersionIdList, token);

        //    var roundingFactorTask = TerminologyAPIService.GetRoundingFactor(formularyVersionIdList, token);

        //    var compatibleDiluentTask = TerminologyAPIService.GetCompatibleDiluent(formularyVersionIdList, token);

        //    var clinicalTrialMedicationTask = TerminologyAPIService.GetClinicalTrialMedication(formularyVersionIdList, token);

        //    var gastroResistantTask = TerminologyAPIService.GetGastroResistant(formularyVersionIdList, token);

        //    var criticalDrugTask = TerminologyAPIService.GetCriticalDrug(formularyVersionIdList, token);

        //    var modifiedReleaseTask = TerminologyAPIService.GetModifiedRelease(formularyVersionIdList, token);

        //    var expensiveMedicationTask = TerminologyAPIService.GetExpensiveMedication(formularyVersionIdList, token);

        //    var highAlertMedicationTask = TerminologyAPIService.GetHighAlertMedication(formularyVersionIdList, token);

        //    var ivToOralTask = TerminologyAPIService.GetIVToOral(formularyVersionIdList, token);

        //    var notForPRNTask = TerminologyAPIService.GetNotForPRN(formularyVersionIdList, token);

        //    var bloodProductTask = TerminologyAPIService.GetBloodProduct(formularyVersionIdList, token);

        //    var diluentTask = TerminologyAPIService.GetDiluent(formularyVersionIdList, token);

        //    var prescribableTask = TerminologyAPIService.GetPrescribable(formularyVersionIdList, token);

        //    var outpatientMedicationTask = TerminologyAPIService.GetOutpatientMedication(formularyVersionIdList, token);

        //    var ignoreDuplicateWarningTask = TerminologyAPIService.GetIgnoreDuplicateWarning(formularyVersionIdList, token);

        //    var controlledDrugTask = TerminologyAPIService.GetControlledDrug(formularyVersionIdList, token);

        //    var prescriptionPrintingRequiredTask = TerminologyAPIService.GetPrescriptionPrintingRequired(formularyVersionIdList, token);

        //    var indicationMandatoryTask = TerminologyAPIService.GetIndicationMandatory(formularyVersionIdList, token);

        //    var witnessingRequiredTask = TerminologyAPIService.GetWitnessingRequired(formularyVersionIdList, token);

        //    var formularyStatusTask = TerminologyAPIService.GetFormularyStatus(formularyVersionIdList, token);

        //    await Task.WhenAll(lclLicUseTask, lclUnlicUseTask, llclLicRouteTask, lclUnlicRouteTask, customWarningTask, reminderTask, endorsementTask, medusaPreparationInstructionTask, titrationTypeTask, roundingFactorTask, compatibleDiluentTask,
        //        clinicalTrialMedicationTask, gastroResistantTask, criticalDrugTask, modifiedReleaseTask, expensiveMedicationTask, highAlertMedicationTask, ivToOralTask, notForPRNTask, bloodProductTask, diluentTask, prescribableTask, outpatientMedicationTask,
        //        ignoreDuplicateWarningTask, controlledDrugTask, prescriptionPrintingRequiredTask, indicationMandatoryTask, witnessingRequiredTask, formularyStatusTask);

        //    var lclLicUseResponse = await lclLicUseTask;

        //    if (lclLicUseResponse.IsNotNull() && lclLicUseResponse.Data.IsCollectionValid())
        //    {
        //        lclLicUseResponse.Data.RemoveAll(item => item.LocalLicensedUse == null && item.LocalLicensedUseMD5 == null);

        //        if(lclLicUseResponse.Data.Count == 1 && lclLicUseResponse.Data[0].FormularyCount == formularyVersionIdList.Count && lclLicUseResponse.Data[0].LocalLicensedUse.IsNotEmpty())
        //        {
        //            var localLicensedUses = JsonConvert.DeserializeObject<List<FormularyLookupAPIModel>>(lclLicUseResponse.Data[0].LocalLicensedUse);

        //            foreach (var localLicensedUse in localLicensedUses)
        //            {
        //                CodeNameSelectorModel cns = new CodeNameSelectorModel();

        //                cns.Id = localLicensedUse.Cd;
        //                cns.Name = localLicensedUse.Desc;
        //                cns.Source = localLicensedUse.Source;

        //                if (vm.LocalLicensedUse.IsCollectionValid())
        //                {
        //                    vm.LocalLicensedUse.Add(cns);
        //                }
        //                else
        //                {
        //                    vm.LocalLicensedUse = new List<CodeNameSelectorModel>();

        //                    vm.LocalLicensedUse.Add(cns);
        //                }
        //            }
        //        }
        //        else
        //        {
        //            vm.DisplayLocalLicensedIndicationLbl = true;
        //        }
        //    }
        //    else
        //    {
        //        vm.DisplayLocalLicensedIndicationLbl = true;
        //    }

        //    var lclUnlicUseResponse = await lclUnlicUseTask;

        //    if (lclUnlicUseResponse.IsNotNull() && lclUnlicUseResponse.Data.IsCollectionValid())
        //    {
        //        lclUnlicUseResponse.Data.RemoveAll(item => item.LocalUnlicensedUse == null && item.LocalUnlicensedUseMD5 == null);

        //        if (lclUnlicUseResponse.Data.Count == 1 && lclUnlicUseResponse.Data[0].FormularyCount == formularyVersionIdList.Count && lclUnlicUseResponse.Data[0].LocalUnlicensedUse.IsNotEmpty())
        //        {
        //            var localUnlicensedUses = JsonConvert.DeserializeObject<List<FormularyLookupAPIModel>>(lclUnlicUseResponse.Data[0].LocalUnlicensedUse);

        //            foreach (var localUnlicensedUse in localUnlicensedUses)
        //            {
        //                CodeNameSelectorModel cns = new CodeNameSelectorModel();

        //                cns.Id = localUnlicensedUse.Cd;
        //                cns.Name = localUnlicensedUse.Desc;
        //                cns.Source = localUnlicensedUse.Source;

        //                if (vm.LocalUnlicensedUse.IsCollectionValid())
        //                {
        //                    vm.LocalUnlicensedUse.Add(cns);
        //                }
        //                else
        //                {
        //                    vm.LocalUnlicensedUse = new List<CodeNameSelectorModel>();

        //                    vm.LocalUnlicensedUse.Add(cns);
        //                }
        //            }
        //        }
        //        else
        //        {
        //            vm.DisplayLocalUnlicensedIndicationLbl = true;
        //        }
        //    }
        //    else
        //    {
        //        vm.DisplayLocalUnlicensedIndicationLbl = true;
        //    }

        //    var lclLicRouteResponse = await llclLicRouteTask;

        //    if (lclLicRouteResponse.IsNotNull() && lclLicRouteResponse.Data.IsCollectionValid())
        //    {
        //        if (lclLicRouteResponse.Data[0].FormularyVersionId.IsNotEmpty())
        //        {
        //            var localLicensedRoutes = lclLicRouteResponse.Data;

        //            var val = localLicensedRoutes.First().LocalLicensedRouteMD5;

        //            if (localLicensedRoutes.All(x => x.LocalLicensedRouteMD5 == val))
        //            {
        //                var lkp = this.HttpContext.Session.GetObject<Dictionary<string, string>>(SynapseSession.RoutesLkpKey);

        //                if(lkp == null)
        //                {
        //                    await _formularyLookupService.LoadLookUps();

        //                    lkp = this.HttpContext.Session.GetObject<Dictionary<string, string>>(SynapseSession.RoutesLkpKey);
        //                }

        //                var lclLicensedRoutes = JsonConvert.DeserializeObject<List<string>>(localLicensedRoutes.First().LocalLicensedRoute);

        //                foreach (var lclLicensedRoute in lclLicensedRoutes)
        //                {
        //                    CodeNameSelectorModel cns = new CodeNameSelectorModel();

        //                    cns.Id = lclLicensedRoute;
        //                    cns.Name = (lkp.IsCollectionValid() && lkp.ContainsKey(Convert.ToString(lclLicensedRoute))) ? lkp[Convert.ToString(lclLicensedRoute)] : Convert.ToString(lclLicensedRoute);
        //                    cns.Source = "RNOH";

        //                    if (vm.LocalLicensedRoute.IsCollectionValid())
        //                    {
        //                        vm.LocalLicensedRoute.Add(cns);
        //                    }
        //                    else
        //                    {
        //                        vm.LocalLicensedRoute = new List<CodeNameSelectorModel>();

        //                        vm.LocalLicensedRoute.Add(cns);
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                vm.DisplayLocalLicensedRouteLbl = true;
        //            }
        //        }
        //        else
        //        {
        //            vm.DisplayLocalLicensedRouteLbl = true;
        //        }
        //    }
        //    else
        //    {
        //        vm.DisplayLocalLicensedRouteLbl = true;
        //    }

        //    var lclUnlicRouteResponse = await lclUnlicRouteTask;

        //    if (lclUnlicRouteResponse.IsNotNull() && lclUnlicRouteResponse.Data.IsCollectionValid())
        //    {
        //        if (lclUnlicRouteResponse.Data[0].FormularyVersionId.IsNotEmpty())
        //        {
        //            var localUnlicensedRoutes = lclUnlicRouteResponse.Data;

        //            var val = localUnlicensedRoutes.First().LocalUnlicensedRouteMD5;

        //            if (localUnlicensedRoutes.All(x => x.LocalUnlicensedRouteMD5 == val))
        //            {
        //                var lkp = this.HttpContext.Session.GetObject<Dictionary<string, string>>(SynapseSession.RoutesLkpKey);

        //                if (lkp == null)
        //                {
        //                    await _formularyLookupService.LoadLookUps();

        //                    lkp = this.HttpContext.Session.GetObject<Dictionary<string, string>>(SynapseSession.RoutesLkpKey);
        //                }

        //                var lclUnlicensedRoutes = JsonConvert.DeserializeObject<List<string>>(localUnlicensedRoutes.First().LocalUnlicensedRoute);

        //                foreach(var lclUnlicensedRoute in lclUnlicensedRoutes)
        //                {
        //                    CodeNameSelectorModel cns = new CodeNameSelectorModel();

        //                    cns.Id = lclUnlicensedRoute;
        //                    cns.Name = (lkp.IsCollectionValid() && lkp.ContainsKey(Convert.ToString(lclUnlicensedRoute))) ? lkp[Convert.ToString(lclUnlicensedRoute)] : Convert.ToString(lclUnlicensedRoute);
        //                    cns.Source = "RNOH";

        //                    if (vm.LocalUnlicensedRoute.IsCollectionValid())
        //                    {
        //                        vm.LocalUnlicensedRoute.Add(cns);
        //                    }
        //                    else
        //                    {
        //                        vm.LocalUnlicensedRoute = new List<CodeNameSelectorModel>();

        //                        vm.LocalUnlicensedRoute.Add(cns);
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                vm.DisplayLocalUnlicensedRouteLbl = true;
        //            }
        //        }
        //        else
        //        {
        //            vm.DisplayLocalUnlicensedRouteLbl = true;
        //        }
        //    }
        //    else
        //    {
        //        vm.DisplayLocalUnlicensedRouteLbl = true;
        //    }

        //    var customWarningResponse = await customWarningTask;

        //    if (customWarningResponse.IsNotNull() && customWarningResponse.Data.IsCollectionValid())
        //    {
        //        customWarningResponse.Data.RemoveAll(item => item.CustomWarning == null && item.CustomWarningMD5 == null);

        //        if (customWarningResponse.Data.Count == 1 && customWarningResponse.Data[0].FormularyCount == formularyVersionIdList.Count && customWarningResponse.Data[0].CustomWarning.IsNotEmpty())
        //        {
        //            var customWarnings = JsonConvert.DeserializeObject<List<FormularyCustomWarningModel>>(customWarningResponse.Data[0].CustomWarning);

        //            foreach (var customWarning in customWarnings)
        //            {
        //                FormularyCustomWarningModel cw = new FormularyCustomWarningModel();

        //                cw.NeedResponse = customWarning.NeedResponse;
        //                cw.Source = customWarning.Source;
        //                cw.Warning = customWarning.Warning;

        //                if (vm.CustomWarnings.IsCollectionValid())
        //                {
        //                    vm.CustomWarnings.Add(cw);
        //                }
        //                else
        //                {
        //                    vm.CustomWarnings = new List<FormularyCustomWarningModel>();

        //                    vm.CustomWarnings.Add(cw);
        //                }
        //            }
        //        }
        //        else
        //        {
        //            vm.DisplayCustomWarningsLbl = true;
        //        }
        //    }
        //    else
        //    {
        //        vm.DisplayCustomWarningsLbl = true;
        //    }

        //    var reminderResponse = await reminderTask;

        //    if (reminderResponse.IsNotNull() && reminderResponse.Data.IsCollectionValid())
        //    {
        //        reminderResponse.Data.RemoveAll(item => item.Reminder == null && item.ReminderMD5 == null);

        //        if (reminderResponse.Data.Count == 1 && reminderResponse.Data[0].FormularyCount == formularyVersionIdList.Count && reminderResponse.Data[0].Reminder.IsNotEmpty())
        //        {
        //            var reminders = JsonConvert.DeserializeObject<List<FormularyReminderModel>>(reminderResponse.Data[0].Reminder);

        //            foreach (var reminder in reminders)
        //            {
        //                FormularyReminderModel rem = new FormularyReminderModel();

        //                rem.Active = reminder.Active;
        //                rem.Duration = reminder.Duration;
        //                rem.Reminder = reminder.Reminder;
        //                rem.Source = reminder.Source;

        //                if (vm.Reminders.IsCollectionValid())
        //                {
        //                    vm.Reminders.Add(rem);
        //                }
        //                else
        //                {
        //                    vm.Reminders = new List<FormularyReminderModel>();

        //                    vm.Reminders.Add(rem);
        //                }
        //            }
        //        }
        //        else
        //        {
        //            vm.DisplayRemindersLbl = true;
        //        }
        //    }
        //    else
        //    {
        //        vm.DisplayRemindersLbl = true;
        //    }

        //    var endorsementResponse = await endorsementTask;

        //    if (endorsementResponse.IsNotNull() && endorsementResponse.Data.IsCollectionValid())
        //    {
        //        endorsementResponse.Data.RemoveAll(item => item.Endorsement == null && item.EndorsementMD5 == null);

        //        if (endorsementResponse.Data.Count == 1 && endorsementResponse.Data[0].FormularyCount == formularyVersionIdList.Count && endorsementResponse.Data[0].Endorsement.IsNotEmpty())
        //        {
        //            var endorsements = JsonConvert.DeserializeObject<List<string>>(endorsementResponse.Data[0].Endorsement);

        //            foreach (var endorsement in endorsements)
        //            {
        //                string end = string.Empty;

        //                end = endorsement;

        //                if (vm.Endorsements.IsCollectionValid())
        //                {
        //                    vm.Endorsements.Add(end);
        //                }
        //                else
        //                {
        //                    vm.Endorsements = new List<string>();

        //                    vm.Endorsements.Add(end);
        //                }
        //            }
        //        }
        //        else
        //        {
        //            vm.DisplayEndorsementsLbl = true;
        //        }
        //    }
        //    else
        //    {
        //        vm.DisplayEndorsementsLbl = true;
        //    }

        //    var medusaPreparationInstructionResponse = await medusaPreparationInstructionTask;

        //    if (medusaPreparationInstructionResponse.IsNotNull() && medusaPreparationInstructionResponse.Data.IsCollectionValid())
        //    {
        //        medusaPreparationInstructionResponse.Data.RemoveAll(item => item.MedusaPreparationInstruction == null && item.MedusaPreparationInstructionMD5 == null);

        //        if (medusaPreparationInstructionResponse.Data.Count == 1 && medusaPreparationInstructionResponse.Data[0].FormularyCount == formularyVersionIdList.Count && medusaPreparationInstructionResponse.Data[0].MedusaPreparationInstruction.IsNotEmpty())
        //        {
        //            vm.MedusaPreparationInstructionsEditable = medusaPreparationInstructionResponse.Data[0].MedusaPreparationInstruction;
        //        }
        //        else
        //        {
        //            vm.DisplayMedusaPreparationLbl = true;
        //        }
        //    }
        //    else
        //    {
        //        vm.DisplayMedusaPreparationLbl = true;
        //    }

        //    var titrationTypeResponse = await titrationTypeTask;

        //    if (titrationTypeResponse.IsNotNull() && titrationTypeResponse.Data.IsCollectionValid())
        //    {
        //        titrationTypeResponse.Data.RemoveAll(item => item.TitrationType == null && item.TitrationTypeMD5 == null);

        //        if (titrationTypeResponse.Data.Count == 1 && titrationTypeResponse.Data[0].FormularyCount == formularyVersionIdList.Count && titrationTypeResponse.Data[0].TitrationType.IsNotEmpty())
        //        {
        //            vm.TitrationTypesEditableId = titrationTypeResponse.Data[0].TitrationType;
        //        }
        //        else
        //        {
        //            vm.DisplayTitrationTypeLbl = true;
        //        }
        //    }
        //    else
        //    {
        //        vm.DisplayTitrationTypeLbl = true;
        //    }

        //    var roundingFactorResponse = await roundingFactorTask;

        //    if (roundingFactorResponse.IsNotNull() && roundingFactorResponse.Data.IsCollectionValid())
        //    {
        //        roundingFactorResponse.Data.RemoveAll(item => item.RoundingFactor == null && item.RoundingFactorMD5 == null);

        //        if (roundingFactorResponse.Data.Count == 1 && roundingFactorResponse.Data[0].FormularyCount == formularyVersionIdList.Count && roundingFactorResponse.Data[0].RoundingFactor.IsNotEmpty())
        //        {
        //            vm.RoundingFactorCd = roundingFactorResponse.Data[0].RoundingFactor;
        //        }
        //        else
        //        {
        //            vm.DisplayRoundingFactorLbl = true;
        //        }
        //    }
        //    else
        //    {
        //        vm.DisplayRoundingFactorLbl = true;
        //    }

        //    var compatibleDiluentResponse = await compatibleDiluentTask;

        //    if (compatibleDiluentResponse.IsNotNull() && compatibleDiluentResponse.Data.IsCollectionValid())
        //    {
        //        compatibleDiluentResponse.Data.RemoveAll(item => item.CompatibleDiluent == null && item.CompatibleDiluentMD5 == null);

        //        if (compatibleDiluentResponse.Data.Count == 1 && compatibleDiluentResponse.Data[0].FormularyCount == formularyVersionIdList.Count && compatibleDiluentResponse.Data[0].CompatibleDiluent.IsNotEmpty())
        //        {
        //            var compatibleDiluents = JsonConvert.DeserializeObject<List<FormularyLookupAPIModel>>(compatibleDiluentResponse.Data[0].CompatibleDiluent);

        //            foreach (var compatibleDiluent in compatibleDiluents)
        //            {
        //                CodeNameSelectorModel cns = new CodeNameSelectorModel();

        //                cns.Id = compatibleDiluent.Cd;
        //                cns.Name = compatibleDiluent.Desc;
        //                cns.Source = compatibleDiluent.Source;

        //                if (vm.Diluents.IsCollectionValid())
        //                {
        //                    vm.Diluents.Add(cns);
        //                }
        //                else
        //                {
        //                    vm.Diluents = new List<CodeNameSelectorModel>();

        //                    vm.Diluents.Add(cns);
        //                }
        //            }
        //        }
        //        else
        //        {
        //            vm.DisplayCompatibleDiluentLbl = true;
        //        }
        //    }
        //    else
        //    {
        //        vm.DisplayCompatibleDiluentLbl = true;
        //    }

        //    var clinicalTrialMedicationResponse = await clinicalTrialMedicationTask;

        //    if (clinicalTrialMedicationResponse.IsNotNull() && clinicalTrialMedicationResponse.Data.IsCollectionValid())
        //    {
        //        clinicalTrialMedicationResponse.Data.RemoveAll(item => item.ClinicalTrialMedication == null && item.ClinicalTrialMedicationMD5 == null);

        //        if (clinicalTrialMedicationResponse.Data.Count == 1 && clinicalTrialMedicationResponse.Data[0].FormularyCount == formularyVersionIdList.Count && clinicalTrialMedicationResponse.Data[0].ClinicalTrialMedication.IsNotEmpty())
        //        {
        //            vm.NullableClinicalTrialMedication = clinicalTrialMedicationResponse.Data[0].ClinicalTrialMedication.ToNullable<bool>();
        //        }
        //        else
        //        {
        //            vm.DisplayClinicalTrialMedicationLbl = true;
        //        }
        //    }
        //    else
        //    {
        //        vm.DisplayClinicalTrialMedicationLbl = true;
        //    }

        //    var gastroResistantResponse = await gastroResistantTask;

        //    if (gastroResistantResponse.IsNotNull() && gastroResistantResponse.Data.IsCollectionValid())
        //    {
        //        gastroResistantResponse.Data.RemoveAll(item => item.GastroResistant == null && item.GastroResistantMD5 == null);

        //        if (gastroResistantResponse.Data.Count == 1 && gastroResistantResponse.Data[0].FormularyCount == formularyVersionIdList.Count && gastroResistantResponse.Data[0].GastroResistant.IsNotEmpty())
        //        {
        //            vm.NullableIsGastroResistant = gastroResistantResponse.Data[0].GastroResistant.ToNullable<bool>();
        //        }
        //        else
        //        {
        //            vm.DisplayGastroResistantLbl = true;
        //        }
        //    }
        //    else
        //    {
        //        vm.DisplayGastroResistantLbl = true;
        //    }

        //    var criticalDrugResponse = await criticalDrugTask;

        //    if (criticalDrugResponse.IsNotNull() && criticalDrugResponse.Data.IsCollectionValid())
        //    {
        //        criticalDrugResponse.Data.RemoveAll(item => item.CriticalDrug == null && item.CriticalDrugMD5 == null);

        //        if (criticalDrugResponse.Data.Count == 1 && criticalDrugResponse.Data[0].FormularyCount == formularyVersionIdList.Count && criticalDrugResponse.Data[0].CriticalDrug.IsNotEmpty())
        //        {
        //            vm.NullableCriticalDrug = criticalDrugResponse.Data[0].CriticalDrug.ToNullable<bool>();
        //        }
        //        else
        //        {
        //            vm.DisplayCriticalDrugLbl = true;
        //        }
        //    }
        //    else
        //    {
        //        vm.DisplayCriticalDrugLbl = true;
        //    }

        //    var modifiedReleaseResponse = await modifiedReleaseTask;

        //    if (modifiedReleaseResponse.IsNotNull() && modifiedReleaseResponse.Data.IsCollectionValid())
        //    {
        //        modifiedReleaseResponse.Data.RemoveAll(item => item.ModifiedRelease == null && item.ModifiedReleaseMD5 == null);

        //        if (modifiedReleaseResponse.Data.Count == 1 && modifiedReleaseResponse.Data[0].FormularyCount == formularyVersionIdList.Count && modifiedReleaseResponse.Data[0].ModifiedRelease.IsNotEmpty())
        //        {
        //            vm.NullableIsModifiedRelease = modifiedReleaseResponse.Data[0].ModifiedRelease.ToNullable<bool>();
        //        }
        //        else
        //        {
        //            vm.DisplayModifiedReleaseLbl = true;
        //        }
        //    }
        //    else
        //    {
        //        vm.DisplayModifiedReleaseLbl = true;
        //    }

        //    var expensiveMedicationResponse = await expensiveMedicationTask;

        //    if (expensiveMedicationResponse.IsNotNull() && expensiveMedicationResponse.Data.IsCollectionValid())
        //    {
        //        expensiveMedicationResponse.Data.RemoveAll(item => item.ExpensiveMedication == null && item.ExpensiveMedicationMD5 == null);

        //        if (expensiveMedicationResponse.Data.Count == 1 && expensiveMedicationResponse.Data[0].FormularyCount == formularyVersionIdList.Count && expensiveMedicationResponse.Data[0].ExpensiveMedication.IsNotEmpty())
        //        {
        //            vm.NullableExpensiveMedication = expensiveMedicationResponse.Data[0].ExpensiveMedication.ToNullable<bool>();
        //        }
        //        else
        //        {
        //            vm.DisplayExpensiveMedicationLbl = true;
        //        }
        //    }
        //    else
        //    {
        //        vm.DisplayExpensiveMedicationLbl = true;
        //    }

        //    var highAlertMedicationResponse = await highAlertMedicationTask;

        //    if (highAlertMedicationResponse.IsNotNull() && highAlertMedicationResponse.Data.IsCollectionValid())
        //    {
        //        highAlertMedicationResponse.Data.RemoveAll(item => item.HighAlertMedication == null && item.HighAlertMedicationMD5 == null);

        //        if (highAlertMedicationResponse.Data.Count == 1 && highAlertMedicationResponse.Data[0].FormularyCount == formularyVersionIdList.Count && highAlertMedicationResponse.Data[0].HighAlertMedication.IsNotEmpty())
        //        {
        //            vm.NullableHighAlertMedication = highAlertMedicationResponse.Data[0].HighAlertMedication.ToNullable<bool>();
        //        }
        //        else
        //        {
        //            vm.DisplayHighAlertMedicationLbl = true;
        //        }
        //    }
        //    else
        //    {
        //        vm.DisplayHighAlertMedicationLbl = true;
        //    }

        //    var ivToOralResponse = await ivToOralTask;

        //    if (ivToOralResponse.IsNotNull() && ivToOralResponse.Data.IsCollectionValid())
        //    {
        //        ivToOralResponse.Data.RemoveAll(item => item.IVToOral == null && item.IVToOralMD5 == null);

        //        if (ivToOralResponse.Data.Count == 1 && ivToOralResponse.Data[0].FormularyCount == formularyVersionIdList.Count && ivToOralResponse.Data[0].IVToOral.IsNotEmpty())
        //        {
        //            vm.NullableIVToOral = ivToOralResponse.Data[0].IVToOral.ToNullable<bool>();
        //        }
        //        else
        //        {
        //            vm.DisplayIVToOralLbl = true;
        //        }
        //    }
        //    else
        //    {
        //        vm.DisplayIVToOralLbl = true;
        //    }

        //    var notForPRNResponse = await notForPRNTask;

        //    if (notForPRNResponse.IsNotNull() && notForPRNResponse.Data.IsCollectionValid())
        //    {
        //        notForPRNResponse.Data.RemoveAll(item => item.NotForPRN == null && item.NotForPRNMD5 == null);

        //        if (notForPRNResponse.Data.Count == 1 && notForPRNResponse.Data[0].FormularyCount == formularyVersionIdList.Count && notForPRNResponse.Data[0].NotForPRN.IsNotEmpty())
        //        {
        //            vm.NullableNotForPrn = notForPRNResponse.Data[0].NotForPRN.ToNullable<bool>();
        //        }
        //        else
        //        {
        //            vm.DisplayNotForPRNLbl = true;
        //        }
        //    }
        //    else
        //    {
        //        vm.DisplayNotForPRNLbl = true;
        //    }

        //    var bloodProductResponse = await bloodProductTask;

        //    if (bloodProductResponse.IsNotNull() && bloodProductResponse.Data.IsCollectionValid())
        //    {
        //        bloodProductResponse.Data.RemoveAll(item => item.BloodProduct == null && item.BloodProductMD5 == null);

        //        if (bloodProductResponse.Data.Count == 1 && bloodProductResponse.Data[0].FormularyCount == formularyVersionIdList.Count && bloodProductResponse.Data[0].BloodProduct.IsNotEmpty())
        //        {
        //            vm.NullableIsBloodProduct = bloodProductResponse.Data[0].BloodProduct.ToNullable<bool>();
        //        }
        //        else
        //        {
        //            vm.DisplayBloodProductLbl = true;
        //        }
        //    }
        //    else
        //    {
        //        vm.DisplayBloodProductLbl = true;
        //    }

        //    var diluentResponse = await diluentTask;

        //    if (diluentResponse.IsNotNull() && diluentResponse.Data.IsCollectionValid())
        //    {
        //        diluentResponse.Data.RemoveAll(item => item.Diluent == null && item.DiluentMD5 == null);

        //        if (diluentResponse.Data.Count == 1 && diluentResponse.Data[0].FormularyCount == formularyVersionIdList.Count && diluentResponse.Data[0].Diluent.IsNotEmpty())
        //        {
        //            vm.NullableIsDiluent = diluentResponse.Data[0].Diluent.ToNullable<bool>();
        //        }
        //        else
        //        {
        //            vm.DisplayDiluentLbl = true;
        //        }
        //    }
        //    else
        //    {
        //        vm.DisplayDiluentLbl = true;
        //    }

        //    var prescribableResponse = await prescribableTask;

        //    if (prescribableResponse.IsNotNull() && prescribableResponse.Data.IsCollectionValid())
        //    {
        //        prescribableResponse.Data.RemoveAll(item => item.Prescribable == null && item.PrescribableMD5 == null);

        //        if (prescribableResponse.Data.Count == 1 && prescribableResponse.Data[0].FormularyCount == formularyVersionIdList.Count && prescribableResponse.Data[0].Prescribable.IsNotEmpty())
        //        {
        //            vm.NullablePrescribable = prescribableResponse.Data[0].Prescribable.ToNullable<bool>();
        //        }
        //        else
        //        {
        //            vm.DisplayPrescribableLbl = true;
        //        }
        //    }
        //    else
        //    {
        //        vm.DisplayPrescribableLbl = true;
        //    }

        //    var outpatientMedicationResponse = await outpatientMedicationTask;

        //    if (outpatientMedicationResponse.IsNotNull() && outpatientMedicationResponse.Data.IsCollectionValid())
        //    {
        //        outpatientMedicationResponse.Data.RemoveAll(item => item.OutpatientMedication == null && item.OutpatientMedicationMD5 == null);

        //        if (outpatientMedicationResponse.Data.Count == 1 && outpatientMedicationResponse.Data[0].FormularyCount == formularyVersionIdList.Count && outpatientMedicationResponse.Data[0].OutpatientMedication.IsNotEmpty())
        //        {
        //            vm.NullableOutpatientMedication = outpatientMedicationResponse.Data[0].OutpatientMedication.ToNullable<bool>();
        //        }
        //        else
        //        {
        //            vm.DisplayOutpatientMedicationLbl = true;
        //        }
        //    }
        //    else
        //    {
        //        vm.DisplayOutpatientMedicationLbl = true;
        //    }

        //    var ignoreDuplicateWarningResponse = await ignoreDuplicateWarningTask;

        //    if (ignoreDuplicateWarningResponse.IsNotNull() && ignoreDuplicateWarningResponse.Data.IsCollectionValid())
        //    {
        //        ignoreDuplicateWarningResponse.Data.RemoveAll(item => item.IgnoreDuplicateWarning == null && item.IgnoreDuplicateWarningMD5 == null);

        //        if (ignoreDuplicateWarningResponse.Data.Count == 1 && ignoreDuplicateWarningResponse.Data[0].FormularyCount == formularyVersionIdList.Count && ignoreDuplicateWarningResponse.Data[0].IgnoreDuplicateWarning.IsNotEmpty())
        //        {
        //            vm.NullableIgnoreDuplicateWarnings = ignoreDuplicateWarningResponse.Data[0].IgnoreDuplicateWarning.ToNullable<bool>();
        //        }
        //        else
        //        {
        //            vm.DisplayIgnoreDuplicateWarningLbl = true;
        //        }
        //    }
        //    else
        //    {
        //        vm.DisplayIgnoreDuplicateWarningLbl = true;
        //    }

        //    var controlledDrugResponse = await controlledDrugTask;

        //    if (controlledDrugResponse.IsNotNull() && controlledDrugResponse.Data.IsCollectionValid())
        //    {
        //        controlledDrugResponse.Data.RemoveAll(item => item.ControlledDrug == null && item.ControlledDrugMD5 == null);

        //        if (controlledDrugResponse.Data.Count == 1 && controlledDrugResponse.Data[0].FormularyCount == formularyVersionIdList.Count && controlledDrugResponse.Data[0].ControlledDrug.IsNotEmpty())
        //        {
        //            vm.NullableIsCustomControlledDrug = controlledDrugResponse.Data[0].ControlledDrug.ToNullable<bool>();
        //        }
        //        else
        //        {
        //            vm.DisplayControlledDrugLbl = true;
        //        }
        //    }
        //    else
        //    {
        //        vm.DisplayControlledDrugLbl = true;
        //    }

        //    var prescriptionPrintingRequiredResponse = await prescriptionPrintingRequiredTask;

        //    if (prescriptionPrintingRequiredResponse.IsNotNull() && prescriptionPrintingRequiredResponse.Data.IsCollectionValid())
        //    {
        //        prescriptionPrintingRequiredResponse.Data.RemoveAll(item => item.PrescriptionPrintingRequired == null && item.PrescriptionPrintingRequiredMD5 == null);

        //        if (prescriptionPrintingRequiredResponse.Data.Count == 1 && prescriptionPrintingRequiredResponse.Data[0].FormularyCount == formularyVersionIdList.Count && prescriptionPrintingRequiredResponse.Data[0].PrescriptionPrintingRequired.IsNotEmpty())
        //        {
        //            vm.NullableIsPrescriptionPrintingRequired = prescriptionPrintingRequiredResponse.Data[0].PrescriptionPrintingRequired.ToNullable<bool>();
        //        }
        //        else
        //        {
        //            vm.DisplayPrescriptionPrintingRequiredLbl = true;
        //        }
        //    }
        //    else
        //    {
        //        vm.DisplayPrescriptionPrintingRequiredLbl = true;
        //    }

        //    var indicationMandatoryResponse = await indicationMandatoryTask;

        //    if (indicationMandatoryResponse.IsNotNull() && indicationMandatoryResponse.Data.IsCollectionValid())
        //    {
        //        indicationMandatoryResponse.Data.RemoveAll(item => item.IndicationMandatory == null && item.IndicationMandatoryMD5 == null);

        //        if (indicationMandatoryResponse.Data.Count == 1 && indicationMandatoryResponse.Data[0].FormularyCount == formularyVersionIdList.Count && indicationMandatoryResponse.Data[0].IndicationMandatory.IsNotEmpty())
        //        {
        //            vm.NullableIsIndicationMandatory = indicationMandatoryResponse.Data[0].IndicationMandatory.ToNullable<bool>();
        //        }
        //        else
        //        {
        //            vm.DisplayIndicationMandatoryLbl = true;
        //        }
        //    }
        //    else
        //    {
        //        vm.DisplayIndicationMandatoryLbl = true;
        //    }

        //    var witnessingRequiredResponse = await witnessingRequiredTask;

        //    if (witnessingRequiredResponse.IsNotNull() && witnessingRequiredResponse.Data.IsCollectionValid())
        //    {
        //        witnessingRequiredResponse.Data.RemoveAll(item => item.WitnessingRequired == null && item.WitnessingRequiredMD5 == null);

        //        if (witnessingRequiredResponse.Data.Count == 1 && witnessingRequiredResponse.Data[0].FormularyCount == formularyVersionIdList.Count && witnessingRequiredResponse.Data[0].WitnessingRequired.IsNotEmpty())
        //        {
        //            vm.NullableWitnessingRequired = witnessingRequiredResponse.Data[0].WitnessingRequired.ToNullable<bool>();
        //        }
        //        else
        //        {
        //            vm.DisplayWitnessingRequiredLbl = true;
        //        }
        //    }
        //    else
        //    {
        //        vm.DisplayWitnessingRequiredLbl = true;
        //    }

        //    var formularyStatusResponse = await formularyStatusTask;

        //    if (formularyStatusResponse.IsNotNull() && formularyStatusResponse.Data.IsCollectionValid())
        //    {
        //        formularyStatusResponse.Data.RemoveAll(item => item.FormularyStatus == null && item.FormularyStatusMD5 == null);

        //        if (formularyStatusResponse.Data.Count == 1 && formularyStatusResponse.Data[0].FormularyCount == formularyVersionIdList.Count && formularyStatusResponse.Data[0].FormularyStatus.IsNotEmpty())
        //        {
        //            var formularyStatus = Convert.ToString(formularyStatusResponse.Data[0].FormularyStatus);

        //            vm.RnohFormularyStatuscd = formularyStatus;
        //        }
        //        else
        //        {
        //            vm.DisplayFormularyStatusLbl = true;
        //        }
        //    }
        //    else
        //    {
        //        vm.DisplayFormularyStatusLbl = true;
        //    }

        //    vm.Status = statusCode;

        //    return PartialView(editPartial, vm);
        //}
        #endregion

        [RequestFormLimits(ValueCountLimit = int.MaxValue)]
        [HttpPost]
        [Route("Formulary/NeedUserConfirmationForBulkUpdate")]
        public async Task<IActionResult> NeedUserConfirmationForBulkUpdate(BulkFormularyEditModel editedData)
        {
            return await NeedUserConfirmationForUpdate(editedData);
        }

        [RequestFormLimits(ValueCountLimit = int.MaxValue)]
        [HttpPost]
        [Route("Formulary/NeedUserConfirmationForUpdate")]
        public async Task<IActionResult> NeedUserConfirmationForUpdate(FormularyEditModel editedData)
        {
            dynamic response = new { needConfirmation = false, msg = string.Empty };

            //Will be validate only in case of 'Active' status codes.
            if ((string.Compare(editedData.ProductType, "amp", true) == 0 && editedData.Status != TerminologyConstants.ACTIVE_STATUS_CD))
                return Json(response);

            var uniqueIds = editedData.FormularyVersionIds;

            return await NeedUserConfirmationForUpdateForFormularyVersionIds(uniqueIds);
        }

        [RequestFormLimits(ValueCountLimit = int.MaxValue)]
        [HttpPost]
        [Route("Formulary/NeedUserConfirmationForUpdateForFormularyVerionIds")]
        public async Task<IActionResult> NeedUserConfirmationForUpdateForIds([FromBody] List<string> formularyVersionIds)
        {
            dynamic response = new { needConfirmation = false, msg = string.Empty };

            if (!formularyVersionIds.IsCollectionValid())
                return Json(response);

            return await NeedUserConfirmationForUpdateForFormularyVersionIds(formularyVersionIds);
        }

        public async Task<IActionResult> NeedUserConfirmationForUpdateForFormularyVersionIds(List<string> formularyVersionIds)
        {
            const string msgHeader = "The following products being moved to ACTIVE have been DELETED in dm+d and will be DELETED in the MMC: {0}";
            const string msgHeaderForParent = "The following products have been DELETED in dm+d and will be DELETED in the MMC: {0}";
            dynamic response = new { needConfirmation = false, msg = string.Empty };

            var uniqueIds = formularyVersionIds;
            var messages = new List<string>();

            var batchSize = 500;
            var batchedRequests = new List<List<string>>();

            for (var reqIndex = 0; reqIndex < uniqueIds.Count; reqIndex += batchSize)
            {
                var batches = uniqueIds.Skip(reqIndex).Take(batchSize);
                batchedRequests.Add(batches.ToList());
            }

            var productType = "";
            foreach (var batch in batchedRequests)
            {
                // get formularies for the entire batch and keep as lkp
                //var fvIdFormularyLkp = await GetFormulariesAsLkp(batch);
                var fvIdFormularyLkp = await GetFormulariesHeaderOnlyAsLkp(batch);

                foreach (var rec in batch)
                {
                    var recForFVId = fvIdFormularyLkp.ContainsKey(rec) ? fvIdFormularyLkp[rec] : null;
                    if (recForFVId != null && recForFVId.IsDmdDeleted == true)
                    {
                        productType = recForFVId.ProductType;
                        messages.Add($"{recForFVId.Code}-{recForFVId.Name}");
                    }
                }
            }


            var msgHead = (string.Compare(productType, "vtm", true) == 0 || string.Compare(productType, "vmp", true) == 0) ? msgHeaderForParent : msgHeader;

            if (messages.IsCollectionValid())
                response = new { needConfirmation = true, msg = msgHead.ToFormat(string.Join(" ", messages.Select(rec => $"<li>{rec}</li>"))) };

            return Json(response);
        }

        #region oldcode - ref only
        //[RequestFormLimits(ValueCountLimit = int.MaxValue)]
        //[HttpPost]
        //[Route("Formulary/NeedUserConfirmationForUpdate")]
        //public async Task<IActionResult> NeedUserConfirmationForUpdate(FormularyEditModel editedData)
        //{
        //    const string msgHeader = "The following products being moved to ACTIVE have been DELETED in dm+d and will be DELETED in the MMC: {0}";
        //    const string msgHeaderForParent = "The following products have been DELETED in dm+d and will be DELETED in the MMC: {0}";

        //    dynamic response = new { needConfirmation = false, msg = string.Empty };

        //    //Will be validate only in case of 'Active' status codes.
        //    if ((string.Compare(editedData.ProductType, "amp", true) == 0 && editedData.Status != TerminologyConstants.ACTIVE_STATUS_CD))
        //        return Json(response);

        //    BulkFormularyEditModel bulkEditedData = null;

        //    if (editedData.IsBulkEdit)
        //        bulkEditedData = editedData as BulkFormularyEditModel;

        //    var uniqueIds = editedData.FormularyVersionIds;
        //    var messages = new List<string>();

        //    var batchSize = 500;
        //    var batchedRequests = new List<List<string>>();

        //    for (var reqIndex = 0; reqIndex < uniqueIds.Count; reqIndex += batchSize)
        //    {
        //        var batches = uniqueIds.Skip(reqIndex).Take(batchSize);
        //        batchedRequests.Add(batches.ToList());
        //    }

        //    //BulkFormularyEditModel orginalObj = null;
        //    ////BulkFormularyEditModel edDataDeserializedObj = null;

        //    //if (editedData.IsBulkEdit)
        //    //{
        //    //    orginalObj = JsonConvert.DeserializeObject<BulkFormularyEditModel>(bulkEditedData.SerializedOriginalObj);
        //    //    //edDataDeserializedObj = _mapper.Map<BulkFormularyEditModel>(bulkEditedData);
        //    //}

        //    foreach(var batch in batchedRequests)
        //    {
        //        // get formularies for the entire batch and keep as lkp
        //        var fvIdFormularyLkp = await GetFormulariesAsLkp(batch);

        //        foreach (var rec in batch)
        //        {
        //            var recForFVId = fvIdFormularyLkp.ContainsKey(rec) ? fvIdFormularyLkp[rec] : null;
        //            if (recForFVId != null && recForFVId.IsDmdDeleted == true)
        //                messages.Add($"{recForFVId.Code}-{recForFVId.Name}");
        //            /*
        //            FormularyHeaderAPIModel apiModel = null;

        //            if (editedData.IsBulkEdit)
        //            {
        //                //var orginalObj = JsonConvert.DeserializeObject<BulkFormularyEditModel>(bulkEditedData.SerializedOriginalObj);
        //                var edDataDeserializedObj = _mapper.Map<BulkFormularyEditModel>(bulkEditedData);
        //                await MergeOnlyModified(edDataDeserializedObj, orginalObj, rec, fvIdFormularyLkp.ContainsKey(rec) ? fvIdFormularyLkp[rec] : null);

        //                apiModel = _mapper.Map<FormularyHeaderAPIModel>(edDataDeserializedObj);
        //            }
        //            else
        //            {
        //                apiModel = _mapper.Map<FormularyHeaderAPIModel>(editedData);
        //            }

        //            if (apiModel.IsDmdDeleted == true)
        //                messages.Add($"{apiModel.Code}-{apiModel.Name}");
        //            */
        //        }
        //    }


        //    var msgHead = (string.Compare(editedData.ProductType, "vtm", true) == 0 || string.Compare(editedData.ProductType, "vmp", true) == 0)? msgHeaderForParent: msgHeader;

        //    if (messages.IsCollectionValid())
        //        response = new { needConfirmation = true, msg = msgHead.ToFormat(string.Join(" ", messages.Select(rec=> $"<li>{rec}</li>"))) };

        //    return Json(response);
        //}
        #endregion

        [RequestFormLimits(ValueCountLimit = int.MaxValue)]
        [HttpPost]
        [Route("Formulary/UpdateBulkFormulary")]
        public async Task<IActionResult> UpdateBulkFormulary(BulkFormularyEditModel editedData)
        {
            return await UpdateFormulary(editedData);
        }

        [RequestFormLimits(ValueCountLimit = int.MaxValue)]
        [HttpPost]
        [Route("Formulary/UpdateFormulary")]
        public async Task<IActionResult> UpdateFormulary(FormularyEditModel editedData)
        {
            BulkFormularyEditModel bulkEditedData = null;

            string token = HttpContext.Session.GetString("access_token");

            var partialView = GetFormularyEditView(editedData.ProductType);

            if (editedData.IsBulkEdit)
            {
                bulkEditedData = editedData as BulkFormularyEditModel;
                partialView = GetFormularyEditViewForBulkEdit(editedData.ProductType);
            }

            if (await HasRunningBackgroundTask(token))
            {
                const string IMPORT_IN_PROGRESS_MSG = "DM+D data import is in progress. Please try after some time.";
                _toastNotification.AddErrorToastMessage(IMPORT_IN_PROGRESS_MSG);
                await FillLookupsToViewBag(editedData.OriginalStatus);
                await FillIdentificationCodeTypesViewBagByCurrentType(editedData.CodeSystem);

                editedData.IsSaveSuccessful = false;
                return PartialView(partialView, editedData);
            }

            if (await IsRecordsUpdateInProgress(token))
            {
                const string UPDATE_IN_PROGRESS_MSG = "Update is in progress. Please try after some time.";
                _toastNotification.AddErrorToastMessage(UPDATE_IN_PROGRESS_MSG);
                await FillLookupsToViewBag(editedData.OriginalStatus);
                await FillIdentificationCodeTypesViewBagByCurrentType(editedData.CodeSystem);

                editedData.IsSaveSuccessful = false;
                return PartialView(partialView, editedData);
            }

            if (!ModelState.IsValid)
            {
                await FillLookupsToViewBag(editedData.OriginalStatus);

                await FillIdentificationCodeTypesViewBagByCurrentType(editedData.CodeSystem);

                editedData.IsSaveSuccessful = false;
                return PartialView(partialView, editedData.IsBulkEdit ? bulkEditedData : editedData);
            }

            //(bool isValidStatusChange, string errorMsg) = await ValidateFormularyStatusChange(editedData);
            if (string.Compare(editedData.ProductType, "amp", true) == 0)
            {
                (bool isValidStatusChange, string errorMsg) = await ValidateFormularyStatusChange(editedData.FormularyVersionIds, editedData.Status);

                if (!isValidStatusChange)
                    return await DisplayErrorMessage(errorMsg, editedData, bulkEditedData, partialView);

                //validate only for AMP
                (bool isValid, string errorMsgFromLocalRoutes) = await ValidateLocalLicensedRoutesEntries(editedData, token);

                if (!isValid)
                    return await DisplayErrorMessage(errorMsgFromLocalRoutes, editedData, bulkEditedData, partialView);
            }

            #region old code - ref
            //var apiModels = new List<FormularyHeaderAPIModel>();

            //BulkFormularyEditModel orginalObj = null;
            //BulkFormularyEditModel edDataDeserializedObj = null;

            //if (editedData.IsBulkEdit)
            //{
            //    orginalObj = JsonConvert.DeserializeObject<BulkFormularyEditModel>(bulkEditedData.SerializedOriginalObj);
            //    edDataDeserializedObj = _mapper.Map<BulkFormularyEditModel>(bulkEditedData);
            //}
            #endregion

            var uniqueIds = editedData.FormularyVersionIds;

            //Need to introduce lock here before
            var lockResponse = await TerminologyAPIService.GetHeaderRecordsLock(uniqueIds, token);

            if (lockResponse == null || !lockResponse.Data)
            {
                var errorMsgForLock = $"This formulary details {string.Join(", ", uniqueIds)} cannot be saved as it has already been modified and a new version of it exists in the system. Please take latest version and try updating.";
                ModelState.AddModelError("Deny Status Change", errorMsgForLock);
                await FillLookupsToViewBag(editedData.OriginalStatus);
                await FillIdentificationCodeTypesViewBagByCurrentType(editedData.CodeSystem);
                editedData.IsSaveSuccessful = false;
                return PartialView(partialView, editedData.IsBulkEdit ? bulkEditedData : editedData);
            }

            var handlerResponse = await UpdateFormularyHandler(editedData, bulkEditedData, partialView);

            //release lock here
            await TerminologyAPIService.TryReleaseHeaderRecordsLock(uniqueIds, token);

            return handlerResponse;

            #region old code
            //foreach (var rec in uniqueIds)
            //{
            //    FormularyHeaderAPIModel apiModel = null;

            //    if (editedData.IsBulkEdit)
            //    {
            //        await MergeOnlyModified(edDataDeserializedObj, orginalObj, rec);

            //        apiModel = _mapper.Map<FormularyHeaderAPIModel>(edDataDeserializedObj);
            //    }
            //    else
            //    {
            //        apiModel = _mapper.Map<FormularyHeaderAPIModel>(editedData);
            //    }

            //    apiModel.FormularyVersionId = rec;
            //    apiModel.RecStatusCode = apiModel.RecStatusCode;  //editedData.IsBulkEdit ? null : apiModel.RecStatusCode;//For bulk-edit mode - Record status is not editable
            //    apiModels.Add(apiModel);
            //}

            //TerminologyAPIResponse<UpdateFormularyAPIResponse> response = await TerminologyAPIService.UpdateFormulary(apiModels, token);

            //if (response.StatusCode != DataService.APIModel.StatusCode.Success)
            //{
            //    await FillLookupsToViewBag(editedData.OriginalStatus);

            //    _toastNotification.AddErrorToastMessage(string.Join('\n', response.ErrorMessages));

            //    editedData.IsSaveSuccessful = false;

            //    return PartialView(partialView, editedData);
            //}

            //if (response.StatusCode == DataService.APIModel.StatusCode.Success)
            //{
            //    if (response.Data.Status != null && response.Data.Status.ErrorMessages.IsCollectionValid())
            //    {
            //        var errors = string.Join('\n', response.Data.Status.ErrorMessages);

            //        _toastNotification.AddInfoToastMessage(errors);

            //        await FillLookupsToViewBag(editedData.OriginalStatus);

            //        editedData.IsSaveSuccessful = true;

            //        return PartialView(partialView, editedData);
            //    }
            //}

            //_toastNotification.AddSuccessToastMessage("Successfully Updated Formulary details");

            //editedData.IsSaveSuccessful = true;

            //return Json(null);
            #endregion
        }

        private async Task<IActionResult> DisplayErrorMessage(string errorMsg, FormularyEditModel editedData, BulkFormularyEditModel bulkEditedData, string partialView)
        {
            if (errorMsg.IsNotEmpty())
                errorMsg.Split("|")?.Each(error => ModelState.AddModelError("Deny Status Change", error));

            await FillLookupsToViewBag(editedData.OriginalStatus);

            await FillIdentificationCodeTypesViewBagByCurrentType(editedData.CodeSystem);
            editedData.IsSaveSuccessful = false;
            return PartialView(partialView, editedData.IsBulkEdit ? bulkEditedData : editedData);
        }

        private async Task<(bool isValid, string errorMsg)> ValidateLocalLicensedRoutesEntries(FormularyEditModel editedData, string token)
        {
            var uniqueIds = editedData.FormularyVersionIds;

            if (!uniqueIds.IsCollectionValid()) return (true, "");

            if (!editedData.LocalLicensedRoute.IsCollectionValid())
                return (true, "");

            var response = await TerminologyAPIService.GetRoutesForIds(new DataService.APIModel.Requests.GetRoutesRequest { FormularyVersionIds = uniqueIds, RouteFieldTypeCd = TerminologyConstants.ROUTEFIELDTYPE_NORMAL_CD }, token);

            var isSuccess = (response.StatusCode == DataService.APIModel.StatusCode.Success);

            if (!isSuccess)
                return (false, "No Licensed routes returned from API. Please try after some time.");

            if (!response.Data.IsCollectionValid() && (editedData.LocalLicensedRoute.IsCollectionValid()))
                return (false, "Cannot add local licensed routes other than licensed routes provided by DM+D");

            var fvIdLicensedRouteLkp = new Dictionary<string, HashSet<string>>();

            var allUniqueLicensedRouteCds = new HashSet<string>();

            response.Data.Each(rec =>
            {
                if (!fvIdLicensedRouteLkp.ContainsKey(rec.FormularyVersionId))
                    fvIdLicensedRouteLkp[rec.FormularyVersionId] = new HashSet<string>();

                if (!fvIdLicensedRouteLkp[rec.FormularyVersionId].Contains(rec.RouteCd))
                    fvIdLicensedRouteLkp[rec.FormularyVersionId].Add(rec.RouteCd);

                if (!allUniqueLicensedRouteCds.Contains(rec.RouteCd))
                    allUniqueLicensedRouteCds.Add(rec.RouteCd);
            });

            var editedLocalLicensedRouteCds = editedData.LocalLicensedRoute?.Select(rec => rec.Id).Distinct().ToHashSet();

            var inputLocalRoutesLkp = new Dictionary<string, HashSet<string>>();

            uniqueIds.Each(rec => {
                if (!inputLocalRoutesLkp.ContainsKey(rec))
                    inputLocalRoutesLkp[rec] = new HashSet<string>();
                inputLocalRoutesLkp[rec] = editedLocalLicensedRouteCds;
            });

            //there is user input
            foreach (var inputLocalRoutesLkpKey in inputLocalRoutesLkp.Keys)
            {
                var licensedRouteForFVId = fvIdLicensedRouteLkp.ContainsKey(inputLocalRoutesLkpKey) ? fvIdLicensedRouteLkp[inputLocalRoutesLkpKey] : null;

                //there is user input but no licensed route in dm+d - not allowed
                if (!licensedRouteForFVId.IsCollectionValid())
                {
                    if (editedData.IsBulkEdit)
                        return (false, "There are records which has local licensed routes different than licensed routes from the DM+D.");

                    return (false, "Local licensed routes different than licensed routes from the DM+D is not allowed.");
                }

                var inputLocalLicRoutes = inputLocalRoutesLkp[inputLocalRoutesLkpKey];

                foreach (var editedLocalLicensedRouteCd in inputLocalLicRoutes)
                {
                    if (!licensedRouteForFVId.Contains(editedLocalLicensedRouteCd))
                    {
                        if (editedData.IsBulkEdit)
                            return (false, "There are records which has local licensed routes different than licensed routes from the DM+D.");

                        return (false, "Local licensed routes different than licensed routes from the DM+D is not allowed.");
                    }
                }
            }

            return (true, "");

            //var uniqueIds = editedData.FormularyVersionIds;
            //if (!uniqueIds.IsCollectionValid()) return (true, "");
            //var licRouteInfo = new Dictionary<string, (HashSet<string> oldLicRoute, HashSet<string> newLocalLicRoute)>();
            //var batchSize = 500;
            //var batchedRequests = new List<List<string>>();

            //for (var reqIndex = 0; reqIndex < uniqueIds.Count; reqIndex += batchSize)
            //{
            //    var batches = uniqueIds.Skip(reqIndex).Take(batchSize);
            //    batchedRequests.Add(batches.ToList());
            //}

            ////var oldLicRoute = fvIdFormularyLkp[rec]?.FormularyLocalRouteDetails?.Where(rec => rec.RouteFieldTypeCd == "003")?.Select(rec => rec.RouteCd)?.Distinct().ToHashSet();
            ////var newLocalLicRoutes = editedData.LocalLicensedRoute?.Select(route => route.Id)?.Distinct().ToHashSet();
            ////licRouteInfo[rec] = (oldLicRoute, newLocalLicRoutes);
        }

        private async Task<IActionResult> UpdateFormularyHandler(FormularyEditModel editedData, BulkFormularyEditModel bulkEditedData, string partialView)
        {
            var batchSize = 500;
            var batchedRequests = new List<List<string>>();
            var uniqueIds = editedData.FormularyVersionIds;
            var isSuccess = true;
            var errorsInResponse = new List<string>();
            string token = HttpContext.Session.GetString("access_token");

            for (var reqIndex = 0; reqIndex < uniqueIds.Count; reqIndex += batchSize)
            {
                var batches = uniqueIds.Skip(reqIndex).Take(batchSize);
                batchedRequests.Add(batches.ToList());
            }

            BulkFormularyEditModel orginalObj = null;
            //BulkFormularyEditModel edDataDeserializedObj = null;

            if (editedData.IsBulkEdit)
            {
                orginalObj = JsonConvert.DeserializeObject<BulkFormularyEditModel>(bulkEditedData.SerializedOriginalObj);
                //edDataDeserializedObj = _mapper.Map<BulkFormularyEditModel>(bulkEditedData);
            }

            var updateFormularyTasks = new List<Task<TerminologyAPIResponse<UpdateFormularyAPIResponse>>>();

            foreach (var batch in batchedRequests)
            {
                var apiModelsInBatch = new List<FormularyHeaderAPIModel>();

                //get formularies for the entire batch and keep as lkp
                var fvIdFormularyLkp = await GetFormulariesAsLkp(batch);

                //foreach (var rec in uniqueIds)
                foreach (var rec in batch)
                {
                    FormularyHeaderAPIModel apiModel = null;

                    if (editedData.IsBulkEdit)
                    {
                        //var orginalObj = JsonConvert.DeserializeObject<BulkFormularyEditModel>(bulkEditedData.SerializedOriginalObj);
                        var edDataDeserializedObj = _mapper.Map<BulkFormularyEditModel>(bulkEditedData);

                        //await MergeOnlyModified(edDataDeserializedObj, orginalObj, rec);
                        await MergeOnlyModified(edDataDeserializedObj, orginalObj, rec, fvIdFormularyLkp.ContainsKey(rec) ? fvIdFormularyLkp[rec] : null);

                        apiModel = _mapper.Map<FormularyHeaderAPIModel>(edDataDeserializedObj);
                    }
                    else
                    {
                        apiModel = _mapper.Map<FormularyHeaderAPIModel>(editedData);
                    }

                    apiModel.FormularyVersionId = rec;
                    apiModel.RecStatusCode = apiModel.RecStatusCode;  //editedData.IsBulkEdit ? null : apiModel.RecStatusCode;//For bulk-edit mode - Record status is not editable
                    //apiModels.Add(apiModel);
                    apiModelsInBatch.Add(apiModel);
                }

                var responseTask = TerminologyAPIService.UpdateFormulary(apiModelsInBatch, token);
                updateFormularyTasks.Add(responseTask);

                #region old code - ref only
                //TerminologyAPIResponse<UpdateFormularyAPIResponse> response = await TerminologyAPIService.UpdateFormulary(apiModelsInBatch, token);

                //isSuccessInBatch = (response.StatusCode == DataService.APIModel.StatusCode.Success);
                //isSuccess = isSuccess == false ? false : isSuccessInBatch;

                //if (isSuccessInBatch && response.Data.Status.ErrorMessages.IsCollectionValid())
                //    errorsInResponse.AddRange(response.Data.Status.ErrorMessages);
                //else if (response.ErrorMessages.IsCollectionValid())
                //    errorsInResponse.AddRange(response.ErrorMessages);
                #endregion
            }

            await Task.WhenAll(updateFormularyTasks);

            foreach (var updateFormularyTask in updateFormularyTasks)
            {
                TerminologyAPIResponse<UpdateFormularyAPIResponse> response = await updateFormularyTask;
                var isSuccessInBatch = (response.StatusCode == DataService.APIModel.StatusCode.Success);
                isSuccess = isSuccess == false ? false : isSuccessInBatch;

                if (isSuccessInBatch && response.Data.Status.ErrorMessages.IsCollectionValid())
                    errorsInResponse.AddRange(response.Data.Status.ErrorMessages);
                else if (response.ErrorMessages.IsCollectionValid())
                    errorsInResponse.AddRange(response.ErrorMessages);
            }

            return await PostUpdateFormularyHandler(isSuccess, errorsInResponse, editedData, partialView);
        }

        private async Task<IActionResult> PostUpdateFormularyHandler(bool isSuccess, List<string> errorsInResponse, FormularyEditModel editedData, string partialView)
        {
            if (!isSuccess)
            {
                await FillLookupsToViewBag(editedData.OriginalStatus);
                await FillIdentificationCodeTypesViewBagByCurrentType(editedData.CodeSystem);

                if (errorsInResponse.IsCollectionValid())
                    _toastNotification.AddErrorToastMessage(string.Join('\n', errorsInResponse));

                editedData.IsSaveSuccessful = false;

                return PartialView(partialView, editedData);
            }

            if (isSuccess && errorsInResponse.IsCollectionValid())
            {
                var errors = string.Join('\n', errorsInResponse);

                _toastNotification.AddInfoToastMessage(errors);

                await FillLookupsToViewBag(editedData.OriginalStatus);
                await FillIdentificationCodeTypesViewBagByCurrentType(editedData.CodeSystem);

                editedData.IsSaveSuccessful = true;

                return PartialView(partialView, editedData);
            }

            _toastNotification.AddSuccessToastMessage("Successfully Updated Formulary details");

            editedData.IsSaveSuccessful = true;

            return Json(null);
        }

        private async Task<(bool validationStatus, string errorMsg)> ValidateFormularyStatusChange(List<string> formularyVersionIds, string changedStatus)
        {
            if (!formularyVersionIds.IsCollectionValid()) return (false, "No records to validate");
            var request = new ValidateFormularyStatusChangeRequest() { RequestsData = new List<ValidateFormularyStatusChangeRequestData>() };

            string token = HttpContext.Session.GetString("access_token");

            var uniqueIds = formularyVersionIds;

            var batchSize = 500;
            var batchedRequests = new List<List<string>>();

            for (var reqIndex = 0; reqIndex < uniqueIds.Count; reqIndex += batchSize)
            {
                var batches = uniqueIds.Skip(reqIndex).Take(batchSize);
                batchedRequests.Add(batches.ToList());
            }

            foreach (var batch in batchedRequests)
            {
                //get formularies for the entire batch and keep as lkp
                //var fvIdFormularyLkp = await GetFormulariesAsLkp(batch);
                var fvIdFormularyLkp = await GetFormulariesHeaderOnlyAsLkp(batch);

                foreach (var rec in batch)
                {
                    var existingRec = fvIdFormularyLkp.ContainsKey(rec) ? fvIdFormularyLkp[rec] : null;
                    if (existingRec == null) continue;
                    request.RequestsData.Add(new ValidateFormularyStatusChangeRequestData { FormularyId = existingRec.FormularyId, FormularyVersionId = rec, RecordStatusCode = changedStatus });
                }
            }

            TerminologyAPIResponse<ValidateAMPStatusChangeAPIResponse> resultResponse = await TerminologyAPIService.ValidateAMPStatusChange(request, token);

            return PostValidateFormularyStatusChangeHandler(resultResponse);
        }

        private (bool validationStatus, string errorMsg) PostValidateFormularyStatusChangeHandler(TerminologyAPIResponse<ValidateAMPStatusChangeAPIResponse> resultResponse)
        {
            if (resultResponse == null)
            {
                _toastNotification.AddErrorToastMessage(UNKNOWN_SAVE_STATUS_MSG);
                return (false, UNKNOWN_SAVE_STATUS_MSG);
            }

            if (resultResponse.StatusCode != DataService.APIModel.StatusCode.Success)
            {
                string errors = "Error updating the changes.";

                if (resultResponse.ErrorMessages.IsCollectionValid())
                    errors = $"{errors}|{string.Join("|", resultResponse.ErrorMessages)}";

                _toastNotification.AddErrorToastMessage(errors);

                return (false, errors);
            }

            if (resultResponse.StatusCode == DataService.APIModel.StatusCode.Success)
            {
                if (resultResponse.Data.Status != null && resultResponse.Data.Status.ErrorMessages.IsCollectionValid())
                {
                    var errors = $"{string.Join("|", resultResponse.Data.Status.ErrorMessages)}";

                    _toastNotification.AddErrorToastMessage(errors);

                    return ((resultResponse.Data.Data != null ? resultResponse.Data.Data == true : false), errors);
                }
            }
            return (true, null);
        }

        #region oldcode - refonly
        //private async Task<(bool validationStatus, string errorMsg)> ValidateFormularyStatusChange(FormularyEditModel editedData)
        //{
        //    var request = new ValidateFormularyStatusChangeRequest() { RequestsData = new List<ValidateFormularyStatusChangeRequestData>() };

        //    //BulkFormularyEditModel bulkEditedData = null;

        //    string token = HttpContext.Session.GetString("access_token");

        //    var changedStatus = editedData.Status;
        //    //if (editedData.IsBulkEdit)
        //    //{
        //    //    bulkEditedData = editedData as BulkFormularyEditModel;
        //    //}

        //    var uniqueIds = editedData.FormularyVersionIds;

        //    var batchSize = 500;
        //    var batchedRequests = new List<List<string>>();

        //    for (var reqIndex = 0; reqIndex < uniqueIds.Count; reqIndex += batchSize)
        //    {
        //        var batches = uniqueIds.Skip(reqIndex).Take(batchSize);
        //        batchedRequests.Add(batches.ToList());
        //    }

        //    //BulkFormularyEditModel orginalObj = null;
        //    //BulkFormularyEditModel edDataDeserializedObj = null;

        //    //if (editedData.IsBulkEdit)
        //    //{
        //    //    orginalObj = JsonConvert.DeserializeObject<BulkFormularyEditModel>(bulkEditedData.SerializedOriginalObj);
        //    //    //edDataDeserializedObj = _mapper.Map<BulkFormularyEditModel>(bulkEditedData);
        //    //}

        //    foreach (var batch in batchedRequests)
        //    {
        //        //get formularies for the entire batch and keep as lkp
        //        var fvIdFormularyLkp = await GetFormulariesAsLkp(batch);

        //        foreach (var rec in batch)
        //        {
        //            var existingRec = fvIdFormularyLkp[rec];
        //            request.RequestsData.Add(new ValidateFormularyStatusChangeRequestData { FormularyId = existingRec.FormularyId, FormularyVersionId = rec, RecordStatusCode = changedStatus });
        //            /*
        //            FormularyHeaderAPIModel apiModel = null;

        //            if (editedData.IsBulkEdit)
        //            {
        //                //var orginalObj = JsonConvert.DeserializeObject<BulkFormularyEditModel>(bulkEditedData.SerializedOriginalObj);
        //                var edDataDeserializedObj = _mapper.Map<BulkFormularyEditModel>(bulkEditedData);
        //                await MergeOnlyModified(edDataDeserializedObj, orginalObj, rec, fvIdFormularyLkp.ContainsKey(rec) ? fvIdFormularyLkp[rec] : null);

        //                apiModel = _mapper.Map<FormularyHeaderAPIModel>(edDataDeserializedObj);
        //            }
        //            else
        //            {
        //                apiModel = _mapper.Map<FormularyHeaderAPIModel>(editedData);
        //            }

        //            request.RequestsData.Add(new ValidateFormularyStatusChangeRequestData { FormularyId = apiModel.FormularyId, FormularyVersionId = rec, RecordStatusCode = apiModel.RecStatusCode });
        //            */
        //        }
        //    }

        //    TerminologyAPIResponse<ValidateAMPStatusChangeAPIResponse> resultResponse = await TerminologyAPIService.ValidateAMPStatusChange(request, token);

        //    if (resultResponse == null)
        //    {
        //        _toastNotification.AddErrorToastMessage(UNKNOWN_SAVE_STATUS_MSG);
        //        return (false, UNKNOWN_SAVE_STATUS_MSG);
        //    }

        //    if (resultResponse.StatusCode != DataService.APIModel.StatusCode.Success)
        //    {
        //        string errors = "Error updating the changes.";

        //        if (resultResponse.ErrorMessages.IsCollectionValid())
        //            errors = $"{errors}|{string.Join("|",resultResponse.ErrorMessages)}";

        //        _toastNotification.AddErrorToastMessage(errors);

        //        return (false, errors);
        //    }

        //    if (resultResponse.StatusCode == DataService.APIModel.StatusCode.Success)
        //    {
        //        if (resultResponse.Data.Status != null && resultResponse.Data.Status.ErrorMessages.IsCollectionValid())
        //        {
        //            var errors = $"{string.Join("|", resultResponse.Data.Status.ErrorMessages)}";

        //            _toastNotification.AddErrorToastMessage(errors);

        //            return ((resultResponse.Data.Data != null ? resultResponse.Data.Data == true : false), errors);
        //        }
        //    }
        //    return (true, null);
        //}
        #endregion

        private async Task FillIdentificationCodeTypesViewBagByCurrentType(string currentSystem)
        {
            if (currentSystem.IsNotEmpty() && string.Compare(currentSystem, "Custom", true) != 0)
            {
                var identificationTypes = (await _formularyLookupService.GetIdentificationCodeTypes());

                if (identificationTypes.IsCollectionValid())
                {
                    ViewBag.IndentificationCodeTypes = identificationTypes.Where(x => x.Value != TerminologyConstants.PRIMARY_IDENTIFICATION_CODE_TYPE)?.ToDictionary(k => k.Key, v => v.Value).ToSelectList(TerminologyConstants.DEFAULT_DROPDOWN_TEXT);
                }
            }
        }

        private async Task<Dictionary<string, FormularyHeaderAPIModel>> GetFormulariesAsLkp(List<string> batch)
        {
            string token = HttpContext.Session.GetString("access_token");

            var response = await TerminologyAPIService.GetFormularyDetailRuleBoundForFVIds(batch, true, token);

            if (response == null || !response.Data.IsCollectionValid()) return null;

            var formulariesfromAPI = response.Data;
            var lkp = formulariesfromAPI.Select(rec => new { FVId = rec.FormularyVersionId, Formulary = rec })?.Distinct(rec => rec.FVId)
                .ToDictionary(k => k.FVId, v => v.Formulary);

            return lkp;
        }

        private async Task<Dictionary<string, FormularyHeaderAPIModel>> GetFormulariesHeaderOnlyAsLkp(List<string> batch)
        {
            string token = HttpContext.Session.GetString("access_token");

            var response = await TerminologyAPIService.GetFormularyHeaderOnlyForFVIds(batch, token);

            if (response == null || !response.Data.IsCollectionValid()) return null;

            var formulariesfromAPI = response.Data;
            var lkp = formulariesfromAPI.Select(rec => new { FVId = rec.FormularyVersionId, Formulary = rec })?.Distinct(rec => rec.FVId)
                .ToDictionary(k => k.FVId, v => v.Formulary);

            return lkp;
        }

        /// <summary>
        /// This method will check if the field is dirty after edit and will consider only those dirty fields for update - rest are ignored
        /// </summary>
        /// <param name="editedData"></param>
        /// <param name="orginalObj"></param>
        private async Task MergeOnlyModified(BulkFormularyEditModel editedData, BulkFormularyEditModel orginalObj, string currentFormularyVersionId, FormularyHeaderAPIModel resolveFormaulryHeaderAPIModel = null)
        {
            FormularyHeaderAPIModel formulariesfromAPI = resolveFormaulryHeaderAPIModel;

            if (resolveFormaulryHeaderAPIModel == null)
            {
                string token = HttpContext.Session.GetString("access_token");

                var response = await TerminologyAPIService.GetFormularyDetailRuleBound(currentFormularyVersionId, true, token);

                formulariesfromAPI = response.Data;
            }

            if (formulariesfromAPI == null) return;

            await FillLookupsToViewBag(formulariesfromAPI.RecStatusCode);

            var modelFromDb = GetFormularyEditModel(formulariesfromAPI);

            IBulkEditMergeHandler mergeHandler = null;

            if (string.Compare(editedData.ProductType, "vtm", true) == 0)
                mergeHandler = new VTMBulkEditMergeHandler();
            else if (string.Compare(editedData.ProductType, "vmp", true) == 0)
                mergeHandler = new VMPBulkEditMergeHandler();
            else if (string.Compare(editedData.ProductType, "amp", true) == 0)
                mergeHandler = new AMPBulkEditMergeHandler();

            mergeHandler.Merge(editedData, orginalObj, modelFromDb);
        }

        [HttpPost]
        [Route("Formulary/LoadHistoryFormularyDetails")]
        public async Task<IActionResult> LoadHistoryFormularyDetails(string previousFormularyVersionId, string currentFormularyVersionId, string previousOrCurrent)
        {
            const string MMC_CONFIG_KEY = "MMC_Control_Source";

            string token = HttpContext.Session.GetString("access_token");

            var previousResponseTask = TerminologyAPIService.GetFormularyDetailRuleBound(previousFormularyVersionId, true, token);// TerminologyAPIService.GetFormularyDetail(formularyVersionId, token);

            var currentResponseTask = TerminologyAPIService.GetFormularyDetailRuleBound(currentFormularyVersionId, true, token);

            var terminologyConfigTask = TerminologyAPIService.GetTerminologyConfiguration(MMC_CONFIG_KEY, token);

            await Task.WhenAll(previousResponseTask, terminologyConfigTask);

            await Task.WhenAll(currentResponseTask, terminologyConfigTask);

            var previousResponse = await previousResponseTask;

            var currentResponse = await currentResponseTask;

            var terminologyConfigResponse = await terminologyConfigTask;

            if ((previousResponse == null || previousResponse.StatusCode == DataService.APIModel.StatusCode.Fail) || (currentResponse == null || currentResponse.StatusCode == DataService.APIModel.StatusCode.Fail) || ((terminologyConfigResponse == null || terminologyConfigResponse.StatusCode == DataService.APIModel.StatusCode.Fail)))
            {
                const string ErrorResponse = "Error getting details for the selected record";
                var errors = "";
                if (previousResponse.ErrorMessages.IsCollectionValid())
                {
                    errors = $"{ErrorResponse}. {string.Join('\n', previousResponse.ErrorMessages)}";
                }
                if (currentResponse.ErrorMessages.IsCollectionValid())
                {
                    errors = $"{ErrorResponse}. {string.Join('\n', currentResponse.ErrorMessages)})";
                }
                if (terminologyConfigResponse.ErrorMessages.IsCollectionValid())
                {
                    errors = $"{ErrorResponse}. {string.Join('\n', terminologyConfigResponse.ErrorMessages)})";
                }
                _toastNotification.AddErrorToastMessage(errors);

                return Ok("");
            }

            var previousFormulariesfromAPI = previousResponse.Data;

            var currentFormulariesfromAPI = currentResponse.Data;

            //await FillLookupsToViewBag(previousFormulariesfromAPI.RecStatusCode);

            //await FillLookupsToViewBag(currentFormulariesfromAPI.RecStatusCode);
            await FillLookupsToViewBag(TerminologyConstants.DRAFT_STATUS_CD);//this should fill all the record statuses

            FormularyEditModel previousVM = GetFormularyEditModel(previousFormulariesfromAPI);

            FormularyEditModel currentVM = GetFormularyEditModel(currentFormulariesfromAPI);

            var prevProductName = previousVM.Name;
            var prevProductType = previousVM.ProductType;
            var prevCode = previousVM.Code;
            var prevCodeSystem = previousVM.CodeSystem;

            var currProductName = currentVM.Name;
            var currProductType = currentVM.ProductType;
            var currCode = currentVM.Code;
            var currCodeSystem = currentVM.CodeSystem;

            var differences = previousVM.Diff(currentVM, ComparisonOptions.All | ComparisonOptions.AllowCollectionsToBeOutOfOrder | ComparisonOptions.AllowEqualsOverride,
                ".FormularyVersionIds", ".Excipients.FormularyVersionId", ".FormularyClassificationCodes.FormularyVersionId", ".FormularyIdentificationCodes.FormularyVersionId", ".Ingredients.FormularyVersionId",
                ".UnlicensedRoute.Source", ".UnlicensedRoute.SourceColor", ".Route.Source", ".Route.SourceColor", ".LocalLicensedRoute.Source", ".LocalLicensedRoute.SourceColor", ".LocalUnlicensedRoute.Source", ".LocalUnlicensedRoute.SourceColor",
                ".Cautions.Source", ".Cautions.SourceColor", ".ContraIndications.Source", ".ContraIndications.SourceColor", ".SideEffects.Source", ".SideEffects.SourceColor", ".SafetyMessages.Source", ".SafetyMessages.SourceColor",
                ".LicensedUse.Source", ".LicensedUse.SourceColor", ".UnlicensedUse.Source", ".UnlicensedUse.SourceColor", ".LocalLicensedUse.Source", ".LocalLicensedUse.SourceColor", ".LocalUnlicensedUse.Source", ".LocalUnlicensedUse.SourceColor",
                ".FormCd.Source", ".FormCd.SourceColor", ".UnitDoseFormUnits.Source", ".UnitDoseFormUnits.SourceColor", ".UnitDoseUnitOfMeasure.Source", ".UnitDoseUnitOfMeasure.SourceColor", ".Supplier.Source", ".Supplier.SourceColor",
                ".TradeFamily.Source", ".TradeFamily.SourceColor", ".MedusaPreparationInstructions.Source", ".MedusaPreparationInstructions.SourceColor", ".ControlledDrugCategories.Source", ".ControlledDrugCategories.SourceColor",
                ".TitrationTypes.Source", ".TitrationTypes.SourceColor", ".Diluents.Source", ".Diluents.SourceColor", ".HighAlertMedicationSource", ".BlackTriangleSource", ".PrescribableSource");

            if (differences.Count > 0)
            {

                List<KeyValuePair<string, object>> prevObjects = new List<KeyValuePair<string, object>>();

                foreach (var diff in differences)
                {
                    prevObjects.Add(new KeyValuePair<string, object>(diff.Property, diff.LeftValue));
                }

                FormularyEditModel prevVM = new FormularyEditModel();

                prevVM.Differences = new List<Difference>();

                prevVM.Differences.AddRange(differences);

                var prevVMType = prevVM.GetType();

                foreach (KeyValuePair<string, object> _pair in prevObjects)
                {
                    if (prevVMType.GetProperty(_pair.Key).PropertyType.Name == "List`1")
                    {
                        if (_pair.Value != null)
                        {
                            object instance = Activator.CreateInstance(prevVMType.GetProperty(_pair.Key).PropertyType);

                            IList list = (IList)instance;

                            list.Add(_pair.Value);

                            if (prevVM.GetType().GetProperty(_pair.Key).GetValue(prevVM, null).IsNotNull())
                            {
                                foreach (var value in (dynamic)prevVM.GetType().GetProperty(_pair.Key).GetValue(prevVM, null))
                                {
                                    list.Add(value);
                                }
                                prevVMType.GetProperty(_pair.Key).SetValue(prevVM, list, null);
                            }
                            else
                            {
                                prevVMType.GetProperty(_pair.Key).SetValue(prevVM, list, null);
                            }
                        }
                        else
                        {
                            object instance = Activator.CreateInstance(prevVMType.GetProperty(_pair.Key).PropertyType);

                            IList list = (IList)instance;

                            list.Add(_pair.Value);

                            prevVMType.GetProperty(_pair.Key).SetValue(prevVM, list, null);
                        }
                    }
                    else
                    {
                        prevVMType.GetProperty(_pair.Key).SetValue(prevVM, _pair.Value);
                    }
                }

                previousVM = prevVM;

                previousVM.IsDiff = true;

                previousVM.Name = prevProductName;
                previousVM.ProductType = prevProductType;
                previousVM.Code = prevCode;
                previousVM.CodeSystem = prevCodeSystem;

                List<KeyValuePair<string, object>> currObjects = new List<KeyValuePair<string, object>>();

                foreach (var diff in differences)
                {
                    currObjects.Add(new KeyValuePair<string, object>(diff.Property, diff.RightValue));
                }

                FormularyEditModel currVM = new FormularyEditModel();

                currVM.Differences = new List<Difference>();

                currVM.Differences.AddRange(differences);

                var currVMType = currVM.GetType();

                foreach (KeyValuePair<string, object> _pair in currObjects)
                {
                    if (currVMType.GetProperty(_pair.Key).PropertyType.Name == "List`1")
                    {
                        if (_pair.Value != null)
                        {
                            object instance = Activator.CreateInstance(currVMType.GetProperty(_pair.Key).PropertyType);

                            IList list = (IList)instance;

                            list.Add(_pair.Value);

                            if (currVM.GetType().GetProperty(_pair.Key).GetValue(currVM, null).IsNotNull())
                            {
                                foreach (var value in (dynamic)currVM.GetType().GetProperty(_pair.Key).GetValue(currVM, null))
                                {
                                    list.Add(value);
                                }
                                currVMType.GetProperty(_pair.Key).SetValue(currVM, list, null);
                            }
                            else
                            {
                                currVMType.GetProperty(_pair.Key).SetValue(currVM, list, null);
                            }
                        }
                        else
                        {
                            object instance = Activator.CreateInstance(currVMType.GetProperty(_pair.Key).PropertyType);

                            IList list = (IList)instance;

                            list.Add(_pair.Value);

                            currVMType.GetProperty(_pair.Key).SetValue(currVM, list, null);
                        }

                    }
                    else
                    {
                        currVMType.GetProperty(_pair.Key).SetValue(currVM, _pair.Value);
                    }

                }

                currentVM = currVM;

                currentVM.IsDiff = true;

                currentVM.Name = currProductName;
                currentVM.ProductType = currProductType;
                currentVM.Code = currCode;
                currentVM.CodeSystem = currCodeSystem;

            }
            else
            {
                previousVM = new FormularyEditModel();
                currentVM = new FormularyEditModel();

                previousVM.IsDiff = false;

                previousVM.Name = prevProductName;
                previousVM.ProductType = prevProductType;
                previousVM.Code = prevCode;
                previousVM.CodeSystem = prevCodeSystem;

                currentVM.IsDiff = false;

                currentVM.Name = currProductName;
                currentVM.ProductType = currProductType;
                currentVM.Code = currCode;
                currentVM.CodeSystem = currCodeSystem;
            }

            if (previousVM.BlackTriangle != currentVM.BlackTriangle)
            {
                previousVM.IsDiffBlackTriangle = true;
                currentVM.IsDiffBlackTriangle = true;
            }
            else
            {
                previousVM.IsDiffBlackTriangle = false;
                currentVM.IsDiffBlackTriangle = false;
            }

            if (previousVM.ClinicalTrialMedication != currentVM.ClinicalTrialMedication)
            {
                previousVM.IsDiffClinicalTrialMedication = true;
                currentVM.IsDiffClinicalTrialMedication = true;
            }
            else
            {
                previousVM.IsDiffClinicalTrialMedication = false;
                currentVM.IsDiffClinicalTrialMedication = false;
            }

            if (previousVM.CriticalDrug != currentVM.CriticalDrug)
            {
                previousVM.IsDiffCriticalDrug = true;
                currentVM.IsDiffCriticalDrug = true;
            }
            else
            {
                previousVM.IsDiffCriticalDrug = false;
                currentVM.IsDiffCriticalDrug = false;
            }

            if (previousVM.EmaAdditionalMonitoring != currentVM.EmaAdditionalMonitoring)
            {
                previousVM.IsDiffEMAAddMontorng = true;
                currentVM.IsDiffEMAAddMontorng = true;
            }
            else
            {
                previousVM.IsDiffEMAAddMontorng = false;
                currentVM.IsDiffEMAAddMontorng = false;
            }

            if (previousVM.IsGastroResistant != currentVM.IsGastroResistant)
            {
                previousVM.IsDiffGastroResistant = true;
                currentVM.IsDiffGastroResistant = true;
            }
            else
            {
                previousVM.IsDiffGastroResistant = false;
                currentVM.IsDiffGastroResistant = false;
            }

            if (previousVM.IsModifiedRelease != currentVM.IsModifiedRelease)
            {
                previousVM.IsDiffModifiedRelease = true;
                currentVM.IsDiffModifiedRelease = true;
            }
            else
            {
                previousVM.IsDiffModifiedRelease = false;
                currentVM.IsDiffModifiedRelease = false;
            }

            if (previousVM.ExpensiveMedication != currentVM.ExpensiveMedication)
            {
                previousVM.IsDiffExpensiveMedication = true;
                currentVM.IsDiffExpensiveMedication = true;
            }
            else
            {
                previousVM.IsDiffExpensiveMedication = false;
                currentVM.IsDiffExpensiveMedication = false;
            }

            if (previousVM.HighAlertMedication != currentVM.HighAlertMedication)
            {
                previousVM.IsDiffHighAlertMedication = true;
                currentVM.IsDiffHighAlertMedication = true;
            }
            else
            {
                previousVM.IsDiffHighAlertMedication = false;
                currentVM.IsDiffHighAlertMedication = false;
            }

            if (previousVM.IVToOral != currentVM.IVToOral)
            {
                previousVM.IsDiffIVtoOral = true;
                currentVM.IsDiffIVtoOral = true;
            }
            else
            {
                previousVM.IsDiffIVtoOral = false;
                currentVM.IsDiffIVtoOral = false;
            }

            if (previousVM.NotForPrn != currentVM.NotForPrn)
            {
                previousVM.IsDiffNotforPRN = true;
                currentVM.IsDiffNotforPRN = true;
            }
            else
            {
                previousVM.IsDiffNotforPRN = false;
                currentVM.IsDiffNotforPRN = false;
            }

            if (previousVM.IsBloodProduct != currentVM.IsBloodProduct)
            {
                previousVM.IsDiffBloodProduct = true;
                currentVM.IsDiffBloodProduct = true;
            }
            else
            {
                previousVM.IsDiffBloodProduct = false;
                currentVM.IsDiffBloodProduct = false;
            }

            if (previousVM.IsDiluent != currentVM.IsDiluent)
            {
                previousVM.IsDiffDiluent = true;
                currentVM.IsDiffDiluent = true;
            }
            else
            {
                previousVM.IsDiffDiluent = false;
                currentVM.IsDiffDiluent = false;
            }

            if (previousVM.Prescribable != currentVM.Prescribable)
            {
                previousVM.IsDiffPrescribable = true;
                currentVM.IsDiffPrescribable = true;
            }
            else
            {
                previousVM.IsDiffPrescribable = false;
                currentVM.IsDiffPrescribable = false;
            }

            if (previousVM.OutpatientMedication != currentVM.OutpatientMedication)
            {
                previousVM.IsDiffOutpatientMedication = true;
                currentVM.IsDiffOutpatientMedication = true;
            }
            else
            {
                previousVM.IsDiffOutpatientMedication = false;
                currentVM.IsDiffOutpatientMedication = false;
            }

            if (previousVM.SugarFree != currentVM.SugarFree)
            {
                previousVM.IsDiffSugarFree = true;
                currentVM.IsDiffSugarFree = true;
            }
            else
            {
                previousVM.IsDiffSugarFree = false;
                currentVM.IsDiffSugarFree = false;
            }

            if (previousVM.GlutenFree != currentVM.GlutenFree)
            {
                previousVM.IsDiffGlutenFree = true;
                currentVM.IsDiffGlutenFree = true;
            }
            else
            {
                previousVM.IsDiffGlutenFree = false;
                currentVM.IsDiffGlutenFree = false;
            }

            if (previousVM.PreservativeFree != currentVM.PreservativeFree)
            {
                previousVM.IsDiffPreservativeFree = true;
                currentVM.IsDiffPreservativeFree = true;
            }
            else
            {
                previousVM.IsDiffPreservativeFree = false;
                currentVM.IsDiffPreservativeFree = false;
            }

            if (previousVM.CFCFree != currentVM.CFCFree)
            {
                previousVM.IsDiffCFCFree = true;
                currentVM.IsDiffCFCFree = true;
            }
            else
            {
                previousVM.IsDiffCFCFree = false;
                currentVM.IsDiffCFCFree = false;
            }

            if (previousVM.UnlicensedMedication != currentVM.UnlicensedMedication)
            {
                previousVM.IsDiffUnlicensedMedication = true;
                currentVM.IsDiffUnlicensedMedication = true;
            }
            else
            {
                previousVM.IsDiffUnlicensedMedication = false;
                currentVM.IsDiffUnlicensedMedication = false;
            }

            if (previousVM.ParallelImport != currentVM.ParallelImport)
            {
                previousVM.IsDiffParallelImport = true;
                currentVM.IsDiffParallelImport = true;
            }
            else
            {
                previousVM.IsDiffParallelImport = false;
                currentVM.IsDiffParallelImport = false;
            }

            if (previousVM.IgnoreDuplicateWarnings != currentVM.IgnoreDuplicateWarnings)
            {
                previousVM.IsDiffIgnoreDuplicateWarnings = true;
                currentVM.IsDiffIgnoreDuplicateWarnings = true;
            }
            else
            {
                previousVM.IsDiffIgnoreDuplicateWarnings = false;
                currentVM.IsDiffIgnoreDuplicateWarnings = false;
            }

            if (previousVM.IsCustomControlledDrug != currentVM.IsCustomControlledDrug)
            {
                previousVM.IsDiffControlledDrug = true;
                currentVM.IsDiffControlledDrug = true;
            }
            else
            {
                previousVM.IsDiffControlledDrug = false;
                currentVM.IsDiffControlledDrug = false;
            }

            if (previousVM.IsPrescriptionPrintingRequired != currentVM.IsPrescriptionPrintingRequired)
            {
                previousVM.IsDiffPrescriptionPrintingRequired = true;
                currentVM.IsDiffPrescriptionPrintingRequired = true;
            }
            else
            {
                previousVM.IsDiffPrescriptionPrintingRequired = false;
                currentVM.IsDiffPrescriptionPrintingRequired = false;
            }

            if (previousVM.IsIndicationMandatory != currentVM.IsIndicationMandatory)
            {
                previousVM.IsDiffIndicationIsMandatory = true;
                currentVM.IsDiffIndicationIsMandatory = true;
            }
            else
            {
                previousVM.IsDiffIndicationIsMandatory = false;
                currentVM.IsDiffIndicationIsMandatory = false;
            }

            if (previousVM.WitnessingRequired != currentVM.WitnessingRequired)
            {
                previousVM.IsDiffWitnessingRequired = true;
                currentVM.IsDiffWitnessingRequired = true;
            }
            else
            {
                previousVM.IsDiffWitnessingRequired = false;
                currentVM.IsDiffWitnessingRequired = false;
            }

            if (previousOrCurrent.IsEmpty()) return Ok();

            string viewName = string.Empty;

            if (string.Compare(previousOrCurrent, "previous", true) == 0)
            {
                viewName = "_FormularyPreviousDetails";
                return PartialView(viewName, previousVM);
            }

            if (string.Compare(previousOrCurrent, "current", true) == 0)
            {
                viewName = "_FormularyCurrentDetails";
                return PartialView(viewName, currentVM);
            }

            return Ok();
        }

    }

}
