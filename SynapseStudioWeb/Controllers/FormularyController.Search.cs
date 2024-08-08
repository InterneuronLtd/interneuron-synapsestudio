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
using SynapseStudioWeb.DataService;
using SynapseStudioWeb.DataService.APIModel;
using SynapseStudioWeb.Helpers;
using SynapseStudioWeb.Models;
using SynapseStudioWeb.Models.MedicinalMgmt;
using SynapseStudioWeb.AppCode.Constants;

namespace SynapseStudioWeb.Controllers
{
    public partial class FormularyController : Controller
    {
        [HttpPost]
        public async Task<JsonResult> LoadFormularyList(FormularyListSearchCriteria searchCriteria)
        {
            var pageSize = 100;
            if (int.TryParse(_configuration["Settings:FormularyResultsPageSize"], out int configPageSize))
            {
                pageSize = configPageSize;
            }
            var paginatedVM = new FormularyTreePaginatedModel { PageSize = pageSize };

            string token = HttpContext.Session.GetString("access_token");

            HttpContext.Session.Remove(SynapseSession.FormularySearchResults);

            FormularyFilterCriteriaAPIModel filterCriteria = ConstructFormularyListFilterModel(searchCriteria);

            var recordStatusDictionary = HttpContext.Session.GetObject<Dictionary<string, string>>(SynapseSession.FormularyRecStatusLkpKey);

            var formularies = await InvokeSearchAPI(filterCriteria, token);

            if (!formularies.IsCollectionValid())
            {
                return Json(paginatedVM);
            }

            if (!recordStatusDictionary.IsCollectionValid())
            {
                var recordStatusLkp = await TerminologyAPIService.GetRecordStatusLookup(token);

                if (recordStatusLkp.IsCollectionValid())
                {
                    recordStatusDictionary = recordStatusLkp.Where(rec => rec.Recordstatus == 1).ToDictionary(k => k.Cd.ToString(), v => v.Desc);

                    HttpContext.Session.SetObject(SynapseSession.FormularyRecStatusLkpKey, recordStatusDictionary);
                }
            }

            await AppendWithFormularyIdVersion(formularies);

            var formulariesAsTreeModel = ConvertToFormularyTreeModel(formularies, recordStatusDictionary);

            paginatedVM.TotalRecords = formulariesAsTreeModel.Count;

            HttpContext.Session.SetObject(SynapseSession.FormularySearchResults, formulariesAsTreeModel);

            paginatedVM.Results = formulariesAsTreeModel.Skip(0).Take(pageSize).ToList();

            return (Json(paginatedVM));
        }

        [HttpPost]
        public async Task<JsonResult> LoadFormularyList_New([FromBody]FormularyListSearchCriteria searchCriteria)
        {
            var pageSize = 100;
            if (int.TryParse(_configuration["Settings:FormularyResultsPageSize"], out int configPageSize))
            {
                pageSize = configPageSize;
            }
            var paginatedVM = new FormularyTreePaginatedModel { PageSize = pageSize };

            string token = HttpContext.Session.GetString("access_token");

            HttpContext.Session.Remove(SynapseSession.FormularySearchResults);

            FormularyFilterCriteriaAPIModel filterCriteria = ConstructFormularyListFilterModel(searchCriteria);

            var recordStatusDictionary = HttpContext.Session.GetObject<Dictionary<string, string>>(SynapseSession.FormularyRecStatusLkpKey);

            var formularies = await InvokeSearchAPI(filterCriteria, token);

            if (!formularies.IsCollectionValid())
            {
                return Json(paginatedVM);
            }

            if (!recordStatusDictionary.IsCollectionValid())
            {
                var recordStatusLkp = await TerminologyAPIService.GetRecordStatusLookup(token);

                if (recordStatusLkp.IsCollectionValid())
                {
                    recordStatusDictionary = recordStatusLkp.Where(rec => rec.Recordstatus == 1).ToDictionary(k => k.Cd.ToString(), v => v.Desc);

                    HttpContext.Session.SetObject(SynapseSession.FormularyRecStatusLkpKey, recordStatusDictionary);
                }
            }

            await AppendWithFormularyIdVersion(formularies);

            var formulariesAsTreeModel = ConvertToFormularyTreeModel(formularies, recordStatusDictionary);

            paginatedVM.TotalRecords = formulariesAsTreeModel.Count;

            HttpContext.Session.SetObject(SynapseSession.FormularySearchResults, formulariesAsTreeModel);

            paginatedVM.Results = formulariesAsTreeModel.Skip(0).Take(pageSize).ToList();

            return (Json(paginatedVM));
        }

