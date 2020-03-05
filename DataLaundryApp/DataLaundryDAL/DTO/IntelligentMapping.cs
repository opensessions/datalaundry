using System;
namespace DataLaundryDAL.DTO
{
    public class IntelligentMapping
    {
        public long Id { get; set; }
        public long? ParentId { get; set; }
        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public string PossibleMatches { get; set; }
        public string PossibleHierarchies { get; set; }
        public string CustomCriteria { get; set; }
        public long? Position { get; set; }
        public bool IsCustomFeedKey { get; set; }
        public bool RemoveFeedKey { get; set; } = false;
    }
}
