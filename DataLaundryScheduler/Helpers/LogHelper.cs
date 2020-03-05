using DataLaundryScheduler.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;

namespace DataLaundryScheduler.Helpers
{
    public class LogHelper
    {
        public static void InsertErrorLogs(string moduleName, string methodName, string exception, string innerException, string stackTrace, long? feedProviderId = null)
        {
            try
            {
                var lstSqlParameter = new List<SqlParameter>();
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@ModuleName", SqlDbType = SqlDbType.NVarChar, Value = moduleName });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@MethodName", SqlDbType = SqlDbType.NVarChar, Value = methodName });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Exception", SqlDbType = SqlDbType.NVarChar, Value = exception });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@InnerException", SqlDbType = SqlDbType.NVarChar, Value = (object)innerException ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@StackTrace", SqlDbType = SqlDbType.NVarChar, Value = stackTrace });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@FeedProviderId", SqlDbType = SqlDbType.BigInt, Value = feedProviderId });

                int i = DBProvider.ExecuteNonQuery("ErrorLog_Insert", CommandType.StoredProcedure, ref lstSqlParameter);
            }
            catch (Exception ex)
            {
                WriteDataToErrorLogFile(ex, feedProviderId);
            }
        }

        public static void WriteDataToErrorLogFile(Exception ex, long? feedProviderId = null)
        {
            StreamWriter sw = null;
            try
            {
                string errorlogfile = DateTime.Now.Date.ToString("dd-MMM-yyyy") + ".txt";
                //string filePath = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), errorlogfile);
                string filePath = Path.Combine(Path.GetDirectoryName(Directory.GetCurrentDirectory()), errorlogfile);
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                if (!File.Exists(filePath))
                {
                    File.Create(filePath).Dispose();
                }
                sw = new StreamWriter(filePath, true);
                
                if(feedProviderId != null)
                    sw.WriteLine("Error Logged for FeedId - " + feedProviderId + " at " + DateTime.UtcNow);
                else
                    sw.WriteLine("Error Logged at " + DateTime.UtcNow);
                sw.WriteLine(Environment.NewLine);
                sw.WriteLine(ex.Message);
                if (ex.StackTrace != null)
                {
                    sw.WriteLine(Environment.NewLine);
                    sw.WriteLine(ex.StackTrace);
                }
                sw.WriteLine(Environment.NewLine);
                sw.Flush();
                sw.Close();
            }
            catch (Exception ex1)
            { }
        }

        public static void InsertUpdateSchedulerLog(long id, long feedProviderId, DateTime? startDate, DateTime? endDate, string status, out long _logId, string note = null)
        {
            _logId = id;
            try
            {
                var lstSqlParameter = new List<SqlParameter>();
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Id", SqlDbType = SqlDbType.BigInt, Value = id });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@FeedProviderId", SqlDbType = SqlDbType.BigInt, Value = feedProviderId });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@StartDate", SqlDbType = SqlDbType.DateTime2, Value = (object)startDate ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@EndDate", SqlDbType = SqlDbType.DateTime2, Value = (object)endDate ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Status", SqlDbType = SqlDbType.NVarChar, Value = status });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Note", SqlDbType = SqlDbType.NVarChar, Value = note });

                _logId = Convert.ToInt32(DBProvider.ExecuteScalar("SchedulerLog_InsertUpdate", CommandType.StoredProcedure, ref lstSqlParameter));
            }
            catch (Exception ex)
            {
                LogHelper.InsertErrorLogs("[DataLaundryScheduler] LogHelper", "InsertUpdateSchedulerLog", ex.Message, ex.InnerException?.Message, ex.StackTrace, feedProviderId);
            }            
        }

        public static void InsertEventLog(long? feedProviderId, string currentPageUrl, string nextPageUrl, string moduleName, string methodName, string note = null)
        {
            try
            {
                var lstSqlParameter = new List<SqlParameter>();
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@FeedProviderId", SqlDbType = SqlDbType.BigInt, Value = (object)feedProviderId ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@CurrentPageUrl", SqlDbType = SqlDbType.NVarChar, Value = (object)currentPageUrl ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@NextPageUrl", SqlDbType = SqlDbType.NVarChar, Value = (object)nextPageUrl ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@ModuleName", SqlDbType = SqlDbType.NVarChar, Value = (object)moduleName ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@MethodName", SqlDbType = SqlDbType.NVarChar, Value = (object)methodName ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Note", SqlDbType = SqlDbType.NVarChar, Value = note });

                int i = DBProvider.ExecuteNonQuery("EventLog_Insert", CommandType.StoredProcedure, ref lstSqlParameter);
            }
            catch (Exception ex)
            {
                LogHelper.InsertErrorLogs("[DataLaundryScheduler] SchedulerHelper", "InsertEventLog", ex.Message, ex.InnerException?.Message, ex.StackTrace, feedProviderId);
            }
        }

        public static string stringValueOf(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attributes.Length > 0)
            {
                return attributes[0].Description;
            }
            else
            {
                return value.ToString();
            }
        }        
    }
        
    public enum LogStatus
    {
        [DescriptionAttribute("Job has been started")]
        JobStarted,
        [DescriptionAttribute("Job has been completed")]
        JobCompleted,
        [DescriptionAttribute("Job has been interrupted")]
        JobInterruption,
        // [DescriptionAttribute("Job has been terminated due to error occurrence")]
        [DescriptionAttribute("Error has been occurred")]
        ErrorOccurred
    }
}
