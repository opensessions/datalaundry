using DataLaundryScheduler.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace DataLaundryScheduler.Helpers
{
    class FilterRuleHelper
    {
        public static FilterModel GetFilterCriteriaByFeedMappingId(long? feedProviderId = null, long? feedMappingId = null)
        {
            var oFilterModel = new FilterModel();
            var lstFilterCriteria = new List<FilterCriteria>();
            var lstSqlParameter = new List<SqlParameter>();

            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@FeedMappingId", SqlDbType = SqlDbType.BigInt, Value = feedMappingId });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@FeedProviderId", SqlDbType = SqlDbType.BigInt, Value = feedProviderId });

            var dt = DBProvider.GetDataSet("GetFilterCriteriaByFeedMappingId", CommandType.StoredProcedure, ref lstSqlParameter);

            if (dt != null && dt.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow row in dt.Tables[0].Rows)
                {
                    var filterCriteria = new FilterCriteria()
                    {
                        FilterCriteriaId = Convert.ToInt32(row["FilterCriteriaId"]),
                        FeedProviderId = Convert.ToInt32(row["FeedProviderId"]),
                        OperatorId = Convert.ToInt32(row["OperatorId"]),
                        OperatorName = Convert.ToString(row["OperatorName"]),
                        OperatorExpression = Convert.ToString(row["OperatorExpression"]),
                        Value = Convert.ToString(row["Value"]),
                        RuleOperatorId = Convert.ToInt32(row["RuleOperatorId"]),
                        RuleOperatorName = Convert.ToString(row["RuleOperatorName"]),
                        FeedKey = Convert.ToString(row["FeedKey"]),
                        FeedKeyPath = Convert.ToString(row["FeedKeyPath"]),
                        ActualFeedKeyPath = Convert.ToString(row["ActualFeedKeyPath"]),
                        ColumnName = Convert.ToString(row["ColumnName"]),
                        TableName = Convert.ToString(row["TableName"]),
                        ColumnDataType = Convert.ToString(row["ColumnDataType"])
                    };

                    if (row["ParentId"] != DBNull.Value)
                        filterCriteria.ParentId = Convert.ToInt32(row["ParentId"]);

                    if (row["RuleId"] != DBNull.Value)
                        filterCriteria.RuleId = Convert.ToInt32(row["RuleId"]);

                    lstFilterCriteria.Add(filterCriteria);
                }

                if (dt.Tables[1].Rows.Count > 0)
                {
                    var lstOperationData = new List<OperationData>();
                    foreach (DataRow row in dt.Tables[1].Rows)
                    {
                        var oOperationData = new OperationData()
                        {
                            OperationId = Convert.ToInt32(row["OperationId"]),
                            RuleId = Convert.ToInt32(row["RuleId"]),
                            FieldId = Convert.ToInt32(row["FieldId"]),
                            Value = Regex.Unescape(Convert.ToString(row["Value"])),
                            CurrentWord = Regex.Unescape(Convert.ToString(row["CurrentWord"])),
                            NewWord = Regex.Unescape(Convert.ToString(row["NewWord"])),
                            Sentance = Regex.Unescape(Convert.ToString(row["Sentance"])),
                            OperationTypeId = Convert.ToInt32(row["OperationTypeId"]),
                            OperationTypeName = Convert.ToString(row["OperationTypeName"]),
                            FeedKey = Convert.ToString(row["FeedKey"]),
                            FeedKeyPath = Convert.ToString(row["FeedKeyPath"]),
                            ActualFeedKeyPath = Convert.ToString(row["ActualFeedKeyPath"]),
                            ColumnName = Convert.ToString(row["ColumnName"]),
                            //TableName = Convert.ToString(row["TableName"]),
                            //ColumnDataType = Convert.ToString(row["ColumnDataType"]),
                            //TempFeedKey = Convert.ToString(row["TempFeedKey"]),
                            //TempFeedKeyPath = Convert.ToString(row["TempFeedKeyPath"]),
                            //TempActualFeedKeyPath = Convert.ToString(row["TempActualFeedKeyPath"]),
                            //TempFRFeedKey = Convert.ToString(row["TempFRFeedKey"]),
                            //TempFRFeedKeyPath = Convert.ToString(row["TempFRFeedKeyPath"]),
                            //TempFRActualFeedKeyPath = Convert.ToString(row["TempFRActualFeedKeyPath"]),
                            //TempSCFeedKey = Convert.ToString(row["TempSCFeedKey"]),
                            //TempSCFeedKeyPath = Convert.ToString(row["TempSCFeedKeyPath"]),
                            //TempSCActualFeedKeyPath = Convert.ToString(row["TempSCActualFeedKeyPath"])
                            TableName = Convert.ToString(row["TableName"]),
                            ColumnDataType = Convert.ToString(row["ColumnDataType"]),
                            TempTableName = Convert.ToString(row["TempTableName"]),
                            TempFeedKey = Convert.ToString(row["TempFeedKey"]),
                            TempFeedKeyPath = Convert.ToString(row["TempFeedKeyPath"]),
                            TempActualFeedKeyPath = Convert.ToString(row["TempActualFeedKeyPath"]),
                            TempColumnName = Convert.ToString(row["TempColumnName"]),
                            TempColumnDataType = Convert.ToString(row["TempColumnDataType"]),
                            TempFRTableName = Convert.ToString(row["TempFRTableName"]),
                            TempFRFeedKey = Convert.ToString(row["TempFRFeedKey"]),
                            TempFRFeedKeyPath = Convert.ToString(row["TempFRFeedKeyPath"]),
                            TempFRActualFeedKeyPath = Convert.ToString(row["TempFRActualFeedKeyPath"]),
                            TempFRColumnName = Convert.ToString(row["TempFRColumnName"]),
                            TempFRColumnDataType = Convert.ToString(row["TempFRColumnDataType"]),
                            TempSCFeedKey = Convert.ToString(row["TempSCFeedKey"]),
                            TempSCFeedKeyPath = Convert.ToString(row["TempSCFeedKeyPath"]),
                            TempSCActualFeedKeyPath = Convert.ToString(row["TempSCActualFeedKeyPath"]),
                            TempSCColumnName = Convert.ToString(row["TempSCColumnName"]),
                            TempSCTableName = Convert.ToString(row["TempSCTableName"]),
                            TempSCColumnDataType = Convert.ToString(row["TempSCColumnDataType"])
                        };
                        lstOperationData.Add(oOperationData);
                    }
                    oFilterModel.OperationData = lstOperationData;
                }
                oFilterModel.FilterCriteria = lstFilterCriteria;
            }
            return oFilterModel;
        }
    }
}
