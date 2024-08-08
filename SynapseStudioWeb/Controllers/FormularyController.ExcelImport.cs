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
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ExcelDataReader;
using Interneuron.Common.Extensions;
using Interneuron.FDBAPI.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using SynapseStudioWeb.DataService;
using SynapseStudioWeb.DataService.APIModel;
using SynapseStudioWeb.Helpers;
using SynapseStudioWeb.Models;
using Microsoft.Extensions.Logging;

namespace SynapseStudioWeb.Controllers
{
    public partial class FormularyController : Controller
    {
        [HttpGet]
        public IActionResult ImportFile()
        {
            return View("FormularyExcelImport");
        }

        [HttpPost]
        [RequestFormLimits(MultipartBodyLengthLimit = 209715200)]
        [RequestSizeLimit(209715200)]
        public async Task<IActionResult> ExcelImport()
        {
            string token = HttpContext.Session.GetString("access_token");

            int.TryParse(_configuration["SynapseCore:Settings:FileImportBatchSize"], out int batSizeForFileFromConfig);

            var retries = 0;

            var responseStringBuilder = new StringBuilder();

            IFormFile xlFile = Request.Form.Files[0];

            var colmsConfigured = GetColumnDetailsFromConfig;

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var xlFileXtn = Path.GetExtension(xlFile.FileName).ToLower();

            if (xlFileXtn != ".xls" && xlFileXtn != ".xlsx") return StatusCode((int)HttpStatusCode.NotAcceptable);

            //var dirPath = CreateDirectory();

            if (xlFile.Length > 0)
            {
                //string fullPath = Path.Combine(dirPath, xlFile.FileName);

                //using (var stream = System.IO.File.Create(fullPath))
                //{
                //    await xlFile.CopyToAsync(stream);
                //}
                var requests = new List<FormularyHeaderAPIModel>();

                using (var reader = ExcelReaderFactory.CreateReader(xlFile.OpenReadStream()))
                {
                    var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                    {
                        ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                        {
                            UseHeaderRow = true,
                        }
                    }).Tables[0];// get the first sheet data with index 0.

                    var rowData = GetRowData(colmsConfigured);

                    //var totalBatches = (result.Rows.Count) / 500;

                    //List<int> source = Enumerable.Range(1, 19).ToList();
                    //int batchsize = 10;
                    //List<List<int>> batches = new List<List<int>>();
                    //for (int i = 0; i < source.Count; i += batchsize)
                    //{
                    //    var batch = source.Skip(i).Take(batchsize);
                    //    batches.Add(batch.ToList());
                    //}

                    var batchsize = batSizeForFileFromConfig;

                    var batchedRequests = new List<List<FormularyHeaderAPIModel>>();

                    for (var rowIndex = 0; rowIndex < result.Rows.Count; rowIndex++)
                    {
                        var row = result.Rows[rowIndex];

                        if (!CanProcessRow(row, colmsConfigured)) continue;

                        var requestRow = new FormularyHeaderAPIModel
                        {
                            Detail = new FormularyDetailAPIModel(),
                            FormularyRouteDetails = new List<FormularyRouteDetailAPIModel>()
                        };

                        colmsConfigured.Keys.Each(k =>
                        {
                            if (rowData.ContainsKey(k))
                                rowData[k](row, requestRow);
                        });

                        requests.Add(requestRow);
                    }

                    for (var reqIndex = 0; reqIndex < requests.Count; reqIndex += batchsize)
                    {
                        var batches = requests.Skip(reqIndex).Take(batchsize);
                        batchedRequests.Add(batches.ToList());
                    }

                    //for (var rowIndex = 0; rowIndex < result.Rows.Count; rowIndex++)
                    //for (var rowIndex = 0; rowIndex < result.Rows.Count; rowIndex += batchsize)
                    //{
                    //    var requests = new List<CreateFormularyAPIRequest>();

                    //    var row = result.Rows[rowIndex];

                    //    if (!CanProcessRow(row, colmsConfigured)) continue;

                    //    var requestRow = new CreateFormularyAPIRequest
                    //    {
                    //        Detail = new CreateFormularyDetailAPIRequest(),
                    //        FormularyRouteDetails = new List<CreateFormularyRouteDetailAPIRequest>()
                    //    };

                    //    colmsConfigured.Keys.Each(k =>
                    //    {
                    //        if (rowData.ContainsKey(k))
                    //            rowData[k](row, requestRow);
                    //    });

                    //    requests.Add(requestRow);

                    //    batchedRequests.Add(requests);

                    //    //var response = await DataService.TerminologyAPIService.FileImportMedication(requests, token);


                    //    //FormatResponseMessage(response, requestRow, responseStringBuilder);
                    //}

                    foreach (var batchReq in batchedRequests)
                    {
                        retries = 0;
                        try
                        {
                            var response = await DataService.TerminologyAPIService.FileImportMedication(batchReq, token);
                            FormatResponseMessageForBulk(response, responseStringBuilder);
                            await Task.Delay(200);

                            if (response.StatusCode == DataService.APIModel.StatusCode.Fail)
                                await RetryPosting(batchReq);
                        }
                        catch (Exception ex)
                        {
                            if (retries == 0)
                            {
                                await RetryPosting(batchReq);
                            }
                            else
                            {
                                throw ex;
                            }
                        }
                    }
                }

                async Task RetryPosting(List<FormularyHeaderAPIModel> batchReq)
                {
                    if (retries >= 3) throw new Exception("Exhausted re-tries during formulary file import");

                    try
                    {
                        retries++;
                        var response = await DataService.TerminologyAPIService.FileImportMedication(batchReq, token);
                        FormatResponseMessageForBulk(response, responseStringBuilder);

                        await Task.Delay(200);

                        if (response.StatusCode == DataService.APIModel.StatusCode.Fail)
                            await RetryPosting(batchReq);

                    }
                    catch (Exception ex)
                    {
                        if (retries <= 2)
                        {
                            await RetryPosting(batchReq);
                        }
                        else
                        {
                            throw ex;
                        }
                    }

                }

                var codesInExcel = new List<string>();

                if (requests.IsCollectionValid())
                {
                    codesInExcel = requests.Select(rec => rec.Code).ToList();
                }

                await UploadNonFormulariesInDMD(codesInExcel, token, responseStringBuilder);

                await DataService.TerminologyAPIService.InvokePostImportProcess(token);

            }

            //return Ok("Uploaded successfully");
            return Json(responseStringBuilder.ToString());
        }

