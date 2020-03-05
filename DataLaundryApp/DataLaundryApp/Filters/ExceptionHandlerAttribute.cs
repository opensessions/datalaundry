using DataLaundryDAL.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using DataLaundryApp.Common;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace DataLaundryApp.Filters
{
    public class ExceptionHandlerAttribute : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext filterContext)
        {
            string controllerName = (string)filterContext.RouteData.Values["controller"];
            string actionName = (string)filterContext.RouteData.Values["action"];

            if (filterContext.HttpContext.Request.IsAjaxRequest())
            {
                int httpStatusCode;
                string errorMessage = "";
                
                errorMessage = filterContext.Exception.Message;
                httpStatusCode = (int)HttpStatusCode.InternalServerError;

                var errorResult = new JsonResult(new{Data = new
                    {
                        status = false,
                        message = errorMessage
                    }});
                

                LogHelper.InsertErrorLogs(controllerName, actionName, errorMessage, filterContext.Exception.InnerException?.Message, filterContext.Exception.StackTrace);

                // return data
                filterContext.Result = errorResult;

                // don't redirect with custom errors since we are ajax
                //filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;

                filterContext.HttpContext.Response.StatusCode = httpStatusCode;
                filterContext.ExceptionHandled = true;
            }
            else // do normal, application error global will be called here
            {
                 var result = new ViewResult { ViewName = "Error" };
                var modelMetadata = new EmptyModelMetadataProvider();
                result.ViewData = new ViewDataDictionary(
                        modelMetadata, filterContext.ModelState);
                result.ViewData.Add("HandleException", 
                        filterContext.Exception);
                filterContext.Result = result;
                filterContext.ExceptionHandled = true;
                
                LogHelper.InsertErrorLogs(controllerName, actionName, filterContext.Exception.Message, filterContext.Exception.InnerException?.Message, filterContext.Exception.StackTrace);
            }
            base.OnException(filterContext);
        }
    }
}