        private async Task AppendWithFormularyIdVersion(List<FormularyListAPIModel> formulariesInput)
        {
            if (!formulariesInput.IsCollectionValid()) return;

            var formularies = new List<FormularyListAPIModel>();
            
            formulariesInput.Each(rec=> formularies.Add(rec));

            var childrenList = new List<FormularyListAPIModel>();
            
            var childrenQueue = new Queue<FormularyListAPIModel>();

            formularies.Each(rec => {
                if (rec.Children.IsCollectionValid())
                {
                    childrenList.AddRange(rec.Children);
                    childrenQueue.Enqueue(rec);
                }
            });

            while(childrenQueue.IsCollectionValid())
            {
                var child = childrenQueue.Dequeue();

                if (child.Children.IsCollectionValid())
                {
                    childrenList.AddRange(child.Children);
                    child.Children.Each(rec=> childrenQueue.Enqueue(rec));
                }
            }

            if(childrenList.IsCollectionValid())
            {
                formularies.AddRange(childrenList);
            }

            var formularyIdsWithRecs = formularies
                .Select(rec=> new { FormularyId = rec.FormularyId, Data = rec })
                ?.Distinct(rec=> rec.FormularyId)
                .ToDictionary(k => k.FormularyId, v=> v.Data);

            var formularyIds = formularyIdsWithRecs.Keys.ToList();

            string token = HttpContext.Session.GetString("access_token");

            var formulariesResponse = await TerminologyAPIService.GetFormularyIdOrderInfoLookup(formularyIds, token);

            if (formulariesResponse.StatusCode != DataService.APIModel.StatusCode.Success)
                return;

            if (formulariesResponse.Data == null) return;

            foreach(var formularyIdOrderKey in formulariesResponse.Data.Keys)
            {
                if(formularyIdsWithRecs.ContainsKey(formularyIdOrderKey))
                {
                    formularyIdsWithRecs[formularyIdOrderKey].Name = $"{formularyIdsWithRecs[formularyIdOrderKey].Name} - v{formulariesResponse.Data[formularyIdOrderKey]}";
                }
            }

        }