        public async Task<IActionResult> UploadAllDMDToFormulary()
        {
            string token = HttpContext.Session.GetString("access_token");

            var response = await DataService.TerminologyAPIService.ImportAllMedsFromDMDWithRules(token);

            if (response == null || response.StatusCode == DataService.APIModel.StatusCode.Fail)
            {
                _toastNotification.AddErrorToastMessage("Error Importing DMD data to MMC System.");
                return Json("0");
            }

            return Json("1");
        }

        [HttpGet]
        public async Task<ActionResult<List<BackgroundTaskResultModel>>> GetUploadFilesStatusHistory()
        {
            string token = HttpContext.Session.GetString("access_token");

            var getTaskByNameResponse = await TerminologyAPIService.GetTaskByNames(new List<string> { "dmdfileupload" }, token);
            //var hasRunningTasks = getTaskByNameResponse?.Data?.Any(rec => rec.StatusCd == 1 || rec.StatusCd == 2);

            if (getTaskByNameResponse == null || getTaskByNameResponse?.Data == null) return NoContent();

            var vm = new List<BackgroundTaskResultModel>();

            var orderedResults = getTaskByNameResponse.Data;//.OrderByDescending(rec => rec.Updateddate).ToArray();

            foreach(var rec in orderedResults)
            {
                BackgroundTaskResultModel vmObj = GetCurrentStatus(rec);

                await GetStatusForFileUploadTask(vmObj, rec, token, false);

                //vmObj.Steps = GetRelevantStepsForDMD(rec);

                //if (rec.Detail.IsNotEmpty())
                //{
                //    var detail = JArray.Parse(rec.Detail)?.FirstOrDefault(det => det != null && det["seq"] != null && det["seq"].Value<string>() == "1");

                //    if (detail != null)
                //    {
                //        var filePath = detail["filepath"]?.Value<string>();
                //        vmObj.FileName = detail["filename"]?.Value<string>();
                //        var fileNameWithPath = detail["filenamewithpath"]?.Value<string>();
                //    }
                //}
                vm.Add(vmObj);
            }

            return Ok(vm);
        }

        [HttpPost]
        public async Task<ActionResult<List<BackgroundTaskResultModel>>> GetTasksByName([FromBody]List<string> taskNames)
        {
            if(!taskNames.IsCollectionValid()) return NoContent();

            string token = HttpContext.Session.GetString("access_token");

            var getTaskByNameResponse = await TerminologyAPIService.GetTaskByNames(taskNames, token);

            if (getTaskByNameResponse == null || getTaskByNameResponse?.Data == null) return NoContent();

            var vm = new List<BackgroundTaskResultModel>();

            var orderedResults = getTaskByNameResponse.Data;

            foreach (var rec in orderedResults)
            {
                BackgroundTaskResultModel vmObj = GetCurrentStatus(rec);
                vm.Add(vmObj);
            }

            return Ok(vm);
        }

        private BackgroundTaskResultModel GetCurrentStatus(BackgroundTaskAPIModel task)
        {
            if (task == null) return null;

            BackgroundTaskResultModel resultVm = new();

            resultVm.TaskId = task.TaskId;
            resultVm.Updateddate = task.Updateddate;
            resultVm.Createddate = task.Createddate;
            resultVm.Updatedtimestamp = task.Updatedtimestamp;
            resultVm.Createdtimestamp = task.Createdtimestamp;
            resultVm.Createdby = task.Createdby;
            resultVm.Name = task.Name;
            resultVm.StatusCd = task.StatusCd;

            if (string.Compare(task.Name, "dmdfileupload", true) == 0)
            {
                var fileDetail = task.Detail.IsNotEmpty() ? task.Detail.TryConvertToJArray()?.FirstOrDefault(det => det != null && det["seq"] != null && det["seq"].Value<string>() == "1") : null;

                var dmdFile = fileDetail?["filename"]?.Value<string>();
                var dmdBonusFile = fileDetail?["bonusfilename"]?.Value<string>();
                var bnfFile = fileDetail?["bnffilename"]?.Value<string>();

                resultVm.FileName = dmdFile;
                resultVm.AllFileNames = new List<string>();
                if (dmdFile.IsNotEmpty())
                    resultVm.AllFileNames.Add(dmdFile);
                if (dmdBonusFile.IsNotEmpty())
                    resultVm.AllFileNames.Add(dmdBonusFile);
                if (bnfFile.IsNotEmpty())
                    resultVm.AllFileNames.Add(bnfFile);
            }

            if (task.StatusCd == 1 || task.StatusCd == 2)
            {
                resultVm.IsStillRunning = true;
                resultVm.Status = "Processing...";
                resultVm.TaskExecutionNote = task.Detail;
            }
            else if (task.StatusCd == 3)
            {
                //var detail = task.Detail.IsNotEmpty() ? JArray.Parse(task.Detail)?.FirstOrDefault(det => det != null && det["seq"] != null && det["seq"].Value<string>() == "3") : null;

                resultVm.Status = "Completed Successfully.";
                resultVm.TaskExecutionNote = task.Detail;// detail?["reason"]?.Value<string>() + detail?["processmessage"]?.Value<string>();
            }
            else if (task.StatusCd == 4)
            {
                //var detail = task.Detail.IsNotEmpty() ? JArray.Parse(task.Detail)?.FirstOrDefault(det => det != null && det["seq"] != null && det["seq"].Value<string>() == "3") : null;

                resultVm.Status = "Failed Executing.";
                resultVm.TaskExecutionNote = task.Detail;// detail?["reason"]?.Value<string>() + detail?["processerror"]?.Value<string>() ?? detail?["reason"]?.Value<string>() + detail?["processmessage"]?.Value<string>();
            }
            return resultVm;
        }


        [HttpGet]
        public async Task<ActionResult<BackgroundTaskResultModel>> GetTaskByTaskId(string taskId)
        {
            if (taskId.IsEmpty()) return NoContent();

            string token = HttpContext.Session.GetString("access_token");

            var getTaskByNameResponse = await TerminologyAPIService.GetTaskByTaskId(taskId, token);

            if (getTaskByNameResponse == null || getTaskByNameResponse?.Data == null) return NoContent();

            var rec = getTaskByNameResponse.Data;

            BackgroundTaskResultModel vmObj = GetCurrentStatus(rec);
            return Ok(vmObj);
        }

        [HttpGet]
        public async Task<ActionResult<List<BackgroundTaskResultModel>>> GetTasksByCorrelationTaskId(string correlationTaskId)
        {
            if (correlationTaskId.IsEmpty()) return NoContent();

            string token = HttpContext.Session.GetString("access_token");

            var getTaskByNameResponse = await TerminologyAPIService.GetTasksByCorrelationTaskId(correlationTaskId, token);

            if (getTaskByNameResponse == null || getTaskByNameResponse?.Data == null) return NoContent();

            var rec = getTaskByNameResponse.Data;

            List<BackgroundTaskResultModel> vmObj = rec?.Select(sts=> GetCurrentStatus(sts))?.ToList();
            return Ok(vmObj);
        }

