using DataLaundryScheduler.DTO;
using DataLaundryScheduler.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
namespace DataLaundryScheduler.Helpers
{
    class SchedulerHelper
    {
        public static string DataCurrentToken = null;
        private static List<SchedulerSettings> GetScheduledFeedProviders(int offset = 0)
        {
            //temp condition
            if (offset > 0)
                return null;

            var lstSchedulerSettings = new List<SchedulerSettings>();

            try
            {
                var lstSqlParameter = new List<SqlParameter>();
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Offset", SqlDbType = SqlDbType.Int, Value = offset });
                var dt = DBProvider.GetDataTable("GetScheduledFeedProviders", CommandType.StoredProcedure, ref lstSqlParameter);

                //temp sp call
                //int feedProviderId = 1;
                //lstSqlParameter.Add(new SqlParameter() { ParameterName = "@FeedProviderId", SqlDbType = SqlDbType.Int, Value = feedProviderId });
                //var dt = DBProvider.GetDataTable("GetSchedulerSettingsByFeedProvider", CommandType.StoredProcedure, ref lstSqlParameter);

                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        var row = dt.Rows[i];

                        var feedProvider = new FeedProvider()
                        {
                            Id = Convert.ToInt32(row["FeedProviderId"]),
                            Name = Convert.ToString(row["Name"]),
                            Source = Convert.ToString(row["Source"]),
                            IsIminConnector = Convert.ToBoolean(row["IsIminConnector"]),
                            DataType = new FeedDataType()
                            {
                                Id = Convert.ToInt32(row["FeedDataTypeId"]),
                                Name = Convert.ToString(row["FeedDataTypeName"])
                            },
                            IsUsesTimestamp = Convert.ToBoolean(row["IsUsesTimestamp"]),
                            IsUtcTimestamp = Convert.ToBoolean(row["IsUtcTimestamp"]),
                            IsUsesChangenumber = Convert.ToBoolean(row["IsUsesChangenumber"]),
                            IsUsesUrlSlug = Convert.ToBoolean(row["IsUsesUrlSlug"]),
                            EndpointUp = Convert.ToBoolean(row["EndpointUp"]),
                            UsesPagingSpec = Convert.ToBoolean(row["UsesPagingSpec"]),
                            IsOpenActiveCompatible = Convert.ToBoolean(row["IsOpenActiveCompatible"]),
                            IncludesCoordinates = Convert.ToBoolean(row["IncludesCoordinates"]),
                            IsFeedMappingConfirmed = Convert.ToBoolean(row["IsFeedMappingConfirmed"])
                        };

                        var schedulerSettings = new SchedulerSettings()
                        {
                            Id = Convert.ToInt32(row["Id"]),
                            FeedProvider = feedProvider,
                            StartDateTime = Convert.ToDateTime(row["StartDateTime"]),
                            liStartDateTime = Convert.ToInt64(row["StartDateTimeStamp"]),
                            IsEnabled = Convert.ToBoolean(row["IsEnabled"]),
                            SchedulerFrequencyId = Convert.ToInt32(row["SchedulerFrequencyId"]),
                            NextPageUrlAfterExecution = Convert.ToString(row["NextPageUrlAfterExecution"]),
                            CurrentUtcTimestamp = Convert.ToInt64(row["CurrentUtcTimestamp"])
                        };

                        if (row["NextPageNumberAfterExecution"] != DBNull.Value)
                            schedulerSettings.NextPageNumberAfterExecution = Convert.ToInt32(row["NextPageNumberAfterExecution"]);

                        if (row["ExpiryDateTime"] != DBNull.Value)
                        {
                            schedulerSettings.ExpiryDateTime = Convert.ToDateTime(row["ExpiryDateTime"]);
                            schedulerSettings.liExpiryDateTime = Convert.ToInt64(row["ExpiryDateTimeStamp"]);
                        }

                        if (row["RecurranceInterval"] != DBNull.Value)
                            schedulerSettings.RecurranceInterval = Convert.ToInt32(row["RecurranceInterval"]);

                        if (row["RecurranceDaysInWeek"] != DBNull.Value)
                            schedulerSettings.RecurranceDaysInWeek = Convert.ToString(row["RecurranceDaysInWeek"]);

                        if (row["RecurranceMonths"] != DBNull.Value)
                            schedulerSettings.RecurranceMonths = Convert.ToString(row["RecurranceMonths"]);

                        if (row["RecurranceDatesInMonth"] != DBNull.Value)
                            schedulerSettings.RecurranceDatesInMonth = Convert.ToString(row["RecurranceDatesInMonth"]);

                        if (row["RecurranceWeekNos"] != DBNull.Value)
                            schedulerSettings.RecurranceWeekNos = Convert.ToString(row["RecurranceWeekNos"]);

                        if (row["RecurranceDaysInWeekForMonth"] != DBNull.Value)
                            schedulerSettings.RecurranceDaysInWeekForMonth = Convert.ToString(row["RecurranceDaysInWeekForMonth"]);

                        if (row["SchedulerLastStartTime"] != DBNull.Value)
                            schedulerSettings.SchedulerLastStartTime = Convert.ToDateTime(row["SchedulerLastStartTime"]);

                        if (row["SchedulerLastStartTimeStamp"] != DBNull.Value)
                            schedulerSettings.SchedulerLastStartTimeStamp = Convert.ToInt64(row["SchedulerLastStartTimeStamp"]);

                        if (row["IsStarted"] != DBNull.Value)
                            schedulerSettings.IsStarted = Convert.ToBoolean(row["IsStarted"]);

                        if (row["IsCompleted"] != DBNull.Value)
                            schedulerSettings.IsCompleted = Convert.ToBoolean(row["IsCompleted"]);

                        if (row["IsTerminated"] != DBNull.Value)
                            schedulerSettings.IsTerminated = Convert.ToBoolean(row["IsTerminated"]);

                        lstSchedulerSettings.Add(schedulerSettings);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertErrorLogs("[DataLaundryScheduler] SchedulerHelper", "GetScheduledFeedProviders", ex.Message, ex.InnerException?.Message, ex.StackTrace);
            }
            return lstSchedulerSettings;
        }

