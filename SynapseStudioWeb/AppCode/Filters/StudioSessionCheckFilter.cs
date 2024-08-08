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
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SynapseStudioWeb.Helpers;
using System;
using System.Linq;

namespace SynapseStudioWeb.AppCode.Filters
{
    public class StudioSessionCheckFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            HttpContext ctx = filterContext.HttpContext;
            var request = ctx.Request;
            if (request == null || string.Compare(Environment.GetEnvironmentVariable("settings_UseAsDMDBrowser") ?? "false", "true", true) == 0)
            {
                base.OnActionExecuting(filterContext);
                return;
            }

            string controllerName = request.RouteValues?["controller"].ToString();

            if (controllerName.IsEmpty() || (controllerName.IsNotEmpty() && string.Compare("account", controllerName, true) == 0))
            {
                base.OnActionExecuting(filterContext);
                return;
            }

            if (ctx.Session == null || ctx.Session.GetString(SynapseSession.UserID).IsEmpty())
            {
                if (IsAjaxCall(filterContext))
                {
                    var absUrl = $"{filterContext.HttpContext.Request.Scheme}://{filterContext.HttpContext.Request.Host}{filterContext.HttpContext.Request.PathBase}/account/logout";
                    //If to be set 307 - set location - logout not working - disable as it is handled by triggering href
                    //filterContext.HttpContext.Response.StatusCode = StatusCodes.Status307TemporaryRedirect;
                    //if (filterContext.HttpContext.Response.Headers != null)
                    //    filterContext.HttpContext.Response.Headers["Location"] = absUrl;
                    filterContext.Result = new JsonResult(new { result = "logout", url = absUrl });
                    return;
                }
                filterContext.Result = new RedirectResult("~/Account");
                //filterContext.Result = new RedirectResult("~/Account/Logout");
                return;
            }
            base.OnActionExecuting(filterContext);
        }
        private bool IsAjaxCall(ActionExecutingContext filterContext)
        {
            return string.Compare(filterContext.HttpContext.Request.Headers["x-requested-with"], "XMLHttpRequest", true) == 0;
        }
    }
}
