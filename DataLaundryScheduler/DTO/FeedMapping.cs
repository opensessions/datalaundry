
using DataLaundryScheduler.Helpers;
using System;
using System.Collections.Generic;

namespace DataLaundryScheduler.DTO
{
    [Serializable]
    class FeedMapping
    {
        public long Id { get; set; }
        public long? ParentId { get; set; }
        public FeedProvider FeedProvider { get; set; }
        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public bool IsCustomFeedKey { get; set; }
        public string FeedKey { get; set; }
        public string FeedKeyPath { get; set; }
        public string ActualFeedKeyPath { get; set; }
        public string FeedDataType { get; set; }
        public object FeedKeyValue { get; set; }
        public string Constraint { get; set; }

        public List<FeedMapping> Childrens { get; set; }
        public List<List<FeedMapping>> ChildrenRecords { get; set; }

        public FilterModel FilterModel { get; set; }
        public FeedMapping()
        {
            Childrens = new List<FeedMapping>();
            ChildrenRecords = new List<List<FeedMapping>>();
            FilterModel = new FilterModel();
           
        }
    }
}
