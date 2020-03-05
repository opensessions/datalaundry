using DataLaundryDAL.DTO;
using DataLaundryDAL.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace DataLaundryDAL.Helper
{
    public class FeedConfigHelper
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
                foreach (DataRow row in dt.Rows)
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
                        Constraint = Convert.ToString(row["Constraint"]),
                        ColumnDataType = Convert.ToString(row["ColumnDataType"])
                    };

                    if (row["ParentId"] != DBNull.Value)
                        feedMapping.ParentId = Convert.ToInt32(row["ParentId"]);

                    lstFeedMapping.Add(feedMapping);
                }
            }

            return lstFeedMapping;
        }
        
        public static FeedMapping GetFeedMappingDetail(long id)
        {
            FeedMapping feedMapping = null;
            var lstSqlParameter = new List<SqlParameter>();

            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@id", SqlDbType = SqlDbType.BigInt, Value = id });

            var dt = DBProvider.GetDataTable("GetFeedMappingDetail", CommandType.StoredProcedure, ref lstSqlParameter);

            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    feedMapping = new FeedMapping()
                    {
                        Id = Convert.ToInt32(row["Id"]),
                        FeedProvider = new FeedProvider()
                        {
                            Id = Convert.ToInt32(row["FeedProviderId"])
                        },
                        //FeedProviderId = Convert.ToInt32(row["FeedProviderId"]),
                        TableName = Convert.ToString(row["TableName"]),
                        ColumnName = Convert.ToString(row["ColumnName"]),
                        IsCustomFeedKey = Convert.ToBoolean(row["IsCustomFeedKey"]),
                        FeedKey = Convert.ToString(row["FeedKey"]),
                        FeedKeyPath = Convert.ToString(row["FeedKeyPath"]),
                        ActualFeedKeyPath = Convert.ToString(row["ActualFeedKeyPath"]),
                        Constraint = Convert.ToString(row["Constraint"])
                    };

                    break;
                }
            }

            return feedMapping;
        }

        public static DataTable GetIntelligentMappingByTableName(int? feedProviderId = null, string tableName = null)
        {
            var lstSqlParameter = new List<SqlParameter>();
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@FeedProviderId", SqlDbType = SqlDbType.Int, Value = (object)feedProviderId ?? DBNull.Value });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@TableName", SqlDbType = SqlDbType.NVarChar, Value = (object)tableName ?? DBNull.Value });
            
            var dt = DBProvider.GetDataTable("GetIntelligentMappingByTableName", CommandType.StoredProcedure, ref lstSqlParameter);

            //if (dt != null && dt.Rows.Count > 0)
            //    return dt;

            return dt;
        }

        public static List<IntelligentFeedMapping> GetIntelligentMapping(int? feedProviderId = null, string tableName = null)
        {
            var lstIntelligentFeedMapping = new List<IntelligentFeedMapping>();
            var lstSqlParameter = new List<SqlParameter>();
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@FeedProviderId", SqlDbType = SqlDbType.Int, Value = (object)feedProviderId ?? DBNull.Value });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@TableName", SqlDbType = SqlDbType.NVarChar, Value = (object)tableName ?? DBNull.Value });

            var dt = DBProvider.GetDataTable("GetIntelligentMappingByTableName", CommandType.StoredProcedure, ref lstSqlParameter);

            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    var IntelligentFeedMapping = new IntelligentFeedMapping()
                    {
                        Id = Convert.ToInt32(row["Id"]),
                        TableName = Convert.ToString(row["TableName"]),
                        ColumnName = Convert.ToString(row["ColumnName"]),
                        PossibleMatches = Convert.ToString(row["PossibleMatches"]),
                        PossibleHierarchies = Convert.ToString(row["PossibleHierarchies"]),
                        CustomCriteria = Convert.ToString(row["CustomCriteria"]),
                    };

                    if (row["ParentId"] != DBNull.Value)
                        IntelligentFeedMapping.ParentId = Convert.ToInt32(row["ParentId"]);

                    if (row["FeedMappingId"] != DBNull.Value)
                    {
                        IntelligentFeedMapping.FeedMapping = new FeedMapping()
                        {
                            Id = Convert.ToInt32(row["FeedMappingId"]),
                            TableName = Convert.ToString(row["TableName"]),
                            ColumnName = Convert.ToString(row["ColumnName"]),
                            IsCustomFeedKey = Convert.ToBoolean(row["IsCustomFeedKey"]),
                            IsDeleted = Convert.ToBoolean(row["IsDeleted"]),
                        };

                        if (row["FeedKey"] != DBNull.Value)
                        {
                            IntelligentFeedMapping.FeedMapping.ColumnDataType = Convert.ToString(row["ColumnDataType"]);
                            IntelligentFeedMapping.FeedMapping.FeedKey = Convert.ToString(row["FeedKey"]);
                            IntelligentFeedMapping.FeedMapping.FeedKeyPath = Convert.ToString(row["FeedKeyPath"]);
                            IntelligentFeedMapping.FeedMapping.ActualFeedKeyPath = Convert.ToString(row["ActualFeedKeyPath"]);
                            IntelligentFeedMapping.FeedMapping.Position = !string.IsNullOrEmpty(Convert.ToString(row["Position"])) ? Convert.ToInt16(row["Position"]) : (long?)null;
                        }

                        if (row["FeedMappingParentId"] != DBNull.Value)
                            IntelligentFeedMapping.FeedMapping.ParentId = Convert.ToInt32(row["FeedMappingParentId"]);
                    }

                    lstIntelligentFeedMapping.Add(IntelligentFeedMapping);
                }
            }

            return lstIntelligentFeedMapping;
        }

        public static List<IntelligentFeedMapping> GetIntelligentMapping_v1(int? feedProviderId = null, string tableName = null)
        {
            var lstIntelligentFeedMapping = new List<IntelligentFeedMapping>();
            var lstSqlParameter = new List<SqlParameter>();
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@FeedProviderId", SqlDbType = SqlDbType.Int, Value = (object)feedProviderId ?? DBNull.Value });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@TableName", SqlDbType = SqlDbType.NVarChar, Value = (object)tableName ?? DBNull.Value });

            var dt = DBProvider.GetDataTable("GetIntelligentMappingByTableName_v1", CommandType.StoredProcedure, ref lstSqlParameter);

            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    var IntelligentFeedMapping = new IntelligentFeedMapping()
                    {
                        Id = Convert.ToInt32(row["Id"]),
                        TableName = Convert.ToString(row["TableName"]),
                        ColumnName = Convert.ToString(row["ColumnName"]),
                        PossibleMatches = Convert.ToString(row["PossibleMatches"]),
                        PossibleHierarchies = Convert.ToString(row["PossibleHierarchies"]),
                        CustomCriteria = Convert.ToString(row["CustomCriteria"]),
                        IsCustomFeedKey = Convert.ToBoolean(row["IsCustomFeedKey"]),
                        Position = !string.IsNullOrEmpty(Convert.ToString(row["Position"])) ? Convert.ToInt16(row["Position"]) : (long?)null,
                    };

                    if (row["ParentId"] != DBNull.Value)
                        IntelligentFeedMapping.ParentId = Convert.ToInt32(row["ParentId"]);

                    if (row["FeedMappingId"] != DBNull.Value)
                    {
                        IntelligentFeedMapping.FeedMapping = new FeedMapping()
                        {
                            Id = Convert.ToInt32(row["FeedMappingId"]),
                            TableName = Convert.ToString(row["TableName"]),
                            ColumnName = Convert.ToString(row["ColumnName"]),
                            IsCustomFeedKey = Convert.ToBoolean(row["IsCustomFeedKey"]),
                            IsDeleted = Convert.ToBoolean(row["IsDeleted"]),
                            //Position = !string.IsNullOrEmpty(Convert.ToString(row["Position"])) ? Convert.ToInt16(row["Position"]) : (long?)null,
                        };

                        if (row["FeedKey"] != DBNull.Value)
                        {
                            IntelligentFeedMapping.FeedMapping.ColumnDataType = Convert.ToString(row["ColumnDataType"]);
                            IntelligentFeedMapping.FeedMapping.FeedKey = Convert.ToString(row["FeedKey"]);
                            IntelligentFeedMapping.FeedMapping.FeedKeyPath = Convert.ToString(row["FeedKeyPath"]);
                            IntelligentFeedMapping.FeedMapping.ActualFeedKeyPath = Convert.ToString(row["ActualFeedKeyPath"]);
                            IntelligentFeedMapping.FeedMapping.Position = !string.IsNullOrEmpty(Convert.ToString(row["Position"])) ? Convert.ToInt16(row["Position"]) : (long?)null;
                        }

                        if (row["FeedMappingParentId"] != DBNull.Value)
                            IntelligentFeedMapping.FeedMapping.ParentId = Convert.ToInt32(row["FeedMappingParentId"]);
                    }

                    lstIntelligentFeedMapping.Add(IntelligentFeedMapping);
                }
            }

            return lstIntelligentFeedMapping;
        }

        public static IntelligentMapping GetIntelligentMappingDetail(long id)
        {
            IntelligentMapping intelligentMapping = null;
            var lstSqlParameter = new List<SqlParameter>();
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Id", SqlDbType = SqlDbType.BigInt, Value = id });

            var dt = DBProvider.GetDataTable("GetIntelligentMappingDetail", CommandType.StoredProcedure, ref lstSqlParameter);

            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    intelligentMapping = new IntelligentMapping()
                    {
                        Id = Convert.ToInt32(row["Id"]),
                        TableName = Convert.ToString(row["TableName"]),
                        ColumnName = Convert.ToString(row["ColumnName"]),
                        PossibleMatches = Convert.ToString(row["PossibleMatches"]),
                        CustomCriteria = Convert.ToString(row["CustomCriteria"])
                    };

                    if (row["ParentId"] != DBNull.Value)
                        intelligentMapping.ParentId = Convert.ToInt32(row["ParentId"]);
                    
                }
            }

            return intelligentMapping;
        }

        public static bool InsertUpdateFeedMapping(FeedMapping feedMapping, out long feedMappingId)
        {
            var lstSqlParameter = new List<SqlParameter>();

            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@FeedProviderId", SqlDbType = SqlDbType.Int, Value = feedMapping.FeedProvider.Id });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@ParentId", SqlDbType = SqlDbType.BigInt, Value = (object)feedMapping.ParentId ?? DBNull.Value });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@TableName", SqlDbType = SqlDbType.NVarChar, Value = feedMapping.TableName });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@ColumnName", SqlDbType = SqlDbType.NVarChar, Value = feedMapping.ColumnName });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@ColumnDataType", SqlDbType = SqlDbType.NVarChar, Value = (object)feedMapping.ColumnDataType ?? DBNull.Value });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@IsCustomFeedKey", SqlDbType = SqlDbType.Bit, Value = (object)feedMapping.IsCustomFeedKey ?? DBNull.Value });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@FeedKey", SqlDbType = SqlDbType.NVarChar, Value = (object)feedMapping.FeedKey ?? DBNull.Value });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@FeedKeyPath", SqlDbType = SqlDbType.NVarChar, Value = (object)feedMapping.FeedKeyPath ?? DBNull.Value });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@ActualFeedKeyPath", SqlDbType = SqlDbType.NVarChar, Value = (object)feedMapping.ActualFeedKeyPath ?? DBNull.Value });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Constraint", SqlDbType = SqlDbType.NVarChar, Value = (object)feedMapping.Constraint ?? DBNull.Value });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Position", SqlDbType = SqlDbType.BigInt, Value = (object)feedMapping.Position ?? DBNull.Value });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@FeedMappingId", SqlDbType = SqlDbType.BigInt, Direction = ParameterDirection.InputOutput, Value = feedMapping.Id });

            int rowsAffected = DBProvider.ExecuteNonQuery("FeedMapping_InsertUpdate", CommandType.StoredProcedure, ref lstSqlParameter);

            var feedMappingIdParam = lstSqlParameter.Where(x => x.ParameterName == "@FeedMappingId").FirstOrDefault().Value;

            feedMappingId = feedMappingIdParam == DBNull.Value ? 0 : (long)feedMappingIdParam;

            return rowsAffected > 0;
        }

        public static bool InsertUpdateFeedMapping(List<FeedMapping> lstFeedMapping)
        {
            int rowsAffected = 0;
            
            if (lstFeedMapping != null && lstFeedMapping.Count > 0)
            {
                for (int i = 0; i < lstFeedMapping.Count; i++)
                {
                    var feedMapping = lstFeedMapping[i];

                    long feedMappingId;

                    bool status = InsertUpdateFeedMapping(feedMapping, out feedMappingId);
                    
                    if (status)
                    {
                        rowsAffected++;

                        var lstFeedMappingChild = lstFeedMapping.Where(x => x.ParentId != null && x.ParentId == feedMapping.Id 
                                                                        //&& x.TableName == feedMapping.TableName
                                                                        && x.ColumnName.StartsWith(feedMapping.ColumnName)).ToList();

                        //update parent's id after insert/update
                        feedMapping.Id = feedMappingId;
                        //update child's parent id to this
                        lstFeedMappingChild.ForEach(x => x.ParentId = feedMappingId);
                    }
                }
            }
            return rowsAffected > 0;
        }

        public static bool InsertUpdateIntelligentMapping(List<IntelligentMapping> lstIntelligentMapping)
        {
            int rowsAffected = 0;

            if (lstIntelligentMapping != null && lstIntelligentMapping.Count > 0)
            {
                foreach (var intelligentMapping in lstIntelligentMapping)
                {
                    bool status = InsertUpdateIntelligentMapping(intelligentMapping);
                    if (status)
                        rowsAffected++;
                }
            }
            return rowsAffected > 0;
        }

        public static bool InsertUpdateIntelligentMapping(IntelligentMapping intelligentMapping)
        {
            var lstSqlParameter = new List<SqlParameter>();

            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@ParentId", SqlDbType = SqlDbType.BigInt, Value = (object)intelligentMapping.ParentId ?? DBNull.Value });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@TableName", SqlDbType = SqlDbType.NVarChar, Value = (object)intelligentMapping.TableName ?? DBNull.Value });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@ColumnName", SqlDbType = SqlDbType.NVarChar, Value = intelligentMapping.ColumnName });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@PossibleMatches", SqlDbType = SqlDbType.NVarChar, Value = (object)intelligentMapping.PossibleMatches ?? DBNull.Value });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@PossibleHierarchies", SqlDbType = SqlDbType.NVarChar, Value = (object)intelligentMapping.PossibleHierarchies ?? DBNull.Value });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@CustomCriteria", SqlDbType = SqlDbType.NVarChar, Value = (object)intelligentMapping.CustomCriteria ?? DBNull.Value });

            int rowsAffected = DBProvider.ExecuteNonQuery("IntelligentMapping_InsertUpdate", CommandType.StoredProcedure, ref lstSqlParameter);
            return rowsAffected > 0;
        }

        public static bool InsertUpdateIntelligentMapping_v1(List<IntelligentMapping> lstIntelligentMapping)
        {
            int rowsAffected = 0;

            if (lstIntelligentMapping != null && lstIntelligentMapping.Count > 0)
            {
                foreach (var intelligentMapping in lstIntelligentMapping)
                {
                    bool status = InsertUpdateIntelligentMapping_v1(intelligentMapping);
                    if (status)
                        rowsAffected++;
                }
            }
            return rowsAffected > 0;
        }

        public static bool InsertUpdateIntelligentMapping_v1(IntelligentMapping intelligentMapping)
        {
            var lstSqlParameter = new List<SqlParameter>();

            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@ParentId", SqlDbType = SqlDbType.BigInt, Value = (object)intelligentMapping.ParentId ?? DBNull.Value });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@TableName", SqlDbType = SqlDbType.NVarChar, Value = (object)intelligentMapping.TableName ?? DBNull.Value });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@ColumnName", SqlDbType = SqlDbType.NVarChar, Value = intelligentMapping.ColumnName });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@PossibleMatches", SqlDbType = SqlDbType.NVarChar, Value = (object)intelligentMapping.PossibleMatches ?? DBNull.Value });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@PossibleHierarchies", SqlDbType = SqlDbType.NVarChar, Value = (object)intelligentMapping.PossibleHierarchies ?? DBNull.Value });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@CustomCriteria", SqlDbType = SqlDbType.NVarChar, Value = (object)intelligentMapping.CustomCriteria ?? DBNull.Value });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@IsCustomFeedKey", SqlDbType = SqlDbType.NVarChar, Value = (object)intelligentMapping.IsCustomFeedKey });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@RemoveFeedKey", SqlDbType = SqlDbType.Bit, Value = (object)intelligentMapping.RemoveFeedKey });

            int rowsAffected = DBProvider.ExecuteNonQuery("IntelligentMapping_InsertUpdate_v1", CommandType.StoredProcedure, ref lstSqlParameter);
            return rowsAffected > 0;
        }

        public static int ValidateFeedMappingColumnName(int feedProviderId, string columnName, long? id)
        {
            var lstSqlParameter = new List<SqlParameter>();
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@feedProviderId", SqlDbType = SqlDbType.Int, Value = feedProviderId });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@columnName", SqlDbType = SqlDbType.NVarChar, Value = columnName });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Id", SqlDbType = SqlDbType.BigInt, Value = (object)id ?? DBNull.Value });

            int count = (int)DBProvider.ExecuteScalar("ValidateFeedMappingColumnName", CommandType.StoredProcedure, ref lstSqlParameter);
            return count;
        }
        
        public static bool DeleteFeedMapping(long id, bool isCustomFeedKey = true, bool effectToIntemapping = false)
        {
            var lstSqlParameter = new List<SqlParameter>();

            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Id", SqlDbType = SqlDbType.BigInt, Value = id });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@IsCustomFeedKey", SqlDbType = SqlDbType.Bit, Value = isCustomFeedKey });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@EffectToInteMapping", SqlDbType = SqlDbType.Bit, Value = effectToIntemapping });

            int rowsAffected = DBProvider.ExecuteNonQuery("FeedMapping_Delete", CommandType.StoredProcedure, ref lstSqlParameter);
            return rowsAffected > 0;
        }
        
        public static bool ActivateDeactivateFeedKey(long id, bool isActive)
        {
            var lstSqlParameter = new List<SqlParameter>();

            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Id", SqlDbType = SqlDbType.BigInt, Value = id });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@IsActive", SqlDbType = SqlDbType.Bit, Value = isActive });

            int rowsAffected = DBProvider.ExecuteNonQuery("FeedMapping_ActivateDeactivateFeed", CommandType.StoredProcedure, ref lstSqlParameter);
            return rowsAffected > 0;
        }

        public static List<IntelligentFeedMapping> GetFeedIntelligentMapping(int? feedProviderId = null, string tableName = null)
        {
            var lstIntelligentFeedMapping = new List<IntelligentFeedMapping>();
            var lstSqlParameter = new List<SqlParameter>();
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@FeedProviderId", SqlDbType = SqlDbType.Int, Value = (object)feedProviderId ?? DBNull.Value });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@TableName", SqlDbType = SqlDbType.NVarChar, Value = (object)tableName ?? DBNull.Value });

            var dt = DBProvider.GetDataTable("GetFeedIntelligentMappingByTableName", CommandType.StoredProcedure, ref lstSqlParameter);

            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    var IntelligentFeedMapping = new IntelligentFeedMapping()
                    {
                        Id = Convert.ToInt32(row["Id"]),
                        TableName = Convert.ToString(row["TableName"]),
                        ColumnName = Convert.ToString(row["ColumnName"]),
                        PossibleMatches = Convert.ToString(row["PossibleMatches"]),
                        PossibleHierarchies = Convert.ToString(row["PossibleHierarchies"]),
                        CustomCriteria = Convert.ToString(row["CustomCriteria"]),
                    };

                    if (row["ParentId"] != DBNull.Value)
                        IntelligentFeedMapping.ParentId = Convert.ToInt32(row["ParentId"]);

                    if (row["FeedMappingId"] != DBNull.Value)
                    {
                        IntelligentFeedMapping.FeedMapping = new FeedMapping()
                        {
                            Id = Convert.ToInt32(row["FeedMappingId"]),
                            TableName = Convert.ToString(row["TableName"]),
                            ColumnName = Convert.ToString(row["ColumnName"]),
                            IsCustomFeedKey = Convert.ToBoolean(row["IsCustomFeedKey"]),
                            IsDeleted = Convert.ToBoolean(row["IsDeleted"]),
                        };

                        if (row["FeedKey"] != DBNull.Value)
                        {
                            IntelligentFeedMapping.FeedMapping.ColumnDataType = Convert.ToString(row["ColumnDataType"]);
                            IntelligentFeedMapping.FeedMapping.FeedKey = Convert.ToString(row["FeedKey"]);
                            IntelligentFeedMapping.FeedMapping.FeedKeyPath = Convert.ToString(row["FeedKeyPath"]);
                            IntelligentFeedMapping.FeedMapping.ActualFeedKeyPath = Convert.ToString(row["ActualFeedKeyPath"]);
                            IntelligentFeedMapping.FeedMapping.Position = !string.IsNullOrEmpty(Convert.ToString(row["Position"])) ? Convert.ToInt16(row["Position"]) : (long?)null;
                        }

                        if (row["FeedMappingParentId"] != DBNull.Value)
                            IntelligentFeedMapping.FeedMapping.ParentId = Convert.ToInt32(row["FeedMappingParentId"]);
                    }

                    lstIntelligentFeedMapping.Add(IntelligentFeedMapping);
                }
            }

            return lstIntelligentFeedMapping;
        }
        
        public static FeedMapping GetFeedMappingDetailByTableColumnName(string tableName, string columnName, int? feedProviderId = null)
        {
            FeedMapping feedMapping = null;
            var lstSqlParameter = new List<SqlParameter>();

            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@FeedProviderId", SqlDbType = SqlDbType.Int, Value = (object)feedProviderId ?? DBNull.Value });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@TableName", SqlDbType = SqlDbType.NVarChar, Value = tableName });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@ColumnName", SqlDbType = SqlDbType.NVarChar, Value = columnName });

            var dt = DBProvider.GetDataTable("GetFeedMappingDetailByTableColumnName", CommandType.StoredProcedure, ref lstSqlParameter);

            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    feedMapping = new FeedMapping()
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
                        Constraint = Convert.ToString(row["Constraint"]),
                        Position = row["Position"] != DBNull.Value ? Convert.ToInt32(row["Position"]) : (long?)null
                    };
                    break;
                }
            }
            return feedMapping;
        }

        public static List<IntelligentFeedMapping> GetIntelligentMappingForAnalyze(string tableName = null)
        {
            var lstIntelligentFeedMapping = new List<IntelligentFeedMapping>();
            var lstSqlParameter = new List<SqlParameter>();
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@TableName", SqlDbType = SqlDbType.NVarChar, Value = (object)tableName ?? DBNull.Value });

            var dt = DBProvider.GetDataTable("GetIntelligentMappingForAnalyze", CommandType.StoredProcedure, ref lstSqlParameter);

            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    var IntelligentFeedMapping = new IntelligentFeedMapping()
                    {
                        Id = Convert.ToInt32(row["Id"]),
                        TableName = Convert.ToString(row["TableName"]),
                        ColumnName = Convert.ToString(row["ColumnName"]),
                        PossibleMatches = Convert.ToString(row["PossibleMatches"]),
                        PossibleHierarchies = Convert.ToString(row["PossibleHierarchies"]),
                        CustomCriteria = Convert.ToString(row["CustomCriteria"]),
                        IsCustomFeedKey = Convert.ToBoolean(row["IsCustomFeedKey"]),
                        Position = !string.IsNullOrEmpty(Convert.ToString(row["Position"])) ? Convert.ToInt16(row["Position"]) : (long?)null,
                    };

                    if (row["ParentId"] != DBNull.Value)
                        IntelligentFeedMapping.ParentId = Convert.ToInt32(row["ParentId"]);                    

                    lstIntelligentFeedMapping.Add(IntelligentFeedMapping);
                }
            }

            return lstIntelligentFeedMapping;
        }

        public static bool DeleteCustomFeedsByFeedProvider(int feedProviderId)
        {
            var lstSqlParameter = new List<SqlParameter>();
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@FeedProviderId", SqlDbType = SqlDbType.Int, Value = feedProviderId });            
            int rowsAffected = DBProvider.ExecuteNonQuery("DeleteCustomFeedsByFeedProvider", CommandType.StoredProcedure, ref lstSqlParameter);
            return rowsAffected > 0;
        }
    }
}