        private async Task<List<FormularyListAPIModel>> InvokeSearchAPI(FormularyFilterCriteriaAPIModel filterCriteria, string token)
        {
            //MMC-477
            //Note: HideArchived is true by default and will be considered as filter only when set to false or used along with other filters

            //If has any search criteria - hit search api
            //if (filterCriteria.searchTerm.IsNotEmpty() || filterCriteria.formularyStatusCd.IsCollectionValid() || filterCriteria.IncludeDeleted || filterCriteria.recStatusCds.IsCollectionValid() || filterCriteria.Flags.IsCollectionValid() || filterCriteria.hideArchived || filterCriteria.ProductType.IsNotEmpty())
            if (filterCriteria.searchTerm.IsNotEmpty() || filterCriteria.formularyStatusCd.IsCollectionValid() || filterCriteria.IncludeDeleted || filterCriteria.recStatusCds.IsCollectionValid() || filterCriteria.Flags.IsCollectionValid() || !filterCriteria.hideArchived || filterCriteria.ProductType.IsNotEmpty())
            {
                var recStsCodes = new List<string>();

                filterCriteria.IncludeInvalid = true;

                if (filterCriteria.recStatusCds.IsCollectionValid())
                    recStsCodes = filterCriteria.recStatusCds;

                //if (filterCriteria.hideArchived)
                //{
                //    recStsCodes.Clear();
                //    recStsCodes.Add("004");
                //}

                if (recStsCodes.IsCollectionValid())
                    filterCriteria.recStatusCds = recStsCodes;

                var formulariesResponse = await TerminologyAPIService.SearchFormularies(filterCriteria, token);

                if (formulariesResponse.StatusCode != DataService.APIModel.StatusCode.Success)
                {
                    string errors = "Error getting the Formularies data.";

                    if (formulariesResponse.ErrorMessages.IsCollectionValid())
                        errors += string.Join('\n', formulariesResponse.ErrorMessages.ToArray());

                    _toastNotification.AddErrorToastMessage(errors);

                    return null;
                }
                return formulariesResponse.Data?.data;
            }
            else
            {
                var formulariesResponse = await TerminologyAPIService.GetLatestTopLevelFormulariesBasicInfo(token);

                if (formulariesResponse.StatusCode != DataService.APIModel.StatusCode.Success)
                {
                    string errors = "Error getting the Formularies data.";
                    if (formulariesResponse.ErrorMessages.IsCollectionValid())
                        errors += string.Join('\n', formulariesResponse.ErrorMessages.ToArray());

                    _toastNotification.AddErrorToastMessage(errors);
                    return null;
                }
                return formulariesResponse.Data;
            }
        }



        [HttpPost]
        public async Task<JsonResult> LoadChildrenFormularies(FormularyListSearchCriteria searchCriteria)
        {
            string token = HttpContext.Session.GetString("access_token");

            var apiRequest = new GetFormularyDescendentForCodesAPIRequest() { Codes = new List<string> { searchCriteria.FormularyCode }, OnlyNonDeleted = false };
            
            var formulariesResponse = await TerminologyAPIService.GetFormularyDescendentForCodes(apiRequest, token);

            var recordStatusDictionary = HttpContext.Session.GetObject<Dictionary<string, string>>(SynapseSession.FormularyRecStatusLkpKey);

            if (formulariesResponse.StatusCode != DataService.APIModel.StatusCode.Success)
            {
                string errors = "Error getting the Formularies data.";

                if (formulariesResponse.ErrorMessages.IsCollectionValid())
                    errors = errors + string.Join('\n', formulariesResponse.ErrorMessages.ToArray());

                _toastNotification.AddErrorToastMessage(errors);

                return Json(null);
            }

            if (formulariesResponse == null || !formulariesResponse.Data.IsCollectionValid())
                return Json(new List<FormularyTreeModel>());

            var formularyData = formulariesResponse.Data;

            if (searchCriteria.HideArchived == true)
            {
                formularyData = formularyData.Where(rec => rec.RecStatusCode != TerminologyConstants.ARCHIEVED_STATUS_CD).ToList();
            }

            await AppendWithFormularyIdVersion(formularyData);

            var vm = ConvertToFormularyTreeModel(formularyData, recordStatusDictionary);

            return Json(vm);
        }

