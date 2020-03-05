using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DataLaundryScheduler.Utilities
{
    class DBProvider
    {
        public static DataSet GetDataSet(string commandText, CommandType commandType, ref List<SqlParameter> parameters)
        {
            DataSet ds = null;
            if (!string.IsNullOrEmpty(commandText))
            {
                using (var cnn = new SqlConnection(Settings.GetConnectionString()))
                {
                    var cmd = cnn.CreateCommand();
                    cmd.CommandText = commandText;
                    cmd.CommandType = commandType;
                    cmd.CommandTimeout = Convert.ToInt32(Settings.GetAppCommandTimeout() ?? "3600");
                    cmd.Parameters.AddRange(parameters.ToArray());
                    
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        ds = new DataSet();
                        da.Fill(ds);
                    }
                    cmd.Dispose();                   
                }
            }
            return ds;
        }

        public static DataTable GetDataTable(string commandText, CommandType commandType, ref List<SqlParameter> parameters)
        {
            DataTable dt = null;
            if (!string.IsNullOrEmpty(commandText))
            {
                using (var cnn = new SqlConnection(Settings.GetConnectionString()))
                {
                    var cmd = cnn.CreateCommand();
                    cmd.CommandText = commandText;
                    cmd.CommandType = commandType;
                    cmd.CommandTimeout = Convert.ToInt32(Settings.GetAppCommandTimeout() ?? "3600"); 
                    cmd.Parameters.AddRange(parameters.ToArray());                   
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        dt = new DataTable();
                        da.Fill(dt);
                    }
                    cmd.Dispose();                   
                }
            }
            return dt;
        }

        public static int ExecuteNonQuery(string commandText, CommandType commandType, ref List<SqlParameter> parameters)
        {
            int result = 0;
            if (!string.IsNullOrEmpty(commandText))
            {
                using (var cnn = new SqlConnection(Settings.GetConnectionString()))
                {
                    var cmd = cnn.CreateCommand();
                    cmd.CommandText = commandText;
                    cmd.CommandType = commandType;
                    cmd.CommandTimeout = Convert.ToInt32(Settings.GetAppCommandTimeout() ?? "3600");
                    cmd.Parameters.AddRange(parameters.ToArray());                   
                    cnn.Open();
                    result = cmd.ExecuteNonQuery();
                    cmd.Dispose();
                    cnn.Close();
                }
            }
            return result;
        }

        public static object ExecuteScalar(string commandText, CommandType commandType, ref List<SqlParameter> parameters)
        {
            object result = null;
            if (!string.IsNullOrEmpty(commandText))
            {
                using (var cnn = new SqlConnection(Settings.GetConnectionString()))
                {
                    var cmd = cnn.CreateCommand();
                    cmd.CommandText = commandText;
                    cmd.CommandType = commandType;
                    cmd.CommandTimeout = Convert.ToInt32(Settings.GetAppCommandTimeout() ?? "3600"); ;
                    cmd.Parameters.AddRange(parameters.ToArray());                    
                    cnn.Open();
                    result = cmd.ExecuteScalar();
                    cmd.Dispose();
                    cnn.Close();
                }
            }
            return result;
        }
        public static object ExecuteScalar(string commandText, CommandType commandType)
        {
            object result = null;
            if (!string.IsNullOrEmpty(commandText))
            {
                using (var cnn = new SqlConnection(Settings.GetConnectionString()))
                {
                    var cmd = cnn.CreateCommand();
                    cmd.CommandText = commandText;
                    cmd.CommandType = commandType;
                    cmd.CommandTimeout = Convert.ToInt32(Settings.GetAppCommandTimeout() ?? "3600");                    
                    cnn.Open();
                    result = cmd.ExecuteScalar();
                    if (result.GetType() == typeof(DBNull))
                        result = null;

                    cmd.Dispose();
                    cnn.Close();
                }
            }
            return result;
        }
        public static int ExecuteNonQuery(string commandText, CommandType commandType)
        {
            int result = 0;
            if (!string.IsNullOrEmpty(commandText))
            {
                using (var cnn = new SqlConnection(Settings.GetConnectionString()))
                {
                    var cmd = cnn.CreateCommand();
                    cmd.CommandText = commandText;
                    cmd.CommandType = commandType;
                    cmd.CommandTimeout = Convert.ToInt32(Settings.GetAppCommandTimeout() ?? "3600");
                    cnn.Open();
                    result = cmd.ExecuteNonQuery();
                    cmd.Dispose();
                    cnn.Close();
                }
            }
            return result;
        }
    }
}
