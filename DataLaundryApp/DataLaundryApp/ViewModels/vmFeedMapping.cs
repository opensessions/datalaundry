
using System.ComponentModel.DataAnnotations;

namespace DataLaundryApp.ViewModels
{
    public class vmFeedMapping
    {
        [Required]
        public long Id { get; set; }
        [Required]
        public int FeedProviderId { get; set; }
        public long? ParentId { get; set; }
        public long? FeedMappingParentId { get; set; }
        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public bool IsCustomFeedKey { get; set; }
        [Required(ErrorMessage = "Please select any FeedKey")]
        public string FeedKey { get; set; }
        public string FeedKeyPath { get; set; }
        public string ActualFeedKeyPath { get; set; }
        public string Constraint { get; set; }
        public string ColumnDataType { get; set; }
        public long? Position { get; set; }
        public bool EffectToInteMapping { get; set; }
    }
}