        [HttpPost]
        public async Task<JsonResult> LoadChildrenFormularies_New([FromBody]FormularyListSearchCriteria searchCriteria)
        {
            string token = HttpContext.Session.GetString("access_token");

            //var apiRequest = new GetFormularyDescendentForCodesAPIRequest() { Codes = new List<string> { searchCriteria.FormularyCode }, OnlyNonDeleted = false };
            //var formulariesResponse = await TerminologyAPIService.GetFormularyDescendentForCodes(apiRequest, token);
            var apiRequest = new GetFormularyDescendentForFormularyVersionIdsAPIRequest() { FormularyVersionIds = new List<string> { searchCriteria.FormularyVersionId }, OnlyNonDeleted = false };
            var formulariesResponse = await TerminologyAPIService.GetFormularyImmediateDescendentsForFormularyVersionIds(apiRequest, token);

            var recordStatusDictionary = HttpContext.Session.GetObject<Dictionary<string, string>>(SynapseSession.FormularyRecStatusLkpKey);

            if (formulariesResponse.StatusCode != DataService.APIModel.StatusCode.Success)
            {
                string errors = "Error getting the Formularies data.";

                if (formulariesResponse.ErrorMessages.IsCollectionValid())
                    errors = errors + string.Join('\n', formulariesResponse.ErrorMessages.ToArray());

                _toastNotification.AddErrorToastMessage(errors);

                return Json(null);
            }

            if (formulariesResponse == null || !formulariesResponse.Data.IsCollectionValid())
                return Json(new List<FormularyTreeModel>());

            var formularyData = formulariesResponse.Data;

            if (searchCriteria.HideArchived == true)
            {
                formularyData = formularyData.Where(rec => rec.RecStatusCode != TerminologyConstants.ARCHIEVED_STATUS_CD).ToList();
            }

            await AppendWithFormularyIdVersion(formularyData);

            var vm = ConvertToFormularyTreeModel(formularyData, recordStatusDictionary);

            return Json(vm);
        }

        private List<FormularyTreeModel> ConvertToFormularyTreeModel(List<FormularyListAPIModel> apiModelList, Dictionary<string, string> recordStatusDictionary)
        {
            var treeList = new List<FormularyTreeModel>();

            if (!apiModelList.IsCollectionValid()) return treeList;

            foreach (FormularyListAPIModel result in apiModelList)
            {
                var formularyTree = new FormularyTreeModel();
                formularyTree.Data["Level"] = result.ProductType;
                formularyTree.Key = result.Code;
                formularyTree.Title = result.Name;
                formularyTree.FormularyVersionId = result.FormularyVersionId;
                formularyTree.FormularyId = result.FormularyId;
                formularyTree.Data["recordstatus"] = new FormaryLookupModel { IsDuplicate = result.IsDuplicate, Code = result.RecStatusCode, Description = result.RecStatusCode.IsNotEmpty() ? recordStatusDictionary[result.RecStatusCode] : null };
                formularyTree.Lazy = true;

                if (string.Compare(result.ProductType, "amp", true) == 0)
                    formularyTree.Children = new List<FormularyTreeModel>();
                else if (result.Children.IsCollectionValid())
                    formularyTree.Children = ConvertToFormularyTreeModel(result.Children, recordStatusDictionary);

                treeList.Add(formularyTree);
            }

            return treeList;
        }

        [HttpPost]
        public async Task<JsonResult> GetFormularyChangeLogForCodes([FromBody] List<string> dmdCodes)
        {
            if(!dmdCodes.IsCollectionValid()) return Json(null);

            string token = HttpContext.Session.GetString("access_token");

            TerminologyAPIResponse<List<FormularyChangeLogAPIModel>> formulariesResponse = await TerminologyAPIService.GetFormularyChangeLogForCodes(dmdCodes, token);

            if (formulariesResponse.StatusCode != DataService.APIModel.StatusCode.Success)
            {
                string errors = "Error getting the Formularies change log data.";

                if (formulariesResponse.ErrorMessages.IsCollectionValid())
                    errors = errors + string.Join('\n', formulariesResponse.ErrorMessages.ToArray());

                _toastNotification.AddErrorToastMessage(errors);

                return Json(null);
            }

            if (formulariesResponse == null || !formulariesResponse.Data.IsCollectionValid())
                return Json(new List<FormularyChangeLogAPIModel>());

            return Json(formulariesResponse.Data);
        }

