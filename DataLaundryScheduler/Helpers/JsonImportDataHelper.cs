using DataLaundryScheduler.DTO;
using DataLaundryScheduler.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DataLaundryScheduler.Helpers
{
    public class JsonImportDataHelper
    {
        public static void InsertJsonToken(JsonImportData Model)
        {
            try
            {
                var lstSqlParameter = new List<SqlParameter>();
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@FeedProviderID", Value = Model.FeedProviderID, SqlDbType = SqlDbType.BigInt });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@EventID", Value = Model.EventID, SqlDbType = SqlDbType.BigInt });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@FeedID", Value = Model.FeedID, SqlDbType = SqlDbType.NVarChar });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@JsonData", Value = Model.JsonData, SqlDbType = SqlDbType.NVarChar });

                var oResult = DBProvider.ExecuteNonQuery("spr_JsonImportData_Insert", CommandType.StoredProcedure, ref lstSqlParameter);
            }
            catch (Exception ex)
            {
                LogHelper.InsertErrorLogs("[DataLaundryScheduler] JsonImportDataHelper", "InsertJsonToken", ex.Message, ex.InnerException?.Message, ex.StackTrace, Model.FeedProviderID);
            }
        }
    }
}
