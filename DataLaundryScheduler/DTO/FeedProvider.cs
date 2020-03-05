
using System;

namespace DataLaundryScheduler.DTO
{
    [Serializable]
    class FeedProvider
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Source { get; set; }
        public bool IsIminConnector { get; set; }
        public FeedDataType DataType { get; set; }
        public bool IsUsesTimestamp { get; set; }
        public bool IsUtcTimestamp { get; set; }
        public bool IsUsesChangenumber { get; set; }
        public bool IsUsesUrlSlug { get; set; }
        public bool EndpointUp { get; set; }
        public bool UsesPagingSpec { get; set; }
        public bool IsOpenActiveCompatible { get; set; }
        public bool IncludesCoordinates { get; set; }
        public bool IsFeedMappingConfirmed { get; set; }
    }
}