        [HttpGet]
        public async Task<ActionResult<BackgroundTaskResultModel>> GetFileUploadTasksStatus(string taskId)
        {
            if (taskId.IsEmpty()) return NoContent();

            string token = HttpContext.Session.GetString("access_token");

            var getTaskByNameResponse = await TerminologyAPIService.GetTaskByTaskId(taskId, token);

            if (getTaskByNameResponse == null || getTaskByNameResponse?.Data == null) return NoContent();

            var rec = getTaskByNameResponse.Data;

            BackgroundTaskResultModel vmObj = GetCurrentStatus(rec);

            await GetStatusForFileUploadTask(vmObj, rec, token, true);

            return Ok(vmObj);
        }

        private async Task GetStatusForFileUploadTask(BackgroundTaskResultModel vmObj, BackgroundTaskAPIModel rec, string token, bool shouldWaitForSubTask)
        {
            //get status of update to formulary
            if (rec.StatusCd == 3)
            {
                BackgroundTaskAPIModel tasksresponse = null;

                if (shouldWaitForSubTask)
                {//wait for sub task to start
                    var cnt = 0;

                    while (cnt < 5)
                    {
                        var getTasksByCorrelationResponse = await TerminologyAPIService.GetTasksByCorrelationTaskId(rec.TaskId, token);
                        tasksresponse = getTasksByCorrelationResponse?.Data?.FirstOrDefault(rec => rec.Name == "importdmdtoformulary");

                        if (tasksresponse != null) break;

                        await Task.Delay(TimeSpan.FromSeconds(100));
                        cnt++;
                    }
                }
                else
                {
                    var getTasksByCorrelationResponse = await TerminologyAPIService.GetTasksByCorrelationTaskId(rec.TaskId, token);
                    tasksresponse = getTasksByCorrelationResponse?.Data?.Where(rec => rec.Name == "importdmdtoformulary")?.FirstOrDefault();
                }

                //var getTasksByCorrelationResponse = await TerminologyAPIService.GetTasksByCorrelationTaskId(rec.TaskId, token);
                if(tasksresponse == null)
                {
                    vmObj.Status = "Partially completed. Same files might have been processed already or no changes in data.";
                    vmObj.IsStillRunning = false;
                }
                else if (tasksresponse.StatusCd == 1 || tasksresponse.StatusCd == 2)
                {
                    vmObj.StatusCd = 1;
                    vmObj.Status = "Updating formulary...";
                    vmObj.IsStillRunning = true;
                    vmObj.TaskExecutionNote = tasksresponse != null ? $"{vmObj.TaskExecutionNote}. {tasksresponse.Detail}" : vmObj.TaskExecutionNote;
                }
                else if (tasksresponse.StatusCd == 3)
                {
                    vmObj.StatusCd = 3;
                    vmObj.Status = "Completed Successfully.";
                    vmObj.IsStillRunning = false;
                    vmObj.TaskExecutionNote = $"{vmObj.TaskExecutionNote}. {tasksresponse.Detail}";
                }
                else if (tasksresponse.StatusCd == 4)
                {
                    vmObj.StatusCd = 4;
                    vmObj.Status = "Failed Executing.";
                    vmObj.IsStillRunning = false;
                    vmObj.TaskExecutionNote = $"{vmObj.TaskExecutionNote}. {tasksresponse.Detail}";
                }
            }
        }

        private List<Step> GetRelevantStepsForDMD(BackgroundTaskAPIModel apiRec)
        {
            var uploadFileProcessingSteps = GetDMDFIleUploadSteps(apiRec.StatusCd).OrderBy(rec => rec.StepCd).ToList();

            if (apiRec.StatusCd == 3 && uploadFileProcessingSteps.Count > 3)
                uploadFileProcessingSteps.RemoveAt(3);

            if (apiRec.StatusCd == 4 && uploadFileProcessingSteps.Count >= 3)
                uploadFileProcessingSteps.RemoveAt(2);

            return uploadFileProcessingSteps;
        }

        private List<Step> GetDMDFIleUploadSteps(int currentStep) => new() { new Step { StepCd = 1, Name = "File Uploaded", IsPending = (currentStep <= 1), IsCurrentStep = (currentStep == 1) }, new Step { StepCd = 2, Name = "Updating Terminology", IsCurrentStep = (currentStep == 2), IsPending = (currentStep <= 2), }, new Step { StepCd = 3, Name = "Updated Terminology", IsCurrentStep = (currentStep == 3), IsPending = (currentStep <= 3), IsSuccess = true }, new Step { StepCd = 4, Name = "Failed updating Terminology", IsCurrentStep = (currentStep == 4), IsPending = (currentStep <= 4), IsFail = true } };

