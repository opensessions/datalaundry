using DataLaundryScheduler.DTO;
using DataLaundryScheduler.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DataLaundryScheduler.Helpers
{
    class FeedConfigHelper
    {
        public static List<FeedMapping> GetFeedMappingByTableName(int feedProviderId, string tableName = null)
        {
            var lstFeedMapping = new List<FeedMapping>();
            var lstSqlParameter = new List<SqlParameter>();

            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@FeedProviderId", SqlDbType = SqlDbType.NVarChar, Value = feedProviderId });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@TableName", SqlDbType = SqlDbType.NVarChar, Value = (object)tableName ?? DBNull.Value });

            var dt = DBProvider.GetDataTable("GetFeedMappingByTableName", CommandType.StoredProcedure, ref lstSqlParameter);

            if (dt != null && dt.Rows.Count > 0)
            {
                var allRows = dt.Select();
                //get first level rows
                var rows = dt.Select("ParentId is null", "Id");

                lstFeedMapping = processGetFeedMappingRows(dt, rows);

                //foreach (DataRow row in dt.Rows)
                //{
                //    var feedMapping = new FeedMapping()
                //    {
                //        Id = Convert.ToInt32(row["Id"]),
                //        FeedProvider = new FeedProvider()
                //        {
                //            Id = Convert.ToInt32(row["FeedProviderId"])
                //        },
                //        TableName = Convert.ToString(row["TableName"]),
                //        ColumnName = Convert.ToString(row["ColumnName"]),
                //        IsCustomFeedKey = Convert.ToBoolean(row["IsCustomFeedKey"]),
                //        FeedKey = Convert.ToString(row["FeedKey"]),
                //        FeedKeyPath = Convert.ToString(row["FeedKeyPath"]),
                //        ActualFeedKeyPath = Convert.ToString(row["ActualFeedKeyPath"]),
                //        Constraint = Convert.ToString(row["Constraint"])
                //    };

                //    if (row["ParentId"] != DBNull.Value)
                //        feedMapping.ParentId = Convert.ToInt32(row["ParentId"]);

                //    lstFeedMapping.Add(feedMapping);
                //}
            }

            return lstFeedMapping;
        }

        private static List<FeedMapping> processGetFeedMappingRows(DataTable dt, DataRow[] currentRows)
        {
            List<FeedMapping> lstFeedMapping = new List<FeedMapping>();
            
            foreach (DataRow row in currentRows)
            {
                var feedMapping = new FeedMapping()
                {
                    Id = Convert.ToInt32(row["Id"]),
                    FeedProvider = new FeedProvider()
                    {
                        Id = Convert.ToInt32(row["FeedProviderId"])
                    },
                    TableName = Convert.ToString(row["TableName"]),
                    ColumnName = Convert.ToString(row["ColumnName"]),
                    IsCustomFeedKey = Convert.ToBoolean(row["IsCustomFeedKey"]),
                    FeedKey = Convert.ToString(row["FeedKey"]),
                    FeedKeyPath = Convert.ToString(row["FeedKeyPath"]),
                    ActualFeedKeyPath = Convert.ToString(row["ActualFeedKeyPath"]),
                    Constraint = Convert.ToString(row["Constraint"])
                };

                if (row["ParentId"] != DBNull.Value)
                    feedMapping.ParentId = Convert.ToInt32(row["ParentId"]);

                var rows = dt.Select("ParentId = " + feedMapping.Id, "Id");

                if (rows != null && rows.Length > 0)
                    feedMapping.Childrens = processGetFeedMappingRows(dt, rows);

                lstFeedMapping.Add(feedMapping);
            }

            return lstFeedMapping;
        }
    }
}
