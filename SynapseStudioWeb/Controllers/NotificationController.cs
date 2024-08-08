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
﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using SynapseStudioWeb.DataService.APIModel.Notification;
using SynapseStudioWeb.DataService.NotificationService;
using NToastNotify;
using Interneuron.Common.Extensions;
using SynapseStudioWeb.DataService;
using SynapseStudioWeb.Models;
using System.Linq;

namespace SynapseStudioWeb.Controllers
{
    public class NotificationController : Controller
    {
        private readonly IToastNotification _toastNotification;
        private const string UNKNOWN_SAVE_STATUS_MSG = "Unknown error saving the status.";

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddNotificationConfiguration([FromBody]NotificationConfigurationAPIModel notificationconfiguration)
        {
            string token = HttpContext.Session.GetString("access_token");

            //var request = Newtonsoft.Json.JsonConvert.DeserializeObject<NotificationConfigurationAPIModel>(notificationconfiguration);

            NotificationConfigurationAPIModel request = new NotificationConfigurationAPIModel();

            request = notificationconfiguration;

            var resultResponse = await NotificationAPIService.AddNotificationConfiguration(request, token);

            if (resultResponse == null)
            {
                //_toastNotification.AddErrorToastMessage(UNKNOWN_SAVE_STATUS_MSG);
                return StatusCode(500, UNKNOWN_SAVE_STATUS_MSG);
            }

            if (resultResponse.StatusCode != SynapseStudioWeb.DataService.APIModel.Notification.StatusCode.Success)
            {
                string errors = "Error while adding notification configuration.";

                if (resultResponse.ErrorMessages.IsCollectionValid())
                    errors += string.Join(". ", resultResponse.ErrorMessages.ToArray());

                //_toastNotification.AddErrorToastMessage(errors);

                return StatusCode(500, errors);
            }

            if (resultResponse.StatusCode == SynapseStudioWeb.DataService.APIModel.Notification.StatusCode.Success)
            {
                if (resultResponse.Data != null && resultResponse.ErrorMessages.IsCollectionValid())
                {
                    var errors = string.Join(". ", resultResponse.Data);

                    //_toastNotification.AddInfoToastMessage(errors);
                    return StatusCode(500, errors);
                }
            }

            return Json(Ok());
        }

        [HttpGet]
        public async Task<IActionResult> GetAllNotificationConfiguration()
        {
            string token = HttpContext.Session.GetString("access_token");

            var resultResponse = await NotificationAPIService.GetAllNotificationConfiguration(token);

            if (resultResponse == null)
            {
                //_toastNotification.AddErrorToastMessage(UNKNOWN_SAVE_STATUS_MSG);
                return StatusCode(500, UNKNOWN_SAVE_STATUS_MSG);
            }

            if (resultResponse.StatusCode != SynapseStudioWeb.DataService.APIModel.Notification.StatusCode.Success)
            {
                string errors = "Error while adding notification configuration.";

                if (resultResponse.ErrorMessages.IsCollectionValid())
                    errors += string.Join('\n', resultResponse.ErrorMessages.ToArray());

                //_toastNotification.AddErrorToastMessage(errors);

                return StatusCode(500, errors);
            }

            if (resultResponse.StatusCode == SynapseStudioWeb.DataService.APIModel.Notification.StatusCode.Success)
            {
                if (resultResponse.Data != null && resultResponse.ErrorMessages.IsCollectionValid())
                {
                    var errors = string.Join('\n', resultResponse.Data);

                    //_toastNotification.AddInfoToastMessage(errors);
                    return StatusCode(500, errors);
                }
            }

            return Json(resultResponse.Data);
        }