        [HttpPost]
        public async Task<JsonResult> GetFormularyChangeLogForFormularyIds([FromBody] List<string> dmdCodes)
        {
            if (!dmdCodes.IsCollectionValid()) return Json(null);

            string token = HttpContext.Session.GetString("access_token");

            TerminologyAPIResponse<List<FormularyChangeLogAPIModel>> formulariesResponse = await TerminologyAPIService.GetFormularyChangeLogForCodes(dmdCodes, token);

            if (formulariesResponse.StatusCode != DataService.APIModel.StatusCode.Success)
            {
                string errors = "Error getting the Formularies change log data.";

                if (formulariesResponse.ErrorMessages.IsCollectionValid())
                    errors = errors + string.Join('\n', formulariesResponse.ErrorMessages.ToArray());

                _toastNotification.AddErrorToastMessage(errors);

                return Json(null);
            }

            if (formulariesResponse == null || !formulariesResponse.Data.IsCollectionValid())
                return Json(new Dictionary<string, string>());

            var formularyIdWithDelta = formulariesResponse.Data
                .Select(rec => new { rec.FormularyId, rec.DeltaDetail })
                .Distinct(rec => rec.FormularyId)
                .ToDictionary(k => k.FormularyId, v => v.DeltaDetail);

            return Json(formularyIdWithDelta);
        }

        [HttpPost]
        public async Task<JsonResult> GetFormularyChangeLogForCodesWithChangeDetailOnly([FromBody] List<string> dmdCodes)
        {
            if (!dmdCodes.IsCollectionValid()) return Json(null);

            string token = HttpContext.Session.GetString("access_token");

            TerminologyAPIResponse<Dictionary<string, string>> formulariesResponse = await TerminologyAPIService.GetFormularyChangeLogForCodesWithChangeDetailOnly(dmdCodes, token);

            if (formulariesResponse.StatusCode != DataService.APIModel.StatusCode.Success)
            {
                string errors = "Error getting the Formularies change log data.";

                if (formulariesResponse.ErrorMessages.IsCollectionValid())
                    errors = errors + string.Join('\n', formulariesResponse.ErrorMessages.ToArray());

                _toastNotification.AddErrorToastMessage(errors);

                return Json(null);
            }

            if (formulariesResponse == null || !formulariesResponse.Data.IsCollectionValid())
                return Json(new Dictionary<string, string> ());

            return Json(formulariesResponse.Data);
        }

        [HttpGet]
        public JsonResult GetFormulariesByPageNumber(int pageNumber, int? pageSizeIn)
        {
            var pageSize = 100;
            if (int.TryParse(_configuration["Settings:FormularyResultsPageSize"], out int configPageSize))
            {
                pageSize = configPageSize;
            }

            pageSize = pageSizeIn ?? pageSize;

            var formularies = HttpContext.Session.GetObject<List<FormularyTreeModel>>(SynapseSession.FormularySearchResults);

            if (!formularies.IsCollectionValid()) return Json(null);

            var viewModel = formularies.Skip(pageNumber == 0 ? 0 : ((pageNumber - 1) * pageSize)).Take(pageSize).ToList();

            return Json(viewModel);
        }

        private FormularyFilterCriteriaAPIModel ConstructFormularyListFilterModel(FormularyListSearchCriteria searchCriteria)
        {
            FormularyFilterCriteriaAPIModel filter = new FormularyFilterCriteriaAPIModel();

            filter.searchTerm = searchCriteria.SearchTerm ?? "";
            filter.hideArchived = searchCriteria.HideArchived ?? false;
            filter.recStatusCds = new List<string>();
            filter.formularyStatusCd = new List<string>();
            filter.Flags = new List<string>();
            filter.ProductType = searchCriteria.ProductType;
           
            if (searchCriteria.RecStatusCds != null)
            {
                AssignFilterByRecStatusCodes(filter, searchCriteria);
            }

            return filter;
        }