        [HttpPost]
        [RequestFormLimits(MultipartBodyLengthLimit = 2097152000)]
        [RequestSizeLimit(2097152000)]
        public async Task<IActionResult> SyncDMDUsingFile()
        {
            try
            {
                string token = HttpContext.Session.GetString("access_token");

                var getTaskByNameResponse = await TerminologyAPIService.GetTaskByNames(new List<string> { "dmdfileupload" }, token);

                var existingData = getTaskByNameResponse?.Data;

                var hasRunningTasks = existingData?.Any(rec => rec.StatusCd == 1 || rec.StatusCd == 2);

                if (hasRunningTasks == true)
                {
                    _toastNotification.AddErrorToastMessage("Cannot upload the file, as the previous upload is still pending for process.");
                    return Json("Error: Cannot upload the file, as the previous upload is still pending for process.");
                }

                var files = Request.Form.Files;
                
                var (dmdFile, dmdBonusFile, bnfFile, error) = HasValidFilesUploaded(files);

                if (error.IsNotEmpty())
                {
                    _toastNotification.AddErrorToastMessage(error);
                    return Json(error);
                }
                /*
                                var file = Request.Form.Files[0];

                                if (file == null || file.Length == 0)
                                {
                                    _toastNotification.AddErrorToastMessage("file not selected");
                                    return Json("Error: file not selected");
                                }




                                var syncMode = Request.Form["syncMode"].ToString();

                                if (file == null || file.Length == 0)
                                {
                                    _toastNotification.AddErrorToastMessage("file not selected");
                                    return Json("Error: file not selected");
                                }

                                if (string.Compare(Path.GetExtension(file.FileName), ".zip", true) != 0)
                                {
                                    _toastNotification.AddErrorToastMessage("Upload the DMD Zip file downloaded from TRUD");
                                    return Json("Error: Upload the DMD Zip file downloaded from TRUD");
                                }

                                if (!file.FileName.StartsWith("nhsbsa_dmd_", StringComparison.OrdinalIgnoreCase))
                                {
                                    _toastNotification.AddErrorToastMessage("Upload the correct DMD file downloaded from TRUD");
                                    return Json("Error: Upload the correct DMD file downloaded from TRUD");
                                }
                */

                //check if the same version of dmd exists in FDB (if configured to import)
                (bool isSameVersion, string? dmdInFDB) = await HasSameVersionOfDMDInFDB(dmdFile.FileName);
                if (!isSameVersion)
                {
                    var msg = $"The DMD version being used in the FDB is different then the DMD file being uploaded. DMD version in FDB: {dmdInFDB ?? ""}.";
                    _toastNotification.AddErrorToastMessage(msg);
                    return Json($"Error: {msg}");
                }

                //if (DoesNewerVersionExists(file.FileName, existingData))
                if (DoesNewerVersionExists(dmdFile.FileName, existingData))
                {
                    _toastNotification.AddErrorToastMessage("A New version of the uploaded file has already been imported to the system.");
                    return Json("Error: A New version of the uploaded file has already been imported to the system.");
                }

                var (hasValidPath, uploadDir) = GetUploadDir();// Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\DMDUploads");
                if (!hasValidPath)
                {
                    _toastNotification.AddErrorToastMessage("Could not find any valid path to upload the DMD file");
                    return Json("Error: Could not find any valid path to upload the DMD file.");
                }

                await UploadFilesToPath(uploadDir, files);

                //var pathWithFileName = Path.Combine(uploadDir, file.FileName);
                //var pathWithFileName = Path.Combine(uploadDir, dmdFile.FileName);

                //using (var stream = new FileStream(pathWithFileName, FileMode.Create))
                //{
                //    await dmdFile.CopyToAsync(stream);
                //}


                //MMC-477
                var bnfLkpFile = files.Where(rec => rec.FileName.Contains("BNF_Code_Information", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

                var jObj = JObject.FromObject(new { seq = 1, stepname = "fileuploaded", filepath = uploadDir, filename = dmdFile.FileName, bonusfilename = dmdBonusFile.FileName, bnffilename = bnfFile.FileName, bnfLkpfilename = bnfLkpFile != null? bnfLkpFile.FileName: "" });
                
                JArray arr = new() { jObj };

                var request = new BackgroundTaskAPIModel { Name = "dmdfileupload", Status = "fileuploaded", StatusCd = 1, Detail = arr.ToString() };
                var response = await TerminologyAPIService.CreateTerminologyBGTask(request, token);

                //MMC-477
                //UnzipFile(uploadDir, file.FileName);

                //new TaskFactory().StartNew(() => ProcessDMDFile(uploadDir, file.FileName));
                //(statusCode, processMsg, processErrorMsg) = await ProcessDMDFile(uploadDir, file.FileName, syncMode);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                _toastNotification.AddErrorToastMessage(e.ToString());
                return Json("Error: Error processing the file.");// new { statusCd = 0, processText = processMsg, processError = processErrorMsg });
            }

            return Json("Success: Successfully uploaded the file for processing.");// new { statusCd = statusCode, processText = processMsg, processError = processErrorMsg });
        }

        private async Task<(bool isSameVersion, string? dmdInFDB)> HasSameVersionOfDMDInFDB(string uploadedFileName)
        {
            if (string.Compare(_configuration["SynapseCore:Settings:FormularyEnableFDBVersionCheck"] ?? "", "true", true) != 0) return (true, null);

            string token = HttpContext.Session.GetString("access_token");

            var baseFDBUrl = _configuration["FDB:BaseURL"];

            baseFDBUrl = baseFDBUrl.EndsWith("/") ? baseFDBUrl.TrimEnd('/') : baseFDBUrl;

            var fdbClient = new FDBAPIClient(baseFDBUrl);
            var fdbVersionResp =  await fdbClient.GetVersion(token);
            if (fdbVersionResp == null || fdbVersionResp.Data == null) return (true, null);

            if (uploadedFileName.IsEmpty()) return (false, null);

            var isSame = string.Compare(fdbVersionResp.Data.DMDVersion, uploadedFileName, true) == 0;

            return (isSame, fdbVersionResp.Data.DMDVersion);
        }

        private async Task UploadFilesToPath(string uploadDir, IFormFileCollection files)
        {
            foreach(var file in files)
            {
                var pathWithFileName = Path.Combine(uploadDir, file.FileName);
                using (var stream = new FileStream(pathWithFileName, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
            }

            try
            {
                foreach (var file in files)
                {
                    var cnt = 0;

                    while (cnt < 5)
                    {
                        if (System.IO.File.Exists(Path.Combine(uploadDir, file.FileName))) break;
                        await Task.Delay(TimeSpan.FromSeconds(10));
                        cnt++;
                    }
                }
            }
            catch { }
        }

        private (IFormFile dmdFile, IFormFile dmdBonusFile, IFormFile bnfFile, string error)  HasValidFilesUploaded(IFormFileCollection files)
        {
            if (files == null || files.Count < 3)
                return (null, null, null, "Error: Please upload all these files: DMD File 'nhsbsa_dmd_<version>.zip', DMD Supplementary file: nhsbsa_dmdbonus_<version>.zip and BNF File: BNF Snomed Mapping data <version>.zip");


            var dmdFile = files.Where(rec => rec.FileName.StartsWith("nhsbsa_dmd_", StringComparison.OrdinalIgnoreCase))?.FirstOrDefault();

            var dmdFileVer = dmdFile?.FileName.Replace("nhsbsa_dmd_", "", StringComparison.OrdinalIgnoreCase);

            var dmdBonusFile = files.Where(rec => rec.FileName.StartsWith("nhsbsa_dmdbonus_", StringComparison.OrdinalIgnoreCase))?.FirstOrDefault();
            
            var dmdBonusFileVer = dmdBonusFile?.FileName.Replace("nhsbsa_dmdbonus_", "", StringComparison.OrdinalIgnoreCase);

            var hasDiffVersions = (dmdFileVer.IsEmpty() || dmdBonusFileVer.IsEmpty() || dmdFileVer != dmdBonusFileVer);

            if (hasDiffVersions)
                return (null, null, null, "Error: Both DMD file and it's supplementary file needs to be uploaded and should be of same version");

            var bnfFile = files.Where(rec => rec.FileName.StartsWith("BNF Snomed Mapping data", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

            if (bnfFile == null)
                return (null, null, null, "Error: BNF Snomed Mapping data file should be uploaded");

            if (files.Any(file => string.Compare(Path.GetExtension(file.FileName), ".zip", true) != 0))
                return (null, null, null, "Error: Upload the Zip files of DMD, DMDBonus and BNF Snomed Mapping file");

            return (dmdFile, dmdBonusFile, bnfFile, null); ;
        }

        private bool DoesNewerVersionExists(string uploadedFileName, List<BackgroundTaskAPIModel> existingData)
        {
            //helps for back-filling of previous versions - stop at fileupload and do not enable localformulary process in bgtaskservice
            if (string.Compare(_configuration["SynapseCore:Settings:FormularyEnableFileVersionCheck"] ?? "", "true", true) != 0) return false;

            if (!existingData.IsCollectionValid()) return false;

            string pattern = @"\d{0,}.zip";
            //string input = @"nhsbsa_dmd_1.0.0_20230102000001.zip";
            var options = RegexOptions.Singleline;
            var uploadedFileMatch = Regex.Match(uploadedFileName, pattern, options);

            if (!uploadedFileMatch.Success || uploadedFileMatch.Value.IsEmpty()) return false;

            var uploadedFileMatchValue = uploadedFileMatch.Value.Replace(".zip", "");
            foreach (var task in existingData)
            {
                var fileDetail = task.Detail.IsNotEmpty() ? task.Detail.TryConvertToJArray()?.FirstOrDefault(det => det != null && det["seq"] != null && det["seq"].Value<string>() == "1") : null;

                var taskFileName = fileDetail?["filename"]?.Value<string>();

                if (taskFileName.IsNotEmpty())
                {
                    var taskFileMatch = Regex.Match(taskFileName, pattern, options);

                    if (taskFileMatch.Success && taskFileMatch.Value.IsNotEmpty())
                    {
                        var taskFileMatchValue = taskFileMatch.Value.Replace(".zip", "");
                        if (task.StatusCd == 3 && long.Parse(taskFileMatchValue) > long.Parse(uploadedFileMatchValue))
                            return true;
                    }
                }
            }

            return false;
        }

        private (bool hasValidUploadableFolder, string folderPath) GetUploadDir()
        {
            var filePathsAsStr = _configuration["MMCSyncDMDDBConfig:fileUploadPaths"];
            if (filePathsAsStr.IsEmpty()) return (false, null);
            var uploadableDirs = filePathsAsStr.Split("|");
            foreach (var dir in uploadableDirs)
            {
                try
                {
                    System.IO.File.Create(Path.Combine(dir, $"temp_{new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds()}.txt")).Close();
                    //System.IO.File.Delete(tempfilepath + "temp.txt");
                    return (true, dir);
                }
                catch { }
            }

            return (false, null);
        }

        private void UnzipFile(string uploadDir, string fileName)
        {
            var pathWithFileName = Path.Combine(uploadDir, fileName);
            uploadDir = Path.Combine(uploadDir, Path.GetFileNameWithoutExtension(fileName));
            ZipFile.ExtractToDirectory(pathWithFileName, uploadDir, true);
        }

        private async Task<(int status, string processText, string processError)> ProcessDMDFile(string uploadDir, string fileName, string syncMode = "auto")
        {
            var dmdVersion = GetDMDVersion(fileName);

            var dmdFilePath = $@"{Path.Combine(uploadDir, Path.GetFileNameWithoutExtension(fileName))}";
            dmdFilePath = dmdFilePath.Replace(@"\", "/");

            var dmdDb = _configuration["MMCSyncDMDDBConfig:dmdDb"];
            var dmdServer = _configuration["MMCSyncDMDDBConfig:dmdServer"];
            var dmdPort = _configuration["MMCSyncDMDDBConfig:dmdPort"]; ;
            var dmdSchema = _configuration["MMCSyncDMDDBConfig:dmdSchema"];
            var dmdUId = _configuration["MMCSyncDMDDBConfig:dmdUId"];
            var dmdPassword = _configuration["MMCSyncDMDDBConfig:dmdPassword"];
            var dmdStgDb = _configuration["MMCSyncDMDDBConfig:dmdStgDb"];
            var dmdStgServer = _configuration["MMCSyncDMDDBConfig:dmdStgServer"];
            var dmdStgPort = _configuration["MMCSyncDMDDBConfig:dmdStgPort"];
            var dmdStgSchema = _configuration["MMCSyncDMDDBConfig:dmdStgSchema"];
            var dmdStgUId = _configuration["MMCSyncDMDDBConfig:dmdStgUId"];
            var dmdStgPassword = _configuration["MMCSyncDMDDBConfig:dmdStgPassword"];

            var batchFileDir = Path.Combine(Directory.GetCurrentDirectory(), @"ETLJobs\DMDDeltaProcessor\dmd_delta_processor\dmd_delta_processor", "dmd_delta_processor_run.bat");

            var batArgs = $"--context_param dmd_version=\"{dmdVersion}\" --context_param dmd_db_additionalparams=  --context_param dmd_db_host=\"{dmdServer}\" --context_param dmd_db_name=\"{dmdDb}\" --context_param dmd_db_password=\"{dmdPassword}\" --context_param dmd_db_port={dmdPort} --context_param dmd_db_psql_path=  --context_param dmd_db_pwd_string=\"{dmdPassword}\" --context_param dmd_db_schema=\"{dmdSchema}\" --context_param dmd_db_script_path= --context_param dmd_db_user=\"{dmdUId}\" --context_param dmd_file_path=\"{dmdFilePath}\" --context_param dmd_db_stg_additionalparams=  --context_param dmd_db_stg_host=\"{dmdStgServer}\" --context_param dmd_db_stg_name=\"{dmdStgDb}\" --context_param dmd_db_stg_password=\"{dmdStgPassword}\" --context_param dmd_db_stg_port={dmdStgPort} --context_param dmd_db_stg_pwd_string=\"{dmdStgPassword}\" --context_param dmd_db_stg_schema=\"{dmdStgSchema}\" --context_param dmd_db_script_path= --context_param dmd_db_stg_user=\"{dmdStgUId}\"";


            //No need to execute the batch file
            //batchFileDir = Path.Combine(Directory.GetCurrentDirectory(), @"ETLJobs\DMDDeltaProcessor\dmd_delta_process.bat");
            //var command = $"dmd_delta_processor_run.bat {batArgs}";

            var psi = new ProcessStartInfo(batchFileDir)
            //var psi = new ProcessStartInfo("cmd.exe", "/c " + command)
            {
                //WorkingDirectory = Path.Combine(Directory.GetCurrentDirectory(), @"ETLJobs\DMDDeltaProcessor\dmd_delta_processor\dmd_delta_processor"),
                Arguments = batArgs,//dmdFilePath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WindowStyle = ProcessWindowStyle.Normal,
                UseShellExecute = false,
                //CreateNoWindow = true
            };

            var batchProcess = new Process();

            var consoleOutput = new StringBuilder();
            var errorOutput = new StringBuilder();

            ////Debuggging only or can be for realtime later
            batchProcess.OutputDataReceived += (s, d) =>
            {
                consoleOutput.AppendLine(d.Data);
            };

            batchProcess.ErrorDataReceived += (s, d) =>
            {
                errorOutput.AppendLine(d.Data);
            };

            batchProcess.StartInfo = psi;
            batchProcess.Start();

            batchProcess.BeginOutputReadLine();
            batchProcess.BeginErrorReadLine();

            var status = 0;

            batchProcess.WaitForExit();

            if (batchProcess.HasExited)
            {
                var exitCode = batchProcess.ExitCode;

                if (string.Compare(syncMode, "auto", true) == 0 && exitCode == 0)
                {
                    status = await ImportDeltasToFormulary();
                }
            }

            batchProcess.Close();

            return (status, consoleOutput.ToString(), errorOutput.ToString());
        }

        private void BatchProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private async Task<int> ImportDeltasToFormulary()
        {
            string token = HttpContext.Session.GetString("access_token");

            var response = await TerminologyAPIService.ImportDeltas(token);

            if (response == null || response.StatusCode == DataService.APIModel.StatusCode.Fail)
            {
                _toastNotification.AddErrorToastMessage("Error Importing DMD data to MMC System.");
                return 0;
            }

            return 1;
        }

        private string GetDMDVersion(string fileName)
        {
            var allVers = Regex.Split(fileName, @"[^0-9\.]+");

            if (allVers.IsCollectionValid())
            {
                var versionNo = allVers.FirstOrDefault(rec => rec.IsNotEmpty());
                return versionNo;
            }

            return "";
        }

        private async Task UploadNonFormulariesInDMD(List<string> codesInExcel, string token, StringBuilder responseStringBuilder)
        {
            int.TryParse(_configuration["SynapseCore:Settings:DMDCodeBatchSize"], out int batSizeForDMDFromConfig);

            var retries = 0;

            var allCodesInDMDResponse = await DataService.TerminologyAPIService.GetAllDMDCodes(token);

            if (allCodesInDMDResponse.StatusCode != DataService.APIModel.StatusCode.Success || !allCodesInDMDResponse.Data.IsCollectionValid()) return;

            var nonFormularyCodes = allCodesInDMDResponse.Data.Distinct().Except(codesInExcel).ToList();

            var batchsize = batSizeForDMDFromConfig;

            var batchedRequests = new List<List<string>>();

            for (var reqIndex = 0; reqIndex < nonFormularyCodes.Count; reqIndex += batchsize)
            {
                var batches = nonFormularyCodes.Skip(reqIndex).Take(batchsize);
                batchedRequests.Add(batches.ToList());
            }

            foreach (var batchReq in batchedRequests)
            {
                retries = 0;
                try
                {
                    var response = await DataService.TerminologyAPIService.ImportMeds(batchReq, token, "002", "003");//Non-formulary and Active
                                                                                                                     //FormatResponseMessageForBulk(response, responseStringBuilder);
                    await Task.Delay(200);

                    if (response.StatusCode == DataService.APIModel.StatusCode.Fail)
                        await RetryPosting(batchReq);
                }
                catch (Exception ex)
                {
                    if (retries == 0)
                    {
                        await RetryPosting(batchReq);
                    }
                    else
                    {
                        throw ex;
                    }
                }
            }

            async Task RetryPosting(List<string> batchReq)
            {
                if (retries >= 3) throw new Exception("Exhausted re-tries during formulary file import");

                try
                {
                    retries++;
                    var response = await DataService.TerminologyAPIService.ImportMeds(batchReq, token, "002", "003");//Non-formulary and Active
                                                                                                                     //FormatResponseMessageForBulk(response, responseStringBuilder);
                    await Task.Delay(200);

                    if (response.StatusCode == DataService.APIModel.StatusCode.Fail)
                        await RetryPosting(batchReq);
                }
                catch (Exception ex)
                {
                    if (retries <= 2)
                    {
                        await RetryPosting(batchReq);
                    }
                    else
                    {
                        throw ex;
                    }
                }

            }
        }

        private void FormatResponseMessageForBulk(TerminologyAPIResponse<CreateFormularyAPIResponse> response, StringBuilder responseStringBuilder)
        {
            if (response.StatusCode != DataService.APIModel.StatusCode.Success)
            {
                responseStringBuilder.AppendLine($"{string.Join('\n', response.ErrorMessages.ToArray())} \n");
            }

            else if (response.StatusCode == DataService.APIModel.StatusCode.Success)
            {
                if (response.Data.Status != null && response.Data.Status.ErrorMessages.IsCollectionValid())
                {
                    var errors = string.Join('\n', response.Data.Status.ErrorMessages.ToArray());

                    responseStringBuilder.AppendLine($"{errors} \n");
                }
                else
                {
                    if (response.Data != null && response.Data.Data != null)
                    {
                        response.Data.Data.Each(res =>
                        {
                            responseStringBuilder.AppendLine($"{res.Code}-Success \n");
                        });
                    }
                }
            }
        }

        private bool CanProcessRow(DataRow row, Dictionary<string, string> colsNames)
        {
            //if (row != null && (row[colsNames[ColumnKeyNames.FORMULARYSTATUS]].IsNotNull() && row[colsNames[ColumnKeyNames.FORMULARYSTATUS]].ToString().IsNotEmpty()) && (
            //    string.Compare(row[colsNames[ColumnKeyNames.FORMULARYSTATUS]].ToString().Trim(), "yes", true) == 0 || string.Compare(row[colsNames[ColumnKeyNames.FORMULARYSTATUS]].ToString().Trim(), "no", true) == 0))
            //    return true;

            //Can import both formulary and non-formulary
            if (row != null) return true;

            return false;
        }

        //private void FormatResponseMessage(TerminiologyAPIResponse<CreateFormularyAPIResponse> response, CreateFormularyAPIRequest requestRow, StringBuilder responseStringBuilder)
        //{
        //    if (response.StatusCode != DataService.APIModel.StatusCode.Success)
        //    {
        //        responseStringBuilder.AppendLine($"{requestRow.Code}-{string.Join('\n', response.ErrorMessages.ToArray())}");
        //    }

        //    else if (response.StatusCode == DataService.APIModel.StatusCode.Success)
        //    {
        //        if (response.Data.Status != null && response.Data.Status.ErrorMessages.IsCollectionValid())
        //        {
        //            var errors = string.Join('\n', response.Data.Status.ErrorMessages.ToArray());

        //            responseStringBuilder.AppendLine($"{requestRow.Code}-{errors}");
        //        }
        //        else
        //        {
        //            responseStringBuilder.AppendLine($"{requestRow.Code}-Success");
        //        }
        //    }

        //}

        private Dictionary<string, Action<DataRow, FormularyHeaderAPIModel>> GetRowData(Dictionary<string, string> colsNames)
        {
            const char DataDelimiter = '|';

            var dataDictionary = new Dictionary<string, Action<DataRow, FormularyHeaderAPIModel>>();

            dataDictionary.Add(ColumnKeyNames.Name, (dr, apiModel) =>
            {
                if (dr[colsNames[ColumnKeyNames.Name]].IsNotNull() && dr[colsNames[ColumnKeyNames.Name]].ToString().IsNotEmpty())
                    apiModel.Name = dr[colsNames[ColumnKeyNames.Name]]?.ToString()?.Trim();
            });
            dataDictionary.Add(ColumnKeyNames.Code, (dr, apiModel) =>
            {
                if (dr[colsNames[ColumnKeyNames.Code]].IsNotNull() && dr[colsNames[ColumnKeyNames.Code]].ToString().IsNotEmpty())
                    apiModel.Code = dr[colsNames[ColumnKeyNames.Code]]?.ToString()?.Trim();
            });
            dataDictionary.Add(ColumnKeyNames.Level, (dr, apiModel) =>
            {
                if (dr[colsNames[ColumnKeyNames.Level]].IsNotNull() && dr[colsNames[ColumnKeyNames.Level]].ToString().IsNotEmpty())
                    apiModel.ProductType = dr[colsNames[ColumnKeyNames.Level]]?.ToString()?.Trim();
            });
            dataDictionary.Add(ColumnKeyNames.ParentCode, (dr, apiModel) =>
            {
                if (dr[colsNames[ColumnKeyNames.ParentCode]].IsNotNull() && dr[colsNames[ColumnKeyNames.ParentCode]].ToString().IsNotEmpty())
                    apiModel.ParentCode = dr[colsNames[ColumnKeyNames.ParentCode]]?.ToString()?.Trim();
            });
            dataDictionary.Add(ColumnKeyNames.ParentLevel, (dr, apiModel) =>
            {
                if (dr[colsNames[ColumnKeyNames.ParentLevel]].IsNotNull() && dr[colsNames[ColumnKeyNames.ParentLevel]].ToString().IsNotEmpty())
                    apiModel.ParentProductType = dr[colsNames[ColumnKeyNames.ParentLevel]]?.ToString()?.Trim();
            });

            dataDictionary.Add(ColumnKeyNames.FormCode, (dr, apiModel) =>
            {
                if (dr[colsNames[ColumnKeyNames.FormCode]].IsNotNull() && dr[colsNames[ColumnKeyNames.FormCode]].ToString().IsNotEmpty())
                    apiModel.Detail.FormCd = dr[colsNames[ColumnKeyNames.FormCode]]?.ToString()?.Trim();
            });
            dataDictionary.Add(ColumnKeyNames.ControlledDrugStatusCode, (dr, apiModel) =>
            {
                if (dr[colsNames[ColumnKeyNames.ControlledDrugStatusCode]].IsNotNull() && dr[colsNames[ColumnKeyNames.ControlledDrugStatusCode]].ToString().IsNotEmpty())
                    apiModel.Detail.ControlledDrugCategories = new List<FormularyLookupAPIModel> { new FormularyLookupAPIModel { Cd = dr[colsNames[ColumnKeyNames.ControlledDrugStatusCode]]?.ToString()?.Trim() } };
            });

            dataDictionary.Add(ColumnKeyNames.PrescribingStatusCode, (dr, apiModel) =>
            {
                if (dr[colsNames[ColumnKeyNames.PrescribingStatusCode]].IsNotNull() && dr[colsNames[ColumnKeyNames.PrescribingStatusCode]].ToString().IsNotEmpty())
                    apiModel.Detail.PrescribingStatusCd = dr[colsNames[ColumnKeyNames.PrescribingStatusCode]]?.ToString()?.Trim();
            });

            dataDictionary.Add(ColumnKeyNames.SupplierCode, (dr, apiModel) =>
            {
                if (dr[colsNames[ColumnKeyNames.SupplierCode]].IsNotNull() && dr[colsNames[ColumnKeyNames.SupplierCode]].ToString().IsNotEmpty())
                    apiModel.Detail.SupplierCd = dr[colsNames[ColumnKeyNames.SupplierCode]]?.ToString()?.Trim();
            });
            dataDictionary.Add(ColumnKeyNames.FORMULARYSTATUS, (dr, apiModel) =>
            {
                if (dr[colsNames[ColumnKeyNames.FORMULARYSTATUS]].IsNotNull() && dr[colsNames[ColumnKeyNames.FORMULARYSTATUS]].ToString().IsNotEmpty())
                {
                    if (string.Compare(dr[colsNames[ColumnKeyNames.FORMULARYSTATUS]].ToString().Trim(), "yes", true) == 0)
                    {
                        apiModel.Detail.RnohFormularyStatuscd = "001";
                    }
                    else
                    {
                        apiModel.Detail.RnohFormularyStatuscd = "002";
                    }
                //else if (string.Compare(dr[colsNames[ColumnKeyNames.FORMULARYSTATUS]].ToString().Trim(), "no", true) == 0)
                //{
                //    apiModel.Detail.RnohFormularyStatuscd = "002";
                //}
                }
                else
                {
                    apiModel.Detail.RnohFormularyStatuscd = "002";
                }
            // apiModel.Detail.RnohFormularyStatuscd = string.Compare(dr[colsNames[ColumnKeyNames.FORMULARYSTATUS]].ToString().Trim(), "yes", true) == 0 ? "001" : "002";
            });

            dataDictionary.Add(ColumnKeyNames.LicensedRouteCodes, (dr, apiModel) =>
            {
                if (dr[colsNames[ColumnKeyNames.LicensedRouteCodes]].IsNotNull() && dr[colsNames[ColumnKeyNames.LicensedRouteCodes]].ToString().IsNotEmpty())
                {
                    var routeCodes = dr[colsNames[ColumnKeyNames.LicensedRouteCodes]].ToString().Split(DataDelimiter);

                    routeCodes.Each(routeCode =>
                    {

                        if (routeCode.IsNotEmpty())
                        {
                            var routeRequest = new FormularyRouteDetailAPIModel
                            {
                                RouteCd = routeCode,
                                RouteFieldTypeCd = "003"//Normal or Licensed
                            };
                            apiModel.FormularyRouteDetails.Add(routeRequest);
                        }
                    });
                }
            });

            dataDictionary.Add(ColumnKeyNames.UnlicensedRoutesCodes, (dr, apiModel) =>
            {
                if (dr[colsNames[ColumnKeyNames.UnlicensedRoutesCodes]].IsNotNull() && dr[colsNames[ColumnKeyNames.UnlicensedRoutesCodes]].ToString().IsNotEmpty())
                {
                    var routeCodes = dr[colsNames[ColumnKeyNames.UnlicensedRoutesCodes]].ToString().Split(DataDelimiter);

                    routeCodes.Each(routeCode =>
                    {

                        if (routeCode.IsNotEmpty())
                        {
                            var routeRequest = new FormularyRouteDetailAPIModel
                            {
                                RouteCd = routeCode,
                                RouteFieldTypeCd = "002"//UnLicensed
                            };
                            apiModel.FormularyRouteDetails.Add(routeRequest);
                        }
                    });
                }
            });

            dataDictionary.Add(ColumnKeyNames.CriticalDrug, (dr, apiModel) =>
            {
                if (dr[colsNames[ColumnKeyNames.CriticalDrug]].IsNotNull() && dr[colsNames[ColumnKeyNames.CriticalDrug]].ToString().IsNotEmpty())
                    apiModel.Detail.CriticalDrug = string.Compare(dr[colsNames[ColumnKeyNames.CriticalDrug]].ToString().Trim(), "yes", true) == 0 ? "1" : "0";
            });

            dataDictionary.Add(ColumnKeyNames.CYTOTOXIC, (dr, apiModel) =>
            {
                if (dr[colsNames[ColumnKeyNames.CYTOTOXIC]].IsNotNull() && dr[colsNames[ColumnKeyNames.CYTOTOXIC]].ToString().IsNotEmpty())
                    apiModel.Detail.Cytotoxic = string.Compare(dr[colsNames[ColumnKeyNames.CYTOTOXIC]].ToString().Trim(), "yes", true) == 0 ? "1" : "0";
            });

            dataDictionary.Add(ColumnKeyNames.BLACKTRIANGLE, (dr, apiModel) =>
            {
                if (dr[colsNames[ColumnKeyNames.BLACKTRIANGLE]].IsNotNull() && dr[colsNames[ColumnKeyNames.BLACKTRIANGLE]].ToString().IsNotEmpty())
                    apiModel.Detail.BlackTriangle = string.Compare(dr[colsNames[ColumnKeyNames.BLACKTRIANGLE]].ToString().Trim(), "yes", true) == 0 ? "1" : "0";
            });
            //dataDictionary.Add(ColumnKeyNames.RESTRICTEDPRESCRIBING, (dr, apiModel) =>
            //{
            //    if (dr[colsNames[ColumnKeyNames.RESTRICTEDPRESCRIBING]].IsNotNull() && dr[colsNames[ColumnKeyNames.RESTRICTEDPRESCRIBING]].ToString().IsNotEmpty())
            //        apiModel.Detail.RestrictedPrescribing = string.Compare(dr[colsNames[ColumnKeyNames.RESTRICTEDPRESCRIBING]].ToString().Trim(), "yes", true) == 0 ? "1" : "0";
            //});
            dataDictionary.Add(ColumnKeyNames.NOTESFORRESTRICTION, (dr, apiModel) =>
            {
                if (dr[colsNames[ColumnKeyNames.NOTESFORRESTRICTION]].IsNotNull() && dr[colsNames[ColumnKeyNames.NOTESFORRESTRICTION]].ToString().IsNotEmpty())
                    apiModel.Detail.RestrictionNote = dr[colsNames[ColumnKeyNames.NOTESFORRESTRICTION]]?.ToString()?.Trim();
            });

            dataDictionary.Add(ColumnKeyNames.MEDUSIVGUIDE, (dr, apiModel) =>
            {
                if (dr[colsNames[ColumnKeyNames.MEDUSIVGUIDE]].IsNotNull() && dr[colsNames[ColumnKeyNames.MEDUSIVGUIDE]].ToString().IsNotEmpty())
                    apiModel.Detail.MedusaPreparationInstructions = new List<string> { dr[colsNames[ColumnKeyNames.MEDUSIVGUIDE]]?.ToString()?.Trim() };
            });

            return dataDictionary;
        }

        //private string CreateDirectory()
        //{
        //    string folderName = "ImportedFiles";
        //    string webRootPath = _hostingEnvironment.WebRootPath;
        //    string newPath = Path.Combine(webRootPath, folderName);

        //    if (!Directory.Exists(newPath))
        //    {
        //        Directory.CreateDirectory(newPath);
        //    }

        //    return newPath;
        //}

        private Dictionary<string, string> GetColumnDetailsFromConfig => _configuration.GetSection("MMC_Excel_Import_Cols").GetChildren().ToList().ToDictionary(c => c.Key, c => c.Value);

        private class ColumnKeyNames
        {
            public const string Name = "Name";
            public const string Code = "Code";
            public const string Level = "Level";

            public const string FormCode = "FormCode";
            public const string ControlledDrugStatusCode = "ControlledDrugStatusCode";
            public const string PrescribingStatusCode = "PrescribingStatusCode";
            public const string SupplierCode = "SupplierCode";
            public const string ParentCode = "ParentCode";
            public const string ParentLevel = "ParentLevel";
            public const string FORMULARYSTATUS = "FORMULARYSTATUS";
            public const string LicensedRoute = "LicensedRoute";
            public const string LicensedRouteCodes = "LicensedRouteCodes";//New
            public const string AdditonalRoutes = "ADDITIONAL_ROUTE";
            public const string UnlicensedRoutes = "UNLICENSED_ROUTES";
            public const string UnlicensedRoutesCodes = "UnlicensedRoutesCodes";//New

            public const string CriticalDrug = "CRITICAL_DRUG";
            public const string CYTOTOXIC = "CYTOTOXIC";
            public const string BLACKTRIANGLE = "BLACKTRIANGLE";
            public const string RESTRICTEDPRESCRIBING = "RESTRICTEDPRESCRIBING";
            public const string NOTESFORRESTRICTION = "NOTESFORRESTRICTION";
            public const string MEDUSIVGUIDE = "MEDUSAIVGUIDE";


        }
    }
}