        private static void FetchFeedFromFeedProvider(SchedulerSettings schedulerSettings, string afterTimestamp, string afterChangenumber)
        {
            try
            {
                bool usesNextPageUrl = false;
                string newNextPageUrl = "";
                string finalUrl = "";
                string url = schedulerSettings.FeedProvider.Source;

                finalUrl = url;

                if (string.IsNullOrEmpty(afterTimestamp) && string.IsNullOrEmpty(afterChangenumber))
                {
                    if (!string.IsNullOrEmpty(schedulerSettings.NextPageUrlAfterExecution))
                    {
                        // for feeds that needs next page url params
                        usesNextPageUrl = true;
                        string nextPageUrl = schedulerSettings.NextPageUrlAfterExecution;

                        if (nextPageUrl.IndexOf("http") > -1)
                        {
                            finalUrl = nextPageUrl;
                        }
                        else
                        {
                            if (nextPageUrl.IndexOf('?') > -1)
                            {
                                string urlWithQuery = nextPageUrl.Substring(nextPageUrl.IndexOf('?'));
                                finalUrl = url + urlWithQuery;
                            }
                            else
                            {
                                if (schedulerSettings.FeedProvider.IsUsesUrlSlug && nextPageUrl.IndexOf('/') > -1)
                                {
                                    string urlWithQuery = nextPageUrl.Substring(nextPageUrl.LastIndexOf('/'));
                                    finalUrl = url + urlWithQuery;
                                }
                                else
                                {
                                    finalUrl = url;
                                }
                            }
                        }
                    }
                }
                else
                {
                    //for the first time when this function called
                    usesNextPageUrl = true;

                    if (!schedulerSettings.FeedProvider.IsUsesUrlSlug)
                    {
                        if (!string.IsNullOrEmpty(afterTimestamp))
                            finalUrl = url + "?afterTimestamp=" + Uri.EscapeUriString(afterTimestamp);
                        else if (!string.IsNullOrEmpty(afterTimestamp))
                            finalUrl = url + "?afterChangenumber=" + Uri.EscapeUriString(afterChangenumber);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(afterTimestamp))
                            finalUrl = url + "/" + Uri.EscapeUriString(afterTimestamp);
                        else if (!string.IsNullOrEmpty(afterTimestamp))
                            finalUrl = url + "/" + Uri.EscapeUriString(afterChangenumber);
                    }
                }

                if (!string.IsNullOrEmpty(finalUrl))
                {
                    string json = GetWebContent(finalUrl);

                    var token = JToken.Parse(json);

                    if (token != null)
                    {
                        // this is used for open active apis as next page url is mandatory based on predefined key name
                        var nextPageUrlToken = token.SelectToken("$.next");

                        if (usesNextPageUrl || nextPageUrlToken != null)
                        {
                            usesNextPageUrl = true;
                            newNextPageUrl = nextPageUrlToken.Value<string>();
                        }

                        var lstFeedMapping = FeedConfigHelper.GetFeedMappingByTableName(schedulerSettings.FeedProvider.Id);

                        if (lstFeedMapping != null && lstFeedMapping.Count > 0)
                        {
                            var lstFeedMappingMapped = lstFeedMapping.Where(x => !string.IsNullOrEmpty(x.ActualFeedKeyPath)).ToList();

                            if (lstFeedMappingMapped != null && lstFeedMappingMapped.Count > 0)
                            {
                                string actualFeedKeyPath = lstFeedMappingMapped.FirstOrDefault().ActualFeedKeyPath;
                                string currentKeyName = actualFeedKeyPath.Substring(0, actualFeedKeyPath.IndexOf("["));

                                List<FeedMapping> lstFeedMappingCopy = null;

                                ProcessFeedKeyValues(token, lstFeedMapping, out lstFeedMappingCopy, currentKeyName: currentKeyName);

                                //update nextpageurl or nextpage number in db
                                var nextPageURL = schedulerSettings.NextPageUrlAfterExecution;
                                schedulerSettings.NextPageUrlAfterExecution = newNextPageUrl;
                                schedulerSettings.NextPageNumberAfterExecution = null;

                                UpdateLastExecutionDetails(schedulerSettings);

                                //if next page url is different from current one then only fetch more feeds from the same feed provider
                                if (finalUrl.ToLower() != newNextPageUrl.ToLower() && nextPageURL.ToLower() != newNextPageUrl.ToLower())
                                    FetchFeedFromFeedProvider(schedulerSettings, null, null);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertErrorLogs("[DataLaundryScheduler] SchedulerHelper", "FetchFeedFromFeedProvider", ex.Message, ex.InnerException?.Message, ex.StackTrace);
            }
        }

        private static void FetchFeedFromFeedProvider_v1(SchedulerSettings schedulerSettings, string afterTimestamp, string afterChangenumber, long schedulerLogId = 0, List<string> Urls = null, bool isRecursive = false)
        {
            try
            {
                bool usesNextPageUrl = false;
                string newNextPageUrl = "";
                string finalUrl = "";
                string url = schedulerSettings.FeedProvider.Source;

                finalUrl = url;

                if (!isRecursive)
                {
                    #region Save into Eventlog
                    LogHelper.InsertEventLog(schedulerSettings.FeedProvider.Id, finalUrl, newNextPageUrl, "[DataLaundryScheduler] SchedulerHelper", "FetchFeedFromFeedProvider_v1", "started periodically");
                    #endregion
                }

                if (string.IsNullOrEmpty(afterTimestamp) && string.IsNullOrEmpty(afterChangenumber))
                {
                    if (!string.IsNullOrEmpty(schedulerSettings.NextPageUrlAfterExecution))
                    {
                        // for feeds that needs next page url params
                        usesNextPageUrl = true;
                        string nextPageUrl = schedulerSettings.NextPageUrlAfterExecution;

                        if (nextPageUrl.IndexOf("http") > -1)
                        {
                            finalUrl = nextPageUrl;
                        }
                        else
                        {
                            if (nextPageUrl.IndexOf('?') > -1)
                            {
                                string urlWithQuery = nextPageUrl.Substring(nextPageUrl.IndexOf('?'));
                                finalUrl = url + urlWithQuery;
                            }
                            else
                            {
                                if (schedulerSettings.FeedProvider.IsUsesUrlSlug && nextPageUrl.IndexOf('/') > -1)
                                {
                                    string urlWithQuery = nextPageUrl.Substring(nextPageUrl.LastIndexOf('/'));
                                    finalUrl = url + urlWithQuery;
                                }
                                else
                                {
                                    finalUrl = url;
                                }
                            }
                        }
                    }
                }
                else
                {
                    //for the first time when this function called
                    usesNextPageUrl = true;

                    if (!schedulerSettings.FeedProvider.IsUsesUrlSlug)
                    {
                        if (!string.IsNullOrEmpty(afterTimestamp))
                            finalUrl = url + "?afterTimestamp=" + Uri.EscapeUriString(afterTimestamp);
                        else if (!string.IsNullOrEmpty(afterTimestamp))
                            finalUrl = url + "?afterChangenumber=" + Uri.EscapeUriString(afterChangenumber);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(afterTimestamp))
                            finalUrl = url?.TrimEnd('/') + "/" + Uri.EscapeUriString(afterTimestamp);
                        else if (!string.IsNullOrEmpty(afterTimestamp))
                            finalUrl = url?.TrimEnd('/') + "/" + Uri.EscapeUriString(afterChangenumber);
                    }
                }

                if (!string.IsNullOrEmpty(finalUrl))
                {
                    #region Save into Eventlog
                    LogHelper.InsertEventLog(schedulerSettings.FeedProvider.Id, finalUrl, newNextPageUrl, "[DataLaundryScheduler] SchedulerHelper", "FetchFeedFromFeedProvider_v1", "before response");
                    #endregion

                    if (Urls == null)
                        Urls = new List<string>();

                    if (!Urls.Any(o => string.Equals(finalUrl, o, StringComparison.OrdinalIgnoreCase)))
                    {
                        Urls.Add(finalUrl);

                        string json = GetWebContent(finalUrl);

                        var token = JToken.Parse(json);

                        if (token != null)
                        {
                            // this is used for open active apis as next page url is mandatory based on predefined key name
                            var nextPageUrlToken = token.SelectToken("$.next");

                            if (usesNextPageUrl || nextPageUrlToken != null)
                            {
                                usesNextPageUrl = true;
                                newNextPageUrl = nextPageUrlToken.Value<string>();

                                #region Save into Eventlog
                                LogHelper.InsertEventLog(schedulerSettings.FeedProvider.Id, finalUrl, newNextPageUrl, "[DataLaundryScheduler] SchedulerHelper", "FetchFeedFromFeedProvider_v1", "After response");
                                #endregion
                            }

                            var lstFeedMapping = FeedConfigHelper.GetFeedMappingByTableName(schedulerSettings.FeedProvider.Id);

                            if (lstFeedMapping != null && lstFeedMapping.Count > 0)
                            {
                                var lstFeedMappingMapped = lstFeedMapping.Where(x => !string.IsNullOrEmpty(x.ActualFeedKeyPath)).ToList();

                                if (lstFeedMappingMapped != null && lstFeedMappingMapped.Count > 0)
                                {
                                    string actualFeedKeyPath = lstFeedMappingMapped.FirstOrDefault().ActualFeedKeyPath;
                                    string currentKeyName = actualFeedKeyPath.Substring(0, actualFeedKeyPath.IndexOf("["));

                                    List<FeedMapping> lstFeedMappingCopy = null;

                                    //ProcessFeedKeyValues(token, lstFeedMapping, out lstFeedMappingCopy, currentKeyName: currentKeyName);
                                    ProcessFeedKeyValues_v1(token, lstFeedMapping, out lstFeedMappingCopy, currentKeyName: currentKeyName, schedulerLogId: schedulerLogId);

                                    //update nextpageurl or nextpage number in db
                                    var nextPageURL = schedulerSettings.NextPageUrlAfterExecution;
                                    schedulerSettings.NextPageUrlAfterExecution = newNextPageUrl;
                                    schedulerSettings.NextPageNumberAfterExecution = null;

                                    UpdateLastExecutionDetails(schedulerSettings);

                                    //if (finalUrl.ToLower() != newNextPageUrl.ToLower() && nextPageURL.ToLower() != newNextPageUrl.ToLower() 
                                    //        && !Urls.Any(o => string.Equals(newNextPageUrl, o, StringComparison.OrdinalIgnoreCase)))
                                    //    FetchFeedFromFeedProvider_v1(schedulerSettings, null, null, schedulerLogId, Urls);

                                    //if next page url is different from current one then only fetch more feeds from the same feed provider
                                    if (finalUrl.ToLower() != newNextPageUrl.ToLower() && nextPageURL.ToLower() != newNextPageUrl.ToLower()
                                            && !Urls.Any(o => string.Equals(newNextPageUrl, o, StringComparison.OrdinalIgnoreCase))
                                            && finalUrl.Substring(finalUrl.IndexOf('?') > 0 ? finalUrl.IndexOf('?') : 0) != newNextPageUrl.Substring(newNextPageUrl.IndexOf('?') > 0 ? newNextPageUrl.IndexOf('?') : 0))
                                    {
                                        //#region Save into Eventlog
                                        //LogHelper.InsertEventLog(schedulerSettings.FeedProvider.Id, finalUrl, newNextPageUrl, "[DataLaundryScheduler] SchedulerHelper", "FetchFeedFromFeedProvider_v1", "After response");
                                        //#endregion

                                        FetchFeedFromFeedProvider_v1(schedulerSettings, null, null, schedulerLogId, Urls, isRecursive: true);
                                    }
                                    else
                                    {
                                        #region Update to scheduler Log
                                        long logId;
                                        LogHelper.InsertUpdateSchedulerLog(schedulerLogId, schedulerSettings.FeedProvider.Id, null, DateTime.Now, LogHelper.stringValueOf(LogStatus.JobCompleted), out logId);
                                        #endregion

                                        #region Save into Eventlog
                                        LogHelper.InsertEventLog(schedulerSettings.FeedProvider.Id, finalUrl, newNextPageUrl, "[DataLaundryScheduler] SchedulerHelper", "FetchFeedFromFeedProvider_v1", "ended");
                                        #endregion
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertErrorLogs("[DataLaundryScheduler] SchedulerHelper", "FetchFeedFromFeedProvider_v1", ex.Message, ex.InnerException?.Message, ex.StackTrace, schedulerSettings.FeedProvider.Id);
                #region Update to scheduler Log   
                long logId;
                LogHelper.InsertUpdateSchedulerLog(schedulerLogId, schedulerSettings.FeedProvider.Id, null, DateTime.Now, LogHelper.stringValueOf(LogStatus.ErrorOccurred), out logId, "[DataLaundryScheduler] SchedulerHelper - FetchFeedFromFeedProvider_v1");
                #endregion
            }
        }

        /// <summary>
        /// Check feed provider's stratergy for data fetching from next or base url
        /// </summary>
        /// <param name="schedulerSettings"></param>
        /// <param name="afterTimestamp"></param>
        /// <param name="afterChangenumber"></param>
        /// <param name="schedulerLogId"></param>
        /// <param name="Urls"></param>
        /// <param name="isRecursive"></param>
        private static void FetchFeedFromFeedProvider_v2(SchedulerSettings schedulerSettings, string afterTimestamp, string afterChangenumber, long schedulerLogId = 0, List<string> Urls = null, bool isRecursive = false)
        {
            string json = "";
            try
            {
                bool usesNextPageUrl = false;
                string newNextPageUrl = "";
                string finalUrl = "";
                string url = schedulerSettings.FeedProvider.Source;
                var IsSchedulerEnabled = schedulerSettings.IsEnabled;
                var request = (HttpWebRequest)WebRequest.Create(new Uri(url));
                if(IsSchedulerEnabled !=  false)
                {

                request.Method = WebRequestMethods.Http.Get;
var response = (HttpWebResponse)request.GetResponse();

                    if (response.StatusCode == HttpStatusCode.OK)
                    {

                if (string.IsNullOrEmpty(afterTimestamp) && string.IsNullOrEmpty(afterChangenumber)
                    && !string.IsNullOrEmpty(schedulerSettings.NextPageUrlAfterExecution))
                {
                    // for feeds that needs next page url params
                    usesNextPageUrl = true;
                    string nextPageUrl = schedulerSettings.NextPageUrlAfterExecution;

                    if (nextPageUrl.IndexOf("http") > -1)
                    {
                        finalUrl = nextPageUrl;
                    }
                    else
                    {
                        if (nextPageUrl.IndexOf('?') > -1)
                        {
                            string urlWithQuery = nextPageUrl.Substring(nextPageUrl.IndexOf('?'));
                            finalUrl = url + urlWithQuery;
                        }
                        else
                        {
                            if (schedulerSettings.FeedProvider.IsUsesUrlSlug && nextPageUrl.IndexOf('/') > -1)
                            {
                                string urlWithQuery = nextPageUrl.Substring(nextPageUrl.LastIndexOf('/'));
                                finalUrl = url + urlWithQuery;
                            }
                            else
                            {
                                finalUrl = url;
                            }
                        }
                    }
                }
                else
                {
                    //for the first time when this function called
                    usesNextPageUrl = true;

                    if (!schedulerSettings.FeedProvider.IsUsesUrlSlug)
                    {
                        if (!string.IsNullOrEmpty(afterTimestamp))
                            finalUrl = url + "?afterTimestamp=" + Uri.EscapeUriString(afterTimestamp);
                        else if (!string.IsNullOrEmpty(afterTimestamp))
                            finalUrl = url + "?afterChangenumber=" + Uri.EscapeUriString(afterChangenumber);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(afterTimestamp))
                            finalUrl = url?.TrimEnd('/') + "/" + Uri.EscapeUriString(afterTimestamp);
                        else if (!string.IsNullOrEmpty(afterTimestamp))
                            finalUrl = url?.TrimEnd('/') + "/" + Uri.EscapeUriString(afterChangenumber);
                    }
                }

                if (!string.IsNullOrEmpty(finalUrl))
                {
                    #region Save into Eventlog
                    LogHelper.InsertEventLog(schedulerSettings.FeedProvider.Id, finalUrl, newNextPageUrl, "[DataLaundryScheduler] SchedulerHelper", "FetchFeedFromFeedProvider_v2", "before response");
                    #endregion

                    if (Urls == null)
                        Urls = new List<string>();

                    if (!Urls.Any(o => string.Equals(finalUrl, o, StringComparison.OrdinalIgnoreCase)))
                    {
                        Urls.Add(finalUrl);

                        //string json = GetWebContent(finalUrl);
                        //json = GetWebContent(finalUrl, schedulerSettings.FeedProvider.Id);
                        json = GetWebContent_v1(finalUrl, schedulerSettings, schedulerLogId);

                        //var token = JToken.Parse(json);
                        if (!string.IsNullOrEmpty(json))
                        {
                            if (Settings.IsValidJson(json))
                            {
                                var token = JToken.Parse(json);

                                if (token != null)
                                {
                                    // this is used for open active apis as next page url is mandatory based on predefined key name
                                    var nextPageUrlToken = token.SelectToken("$.next");

                                    if (usesNextPageUrl || nextPageUrlToken != null)
                                    {
                                        usesNextPageUrl = true;
                                        newNextPageUrl = nextPageUrlToken?.Value<string>();

                                        #region Save into Eventlog
                                        LogHelper.InsertEventLog(schedulerSettings.FeedProvider.Id, finalUrl, newNextPageUrl, "[DataLaundryScheduler] SchedulerHelper", "FetchFeedFromFeedProvider_v2", "After response");
                                        #endregion
                                    }

                                    var lstFeedMapping = FeedConfigHelper.GetFeedMappingByTableName(schedulerSettings.FeedProvider.Id);

                                    if (lstFeedMapping != null && lstFeedMapping.Count > 0)
                                    {
                                        var lstFeedMappingMapped = lstFeedMapping.Where(x => !string.IsNullOrEmpty(x.ActualFeedKeyPath)).ToList();

                                        if (lstFeedMappingMapped != null && lstFeedMappingMapped.Count > 0)
                                        {
                                            string actualFeedKeyPath = lstFeedMappingMapped.FirstOrDefault().ActualFeedKeyPath;
                                            string currentKeyName = actualFeedKeyPath.Substring(0, actualFeedKeyPath.IndexOf("["));

                                            List<FeedMapping> lstFeedMappingCopy = null;

                                            //ProcessFeedKeyValues(token, lstFeedMapping, out lstFeedMappingCopy, currentKeyName: currentKeyName);
                                            ProcessFeedKeyValues_v1(token, lstFeedMapping, out lstFeedMappingCopy, currentKeyName: currentKeyName, schedulerLogId: schedulerLogId);

                                            //update nextpageurl or nextpage number in db
                                            var nextPageURL = schedulerSettings.NextPageUrlAfterExecution;
                                            schedulerSettings.NextPageUrlAfterExecution = newNextPageUrl;
                                            schedulerSettings.NextPageNumberAfterExecution = null;

                                            UpdateLastExecutionDetails(schedulerSettings);

                                            //if (finalUrl.ToLower() != newNextPageUrl.ToLower() && nextPageURL.ToLower() != newNextPageUrl.ToLower() 
                                            //        && !Urls.Any(o => string.Equals(newNextPageUrl, o, StringComparison.OrdinalIgnoreCase)))
                                            //    FetchFeedFromFeedProvider_v2(schedulerSettings, null, null, schedulerLogId, Urls);

                                            //if next page url is different from current one then only fetch more feeds from the same feed provider
                                            if (finalUrl.ToLower() != newNextPageUrl.ToLower() && nextPageURL.ToLower() != newNextPageUrl.ToLower()
                                                    && !Urls.Any(o => string.Equals(newNextPageUrl, o, StringComparison.OrdinalIgnoreCase))
                                                    && finalUrl.Substring(finalUrl.IndexOf('?') > 0 ? finalUrl.IndexOf('?') : 0) != newNextPageUrl.Substring(newNextPageUrl.IndexOf('?') > 0 ? newNextPageUrl.IndexOf('?') : 0))
                                            {
                                                FetchFeedFromFeedProvider_v2(schedulerSettings, null, null, schedulerLogId, Urls, isRecursive: true);
                                            }
                                            else
                                            {
                                                #region Update to scheduler Log
                                                long logId;
                                                LogHelper.InsertUpdateSchedulerLog(schedulerLogId, schedulerSettings.FeedProvider.Id, null, DateTime.Now, LogHelper.stringValueOf(LogStatus.JobCompleted), out logId);
                                                #endregion

                                                #region Save into Eventlog
                                                LogHelper.InsertEventLog(schedulerSettings.FeedProvider.Id, finalUrl, newNextPageUrl, "[DataLaundryScheduler] SchedulerHelper", "FetchFeedFromFeedProvider_v2", "ended");
                                                #endregion

                                                #region Update Status to Scheduler Settings
                                                schedulerSettings.IsStarted = false;
                                                schedulerSettings.IsCompleted = true;
                                                schedulerSettings.IsTerminated = false;
                                                UpdateSchedulerStatus(schedulerSettings);
                                                #endregion

                                                #region RuleFilter
                                                AutoRuleFilterData(schedulerSettings.FeedProvider.Id);
                                                #endregion

                                                #region Check longitude and latitude exists and not
                                                CheckAndPlaceUpdate(schedulerSettings.FeedProvider.Id);
                                                #endregion
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                    }

                //if (!isRecursive)
                //{
                //    #region Save into Eventlog
                //    LogHelper.InsertEventLog(schedulerSettings.FeedProvider.Id, finalUrl, newNextPageUrl, "[DataLaundryScheduler] SchedulerHelper", "FetchFeedFromFeedProvider_v2", "started periodically");
                //    #endregion
                //}

            }
            }
            catch (Exception ex)
            {
  
   // string Recipient1 =Settings.SMTP_REC1();
    //string Recipient2 =Settings.SMTP_REC2();
    //string Email =Settings.SMTP_Email();
   // string Credential =Settings.SMTP_Cred();

                if(schedulerSettings.IsEnabled !=  false)
                {

                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
 
                mail.From = new MailAddress("datalaundryalerts@gmail.com");
                mail.To.Add("olson.fernandes@differenzsystem.com");
                mail.To.Add("info@londonsport.org");
                
                mail.Subject ="Data Laundry Feed Alert";
                StringBuilder mailBody = new StringBuilder();
                mailBody.AppendFormat("<h2>Data Laundry Feed Alert</h2>");
                mailBody.AppendFormat("<td><b><span style = font-family:Arial;font-size:13pt>Dear Data Laundry Admin,</span></b></td>");
                mailBody.AppendFormat("<br/>");
                mailBody.AppendFormat("<p>The following feed is currently being disabled as it is not providing appropriate data anymore.</p>");
                mailBody.AppendFormat("<p><b>Feed Name:</b> "+schedulerSettings.FeedProvider.Name );
                mailBody.AppendFormat("<br/>");
                mailBody.AppendFormat("<p><b>Feed URL:</b>"+" " + schedulerSettings.FeedProvider.Source );
                mailBody.AppendFormat("<br/>");
                mailBody.AppendFormat("<br/>");
                mailBody.AppendFormat("Thanks & Regards");
                mailBody.AppendFormat("<br/>");
                mailBody.AppendFormat("Data Laundry");
                mailBody.AppendFormat("<br/>");
                mailBody.AppendFormat("<img width=70 width=70 src=https://i.ibb.co/K5xjY1n/data-laundry.png >");
                mail.Body = mailBody.ToString();
                mail.IsBodyHtml =  true;
                SmtpServer.Port = 25;
                SmtpServer.Credentials = new System.Net.NetworkCredential("datalaundryalerts", "!welcome007");
                SmtpServer.EnableSsl = true;
                SmtpServer.Send(mail);
                 var schedulerSettings1 =FeedHelper.SchedulerSettingsDisable(schedulerSettings.FeedProvider.Id);
               }
                if (!string.IsNullOrEmpty(ex.Message) && (ex.Message.StartsWith("Unterminated string. Expected delimiter")
                                                            || ex.Message.Contains("items[")))
                {
                    string errorlogfile = "Feed_" + schedulerSettings.FeedProvider.Id + "_" + DateTime.Now.Date.ToString("dd-MMM-yyyy hhmmss") + ".txt";
                    string filePath = Path.Combine(Path.GetDirectoryName(Directory.GetCurrentDirectory()), errorlogfile);
                    //System.IO.File.WriteAllLines(filePath, new string[] { json });
                    System.IO.File.WriteAllText(filePath, json, System.Text.Encoding.UTF8);
                }

                LogHelper.InsertErrorLogs("[DataLaundryScheduler] SchedulerHelper", "FetchFeedFromFeedProvider_v2", ex.Message, ex.InnerException?.Message, ex.StackTrace, schedulerSettings.FeedProvider.Id);

                #region Update to scheduler Log   
                long logId;
                LogHelper.InsertUpdateSchedulerLog(schedulerLogId, schedulerSettings.FeedProvider.Id, null, DateTime.Now, LogHelper.stringValueOf(LogStatus.ErrorOccurred), out logId, "[DataLaundryScheduler] SchedulerHelper - FetchFeedFromFeedProvider_v2");
                #endregion

                #region Update Status to Scheduler Settings
     

                schedulerSettings.IsStarted = false;
                schedulerSettings.IsCompleted = false;
                schedulerSettings.IsTerminated = true;
                UpdateSchedulerStatus(schedulerSettings);
                #endregion
            }
        }

        private static string GetWebContent(string url, long? feedProviderId = null)
        {
            string output = "";
            try
            {
                var uri = new Uri(url);
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                ServicePointManager.SecurityProtocol = SecurityProtocolType.SystemDefault | SecurityProtocolType.Tls | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11;
                //ServicePointManager.ServerCertificateValidationCallback += ValidateServerCertificate;
                var request = (HttpWebRequest)WebRequest.Create(uri);

                #region Timeout
                request.Timeout = 600000; // set default time 10 minutes
                request.ReadWriteTimeout = 600000; // set default time 10 minutes
                request.KeepAlive = false;
                request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:6.0.2) Gecko/20100101 Firefox/6.0.2";
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
                request.Headers.Add("Accept-Language", "en-gb,en;q=0.5");
                request.Headers.Add("Accept-Charset", "ISO-8859-1,utf-8;q=0.7,*;q=0.7");
                #endregion

                request.Proxy = WebRequest.GetSystemWebProxy();
                request.Method = WebRequestMethods.Http.Get;
                request.ContentType = "application/x-www-form-urlencoded";
                var response = (HttpWebResponse)request.GetResponse();
                //var reader = new StreamReader(response.GetResponseStream());
                //string output = reader.ReadToEnd();

                StringBuilder sb = new StringBuilder();
                Byte[] buf = new byte[8192]; int count;
                Stream resStream = response.GetResponseStream();
                do
                {
                    count = resStream.Read(buf, 0, buf.Length);
                    if (count != 0)
                    {
                        sb.Append(Encoding.UTF8.GetString(buf, 0, count)); // just hardcoding UTF8 here
                    }
                } while (count > 0);
                output = sb.ToString();

                //string output = Encoding.UTF8.GetString(ReadFully(resStream, 0));

                response.Dispose();
                response.Close();
            }
            catch (Exception ex)
            {
                LogHelper.InsertErrorLogs("[DataLaundryScheduler] SchedulerHelper", "GetWebContent", ex.Message, ex.InnerException?.Message, ex.StackTrace, feedProviderId);
            }
            return output;
        }

        private static string GetWebContent_v1(string url, SchedulerSettings schedulerSettings, long schedulerLogId = 0)
        {
            string response = null;
            try
            {
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls | SecurityProtocolType.Ssl3;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.SystemDefault | SecurityProtocolType.Tls | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11;
                //ServicePointManager.ServerCertificateValidationCallback += ValidateServerCertificate;

                //using (var request = new WebClient())
                //{
                //    #region Timeout
                //    request.Timeout = 600000; // set default time 10 minutes                    
                //    #endregion                   

                //    response = request.DownloadString(url);
                //    request.Dispose();
                //}

                using (var request = new GZipWebClient())
                {
                    #region Timeout
                    request.Timeout = 600000; // set default time 10 minutes      
                    request.KeepAlive = false;
                    request.ConnectionLeaseTimeout = 300000;
                    request.MaxIdleTime = 300000;
                    #endregion                   

                    var a = response = request.DownloadString(url);

                    request.Dispose();
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertErrorLogs("[DataLaundryScheduler] SchedulerHelper", "GetWebContent_v1", ex.Message, ex.InnerException?.Message, ex.StackTrace, schedulerSettings.FeedProvider.Id);

                #region Update to scheduler Log   
                long logId;
                LogHelper.InsertUpdateSchedulerLog(schedulerLogId, schedulerSettings.FeedProvider.Id, null, DateTime.Now, LogHelper.stringValueOf(LogStatus.ErrorOccurred), out logId, "[DataLaundryScheduler] SchedulerHelper - GetWebContent_v1");
                #endregion

                #region Update Status to Scheduler Settings
                schedulerSettings.IsStarted = false;
                schedulerSettings.IsCompleted = false;
                schedulerSettings.IsTerminated = true;
                UpdateSchedulerStatus(schedulerSettings);
                #endregion                
            }
            return response;
        }

        private static bool UpdateLastExecutionDetails(SchedulerSettings schedulerSetting)
        {
            var lstSqlParameter = new List<SqlParameter>();

            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Id", SqlDbType = SqlDbType.BigInt, Value = schedulerSetting.Id });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@NextPageUrlAfterExecution", SqlDbType = SqlDbType.NVarChar, Value = (object)schedulerSetting.NextPageUrlAfterExecution ?? DBNull.Value });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@NextPageNumberAfterExecution", SqlDbType = SqlDbType.NVarChar, Value = (object)schedulerSetting.NextPageNumberAfterExecution ?? DBNull.Value });

            int rowsAffected = DBProvider.ExecuteNonQuery("SchedulerSettings_UpdateLastExecutionDetails", CommandType.StoredProcedure, ref lstSqlParameter);
            return rowsAffected > 0;
        }

        private static void ProcessFeedKeyValues(JToken token, List<FeedMapping> lstFeedMapping, out List<FeedMapping> lstFeedMappingCopy, string currentKeyName = "items", int level = 0, int index = 0, bool isArray = true)
        {
            //make new copy of entire list
            lstFeedMappingCopy = GenericCopier<List<FeedMapping>>.DeepCopy(lstFeedMapping);
            try
            {
                if (lstFeedMapping != null && lstFeedMapping.Count > 0)
                {
                    string firstActualFeedKeyPath = lstFeedMapping.Where(x => !string.IsNullOrEmpty(x.ActualFeedKeyPath)).FirstOrDefault().ActualFeedKeyPath;

                    bool isFromArray = isArray;

                    if (isArray)
                    {
                        if (!string.IsNullOrEmpty(currentKeyName))
                        {
                            //get all elements of specified array
                            List<JToken> lstJToken = null;

                            if (level == 0)
                                lstJToken = token.SelectTokens("$." + currentKeyName + "[*]").ToList();
                            else
                                lstJToken = token.Parent.Parent.SelectTokens("$." + currentKeyName + "[*]").ToList();

                            if (lstJToken != null && lstJToken.Count > 0)
                            {
                                //loop through each records from array
                                for (int i = 0; i < lstJToken.Count; i++)
                                {
                                    string feedId = "";
                                    string feedStatus = "";
                                    object modifiedDate = null;

                                    if (level == 0)
                                    {
                                        feedId = lstJToken[i].SelectToken("$.id")?.Value<string>();
                                        feedStatus = lstJToken[i].SelectToken("$.state")?.Value<string>();

                                        var modifiedJValue = lstJToken[i].SelectToken("$.modified");

                                        if (modifiedJValue != null)
                                        {
                                            switch ((int)modifiedJValue.Type)
                                            {
                                                case 6:
                                                    //int or long
                                                    long longVal = modifiedJValue.Value<long>();
                                                    modifiedDate = CommonFunctions.UnixTimeStampToDateTime(longVal);
                                                    break;
                                                case 7:
                                                    //float
                                                    float floatVal = modifiedJValue.Value<float>();
                                                    modifiedDate = CommonFunctions.UnixTimeStampToDateTime((long)floatVal);
                                                    break;
                                                case 8:
                                                    //string
                                                    string strVal = modifiedJValue.Value<string>();

                                                    if (long.TryParse(strVal, out longVal))
                                                        modifiedDate = CommonFunctions.UnixTimeStampToDateTime(longVal);
                                                    else
                                                        modifiedDate = 0; //intial value of utc timestamp
                                                    break;
                                                case 12:
                                                    //date
                                                    modifiedDate = modifiedJValue.Value<DateTime>();
                                                    break;
                                                default:
                                                    break;
                                            }
                                        }
                                    }

                                    //make new copy of entire list after each record processing
                                    lstFeedMappingCopy = GenericCopier<List<FeedMapping>>.DeepCopy(lstFeedMapping);

                                    //map feed key values only if feed is not deleted
                                    if (feedStatus != "deleted")
                                    {
                                        //loop through all configured feed mappings
                                        for (int j = 0; j < lstFeedMappingCopy.Count; j++)
                                        {
                                            lstFeedMappingCopy[j] = UpdateFeedKeyValue(lstJToken[i], lstFeedMappingCopy[j], isFromArray, ref level);
                                        }
                                    }

                                    //save every records i.e. updated and deleted in db
                                    if (level == 0)
                                    {
                                        var lstNonCustomFeedMapping = lstFeedMappingCopy.Where(x => x.IsCustomFeedKey == false).ToList();

                                        var lstCustomFeedMapping = lstFeedMappingCopy.Where(x => x.IsCustomFeedKey == true).ToList();

                                        //insert standard feeds
                                        //long? eventId = FeedHelper.Insert(lstNonCustomFeedMapping, feedId, feedStatus, modifiedDate);
                                        long? eventId = FeedHelper.Insert_v1(lstNonCustomFeedMapping, feedId, feedStatus, modifiedDate, isRootEvent: true);

                                        //insert custom feeds
                                        if (eventId != null && feedStatus != "deleted")
                                            FeedHelper.InsertCustomFeed((long)eventId, lstCustomFeedMapping);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        //make new copy of entire list after each record processing
                        lstFeedMappingCopy = GenericCopier<List<FeedMapping>>.DeepCopy(lstFeedMapping);

                        //loop through all configured feed mappings
                        for (int j = 0; j < lstFeedMappingCopy.Count; j++)
                        {
                            lstFeedMappingCopy[j] = UpdateFeedKeyValue(token, lstFeedMappingCopy[j], isFromArray, ref level);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertErrorLogs("[DataLaundryScheduler] SchedulerHelper", "ProcessFeedKeyValues", ex.Message, ex.InnerException?.Message, ex.StackTrace);
            }
        }

        /// <summary>
        /// Process to fetch value of feed key according to given token and insert them
        /// </summary>
        /// <param name="token"></param>
        /// <param name="lstFeedMapping"></param>
        /// <param name="lstFeedMappingCopy"></param>
        /// <param name="currentKeyName"></param>
        /// <param name="level"></param>
        /// <param name="index"></param>
        /// <param name="isArray"></param>
        /// <param name="schedulerLogId"></param>
        private static void ProcessFeedKeyValues_v1(JToken token, List<FeedMapping> lstFeedMapping, out List<FeedMapping> lstFeedMappingCopy, string currentKeyName = "items", int level = 0, int index = 0, bool isArray = true, long schedulerLogId = 0)
        {
            //make new copy of entire list
            lstFeedMappingCopy = GenericCopier<List<FeedMapping>>.DeepCopy(lstFeedMapping);
            bool IsRemoveSession = false;
  
            try
            {
                if (lstFeedMapping != null && lstFeedMapping.Count > 0)
                {
                    string firstActualFeedKeyPath = lstFeedMapping.Where(x => !string.IsNullOrEmpty(x.ActualFeedKeyPath)).FirstOrDefault().ActualFeedKeyPath;

                    bool isFromArray = isArray;
                    var oFilterModel = FilterRuleHelper.GetFilterCriteriaByFeedMappingId(lstFeedMapping?.FirstOrDefault()?.FeedProvider?.Id);
                    if (isArray)
                    {
                        if (!string.IsNullOrEmpty(currentKeyName))
                        {
                            //get all elements of specified array
                            List<JToken> lstJToken = null;

                            if (level == 0)
                                lstJToken = token.SelectTokens("$." + currentKeyName + "[*]").ToList();
                            else
                                lstJToken = token.Parent.Parent.SelectTokens("$." + currentKeyName + "[*]").ToList();

                            if (lstJToken != null && lstJToken.Count > 0)
                            {
                                //loop through each records from array
                                for (int i = 0; i < lstJToken.Count; i++)
                                {
                                    string feedId = "";
                                    string feedStatus = "";
                                    object modifiedDate = null;
                                    IsRemoveSession = false;
                                    
                                    if (level == 0)
                                    {
                                        feedId = lstJToken[i].SelectToken("$.id")?.Value<string>();

                                        feedStatus = lstJToken[i].SelectToken("$.state")?.Value<string>();
                                   
                                        var modifiedJValue = lstJToken[i].SelectToken("$.modified");

                                        if (modifiedJValue != null)
                                        {
                                            switch ((int)modifiedJValue.Type)
                                            {
                                                case 6:
                                                    //int or long
                                                    long longVal = modifiedJValue.Value<long>();
                                                    modifiedDate = CommonFunctions.UnixTimeStampToDateTime(longVal);
                                                    break;
                                                case 7:
                                                    //float
                                                    float floatVal = modifiedJValue.Value<float>();
                                                    modifiedDate = CommonFunctions.UnixTimeStampToDateTime((long)floatVal);
                                                    break;
                                                case 8:
                                                    //string
                                                    string strVal = modifiedJValue.Value<string>();

                                                    if (long.TryParse(strVal, out longVal))
                                                        modifiedDate = CommonFunctions.UnixTimeStampToDateTime(longVal);
                                                    else
                                                        modifiedDate = 0; //intial value of utc timestamp
                                                    break;
                                                case 12:
                                                    //date
                                                    modifiedDate = modifiedJValue.Value<DateTime>();
                                                    break;
                                                default:
                                                    break;
                                            }
                                        }
                                    }

                                    //make new copy of entire list after each record processing
                                    lstFeedMappingCopy = GenericCopier<List<FeedMapping>>.DeepCopy(lstFeedMapping);
                                    //map feed key values only if feed is not deleted
                                    if (feedStatus != "deleted")
                                    {
                                        //loop through all configured feed mappings
                                        for (int j = 0; j < lstFeedMappingCopy.Count; j++)
                                        {
                                            lstFeedMappingCopy[j].FilterModel = oFilterModel;
                                            //lstFeedMappingCopy[j] = UpdateFeedKeyValue(lstJToken[i], lstFeedMappingCopy[j], isFromArray, ref level);
                                            DataCurrentToken = JsonConvert.SerializeObject(lstJToken[i]);
                                            lstFeedMappingCopy[j] = UpdateFeedKeyValue_v1(lstJToken[i], lstFeedMappingCopy[j], isFromArray, ref level, ref IsRemoveSession, schedulerLogId: schedulerLogId);
                                        }
                                    }

                                    //save every records i.e. updated and deleted in db
                                    if (level == 0)
                                    {
                                        var lstNonCustomFeedMapping = lstFeedMappingCopy.Where(x => x.IsCustomFeedKey == false).ToList();

                                        var lstCustomFeedMapping = lstFeedMappingCopy.Where(x => x.IsCustomFeedKey == true).ToList();


                                        //if (!lstFeedMappingCopy.Any(x => x.FilterModel.OperationData.Any(z => z.OperationTypeId == (int)OperationType.RemoveSession)))
                                        if (!IsRemoveSession)
                                        {
                                            //insert standard feeds
                                            //long? eventId = FeedHelper.Insert(lstNonCustomFeedMapping, feedId, feedStatus, modifiedDate);

                                            #region Check end date missing set default date based on startdate and duration
                                            // var lstFind = lstNonCustomFeedMapping.Where(x => x.ActualFeedKeyPath == "" && (x.ColumnName == "endDate" || x.ColumnName == "subEvent_endDate" || x.ColumnName == "superEvent_endDate"))?.ToList();
                                            // if (lstFind.Count() > 0)
                                            //    AutoSetEndDate(lstJToken[i], lstNonCustomFeedMapping, lstFind.FirstOrDefault());
                                            #endregion

                                            long? eventId = FeedHelper.Insert_v1(lstNonCustomFeedMapping, feedId, feedStatus, modifiedDate, isRootEvent: true, schedulerLogId: schedulerLogId);

                                            //insert custom feeds
                                            if (eventId != null && feedStatus != "deleted")
                                            {
                                                FeedHelper.InsertCustomFeed((long)eventId, lstCustomFeedMapping);

                                                #region InsertJsonToken
                                                //below code added 29-01-2019
                                                //JsonImportDataHelper.InsertJsonToken(new JsonImportData()
                                                //{
                                                //    EventID = (long)eventId,
                                                //    FeedID = feedId,
                                                //    JsonData = JsonConvert.SerializeObject(lstJToken[i]),
                                                //    FeedProviderID = lstNonCustomFeedMapping.Select(x => x.FeedProvider).FirstOrDefault().Id
                                                //});
                                                #endregion
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        //make new copy of entire list after each record processing
                        lstFeedMappingCopy = GenericCopier<List<FeedMapping>>.DeepCopy(lstFeedMapping);

                        //loop through all configured feed mappings
                        for (int j = 0; j < lstFeedMappingCopy.Count; j++)
                        {
                            lstFeedMappingCopy[j].FilterModel = oFilterModel;
                            lstFeedMappingCopy[j] = UpdateFeedKeyValue_v1(token, lstFeedMappingCopy[j], isFromArray, ref level, ref IsRemoveSession, schedulerLogId: schedulerLogId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertErrorLogs("[DataLaundryScheduler] SchedulerHelper", "ProcessFeedKeyValues_v1", ex.Message, ex.InnerException?.Message, ex.StackTrace, lstFeedMapping.FirstOrDefault().FeedProvider.Id);
                #region Update to scheduler Log   
                long logId;
                LogHelper.InsertUpdateSchedulerLog(schedulerLogId, lstFeedMapping.FirstOrDefault().FeedProvider.Id, null, DateTime.Now, LogHelper.stringValueOf(LogStatus.ErrorOccurred), out logId, "[DataLaundryScheduler] SchedulerHelper - ProcessFeedKeyValues_v1");
                #endregion
            }
        }

        private static FeedMapping UpdateFeedKeyValue(JToken token, FeedMapping feedMapping, bool isFromArray, ref int level)
        {
            if (token == null || feedMapping == null)
                return feedMapping;

            //get configured value based on saved json path
            string tableName = feedMapping.TableName;
            string columnName = feedMapping.ColumnName;
            string actualFeedKeyPath = feedMapping.ActualFeedKeyPath;
            string feedDataType = feedMapping.FeedDataType;

            if (!string.IsNullOrEmpty(actualFeedKeyPath))
            {
                //var mappedChildrens = feedMapping?.Childrens.Where(x => !string.IsNullOrEmpty(x.ActualFeedKeyPath)).ToList();
                var mappedChildrens = feedMapping?.Childrens;

                int? mappedChildrenCount = feedMapping?.Childrens.Where(x => !string.IsNullOrEmpty(x.ActualFeedKeyPath)).ToList().Count;

                string currentKeyName = "";

                if (isFromArray)
                    currentKeyName = actualFeedKeyPath.Substring(actualFeedKeyPath.LastIndexOf(']') + 1);
                else
                    currentKeyName = actualFeedKeyPath.Substring(actualFeedKeyPath.LastIndexOf('.') + 1);
                try
                {
                    //var jValue = token.SelectToken("$." + currentKeyName);
                    var jValue = token.SelectTokens("$." + currentKeyName).FirstOrDefault();

                    if (jValue != null)
                    {
                        switch ((int)jValue.Type)
                        {
                            case 1:
                                //object
                                var jObject = jValue.Value<JObject>();
                                if (jObject != null && jObject.HasValues)
                                {
                                    feedMapping.FeedDataType = "object";
                                    //feedMapping.FeedKeyValue = jObject;
                                    feedMapping.FeedKeyValue = JsonConvert.SerializeObject(jObject);

                                    if (mappedChildrenCount != null && mappedChildrenCount > 0)
                                    {
                                        //childtoken has any configured objects to fetch
                                        List<FeedMapping> mappedChildrensCopy = null;

                                        ProcessFeedKeyValues(jValue, mappedChildrens, out mappedChildrensCopy, currentKeyName: currentKeyName, level: level, isArray: false);

                                        feedMapping.Childrens = mappedChildrensCopy;
                                    }
                                }

                                break;
                            case 2:
                                //array
                                var jArray = jValue.Value<JArray>();
                                if (jArray.HasValues)
                                {
                                    feedMapping.FeedDataType = "array";
                                    //feedMapping.FeedKeyValue = jArray;
                                    feedMapping.FeedKeyValue = JsonConvert.SerializeObject(jArray);

                                    currentKeyName = actualFeedKeyPath.Substring(actualFeedKeyPath.LastIndexOf('.') + 1);

                                    //Commented on 
                                    //Date : 07/08/2018
                                    //Reason : due to multiple subevents not getting stored, only last item of sub event getting stored
                                    //var currentTokens = jValue.Parent.Parent.SelectTokens("$." + currentKeyName).ToList();

                                    var currentTokens = jValue.Parent.Parent.SelectToken("$." + currentKeyName);

                                    if (mappedChildrenCount != null && mappedChildrenCount > 0)
                                    {
                                        //this is array of complex type such as object
                                        level++;

                                        //Commented on 
                                        //Date : 07/08/2018
                                        //Reason : due to multiple subevents not getting stored, only last item of sub event getting stored
                                        //for (int j = 0; j < currentTokens.Count; j++)

                                        for (int j = 0; j < currentTokens.Count(); j++)
                                        {
                                            if (j == 0)
                                                feedMapping.ChildrenRecords = new List<List<FeedMapping>>();

                                            //make new copy of entire list
                                            var mappedChildrensCopy = GenericCopier<List<FeedMapping>>.DeepCopy(mappedChildrens);


                                            //Commented on 
                                            //Date : 07/08/2018
                                            //Reason : due to multiple subevents not getting stored, only last item of sub event getting stored
                                            //ProcessFeedKeyValues(currentTokens[j], mappedChildrens, out mappedChildrensCopy, currentKeyName: currentKeyName, level: level, isArray: true);

                                            ProcessFeedKeyValues(currentTokens[j], mappedChildrens, out mappedChildrensCopy, currentKeyName: currentKeyName, level: level, isArray: false);

                                            feedMapping.ChildrenRecords.Add(mappedChildrensCopy);
                                        }
                                        level--;
                                    }
                                    else
                                    {
                                        //this is the array of simple types such as string, int, float, bool etc
                                    }
                                }

                                break;
                            case 6:
                                //integer
                                int integer = jValue.Value<int>();
                                feedMapping.FeedDataType = "int";
                                feedMapping.FeedKeyValue = integer;
                                break;
                            case 7:
                                //float
                                float floatVal = jValue.Value<float>();
                                feedMapping.FeedDataType = "float";
                                feedMapping.FeedKeyValue = floatVal;
                                break;
                            case 8:
                                //string
                                string strVal = jValue.Value<string>();
                                feedMapping.FeedDataType = "string";
                                feedMapping.FeedKeyValue = strVal;
                                break;
                            case 9:
                                //bool
                                bool boolVal = jValue.Value<bool>();
                                feedMapping.FeedDataType = "bool";
                                feedMapping.FeedKeyValue = boolVal;
                                break;
                            case 10:
                                //Null
                                feedMapping.FeedDataType = "null";
                                break;
                            case 12:
                                //date
                                var date = jValue.Value<DateTime>();
                                feedMapping.FeedDataType = "date";
                                feedMapping.FeedKeyValue = date;
                                break;
                            case 17:
                                //timespan
                                var timeSpan = jValue.Value<TimeSpan>();
                                feedMapping.FeedDataType = "timespan";
                                feedMapping.FeedKeyValue = timeSpan;
                                break;
                            default:
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.InsertErrorLogs("[DataLaundryScheduler] SchedulerHelper", "UpdateFeedKeyValue", ex.Message, ex.InnerException?.Message, ex.StackTrace);
                }
            }

            return feedMapping;
        }

        /// <summary>
        /// fetch the feed key value from token
        /// </summary>
        /// <param name="token"></param>
        /// <param name="feedMapping"></param>
        /// <param name="isFromArray"></param>
        /// <param name="level"></param>
        /// <param name="schedulerLogId"></param>
        /// <returns></returns>
        private static FeedMapping UpdateFeedKeyValue_v1(JToken token, FeedMapping feedMapping, bool isFromArray, ref int level, ref bool IsRemoveSession, long schedulerLogId = 0)
        {
            if (token == null || feedMapping == null)
                return feedMapping;

            //get configured value based on saved json path
            string tableName = feedMapping.TableName;
            string columnName = feedMapping.ColumnName;
            string actualFeedKeyPath = feedMapping.ActualFeedKeyPath;
            string feedDataType = feedMapping.FeedDataType;

            if (!string.IsNullOrEmpty(actualFeedKeyPath))
            {
                //var mappedChildrens = feedMapping?.Childrens.Where(x => !string.IsNullOrEmpty(x.ActualFeedKeyPath)).ToList();
                var mappedChildrens = feedMapping?.Childrens;

                int? mappedChildrenCount = feedMapping?.Childrens.Where(x => !string.IsNullOrEmpty(x.ActualFeedKeyPath)).ToList().Count;

                string currentKeyName = "";

                if (isFromArray)
                    currentKeyName = actualFeedKeyPath.Substring(actualFeedKeyPath.LastIndexOf(']') + 1);
                else
                    currentKeyName = actualFeedKeyPath.Substring(actualFeedKeyPath.LastIndexOf('.') + 1);
                try
                {                   

                    var jValue = token.SelectTokens("$." + currentKeyName).FirstOrDefault();

                    if (jValue != null)
                    {
                        #region Filter Data Here
                        if (feedMapping.FilterModel != null && feedMapping.FilterModel.FilterCriteria.Count > 0)
                        {
                            token = FilterData(token, feedMapping, actualFeedKeyPath, isFromArray, ref IsRemoveSession, DataCurrentToken);
                            jValue = token.SelectTokens("$." + currentKeyName).FirstOrDefault();
                        }
                        #endregion

                        switch ((int)jValue.Type)
                        {
                            case 1:
                                //object
                                var jObject = jValue.Value<JObject>();
                                if (jObject != null && jObject.HasValues)
                                {
                                    feedMapping.FeedDataType = "object";
                                    //feedMapping.FeedKeyValue = jObject;
                                    feedMapping.FeedKeyValue = JsonConvert.SerializeObject(jObject);

                                    if (mappedChildrenCount != null && mappedChildrenCount > 0)
                                    {
                                        //childtoken has any configured objects to fetch
                                        List<FeedMapping> mappedChildrensCopy = null;

                                        //ProcessFeedKeyValues(jValue, mappedChildrens, out mappedChildrensCopy, currentKeyName: currentKeyName, level: level, isArray: false);
                                        ProcessFeedKeyValues_v1(jValue, mappedChildrens, out mappedChildrensCopy, currentKeyName: currentKeyName, level: level, isArray: false, schedulerLogId: schedulerLogId);

                                        feedMapping.Childrens = mappedChildrensCopy;
                                    }
                                }

                                break;
                            case 2:
                                //array
                                var jArray = jValue.Value<JArray>();
                                if (jArray.HasValues)
                                {
                                    feedMapping.FeedDataType = "array";
                                    //feedMapping.FeedKeyValue = jArray;
                                    feedMapping.FeedKeyValue = JsonConvert.SerializeObject(jArray);

                                    currentKeyName = actualFeedKeyPath.Substring(actualFeedKeyPath.LastIndexOf('.') + 1);

                                    //Commented on 
                                    //Date : 07/08/2018
                                    //Reason : due to multiple subevents not getting stored, only last item of sub event getting stored
                                    //var currentTokens = jValue.Parent.Parent.SelectTokens("$." + currentKeyName).ToList();

                                    var currentTokens = jValue.Parent.Parent.SelectToken("$." + currentKeyName);

                                    if (mappedChildrenCount != null && mappedChildrenCount > 0)
                                    {
                                        //this is array of complex type such as object
                                        level++;

                                        //Commented on 
                                        //Date : 07/08/2018
                                        //Reason : due to multiple subevents not getting stored, only last item of sub event getting stored
                                        //for (int j = 0; j < currentTokens.Count; j++)

                                        for (int j = 0; j < currentTokens.Count(); j++)
                                        {
                                            if (j == 0)
                                                feedMapping.ChildrenRecords = new List<List<FeedMapping>>();

                                            //make new copy of entire list
                                            var mappedChildrensCopy = GenericCopier<List<FeedMapping>>.DeepCopy(mappedChildrens);


                                            //Commented on 
                                            //Date : 07/08/2018
                                            //Reason : due to multiple subevents not getting stored, only last item of sub event getting stored
                                            //ProcessFeedKeyValues(currentTokens[j], mappedChildrens, out mappedChildrensCopy, currentKeyName: currentKeyName, level: level, isArray: true);

                                            //ProcessFeedKeyValues(currentTokens[j], mappedChildrens, out mappedChildrensCopy, currentKeyName: currentKeyName, level: level, isArray: false);
                                            ProcessFeedKeyValues_v1(currentTokens[j], mappedChildrens, out mappedChildrensCopy, currentKeyName: currentKeyName, level: level, isArray: false, schedulerLogId: schedulerLogId);

                                            feedMapping.ChildrenRecords.Add(mappedChildrensCopy);
                                        }
                                        level--;
                                    }
                                    else
                                    {
                                        //this is the array of simple types such as string, int, float, bool etc
                                    }
                                }
                                break;
                            case 6:
                                //integer
                                int integer = jValue.Value<int>();
                                feedMapping.FeedDataType = "int";
                                feedMapping.FeedKeyValue = integer;
                                break;
                            case 7:
                                //float
                                float floatVal = jValue.Value<float>();
                                feedMapping.FeedDataType = "float";
                                feedMapping.FeedKeyValue = floatVal;
                                break;
                            case 8:
                                //string
                                string strVal = jValue.Value<string>();
                                feedMapping.FeedDataType = "string";
                                feedMapping.FeedKeyValue = strVal;
                                break;
                            case 9:
                                //bool
                                bool boolVal = jValue.Value<bool>();
                                feedMapping.FeedDataType = "bool";
                                feedMapping.FeedKeyValue = boolVal;
                                break;
                            case 10:
                                //Null
                                feedMapping.FeedDataType = "null";
                                break;
                            case 12:
                                //date
                                var date = jValue.Value<DateTime>();
                                feedMapping.FeedDataType = "date";
                                feedMapping.FeedKeyValue = date;
                                break;
                            case 17:
                                //timespan
                                var timeSpan = jValue.Value<TimeSpan>();
                                feedMapping.FeedDataType = "timespan";
                                feedMapping.FeedKeyValue = timeSpan;
                                break;
                            default:
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.InsertErrorLogs("[DataLaundryScheduler] SchedulerHelper", "UpdateFeedKeyValue_v1", ex.Message, ex.InnerException?.Message, ex.StackTrace, feedMapping.FeedProvider.Id);
                    #region Update to scheduler Log   
                    long logId;
                    LogHelper.InsertUpdateSchedulerLog(schedulerLogId, feedMapping.FeedProvider.Id, null, DateTime.Now, LogHelper.stringValueOf(LogStatus.ErrorOccurred), out logId, "[DataLaundryScheduler] SchedulerHelper - UpdateFeedKeyValue_v1");
                    #endregion
                }
            }

            return feedMapping;
        }

        public static void FetchFeeds(int offset = 0)
        {
            #region Save into Eventlog
            LogHelper.InsertEventLog(null, null, null, "[DataLaundryScheduler] SchedulerHelper", "FetchFeeds", "Ready to fetch scheduled feed providers");
            #endregion

            var lstSchedulerSettings = GetScheduledFeedProviders(offset);

            if (lstSchedulerSettings == null || lstSchedulerSettings.Count == 0)
                return;

            //get feeds of each feed providers
            foreach (var schedulerSettings in lstSchedulerSettings)
            {
                //create background threads for each feed providers
                var threadFeedFetchData = new Thread(() =>
                {
                    #region Insert to scheduler Log
                    long logId;
                    LogHelper.InsertUpdateSchedulerLog(0, schedulerSettings.FeedProvider.Id, DateTime.Now, null, LogHelper.stringValueOf(LogStatus.JobStarted), out logId);
                    #endregion

                    #region Fetch data
                    string afterTimestamp = "";
                    string afterChangenumber = "";

                    if (schedulerSettings.FeedProvider.IsUsesTimestamp)
                    {
                        if (schedulerSettings.FeedProvider.IsUtcTimestamp)
                            afterTimestamp = schedulerSettings.CurrentUtcTimestamp.ToString();
                        else
                            afterTimestamp = DateTime.UtcNow.AddDays(Convert.ToInt16(Settings.GetAppSettingDaysBefore() ?? "-5")).ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        afterChangenumber = "";
                    }

                    //FetchFeedFromFeedProvider(schedulerSettings, afterTimestamp, afterChangenumber);
                    FetchFeedFromFeedProvider_v1(schedulerSettings, afterTimestamp, afterChangenumber, logId, new List<string>());
                    #endregion

                    //#region Update to scheduler Log
                    //LogHelper.InsertUpdateDataToLogFile(logId, schedulerSettings.FeedProvider.Id, null, DateTime.Now, LogHelper.stringValueOf(LogStatus.JobCompleted), out logId);
                    //#endregion
                });

                threadFeedFetchData.IsBackground = true;
                threadFeedFetchData.Start();
            }
        }

        /// <summary>
        /// List out the scheduled feeds for data importation
        /// </summary>
        /// <param name="offset"></param>
        public static void FetchFeeds_v1(int offset = 0)
        {
            #region Save into Eventlog
            LogHelper.InsertEventLog(null, null, null, "[DataLaundryScheduler] SchedulerHelper", "FetchFeeds_v1", "Ready to fetch scheduled feed providers");
            #endregion

            var lstSchedulerSettings = GetScheduledFeedProviders(offset);

            if (lstSchedulerSettings == null || lstSchedulerSettings.Count == 0)
                return;

            foreach (var schedulerSettings in lstSchedulerSettings)
            {
                var threadFeedFetchData = new Thread(() =>
                {
                    #region Update Status to Scheduler Settings

                    #region Copy scheduler setting object to another without reference
                    //var schedulerSettingsCopy = GenericCopier<SchedulerSettings>.DeepCopy(schedulerSettings);
                    //schedulerSettingsCopy.IsStarted = true;
                    //schedulerSettingsCopy.IsCompleted = false;
                    //schedulerSettingsCopy.IsTerminated = false;
                    //var isStarted = UpdateSchedulerStatus(schedulerSettingsCopy);
                    #endregion

                    schedulerSettings.IsStarted = true;
                    schedulerSettings.IsCompleted = false;
                    schedulerSettings.IsTerminated = false;
                    var isStarted = UpdateSchedulerStatus(schedulerSettings);
                    if (isStarted)
                    {
                        #region Insert to scheduler Log
                        long logId;
                        LogHelper.InsertUpdateSchedulerLog(0, schedulerSettings.FeedProvider.Id, DateTime.Now, null, LogHelper.stringValueOf(LogStatus.JobStarted), out logId);
                        #endregion

                        #region Save into Eventlog
                        LogHelper.InsertEventLog(schedulerSettings.FeedProvider.Id, null, null, "[DataLaundryScheduler] SchedulerHelper", "FetchFeeds_v1", "started periodically");
                        #endregion

                        #region Fetch data
                        string afterTimestamp = "";
                        string afterChangenumber = "";

                        //if (schedulerSettings.FeedProvider.IsUsesTimestamp)
                        //{
                        //    if (schedulerSettings.FeedProvider.IsUtcTimestamp)
                        //        afterTimestamp = schedulerSettings.CurrentUtcTimestamp.ToString();
                        //    else
                        //        afterTimestamp = DateTime.UtcNow.AddDays(Convert.ToInt16(Settings.GetAppSetting("DaysBefore") ?? "-5")).ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                        //}

                        if (!string.IsNullOrEmpty(schedulerSettings.NextPageUrlAfterExecution) && schedulerSettings.FeedProvider.IsUsesTimestamp)
                        {
                            if (!schedulerSettings.IsTerminated && !string.IsNullOrEmpty(schedulerSettings.SchedulerLastStartTime.ToString()))
                                afterTimestamp = Convert.ToDateTime(schedulerSettings.SchedulerLastStartTime).ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);

                            if (!schedulerSettings.IsTerminated && !string.IsNullOrEmpty(schedulerSettings.SchedulerLastStartTimeStamp.ToString())
                                     && schedulerSettings.FeedProvider.IsUtcTimestamp)
                                afterTimestamp = schedulerSettings.SchedulerLastStartTimeStamp.ToString();
                        }

                        FetchFeedFromFeedProvider_v2(schedulerSettings, afterTimestamp, afterChangenumber, logId, new List<string>());
                        #endregion
                    }
                    #endregion
               });
               threadFeedFetchData.IsBackground = true;
             threadFeedFetchData.Start();
            }
        }

        public static void DeleteFeeds()
        {
            var threadFeedFetchData = new Thread(() =>
            {
                DeleteEventData();
            });

            threadFeedFetchData.IsBackground = true;
            threadFeedFetchData.Start();
        }

        private static void DeleteEventData()
        {
            try
            {
                int rowsAffected = DBProvider.ExecuteNonQuery("Event_Delete", CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                LogHelper.InsertErrorLogs("[DataLaundryScheduler] SchedulerHelper", "DeleteEventData", ex.Message, ex.InnerException?.Message, ex.StackTrace);
            }
        }

        public static void DeletePastOccurredEvent()
        {
            var threadFeedFetchData = new Thread(() =>
            {
                DeletePastOccurredEventData();
            });

            threadFeedFetchData.IsBackground = true;
            threadFeedFetchData.Start();
        }

        private static void DeletePastOccurredEventData()
        {
            try
            {
                int rowsAffected = DBProvider.ExecuteNonQuery("Event_DeletePastOccurrence", CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                LogHelper.InsertErrorLogs("[DataLaundryScheduler] SchedulerHelper", "DeletePastOccurredEvent", ex.Message, ex.InnerException?.Message, ex.StackTrace);
            }
        }

        /// <summary>
        /// Update feed provider's scheduling status whether its started,completed or terminated
        /// </summary>
        /// <param name="schedulerSetting"></param>
        /// <returns></returns>
        private static bool UpdateSchedulerStatus(SchedulerSettings schedulerSetting)
        {
            var lstSqlParameter = new List<SqlParameter>();

            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Id", SqlDbType = SqlDbType.BigInt, Value = schedulerSetting.Id });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@IsStarted", SqlDbType = SqlDbType.Bit, Value = (object)schedulerSetting.IsStarted ?? DBNull.Value });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@IsCompleted", SqlDbType = SqlDbType.Bit, Value = (object)schedulerSetting.IsCompleted ?? DBNull.Value });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@IsTerminated", SqlDbType = SqlDbType.Bit, Value = (object)schedulerSetting.IsTerminated ?? DBNull.Value });

            int rowsAffected = DBProvider.ExecuteNonQuery("SchedulerSettings_UpdateStatus", CommandType.StoredProcedure, ref lstSqlParameter);
            return rowsAffected > 0;
        }

        public static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            Console.WriteLine("Certificate error: {0}", sslPolicyErrors);

            // Do not allow this client to communicate with unauthenticated servers. 
            return false;
        }

        public static byte[] ReadFully(Stream stream, int initialLength)
        {
            // If we've been passed an unhelpful initial length, just
            // use 32K.
            if (initialLength < 1)
            {
                initialLength = 32768;
            }

            byte[] buffer = new byte[initialLength];
            int read = 0;


            int chunk;
            while ((chunk = stream.Read(buffer, read, buffer.Length - read)) > 0)
            {
                read += chunk;


                // If we've reached the end of our buffer, check to see if there's
                // any more information
                if (read == buffer.Length)
                {
                    int nextByte = stream.ReadByte();


                    // End of stream? If so, we're done
                    if (nextByte == -1)
                    {
                        return buffer;
                    }


                    // Nope. Resize the buffer, put in the byte we've just
                    // read, and continue
                    byte[] newBuffer = new byte[buffer.Length * 2];
                    Array.Copy(buffer, newBuffer, buffer.Length);
                    newBuffer[read] = (byte)nextByte;
                    buffer = newBuffer;
                    read++;
                }
            }
            // Buffer is now too big. Shrink it.
            byte[] ret = new byte[read];
            Array.Copy(buffer, ret, read);
            return ret;
        }

        public static JToken FilterData(JToken token, FeedMapping feedMapping, string ActualFeedKeyPath, bool isFromArray, ref bool IsRemoveSession, string DataCurrentToken)
        {
            JToken oData = token;
            try
            {
                var CloneToken = JsonConvert.DeserializeObject<JToken>(DataCurrentToken).DeepClone();

                List<ValidData> lstValidData = new List<ValidData>();
                int oCounter = 0;
                bool IsFindValue = false, IsLastFalg = false;
                int LastRuleOperatorId = 0;
                var lstRuleId = new int[] { 0, 1, 2 };

                #region Apply Rule & Filter value
                if (feedMapping.FilterModel != null && feedMapping.FilterModel.FilterCriteria.Count > 0)
                {
                    var oGroupeFilterCriteria = feedMapping.FilterModel.FilterCriteria.GroupBy(x => x.RuleId).ToList();

                    var oGroupeOperation = feedMapping.FilterModel.OperationData.Where(x => oGroupeFilterCriteria.Any(z => z.Key == x.RuleId)).GroupBy(x => x.RuleId).ToList();

                    /*First Maping Filter data*/
                    foreach (var lstGroupFilterCriteria in oGroupeFilterCriteria)
                    {
                        lstValidData = new List<ValidData>();
                        IsLastFalg = false;
                        IsFindValue = false;
                        foreach (var oFilter in lstGroupFilterCriteria)
                        {
                            string currentKeyName = "";
                            //if (isFromArray)
                            currentKeyName = oFilter.ActualFeedKeyPath.Substring(oFilter.ActualFeedKeyPath.LastIndexOf(']') + 1);
                            if (string.IsNullOrEmpty(currentKeyName))
                                return oData;
                           
                            var jValue = CloneToken.SelectTokens("$." + currentKeyName).FirstOrDefault();
                            if (jValue == null)
                            {
                                return oData;
                            }
                            #region AllOperator
                            switch (oFilter.OperatorId)
                            {
                                case 1: // Is less than
                                    #region ISLESSTHAN
                                    switch ((int)jValue.Type)
                                    {
                                        case 6:
                                            //integer
                                            int intFilterValue = Convert.ToInt32(oFilter.Value);
                                            int integerValue = jValue.Value<int>();
                                            if (integerValue < intFilterValue)
                                                IsFindValue = true;
                                            else
                                                IsFindValue = false;
                                            break;
                                        case 7:
                                            //float
                                            float floatFilterValue = float.Parse(oFilter.Value);
                                            float floatvalue = jValue.Value<float>();
                                            if (floatvalue < floatFilterValue)
                                                IsFindValue = true;
                                            else
                                                IsFindValue = false;
                                            break;
                                        case 8:
                                            //string                                       
                                            break;
                                        case 9:
                                            //bool                                     
                                            break;
                                        case 10:
                                            //Null                                      
                                            break;
                                        case 12:
                                            //date
                                            DateTime dtFilterValue = Convert.ToDateTime(oFilter.Value);
                                            DateTime dtvalue = jValue.Value<DateTime>();
                                            if (dtvalue < dtFilterValue)
                                                IsFindValue = true;
                                            else
                                                IsFindValue = false;
                                            break;
                                        case 17:
                                            //timespan
                                            TimeSpan timeFilterValue = TimeSpan.Parse(oFilter.Value);
                                            TimeSpan timevalue = jValue.Value<TimeSpan>();
                                            if (timevalue < timeFilterValue)
                                                IsFindValue = true;
                                            else
                                                IsFindValue = false;
                                            break;
                                    }
                                    #endregion
                                    break;

                                case 2: // Is higher than
                                    #region ISHIGERTHAN
                                    switch ((int)jValue.Type)
                                    {
                                        case 6:
                                            //integer
                                            int intFilterValue = Convert.ToInt32(oFilter.Value);
                                            int integerValue = jValue.Value<int>();
                                            if (integerValue > intFilterValue)
                                                IsFindValue = true;
                                            else
                                                IsFindValue = false;
                                            break;
                                        case 7:
                                            //float
                                            float floatFilterValue = float.Parse(oFilter.Value);
                                            float floatvalue = jValue.Value<float>();
                                            if (floatvalue > floatFilterValue)
                                                IsFindValue = true;
                                            else
                                                IsFindValue = false;
                                            break;
                                        case 8:
                                            //string                                       
                                            break;
                                        case 9:
                                            //bool                                     
                                            break;
                                        case 10:
                                            //Null                                      
                                            break;
                                        case 12:
                                            //date
                                            DateTime dtFilterValue = Convert.ToDateTime(oFilter.Value);
                                            DateTime dtvalue = jValue.Value<DateTime>();
                                            if (dtvalue > dtFilterValue)
                                                IsFindValue = true;
                                            else
                                                IsFindValue = false;
                                            break;
                                        case 17:
                                            //timespan
                                            TimeSpan timeFilterValue = TimeSpan.Parse(oFilter.Value);
                                            TimeSpan timevalue = jValue.Value<TimeSpan>();
                                            if (timevalue > timeFilterValue)
                                                IsFindValue = true;
                                            else
                                                IsFindValue = false;
                                            break;
                                    }
                                    #endregion
                                    break;

                                case 3: //Is equals
                                    #region ISEQUAL
                                    switch ((int)jValue.Type)
                                    {
                                        case 6:
                                            //integer
                                            int intFilterValue = Convert.ToInt32(oFilter.Value);
                                            int integerValue = jValue.Value<int>();
                                            if (integerValue == intFilterValue)
                                                IsFindValue = true;
                                            else
                                                IsFindValue = false;
                                            break;
                                        case 7:
                                            //float
                                            float floatFilterValue = float.Parse(oFilter.Value);
                                            float floatvalue = jValue.Value<float>();
                                            if (floatvalue == floatFilterValue)
                                                IsFindValue = true;
                                            else
                                                IsFindValue = false;
                                            break;
                                        case 8:
                                            //string    
                                            string stringFilterValue = Convert.ToString(oFilter.Value);
                                            string stringvalue = jValue.Value<string>();
                                            if (stringvalue == stringFilterValue)
                                                IsFindValue = true;
                                            else
                                                IsFindValue = false;
                                            break;
                                        case 9:
                                            //bool     
                                            bool boolFilterValue = Convert.ToBoolean(oFilter.Value);
                                            bool boolvalue = jValue.Value<bool>();
                                            if (boolvalue == boolFilterValue)
                                                IsFindValue = true;
                                            else
                                                IsFindValue = false;
                                            break;
                                        case 10:
                                            //Null                                      
                                            break;
                                        case 12:
                                            //date
                                            DateTime dtFilterValue = Convert.ToDateTime(oFilter.Value);
                                            DateTime dtvalue = jValue.Value<DateTime>();
                                            if (dtvalue == dtFilterValue)
                                                IsFindValue = true;
                                            else
                                                IsFindValue = false;
                                            break;
                                        case 17:
                                            //timespan
                                            TimeSpan timeFilterValue = TimeSpan.Parse(oFilter.Value);
                                            TimeSpan timevalue = jValue.Value<TimeSpan>();
                                            if (timevalue == timeFilterValue)
                                                IsFindValue = true;
                                            else
                                                IsFindValue = false;
                                            break;
                                    }
                                    #endregion
                                    break;

                                case 4: // Is not equal
                                    #region ISNOTEQUAL
                                    switch ((int)jValue.Type)
                                    {
                                        case 6:
                                            //integer
                                            int intFilterValue = Convert.ToInt32(oFilter.Value);
                                            int integerValue = jValue.Value<int>();
                                            if (integerValue != intFilterValue)
                                                IsFindValue = true;
                                            else
                                                IsFindValue = false;
                                            break;
                                        case 7:
                                            //float
                                            float floatFilterValue = float.Parse(oFilter.Value);
                                            float floatvalue = jValue.Value<float>();
                                            if (floatvalue != floatFilterValue)
                                                IsFindValue = true;
                                            else
                                                IsFindValue = false;
                                            break;
                                        case 8:
                                            //string    
                                            string stringFilterValue = Convert.ToString(oFilter.Value);
                                            string stringvalue = jValue.Value<string>();
                                            if (stringvalue != stringFilterValue)
                                                IsFindValue = true;
                                            else
                                                IsFindValue = false;
                                            break;
                                        case 9:
                                            //bool     
                                            bool boolFilterValue = Convert.ToBoolean(oFilter.Value);
                                            bool boolvalue = jValue.Value<bool>();
                                            if (boolvalue != boolFilterValue)
                                                IsFindValue = true;
                                            else
                                                IsFindValue = false;
                                            break;
                                        case 10:
                                            //Null                                      
                                            break;
                                        case 12:
                                            //date
                                            DateTime dtFilterValue = Convert.ToDateTime(oFilter.Value);
                                            DateTime dtvalue = jValue.Value<DateTime>();
                                            if (dtvalue != dtFilterValue)
                                                IsFindValue = true;
                                            else
                                                IsFindValue = false;
                                            break;
                                        case 17:
                                            //timespan
                                            TimeSpan timeFilterValue = TimeSpan.Parse(oFilter.Value);
                                            TimeSpan timevalue = jValue.Value<TimeSpan>();
                                            if (timevalue != timeFilterValue)
                                                IsFindValue = true;
                                            else
                                                IsFindValue = false;
                                            break;
                                    }
                                    #endregion
                                    break;

                                case 5: // Starts with
                                    #region  startwith
                                    if (Convert.ToString(jValue).ToLower().StartsWith(oFilter.Value.ToLower()))
                                        IsFindValue = true;
                                    else
                                        IsFindValue = false;
                                    #endregion
                                    break;

                                case 6: // Contains
                                    #region Contains
                                    if (Convert.ToString(jValue).ToLower().Contains(oFilter.Value.ToLower()))
                                        IsFindValue = true;
                                    else
                                        IsFindValue = false;
                                    #endregion
                                    break;

                                case 7: // End with
                                    #region EndWith
                                    if (Convert.ToString(jValue).ToLower().EndsWith(oFilter.Value.ToLower()))
                                        IsFindValue = true;
                                    else
                                        IsFindValue = false;
                                    #endregion
                                    break;
                                case 8://IS NULL
                                    #region IS NULL
                                    if (string.IsNullOrEmpty(Convert.ToString(jValue)))
                                        IsFindValue = true;
                                    else
                                        IsFindValue = false;
                                    break;
                                #endregion
                                case 9://IS NOT NULL
                                    #region  ISNOTNULL
                                    if (!string.IsNullOrEmpty(Convert.ToString(jValue)))
                                        IsFindValue = true;
                                    else
                                        IsFindValue = false;
                                    #endregion
                                    break;

                            }
                            #endregion

                            oCounter++;

                            lstValidData.Add(new ValidData()
                            {
                                Counter = oCounter,
                                IsValid = IsFindValue,
                                RuleOperatiorName = oFilter.RuleOperatorName,
                                RuleOperatiorId = oFilter.RuleOperatorId,
                                Value = jValue
                            });

                            if ((IsLastFalg || IsFindValue) && (LastRuleOperatorId == 0 || LastRuleOperatorId == 2))
                            {
                                lstValidData.ForEach(x =>
                                {
                                    x.IsValid = true;
                                });
                            }

                            IsLastFalg = IsFindValue;

                            LastRuleOperatorId = oFilter.RuleOperatorId;
                        }

                        foreach (var lstGroupeOperation in oGroupeOperation.Where(x => x.Key == lstGroupFilterCriteria.Key).ToList())
                        {
                            foreach (var oOperation in lstGroupeOperation)
                            {
                                switch (oOperation.OperationTypeId)
                                {
                                    case 1: //Value Assignment
                                        #region Value Assignment
                                        if (ActualFeedKeyPath == oOperation.ActualFeedKeyPath && !lstValidData.Any(x => x.IsValid == false))
                                        {
                                            string currentKeyName = "";
                                            if (isFromArray)
                                                currentKeyName = oOperation.ActualFeedKeyPath.Substring(oOperation.ActualFeedKeyPath.LastIndexOf(']') + 1);
                                            else
                                                currentKeyName = oOperation.ActualFeedKeyPath.Substring(oOperation.ActualFeedKeyPath.LastIndexOf('.') + 1);

                                            token.SelectTokens("$." + currentKeyName).FirstOrDefault().Replace(oOperation.Value);

                                            oData = token;                                            
                                        }
                                        #endregion
                                        break;

                                    case 2: //Field Assignment
                                        #region Field Assignment
                                        if (ActualFeedKeyPath == oOperation.ActualFeedKeyPath && !lstValidData.Any(x => x.IsValid == false))
                                        {
                                            if (!string.IsNullOrEmpty(oOperation.TempActualFeedKeyPath) && !string.IsNullOrEmpty(oOperation.TempFeedKey))
                                            {
                                                string currentKeyName = "";
                                                if (isFromArray)
                                                    currentKeyName = oOperation.TempActualFeedKeyPath.Substring(oOperation.TempActualFeedKeyPath.LastIndexOf(']') + 1);
                                                else
                                                    currentKeyName = oOperation.TempActualFeedKeyPath.Substring(oOperation.TempActualFeedKeyPath.LastIndexOf('.') + 1);

                                                var jValue = (token.SelectTokens("$." + currentKeyName).FirstOrDefault() != null ? token.SelectTokens("$." + currentKeyName).FirstOrDefault() : CloneToken.SelectTokens("$." + currentKeyName).FirstOrDefault());

                                                if (jValue != null)
                                                {
                                                    if (isFromArray)
                                                        currentKeyName = oOperation.ActualFeedKeyPath.Substring(oOperation.ActualFeedKeyPath.LastIndexOf(']') + 1);
                                                    else
                                                        currentKeyName = oOperation.ActualFeedKeyPath.Substring(oOperation.ActualFeedKeyPath.LastIndexOf('.') + 1);

                                                    token.SelectTokens("$." + currentKeyName).FirstOrDefault().Replace(jValue);

                                                    oData = token;
                                                }
                                            }                                            
                                        }
                                        #endregion
                                        break;

                                    case 3: //Keyword/Sentence Replacement
                                        #region Keyword/Sentence Replacement
                                        var lstWords = lstGroupeOperation.ToList();
                                        if (lstWords.Count() > 0)
                                        {
                                            for (int i = 0; i < lstWords.Count(); i++)
                                            {
                                                if (ActualFeedKeyPath == lstWords[i].ActualFeedKeyPath && !lstValidData.Any(x => x.IsValid == false))
                                                {
                                                    string currentKeyName = "";
                                                    if (isFromArray)
                                                        currentKeyName = lstWords[i].ActualFeedKeyPath.Substring(lstWords[i].ActualFeedKeyPath.LastIndexOf(']') + 1);
                                                    else
                                                        currentKeyName = lstWords[i].ActualFeedKeyPath.Substring(lstWords[i].ActualFeedKeyPath.LastIndexOf('.') + 1);

                                                    var jValue = token.SelectTokens("$." + currentKeyName).FirstOrDefault();

                                                    if (jValue != null)
                                                    {
                                                        var newjValue = jValue.ToString().Replace(lstWords[i].CurrentWord, lstWords[i].NewWord);

                                                        token.SelectTokens("$." + currentKeyName).FirstOrDefault().Replace(newjValue);                                                        
                                                        oData = token;
                                                    }
                                                }
                                            }                                            
                                        }
                                        #endregion
                                        break;

                                    case 4: //Remove Sentence
                                        #region Remove Sentence
                                        var lstRemoveWords = lstGroupeOperation.ToList();
                                        if (lstRemoveWords.Count() > 0)
                                        {
                                            for (int i = 0; i < lstRemoveWords.Count(); i++)
                                            {
                                                if (ActualFeedKeyPath == lstRemoveWords[i].ActualFeedKeyPath && !lstValidData.Any(x => x.IsValid == false))
                                                {
                                                    string currentKeyName = "";
                                                    if (isFromArray)
                                                        currentKeyName = lstRemoveWords[i].ActualFeedKeyPath.Substring(lstRemoveWords[i].ActualFeedKeyPath.LastIndexOf(']') + 1);
                                                    else
                                                        currentKeyName = lstRemoveWords[i].ActualFeedKeyPath.Substring(lstRemoveWords[i].ActualFeedKeyPath.LastIndexOf('.') + 1);

                                                    var jValue = token.SelectTokens("$." + currentKeyName).FirstOrDefault();

                                                    if (jValue != null)
                                                    {

                                                        var newjValue = jValue.ToString().Replace(lstRemoveWords[i].Sentance, "");

                                                        token.SelectTokens("$." + currentKeyName).FirstOrDefault().Replace(newjValue);

                                                        oData = token;
                                                    }
                                                }
                                            }                                            
                                        }
                                        #endregion
                                        break;

                                    case 5: //Calculation
                                        #region Calculation
                                        var lstCalculation = lstGroupeOperation.ToList();
                                        if (lstCalculation.Count > 0)
                                        {
                                            for (int i = 0; i < lstCalculation.Count(); i++)
                                            {
                                                if (ActualFeedKeyPath == lstCalculation[i].ActualFeedKeyPath
                                                    && !lstValidData.Any(x => x.IsValid == false))
                                                {
                                                    string currentKeyName = "";

                                                    if (lstCalculation[i].Value == "0")//additional
                                                    {
                                                        #region additional
                                                        if (isFromArray)
                                                            currentKeyName = lstCalculation[i].TempFRActualFeedKeyPath.Substring(lstCalculation[i].TempFRActualFeedKeyPath.LastIndexOf(']') + 1);
                                                        else
                                                            currentKeyName = lstCalculation[i].TempFRActualFeedKeyPath.Substring(lstCalculation[i].TempFRActualFeedKeyPath.LastIndexOf('.') + 1);

                                                        var jFirstValue = TokenValue(token, CloneToken, currentKeyName, lstCalculation[i].TempFRActualFeedKeyPath);

                                                        if (isFromArray)
                                                            currentKeyName = lstCalculation[i].TempSCActualFeedKeyPath.Substring(lstCalculation[i].TempSCActualFeedKeyPath.LastIndexOf(']') + 1);
                                                        else
                                                            currentKeyName = lstCalculation[i].TempSCActualFeedKeyPath.Substring(lstCalculation[i].TempSCActualFeedKeyPath.LastIndexOf('.') + 1);

                                                        var jSecondValue = TokenValue(token, CloneToken, currentKeyName, lstCalculation[i].TempSCActualFeedKeyPath);

                                                        if (jFirstValue != null && jSecondValue != null)
                                                        {
                                                            if (((int)jFirstValue.Type == 6 && (int)jSecondValue.Type == 6) || ((int)jFirstValue.Type == 6 && (int)jSecondValue.Type == 7) || ((int)jFirstValue.Type == 7 && (int)jSecondValue.Type == 6))//Both Type int
                                                            {
                                                                if (isFromArray)
                                                                    currentKeyName = lstCalculation[i].ActualFeedKeyPath.Substring(lstCalculation[i].ActualFeedKeyPath.LastIndexOf(']') + 1);
                                                                else
                                                                    currentKeyName = lstCalculation[i].ActualFeedKeyPath.Substring(lstCalculation[i].ActualFeedKeyPath.LastIndexOf('.') + 1);

                                                                int oIntResult = (jFirstValue.Value<int>() + jSecondValue.Value<int>());

                                                                token.SelectTokens("$." + currentKeyName).FirstOrDefault().Replace(oIntResult);

                                                                oData = token;
                                                            }
                                                            else if (((int)jFirstValue.Type == 7 && (int)jSecondValue.Type == 7) || ((int)jFirstValue.Type == 7 && (int)jSecondValue.Type == 6) || ((int)jFirstValue.Type == 6 && (int)jSecondValue.Type == 7))//Both Type Float
                                                            {
                                                                if (isFromArray)
                                                                    currentKeyName = lstCalculation[i].ActualFeedKeyPath.Substring(lstCalculation[i].ActualFeedKeyPath.LastIndexOf(']') + 1);
                                                                else
                                                                    currentKeyName = lstCalculation[i].ActualFeedKeyPath.Substring(lstCalculation[i].ActualFeedKeyPath.LastIndexOf('.') + 1);

                                                                float oFloatResult = (jFirstValue.Value<float>() + jSecondValue.Value<float>());

                                                                token.SelectTokens("$." + currentKeyName).FirstOrDefault().Replace(oFloatResult);

                                                                oData = token;
                                                            }
                                                            else if ((int)jFirstValue.Type == 12 && (int)jSecondValue.Type == 12)//Both Type Date
                                                            {
                                                                if (isFromArray)
                                                                    currentKeyName = lstCalculation[i].ActualFeedKeyPath.Substring(lstCalculation[i].ActualFeedKeyPath.LastIndexOf(']') + 1);
                                                                else
                                                                    currentKeyName = lstCalculation[i].ActualFeedKeyPath.Substring(lstCalculation[i].ActualFeedKeyPath.LastIndexOf('.') + 1);

                                                                DateTime oDateTimeResult = (jFirstValue.Value<DateTime>().Add(Settings.ConvertToTimestamp(jSecondValue.Value<DateTime>())));

                                                                token.SelectTokens("$." + currentKeyName).FirstOrDefault().Replace(oDateTimeResult);

                                                                oData = token;
                                                            }
                                                            else if ((int)jFirstValue.Type == 17 && (int)jSecondValue.Type == 17)//Both Type TimeSpan
                                                            {
                                                                if (isFromArray)
                                                                    currentKeyName = lstCalculation[i].ActualFeedKeyPath.Substring(lstCalculation[i].ActualFeedKeyPath.LastIndexOf(']') + 1);
                                                                else
                                                                    currentKeyName = lstCalculation[i].ActualFeedKeyPath.Substring(lstCalculation[i].ActualFeedKeyPath.LastIndexOf('.') + 1);

                                                                TimeSpan oTimeSpanResult = (jFirstValue.Value<TimeSpan>().Add(jSecondValue.Value<TimeSpan>()));

                                                                token.SelectTokens("$." + currentKeyName).FirstOrDefault().Replace(oTimeSpanResult);

                                                                oData = token;
                                                            }
                                                        }
                                                        #endregion
                                                    }
                                                    else if (lstCalculation[i].Value == "1")//subtract
                                                    {
                                                        #region subtract
                                                        if (isFromArray)
                                                            currentKeyName = lstCalculation[i].TempFRActualFeedKeyPath.Substring(lstCalculation[i].TempFRActualFeedKeyPath.LastIndexOf(']') + 1);
                                                        else
                                                            currentKeyName = lstCalculation[i].TempFRActualFeedKeyPath.Substring(lstCalculation[i].TempFRActualFeedKeyPath.LastIndexOf('.') + 1);

                                                        var jFirstValue = TokenValue(token, CloneToken, currentKeyName, lstCalculation[i].TempFRActualFeedKeyPath);

                                                        if (isFromArray)
                                                            currentKeyName = lstCalculation[i].TempSCActualFeedKeyPath.Substring(lstCalculation[i].TempSCActualFeedKeyPath.LastIndexOf(']') + 1);
                                                        else
                                                            currentKeyName = lstCalculation[i].TempSCActualFeedKeyPath.Substring(lstCalculation[i].TempSCActualFeedKeyPath.LastIndexOf('.') + 1);

                                                        var jSecondValue = TokenValue(token, CloneToken, currentKeyName, lstCalculation[i].TempSCActualFeedKeyPath);


                                                        if (jFirstValue != null && jSecondValue != null)
                                                        {
                                                            if (((int)jFirstValue.Type == 6 && (int)jSecondValue.Type == 6) || ((int)jFirstValue.Type == 6 && (int)jSecondValue.Type == 7) || ((int)jFirstValue.Type == 7 && (int)jSecondValue.Type == 6))//Both Type int
                                                            {
                                                                if (isFromArray)
                                                                    currentKeyName = lstCalculation[i].ActualFeedKeyPath.Substring(lstCalculation[i].ActualFeedKeyPath.LastIndexOf(']') + 1);
                                                                else
                                                                    currentKeyName = lstCalculation[i].ActualFeedKeyPath.Substring(lstCalculation[i].ActualFeedKeyPath.LastIndexOf('.') + 1);

                                                                int oIntResult = (jFirstValue.Value<int>() - jSecondValue.Value<int>());

                                                                token.SelectTokens("$." + currentKeyName).FirstOrDefault().Replace(oIntResult);

                                                                oData = token;
                                                            }
                                                            else if (((int)jFirstValue.Type == 7 && (int)jSecondValue.Type == 7) || ((int)jFirstValue.Type == 7 && (int)jSecondValue.Type == 6) || ((int)jFirstValue.Type == 6 && (int)jSecondValue.Type == 7))//Both Type Float
                                                            {
                                                                if (isFromArray)
                                                                    currentKeyName = lstCalculation[i].ActualFeedKeyPath.Substring(lstCalculation[i].ActualFeedKeyPath.LastIndexOf(']') + 1);
                                                                else
                                                                    currentKeyName = lstCalculation[i].ActualFeedKeyPath.Substring(lstCalculation[i].ActualFeedKeyPath.LastIndexOf('.') + 1);

                                                                float oFloatResult = (jFirstValue.Value<float>() - jSecondValue.Value<float>());

                                                                token.SelectTokens("$." + currentKeyName).FirstOrDefault().Replace(oFloatResult);

                                                                oData = token;
                                                            }
                                                            else if ((int)jFirstValue.Type == 12 && (int)jSecondValue.Type == 12)//Both Type Date
                                                            {
                                                                if (isFromArray)
                                                                    currentKeyName = lstCalculation[i].ActualFeedKeyPath.Substring(lstCalculation[i].ActualFeedKeyPath.LastIndexOf(']') + 1);
                                                                else
                                                                    currentKeyName = lstCalculation[i].ActualFeedKeyPath.Substring(lstCalculation[i].ActualFeedKeyPath.LastIndexOf('.') + 1);


                                                                if (currentKeyName.Contains("duration"))
                                                                {                                                                   
                                                                    TimeSpan oTimeResult = (jFirstValue.Value<DateTime>().Subtract(jSecondValue.Value<DateTime>()));

                                                                    var Days = oTimeResult.Days;
                                                                    var Hours = oTimeResult.Hours;
                                                                    var Minutes = oTimeResult.Minutes;
                                                                    var Seconds = oTimeResult.Seconds;
                                                                    var Milliseconds = oTimeResult.Milliseconds;

                                                                    string Duration = "PT";

                                                                    Duration += (Days > 0 ? Days + "D" : "");
                                                                    Duration += (Hours > 0 ? Hours + "H" : "");
                                                                    Duration += (Minutes > 0 ? Minutes + "M" : "");
                                                                    Duration += (Seconds > 0 ? Seconds + "S" : "");
                                                                    Duration += (Milliseconds > 0 ? Milliseconds + "MS" : "");

                                                                    if (Duration != "PT")
                                                                    {
                                                                        token.SelectTokens("$." + currentKeyName).FirstOrDefault().Replace(Duration);
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    DateTime oDateTimeResult = (jFirstValue.Value<DateTime>().Add(-Settings.ConvertToTimestamp(jSecondValue.Value<DateTime>())));
                                                                    token.SelectTokens("$." + currentKeyName).FirstOrDefault().Replace(oDateTimeResult);
                                                                }
                                                                oData = token;
                                                            }
                                                            else if ((int)jFirstValue.Type == 17 && (int)jSecondValue.Type == 17)//Both Type TimeSpan
                                                            {
                                                                if (isFromArray)
                                                                    currentKeyName = lstCalculation[i].ActualFeedKeyPath.Substring(lstCalculation[i].ActualFeedKeyPath.LastIndexOf(']') + 1);
                                                                else
                                                                    currentKeyName = lstCalculation[i].ActualFeedKeyPath.Substring(lstCalculation[i].ActualFeedKeyPath.LastIndexOf('.') + 1);

                                                                TimeSpan oTimeSpanResult = (jFirstValue.Value<TimeSpan>().Add(-jSecondValue.Value<TimeSpan>()));

                                                                token.SelectTokens("$." + currentKeyName).FirstOrDefault().Replace(oTimeSpanResult);

                                                                oData = token;
                                                            }
                                                            else if ((int)jFirstValue.Type == 8 && (int)jSecondValue.Type == 8)
                                                            {
                                                                string jFirstValueinput = jFirstValue.ToString();
                                                                DateTime jFirstValuedateTime, jSecondValuedateTime;
                                                                string jSecondValueinput = jSecondValue.ToString();

                                                                if (isFromArray)
                                                                    currentKeyName = lstCalculation[i].ActualFeedKeyPath.Substring(lstCalculation[i].ActualFeedKeyPath.LastIndexOf(']') + 1);
                                                                else
                                                                    currentKeyName = lstCalculation[i].ActualFeedKeyPath.Substring(lstCalculation[i].ActualFeedKeyPath.LastIndexOf('.') + 1);

                                                                if (DateTime.TryParse(jFirstValueinput, out jFirstValuedateTime) && DateTime.TryParse(jSecondValueinput, out jSecondValuedateTime))
                                                                {
                                                                    if (currentKeyName.Contains("duration"))
                                                                    {                                                                        
                                                                        TimeSpan oTimeResult = (jFirstValuedateTime.Subtract(jSecondValuedateTime));

                                                                        var Days = oTimeResult.Days;
                                                                        var Hours = oTimeResult.Hours;
                                                                        var Minutes = oTimeResult.Minutes;
                                                                        var Seconds = oTimeResult.Seconds;
                                                                        var Milliseconds = oTimeResult.Milliseconds;

                                                                        string Duration = "PT";

                                                                        Duration += (Days > 0 ? Days + "D" : "");
                                                                        Duration += (Hours > 0 ? Hours + "H" : "");
                                                                        Duration += (Minutes > 0 ? Minutes + "M" : "");
                                                                        Duration += (Seconds > 0 ? Seconds + "S" : "");
                                                                        Duration += (Milliseconds > 0 ? Milliseconds + "MS" : "");

                                                                        if (Duration != "PT")
                                                                        {
                                                                            token.SelectTokens("$." + currentKeyName).FirstOrDefault().Replace(Duration);
                                                                        }
                                                                    }
                                                                    oData = token;
                                                                }
                                                            }
                                                        }
                                                        #endregion
                                                    }
                                                    else if (lstCalculation[i].Value == "2")//Multi
                                                    {
                                                        #region division
                                                        if (isFromArray)
                                                            currentKeyName = lstCalculation[i].TempFRActualFeedKeyPath.Substring(lstCalculation[i].TempFRActualFeedKeyPath.LastIndexOf(']') + 1);
                                                        else
                                                            currentKeyName = lstCalculation[i].TempFRActualFeedKeyPath.Substring(lstCalculation[i].TempFRActualFeedKeyPath.LastIndexOf('.') + 1);

                                                        var jFirstValue = TokenValue(token, CloneToken, currentKeyName, lstCalculation[i].TempFRActualFeedKeyPath);

                                                        if (isFromArray)
                                                            currentKeyName = lstCalculation[i].TempSCActualFeedKeyPath.Substring(lstCalculation[i].TempSCActualFeedKeyPath.LastIndexOf(']') + 1);
                                                        else
                                                            currentKeyName = lstCalculation[i].TempSCActualFeedKeyPath.Substring(lstCalculation[i].TempSCActualFeedKeyPath.LastIndexOf('.') + 1);

                                                        var jSecondValue = TokenValue(token, CloneToken, currentKeyName, lstCalculation[i].TempSCActualFeedKeyPath);


                                                        if (jFirstValue != null && jSecondValue != null)
                                                        {
                                                            if (((int)jFirstValue.Type == 6 && (int)jSecondValue.Type == 6) || ((int)jFirstValue.Type == 6 && (int)jSecondValue.Type == 7) || ((int)jFirstValue.Type == 7 && (int)jSecondValue.Type == 6))//Both Type int
                                                            {
                                                                if (isFromArray)
                                                                    currentKeyName = lstCalculation[i].ActualFeedKeyPath.Substring(lstCalculation[i].ActualFeedKeyPath.LastIndexOf(']') + 1);
                                                                else
                                                                    currentKeyName = lstCalculation[i].ActualFeedKeyPath.Substring(lstCalculation[i].ActualFeedKeyPath.LastIndexOf('.') + 1);

                                                                int oIntResult = (jFirstValue.Value<int>() * jSecondValue.Value<int>());

                                                                token.SelectTokens("$." + currentKeyName).FirstOrDefault().Replace(oIntResult);

                                                                oData = token;
                                                            }
                                                            else if (((int)jFirstValue.Type == 7 && (int)jSecondValue.Type == 7) || ((int)jFirstValue.Type == 7 && (int)jSecondValue.Type == 6) || ((int)jFirstValue.Type == 6 && (int)jSecondValue.Type == 7))//Both Type Float
                                                            {
                                                                if (isFromArray)
                                                                    currentKeyName = lstCalculation[i].ActualFeedKeyPath.Substring(lstCalculation[i].ActualFeedKeyPath.LastIndexOf(']') + 1);
                                                                else
                                                                    currentKeyName = lstCalculation[i].ActualFeedKeyPath.Substring(lstCalculation[i].ActualFeedKeyPath.LastIndexOf('.') + 1);

                                                                float oFloatResult = (jFirstValue.Value<float>() * jSecondValue.Value<float>());

                                                                token.SelectTokens("$." + currentKeyName).FirstOrDefault().Replace(oFloatResult);

                                                                oData = token;
                                                            }
                                                        }
                                                        #endregion
                                                    }
                                                    else if (lstCalculation[i].Value == "3")//division
                                                    {
                                                        #region division
                                                        if (isFromArray)
                                                            currentKeyName = lstCalculation[i].TempFRActualFeedKeyPath.Substring(lstCalculation[i].TempFRActualFeedKeyPath.LastIndexOf(']') + 1);
                                                        else
                                                            currentKeyName = lstCalculation[i].TempFRActualFeedKeyPath.Substring(lstCalculation[i].TempFRActualFeedKeyPath.LastIndexOf('.') + 1);

                                                        var jFirstValue = TokenValue(token, CloneToken, currentKeyName, lstCalculation[i].TempFRActualFeedKeyPath);

                                                        if (isFromArray)
                                                            currentKeyName = lstCalculation[i].TempSCActualFeedKeyPath.Substring(lstCalculation[i].TempSCActualFeedKeyPath.LastIndexOf(']') + 1);
                                                        else
                                                            currentKeyName = lstCalculation[i].TempSCActualFeedKeyPath.Substring(lstCalculation[i].TempSCActualFeedKeyPath.LastIndexOf('.') + 1);

                                                        var jSecondValue = TokenValue(token, CloneToken, currentKeyName, lstCalculation[i].TempSCActualFeedKeyPath);

                                                        if (jFirstValue != null && jSecondValue != null)
                                                        {
                                                            if (((int)jFirstValue.Type == 6 && (int)jSecondValue.Type == 6) || ((int)jFirstValue.Type == 6 && (int)jSecondValue.Type == 7) || ((int)jFirstValue.Type == 7 && (int)jSecondValue.Type == 6))//Both Type int
                                                            {
                                                                if (isFromArray)
                                                                    currentKeyName = lstCalculation[i].ActualFeedKeyPath.Substring(lstCalculation[i].ActualFeedKeyPath.LastIndexOf(']') + 1);
                                                                else
                                                                    currentKeyName = lstCalculation[i].ActualFeedKeyPath.Substring(lstCalculation[i].ActualFeedKeyPath.LastIndexOf('.') + 1);

                                                                int oIntResult = (jFirstValue.Value<int>() / jSecondValue.Value<int>());

                                                                token.SelectTokens("$." + currentKeyName).FirstOrDefault().Replace(oIntResult);

                                                                oData = token;
                                                            }
                                                            else if (((int)jFirstValue.Type == 7 && (int)jSecondValue.Type == 7) || ((int)jFirstValue.Type == 7 && (int)jSecondValue.Type == 6) || ((int)jFirstValue.Type == 6 && (int)jSecondValue.Type == 7))//Both Type Float
                                                            {
                                                                if (isFromArray)
                                                                    currentKeyName = lstCalculation[i].ActualFeedKeyPath.Substring(lstCalculation[i].ActualFeedKeyPath.LastIndexOf(']') + 1);
                                                                else
                                                                    currentKeyName = lstCalculation[i].ActualFeedKeyPath.Substring(lstCalculation[i].ActualFeedKeyPath.LastIndexOf('.') + 1);

                                                                float oFloatResult = (jFirstValue.Value<float>() / jSecondValue.Value<float>());

                                                                token.SelectTokens("$." + currentKeyName).FirstOrDefault().Replace(oFloatResult);

                                                                oData = token;
                                                            }
                                                        }
                                                        #endregion
                                                    }
                                                    else if (lstCalculation[i].Value == "4")//concat
                                                    {
                                                        /*First Iteam */
                                                        #region concat
                                                        if (isFromArray)
                                                            currentKeyName = lstCalculation[i].TempFRActualFeedKeyPath.Substring(lstCalculation[i].TempFRActualFeedKeyPath.LastIndexOf(']') + 1);
                                                        else
                                                            currentKeyName = lstCalculation[i].TempFRActualFeedKeyPath.Substring(lstCalculation[i].TempFRActualFeedKeyPath.LastIndexOf('.') + 1);

                                                        var jFirstValue = TokenValue(token, CloneToken, currentKeyName, lstCalculation[i].TempFRActualFeedKeyPath);

                                                        if (isFromArray)
                                                            currentKeyName = lstCalculation[i].TempSCActualFeedKeyPath.Substring(lstCalculation[i].TempSCActualFeedKeyPath.LastIndexOf(']') + 1);
                                                        else
                                                            currentKeyName = lstCalculation[i].TempSCActualFeedKeyPath.Substring(lstCalculation[i].TempSCActualFeedKeyPath.LastIndexOf('.') + 1);
                                                       
                                                        var jSecondValue = TokenValue(token, CloneToken, currentKeyName, lstCalculation[i].TempSCActualFeedKeyPath);

                                                        if (isFromArray)
                                                            currentKeyName = lstCalculation[i].ActualFeedKeyPath.Substring(lstCalculation[i].ActualFeedKeyPath.LastIndexOf(']') + 1);
                                                        else
                                                            currentKeyName = lstCalculation[i].ActualFeedKeyPath.Substring(lstCalculation[i].ActualFeedKeyPath.LastIndexOf('.') + 1);

                                                        if (jFirstValue != null && jSecondValue != null)
                                                        {
                                                            token.SelectTokens("$." + currentKeyName).FirstOrDefault().Replace(string.Concat(jFirstValue, jSecondValue));

                                                            oData = token;
                                                        }
                                                        #endregion
                                                    }
                                                }
                                            }                                            
                                        }
                                        #endregion
                                        break;

                                    case 6: // Remove Session
                                        if (!lstValidData.Any(x => x.IsValid == false))
                                        {
                                            IsRemoveSession = true;
                                        }
                                        break;
                                }
                            }
                        }
                    }
                }
                #endregion

            }
            catch (Exception ex)
            {
                LogHelper.InsertErrorLogs("[DataLaundryScheduler] SchedulerHelper", "FilterData", ex.Message, ex.InnerException?.Message, ex.StackTrace, feedMapping.FeedProvider.Id);
            }
            return oData;
        }

        public static JToken TokenValue(JToken CurrentToken, JToken RootToken, string CurrentPath, string ActualPath)
        {
            JToken oToken = null;
            if (CurrentToken.SelectTokens("$." + CurrentPath).Count() > 0)
            {
                if (CurrentPath.Contains("endDate"))
                    oToken = CurrentToken.SelectTokens("$." + CurrentPath).LastOrDefault();
                else
                    oToken = CurrentToken.SelectTokens("$." + CurrentPath).FirstOrDefault();
            }
            else
            {
                oToken = CurrentToken.SelectTokens("$." + CurrentPath).FirstOrDefault();
            }
            if (oToken != null)
            {
                return oToken;
            }
            else
            {
                ActualPath = ActualPath.Substring(ActualPath.LastIndexOf(']') + 1);

                if (RootToken.SelectTokens("$." + ActualPath).Count() > 0)
                {
                    if (ActualPath.Contains("endDate"))
                        oToken = RootToken.SelectTokens("$." + ActualPath).LastOrDefault();
                    else
                        oToken = RootToken.SelectTokens("$." + ActualPath).FirstOrDefault();
                }
                else
                {
                    oToken = RootToken.SelectTokens("$." + ActualPath).FirstOrDefault();
                }
            }
            return oToken;
        }

        #region Rule-Filter-Criteria applied added date 21-02-2019
        public static void AutoRuleFilterData(int Id)
        {
            #region FilterUpdateRule                                                
            try
            {
                #region If json key not match and rule/filter applied (added date 09-02-2019)
                var oQueryBuilder = new StringBuilder();
                bool IsEventDelete = false;
                string currentKeyName = "", currentdatatype = "", currentcolumnname = "", currenttable = "";
                int oCunter = 0;
                var oFilterModel = FilterRuleHelper.GetFilterCriteriaByFeedMappingId(Id);
                if (oFilterModel != null && oFilterModel.FilterCriteria.Count > 0)
                {
                    #region Apply Rule and Filter
                    var oGroupeFilterCriteria = oFilterModel.FilterCriteria.GroupBy(x => x.RuleId).ToList();

                    var oGroupeOperation = oFilterModel.OperationData.Where(x => oGroupeFilterCriteria.Any(z => z.Key == x.RuleId)).GroupBy(x => x.RuleId).ToList();
                    foreach (var lstGroupFilterCriteria in oGroupeFilterCriteria)
                    {
                        oQueryBuilder = new StringBuilder();
                        #region Filter 
                        foreach (var oFilter in lstGroupFilterCriteria)
                        {
                            currentKeyName = ""; currentdatatype = ""; currentcolumnname = ""; currenttable = "";
                            currentdatatype = oFilter.ColumnDataType;
                            currentcolumnname = oFilter.ColumnName;
                            currenttable = oFilter.TableName;
                            if (!string.IsNullOrEmpty(currenttable))
                            {
                                #region Select_V1
                                if (oCunter == 0)
                                {
                                    IsEventDelete = (currenttable == "Event" ? true : false);
                                    var lstTableSelecte = lstGroupFilterCriteria.Select(x => x.TableName).ToList();
                                    oQueryBuilder.Append("SELECT " + currenttable + ".");

                                    if (currenttable == "Event")
                                        oQueryBuilder.Append("Id ");
                                    else
                                    {
                                        oQueryBuilder.Append("Id ");
                                        oQueryBuilder.Append(",");
                                        oQueryBuilder.Append(currenttable);
                                        oQueryBuilder.Append(".");
                                        oQueryBuilder.Append("EventId");
                                    }

                                    for (int i = 0; i < lstTableSelecte.Count(); i++)
                                    {
                                        if (lstTableSelecte[i].ToString() != currenttable)
                                        {
                                            oQueryBuilder.Append(" ,");
                                            oQueryBuilder.Append(lstTableSelecte[i]);
                                            oQueryBuilder.Append(".");
                                            if (lstTableSelecte[i] == "Event")
                                                oQueryBuilder.Append("Id ");
                                            else
                                            {
                                                oQueryBuilder.Append("Id ");
                                                oQueryBuilder.Append(",");
                                                oQueryBuilder.Append(lstTableSelecte[i]);
                                                oQueryBuilder.Append(".");
                                                oQueryBuilder.Append("EventId ");
                                            }

                                        }
                                    }
                                    oQueryBuilder.Append(" FROM ");
                                    oQueryBuilder.Append(currenttable);
                                    oQueryBuilder.Append(" WITH (NOLOCK) ");
                                    var oFirstTable = currenttable;

                                    for (int i = 0; i < lstTableSelecte.Count(); i++)
                                    {
                                        if (lstTableSelecte[i].ToString() != currenttable)
                                        {
                                            oQueryBuilder.Append(" LEFT JOIN ");
                                            oQueryBuilder.Append(lstTableSelecte[i]);
                                            oQueryBuilder.Append(" ");
                                            oQueryBuilder.Append(lstTableSelecte[i]);
                                            oQueryBuilder.Append(" WITH (NOLOCK) ");
                                            oQueryBuilder.Append(" ON ");
                                            oQueryBuilder.Append(lstTableSelecte[i]);
                                            oQueryBuilder.Append((lstTableSelecte[i] == "Event" ? ".Id = " : ".EventId = "));
                                            oQueryBuilder.Append(oFirstTable);
                                            oQueryBuilder.Append((oFirstTable == "Event" ? ".Id" : ".EventId"));
                                        }
                                    }


                                    oQueryBuilder.Append(" WHERE");
                                    if (lstTableSelecte.Any(x => x == "Event"))//Set FeedProvider id for particular feed data
                                    {  
                                        oQueryBuilder.Append(" FeedProviderId=" + oFilter.FeedProviderId + " AND FeedId IS NOT NULL AND ");
                                    }
                                }
                                #endregion

                                oQueryBuilder.Append(" ");
                                var oColumnName = string.Empty;
                                #region Skip for Event /Place / Physical Activity
                                oColumnName = TableField(currentcolumnname);
                                #endregion

                                if (!string.IsNullOrEmpty(oColumnName))
                                {
                                    oQueryBuilder.Append(string.Concat(currenttable, ".", oColumnName));
                                    #region All Operator cheacker
                                    switch (oFilter.OperatorId)
                                    {
                                        case 1://Is less than(Date,Integer,Decimal) <
                                            oQueryBuilder.Append(" < ");
                                            oQueryBuilder.Append("'" + oFilter.Value + "'");
                                            break;
                                        case 2://Is higher than(Date,Integer,Decimal) >
                                            oQueryBuilder.Append(" > ");
                                            oQueryBuilder.Append("'" + oFilter.Value + "'");
                                            break;
                                        case 3://Is equals (Date,Integer,Decimal,String) ==
                                            oQueryBuilder.Append(" = ");
                                            oQueryBuilder.Append("'" + oFilter.Value + "'");
                                            break;
                                        case 4://Is not equal (Date,Integer,Decimal,String) !=
                                            oQueryBuilder.Append(" != ");
                                            oQueryBuilder.Append("'" + oFilter.Value + "'");
                                            break;
                                        case 5://Starts with (String)
                                            oQueryBuilder.Append(" LIKE ");
                                            oQueryBuilder.Append("'" + oFilter.Value + "%'");
                                            break;
                                        case 6://Contains (String)
                                            oQueryBuilder.Append(" LIKE ");
                                            oQueryBuilder.Append("'%" + oFilter.Value + "%'");
                                            break;
                                        case 7://End with (String)
                                            oQueryBuilder.Append(" LIKE ");
                                            oQueryBuilder.Append("'%" + oFilter.Value + "'");
                                            break;
                                        case 8:// IS NULL
                                            oQueryBuilder.Append(" IS NULL ");
                                            break;
                                        case 9: // IS NOT NULL
                                            oQueryBuilder.Append(" IS NOT NULL ");
                                            break;
                                        default:
                                            break;
                                    }
                                    if (lstGroupFilterCriteria.Count() > 1)
                                    {
                                        if (oFilter.RuleOperatorId > 0)
                                        {
                                            oQueryBuilder.Append((oFilter.RuleOperatorId == 1 ? " AND " : ""));
                                            oQueryBuilder.Append((oFilter.RuleOperatorId == 2 ? " OR " : ""));
                                            oQueryBuilder.Append(" ");
                                        }
                                    }
                                    #endregion
                                }
                                oCunter++;
                            }
                        }
                        #endregion
                        oCunter = 0;
                        #region Operation
                        foreach (var lstGroupeOperation in oGroupeOperation.Where(x => x.Key == lstGroupFilterCriteria.Key).ToList())
                        {
                            foreach (var oOperation in lstGroupeOperation)
                            {
                                if (!string.IsNullOrEmpty(oOperation.TableName) || oOperation.OperationTypeId == 6)
                                {
                                    string PrimaryIDBasedOnTable = "", PrimaryIDValueBasedOnTable = "";
                                    PrimaryIDBasedOnTable = (oOperation.TableName == "Event" ? "Id" : "EventId");
                                    
                                    var oIndex = lstGroupFilterCriteria.Select(x => x.TableName).Distinct().ToList().FindIndex(x => x == "Event");
                                    if (lstGroupFilterCriteria.Count() > 1)
                                    {
                                        if (lstGroupFilterCriteria.Any(x => x.TableName == "Event"))
                                            PrimaryIDValueBasedOnTable = "Id" + (oIndex > 0 ? "" + oIndex : "");
                                        else
                                            PrimaryIDValueBasedOnTable = "EventId";
                                    }
                                    else
                                    {
                                        var oTableName = lstGroupFilterCriteria.FirstOrDefault().TableName;
                                        if (oTableName == "Event")
                                            PrimaryIDValueBasedOnTable = "Id";
                                        else
                                            PrimaryIDValueBasedOnTable = "EventId";
                                    }
                                    switch (oOperation.OperationTypeId)
                                    {
                                        case 1://Value Assignment
                                            #region Value Assignment

                                            var oQuery = oQueryBuilder.ToString();
                                            var lst = new List<SqlParameter>();
                                            var oAutoData = DBProvider.GetDataSet(oQuery, CommandType.Text, ref lst);
                                            if (oAutoData != null && oAutoData.Tables.Count > 0)
                                            {
                                                foreach (DataRow oRow in oAutoData.Tables[0].Rows)
                                                {
                                                    var oColumnName = string.Empty;
                                                    currentcolumnname = oOperation.ColumnName;
                                                    #region Skip for Event /Place / Physical Activity                                                    
                                                    oColumnName = TableField(currentcolumnname);
                                                    #endregion       
                                                    if (oOperation.TableName == "Event" && (currentcolumnname.StartsWith("superEvent_") || currentcolumnname.StartsWith("subEvent_")))
                                                    {
                                                        var oInsertUpdateQuery = PreparedQuery(oOperation.TableName, oColumnName, oOperation.Value, PrimaryIDBasedOnTable, oRow[PrimaryIDValueBasedOnTable].ToString(), true);
                                                        DBProvider.ExecuteNonQuery(oInsertUpdateQuery, CommandType.Text);
                                                    }
                                                    else
                                                    {
                                                        var oInsertUpdateQuery = PreparedQuery(oOperation.TableName, oColumnName, oOperation.Value, PrimaryIDBasedOnTable, oRow[PrimaryIDValueBasedOnTable].ToString());
                                                        DBProvider.ExecuteNonQuery(oInsertUpdateQuery, CommandType.Text);
                                                    }
                                                }
                                            }
                                            #endregion
                                            break;
                                        case 2://Field Assignment
                                            #region Field Assignment
                                            var oFieldQuery = oQueryBuilder.ToString();
                                            var lstField = new List<SqlParameter>();
                                            var oFieldAutoData = DBProvider.GetDataSet(oFieldQuery, CommandType.Text, ref lstField);
                                            if (oFieldAutoData != null && oFieldAutoData.Tables.Count > 0)
                                            {
                                                foreach (DataRow oRow in oFieldAutoData.Tables[0].Rows)
                                                {
                                                    var oColumnName = string.Empty;
                                                    currentcolumnname = oOperation.TempColumnName;
                                                    #region Skip for Event /Place / Physical Activity                                                    
                                                    oColumnName = TableField(currentcolumnname);
                                                    #endregion

                                                    var oPrimaryID = (oOperation.TempTableName == "Event" ? "Id" : "EventId");
                                                    var oSelectQuery = "SELECT " + oOperation.TempTableName;
                                                    oSelectQuery += "." + oColumnName;
                                                    oSelectQuery += " FROM " + oOperation.TempTableName + " " + oOperation.TempTableName;
                                                    oSelectQuery += " WITH(NOLOCK) WHERE " + oOperation.TempTableName + "." + oPrimaryID + " = " + oRow[PrimaryIDValueBasedOnTable];

                                                    var oResult = DBProvider.ExecuteScalar(oSelectQuery, CommandType.Text);
                                                    if (oResult != null)
                                                    {
                                                        oColumnName = string.Empty;
                                                        currentcolumnname = oOperation.ColumnName;

                                                        #region Skip for Event /Place / Physical Activity
                                                        oColumnName = TableField(currentcolumnname);
                                                        #endregion
                                                        if (oOperation.TableName == "Event" && (currentcolumnname.StartsWith("superEvent_") || currentcolumnname.StartsWith("subEvent_")))
                                                        {
                                                            var oInsertUpdateQuery = PreparedQuery(oOperation.TableName, oColumnName, oResult, PrimaryIDBasedOnTable, oRow[PrimaryIDValueBasedOnTable].ToString(), true);
                                                            DBProvider.ExecuteNonQuery(oInsertUpdateQuery, CommandType.Text);
                                                        }
                                                        else
                                                        {
                                                            var oInsertUpdateQuery = PreparedQuery(oOperation.TableName, oColumnName, oResult, PrimaryIDBasedOnTable, oRow[PrimaryIDValueBasedOnTable].ToString());
                                                            DBProvider.ExecuteNonQuery(oInsertUpdateQuery, CommandType.Text);
                                                        }
                                                    }
                                                }
                                            }
                                            #endregion
                                            break;
                                        case 3://Keyword/Sentence Replacement
                                            #region Keyword/Sentence Replacement
                                            var oKeywordQuery = oQueryBuilder.ToString();
                                            var lstKeyword = new List<SqlParameter>();
                                            var oKeywordAutoData = DBProvider.GetDataSet(oKeywordQuery, CommandType.Text, ref lstKeyword);
                                            if (oKeywordAutoData != null && oKeywordAutoData.Tables.Count > 0)
                                            {
                                                foreach (DataRow oRow in oKeywordAutoData.Tables[0].Rows)
                                                {
                                                    var oColumnName = string.Empty;
                                                    currentcolumnname = oOperation.ColumnName;
                                                    #region Skip for Event /Place / Physical Activity                                                    
                                                    oColumnName = TableField(currentcolumnname);
                                                    #endregion

                                                    var oPrimaryID = (oOperation.TableName == "Event" ? "Id" : "EventId");
                                                    var oSelectQuery = "SELECT " + oOperation.TableName;
                                                    oSelectQuery += "." + oColumnName;
                                                    oSelectQuery += " FROM " + oOperation.TableName + " " + oOperation.TableName;
                                                    oSelectQuery += " WITH(NOLOCK) WHERE " + oOperation.TableName + "." + oPrimaryID + " = " + oRow[PrimaryIDValueBasedOnTable];

                                                    var oResult = DBProvider.ExecuteScalar(oSelectQuery, CommandType.Text);

                                                    if (oResult != null)
                                                    {
                                                        oResult = Convert.ToString(oResult).Replace(oOperation.CurrentWord, oOperation.NewWord);
                                                        if (oOperation.TableName == "Event" && (currentcolumnname.StartsWith("superEvent_") || currentcolumnname.StartsWith("subEvent_")))
                                                        {
                                                            var oInsertUpdateQuery = PreparedQuery(oOperation.TableName, oColumnName, oResult, PrimaryIDBasedOnTable, oRow[PrimaryIDValueBasedOnTable].ToString(), true);
                                                            DBProvider.ExecuteNonQuery(oInsertUpdateQuery, CommandType.Text);
                                                        }
                                                        else
                                                        {
                                                            var oInsertUpdateQuery = PreparedQuery(oOperation.TableName, oColumnName, oResult, PrimaryIDBasedOnTable, oRow[PrimaryIDValueBasedOnTable].ToString());
                                                            DBProvider.ExecuteNonQuery(oInsertUpdateQuery, CommandType.Text);
                                                        }
                                                    }
                                                }
                                            }
                                            #endregion
                                            break;
                                        case 4://Remove Sentence
                                            #region Remove Sentence
                                            var oRemoveQuery = oQueryBuilder.ToString();
                                            var lstRemove = new List<SqlParameter>();
                                            var oRemoveAutoData = DBProvider.GetDataSet(oRemoveQuery, CommandType.Text, ref lstRemove);
                                            if (oRemoveAutoData != null && oRemoveAutoData.Tables.Count > 0)
                                            {
                                                foreach (DataRow oRow in oRemoveAutoData.Tables[0].Rows)
                                                {
                                                    var oColumnName = string.Empty;
                                                    currentcolumnname = oOperation.ColumnName;
                                                    #region Skip for Event /Place / Physical Activity                                                   
                                                    oColumnName = TableField(currentcolumnname);
                                                    #endregion

                                                    var oPrimaryID = (oOperation.TableName == "Event" ? "Id" : "EventId");
                                                    var oSelectQuery = "SELECT " + oOperation.TableName;
                                                    oSelectQuery += "." + oColumnName;
                                                    oSelectQuery += " FROM " + oOperation.TableName + " " + oOperation.TableName;
                                                    oSelectQuery += " WITH (NOLOCK) WHERE " + oOperation.TableName + "." + oPrimaryID + " = " + oRow[PrimaryIDValueBasedOnTable];

                                                    var oResult = DBProvider.ExecuteScalar(oSelectQuery, CommandType.Text);

                                                    if (oResult != null)
                                                    {
                                                        oResult = Convert.ToString(oResult).Replace(oOperation.Sentance, "");
                                                        if (oOperation.TableName == "Event" && (currentcolumnname.StartsWith("superEvent_") || currentcolumnname.StartsWith("subEvent_")))
                                                        {
                                                            var oInsertUpdateQuery = PreparedQuery(oOperation.TableName, oColumnName, oResult, PrimaryIDBasedOnTable, oRow[PrimaryIDValueBasedOnTable].ToString(), true);
                                                            DBProvider.ExecuteNonQuery(oInsertUpdateQuery, CommandType.Text);
                                                        }
                                                        else
                                                        {
                                                            var oInsertUpdateQuery = PreparedQuery(oOperation.TableName, oColumnName, oResult, PrimaryIDBasedOnTable, oRow[PrimaryIDValueBasedOnTable].ToString());
                                                            DBProvider.ExecuteNonQuery(oInsertUpdateQuery, CommandType.Text);
                                                        }
                                                    }
                                                }
                                            }
                                            #endregion
                                            break;
                                        case 5://Calculation
                                            #region Calculation
                                            var oCalculationQuery = oQueryBuilder.ToString();
                                            var lstCalculation = new List<SqlParameter>();
                                            var oCalculationAutoData = DBProvider.GetDataSet(oCalculationQuery, CommandType.Text, ref lstCalculation);
                                            if (oCalculationAutoData != null && oCalculationAutoData.Tables.Count > 0 && oCalculationAutoData.Tables[0].Rows.Count > 0)
                                            {
                                                if (oOperation.Value == "0") //additional (int,float,decimal,datetime,timespan)
                                                {
                                                    #region additional
                                                    //if key has filed
                                                    var oCurrentKey = oOperation.ColumnName.Split('_').LastOrDefault();
                                                    if (!string.IsNullOrEmpty(oCurrentKey))
                                                    {
                                                        foreach (DataRow oRow in oCalculationAutoData.Tables[0].Rows)
                                                        {
                                                            var oColumnName = string.Empty;
                                                            currentcolumnname = oOperation.TempFRColumnName;
                                                            #region Skip for Event /Place / Physical Activity                                                            
                                                            oColumnName = TableField(currentcolumnname);
                                                            #endregion

                                                            object oValues = null;
                                                            var oFirstPrimaryID = (oOperation.TempFRTableName == "Event" ? "Id" : "EventId");
                                                            var oFirstSelectQuery = "SELECT " + oOperation.TempFRTableName;
                                                            oFirstSelectQuery += "." + oColumnName;
                                                            oFirstSelectQuery += " FROM " + oOperation.TempFRTableName + " " + oOperation.TempFRTableName;
                                                            oFirstSelectQuery += " WITH (NOLOCK) WHERE " + oOperation.TempFRTableName + "." + oFirstPrimaryID + " = " + oRow[PrimaryIDValueBasedOnTable];


                                                            oColumnName = string.Empty;
                                                            currentcolumnname = oOperation.TempSCColumnName;
                                                            #region Skip for Event /Place / Physical Activity                                                            
                                                            oColumnName = TableField(currentcolumnname);
                                                            #endregion

                                                            var oSecondPrimaryID = (oOperation.TempSCTableName == "Event" ? "Id" : "EventId");
                                                            var oSecondSelectQuery = "SELECT " + oOperation.TempSCTableName;
                                                            oSecondSelectQuery += "." + oColumnName;
                                                            oSecondSelectQuery += " FROM " + oOperation.TempSCTableName + " " + oOperation.TempSCTableName;
                                                            oSecondSelectQuery += " WITH (NOLOCK) WHERE " + oOperation.TempSCTableName + "." + oSecondPrimaryID + " = " + oRow[PrimaryIDValueBasedOnTable];

                                                            var oFirstResult = DBProvider.ExecuteScalar(oFirstSelectQuery, CommandType.Text);

                                                            var oSecondResult = DBProvider.ExecuteScalar(oSecondSelectQuery, CommandType.Text);

                                                            if (oFirstResult != null)
                                                            {
                                                                //float,date,integer
                                                                switch (oOperation.TempFRColumnDataType)
                                                                {
                                                                    case "float":
                                                                        oValues = oFirstResult;
                                                                        break;
                                                                    case "date":
                                                                        oValues = oFirstResult;
                                                                        break;
                                                                    case "integer":
                                                                        oValues = oFirstResult;
                                                                        break;
                                                                    case "string":
                                                                        oValues = oFirstResult;
                                                                        break;
                                                                    default:
                                                                        break;
                                                                }
                                                            }

                                                            if (oSecondResult != null)
                                                            {
                                                                var oOperationValues = oValues;
                                                                //float,date,integer                                                        
                                                                switch (oOperation.TempSCColumnDataType)
                                                                {
                                                                    case "float":
                                                                        oOperationValues = (float.Parse(Convert.ToString(oValues)) + float.Parse(Convert.ToString(oSecondResult)));
                                                                        break;
                                                                    case "date":
                                                                        oOperationValues = Convert.ToDateTime(oValues).Add(Settings.ConvertToTimestamp(Convert.ToDateTime(oSecondResult)));
                                                                        break;
                                                                    case "integer":
                                                                        oOperationValues = (float.Parse(Convert.ToString(oValues)) + float.Parse(Convert.ToString(oSecondResult)));
                                                                        break;
                                                                    case "string":
                                                                        if (oColumnName.Contains("duration"))
                                                                        {
                                                                            var oTimeStamp = System.Xml.XmlConvert.ToTimeSpan(Convert.ToString(oSecondResult));
                                                                            var oStartDate = DateTime.Now;
                                                                            if (DateTime.TryParse(Convert.ToString(oFirstResult), out oStartDate))
                                                                                oOperationValues = oStartDate.AddMilliseconds(oTimeStamp.TotalMilliseconds);
                                                                        }
                                                                        else
                                                                        {
                                                                            var oStartDate = DateTime.Now;
                                                                            if (DateTime.TryParse(Convert.ToString(oSecondResult), out oStartDate))
                                                                            {
                                                                                var oTimeStamp = System.Xml.XmlConvert.ToTimeSpan(Convert.ToString(oOperationValues));
                                                                                oOperationValues = oStartDate.AddMilliseconds(oTimeStamp.TotalMilliseconds);
                                                                            }
                                                                        }
                                                                        break;
                                                                    default:
                                                                        break;
                                                                }
                                                                oValues = oOperationValues;
                                                            }
                                                            oColumnName = string.Empty;
                                                            currentcolumnname = oOperation.ColumnName;
                                                            #region Skip for Event /Place / Physical Activity                                                            
                                                            oColumnName = TableField(currentcolumnname);
                                                            #endregion
                                                            if (oOperation.TableName == "Event" && (currentcolumnname.StartsWith("superEvent_") || currentcolumnname.StartsWith("subEvent_")))
                                                            {
                                                                var oInsertUpdateQuery = PreparedQuery(oOperation.TableName, oColumnName, oValues, PrimaryIDBasedOnTable, oRow[PrimaryIDValueBasedOnTable].ToString(), true);
                                                                DBProvider.ExecuteNonQuery(oInsertUpdateQuery, CommandType.Text);
                                                            }
                                                            else
                                                            {
                                                                var oInsertUpdateQuery = PreparedQuery(oOperation.TableName, oColumnName, oValues, PrimaryIDBasedOnTable, oRow[PrimaryIDValueBasedOnTable].ToString());
                                                                DBProvider.ExecuteNonQuery(oInsertUpdateQuery, CommandType.Text);
                                                            }
                                                        }
                                                    }
                                                    #endregion
                                                }
                                                else if (oOperation.Value == "1")//subtract (int,float,decimal,datetime,timespan)
                                                {
                                                    #region subtract
                                                    //if key has filed
                                                    var oCurrentKey = oOperation.ColumnName.Split('_').LastOrDefault();
                                                    if (!string.IsNullOrEmpty(oCurrentKey))
                                                    {
                                                        if (oCurrentKey.Contains("duration"))
                                                        {
                                                            TimeSpan oTimeResult = TimeSpan.FromMilliseconds(1);
                                                            #region for-duration
                                                            foreach (DataRow oRow in oCalculationAutoData.Tables[0].Rows)
                                                            {
                                                                var oColumnName = string.Empty;
                                                                currentcolumnname = oOperation.TempFRColumnName;
                                                                #region Skip for Event /Place / Physical Activity                                                                
                                                                oColumnName = TableField(currentcolumnname);
                                                                #endregion

                                                                object oValues = null;
                                                                var oFirstPrimaryID = (oOperation.TempFRTableName == "Event" ? "Id" : "EventId");
                                                                var oFirstSelectQuery = "SELECT " + oOperation.TempFRTableName;
                                                                oFirstSelectQuery += "." + oColumnName;
                                                                oFirstSelectQuery += " FROM " + oOperation.TempFRTableName + " " + oOperation.TempFRTableName;
                                                                oFirstSelectQuery += " WITH (NOLOCK) WHERE " + oOperation.TempFRTableName + "." + oFirstPrimaryID + " = " + oRow[PrimaryIDValueBasedOnTable];

                                                                oColumnName = string.Empty;
                                                                currentcolumnname = oOperation.TempSCColumnName;
                                                                #region Skip for Event /Place / Physical Activity                                                               
                                                                oColumnName = TableField(currentcolumnname);
                                                                #endregion

                                                                var oSecondPrimaryID = (oOperation.TempSCTableName == "Event" ? "Id" : "EventId");
                                                                var oSecondSelectQuery = "SELECT " + oOperation.TempSCTableName;
                                                                oSecondSelectQuery += "." + oColumnName;
                                                                oSecondSelectQuery += " FROM " + oOperation.TempSCTableName + " " + oOperation.TempSCTableName;
                                                                oSecondSelectQuery += " WITH (NOLOCK) WHERE " + oOperation.TempSCTableName + "." + oSecondPrimaryID + " = " + oRow[PrimaryIDValueBasedOnTable];

                                                                var oFirstResult = DBProvider.ExecuteScalar(oFirstSelectQuery, CommandType.Text);

                                                                var oSecondResult = DBProvider.ExecuteScalar(oSecondSelectQuery, CommandType.Text);

                                                                if (oFirstResult != null)
                                                                {
                                                                    //float,date,integer
                                                                    switch (oOperation.TempFRColumnDataType)
                                                                    {
                                                                        case "date":
                                                                            oValues = oFirstResult;
                                                                            break;
                                                                        case "string":
                                                                            DateTime oResultDate;
                                                                            if (DateTime.TryParse(oFirstResult.ToString(), out oResultDate))
                                                                            {
                                                                                oValues = oResultDate;
                                                                            }
                                                                            break;
                                                                        default:
                                                                            break;
                                                                    }
                                                                }
                                                                if (oSecondResult != null)
                                                                {
                                                                    var oOperationValues = oValues;
                                                                    //float,date,integer                                                        
                                                                    switch (oOperation.TempSCColumnDataType)
                                                                    {
                                                                        case "date":
                                                                            oTimeResult = (Convert.ToDateTime(oOperationValues).Subtract(Convert.ToDateTime(oSecondResult)));
                                                                            break;
                                                                        case "string":
                                                                            DateTime oResultDate;
                                                                            if (DateTime.TryParse(oSecondResult.ToString(), out oResultDate))
                                                                            {
                                                                                oTimeResult = (Convert.ToDateTime(oOperationValues).Subtract(oResultDate));
                                                                            }
                                                                            break;
                                                                        default:
                                                                            break;
                                                                    }
                                                                    oValues = oOperationValues;
                                                                }

                                                                var Days = oTimeResult.Days;
                                                                var Hours = oTimeResult.Hours;
                                                                var Minutes = oTimeResult.Minutes;
                                                                var Seconds = oTimeResult.Seconds;
                                                                var Milliseconds = oTimeResult.Milliseconds;

                                                                string Duration = "PT";

                                                                Duration += (Days > 0 ? Days + "D" : "");
                                                                Duration += (Hours > 0 ? Hours + "H" : "");
                                                                Duration += (Minutes > 0 ? Minutes + "M" : "");
                                                                Duration += (Seconds > 0 ? Seconds + "S" : "");
                                                                Duration += (Milliseconds > 0 ? Milliseconds + "MS" : "");

                                                                if (Duration != "PT")
                                                                {
                                                                    oColumnName = string.Empty;
                                                                    currentcolumnname = oOperation.ColumnName;
                                                                    #region Skip for Event /Place / Physical Activity                                                                    
                                                                    oColumnName = TableField(currentcolumnname);
                                                                    #endregion
                                                                    if (oOperation.TableName == "Event" && (currentcolumnname.StartsWith("superEvent_") || currentcolumnname.StartsWith("subEvent_")))
                                                                    {
                                                                        var oInsertUpdateQuery = PreparedQuery(oOperation.TableName, oColumnName, Duration, PrimaryIDBasedOnTable, oRow[PrimaryIDValueBasedOnTable].ToString(), true);
                                                                        DBProvider.ExecuteNonQuery(oInsertUpdateQuery, CommandType.Text);
                                                                    }
                                                                    else
                                                                    {
                                                                        var oInsertUpdateQuery = PreparedQuery(oOperation.TableName, oColumnName, Duration, PrimaryIDBasedOnTable, oRow[PrimaryIDValueBasedOnTable].ToString());
                                                                        DBProvider.ExecuteNonQuery(oInsertUpdateQuery, CommandType.Text);
                                                                    }
                                                                }
                                                            }
                                                            #endregion
                                                        }
                                                        else
                                                        {
                                                            #region all-field
                                                            foreach (DataRow oRow in oCalculationAutoData.Tables[0].Rows)
                                                            {
                                                                var oColumnName = string.Empty;
                                                                currentcolumnname = oOperation.TempFRColumnName;
                                                                #region Skip for Event /Place / Physical Activity                                                               
                                                                oColumnName = TableField(currentcolumnname);
                                                                #endregion
                                                                object oValues = null;
                                                                var oFirstPrimaryID = (oOperation.TempFRTableName == "Event" ? "Id" : "EventId");
                                                                var oFirstSelectQuery = "SELECT " + oOperation.TempFRTableName;
                                                                oFirstSelectQuery += "." + oColumnName;
                                                                oFirstSelectQuery += " FROM " + oOperation.TempFRTableName + " " + oOperation.TempFRTableName;
                                                                oFirstSelectQuery += " WITH (NOLOCK) WHERE " + oOperation.TempFRTableName + "." + oFirstPrimaryID + " = " + oRow[PrimaryIDValueBasedOnTable];

                                                                oColumnName = string.Empty;
                                                                currentcolumnname = oOperation.TempSCColumnName;
                                                                #region Skip for Event /Place / Physical Activity                                                                
                                                                oColumnName = TableField(currentcolumnname);
                                                                #endregion
                                                                var oSecondPrimaryID = (oOperation.TempSCTableName == "Event" ? "Id" : "EventId");
                                                                var oSecondSelectQuery = "SELECT " + oOperation.TempSCTableName;
                                                                oSecondSelectQuery += "." + oColumnName;
                                                                oSecondSelectQuery += " FROM " + oOperation.TempSCTableName + " " + oOperation.TempSCTableName;
                                                                oSecondSelectQuery += " WITH (NOLOCK) WHERE " + oOperation.TempSCTableName + "." + oSecondPrimaryID + " = " + oRow[PrimaryIDValueBasedOnTable];

                                                                var oFirstResult = DBProvider.ExecuteScalar(oFirstSelectQuery, CommandType.Text);

                                                                var oSecondResult = DBProvider.ExecuteScalar(oSecondSelectQuery, CommandType.Text);

                                                                if (oFirstResult != null)
                                                                {
                                                                    //float,date,integer
                                                                    switch (oOperation.TempFRColumnDataType)
                                                                    {
                                                                        case "float":
                                                                            oValues = oFirstResult;
                                                                            break;
                                                                        case "date":
                                                                            oValues = oFirstResult;
                                                                            break;
                                                                        case "integer":
                                                                            oValues = oFirstResult;
                                                                            break;
                                                                        case "string":
                                                                            DateTime oResultDate;
                                                                            if (DateTime.TryParse(oFirstResult.ToString(), out oResultDate))
                                                                            {
                                                                                oValues = oResultDate;
                                                                            }
                                                                            break;
                                                                        default:
                                                                            break;
                                                                    }
                                                                }

                                                                if (oSecondResult != null)
                                                                {
                                                                    var oOperationValues = oValues;
                                                                    //float,date,integer                                                        
                                                                    switch (oOperation.TempSCColumnDataType)
                                                                    {
                                                                        case "float":
                                                                            oOperationValues = (float.Parse(Convert.ToString(oValues)) - float.Parse(Convert.ToString(oSecondResult)));
                                                                            break;
                                                                        case "date":
                                                                            oOperationValues = Convert.ToDateTime(oValues).Add(-Settings.ConvertToTimestamp(Convert.ToDateTime(oSecondResult)));
                                                                            break;
                                                                        case "integer":
                                                                            oOperationValues = (float.Parse(Convert.ToString(oValues)) - float.Parse(Convert.ToString(oSecondResult)));
                                                                            break;
                                                                        case "string":
                                                                            DateTime oResultDate;
                                                                            if (DateTime.TryParse(oSecondResult.ToString(), out oResultDate))
                                                                            {
                                                                                oOperationValues = (Convert.ToDateTime(oValues).Add(-Settings.ConvertToTimestamp(Convert.ToDateTime(oResultDate))));
                                                                            }
                                                                            break;
                                                                        default:
                                                                            break;
                                                                    }
                                                                    oValues = oOperationValues;
                                                                }

                                                                oColumnName = string.Empty;
                                                                currentcolumnname = oOperation.ColumnName;
                                                                #region Skip for Event /Place / Physical Activity                                                               
                                                                oColumnName = TableField(currentcolumnname);
                                                                #endregion
                                                                if (oOperation.TableName == "Event" && (currentcolumnname.StartsWith("superEvent_") || currentcolumnname.StartsWith("subEvent_")))
                                                                {
                                                                    var oInsertUpdateQuery = PreparedQuery(oOperation.TableName, oColumnName, oValues, PrimaryIDBasedOnTable, oRow[PrimaryIDValueBasedOnTable].ToString(), true);
                                                                    DBProvider.ExecuteNonQuery(oInsertUpdateQuery, CommandType.Text);
                                                                }
                                                                else
                                                                {
                                                                    var oInsertUpdateQuery = PreparedQuery(oOperation.TableName, oColumnName, oValues, PrimaryIDBasedOnTable, oRow[PrimaryIDValueBasedOnTable].ToString());
                                                                    DBProvider.ExecuteNonQuery(oInsertUpdateQuery, CommandType.Text);
                                                                }
                                                            }
                                                            #endregion
                                                        }
                                                    }
                                                    #endregion
                                                }
                                                else if (oOperation.Value == "2")//multiplication (int,float,decimal)
                                                {
                                                    #region multiplication
                                                    //if key has filed
                                                    var oCurrentKey = oOperation.ColumnName.Split('_').LastOrDefault();
                                                    if (!string.IsNullOrEmpty(oCurrentKey))
                                                    {
                                                        foreach (DataRow oRow in oCalculationAutoData.Tables[0].Rows)
                                                        {
                                                            var oColumnName = string.Empty;
                                                            currentcolumnname = oOperation.TempFRColumnName;
                                                            #region Skip for Event /Place / Physical Activity                                                           
                                                            oColumnName = TableField(currentcolumnname);
                                                            #endregion


                                                            object oValues = null;
                                                            var oFirstPrimaryID = (oOperation.TempFRTableName == "Event" ? "Id" : "EventId");
                                                            var oFirstSelectQuery = "SELECT " + oOperation.TempFRTableName;
                                                            oFirstSelectQuery += "." + oColumnName;
                                                            oFirstSelectQuery += " FROM " + oOperation.TempFRTableName + " " + oOperation.TempFRTableName;
                                                            oFirstSelectQuery += " WITH (NOLOCK) WHERE " + oOperation.TempFRTableName + "." + oFirstPrimaryID + " = " + oRow[PrimaryIDValueBasedOnTable];

                                                            oColumnName = string.Empty;
                                                            currentcolumnname = oOperation.TempSCColumnName;
                                                            #region Skip for Event /Place / Physical Activity                                                            
                                                            oColumnName = TableField(currentcolumnname);
                                                            #endregion
                                                            var oSecondPrimaryID = (oOperation.TempSCTableName == "Event" ? "Id" : "EventId");
                                                            var oSecondSelectQuery = "SELECT " + oOperation.TempSCTableName;
                                                            oSecondSelectQuery += "." + oColumnName;
                                                            oSecondSelectQuery += " FROM " + oOperation.TempSCTableName + " " + oOperation.TempSCTableName;
                                                            oSecondSelectQuery += " WITH (NOLOCK) WHERE " + oOperation.TempSCTableName + "." + oSecondPrimaryID + " = " + oRow[PrimaryIDValueBasedOnTable];

                                                            var oFirstResult = DBProvider.ExecuteScalar(oFirstSelectQuery, CommandType.Text);

                                                            var oSecondResult = DBProvider.ExecuteScalar(oSecondSelectQuery, CommandType.Text);

                                                            if (oFirstResult != null)
                                                            {
                                                                //float,date,integer
                                                                switch (oOperation.TempFRColumnDataType)
                                                                {
                                                                    case "float":
                                                                        oValues = oFirstResult;
                                                                        break;
                                                                    case "date":
                                                                        oValues = oFirstResult;
                                                                        break;
                                                                    case "integer":
                                                                        oValues = oFirstResult;
                                                                        break;
                                                                    default:
                                                                        break;
                                                                }
                                                            }

                                                            if (oSecondResult != null)
                                                            {
                                                                var oOperationValues = oValues;
                                                                //float,date,integer                                                        
                                                                switch (oOperation.TempSCColumnDataType)
                                                                {
                                                                    case "float":
                                                                        oOperationValues = (float.Parse(Convert.ToString(oValues)) * float.Parse(Convert.ToString(oSecondResult)));
                                                                        break;
                                                                    case "date":
                                                                        //oOperationValues = Convert.ToDateTime(oValues).Add(-Settings.ConvertToTimestamp(Convert.ToDateTime(oSecondResult)));
                                                                        break;
                                                                    case "integer":
                                                                        oOperationValues = (float.Parse(Convert.ToString(oValues)) * float.Parse(Convert.ToString(oSecondResult)));
                                                                        break;
                                                                    default:
                                                                        break;
                                                                }
                                                                oValues = oOperationValues;
                                                            }

                                                            oColumnName = string.Empty;
                                                            currentcolumnname = oOperation.ColumnName;
                                                            #region Skip for Event /Place / Physical Activity                                                           
                                                            oColumnName = TableField(currentcolumnname);
                                                            #endregion
                                                            if (oOperation.TableName == "Event" && (currentcolumnname.StartsWith("superEvent_") || currentcolumnname.StartsWith("subEvent_")))
                                                            {
                                                                var oInsertUpdateQuery = PreparedQuery(oOperation.TableName, oColumnName, oValues, PrimaryIDBasedOnTable, oRow[PrimaryIDValueBasedOnTable].ToString(), true);
                                                                DBProvider.ExecuteNonQuery(oInsertUpdateQuery, CommandType.Text);
                                                            }
                                                            else
                                                            {
                                                                var oInsertUpdateQuery = PreparedQuery(oOperation.TableName, oColumnName, oValues, PrimaryIDBasedOnTable, oRow[PrimaryIDValueBasedOnTable].ToString());
                                                                DBProvider.ExecuteNonQuery(oInsertUpdateQuery, CommandType.Text);
                                                            }
                                                        }
                                                    }
                                                    #endregion
                                                }
                                                else if (oOperation.Value == "3")//division (int,float,decimal)
                                                {
                                                    #region division
                                                    //if key has filed
                                                    var oCurrentKey = oOperation.ColumnName.Split('_').LastOrDefault();
                                                    if (!string.IsNullOrEmpty(oCurrentKey))
                                                    {
                                                        foreach (DataRow oRow in oCalculationAutoData.Tables[0].Rows)
                                                        {
                                                            var oColumnName = string.Empty;
                                                            currentcolumnname = oOperation.TempFRColumnName;
                                                            #region Skip for Event /Place / Physical Activity                                                           
                                                            oColumnName = TableField(currentcolumnname);
                                                            #endregion

                                                            object oValues = null;
                                                            var oFirstPrimaryID = (oOperation.TempFRTableName == "Event" ? "Id" : "EventId");
                                                            var oFirstSelectQuery = "SELECT " + oOperation.TempFRTableName;
                                                            oFirstSelectQuery += "." + oColumnName;
                                                            oFirstSelectQuery += " FROM " + oOperation.TempFRTableName + " " + oOperation.TempFRTableName;
                                                            oFirstSelectQuery += " WITH (NOLOCK) WHERE " + oOperation.TempFRTableName + "." + oFirstPrimaryID + " = " + oRow[PrimaryIDValueBasedOnTable];

                                                            oColumnName = string.Empty;
                                                            currentcolumnname = oOperation.TempSCColumnName;
                                                            #region Skip for Event /Place / Physical Activity                                                          
                                                            oColumnName = TableField(currentcolumnname);
                                                            #endregion

                                                            var oSecondPrimaryID = (oOperation.TempSCTableName == "Event" ? "Id" : "EventId");
                                                            var oSecondSelectQuery = "SELECT " + oOperation.TempSCTableName;
                                                            oSecondSelectQuery += "." + oColumnName;
                                                            oSecondSelectQuery += " FROM " + oOperation.TempSCTableName + " " + oOperation.TempSCTableName;
                                                            oSecondSelectQuery += " WITH (NOLOCK) WHERE " + oOperation.TempSCTableName + "." + oSecondPrimaryID + " = " + oRow[PrimaryIDValueBasedOnTable];

                                                            var oFirstResult = DBProvider.ExecuteScalar(oFirstSelectQuery, CommandType.Text);

                                                            var oSecondResult = DBProvider.ExecuteScalar(oSecondSelectQuery, CommandType.Text);

                                                            if (oFirstResult != null)
                                                            {
                                                                //float,date,integer
                                                                switch (oOperation.TempFRColumnDataType)
                                                                {
                                                                    case "float":
                                                                        oValues = oFirstResult;
                                                                        break;
                                                                    case "date":
                                                                        oValues = oFirstResult;
                                                                        break;
                                                                    case "integer":
                                                                        oValues = oFirstResult;
                                                                        break;
                                                                    default:

                                                                        break;
                                                                }
                                                            }

                                                            if (oSecondResult != null)
                                                            {
                                                                var oOperationValues = oValues;
                                                                //float,date,integer                                                        
                                                                switch (oOperation.TempSCColumnDataType)
                                                                {
                                                                    case "float":
                                                                        oOperationValues = (float.Parse(Convert.ToString(oValues)) / float.Parse(Convert.ToString(oSecondResult)));
                                                                        break;
                                                                    case "date":
                                                                        //oOperationValues = Convert.ToDateTime(oValues).Add(-Settings.ConvertToTimestamp(Convert.ToDateTime(oSecondResult)));
                                                                        break;
                                                                    case "integer":
                                                                        oOperationValues = (float.Parse(Convert.ToString(oValues)) / float.Parse(Convert.ToString(oSecondResult)));
                                                                        break;
                                                                    default:
                                                                        break;
                                                                }
                                                                oValues = oOperationValues;
                                                            }

                                                            oColumnName = string.Empty;
                                                            currentcolumnname = oOperation.ColumnName;
                                                            #region Skip for Event /Place / Physical Activity                                                           
                                                            oColumnName = TableField(currentcolumnname);
                                                            #endregion
                                                            if (oOperation.TableName == "Event" && (currentcolumnname.StartsWith("superEvent_") || currentcolumnname.StartsWith("subEvent_")))
                                                            {
                                                                var oInsertUpdateQuery = PreparedQuery(oOperation.TableName, oColumnName, oValues, PrimaryIDBasedOnTable, oRow[PrimaryIDValueBasedOnTable].ToString(), true);
                                                                DBProvider.ExecuteNonQuery(oInsertUpdateQuery, CommandType.Text);
                                                            }
                                                            else
                                                            {
                                                                var oInsertUpdateQuery = PreparedQuery(oOperation.TableName, oColumnName, oValues, PrimaryIDBasedOnTable, oRow[PrimaryIDValueBasedOnTable].ToString());
                                                                DBProvider.ExecuteNonQuery(oInsertUpdateQuery, CommandType.Text);
                                                            }
                                                        }
                                                    }
                                                    #endregion
                                                }
                                                else if (oOperation.Value == "4") //concat 
                                                {
                                                    #region concat
                                                    foreach (DataRow oRow in oCalculationAutoData.Tables[0].Rows)
                                                    {
                                                        var oColumnName = string.Empty;
                                                        currentcolumnname = oOperation.TempFRColumnName;
                                                        #region Skip for Event /Place / Physical Activity                                                        
                                                        oColumnName = TableField(currentcolumnname);
                                                        #endregion

                                                        string oValues = null;
                                                        var oFirstPrimaryID = (oOperation.TempFRTableName == "Event" ? "Id" : "EventId");
                                                        var oFirstSelectQuery = "SELECT " + oOperation.TempFRTableName;
                                                        oFirstSelectQuery += "." + oColumnName;
                                                        oFirstSelectQuery += " FROM " + oOperation.TempFRTableName + " " + oOperation.TempFRTableName;
                                                        oFirstSelectQuery += " WITH (NOLOCK) WHERE " + oOperation.TempFRTableName + "." + oFirstPrimaryID + " = " + oRow[PrimaryIDValueBasedOnTable];

                                                        var oFirstResult = DBProvider.ExecuteScalar(oFirstSelectQuery, CommandType.Text);

                                                        if (oFirstResult != null)
                                                        {
                                                            oValues = string.Concat(oFirstResult);
                                                        }

                                                        oColumnName = string.Empty;
                                                        currentcolumnname = oOperation.TempSCColumnName;
                                                        #region Skip for Event /Place / Physical Activity                                                       
                                                        oColumnName = TableField(currentcolumnname);
                                                        #endregion

                                                        var oSecondPrimaryID = (oOperation.TempSCTableName == "Event" ? "Id" : "EventId");
                                                        var oSecondSelectQuery = "SELECT " + oOperation.TempSCTableName;
                                                        oSecondSelectQuery += "." + oColumnName;
                                                        oSecondSelectQuery += " FROM " + oOperation.TempSCTableName + " " + oOperation.TempSCTableName;
                                                        oSecondSelectQuery += " WITH (NOLOCK) WHERE " + oOperation.TempSCTableName + "." + oSecondPrimaryID + " = " + oRow[PrimaryIDValueBasedOnTable];

                                                        var oSecondResult = DBProvider.ExecuteScalar(oSecondSelectQuery, CommandType.Text);

                                                        if (oSecondResult != null)
                                                        {
                                                            oValues = string.Concat(oValues, " ", oSecondResult);

                                                        }

                                                        oColumnName = string.Empty;
                                                        currentcolumnname = oOperation.ColumnName;
                                                        #region Skip for Event /Place / Physical Activity                                                       
                                                        oColumnName = TableField(currentcolumnname);
                                                        #endregion
                                                        if (oOperation.TableName == "Event" && (currentcolumnname.StartsWith("superEvent_") || currentcolumnname.StartsWith("subEvent_")))
                                                        {
                                                            var oInsertUpdateQuery = PreparedQuery(oOperation.TableName, oColumnName, oValues, PrimaryIDBasedOnTable, oRow[PrimaryIDValueBasedOnTable].ToString(), true);
                                                            DBProvider.ExecuteNonQuery(oInsertUpdateQuery, CommandType.Text);
                                                        }
                                                        else
                                                        {
                                                            var oInsertUpdateQuery = PreparedQuery(oOperation.TableName, oColumnName, oValues, PrimaryIDBasedOnTable, oRow[PrimaryIDValueBasedOnTable].ToString());
                                                            DBProvider.ExecuteNonQuery(oInsertUpdateQuery, CommandType.Text);
                                                        }
                                                    }
                                                    #endregion
                                                }
                                            }
                                            #endregion
                                            break;
                                        case 6://Remove
                                            #region Remove
                                            var oRemoveSessionQuery = oQueryBuilder.ToString();
                                            var lstRemoveSession = new List<SqlParameter>();
                                            var oRemoveSessionAutoData = DBProvider.GetDataSet(oRemoveSessionQuery, CommandType.Text, ref lstRemoveSession);
                                            if (oRemoveSessionAutoData != null && oRemoveSessionAutoData.Tables.Count > 0)
                                            {
                                                if (IsEventDelete)
                                                {
                                                    var lstEventId = new List<string>();
                                                    foreach (DataRow oRow in oRemoveSessionAutoData.Tables[0].Rows)
                                                    {
                                                        lstEventId.Add(oRow["Id"].ToString());
                                                    }
                                                    AutoFlushEvent_Delete(string.Join(",", lstEventId));
                                                }
                                                else
                                                {
                                                    var lstEventId = new List<string>();
                                                    foreach (DataRow oRow in oRemoveSessionAutoData.Tables[0].Rows)
                                                    {
                                                        lstEventId.Add(oRow["EventId"].ToString());
                                                    }
                                                    AutoFlushEvent_Delete(string.Join(",", lstEventId));
                                                }
                                            }
                                            #endregion
                                            break;
                                    }
                                }
                            }
                        }
                        #endregion
                    }
                    #endregion
                }
                #endregion
            }
            catch (Exception ex)
            {
                LogHelper.InsertErrorLogs("[DataLaundryScheduler] SchedulerHelper", "AutoRuleFilterData - else - Dynamic Query", ex.Message, ex.InnerException?.Message, ex.StackTrace, Id);
            }
            #endregion
        }
        #region return table Key 
        public static string TableField(string currentcolumnname)
        {
            string oColumnName = string.Empty;
            currentcolumnname = currentcolumnname.ToLower();
            if (currentcolumnname == "image_url" || currentcolumnname == "superEvent_image_url" || currentcolumnname == "subEvent_image_url") //FOR Event Table
                oColumnName = "Image";
            else if (currentcolumnname == "image_thumbnail" || currentcolumnname == "superEvent_image_thumbnail" || currentcolumnname == "subEvent_image_thumbnail")
                oColumnName = "ImageThumbnail";
            else if (currentcolumnname == "activity_broader" || currentcolumnname == "subEvent_activity_broader" || currentcolumnname == "superEvent_activity_broader")//FOR PhysicalActivity
                oColumnName = "BroaderId";
            else if (currentcolumnname == "activity_narrower" || currentcolumnname == "subEvent_activity_narrower" || currentcolumnname == "superEvent_activity_narrower")
                oColumnName = "NarrowerId";
            else if (currentcolumnname == "location_geo_latitude" || currentcolumnname == "superEvent_location_geo_latitude" || currentcolumnname == "subEvent_location_geo_latitude")//FOR PLACE || LOCATION
                oColumnName = "Lat";
            else if (currentcolumnname == "location_geo_longitude" || currentcolumnname == "superEvent_location_geo_longitude" || currentcolumnname == "subEvent_location_geo_longitude")
                oColumnName = "Long";
            else if (currentcolumnname == "activity_type" || currentcolumnname == "superEvent_activity_type" || currentcolumnname == "subEvent_activity_type")
                oColumnName = "PrefLabel";
            else
                oColumnName = currentcolumnname.Split('_').LastOrDefault();

            return oColumnName;
        }

        public static string PreparedQuery(string TableName, string oColumnName, object oResult, string PrimaryIDBasedOnTable, string oRowValues, bool IsSubEventOrSuperEvent = false)
        {
            var oInsertUpdateQuery = "BEGIN";
            if (IsSubEventOrSuperEvent)
            {
                oInsertUpdateQuery += " UPDATE " + TableName + " SET " + oColumnName + " = '" + oResult + "'";
                oInsertUpdateQuery += " WHERE " + PrimaryIDBasedOnTable + " = " + oRowValues;
                oInsertUpdateQuery += " OR SuperEventId = "+oRowValues;
            }
            else
            {
                oInsertUpdateQuery += " IF NOT EXISTS(SELECT id FROM " + TableName;
                oInsertUpdateQuery += " WITH (NOLOCK)  WHERE " + PrimaryIDBasedOnTable + " = " + oRowValues;
                oInsertUpdateQuery += ")";
                oInsertUpdateQuery += "  BEGIN";
                oInsertUpdateQuery += "    INSERT INTO " + TableName + "(" + oColumnName + "," + PrimaryIDBasedOnTable + ")";
                oInsertUpdateQuery += "     VALUES('" + oResult + "'," + oRowValues + ")";
                oInsertUpdateQuery += " END";
                oInsertUpdateQuery += " ELSE";
                oInsertUpdateQuery += " BEGIN ";
                oInsertUpdateQuery += " UPDATE " + TableName + " SET " + oColumnName + " = '" + oResult + "'";
                oInsertUpdateQuery += " WHERE " + PrimaryIDBasedOnTable + " = " + oRowValues;
                oInsertUpdateQuery += " END";
            }
            oInsertUpdateQuery += " END";

            return oInsertUpdateQuery;
        }
        #endregion

        #endregion

        #region Check longitude and latitude exists and not
        public static void CheckAndPlaceUpdate(int FeedProviderId)
        {
            try
            {
                var lstSqlParameter = new List<SqlParameter>();
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@FeedProviderId", Value = FeedProviderId, SqlDbType = SqlDbType.BigInt });
                var ds = DBProvider.GetDataTable("spr_CheckPlaceData", CommandType.StoredProcedure, ref lstSqlParameter);
                if (ds != null && ds.Rows.Count > 0)
                {
                    string Lat = "", Long = "";
                    var dtPlace = new DataTable();
                    dtPlace.Columns.Add("Id", typeof(Int64));
                    dtPlace.Columns.Add("PostalCode", typeof(string));
                    dtPlace.Columns.Add("Lat", typeof(string));
                    dtPlace.Columns.Add("Long", typeof(string));
                    dtPlace.Columns.Add("FeedProviderId", typeof(int));
                    dtPlace.Columns.Add("EventId", typeof(Int64));
                    for (int i = 0; i < ds.Rows.Count; i++)
                    {
                        var row = ds.Rows[i];
                        #region Call webapi  and add row in datatable
                        string PostalCode = Convert.ToString(row["PostalCode"]);
                        string oUrl = string.Concat("http://api.postcodes.io/postcodes/", PostalCode);

                        var oJsonData = GetWebContent(oUrl, FeedProviderId);
                        if (!string.IsNullOrEmpty(oJsonData))
                        {
                            JObject obj = JObject.Parse(oJsonData);
                            if (obj.SelectToken("status").ToString() == "200")
                            {
                                JToken longitude = obj.SelectToken("result.longitude");
                                JToken latitude = obj.SelectToken("result.latitude");
                                Lat = latitude.ToString();
                                Long = longitude.ToString();

                                if (string.IsNullOrEmpty(Lat) && string.IsNullOrEmpty(Long))
                                    continue;

                                var dtplacerow = dtPlace.NewRow();
                                dtplacerow["Lat"] = Lat;
                                dtplacerow["Long"] = Long;
                                dtplacerow["PostalCode"] = PostalCode;
                                dtplacerow["FeedProviderId"] = FeedProviderId;
                                dtPlace.Rows.Add(dtplacerow);
                            }
                        }
                        #endregion                       
                    }
                    #region Update / spr_PlaceUpdateLatLong
                    lstSqlParameter = new List<SqlParameter>();
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@TTPlace", Value = dtPlace, SqlDbType = SqlDbType.Structured });
                    DBProvider.ExecuteNonQuery("spr_PlaceUpdateLatLong", CommandType.StoredProcedure, ref lstSqlParameter);
                    #endregion
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertErrorLogs("[DataLaundryScheduler] SchedulerHelper", "AutoPlaceUpdate", ex.Message, ex.InnerException?.Message, ex.StackTrace, FeedProviderId);
            }
        }
        #endregion

        #region AutoFlush_delete added date 14-02-2019
        public static void AutoFlushEvent_Delete(string EventIDs)
        {
            try
            {
                var lstSqlParameter = new List<SqlParameter>();
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@EventID", Value = EventIDs, SqlDbType = SqlDbType.NVarChar });
                DBProvider.ExecuteNonQuery("spr_AutoFlushEvent_Delete", CommandType.StoredProcedure, ref lstSqlParameter);
            }
            catch (Exception ex)
            {
                LogHelper.InsertErrorLogs("[DataLaundryScheduler] SchedulerHelper", "AutoFlushEvent_Delete", ex.Message, ex.InnerException?.Message, ex.StackTrace);
            }
        }
        #endregion       

    }
    public class WebClient : System.Net.WebClient
    {
        public int Timeout { get; set; }
        public int MaxIdleTime { get; set; }
        public int ConnectionLeaseTimeout { get; set; }
        public bool KeepAlive { get; set; }
        protected override WebRequest GetWebRequest(Uri uri)
        {
            WebRequest lWebRequest = base.GetWebRequest(uri);
            lWebRequest.Timeout = Timeout;
            ((HttpWebRequest)lWebRequest).ReadWriteTimeout = Timeout;
            ((HttpWebRequest)lWebRequest).KeepAlive = KeepAlive;
            ((HttpWebRequest)lWebRequest).UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:6.0.2) Gecko/20100101 Firefox/6.0.2";
            ((HttpWebRequest)lWebRequest).Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            ((HttpWebRequest)lWebRequest).Headers.Add("Accept-Language", "en-gb,en;q=0.5");
            ((HttpWebRequest)lWebRequest).Headers.Add("Accept-Charset", "ISO-8859-1,utf-8;q=0.7,*;q=0.7");
            ((HttpWebRequest)lWebRequest).Proxy = WebRequest.GetSystemWebProxy();
            ((HttpWebRequest)lWebRequest).ContentType = "application/x-www-form-urlencoded";
            ((HttpWebRequest)lWebRequest).ServicePoint.ConnectionLeaseTimeout = 50000;
            ((HttpWebRequest)lWebRequest).ServicePoint.MaxIdleTime = MaxIdleTime;
            //((HttpWebRequest)lWebRequest).ContentType = "text/xml";
            return lWebRequest;
        }
    }

    public class GZipWebClient : WebClient
    {
        protected override WebRequest GetWebRequest(Uri address)
        {
            HttpWebRequest request = (HttpWebRequest)base.GetWebRequest(address);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            return request;
        }
    }
}