        private void AssignFilterByRecStatusCodes(FormularyFilterCriteriaAPIModel filter, FormularyListSearchCriteria searchCriteria)
        {
            if (searchCriteria.RecStatusCds.IndexOf(',') > -1)
            {
                filter.CategoryDifference = new CategoryDiffenceFilter();

                foreach (string status in searchCriteria.RecStatusCds.Split(','))
                {
                    if (status == "Duplicate")
                    {
                        filter.showOnlyDuplicate = true;
                    }
                    else if (status.Split('|')[0] == "Rec")
                    {
                        filter.recStatusCds.Add(status.Split('|')[1]);
                    }
                    else if (status.Split('|')[0] == "Form")
                    {
                        filter.formularyStatusCd.Add(status.Split('|')[1]);
                    }
                    else if (status.Split('|')[0] == "Flags")
                    {
                        filter.Flags.Add(status.Split('|')[1]);
                    }
                    else if (status.Split('|')[0] == "Cat")
                    {
                        var catgry = status.Split('|')[1];
                        if (catgry == "ProductDetails")
                            filter.CategoryDifference.IsDetailChanged = true;
                        if (catgry == "FlagsClassification")
                            filter.CategoryDifference.IsFlagsChanged = true;
                        if (catgry == "Guidance")
                            filter.CategoryDifference.IsGuidanceChanged = true;
                        if (catgry == "Posology")
                            filter.CategoryDifference.IsPosologyChanged = true;
                        if (catgry == "IsDeleted")
                            filter.CategoryDifference.IsDeleted = true;
                        if (catgry == "InValid")
                            filter.CategoryDifference.IsInvalid = true;
                    }
                }
            }
            else
            {
                filter.CategoryDifference = new CategoryDiffenceFilter();

                if (searchCriteria.RecStatusCds == "Duplicate")
                {
                    filter.showOnlyDuplicate = true;
                }
                else if (searchCriteria.RecStatusCds.IndexOf('|') > -1 && searchCriteria.RecStatusCds.Split('|')[0] == "Rec")
                {
                    filter.recStatusCds.Add(searchCriteria.RecStatusCds.Split('|')[1]);
                }
                else if (searchCriteria.RecStatusCds.IndexOf('|') > -1 && searchCriteria.RecStatusCds.Split('|')[0] == "Form")
                {
                    filter.formularyStatusCd.Add(searchCriteria.RecStatusCds.Split('|')[1]);
                }
                else if (searchCriteria.RecStatusCds.IndexOf('|') > -1 && searchCriteria.RecStatusCds.Split('|')[0] == "Flags")
                {
                    filter.Flags.Add(searchCriteria.RecStatusCds.Split('|')[1]);
                }
                else if (searchCriteria.RecStatusCds.IndexOf('|') > -1 && searchCriteria.RecStatusCds.Split('|')[0] == "Cat")
                {
                    UpdateCategoryDifference(filter, searchCriteria);
                    
                }
            }
        }

        private void UpdateCategoryDifference(FormularyFilterCriteriaAPIModel filter, FormularyListSearchCriteria searchCriteria)
        {   
            if (filter == null || searchCriteria == null || filter.CategoryDifference == null || searchCriteria.RecStatusCds.IsEmpty()) return;

            var ctgry = searchCriteria.RecStatusCds.Split('|')[1];

            if (ctgry == "ProductDetails")
                filter.CategoryDifference.IsDetailChanged = true;

            if (ctgry == "FlagsClassification")
                filter.CategoryDifference.IsFlagsChanged = true;

            if (ctgry == "Guidance")
                filter.CategoryDifference.IsGuidanceChanged = true;

            if (ctgry == "Posology")
                filter.CategoryDifference.IsPosologyChanged = true;

            if (ctgry == "IsDeleted")
                filter.CategoryDifference.IsDeleted = true;

            if (ctgry == "InValid")
                filter.CategoryDifference.IsInvalid = true;
        }
    }

