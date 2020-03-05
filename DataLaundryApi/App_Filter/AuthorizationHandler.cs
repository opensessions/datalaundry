using DataLaundryApi.Helpers;
using DataLaundryApi.Utilities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace DataLaundryApi.App_Filter
{
    public class AuthorizationHandlerAttribute : ActionFilterAttribute
    {
        string AccessTokenFromRequest = null;
        bool IsCheckedAction = true;
        public AuthorizationHandlerAttribute(bool IsAllowSkeepAction = false)
        {
            IsCheckedAction = IsAllowSkeepAction;
        }
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (IsCheckedAction)
            {
                if (context.HttpContext.Request.Headers.Count > 0)
                    AccessTokenFromRequest = context.HttpContext.Request.Headers["X-API-KEY"];
                else
                    context.Result = new UnauthorizedObjectResult(401);

                if (AccessTokenFromRequest == null)
                    context.Result = new UnauthorizedObjectResult(401);
                else if (!CheckAccessTokenValidity(AccessTokenFromRequest))
                    context.Result = new UnauthorizedObjectResult(401);
            }
        }

        private bool CheckAccessTokenValidity(string accessToken)
        {
            bool IsAuthorized = false;
            try
            {
                #region Allow same access token by pass added 01-04-2019
                var oCacheKey = string.Concat("AccessToken_", accessToken);
                var oResult = MemoryCacheHelper.GetValue(oCacheKey);
                if (oResult != null && (string)oResult == accessToken)
                    return true;
                #endregion

                var lstSqlParameter = new List<SqlParameter>();

                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@AccessToken", SqlDbType = SqlDbType.NVarChar, Value = (object)accessToken ?? DBNull.Value });

                var dt = DBProvider.GetDataTable("AccessToken_Authorize", CommandType.StoredProcedure, ref lstSqlParameter);
                if (dt != null && dt.Rows.Count > 0)
                {
                    if (dt.Rows[0]["IsAuthorized"] != DBNull.Value)
                    {
                        MemoryCacheHelper.Add(oCacheKey, accessToken, DateTimeOffset.UtcNow.AddDays(1));
                        IsAuthorized = Convert.ToBoolean(dt.Rows[0]["IsAuthorized"]);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertErrorLogs("[DataLaundryApi] AuthorizationHandler", "CheckAccessTokenValid", ex.Message, ex.InnerException?.Message, ex.StackTrace);
            }
            return IsAuthorized;
        }
    }
}