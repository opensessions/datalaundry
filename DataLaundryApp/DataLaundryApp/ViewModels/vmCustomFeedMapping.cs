
using System.ComponentModel.DataAnnotations;

namespace DataLaundryApp.ViewModels
{
    public class vmCustomFeedMapping
    {
        public long Id { get; set; }
        public int FeedProviderId { get; set; }
        public long? ParentId { get; set; }
        //[Required]
        //public string TableName { get; set; }
        [Required]
        public string CustomKeyName { get; set; }
        [Required]
        public string FeedKeyPath { get; set; }
        //[Required]
        //public string PossibleMatches { get; set; }
    }
}