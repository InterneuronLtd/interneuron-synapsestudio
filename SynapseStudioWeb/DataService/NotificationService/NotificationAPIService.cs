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
﻿using Interneuron.Common.Extensions;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using SynapseStudioWeb.DataService.APIModel.Notification;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SynapseStudioWeb.DataService.NotificationService
{
    public class NotificationAPIService
    {
        const string NotificationAPI_URI = "api/v1";

        public static async Task<NotificationAPIResponse<NotificationConfigurationAPIModel>> AddNotificationConfiguration(NotificationConfigurationAPIModel notificationConfiguration, string token)
        {
            var results = await InvokeService<NotificationConfigurationAPIModel>($"{NotificationAPI_URI}/NotificationConfiguration/", token, HttpMethod.Post, notificationConfiguration);

            return results;
        }

        public static async Task<NotificationAPIResponse<List<NotificationTypeAPIModel>>> GetAllNotificationConfiguration(string token)
        {
            var results = await InvokeService<List<NotificationTypeAPIModel>>($"{NotificationAPI_URI}/NotificationConfiguration/all", token, HttpMethod.Get);

            return results;
        }

        public static async Task<NotificationAPIResponse<NotificationTypeAPIModel>> GetNotificationConfigurationById(string token, string id)
        {
            var results = await InvokeService<NotificationTypeAPIModel>($"{NotificationAPI_URI}/NotificationConfiguration/{id}", token, HttpMethod.Get);

            return results;
        }

        public static async Task<NotificationAPIResponse<bool>> DeleteNotificationConfigurationById(string token, string id)
        {
            DeleteNotificationConfigurationItemsCommand delNotification = new DeleteNotificationConfigurationItemsCommand();
            delNotification.NotificationTypeIds = new List<string>();
            delNotification.NotificationTypeIds.Add(id);
            
            var results = await InvokeService<bool>($"{NotificationAPI_URI}/NotificationConfiguration", token, HttpMethod.Delete, delNotification);

            return results;
        }

        public static async Task<NotificationAPIResponse<List<NotificationLookupAPIModel>>> GetAllNotificationLookup(string token)
        {
            var results = await InvokeService<List<NotificationLookupAPIModel>>($"{NotificationAPI_URI}/NotificationLookup/all", token, HttpMethod.Get);

            return results;
        }

        private static async Task<NotificationAPIResponse<T>> InvokeService<T>(string apiEndpoint, string token, HttpMethod method, dynamic payload = null, Func<string, bool> onError = null, Dictionary<string, string> headers = null, List<KeyValuePair<string, string>> queryStringParams = null)
        {
            var response = new NotificationAPIResponse<T> { StatusCode = StatusCode.Success };

            string baseUrl = Environment.GetEnvironmentVariable("connectionString_NotificationServiceBaseURL");

            using (var client = new HttpClient())
            {
                var url = baseUrl.EndsWith("/") ? $"{baseUrl}{apiEndpoint}" : $"{baseUrl}/{apiEndpoint}";

                if (queryStringParams.IsCollectionValid())
                    url = QueryHelpers.AddQueryString(url, queryStringParams);

                UriBuilder builder = new UriBuilder(url);

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                client.Timeout = TimeSpan.FromHours(24);

                var requestMessage = new HttpRequestMessage(method, builder.Uri);

                if ((payload != null) && (method == HttpMethod.Post || method == HttpMethod.Put || method == HttpMethod.Delete))
                {
                    var json = JsonConvert.SerializeObject(payload);
                    var stringContent = new StringContent(json, UnicodeEncoding.UTF8, "application/json");
                    requestMessage.Content = stringContent;

                    if (headers.IsCollectionValid())
                    {
                        headers.Keys.Each(k => requestMessage.Headers.Add(k, headers[k]));
                    }

                }

                var result = await client.SendAsync(requestMessage);

                using (StreamReader sr = new StreamReader(result.Content.ReadAsStreamAsync().Result))
                {
                    string content = sr.ReadToEnd();

                    if (!result.IsSuccessStatusCode)
                    {
                        response.StatusCode = StatusCode.Fail;

                        response.ErrorMessages = new List<string> { content };

                        onError?.Invoke(content);
                    }
                    else
                    {
                        var data = JsonConvert.DeserializeObject<T>(content);
                        response.Data = data;
                    }
                }

                return response;
            }
        }
    }
}
