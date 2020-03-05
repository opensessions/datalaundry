using DataLaundryDAL.DTO;
using DataLaundryDAL.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DataLaundryDAL.Helper
{
    public class EventHelper
    {
        public static bool Insert(Data data)
        {
            var lstSqlParameter = new List<SqlParameter>();

            string image = null, location = null, organizer = null, activity = null, programme = null, schedule = null;

            if (data.Image != null)
                image = JsonConvert.SerializeObject(data.Image);

            if (data.Location != null)
                location = JsonConvert.SerializeObject(data.Location);

            if (data.Organizer != null)
                organizer = JsonConvert.SerializeObject(data.Organizer);

            if (data.Activity != null)
                activity = JsonConvert.SerializeObject(data.Activity);

            if (data.Programme != null)
                programme = JsonConvert.SerializeObject(data.Programme);
            

            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Name", SqlDbType = SqlDbType.NVarChar, Value = (object)data.Name ?? DBNull.Value });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Description", SqlDbType = SqlDbType.NVarChar, Value = (object)data.Description ?? DBNull.Value });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Image", SqlDbType = SqlDbType.NVarChar, Value = (object)image ?? DBNull.Value });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@StartDate", SqlDbType = SqlDbType.DateTime, Value = (object)data.StartDate.Date ?? DBNull.Value });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@EndDate", SqlDbType = SqlDbType.DateTime, Value = (object)data.EndDate.Date ?? DBNull.Value });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Duration", SqlDbType = SqlDbType.NVarChar, Value = (object)data.Duration ?? DBNull.Value });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Location", SqlDbType = SqlDbType.NVarChar, Value = (object)location ?? DBNull.Value });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Organizer", SqlDbType = SqlDbType.NVarChar, Value = (object)organizer ?? DBNull.Value });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Activity", SqlDbType = SqlDbType.NVarChar, Value = (object)activity ?? DBNull.Value });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Programme", SqlDbType = SqlDbType.NVarChar, Value = (object)programme ?? DBNull.Value });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Schedule", SqlDbType = SqlDbType.NVarChar, Value = (object)schedule ?? DBNull.Value });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Url", SqlDbType = SqlDbType.NVarChar, Value = (object)data.Url ?? DBNull.Value });

            int rowsAffected = DBProvider.ExecuteNonQuery("Event_Insert", CommandType.StoredProcedure, ref lstSqlParameter);
            
            return rowsAffected > 0;
        }
    }
}
