using DataLaundryDAL.DTO;
using DataLaundryDAL.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace DataLaundryDAL.Helper
{
    public class FeedProviderHelper
    {
        public static List<FeedDataType> GetFeedDataTypes()
        {
            var lstFeedDataType = new List<FeedDataType>();
            var lstSqlParameter = new List<SqlParameter>();
            var dt = DBProvider.GetDataTable("GetFeedDataTypes", CommandType.StoredProcedure, ref lstSqlParameter);

            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    var feedDataType = new FeedDataType()
                    {
                        Id = Convert.ToInt32(row["Id"]),
                        Name = Convert.ToString(row["Name"])
                    };

                    lstFeedDataType.Add(feedDataType);
                }
            }

            return lstFeedDataType;
        }
        public static DataTableResponse GetFeedProviders(DataTableRequest dataTableRequest)
        {
            var dataTableResponse = new DataTableResponse();
            var lstFeedProvider = new List<FeedProvider>();

            var lstSqlParameter = new List<SqlParameter>();

            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Offset", SqlDbType = SqlDbType.Int, Value = dataTableRequest.PageNo });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@pageSize", SqlDbType = SqlDbType.Int, Value = dataTableRequest.PageSize });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@sortColumnNo", SqlDbType = SqlDbType.Int, Value = dataTableRequest.SortField });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@sortDirection", SqlDbType = SqlDbType.NVarChar, Value = dataTableRequest.SortOrder });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@searchText", SqlDbType = SqlDbType.NVarChar, Value = dataTableRequest.Filter });

            var ds = DBProvider.GetDataSet("GetFeedProviders", CommandType.StoredProcedure, ref lstSqlParameter);

            foreach (DataRow row in ds?.Tables[0]?.Rows)
            {
                var feedProvider = new FeedProvider()
                {
                    Id = Convert.ToInt32(row["Id"]),
                    Name = Convert.ToString(row["Name"]),
                    Source = Convert.ToString(row["Source"]),
                    IsIminConnector = Convert.ToBoolean(row["IsIminConnector"]),
                    DataType = new FeedDataType()
                    {
                        Id = Convert.ToInt32(row["FeedDataTypeId"]),
                        Name = Convert.ToString(row["FeedDataTypeName"])
                    },
                    EndpointUp = Convert.ToBoolean(row["EndpointUp"]),
                    UsesPagingSpec = Convert.ToBoolean(row["UsesPagingSpec"]),
                    IsOpenActiveCompatible = Convert.ToBoolean(row["IsOpenActiveCompatible"]),
                    IncludesCoordinates = Convert.ToBoolean(row["IncludesCoordinates"]),
                    HasFoundAllFieldMatches = Convert.ToBoolean(row["HasFoundAllFieldMatches"]),
                    IsSchedulerEnabled = Convert.ToBoolean(row["IsSchedulerEnabled"]),
                    TotalEvent = Convert.ToInt64(row["TotalEvent"]),
                };

                lstFeedProvider.Add(feedProvider);
            }

            dataTableResponse.totalNumberofRecord = Convert.ToInt32(ds.Tables[1]?.Rows[0][0]);
            dataTableResponse.filteredRecord = Convert.ToInt32(ds.Tables[2]?.Rows[0][0]);

            dataTableResponse.data = lstFeedProvider;

            return dataTableResponse;
        }

        public static FeedProvider GetFeedProviderDetail(int feedProviderId)
        {
            FeedProvider feedProvider = null;
            var lstSqlParameter = new List<SqlParameter>();

            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@FeedProviderId", SqlDbType = SqlDbType.NVarChar, Value = feedProviderId });

            var dt = DBProvider.GetDataTable("GetFeedProviderDetail", CommandType.StoredProcedure, ref lstSqlParameter);

            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    feedProvider = new FeedProvider()
                    {
                        Id = Convert.ToInt32(row["Id"]),
                        Name = Convert.ToString(row["Name"]),
                        Source = Convert.ToString(row["Source"]),
                        IsIminConnector = Convert.ToBoolean(row["IsIminConnector"]),
                        DataType = new FeedDataType()
                        {
                            Id = Convert.ToInt32(row["FeedDataTypeId"]),
                            Name = Convert.ToString(row["FeedDataTypeName"])
                        },
                        EndpointUp = Convert.ToBoolean(row["EndpointUp"]),
                        UsesPagingSpec = Convert.ToBoolean(row["UsesPagingSpec"]),
                        IsOpenActiveCompatible = Convert.ToBoolean(row["IsOpenActiveCompatible"]),
                        IncludesCoordinates = Convert.ToBoolean(row["IncludesCoordinates"]),
                        IsFeedMappingConfirmed = Convert.ToBoolean(row["IsFeedMappingConfirmed"]),
                        JSONTreeFileName = Convert.ToString(row["JSONTreeFileName"]),
                        SampleJSONFIleName = Convert.ToString(row["SampleJSONFIleName"]),
                        JsonTreeWithDisabledKeysFileName = Convert.ToString(row["JsonTreeWithDisabledKeysFileName"])
                    };

                    break;
                }
            }

            return feedProvider;
        }

        public static FeedProvider GetFeedProviderDetail(string source)
        {
            FeedProvider feedProvider = null;
            var lstSqlParameter = new List<SqlParameter>();

            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Source", SqlDbType = SqlDbType.NVarChar, Value = source });

            var dt = DBProvider.GetDataTable("GetFeedProviderDetailBySource", CommandType.StoredProcedure, ref lstSqlParameter);

            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    feedProvider = new FeedProvider()
                    {
                        Id = Convert.ToInt32(row["Id"]),
                        Name = Convert.ToString(row["Name"]),
                        Source = Convert.ToString(row["Source"]),
                        IsIminConnector = Convert.ToBoolean(row["IsIminConnector"]),
                        DataType = new FeedDataType()
                        {
                            Id = Convert.ToInt32(row["FeedDataTypeId"]),
                            Name = Convert.ToString(row["FeedDataTypeName"])
                        },
                        EndpointUp = Convert.ToBoolean(row["EndpointUp"]),
                        UsesPagingSpec = Convert.ToBoolean(row["UsesPagingSpec"]),
                        IsOpenActiveCompatible = Convert.ToBoolean(row["IsOpenActiveCompatible"]),
                        IncludesCoordinates = Convert.ToBoolean(row["IncludesCoordinates"])
                    };

                    break;
                }
            }

            return feedProvider;
        }

        public static bool InsertFeedProvider(FeedProvider feedProvider, out int feedProviderId)
        {
            var lstSqlParameter = new List<SqlParameter>();

            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Name", SqlDbType = SqlDbType.NVarChar, Value = (object)feedProvider.Name ?? DBNull.Value });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Source", SqlDbType = SqlDbType.NVarChar, Value = feedProvider.Source });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@IsIminConnector", SqlDbType = SqlDbType.Bit, Value = feedProvider.IsIminConnector });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@FeedDataTypeId", SqlDbType = SqlDbType.NVarChar, Value = (object)feedProvider.DataType?.Id ?? DBNull.Value });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@EndpointUp", SqlDbType = SqlDbType.NVarChar, Value = feedProvider.EndpointUp });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@UsesPagingSpec", SqlDbType = SqlDbType.NVarChar, Value = feedProvider.UsesPagingSpec });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@IsOpenActiveCompatible", SqlDbType = SqlDbType.NVarChar, Value = feedProvider.IsOpenActiveCompatible });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@IncludesCoordinates", SqlDbType = SqlDbType.NVarChar, Value = feedProvider.IncludesCoordinates });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@FeedProviderId", SqlDbType = SqlDbType.Int, Direction = ParameterDirection.Output });

            int rowsAffected = DBProvider.ExecuteNonQuery("FeedProvider_Insert", CommandType.StoredProcedure, ref lstSqlParameter);


            var feedProviderIdParam = lstSqlParameter.Where(x => x.ParameterName == "@FeedProviderId").FirstOrDefault().Value;

            feedProviderId = feedProviderIdParam == DBNull.Value ? 0 : (int)feedProviderIdParam;

            return rowsAffected > 0;
        }

        public static bool UpdateFeedProvider_ConfirmFeedMapping(int feedProviderId, bool isFeedMappingConfirmed = false)
        {
            var lstSqlParameter = new List<SqlParameter>();

            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@FeedProviderId", SqlDbType = SqlDbType.Int, Value = feedProviderId });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@IsFeedMappingConfirmed", SqlDbType = SqlDbType.Bit, Value = isFeedMappingConfirmed });

            int rowsAffected = DBProvider.ExecuteNonQuery("FeedProvider_UpdateFeedMappingConfirmStatus", CommandType.StoredProcedure, ref lstSqlParameter);

            return rowsAffected > 0;
        }

        public static bool UpdateFeedProvider_OrderingStrategy(int feedProviderId, bool isUsesTimestamp, bool isUtcTimestamp, bool isUsesChangenumber, bool isUsesUrlSlug)
        {
            var lstSqlParameter = new List<SqlParameter>();

            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@FeedProviderId", SqlDbType = SqlDbType.Int, Value = feedProviderId });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@IsUsesTimestamp", SqlDbType = SqlDbType.Bit, Value = isUsesTimestamp });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@IsUtcTimestamp", SqlDbType = SqlDbType.Bit, Value = isUtcTimestamp });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@IsUsesChangenumber", SqlDbType = SqlDbType.Bit, Value = isUsesChangenumber });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@isUsesUrlSlug", SqlDbType = SqlDbType.Bit, Value = isUsesUrlSlug });

            int rowsAffected = DBProvider.ExecuteNonQuery("FeedProvider_UpdateOrderingStrategy", CommandType.StoredProcedure, ref lstSqlParameter);

            return rowsAffected > 0;
        }

        public static bool DeleteFeedProvider(int feedProviderId, bool isDeleted)
        {
            var lstSqlParameter = new List<SqlParameter>();

            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@FeedProviderId", SqlDbType = SqlDbType.Int, Value = feedProviderId });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@IsDeleted", SqlDbType = SqlDbType.Bit, Value = isDeleted });

            int rowsAffected = DBProvider.ExecuteNonQuery("FeedProvider_Delete", CommandType.StoredProcedure, ref lstSqlParameter);

            return rowsAffected > 0;
        }

        public static bool UpdateFeedProvider_JSONFileName(int feedProviderId, string jsonTreeFileName, string sampleJSONFIleName, string jsonTreeWithDisabledKeysFileName)
        {
            var lstSqlParameter = new List<SqlParameter>();

            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@FeedProviderId", SqlDbType = SqlDbType.Int, Value = feedProviderId });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@JSONTreeFileName", SqlDbType = SqlDbType.NVarChar, Value = jsonTreeFileName });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@SampleJSONFIleName", SqlDbType = SqlDbType.NVarChar, Value = sampleJSONFIleName });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@JsonTreeWithDisabledKeysFileName", SqlDbType = SqlDbType.NVarChar, Value = jsonTreeWithDisabledKeysFileName });

            int rowsAffected = DBProvider.ExecuteNonQuery("FeedProvider_UpdateJSONFIleName", CommandType.StoredProcedure, ref lstSqlParameter);

            return rowsAffected > 0;
        }

        public static bool IsNameAvailable(string Name, int Id)
        {
            var result = false;
            var lstSqlParameter = new List<SqlParameter>();

            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Name", SqlDbType = SqlDbType.NVarChar, Value = Name });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Id", SqlDbType = SqlDbType.Int, Value = Id });
            var oResult = DBProvider.ExecuteScalar("FeedProviderNameAvailable", CommandType.StoredProcedure, ref lstSqlParameter);
            if (oResult != null)
            {
                result = Convert.ToBoolean(oResult);
            }
            return result;
        }

        public static bool FeedProviderNameUpdate(string Name, int Id)
        {  
            var lstSqlParameter = new List<SqlParameter>();

            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Name", SqlDbType = SqlDbType.NVarChar, Value = Name });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Id", SqlDbType = SqlDbType.Int, Value = Id });
            int result = DBProvider.ExecuteNonQuery("FeedProviderNameUpdate", CommandType.StoredProcedure, ref lstSqlParameter);
            
            return result > 0;
        }
    }
}
