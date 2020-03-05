using DataLaundryDAL;
using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using DataLaundryApp.Common;
using Microsoft.AspNetCore.Mvc.Routing;
using DataLaundryDAL.Constants;

namespace DataLaundryApp.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthoriseAttribute : ActionFilterAttribute
    {
        private bool _IsSessionOptional;
        public AuthoriseAttribute(bool isSessionOptional = false)
        {
            _IsSessionOptional = isSessionOptional;
        }
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var controller = filterContext.Controller as Controller;
            var ctx = filterContext.HttpContext;

            // //Get TimeZone Detail From Cookie And Set In Session
            // var cookie = ctx.Request.Cookies["timeZoneOffset"];
            // Int16 n = 0;
            // var tzo = (cookie != null && Int16.TryParse(cookie.Value, out n) ? n : 0);
            // ctx.Session.GetString("tzo") = tzo;

            if (_IsSessionOptional)
            {
                if (ctx.Session.GetString("UserID") != null)
                {
                    filterContext.Result = new RedirectResult("/FeedProvider/Index");
                    return;
                }
            }
            else
            {
                if (ctx.Session.GetString("UserID") == null)
                {
                    if (ctx.Request.IsAjaxRequest())
                    {
                        ctx.Response.StatusCode = (int)System.Net.HttpStatusCode.Unauthorized;
                        ctx.Response.StatusCode = 401;
                        return;
                    }
                    else
                    {
                        var controllerActionDescriptor = filterContext.ActionDescriptor as ControllerActionDescriptor;
                        if (controllerActionDescriptor != null)
                        {
                            var controllerName = controllerActionDescriptor.ControllerName;
                            var actionName = controllerActionDescriptor.ActionName;
                            var param1 = controllerActionDescriptor.Parameters;
                            string loadUrl = Settings.GetAppSetting("GetPath");
                            string returnURL = null;
                            if (string.IsNullOrEmpty(loadUrl))
                                returnURL = string.Concat(CommonFunctions.FullyQualifiedAppUrl(), controllerName, "/", actionName);
                            else
                                returnURL = string.Concat(CommonFunctions.FullyQualifiedAppUrl(),loadUrl,"/", controllerName, "/", actionName);
                            string otherURLParamValue = string.Empty;
                            if (param1.Count == 1)
                            {
                                otherURLParamValue = filterContext.RouteData.Values[param1[0].Name].ToString();
                                returnURL += "/" + otherURLParamValue;
                            }
                            filterContext.Result = new RedirectResult("~/Account/SessionTimeout?returnUrl=" + WebUtility.UrlEncode(returnURL));
                            //filterContext.Result = new RedirectResult(new UrlHelper(actionContext).Action("SessionTimeout", "Account", new { returnUrl = WebUtility.UrlDecode(returnURL) }));
                        }
                        return;
                    }
                }
            }
            base.OnActionExecuting(filterContext);
        }
    }
}