    //public class Test
    //{
    //    public string Code { get; set; }
    //    public string Name { get; set; }
    //    public string ProductType { get; set; }
    //    public string ParentCode { get; set; }
    //    public bool HasDeleted { get; set; }
    //    public bool HasProductDetailChanged { get; set; }
    //    public bool HasProductFlagsChanged { get; set; }
    //    public bool HasProductGuidanceChanged { get; set; }
    //    public bool HasInvalidFlagChanged { get; set; }
    //    public bool HasProductPosologyChanged { get; set; }
    //    public string? ProductDetailChanges { get; set; }
    //    public string? ProductFlagsChanges { get; set; }
    //    public string? ProductInvalidChanges { get; set; }
    //    public string? ProductPosologyChanges { get; set; }
    //    public string? ProductGuidanceChanges { get; set; }
    //}

    //public class ProductAddnlCodes
    //{
    //    public string AdditionalCodeDesc { get; set; }
    //    public string AdditionalCode { get; set; }
    //}

    //public class ProductRoutes
    //{
    //    public string RouteCd{ get; set; } 
    //    public string RouteDesc { get; set; } 
    //    public string RouteFieldTypeCd { get; set; }
    //}

    //public class ProductIngredients
    //{
    //    public string BasisOfPharmaceuticalStrengthCd { get; set; } 
    //    public string BasisOfPharmaceuticalStrengthDesc { get; set; } 
    //    public string IngredientCd { get; set; } 
    //    public string IngredientName { get; set; } 
    //    public string StrengthValueDenominator { get; set; } 
    //    public string StrengthValueDenominatorUnitCd { get; set; } 
    //    public string StrengthValueDenominatorUnitDesc { get; set; } 
    //    public string StrengthValueNumerator { get; set; } 
    //    public string StrengthValueNumeratorUnitCd { get; set; } 
    //    public string StrengthValueNumeratorUnitDesc { get; set; }
    //}

    //public class ProductDetail
    //{
    //    public string Name { get; set; } 
    //    public string ParentCode { get; set; } 
    //    public string Prevcode { get; set; } 
    //    public string VtmId { get; set; } 
    //    public string VmpId { get; set; } 
    //    public string IsDmdInvalid { get; set; }
    //    public string IsDmdDeleted { get; set; }
    //    public string BasisOfPreferredNameCd { get; set; }
    //    public string BasisOfPreferredNameDesc { get; set; }
    //    public string CurrentLicensingAuthorityCd { get; set; }
    //    public string CurrentLicensingAuthorityDesc { get; set; }
    //    public string SupplierCd { get; set; }
    //    public string SupplierName { get; set; }
    //    public string DoseFormCd { get; set; }
    //    public string DoseFormDesc { get; set; }
    //    public string FormCd { get; set; }
    //    public string FormDesc { get; set; }
    //    public string UnitDoseFormSize { get; set; }
    //    public string UnitDoseFormUnits { get; set; }
    //    public string UnitDoseFormUnitsDesc { get; set; }
    //    public string UnitDoseUnitOfMeasureCd { get; set; }
    //    public string UnitDoseUnitOfMeasureDesc { get; set; }
    //    public string ControlledDrugCategoryCd { get; set; }
    //    public string ControlledDrugCategoryDesc { get; set; }
    //    public string RestrictionsOnAvailabilityCd { get; set; }
    //    public string RestrictionsOnAvailabilityDesc { get; set; }
    //    public string PrescribingStatusCd { get; set; }
    //    public string PrescribingStatusDesc { get; set; }
    //    public string EmaAdditionalMonitoring { get; set; }
    //    public string Prescribable { get; set; }
    //    public string SugarFree { get; set; }
    //    public string GlutenFree { get; set; }
    //    public string PreservativeFree { get; set; }
    //    public string CfcFree { get; set; }
    //    public string UnlicensedMedicationCd { get; set; }
    //    public string ParallelImport { get; set; }
    //}
}
