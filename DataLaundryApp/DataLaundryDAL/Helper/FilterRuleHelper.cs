using DataLaundryDAL.Constants;
using DataLaundryDAL.DTO;
using DataLaundryDAL.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DataLaundryDAL.Helper
{
    public class FilterRuleHelper
    {
        #region  GetAllRule
        public static DataTableResponse GetAllRuleByFeedID(DataTableRequest oDataTableRequest, int FeedProviderId)
        {
            var dataTableResponse = new DataTableResponse();

            try
            {
                var lstSqlParameter = new List<SqlParameter>();

                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@FeedProviderId", SqlDbType = SqlDbType.Int, Value = FeedProviderId });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Offset", SqlDbType = SqlDbType.Int, Value = oDataTableRequest.PageNo });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@pageSize", SqlDbType = SqlDbType.Int, Value = oDataTableRequest.PageSize });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@sortColumnNo", SqlDbType = SqlDbType.Int, Value = oDataTableRequest.SortField });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@sortDirection", SqlDbType = SqlDbType.NVarChar, Value = oDataTableRequest.SortOrder });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@searchText", SqlDbType = SqlDbType.NVarChar, Value = oDataTableRequest.Filter });

                var ds = DBProvider.GetDataSet("GetAllRuleByFeedID", CommandType.StoredProcedure, ref lstSqlParameter);

                var lstFilterRule = new List<FilterRule>();

                foreach (DataRow row in ds?.Tables[0]?.Rows)
                {
                    var oFilterRule = new FilterRule()
                    {
                        Id = Convert.ToInt32(row["Id"]),
                        FieldId = Convert.ToInt32(row["FeedProviderId"]),
                        RuleName = Convert.ToString(row["RuleName"]),
                        IsEnable = Convert.ToBoolean(row["IsEnabled"])
                    };
                    lstFilterRule.Add(oFilterRule);
                }

                dataTableResponse.totalNumberofRecord = Convert.ToInt32(ds.Tables[1]?.Rows[0][0]);

                dataTableResponse.filteredRecord = Convert.ToInt32(ds.Tables[2]?.Rows[0][0]);

                dataTableResponse.data = lstFilterRule;
            }
            catch (Exception ex)
            {
                LogHelper.InsertErrorLogs("[DataLaundryApp]", "FilterRuleHelper : GetAllRuleByFeedID", ex.Message, ex.InnerException?.Message, ex.StackTrace);
            }

            return dataTableResponse;
        }
        #endregion

        #region Delete
        public static bool DeleteRule(int id)
        {
            var lstSqlParameter = new List<SqlParameter>();

            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@id", SqlDbType = SqlDbType.Int, Value = id });

            int rowsAffected = DBProvider.ExecuteNonQuery("Rule_Delete", CommandType.StoredProcedure, ref lstSqlParameter);

            return rowsAffected > 0;
        }
        #endregion

        #region GetALLRuleOperator
        public static List<RuleOperator> GetALLRuleOperator()
        {
            var lstSqlParameter = new List<SqlParameter>();

            var ds = DBProvider.GetDataSet("GetALLRuleOperator", CommandType.StoredProcedure, ref lstSqlParameter);

            var lstRuleOperator = new List<RuleOperator>();
            foreach (DataRow row in ds?.Tables[0]?.Rows)
            {
                var oRuleOperator = new RuleOperator()
                {
                    Id = Convert.ToInt32(row["Id"]),
                    Name = Convert.ToString(row["Name"])
                };
                lstRuleOperator.Add(oRuleOperator);
            }
            return lstRuleOperator;
        }
        #endregion

        #region RuleInsert
        public static bool InsertRule(FilterRule Model)
        {
            try
            {
                int ParentIndex = 0;
                var lstSqlParameter = new List<SqlParameter>();
                var dtFilterCriteria = new DataTable();
                var dtOperation = new DataTable();

                dtFilterCriteria.Columns.Add("FilterCriteriaId", typeof(int));
                dtFilterCriteria.Columns.Add("RuleId", typeof(int));
                dtFilterCriteria.Columns.Add("FieldMappingId", typeof(int));
                dtFilterCriteria.Columns.Add("FieldId", typeof(string));
                dtFilterCriteria.Columns.Add("OperatorId", typeof(int));
                dtFilterCriteria.Columns.Add("Value", typeof(string));
                dtFilterCriteria.Columns.Add("OperationId", typeof(int));
                dtFilterCriteria.Columns.Add("ParentId", typeof(int));

                dtOperation.Columns.Add("OperationId", typeof(int));
                dtOperation.Columns.Add("FieldId", typeof(int));
                dtOperation.Columns.Add("Value", typeof(string));
                dtOperation.Columns.Add("CurrentWord", typeof(string));
                dtOperation.Columns.Add("NewWord", typeof(string));
                dtOperation.Columns.Add("Sentance", typeof(string));
                dtOperation.Columns.Add("FirstFieldId", typeof(int));
                dtOperation.Columns.Add("SecondFieldId", typeof(int));
                dtOperation.Columns.Add("OperationTypeId", typeof(int));
                dtOperation.Columns.Add("OperationType", typeof(string));

                #region FilterCriteria
                if (Model.FilterCriteria != null && Model.FilterCriteria.Count > 0)
                {
                    foreach (var oFilterCriteria in Model.FilterCriteria)
                    {
                        if (!string.IsNullOrEmpty(oFilterCriteria.Value) || oFilterCriteria.OperatorId == 8 || oFilterCriteria.OperatorId == 9)
                        {
                            var rwFilterCriteria = dtFilterCriteria.NewRow();

                            rwFilterCriteria["FilterCriteriaId"] = oFilterCriteria.FilterCriteriaId;
                            rwFilterCriteria["FieldId"] = oFilterCriteria.FieldId;
                            rwFilterCriteria["OperatorId"] = oFilterCriteria.OperatorId;
                            rwFilterCriteria["OperationId"] = oFilterCriteria.OperationId;
                            rwFilterCriteria["Value"] = oFilterCriteria.Value;


                            dtFilterCriteria.Rows.Add(rwFilterCriteria);

                            if (oFilterCriteria.ChildFilterCriteria != null && oFilterCriteria.ChildFilterCriteria.Count > 0)
                            {
                                foreach (var oChildFilterCriteria in oFilterCriteria.ChildFilterCriteria)
                                {
                                    if (!string.IsNullOrEmpty(oChildFilterCriteria.Value))
                                    {
                                        var rwChildFilterCriteria = dtFilterCriteria.NewRow();

                                        rwChildFilterCriteria["FilterCriteriaId"] = oChildFilterCriteria.FilterCriteriaId;
                                        rwChildFilterCriteria["FieldId"] = oChildFilterCriteria.FieldId;
                                        rwChildFilterCriteria["OperatorId"] = oChildFilterCriteria.OperatorId;
                                        rwChildFilterCriteria["OperationId"] = oChildFilterCriteria.OperationId;
                                        rwChildFilterCriteria["Value"] = oChildFilterCriteria.Value;
                                        rwChildFilterCriteria["ParentId"] = ParentIndex;

                                        dtFilterCriteria.Rows.Add(rwChildFilterCriteria);
                                    }
                                }
                            }
                        }
                        ParentIndex++;
                    }
                }
                #endregion

                if (Model.OperationTypeId > 0)
                {
                    var orowOperation = dtOperation.NewRow();
                    if (Model.OperationTypeId == 1)//Value Assignment
                    {
                        orowOperation["FieldId"] = Model.OperationForValue.FieldId;
                        orowOperation["Value"] = Model.OperationForValue.Value.Trim('"');

                        dtOperation.Rows.Add(orowOperation);
                    }
                    else if (Model.OperationTypeId == 2)//Field Assignment
                    {
                        orowOperation["FieldId"] = Model.OperationForField.FieldId;
                        orowOperation["Value"] = Model.OperationForField.Value.Trim('"');

                        dtOperation.Rows.Add(orowOperation);
                    }
                    else if (Model.OperationTypeId == 3)//Keyword/Sentence Replacement
                    {
                        if (Model.KeywordSentenceReplacement != null && Model.KeywordSentenceReplacement.Count > 0)
                        {
                            foreach (var oKeyWordSentence in Model.KeywordSentenceReplacement)
                            {
                                orowOperation = dtOperation.NewRow();
                                orowOperation["FieldId"] = oKeyWordSentence.FieldId;
                                orowOperation["CurrentWord"] = oKeyWordSentence.CurrentWord.Trim('"');
                                orowOperation["NewWord"] = oKeyWordSentence.NewWord.Trim('"');

                                dtOperation.Rows.Add(orowOperation);
                            }
                        }
                    }
                    else if (Model.OperationTypeId == 4)//Remove Sentence
                    {
                        if (Model.RemoveSentence != null && Model.RemoveSentence.Count > 0)
                        {
                            foreach (var oRemoveSentence in Model.RemoveSentence)
                            {
                                orowOperation = dtOperation.NewRow();
                                orowOperation["FieldId"] = oRemoveSentence.FieldId;
                                orowOperation["Sentance"] = oRemoveSentence.Sentence.Trim('"');

                                dtOperation.Rows.Add(orowOperation);
                            }
                        }
                    }
                    else if (Model.OperationTypeId == 5)//Calculation
                    {
                        if (Model.Calculation != null && Model.Calculation.Count > 0)
                        {
                            foreach (var oCalculation in Model.Calculation)
                            {
                                orowOperation = dtOperation.NewRow();
                                orowOperation["FieldId"] = oCalculation.FieldId;
                                orowOperation["Value"] = oCalculation.OperatorId;
                                orowOperation["FirstFieldId"] = oCalculation.FirstFieldId;
                                orowOperation["SecondFieldId"] = oCalculation.SecondFieldId;
                                dtOperation.Rows.Add(orowOperation);
                            }
                        }
                    }
                    else if (Model.OperationTypeId == 6)//Remove Session
                    {
                        dtOperation.Rows.Add(orowOperation);
                    }
                }

                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Id", SqlDbType = SqlDbType.Int, Value = 0 });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@FeedProviderId", SqlDbType = SqlDbType.Int, Value = Model.Id });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@RuleName", SqlDbType = SqlDbType.NVarChar, Value = Model.RuleName });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@IsEnabled", SqlDbType = SqlDbType.Bit, Value = Model.IsEnable });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@OperationTypeId", SqlDbType = SqlDbType.Int, Value = Model.OperationTypeId });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@TTFilterCriteria", SqlDbType = SqlDbType.Structured, Value = dtFilterCriteria });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@TTOperation", SqlDbType = SqlDbType.Structured, Value = dtOperation });

                int rowsAffected = DBProvider.ExecuteNonQuery("Rules_Insert", CommandType.StoredProcedure, ref lstSqlParameter);

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                LogHelper.InsertErrorLogs("FilterRuleHelper", "InsertRule", ex.Message, ex.InnerException?.Message, ex.StackTrace);
                return false;
            }
        }
        #endregion

        #region GetAllOperationType
        public static List<OperationTypeMaster> GetAllOperationType()
        {
            var lstOperationType = new List<OperationTypeMaster>();
            try
            {
                var lstSqlParameter = new List<SqlParameter>();

                var ds = DBProvider.GetDataSet("GetAllOperationType", CommandType.StoredProcedure, ref lstSqlParameter);

                foreach (DataRow row in ds?.Tables[0]?.Rows)
                {
                    var oOperationType = new OperationTypeMaster()
                    {
                        OperationTypeId = Convert.ToInt32(row["OperationTypeId"]),
                        Name = Convert.ToString(row["Name"])
                    };
                    lstOperationType.Add(oOperationType);
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertErrorLogs("FilterRuleHelper", "GetAllOperationType", ex.Message, ex.InnerException?.Message, ex.StackTrace);
            }
            return lstOperationType;
        }
        #endregion

        #region GetAllOperator
        public static List<Operator> GetAllOperator()
        {
            var lstOperator = new List<Operator>();
            var lstSqlParameter = new List<SqlParameter>();
            try
            {
                var ds = DBProvider.GetDataSet("GetAllOperator", CommandType.StoredProcedure, ref lstSqlParameter);

                foreach (DataRow row in ds?.Tables[0]?.Rows)
                {
                    var oOperator = new Operator()
                    {
                        Id = Convert.ToInt32(row["Id"]),
                        Name = Convert.ToString(row["Name"]),
                        SupportedDataType = Convert.ToString(row["SupportedDataType"]),
                        OperatorExpression = Convert.ToString(row["OperatorExpression"])
                    };
                    lstOperator.Add(oOperator);
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertErrorLogs("FilterRuleHelper", "GetAllOperator", ex.Message, ex.InnerException?.Message, ex.StackTrace);
            }
            return lstOperator;
        }
        #endregion

        #region GetRuleDetailByID
        public static FilterRule GetRuleDetailByID(int id)
        {
            FilterRule Model = new FilterRule();
            try
            {
                var lstSqlParameter = new List<SqlParameter>();

                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Id", SqlDbType = SqlDbType.Int, Value = id });

                var ds = DBProvider.GetDataSet("GetRuleDetailByID", CommandType.StoredProcedure, ref lstSqlParameter);

                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    var oRuleRow = ds.Tables[0].Rows[0];
                    var oFilterRule = new FilterRule();
                    oFilterRule.RuleId = Convert.ToInt32(oRuleRow["Id"]);
                    oFilterRule.RuleName = Convert.ToString(oRuleRow["RuleName"]);
                    oFilterRule.IsEnable = Convert.ToBoolean(oRuleRow["IsEnabled"]);
                    oFilterRule.Id = Convert.ToInt32(oRuleRow["FeedProviderId"]);

                    if (ds.Tables[1].Rows.Count > 0)
                    {
                        var lstFilterCriteria = new List<FilterCriteria>();
                        for (int i = 0; i < ds.Tables[1].Rows.Count; i++)
                        {
                            var oRowFilterCriteria = ds.Tables[1].Rows[i];
                            var oFilterCriteria = new FilterCriteria()
                            {
                                FilterCriteriaId = Convert.ToInt32(oRowFilterCriteria["FilterCriteriaId"]),
                                ParentId = Convert.ToInt32(oRowFilterCriteria["ParentId"]),
                                FieldId = Convert.ToString(oRowFilterCriteria["FieldMappingId"]),
                                OperatorId = Convert.ToInt32(oRowFilterCriteria["OperatorId"]),
                                Value = Convert.ToString(oRowFilterCriteria["Value"]),
                                OperationId = Convert.ToInt32(oRowFilterCriteria["OperationId"]),
                                FeedKey = Convert.ToString(oRowFilterCriteria["FeedKey"]),
                                FeedKeyPath = Convert.ToString(oRowFilterCriteria["FeedKeyPath"]),
                                ActualFeedKeyPath = Convert.ToString(oRowFilterCriteria["ActualFeedKeyPath"]),
                                ColumnName = Convert.ToString(oRowFilterCriteria["ColumnName"]),
                                TableName = Convert.ToString(oRowFilterCriteria["TableName"]),
                                ColumnDataType = Convert.ToString(oRowFilterCriteria["ColumnDataType"])
                            };
                            if (Convert.ToString(oRowFilterCriteria["FeedKey"]) == null || Convert.ToString(oRowFilterCriteria["FeedKey"]) == "")
                            {
                                oFilterCriteria.IsMatch = false;
                                oFilterCriteria.FeedKey = Convert.ToString(oRowFilterCriteria["ColumnName"]);
                            }
                            lstFilterCriteria.Add(oFilterCriteria);
                        }
                        oFilterRule.FilterCriteria = lstFilterCriteria;
                    }

                    if (ds.Tables[2].Rows.Count > 0)
                    {
                        var lstOperationData = new List<OperationData>();
                        for (int i = 0; i < ds.Tables[2].Rows.Count; i++)
                        {
                            var oRowOperationData = ds.Tables[2].Rows[i];
                            var oOperationData = new OperationData()
                            {
                                OperationId = Convert.ToInt32(oRowOperationData["OperationId"]),
                                FieldId = Convert.ToInt32(oRowOperationData["FieldId"]),
                                Value = Convert.ToString(oRowOperationData["Value"]),
                                CurrentWord = Convert.ToString(oRowOperationData["CurrentWord"]),
                                NewWord = Convert.ToString(oRowOperationData["NewWord"]),
                                Sentance = Convert.ToString(oRowOperationData["Sentance"]),
                                FirstFieldId = Convert.ToInt32(oRowOperationData["FirstFieldId"]),
                                SecondFieldId = Convert.ToInt32(oRowOperationData["SecondFieldId"]),
                                OperationTypeId = Convert.ToInt32(oRowOperationData["OperationTypeId"]),
                                OperationTypeName = Convert.ToString(oRowOperationData["OperationTypeName"]),
                                FeedKey = Convert.ToString(oRowOperationData["FeedKey"]),
                                FeedKeyPath = Convert.ToString(oRowOperationData["FeedKeyPath"]),
                                ActualFeedKeyPath = Convert.ToString(oRowOperationData["ActualFeedKeyPath"]),
                                ColumnName = Convert.ToString(oRowOperationData["ColumnName"]),
                                TableName = Convert.ToString(oRowOperationData["TableName"]),
                                ColumnDataType = Convert.ToString(oRowOperationData["ColumnDataType"]),
                                TempTableName = Convert.ToString(oRowOperationData["TempTableName"]),
                                TempFeedKey = Convert.ToString(oRowOperationData["TempFeedKey"]),
                                TempFeedKeyPath = Convert.ToString(oRowOperationData["TempFeedKeyPath"]),
                                TempActualFeedKeyPath = Convert.ToString(oRowOperationData["TempActualFeedKeyPath"]),
                                TempColumnName = Convert.ToString(oRowOperationData["TempColumnName"]),
                                TempFRTableName = Convert.ToString(oRowOperationData["TempFRTableName"]),
                                TempFRFeedKey = Convert.ToString(oRowOperationData["TempFRFeedKey"]),
                                TempFRFeedKeyPath = Convert.ToString(oRowOperationData["TempFRFeedKeyPath"]),
                                TempFRActualFeedKeyPath = Convert.ToString(oRowOperationData["TempFRActualFeedKeyPath"]),
                                TempFRColumnName = Convert.ToString(oRowOperationData["TempFRColumnName"]),
                                TempSCFeedKey = Convert.ToString(oRowOperationData["TempSCFeedKey"]),
                                TempSCFeedKeyPath = Convert.ToString(oRowOperationData["TempSCFeedKeyPath"]),
                                TempSCActualFeedKeyPath = Convert.ToString(oRowOperationData["TempSCActualFeedKeyPath"]),
                                TempSCColumnName = Convert.ToString(oRowOperationData["TempSCColumnName"]),
                                TempSCTableName = Convert.ToString(oRowOperationData["TempSCTableName"])
                            };
                            lstOperationData.Add(oOperationData);
                        }

                        oFilterRule.OperationData = lstOperationData;
                    }
                    oFilterRule.Operator = GetAllOperator();
                    oFilterRule.RuleOperator = GetALLRuleOperator();
                    Model = oFilterRule;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertErrorLogs("[DataLaundryApp]", "FilterRuleHelper : GetRuleDetailByID", ex.Message, ex.InnerException?.Message, ex.StackTrace);
            }
            return Model;
        }
        #endregion

        #region Update
        public static bool UpdateRule(int id, bool IsEnable)
        {
            var lstSqlParameter = new List<SqlParameter>();

            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@id", SqlDbType = SqlDbType.Int, Value = id });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@IsEnable", SqlDbType = SqlDbType.Bit, Value = IsEnable });

            int rowsAffected = DBProvider.ExecuteNonQuery("Rule_Update", CommandType.StoredProcedure, ref lstSqlParameter);

            return rowsAffected > 0;
        }
        #endregion

        #region Auto Flush
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

        public static DataSet GetAllJsonEventData(int FeedProviderID)
        {
            DataSet ds = null;
            try
            {
                var lstSqlParameter = new List<SqlParameter>();
                var oQueryBuilder = new StringBuilder();
                string currentKeyName = "", currentdatatype = "", currentcolumnname = "", currenttable = "";
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@FeedProviderID", Value = FeedProviderID, SqlDbType = SqlDbType.NVarChar });

                var ods = DBProvider.GetDataSet("spr_GetEventByFeedProviderID", CommandType.StoredProcedure, ref lstSqlParameter);

                if (ods != null && ods.Tables.Count > 0 && ods.Tables[0].Rows.Count > 0)
                {
                    var oFilterModel = GetFilterCriteriaByFeedMappingId(FeedProviderID);
                    if (oFilterModel != null && oFilterModel.FilterCriteria.Count > 0)
                    {
                        #region Apply Rule and Filter
                        var oGroupeFilterCriteria = oFilterModel.FilterCriteria.GroupBy(x => x.RuleId).ToList();

                        var oGroupeOperation = oFilterModel.OperationData.Where(x => oGroupeFilterCriteria.Any(z => z.Key == x.RuleId)).GroupBy(x => x.RuleId).ToList();

                        foreach (var lstGroupFilterCriteria in oGroupeFilterCriteria)
                        {
                            oQueryBuilder = new StringBuilder();
                            foreach (var oFilter in lstGroupFilterCriteria)
                            {
                                currentKeyName = ""; currentdatatype = ""; currentcolumnname = ""; currenttable = "";
                                currentdatatype = oFilter.ColumnDataType;
                                currentcolumnname = oFilter.ColumnName;
                                currenttable = oFilter.TableName;
                                if (!string.IsNullOrEmpty(currenttable) && currenttable == "Event")
                                {
                                    oQueryBuilder.Append(currentcolumnname);
                                    oQueryBuilder.Append(" ");
                                    #region All Operator cheacker
                                    switch (oFilter.OperatorId)
                                    {
                                        case 1://Is less than(Date,Integer,Decimal) <
                                            oQueryBuilder.Append("< ");
                                            oQueryBuilder.Append("'" + oFilter.Value + "'");
                                            break;
                                        case 2://Is higher than(Date,Integer,Decimal) >
                                            oQueryBuilder.Append("> ");
                                            oQueryBuilder.Append("'" + oFilter.Value + "'");
                                            break;
                                        case 3://Is equals (Date,Integer,Decimal,String) ==
                                            oQueryBuilder.Append("= ");
                                            oQueryBuilder.Append("'" + oFilter.Value + "'");
                                            break;
                                        case 4://Is not equal (Date,Integer,Decimal,String) !=
                                            oQueryBuilder.Append("!= ");
                                            oQueryBuilder.Append("'" + oFilter.Value + "'");
                                            break;
                                        case 5://Starts with (String)
                                            oQueryBuilder.Append("LIKE ");
                                            oQueryBuilder.Append("'" + oFilter.Value + "%'");
                                            break;
                                        case 6://Contains (String)
                                            oQueryBuilder.Append("LIKE ");
                                            oQueryBuilder.Append("'%" + oFilter.Value + "%'");
                                            break;
                                        case 7://End with (String)
                                            oQueryBuilder.Append("LIKE ");
                                            oQueryBuilder.Append("'%" + oFilter.Value + "'");
                                            break;
                                        default:
                                            break;
                                    }
                                    #endregion
                                    oQueryBuilder.Append(" ");
                                    /*Apply RuleOperatior such as && ||*/
                                    if (lstGroupFilterCriteria.Count() > 1)
                                    {
                                        if (oFilter.RuleOperatorId > 0)
                                        {
                                            oQueryBuilder.Append((oFilter.RuleOperatorId == 1 ? "AND" : ""));
                                            oQueryBuilder.Append((oFilter.RuleOperatorId == 2 ? "OR" : ""));
                                            oQueryBuilder.Append(" ");
                                        }
                                    }
                                }
                            }
                            #region Select Event data above query prepared
                            var EventFilterData = ods.Tables[0].Select(oQueryBuilder.ToString());
                            if (EventFilterData != null && EventFilterData.Length > 0)
                            {
                                #region Operation
                                foreach (var lstGroupeOperation in oGroupeOperation.Where(x => x.Key == lstGroupFilterCriteria.Key).ToList())
                                {
                                    foreach (var oOperation in lstGroupeOperation)
                                    {
                                        if (oOperation.TableName == "Event" || oOperation.OperationTypeId == 6)
                                        {
                                            currentKeyName = ""; currentdatatype = ""; currentcolumnname = ""; currenttable = "";
                                            switch (oOperation.OperationTypeId)
                                            {
                                                case 1://Value Assignment
                                                    #region Value Assignment
                                                    currentcolumnname = oOperation.ColumnName;
                                                    foreach (var oEvent in EventFilterData)
                                                    {
                                                        //loop trough data update in event AutoFlush
                                                        Update_event((long)oEvent["id"], currentcolumnname, oOperation.Value);
                                                    }
                                                    #endregion
                                                    break;
                                                case 2://Field Assignment
                                                    #region Field Assignment
                                                    currentcolumnname = oOperation.ColumnName;
                                                    foreach (var oEvent in EventFilterData)
                                                    {
                                                        //loop trough data update in event AutoFlush                                                   
                                                        if (oEvent.Table.Columns.Contains(oOperation.TempColumnName))
                                                        {
                                                            Update_event((long)oEvent["id"], currentcolumnname, oEvent[oOperation.TempColumnName].ToString());
                                                        }
                                                    }
                                                    #endregion
                                                    break;
                                                case 3://Keyword/Sentence Replacement
                                                    #region Keyword/Sentence Replacement
                                                    currentcolumnname = oOperation.ColumnName;
                                                    foreach (var oEvent in EventFilterData)
                                                    {
                                                        var oReplaceValue = "";
                                                        //loop trough data update in event AutoFlush        
                                                        oReplaceValue = oEvent[currentcolumnname].ToString();
                                                        oReplaceValue = (!string.IsNullOrEmpty(oReplaceValue) ? oReplaceValue.Replace(oOperation.CurrentWord, oOperation.NewWord) : "");
                                                        Update_event((long)oEvent["id"], currentcolumnname, oReplaceValue);
                                                    }
                                                    #endregion
                                                    break;
                                                case 4://Remove Sentence
                                                    #region Remove Sentence
                                                    currentcolumnname = oOperation.ColumnName;
                                                    foreach (var oEvent in EventFilterData)
                                                    {
                                                        var oReplaceValue = "";
                                                        //loop trough data update in event AutoFlush        
                                                        oReplaceValue = oEvent[currentcolumnname].ToString();
                                                        oReplaceValue = (!string.IsNullOrEmpty(oReplaceValue) ? oReplaceValue.Replace(oOperation.Sentance, "") : "");
                                                        Update_event((long)oEvent["id"], currentcolumnname, oReplaceValue);
                                                    }
                                                    #endregion
                                                    break;
                                                case 5://Calculation
                                                    #region Calculation
                                                    currentcolumnname = oOperation.ColumnName;
                                                    if (oOperation.Value == "0")//Add
                                                    {
                                                        foreach (var oEvent in EventFilterData)
                                                        {
                                                            // Update_event((long)oEvent["id"], currentcolumnname, "");
                                                        }
                                                    }
                                                    else if (oOperation.Value == "1")//Sub
                                                    {
                                                        foreach (var oEvent in EventFilterData)
                                                        {
                                                            // Update_event((long)oEvent["id"], currentcolumnname, "");
                                                        }
                                                    }
                                                    else if (oOperation.Value == "2")//Mul
                                                    {
                                                        foreach (var oEvent in EventFilterData)
                                                        {
                                                            //Update_event((long)oEvent["id"], currentcolumnname, "");
                                                        }
                                                    }
                                                    else if (oOperation.Value == "3")//Div
                                                    {
                                                        foreach (var oEvent in EventFilterData)
                                                        {
                                                            // Update_event((long)oEvent["id"], currentcolumnname, "");
                                                        }
                                                    }
                                                    else if (oOperation.Value == "4")//Concat
                                                    {
                                                        foreach (var oEvent in EventFilterData)
                                                        {
                                                            //Update_event((long)oEvent["id"], currentcolumnname, "");
                                                        }
                                                    }
                                                    else { }
                                                    #endregion
                                                    break;
                                                case 6://Remove
                                                    #region Remove
                                                    foreach (var oEvent in EventFilterData)
                                                    {
                                                        //AutoFlushEvent_Delete((long)oEvent["id"]);
                                                    }
                                                    #endregion
                                                    break;
                                            }
                                        }
                                    }
                                }
                                #endregion
                            }
                            #endregion
                        }
                        #endregion
                    }
                    ds = ods;
                }
            }

            catch (Exception ex)
            {
                LogHelper.InsertErrorLogs("FilterRuleHelper", "GetAllJsonEventData", ex.Message, ex.InnerException?.Message, ex.StackTrace);
            }
            return ds;
        }

        public static Tuple<bool, int> GetAllJsonEventData_V1(int FeedProviderID)
        {
            bool oResultsate = false;
            #region Rule and Filter
            try
            {
                var oQueryBuilder = new StringBuilder();
                bool IsEventDelete = false;
                string currentKeyName = "", currentdatatype = "", currentcolumnname = "", currenttable = "";
                int oCunter = 0;
                var oFilterModel = GetFilterCriteriaByFeedMappingId(FeedProviderID);
                if (oFilterModel != null && oFilterModel.FilterCriteria.Count > 0)
                {
                    #region Apply Rule and Filter
                    var oGroupeFilterCriteria = oFilterModel.FilterCriteria.GroupBy(x => x.RuleId).ToList();

                    var oGroupeOperation = oFilterModel.OperationData.Where(x => oGroupeFilterCriteria.Any(z => z.Key == x.RuleId)).GroupBy(x => x.RuleId).ToList();

                    foreach (var lstGroupFilterCriteria in oGroupeFilterCriteria)
                    {
                        oQueryBuilder = new StringBuilder();
                        #region Filter 
                        foreach (var oFilter in lstGroupFilterCriteria)
                        {
                            currentKeyName = ""; currentdatatype = ""; currentcolumnname = ""; currenttable = "";
                            currentdatatype = oFilter.ColumnDataType;
                            currentcolumnname = oFilter.ColumnName;
                            currenttable = oFilter.TableName;
                            if (!string.IsNullOrEmpty(currenttable))
                            {
                                #region Select_V1
                                if (oCunter == 0)
                                {
                                    IsEventDelete = (currenttable == "Event" ? true : false);
                                    var lstTableSelecte = lstGroupFilterCriteria.Select(x => x.TableName).ToList();
                                    oQueryBuilder.Append("SELECT " + currenttable + ".");

                                    if (currenttable == "Event")
                                        oQueryBuilder.Append("Id ");
                                    else
                                    {
                                        oQueryBuilder.Append("Id ");
                                        oQueryBuilder.Append(",");
                                        oQueryBuilder.Append(currenttable);
                                        oQueryBuilder.Append(".");
                                        oQueryBuilder.Append("EventId");
                                    }

                                    for (int i = 0; i < lstTableSelecte.Count(); i++)
                                    {
                                        if (lstTableSelecte[i].ToString() != currenttable)
                                        {
                                            oQueryBuilder.Append(" ,");
                                            oQueryBuilder.Append(lstTableSelecte[i]);
                                            oQueryBuilder.Append(".");
                                            if (lstTableSelecte[i] == "Event")
                                                oQueryBuilder.Append("Id ");
                                            else
                                            {
                                                oQueryBuilder.Append("Id ");
                                                oQueryBuilder.Append(",");
                                                oQueryBuilder.Append(lstTableSelecte[i]);
                                                oQueryBuilder.Append(".");
                                                oQueryBuilder.Append("EventId ");
                                            }

                                        }
                                    }
                                    oQueryBuilder.Append(" FROM ");
                                    oQueryBuilder.Append(currenttable);
                                    oQueryBuilder.Append(" WITH (NOLOCK) ");
                                    var oFirstTable = currenttable;

                                    for (int i = 0; i < lstTableSelecte.Count(); i++)
                                    {
                                        if (lstTableSelecte[i].ToString() != currenttable)
                                        {
                                            oQueryBuilder.Append(" LEFT JOIN ");
                                            oQueryBuilder.Append(lstTableSelecte[i]);
                                            oQueryBuilder.Append(" ");
                                            oQueryBuilder.Append(lstTableSelecte[i]);
                                            oQueryBuilder.Append(" WITH (NOLOCK) ");
                                            oQueryBuilder.Append(" ON ");
                                            oQueryBuilder.Append(lstTableSelecte[i]);
                                            oQueryBuilder.Append((lstTableSelecte[i] == "Event" ? ".Id = " : ".EventId = "));
                                            oQueryBuilder.Append(oFirstTable);
                                            oQueryBuilder.Append((oFirstTable == "Event" ? ".Id" : ".EventId"));
                                        }
                                    }


                                    oQueryBuilder.Append(" WHERE");
                                    if (lstTableSelecte.Any(x => x == "Event"))//Set FeedProvider id for particular feed data
                                    {
                                       oQueryBuilder.Append(" FeedProviderId=" + oFilter.FeedProviderId + " AND FeedId IS NOT NULL AND ");
                                    }
                                }
                                #endregion

                                oQueryBuilder.Append(" ");
                                var oColumnName = string.Empty;
                                #region Skip for Event /Place / Physical Activity
                                oColumnName = TableField(currentcolumnname);
                                #endregion

                                if (!string.IsNullOrEmpty(oColumnName))
                                {
                                    oQueryBuilder.Append(string.Concat(currenttable, ".", oColumnName));
                                    #region All Operator cheacker
                                    switch (oFilter.OperatorId)
                                    {
                                        case 1://Is less than(Date,Integer,Decimal) <
                                            oQueryBuilder.Append(" < ");
                                            oQueryBuilder.Append("'" + oFilter.Value + "'");
                                            break;
                                        case 2://Is higher than(Date,Integer,Decimal) >
                                            oQueryBuilder.Append(" > ");
                                            oQueryBuilder.Append("'" + oFilter.Value + "'");
                                            break;
                                        case 3://Is equals (Date,Integer,Decimal,String) ==
                                            oQueryBuilder.Append(" = ");
                                            oQueryBuilder.Append("'" + oFilter.Value + "'");
                                            break;
                                        case 4://Is not equal (Date,Integer,Decimal,String) !=
                                            oQueryBuilder.Append(" != ");
                                            oQueryBuilder.Append("'" + oFilter.Value + "'");
                                            break;
                                        case 5://Starts with (String)
                                            oQueryBuilder.Append(" LIKE ");
                                            oQueryBuilder.Append("'" + oFilter.Value + "%'");
                                            break;
                                        case 6://Contains (String)
                                            oQueryBuilder.Append(" LIKE ");
                                            oQueryBuilder.Append("'%" + oFilter.Value + "%'");
                                            break;
                                        case 7://End with (String)
                                            oQueryBuilder.Append(" LIKE ");
                                            oQueryBuilder.Append("'%" + oFilter.Value + "'");
                                            break;
                                        case 8:// IS NULL
                                            oQueryBuilder.Append(" IS NULL ");
                                            break;
                                        case 9: // IS NOT NULL
                                            oQueryBuilder.Append(" IS NOT NULL ");
                                            break;
                                        default:
                                            break;
                                    }
                                    if (lstGroupFilterCriteria.Count() > 1)
                                    {
                                        if (oFilter.RuleOperatorId > 0)
                                        {
                                            oQueryBuilder.Append((oFilter.RuleOperatorId == 1 ? " AND " : ""));
                                            oQueryBuilder.Append((oFilter.RuleOperatorId == 2 ? " OR " : ""));
                                            oQueryBuilder.Append(" ");
                                        }
                                    }
                                    #endregion
                                }
                                oCunter++;
                            }
                        }
                        #endregion
                        oCunter = 0;
                        #region Operation
                        foreach (var lstGroupeOperation in oGroupeOperation.Where(x => x.Key == lstGroupFilterCriteria.Key).ToList())
                        {
                            foreach (var oOperation in lstGroupeOperation)
                            {
                                if (!string.IsNullOrEmpty(oOperation.TableName) || oOperation.OperationTypeId == 6)
                                {
                                    string PrimaryIDBasedOnTable = "", PrimaryIDValueBasedOnTable = "";
                                    PrimaryIDBasedOnTable = (oOperation.TableName == "Event" ? "Id" : "EventId");
                                    //var oIndex = lstGroupFilterCriteria.DistinctBy(x => x.TableName).ToList().FindIndex(x => x.TableName == "Event");
                                    var oIndex = lstGroupFilterCriteria.Select(x => x.TableName).Distinct().ToList().FindIndex(x => x == "Event");
                                    if (lstGroupFilterCriteria.Count() > 1)
                                    {
                                        if (lstGroupFilterCriteria.Any(x => x.TableName == "Event"))
                                            PrimaryIDValueBasedOnTable = "Id" + (oIndex > 0 ? "" + oIndex : "");
                                        else
                                            PrimaryIDValueBasedOnTable = "EventId";
                                    }
                                    else
                                    {
                                        var oTableName = lstGroupFilterCriteria.FirstOrDefault().TableName;
                                        if (oTableName == "Event")
                                            PrimaryIDValueBasedOnTable = "Id";
                                        else
                                            PrimaryIDValueBasedOnTable = "EventId";
                                    }
                                    switch (oOperation.OperationTypeId)
                                    {
                                        case 1://Value Assignment
                                            #region Value Assignment

                                            var oQuery = oQueryBuilder.ToString();
                                            var lst = new List<SqlParameter>();
                                            var oAutoData = DBProvider.GetDataSet(oQuery, CommandType.Text, ref lst);
                                            if (oAutoData != null && oAutoData.Tables.Count > 0)
                                            {
                                                foreach (DataRow oRow in oAutoData.Tables[0].Rows)
                                                {
                                                    var oColumnName = string.Empty;
                                                    currentcolumnname = oOperation.ColumnName;
                                                    #region Skip for Event /Place / Physical Activity                                                    
                                                    oColumnName = TableField(currentcolumnname);
                                                    #endregion       
                                                    if (oOperation.TableName == "Event" && (currentcolumnname.StartsWith("superEvent_") || currentcolumnname.StartsWith("subEvent_")))
                                                    {
                                                        var oInsertUpdateQuery = PreparedQuery(oOperation.TableName, oColumnName, oOperation.Value, PrimaryIDBasedOnTable, oRow[PrimaryIDValueBasedOnTable].ToString(), true);
                                                        DBProvider.ExecuteNonQuery(oInsertUpdateQuery, CommandType.Text);
                                                    }
                                                    else
                                                    {
                                                        var oInsertUpdateQuery = PreparedQuery(oOperation.TableName, oColumnName, oOperation.Value, PrimaryIDBasedOnTable, oRow[PrimaryIDValueBasedOnTable].ToString());
                                                        DBProvider.ExecuteNonQuery(oInsertUpdateQuery, CommandType.Text);
                                                    }
                                                }
                                            }
                                            #endregion
                                            break;
                                        case 2://Field Assignment
                                            #region Field Assignment
                                            var oFieldQuery = oQueryBuilder.ToString();
                                            var lstField = new List<SqlParameter>();
                                            var oFieldAutoData = DBProvider.GetDataSet(oFieldQuery, CommandType.Text, ref lstField);
                                            if (oFieldAutoData != null && oFieldAutoData.Tables.Count > 0)
                                            {
                                                foreach (DataRow oRow in oFieldAutoData.Tables[0].Rows)
                                                {
                                                    var oColumnName = string.Empty;
                                                    currentcolumnname = oOperation.TempColumnName;
                                                    #region Skip for Event /Place / Physical Activity                                                    
                                                    oColumnName = TableField(currentcolumnname);
                                                    #endregion

                                                    var oPrimaryID = (oOperation.TempTableName == "Event" ? "Id" : "EventId");
                                                    var oSelectQuery = "SELECT " + oOperation.TempTableName;
                                                    oSelectQuery += "." + oColumnName;
                                                    oSelectQuery += " FROM " + oOperation.TempTableName + " " + oOperation.TempTableName;
                                                    oSelectQuery += " WITH(NOLOCK) WHERE " + oOperation.TempTableName + "." + oPrimaryID + " = " + oRow[PrimaryIDValueBasedOnTable];

                                                    var oResult = DBProvider.ExecuteScalar(oSelectQuery, CommandType.Text);
                                                    if (oResult != null)
                                                    {
                                                        oColumnName = string.Empty;
                                                        currentcolumnname = oOperation.ColumnName;

                                                        #region Skip for Event /Place / Physical Activity
                                                        oColumnName = TableField(currentcolumnname);
                                                        #endregion
                                                        if (oOperation.TableName == "Event" && (currentcolumnname.StartsWith("superEvent_") || currentcolumnname.StartsWith("subEvent_")))
                                                        {
                                                            var oInsertUpdateQuery = PreparedQuery(oOperation.TableName, oColumnName, oResult, PrimaryIDBasedOnTable, oRow[PrimaryIDValueBasedOnTable].ToString(), true);
                                                            DBProvider.ExecuteNonQuery(oInsertUpdateQuery, CommandType.Text);
                                                        }
                                                        else
                                                        {
                                                            var oInsertUpdateQuery = PreparedQuery(oOperation.TableName, oColumnName, oResult, PrimaryIDBasedOnTable, oRow[PrimaryIDValueBasedOnTable].ToString());
                                                            DBProvider.ExecuteNonQuery(oInsertUpdateQuery, CommandType.Text);
                                                        }
                                                    }
                                                }
                                            }
                                            #endregion
                                            break;
                                        case 3://Keyword/Sentence Replacement
                                            #region Keyword/Sentence Replacement
                                            var oKeywordQuery = oQueryBuilder.ToString();
                                            var lstKeyword = new List<SqlParameter>();
                                            var oKeywordAutoData = DBProvider.GetDataSet(oKeywordQuery, CommandType.Text, ref lstKeyword);
                                            if (oKeywordAutoData != null && oKeywordAutoData.Tables.Count > 0)
                                            {
                                                foreach (DataRow oRow in oKeywordAutoData.Tables[0].Rows)
                                                {
                                                    var oColumnName = string.Empty;
                                                    currentcolumnname = oOperation.ColumnName;
                                                    #region Skip for Event /Place / Physical Activity                                                    
                                                    oColumnName = TableField(currentcolumnname);
                                                    #endregion

                                                    var oPrimaryID = (oOperation.TableName == "Event" ? "Id" : "EventId");
                                                    var oSelectQuery = "SELECT " + oOperation.TableName;
                                                    oSelectQuery += "." + oColumnName;
                                                    oSelectQuery += " FROM " + oOperation.TableName + " " + oOperation.TableName;
                                                    oSelectQuery += " WITH(NOLOCK) WHERE " + oOperation.TableName + "." + oPrimaryID + " = " + oRow[PrimaryIDValueBasedOnTable];

                                                    var oResult = DBProvider.ExecuteScalar(oSelectQuery, CommandType.Text);

                                                    if (oResult != null)
                                                    {
                                                        oResult = Convert.ToString(oResult).Replace(oOperation.CurrentWord, oOperation.NewWord);
                                                        if (oOperation.TableName == "Event" && (currentcolumnname.StartsWith("superEvent_") || currentcolumnname.StartsWith("subEvent_")))
                                                        {
                                                            var oInsertUpdateQuery = PreparedQuery(oOperation.TableName, oColumnName, oResult, PrimaryIDBasedOnTable, oRow[PrimaryIDValueBasedOnTable].ToString(), true);
                                                            DBProvider.ExecuteNonQuery(oInsertUpdateQuery, CommandType.Text);
                                                        }
                                                        else
                                                        {
                                                            var oInsertUpdateQuery = PreparedQuery(oOperation.TableName, oColumnName, oResult, PrimaryIDBasedOnTable, oRow[PrimaryIDValueBasedOnTable].ToString());
                                                            DBProvider.ExecuteNonQuery(oInsertUpdateQuery, CommandType.Text);
                                                        }
                                                    }
                                                }
                                            }
                                            #endregion
                                            break;
                                        case 4://Remove Sentence
                                            #region Remove Sentence
                                            var oRemoveQuery = oQueryBuilder.ToString();
                                            var lstRemove = new List<SqlParameter>();
                                            var oRemoveAutoData = DBProvider.GetDataSet(oRemoveQuery, CommandType.Text, ref lstRemove);
                                            if (oRemoveAutoData != null && oRemoveAutoData.Tables.Count > 0)
                                            {
                                                foreach (DataRow oRow in oRemoveAutoData.Tables[0].Rows)
                                                {
                                                    var oColumnName = string.Empty;
                                                    currentcolumnname = oOperation.ColumnName;
                                                    #region Skip for Event /Place / Physical Activity                                                   
                                                    oColumnName = TableField(currentcolumnname);
                                                    #endregion

                                                    var oPrimaryID = (oOperation.TableName == "Event" ? "Id" : "EventId");
                                                    var oSelectQuery = "SELECT " + oOperation.TableName;
                                                    oSelectQuery += "." + oColumnName;
                                                    oSelectQuery += " FROM " + oOperation.TableName + " " + oOperation.TableName;
                                                    oSelectQuery += " WITH (NOLOCK) WHERE " + oOperation.TableName + "." + oPrimaryID + " = " + oRow[PrimaryIDValueBasedOnTable];

                                                    var oResult = DBProvider.ExecuteScalar(oSelectQuery, CommandType.Text);

                                                    if (oResult != null)
                                                    {
                                                        oResult = Convert.ToString(oResult).Replace(oOperation.Sentance, "");
                                                        if (oOperation.TableName == "Event" && (currentcolumnname.StartsWith("superEvent_") || currentcolumnname.StartsWith("subEvent_")))
                                                        {
                                                            var oInsertUpdateQuery = PreparedQuery(oOperation.TableName, oColumnName, oResult, PrimaryIDBasedOnTable, oRow[PrimaryIDValueBasedOnTable].ToString(), true);
                                                            DBProvider.ExecuteNonQuery(oInsertUpdateQuery, CommandType.Text);
                                                        }
                                                        else
                                                        {
                                                            var oInsertUpdateQuery = PreparedQuery(oOperation.TableName, oColumnName, oResult, PrimaryIDBasedOnTable, oRow[PrimaryIDValueBasedOnTable].ToString());
                                                            DBProvider.ExecuteNonQuery(oInsertUpdateQuery, CommandType.Text);
                                                        }
                                                    }
                                                }
                                            }
                                            #endregion
                                            break;
                                        case 5://Calculation
                                            #region Calculation
                                            var oCalculationQuery = oQueryBuilder.ToString();
                                            var lstCalculation = new List<SqlParameter>();
                                            var oCalculationAutoData = DBProvider.GetDataSet(oCalculationQuery, CommandType.Text, ref lstCalculation);
                                            if (oCalculationAutoData != null && oCalculationAutoData.Tables.Count > 0 && oCalculationAutoData.Tables[0].Rows.Count > 0)
                                            {
                                                if (oOperation.Value == "0") //additional (int,float,decimal,datetime,timespan)
                                                {
                                                    #region additional
                                                    //if key has filed
                                                    var oCurrentKey = oOperation.ColumnName.Split('_').LastOrDefault();
                                                    if (!string.IsNullOrEmpty(oCurrentKey))
                                                    {
                                                        foreach (DataRow oRow in oCalculationAutoData.Tables[0].Rows)
                                                        {
                                                            var oColumnName = string.Empty;
                                                            currentcolumnname = oOperation.TempFRColumnName;
                                                            #region Skip for Event /Place / Physical Activity                                                            
                                                            oColumnName = TableField(currentcolumnname);
                                                            #endregion

                                                            object oValues = null;
                                                            var oFirstPrimaryID = (oOperation.TempFRTableName == "Event" ? "Id" : "EventId");
                                                            var oFirstSelectQuery = "SELECT " + oOperation.TempFRTableName;
                                                            oFirstSelectQuery += "." + oColumnName;
                                                            oFirstSelectQuery += " FROM " + oOperation.TempFRTableName + " " + oOperation.TempFRTableName;
                                                            oFirstSelectQuery += " WITH (NOLOCK) WHERE " + oOperation.TempFRTableName + "." + oFirstPrimaryID + " = " + oRow[PrimaryIDValueBasedOnTable];


                                                            oColumnName = string.Empty;
                                                            currentcolumnname = oOperation.TempSCColumnName;
                                                            #region Skip for Event /Place / Physical Activity                                                            
                                                            oColumnName = TableField(currentcolumnname);
                                                            #endregion

                                                            var oSecondPrimaryID = (oOperation.TempSCTableName == "Event" ? "Id" : "EventId");
                                                            var oSecondSelectQuery = "SELECT " + oOperation.TempSCTableName;
                                                            oSecondSelectQuery += "." + oColumnName;
                                                            oSecondSelectQuery += " FROM " + oOperation.TempSCTableName + " " + oOperation.TempSCTableName;
                                                            oSecondSelectQuery += " WITH (NOLOCK) WHERE " + oOperation.TempSCTableName + "." + oSecondPrimaryID + " = " + oRow[PrimaryIDValueBasedOnTable];

                                                            var oFirstResult = DBProvider.ExecuteScalar(oFirstSelectQuery, CommandType.Text);

                                                            var oSecondResult = DBProvider.ExecuteScalar(oSecondSelectQuery, CommandType.Text);

                                                            if (oFirstResult != null)
                                                            {
                                                                //float,date,integer
                                                                switch (oOperation.TempFRColumnDataType)
                                                                {
                                                                    case "float":
                                                                        oValues = oFirstResult;
                                                                        break;
                                                                    case "date":
                                                                        oValues = oFirstResult;
                                                                        break;
                                                                    case "integer":
                                                                        oValues = oFirstResult;
                                                                        break;
                                                                    case "string":
                                                                        oValues = oFirstResult;
                                                                        break;
                                                                    default:
                                                                        break;
                                                                }
                                                            }

                                                            if (oSecondResult != null)
                                                            {
                                                                var oOperationValues = oValues;
                                                                //float,date,integer                                                        
                                                                switch (oOperation.TempSCColumnDataType)
                                                                {
                                                                    case "float":
                                                                        oOperationValues = (float.Parse(Convert.ToString(oValues)) + float.Parse(Convert.ToString(oSecondResult)));
                                                                        break;
                                                                    case "date":
                                                                        oOperationValues = Convert.ToDateTime(oValues).Add(Settings.ConvertToTimestamp(Convert.ToDateTime(oSecondResult)));
                                                                        break;
                                                                    case "integer":
                                                                        oOperationValues = (float.Parse(Convert.ToString(oValues)) + float.Parse(Convert.ToString(oSecondResult)));
                                                                        break;
                                                                    case "string":
                                                                        if (oColumnName.Contains("duration"))
                                                                        {
                                                                            var oTimeStamp = System.Xml.XmlConvert.ToTimeSpan(Convert.ToString(oSecondResult));
                                                                            var oStartDate = DateTime.Now;
                                                                            if (DateTime.TryParse(Convert.ToString(oFirstResult), out oStartDate))
                                                                                oOperationValues = oStartDate.AddMilliseconds(oTimeStamp.TotalMilliseconds);
                                                                        }
                                                                        else
                                                                        {
                                                                            var oStartDate = DateTime.Now;
                                                                            if (DateTime.TryParse(Convert.ToString(oSecondResult), out oStartDate))
                                                                            {
                                                                                var oTimeStamp = System.Xml.XmlConvert.ToTimeSpan(Convert.ToString(oOperationValues));
                                                                                oOperationValues = oStartDate.AddMilliseconds(oTimeStamp.TotalMilliseconds);
                                                                            }
                                                                        }
                                                                        break;
                                                                    default:
                                                                        break;
                                                                }
                                                                oValues = oOperationValues;
                                                            }
                                                            oColumnName = string.Empty;
                                                            currentcolumnname = oOperation.ColumnName;
                                                            #region Skip for Event /Place / Physical Activity                                                            
                                                            oColumnName = TableField(currentcolumnname);
                                                            #endregion
                                                            if (oOperation.TableName == "Event" && (currentcolumnname.StartsWith("superEvent_") || currentcolumnname.StartsWith("subEvent_")))
                                                            {
                                                                var oInsertUpdateQuery = PreparedQuery(oOperation.TableName, oColumnName, oValues, PrimaryIDBasedOnTable, oRow[PrimaryIDValueBasedOnTable].ToString(), true);
                                                                DBProvider.ExecuteNonQuery(oInsertUpdateQuery, CommandType.Text);
                                                            }
                                                            else
                                                            {
                                                                var oInsertUpdateQuery = PreparedQuery(oOperation.TableName, oColumnName, oValues, PrimaryIDBasedOnTable, oRow[PrimaryIDValueBasedOnTable].ToString());
                                                                DBProvider.ExecuteNonQuery(oInsertUpdateQuery, CommandType.Text);
                                                            }
                                                        }
                                                    }
                                                    #endregion
                                                }
                                                else if (oOperation.Value == "1")//subtract (int,float,decimal,datetime,timespan)
                                                {
                                                    #region subtract
                                                    //if key has filed
                                                    var oCurrentKey = oOperation.ColumnName.Split('_').LastOrDefault();
                                                    if (!string.IsNullOrEmpty(oCurrentKey))
                                                    {
                                                        if (oCurrentKey.Contains("duration"))
                                                        {
                                                            TimeSpan oTimeResult = TimeSpan.FromMilliseconds(1);
                                                            #region for-duration
                                                            foreach (DataRow oRow in oCalculationAutoData.Tables[0].Rows)
                                                            {
                                                                var oColumnName = string.Empty;
                                                                currentcolumnname = oOperation.TempFRColumnName;
                                                                #region Skip for Event /Place / Physical Activity                                                                
                                                                oColumnName = TableField(currentcolumnname);
                                                                #endregion

                                                                object oValues = null;
                                                                var oFirstPrimaryID = (oOperation.TempFRTableName == "Event" ? "Id" : "EventId");
                                                                var oFirstSelectQuery = "SELECT " + oOperation.TempFRTableName;
                                                                oFirstSelectQuery += "." + oColumnName;
                                                                oFirstSelectQuery += " FROM " + oOperation.TempFRTableName + " " + oOperation.TempFRTableName;
                                                                oFirstSelectQuery += " WITH (NOLOCK) WHERE " + oOperation.TempFRTableName + "." + oFirstPrimaryID + " = " + oRow[PrimaryIDValueBasedOnTable];

                                                                oColumnName = string.Empty;
                                                                currentcolumnname = oOperation.TempSCColumnName;
                                                                #region Skip for Event /Place / Physical Activity                                                               
                                                                oColumnName = TableField(currentcolumnname);
                                                                #endregion

                                                                var oSecondPrimaryID = (oOperation.TempSCTableName == "Event" ? "Id" : "EventId");
                                                                var oSecondSelectQuery = "SELECT " + oOperation.TempSCTableName;
                                                                oSecondSelectQuery += "." + oColumnName;
                                                                oSecondSelectQuery += " FROM " + oOperation.TempSCTableName + " " + oOperation.TempSCTableName;
                                                                oSecondSelectQuery += " WITH (NOLOCK) WHERE " + oOperation.TempSCTableName + "." + oSecondPrimaryID + " = " + oRow[PrimaryIDValueBasedOnTable];

                                                                var oFirstResult = DBProvider.ExecuteScalar(oFirstSelectQuery, CommandType.Text);

                                                                var oSecondResult = DBProvider.ExecuteScalar(oSecondSelectQuery, CommandType.Text);

                                                                if (oFirstResult != null)
                                                                {
                                                                    //float,date,integer
                                                                    switch (oOperation.TempFRColumnDataType)
                                                                    {
                                                                        case "date":
                                                                            oValues = oFirstResult;
                                                                            break;
                                                                        case "string":
                                                                            DateTime oResultDate;
                                                                            if (DateTime.TryParse(oFirstResult.ToString(), out oResultDate))
                                                                            {
                                                                                oValues = oResultDate;
                                                                            }
                                                                            break;
                                                                        default:
                                                                            break;
                                                                    }
                                                                }
                                                                if (oSecondResult != null)
                                                                {
                                                                    var oOperationValues = oValues;
                                                                    //float,date,integer                                                        
                                                                    switch (oOperation.TempSCColumnDataType)
                                                                    {
                                                                        case "date":
                                                                            oTimeResult = (Convert.ToDateTime(oOperationValues).Subtract(Convert.ToDateTime(oSecondResult)));
                                                                            break;
                                                                        case "string":
                                                                            DateTime oResultDate;
                                                                            if (DateTime.TryParse(oSecondResult.ToString(), out oResultDate))
                                                                            {
                                                                                oTimeResult = (Convert.ToDateTime(oOperationValues).Subtract(oResultDate));
                                                                            }
                                                                            break;
                                                                        default:
                                                                            break;
                                                                    }
                                                                    oValues = oOperationValues;
                                                                }

                                                                var Days = oTimeResult.Days;
                                                                var Hours = oTimeResult.Hours;
                                                                var Minutes = oTimeResult.Minutes;
                                                                var Seconds = oTimeResult.Seconds;
                                                                var Milliseconds = oTimeResult.Milliseconds;

                                                                string Duration = "PT";

                                                                Duration += (Days > 0 ? Days + "D" : "");
                                                                Duration += (Hours > 0 ? Hours + "H" : "");
                                                                Duration += (Minutes > 0 ? Minutes + "M" : "");
                                                                Duration += (Seconds > 0 ? Seconds + "S" : "");
                                                                Duration += (Milliseconds > 0 ? Milliseconds + "MS" : "");

                                                                if (Duration != "PT")
                                                                {
                                                                    oColumnName = string.Empty;
                                                                    currentcolumnname = oOperation.ColumnName;
                                                                    #region Skip for Event /Place / Physical Activity                                                                    
                                                                    oColumnName = TableField(currentcolumnname);
                                                                    #endregion
                                                                    if (oOperation.TableName == "Event" && (currentcolumnname.StartsWith("superEvent_") || currentcolumnname.StartsWith("subEvent_")))
                                                                    {
                                                                        var oInsertUpdateQuery = PreparedQuery(oOperation.TableName, oColumnName, Duration, PrimaryIDBasedOnTable, oRow[PrimaryIDValueBasedOnTable].ToString(), true);
                                                                        DBProvider.ExecuteNonQuery(oInsertUpdateQuery, CommandType.Text);
                                                                    }
                                                                    else
                                                                    {
                                                                        var oInsertUpdateQuery = PreparedQuery(oOperation.TableName, oColumnName, Duration, PrimaryIDBasedOnTable, oRow[PrimaryIDValueBasedOnTable].ToString());
                                                                        DBProvider.ExecuteNonQuery(oInsertUpdateQuery, CommandType.Text);
                                                                    }
                                                                }
                                                            }
                                                            #endregion
                                                        }
                                                        else
                                                        {
                                                            #region all-field
                                                            foreach (DataRow oRow in oCalculationAutoData.Tables[0].Rows)
                                                            {
                                                                var oColumnName = string.Empty;
                                                                currentcolumnname = oOperation.TempFRColumnName;
                                                                #region Skip for Event /Place / Physical Activity                                                               
                                                                oColumnName = TableField(currentcolumnname);
                                                                #endregion
                                                                object oValues = null;
                                                                var oFirstPrimaryID = (oOperation.TempFRTableName == "Event" ? "Id" : "EventId");
                                                                var oFirstSelectQuery = "SELECT " + oOperation.TempFRTableName;
                                                                oFirstSelectQuery += "." + oColumnName;
                                                                oFirstSelectQuery += " FROM " + oOperation.TempFRTableName + " " + oOperation.TempFRTableName;
                                                                oFirstSelectQuery += " WITH (NOLOCK) WHERE " + oOperation.TempFRTableName + "." + oFirstPrimaryID + " = " + oRow[PrimaryIDValueBasedOnTable];

                                                                oColumnName = string.Empty;
                                                                currentcolumnname = oOperation.TempSCColumnName;
                                                                #region Skip for Event /Place / Physical Activity                                                                
                                                                oColumnName = TableField(currentcolumnname);
                                                                #endregion
                                                                var oSecondPrimaryID = (oOperation.TempSCTableName == "Event" ? "Id" : "EventId");
                                                                var oSecondSelectQuery = "SELECT " + oOperation.TempSCTableName;
                                                                oSecondSelectQuery += "." + oColumnName;
                                                                oSecondSelectQuery += " FROM " + oOperation.TempSCTableName + " " + oOperation.TempSCTableName;
                                                                oSecondSelectQuery += " WITH (NOLOCK) WHERE " + oOperation.TempSCTableName + "." + oSecondPrimaryID + " = " + oRow[PrimaryIDValueBasedOnTable];

                                                                var oFirstResult = DBProvider.ExecuteScalar(oFirstSelectQuery, CommandType.Text);

                                                                var oSecondResult = DBProvider.ExecuteScalar(oSecondSelectQuery, CommandType.Text);

                                                                if (oFirstResult != null)
                                                                {
                                                                    //float,date,integer
                                                                    switch (oOperation.TempFRColumnDataType)
                                                                    {
                                                                        case "float":
                                                                            oValues = oFirstResult;
                                                                            break;
                                                                        case "date":
                                                                            oValues = oFirstResult;
                                                                            break;
                                                                        case "integer":
                                                                            oValues = oFirstResult;
                                                                            break;
                                                                        case "string":
                                                                            DateTime oResultDate;
                                                                            if (DateTime.TryParse(oFirstResult.ToString(), out oResultDate))
                                                                            {
                                                                                oValues = oResultDate;
                                                                            }
                                                                            break;
                                                                        default:
                                                                            break;
                                                                    }
                                                                }

                                                                if (oSecondResult != null)
                                                                {
                                                                    var oOperationValues = oValues;
                                                                    //float,date,integer                                                        
                                                                    switch (oOperation.TempSCColumnDataType)
                                                                    {
                                                                        case "float":
                                                                            oOperationValues = (float.Parse(Convert.ToString(oValues)) - float.Parse(Convert.ToString(oSecondResult)));
                                                                            break;
                                                                        case "date":
                                                                            oOperationValues = Convert.ToDateTime(oValues).Add(-Settings.ConvertToTimestamp(Convert.ToDateTime(oSecondResult)));
                                                                            break;
                                                                        case "integer":
                                                                            oOperationValues = (float.Parse(Convert.ToString(oValues)) - float.Parse(Convert.ToString(oSecondResult)));
                                                                            break;
                                                                        case "string":
                                                                            DateTime oResultDate;
                                                                            if (DateTime.TryParse(oSecondResult.ToString(), out oResultDate))
                                                                            {
                                                                                oOperationValues = (Convert.ToDateTime(oValues).Add(-Settings.ConvertToTimestamp(Convert.ToDateTime(oResultDate))));
                                                                            }
                                                                            break;
                                                                        default:
                                                                            break;
                                                                    }
                                                                    oValues = oOperationValues;
                                                                }

                                                                oColumnName = string.Empty;
                                                                currentcolumnname = oOperation.ColumnName;
                                                                #region Skip for Event /Place / Physical Activity                                                               
                                                                oColumnName = TableField(currentcolumnname);
                                                                #endregion
                                                                if (oOperation.TableName == "Event" && (currentcolumnname.StartsWith("superEvent_") || currentcolumnname.StartsWith("subEvent_")))
                                                                {
                                                                    var oInsertUpdateQuery = PreparedQuery(oOperation.TableName, oColumnName, oValues, PrimaryIDBasedOnTable, oRow[PrimaryIDValueBasedOnTable].ToString(), true);
                                                                    DBProvider.ExecuteNonQuery(oInsertUpdateQuery, CommandType.Text);
                                                                }
                                                                else
                                                                {
                                                                    var oInsertUpdateQuery = PreparedQuery(oOperation.TableName, oColumnName, oValues, PrimaryIDBasedOnTable, oRow[PrimaryIDValueBasedOnTable].ToString());
                                                                    DBProvider.ExecuteNonQuery(oInsertUpdateQuery, CommandType.Text);
                                                                }
                                                            }
                                                            #endregion
                                                        }
                                                    }
                                                    #endregion
                                                }
                                                else if (oOperation.Value == "2")//multiplication (int,float,decimal)
                                                {
                                                    #region multiplication
                                                    //if key has filed
                                                    var oCurrentKey = oOperation.ColumnName.Split('_').LastOrDefault();
                                                    if (!string.IsNullOrEmpty(oCurrentKey))
                                                    {
                                                        foreach (DataRow oRow in oCalculationAutoData.Tables[0].Rows)
                                                        {
                                                            var oColumnName = string.Empty;
                                                            currentcolumnname = oOperation.TempFRColumnName;
                                                            #region Skip for Event /Place / Physical Activity                                                           
                                                            oColumnName = TableField(currentcolumnname);
                                                            #endregion


                                                            object oValues = null;
                                                            var oFirstPrimaryID = (oOperation.TempFRTableName == "Event" ? "Id" : "EventId");
                                                            var oFirstSelectQuery = "SELECT " + oOperation.TempFRTableName;
                                                            oFirstSelectQuery += "." + oColumnName;
                                                            oFirstSelectQuery += " FROM " + oOperation.TempFRTableName + " " + oOperation.TempFRTableName;
                                                            oFirstSelectQuery += " WITH (NOLOCK) WHERE " + oOperation.TempFRTableName + "." + oFirstPrimaryID + " = " + oRow[PrimaryIDValueBasedOnTable];

                                                            oColumnName = string.Empty;
                                                            currentcolumnname = oOperation.TempSCColumnName;
                                                            #region Skip for Event /Place / Physical Activity                                                            
                                                            oColumnName = TableField(currentcolumnname);
                                                            #endregion
                                                            var oSecondPrimaryID = (oOperation.TempSCTableName == "Event" ? "Id" : "EventId");
                                                            var oSecondSelectQuery = "SELECT " + oOperation.TempSCTableName;
                                                            oSecondSelectQuery += "." + oColumnName;
                                                            oSecondSelectQuery += " FROM " + oOperation.TempSCTableName + " " + oOperation.TempSCTableName;
                                                            oSecondSelectQuery += " WITH (NOLOCK) WHERE " + oOperation.TempSCTableName + "." + oSecondPrimaryID + " = " + oRow[PrimaryIDValueBasedOnTable];

                                                            var oFirstResult = DBProvider.ExecuteScalar(oFirstSelectQuery, CommandType.Text);

                                                            var oSecondResult = DBProvider.ExecuteScalar(oSecondSelectQuery, CommandType.Text);

                                                            if (oFirstResult != null)
                                                            {
                                                                //float,date,integer
                                                                switch (oOperation.TempFRColumnDataType)
                                                                {
                                                                    case "float":
                                                                        oValues = oFirstResult;
                                                                        break;
                                                                    case "date":
                                                                        oValues = oFirstResult;
                                                                        break;
                                                                    case "integer":
                                                                        oValues = oFirstResult;
                                                                        break;
                                                                    default:
                                                                        break;
                                                                }
                                                            }

                                                            if (oSecondResult != null)
                                                            {
                                                                var oOperationValues = oValues;
                                                                //float,date,integer                                                        
                                                                switch (oOperation.TempSCColumnDataType)
                                                                {
                                                                    case "float":
                                                                        oOperationValues = (float.Parse(Convert.ToString(oValues)) * float.Parse(Convert.ToString(oSecondResult)));
                                                                        break;
                                                                    case "date":
                                                                        //oOperationValues = Convert.ToDateTime(oValues).Add(-Settings.ConvertToTimestamp(Convert.ToDateTime(oSecondResult)));
                                                                        break;
                                                                    case "integer":
                                                                        oOperationValues = (float.Parse(Convert.ToString(oValues)) * float.Parse(Convert.ToString(oSecondResult)));
                                                                        break;
                                                                    default:
                                                                        break;
                                                                }
                                                                oValues = oOperationValues;
                                                            }

                                                            oColumnName = string.Empty;
                                                            currentcolumnname = oOperation.ColumnName;
                                                            #region Skip for Event /Place / Physical Activity                                                           
                                                            oColumnName = TableField(currentcolumnname);
                                                            #endregion
                                                            if (oOperation.TableName == "Event" && (currentcolumnname.StartsWith("superEvent_") || currentcolumnname.StartsWith("subEvent_")))
                                                            {
                                                                var oInsertUpdateQuery = PreparedQuery(oOperation.TableName, oColumnName, oValues, PrimaryIDBasedOnTable, oRow[PrimaryIDValueBasedOnTable].ToString(), true);
                                                                DBProvider.ExecuteNonQuery(oInsertUpdateQuery, CommandType.Text);
                                                            }
                                                            else
                                                            {
                                                                var oInsertUpdateQuery = PreparedQuery(oOperation.TableName, oColumnName, oValues, PrimaryIDBasedOnTable, oRow[PrimaryIDValueBasedOnTable].ToString());
                                                                DBProvider.ExecuteNonQuery(oInsertUpdateQuery, CommandType.Text);
                                                            }
                                                        }
                                                    }
                                                    #endregion
                                                }
                                                else if (oOperation.Value == "3")//division (int,float,decimal)
                                                {
                                                    #region division
                                                    //if key has filed
                                                    var oCurrentKey = oOperation.ColumnName.Split('_').LastOrDefault();
                                                    if (!string.IsNullOrEmpty(oCurrentKey))
                                                    {
                                                        foreach (DataRow oRow in oCalculationAutoData.Tables[0].Rows)
                                                        {
                                                            var oColumnName = string.Empty;
                                                            currentcolumnname = oOperation.TempFRColumnName;
                                                            #region Skip for Event /Place / Physical Activity                                                           
                                                            oColumnName = TableField(currentcolumnname);
                                                            #endregion

                                                            object oValues = null;
                                                            var oFirstPrimaryID = (oOperation.TempFRTableName == "Event" ? "Id" : "EventId");
                                                            var oFirstSelectQuery = "SELECT " + oOperation.TempFRTableName;
                                                            oFirstSelectQuery += "." + oColumnName;
                                                            oFirstSelectQuery += " FROM " + oOperation.TempFRTableName + " " + oOperation.TempFRTableName;
                                                            oFirstSelectQuery += " WITH (NOLOCK) WHERE " + oOperation.TempFRTableName + "." + oFirstPrimaryID + " = " + oRow[PrimaryIDValueBasedOnTable];

                                                            oColumnName = string.Empty;
                                                            currentcolumnname = oOperation.TempSCColumnName;
                                                            #region Skip for Event /Place / Physical Activity                                                          
                                                            oColumnName = TableField(currentcolumnname);
                                                            #endregion

                                                            var oSecondPrimaryID = (oOperation.TempSCTableName == "Event" ? "Id" : "EventId");
                                                            var oSecondSelectQuery = "SELECT " + oOperation.TempSCTableName;
                                                            oSecondSelectQuery += "." + oColumnName;
                                                            oSecondSelectQuery += " FROM " + oOperation.TempSCTableName + " " + oOperation.TempSCTableName;
                                                            oSecondSelectQuery += " WITH (NOLOCK) WHERE " + oOperation.TempSCTableName + "." + oSecondPrimaryID + " = " + oRow[PrimaryIDValueBasedOnTable];

                                                            var oFirstResult = DBProvider.ExecuteScalar(oFirstSelectQuery, CommandType.Text);

                                                            var oSecondResult = DBProvider.ExecuteScalar(oSecondSelectQuery, CommandType.Text);

                                                            if (oFirstResult != null)
                                                            {
                                                                //float,date,integer
                                                                switch (oOperation.TempFRColumnDataType)
                                                                {
                                                                    case "float":
                                                                        oValues = oFirstResult;
                                                                        break;
                                                                    case "date":
                                                                        oValues = oFirstResult;
                                                                        break;
                                                                    case "integer":
                                                                        oValues = oFirstResult;
                                                                        break;
                                                                    default:

                                                                        break;
                                                                }
                                                            }

                                                            if (oSecondResult != null)
                                                            {
                                                                var oOperationValues = oValues;
                                                                //float,date,integer                                                        
                                                                switch (oOperation.TempSCColumnDataType)
                                                                {
                                                                    case "float":
                                                                        oOperationValues = (float.Parse(Convert.ToString(oValues)) / float.Parse(Convert.ToString(oSecondResult)));
                                                                        break;
                                                                    case "date":
                                                                        //oOperationValues = Convert.ToDateTime(oValues).Add(-Settings.ConvertToTimestamp(Convert.ToDateTime(oSecondResult)));
                                                                        break;
                                                                    case "integer":
                                                                        oOperationValues = (float.Parse(Convert.ToString(oValues)) / float.Parse(Convert.ToString(oSecondResult)));
                                                                        break;
                                                                    default:
                                                                        break;
                                                                }
                                                                oValues = oOperationValues;
                                                            }

                                                            oColumnName = string.Empty;
                                                            currentcolumnname = oOperation.ColumnName;
                                                            #region Skip for Event /Place / Physical Activity                                                           
                                                            oColumnName = TableField(currentcolumnname);
                                                            #endregion
                                                            if (oOperation.TableName == "Event" && (currentcolumnname.StartsWith("superEvent_") || currentcolumnname.StartsWith("subEvent_")))
                                                            {
                                                                var oInsertUpdateQuery = PreparedQuery(oOperation.TableName, oColumnName, oValues, PrimaryIDBasedOnTable, oRow[PrimaryIDValueBasedOnTable].ToString(), true);
                                                                DBProvider.ExecuteNonQuery(oInsertUpdateQuery, CommandType.Text);
                                                            }
                                                            else
                                                            {
                                                                var oInsertUpdateQuery = PreparedQuery(oOperation.TableName, oColumnName, oValues, PrimaryIDBasedOnTable, oRow[PrimaryIDValueBasedOnTable].ToString());
                                                                DBProvider.ExecuteNonQuery(oInsertUpdateQuery, CommandType.Text);
                                                            }
                                                        }
                                                    }
                                                    #endregion
                                                }
                                                else if (oOperation.Value == "4") //concat 
                                                {
                                                    #region concat
                                                    foreach (DataRow oRow in oCalculationAutoData.Tables[0].Rows)
                                                    {
                                                        var oColumnName = string.Empty;
                                                        currentcolumnname = oOperation.TempFRColumnName;
                                                        #region Skip for Event /Place / Physical Activity                                                        
                                                        oColumnName = TableField(currentcolumnname);
                                                        #endregion

                                                        string oValues = null;
                                                        var oFirstPrimaryID = (oOperation.TempFRTableName == "Event" ? "Id" : "EventId");
                                                        var oFirstSelectQuery = "SELECT " + oOperation.TempFRTableName;
                                                        oFirstSelectQuery += "." + oColumnName;
                                                        oFirstSelectQuery += " FROM " + oOperation.TempFRTableName + " " + oOperation.TempFRTableName;
                                                        oFirstSelectQuery += " WITH (NOLOCK) WHERE " + oOperation.TempFRTableName + "." + oFirstPrimaryID + " = " + oRow[PrimaryIDValueBasedOnTable];

                                                        var oFirstResult = DBProvider.ExecuteScalar(oFirstSelectQuery, CommandType.Text);

                                                        if (oFirstResult != null)
                                                        {
                                                            oValues = string.Concat(oFirstResult);
                                                        }

                                                        oColumnName = string.Empty;
                                                        currentcolumnname = oOperation.TempSCColumnName;
                                                        #region Skip for Event /Place / Physical Activity                                                       
                                                        oColumnName = TableField(currentcolumnname);
                                                        #endregion

                                                        var oSecondPrimaryID = (oOperation.TempSCTableName == "Event" ? "Id" : "EventId");
                                                        var oSecondSelectQuery = "SELECT " + oOperation.TempSCTableName;
                                                        oSecondSelectQuery += "." + oColumnName;
                                                        oSecondSelectQuery += " FROM " + oOperation.TempSCTableName + " " + oOperation.TempSCTableName;
                                                        oSecondSelectQuery += " WITH (NOLOCK) WHERE " + oOperation.TempSCTableName + "." + oSecondPrimaryID + " = " + oRow[PrimaryIDValueBasedOnTable];

                                                        var oSecondResult = DBProvider.ExecuteScalar(oSecondSelectQuery, CommandType.Text);

                                                        if (oSecondResult != null)
                                                        {
                                                            oValues = string.Concat(oValues, " ", oSecondResult);

                                                        }

                                                        oColumnName = string.Empty;
                                                        currentcolumnname = oOperation.ColumnName;
                                                        #region Skip for Event /Place / Physical Activity                                                       
                                                        oColumnName = TableField(currentcolumnname);
                                                        #endregion
                                                        if (oOperation.TableName == "Event" && (currentcolumnname.StartsWith("superEvent_") || currentcolumnname.StartsWith("subEvent_")))
                                                        {
                                                            var oInsertUpdateQuery = PreparedQuery(oOperation.TableName, oColumnName, oValues, PrimaryIDBasedOnTable, oRow[PrimaryIDValueBasedOnTable].ToString(), true);
                                                            DBProvider.ExecuteNonQuery(oInsertUpdateQuery, CommandType.Text);
                                                        }
                                                        else
                                                        {
                                                            var oInsertUpdateQuery = PreparedQuery(oOperation.TableName, oColumnName, oValues, PrimaryIDBasedOnTable, oRow[PrimaryIDValueBasedOnTable].ToString());
                                                            DBProvider.ExecuteNonQuery(oInsertUpdateQuery, CommandType.Text);
                                                        }
                                                    }
                                                    #endregion
                                                }
                                            }
                                            #endregion
                                            break;
                                        case 6://Remove
                                            #region Remove
                                            var oRemoveSessionQuery = oQueryBuilder.ToString();
                                            var lstRemoveSession = new List<SqlParameter>();
                                            var oRemoveSessionAutoData = DBProvider.GetDataSet(oRemoveSessionQuery, CommandType.Text, ref lstRemoveSession);
                                            if (oRemoveSessionAutoData != null && oRemoveSessionAutoData.Tables.Count > 0)
                                            {
                                                if (IsEventDelete)
                                                {
                                                    var lstEventId = new List<string>();
                                                    foreach (DataRow oRow in oRemoveSessionAutoData.Tables[0].Rows)
                                                    {
                                                        lstEventId.Add(oRow["Id"].ToString());
                                                    }
                                                    AutoFlushEvent_Delete(string.Join(",", lstEventId));
                                                }
                                                else
                                                {
                                                    var lstEventId = new List<string>();
                                                    foreach (DataRow oRow in oRemoveSessionAutoData.Tables[0].Rows)
                                                    {
                                                        lstEventId.Add(oRow["EventId"].ToString());
                                                    }
                                                    AutoFlushEvent_Delete(string.Join(",", lstEventId));
                                                }
                                            }
                                            #endregion
                                            break;
                                    }
                                }
                            }
                        }
                        #endregion
                    }
                    #endregion

                    oResultsate = true;

                    if (oFilterModel.OperationData.Any(x => x.TableName == "Place"))//IF Any Place or Location chnages
                    {
                        AutoFlushPlaceUpdate(FeedProviderID);
                    }
                }
                else
                {
                    return Tuple.Create(oResultsate, 1);
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertErrorLogs("[DataLaundryApp]", "FilterRuleHelper : GetAllJsonEventData_V1", ex.Message, ex.InnerException?.Message, ex.StackTrace);
            }
            #endregion
            return Tuple.Create(oResultsate, 0);
        }

        public static void Update_event(long EventID, string ColumnName, string ColumnValue)
        {
            try
            {
                var lstSqlParameter = new List<SqlParameter>();
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@EventID", Value = EventID, SqlDbType = SqlDbType.BigInt });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@ColumnName", Value = ColumnName, SqlDbType = SqlDbType.NVarChar });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Value", Value = ColumnValue, SqlDbType = SqlDbType.NVarChar });
                DBProvider.ExecuteNonQuery("spr_Event_Update", CommandType.StoredProcedure, ref lstSqlParameter);
            }
            catch (Exception ex)
            {
                LogHelper.InsertErrorLogs("FilterRuleHelper", "Update_event", ex.Message, ex.InnerException?.Message, ex.StackTrace);
            }
        }

        public static void AutoFlushEvent_Delete(string EventIDs)
        {
            try
            {
                var lstSqlParameter = new List<SqlParameter>();
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@EventID", Value = EventIDs, SqlDbType = SqlDbType.NVarChar });
                DBProvider.ExecuteNonQuery("spr_AutoFlushEvent_Delete", CommandType.StoredProcedure, ref lstSqlParameter);
            }
            catch (Exception ex)
            {
                LogHelper.InsertErrorLogs("[DataLaundryApp]", "FilterRuleHelper : AutoFlushEvent_Delete", ex.Message, ex.InnerException?.Message, ex.StackTrace);
            }
        }

        public static void AutoFlushPlaceUpdate(int FeedProviderID)
        {
            try
            {
                var lstSqlParameter = new List<SqlParameter>();
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@FeedProviderId", Value = FeedProviderID, SqlDbType = SqlDbType.Int });
                DBProvider.ExecuteNonQuery("spr_Place_UpdateAddress", CommandType.StoredProcedure, ref lstSqlParameter);
            }
            catch (Exception ex)
            {
                LogHelper.InsertErrorLogs("[DataLaundryApp]", "FilterRuleHelper : AutoFlushPlaceUpdate", ex.Message, ex.InnerException?.Message, ex.StackTrace);
            }
        }
        #endregion

        #region GetSampleData
        public static List<FeedSampleData> GetSampleData(int FeedProviderId)
        {
            var lstFeedSampleData = new List<FeedSampleData>();
            try
            {
                var lstSqlParameter = new List<SqlParameter>();
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@FeedProviderID", Value = FeedProviderId, SqlDbType = SqlDbType.Int });
                var ds = DBProvider.GetDataTable("spr_GetSampleFeedData", CommandType.StoredProcedure, ref lstSqlParameter);

                if (ds != null && ds.Rows.Count > 0)
                {
                    foreach (DataRow oRow in ds.Rows)
                    {
                        var oSampleData = new FeedSampleData()
                        {
                            FeedProviderID = FeedProviderId,
                            ActualFeedKeyPath = Convert.ToString(oRow["ActualFeedKeyPath"]),
                            FeedKeyPath = Convert.ToString(oRow["FeedKeyPath"]),
                            ID = Convert.ToInt64(oRow["ID"])
                        };
                        lstFeedSampleData.Add(oSampleData);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertErrorLogs("[DataLaundryApp]", "FilterRuleHelper : GetSampleData", ex.Message, ex.InnerException?.Message, ex.StackTrace);
            }
            return lstFeedSampleData;
        }
        #endregion

        #region return table Key 
        public static string TableField(string currentcolumnname)
        {
            string oColumnName = string.Empty;
            currentcolumnname = currentcolumnname.ToLower();
            if (currentcolumnname == "image_url" || currentcolumnname == "superEvent_image_url" || currentcolumnname == "subEvent_image_url") //FOR Event Table
                oColumnName = "Image";
            else if (currentcolumnname == "image_thumbnail" || currentcolumnname == "superEvent_image_thumbnail" || currentcolumnname == "subEvent_image_thumbnail")
                oColumnName = "ImageThumbnail";
            else if (currentcolumnname == "activity_broader" || currentcolumnname == "subEvent_activity_broader" || currentcolumnname == "superEvent_activity_broader")//FOR PhysicalActivity
                oColumnName = "BroaderId";
            else if (currentcolumnname == "activity_narrower" || currentcolumnname == "subEvent_activity_narrower" || currentcolumnname == "superEvent_activity_narrower")
                oColumnName = "NarrowerId";
            else if (currentcolumnname == "location_geo_latitude" || currentcolumnname == "superEvent_location_geo_latitude" || currentcolumnname == "subEvent_location_geo_latitude")//FOR PLACE || LOCATION
                oColumnName = "Lat";
            else if (currentcolumnname == "location_geo_longitude" || currentcolumnname == "superEvent_location_geo_longitude" || currentcolumnname == "subEvent_location_geo_longitude")
                oColumnName = "Long";
            else if (currentcolumnname == "activity_type" || currentcolumnname == "superEvent_activity_type" || currentcolumnname == "subEvent_activity_type")
                oColumnName = "PrefLabel";
            else
                oColumnName = currentcolumnname.Split('_').LastOrDefault();

            return oColumnName;
        }
        #endregion

        #region  PreparedQuery
        public static string PreparedQuery(string TableName, string oColumnName, object oResult, string PrimaryIDBasedOnTable, string oRowValues, bool IsSubEventOrSuperEvent = false)
        {
            var oInsertUpdateQuery = "BEGIN";
            oInsertUpdateQuery += " IF NOT EXISTS(SELECT id FROM " + TableName;
            oInsertUpdateQuery += " WITH (NOLOCK)  WHERE " + PrimaryIDBasedOnTable + " = " + oRowValues;
            oInsertUpdateQuery += ")";
            oInsertUpdateQuery += "  BEGIN";
            oInsertUpdateQuery += "    INSERT INTO " + TableName + "(" + oColumnName + "," + PrimaryIDBasedOnTable + ")";
            oInsertUpdateQuery += "     VALUES('" + oResult + "'," + oRowValues + ")";
            oInsertUpdateQuery += " END";
            oInsertUpdateQuery += " ELSE";
            oInsertUpdateQuery += " BEGIN ";
            oInsertUpdateQuery += " UPDATE " + TableName + " SET " + oColumnName + " = '" + oResult + "'";
            oInsertUpdateQuery += " WHERE " + PrimaryIDBasedOnTable + " = " + oRowValues;
            if (IsSubEventOrSuperEvent)
                oInsertUpdateQuery += " OR SuperEventId = " + oRowValues;

            oInsertUpdateQuery += " END";
            oInsertUpdateQuery += " END";

            return oInsertUpdateQuery;
        }
        #endregion
    }
}