        [HttpPost]
        public async Task<IActionResult> GetNotificationConfigurationById([FromBody]string id)
        {
            string token = HttpContext.Session.GetString("access_token");

            var resultResponse = await NotificationAPIService.GetNotificationConfigurationById(token, id);

            if (resultResponse == null)
            {
                //_toastNotification.AddErrorToastMessage(UNKNOWN_SAVE_STATUS_MSG);
                return StatusCode(500, UNKNOWN_SAVE_STATUS_MSG);
            }

            if (resultResponse.StatusCode != SynapseStudioWeb.DataService.APIModel.Notification.StatusCode.Success)
            {
                string errors = "Error while adding notification configuration.";

                if (resultResponse.ErrorMessages.IsCollectionValid())
                    errors += string.Join('\n', resultResponse.ErrorMessages.ToArray());

                //_toastNotification.AddErrorToastMessage(errors);

                return StatusCode(500, errors);
            }

            if (resultResponse.StatusCode == SynapseStudioWeb.DataService.APIModel.Notification.StatusCode.Success)
            {
                if (resultResponse.Data != null && resultResponse.ErrorMessages.IsCollectionValid())
                {
                    var errors = string.Join('\n', resultResponse.Data);

                    //_toastNotification.AddInfoToastMessage(errors);
                    return StatusCode(500, errors);
                }
            }

            return Json(resultResponse.Data);
        }

        [HttpGet]
        public async Task<List<ModulesModel>> GetModulesList()
        {
            string token = HttpContext.Session.GetString("access_token");
            var modulelist = await APIservice.GetListAsync<ModulesModel>("synapsenamespace=meta&synapseentityname=module", token);
            return modulelist?.OrderBy(rec => rec.modulename)?.ToList();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteNotificationConfigurationById([FromBody] string id)
        {
            string token = HttpContext.Session.GetString("access_token");

            var resultResponse = await NotificationAPIService.DeleteNotificationConfigurationById(token, id);

            if (resultResponse == null)
            {
                //_toastNotification.AddErrorToastMessage(UNKNOWN_SAVE_STATUS_MSG);
                return StatusCode(500, UNKNOWN_SAVE_STATUS_MSG);
            }

            if (resultResponse.StatusCode != SynapseStudioWeb.DataService.APIModel.Notification.StatusCode.Success)
            {
                string errors = "Error while adding notification configuration.";

                if (resultResponse.ErrorMessages.IsCollectionValid())
                    errors += string.Join('\n', resultResponse.ErrorMessages.ToArray());

                //_toastNotification.AddErrorToastMessage(errors);

                return StatusCode(500, errors);
            }

            if (resultResponse.StatusCode == SynapseStudioWeb.DataService.APIModel.Notification.StatusCode.Success)
            {
                if (resultResponse.Data != null && resultResponse.ErrorMessages.IsCollectionValid())
                {
                    var errors = string.Join('\n', resultResponse.Data);

                    //_toastNotification.AddInfoToastMessage(errors);
                    return StatusCode(500, errors);
                }
            }

            return Json(resultResponse.Data);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllNotificationLookup()
        {
            string token = HttpContext.Session.GetString("access_token");

            var resultResponse = await NotificationAPIService.GetAllNotificationLookup(token);

            if (resultResponse == null)
            {
                //_toastNotification.AddErrorToastMessage(UNKNOWN_SAVE_STATUS_MSG);
                return StatusCode(500, UNKNOWN_SAVE_STATUS_MSG);
            }

            if (resultResponse.StatusCode != SynapseStudioWeb.DataService.APIModel.Notification.StatusCode.Success)
            {
                string errors = "Error while adding notification configuration.";

                if (resultResponse.ErrorMessages.IsCollectionValid())
                    errors += string.Join('\n', resultResponse.ErrorMessages.ToArray());

                //_toastNotification.AddErrorToastMessage(errors);

                return StatusCode(500, errors);
            }

            if (resultResponse.StatusCode == SynapseStudioWeb.DataService.APIModel.Notification.StatusCode.Success)
            {
                if (resultResponse.Data != null && resultResponse.ErrorMessages.IsCollectionValid())
                {
                    var errors = string.Join('\n', resultResponse.Data);

                    //_toastNotification.AddInfoToastMessage(errors);
                    return StatusCode(500, errors);
                }
            }

            return Json(resultResponse.Data);
        }
    }
}
