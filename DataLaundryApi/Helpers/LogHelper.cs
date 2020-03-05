using DataLaundryApi.Constants;
using DataLaundryApi.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace DataLaundryApi.Helpers
{
    public class LogHelper
    {
        private static IHostingEnvironment _hostingEnvironment;

        public static void Configure(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }
        //public static string IsServiceLogEnabled = System.Configuration.ConfigurationManager.AppSettings["IsServiceLogEnabled"].ToString();
        public static string IsServiceLogEnabled = Settings.GetAppSetting("IsServiceLogEnabled");

        public static void InsertServiceLogs(string MethodName, string Model = null, DateTime? RequestTime = null, DateTime? ResponseTime = null)
        {
            try
            {
                if (IsServiceLogEnabled == "1")
                {
                    var lstSqlParameter = new List<SqlParameter>();
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@MethodName", SqlDbType = SqlDbType.NVarChar, Value = MethodName });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Model", SqlDbType = SqlDbType.NVarChar, Value = Model });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@RequestTime", SqlDbType = SqlDbType.DateTime, Value = RequestTime });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@ResponseTime", SqlDbType = SqlDbType.DateTime, Value = ResponseTime });
                    int i = DBProvider.ExecuteNonQuery("spr_ServiceLog_Insert", CommandType.StoredProcedure, ref lstSqlParameter);
                }
            }
            catch (Exception ex)
            {
                WriteDataToErrorLogFile(ex);
            }
        }

        public static void InsertErrorLogs(string moduleName, string methodName, string exception, string innerException, string stackTrace)
        {
            try
            {
                var lstSqlParameter = new List<SqlParameter>();
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@ModuleName", SqlDbType = SqlDbType.NVarChar, Value = moduleName });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@MethodName", SqlDbType = SqlDbType.NVarChar, Value = methodName });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Exception", SqlDbType = SqlDbType.NVarChar, Value = exception });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@InnerException", SqlDbType = SqlDbType.NVarChar, Value = (object)innerException ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@StackTrace", SqlDbType = SqlDbType.NVarChar, Value = stackTrace });

                int i = DBProvider.ExecuteNonQuery("ErrorLog_Insert", CommandType.StoredProcedure, ref lstSqlParameter);
            }
            catch (Exception ex)
            {
                WriteDataToErrorLogFile(ex);
            }
        }

        public static void WriteDataToErrorLogFile(Exception ex)
        {
            StreamWriter sw = null;
            try
            {
                 //string webRootPath = _hostingEnvironment.WebRootPath;
                string contentRootPath = _hostingEnvironment.ContentRootPath;
                string errorlogfile = DateTime.Now.Date.ToString("dd-MMM-yyyy") + ".txt";
                string filePath = Path.Combine(string.Concat(contentRootPath,"/",Settings.ErrorLogPath), errorlogfile);
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                if (!File.Exists(filePath))
                {
                    File.Create(filePath).Dispose();
                }
                sw = new StreamWriter(filePath, true);
                sw.WriteLine("Error Logged at " + DateTime.Now);
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
            {
                Console.WriteLine(ex1);
            }
        }
    }
}