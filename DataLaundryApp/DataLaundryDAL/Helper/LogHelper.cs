using DataLaundryDAL.Constants;
using DataLaundryDAL.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace DataLaundryDAL.Helper
{
    public class LogHelper
    {
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
                string errorlogfile = DateTime.Now.Date.ToString("dd-MMM-yyyy") + ".txt";
                //string filePath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/" + Settings.ErrorLogPath), errorlogfile);
                string filePath="";
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
            }
        }
    }
}
