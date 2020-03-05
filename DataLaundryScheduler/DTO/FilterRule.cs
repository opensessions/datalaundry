using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLaundryScheduler.Helpers
{
    class FilterRule
    {
        public int Id { get; set; }
        public int RuleId { get; set; }        
        public string RuleName { get; set; }
        public bool IsEnable { get; set; }
        public int? FieldId { get; set; }
        public int OperationTypeId { get; set; }
        public string OperationType { get; set; }
        public Operation OperationForValue { get; set; }
        public Operation OperationForField { get; set; }
        public List<FilterCriteria> FilterCriteria { get; set; }
        public List<KeywordSentenceReplacement> KeywordSentenceReplacement { get; set; }
        public List<KeywordSentenceReplacement> RemoveSentence { get; set; }
        public List<Calculation> Calculation { get; set; }
        public List<OperationData> OperationData { get; set; }
        public List<Operator> Operator { get; set; }
        public List<RuleOperator> RuleOperator { get; set; }
        public FilterRule()
        {
            OperationData = new List<OperationData>();
            Operator = new List<Operator>();
            RuleOperator = new List<RuleOperator>();
        }
    }

    [Serializable]
    public class FilterCriteria
    {
        public int FilterCriteriaId { get; set; }
        public int RuleId { get; set; }
        public int FeedProviderId { get; set; }
        public int? ParentId { get; set; }
        public string FieldId { get; set; }
        public string FieldName { get; set; }
        public int OperatorId { get; set; }
        public string OperatorExpression { get; set; }
        public string OperatorName { get; set; }
        public int RuleOperatorId { get; set; }
        public string RuleOperatorName { get; set; }
        public string Value { get; set; }
        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public string ColumnDataType { get; set; }
        public string FeedKey { get; set; }
        public string FeedKeyPath { get; set; }
        public string ActualFeedKeyPath { get; set; }
        public List<FilterCriteria> ChildFilterCriteria { get; set; }
        public FilterCriteria()
        {
            ChildFilterCriteria = new List<FilterCriteria>();
        }
    }

    public class RuleOperator
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class Operation
    {
        public string FieldId { get; set; }
        public string Value { get; set; }
        public bool IsSessionRemove { get; set; }
    }

    public class KeywordSentenceReplacement
    {
        public string FieldId { get; set; }
        public string CurrentWord { get; set; }
        public string NewWord { get; set; }
        public string Sentence { get; set; }
    }

    public class Calculation
    {
        public string FieldId { get; set; }
        public string FirstFieldId { get; set; }
        public string OperatorId { get; set; }
        public string OperatorName { get; set; }
        public string SecondFieldId { get; set; }
    }

    public class OperationTypeMaster
    {
        public int OperationTypeId { get; set; }
        public string Name { get; set; }
    }

    public class Operator
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string SupportedDataType { get; set; }
        public string OperatorExpression { get; set; }
    }

    [Serializable]
    public class OperationData
    {
        public int OperationId { get; set; }
        public int RuleId { get; set; }
        public int FeedProviderId { get; set; }
        public int FieldId { get; set; }
        public string Value { get; set; }
        public string CurrentWord { get; set; }
        public string NewWord { get; set; }
        public string Sentance { get; set; }
        public int FirstFieldId { get; set; }
        public int SecondFieldId { get; set; }
        public int OperationTypeId { get; set; }
        public string OperationTypeName { get; set; }
        public string FeedKey { get; set; }
        public string FeedKeyPath { get; set; }
        public string ActualFeedKeyPath { get; set; }
        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public string ColumnDataType { get; set; }
        public string TempTableName { get; set; }
        public string TempColumnName { get; set; }
        public string TempFeedKey { get; set; }
        public string TempFeedKeyPath { get; set; }
        public string TempActualFeedKeyPath { get; set; }
        public string TempColumnDataType { get; set; }
        public string TempFRFeedKey { get; set; }
        public string TempFRTableName { get; set; }
        public string TempFRColumnName { get; set; }
        public string TempFRFeedKeyPath { get; set; }
        public string TempFRActualFeedKeyPath { get; set; }
        public string TempFRColumnDataType { get; set; }
        public string TempSCFeedKey { get; set; }
        public string TempSCTableName { get; set; }
        public string TempSCColumnName { get; set; }
        public string TempSCFeedKeyPath { get; set; }
        public string TempSCActualFeedKeyPath { get; set; }
        public string TempSCColumnDataType { get; set; }
        //public string ColumnDataType { get; set; }
        //public string TempFeedKey { get; set; }
        //public string TempFeedKeyPath { get; set; }
        //public string TempActualFeedKeyPath { get; set; }
        //public string TempFRFeedKey { get; set; }
        //public string TempFRFeedKeyPath { get; set; }
        //public string TempFRActualFeedKeyPath { get; set; }
        //public string TempSCFeedKey { get; set; }
        //public string TempSCFeedKeyPath { get; set; }
        //public string TempSCActualFeedKeyPath { get; set; }

    }

    [Serializable]
    public class FilterModel
    {
        public List<FilterCriteria> FilterCriteria { get; set; }
        public List<OperationData> OperationData { get; set; }
        public FilterModel()
        {
            FilterCriteria = new List<FilterCriteria>();
            OperationData = new List<OperationData>();
        }
    }

    public class ValidData
    {
        public object Value { get; set; }
        public string RuleOperatiorName { get; set; }
        public int RuleOperatiorId { get; set; }
        public bool IsValid { get; set; }
        public int Counter { get; set; }
    }

    public enum OperationType
    {
        ValueAssignment = 1,
        FieldAssignment = 2,
        KeywordOrSentenceReplacement = 3,
        RemoveSentence = 4,
        Calculation = 5,
        RemoveSession = 6
    }
}
