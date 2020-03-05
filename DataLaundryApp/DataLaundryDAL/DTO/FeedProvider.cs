namespace DataLaundryDAL.DTO
{
    public class FeedProvider
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Source { get; set; }
        public bool IsIminConnector { get; set; }
        public FeedDataType DataType { get; set; }
        public bool EndpointUp { get; set; }
        public bool UsesPagingSpec { get; set; }
        public bool IsOpenActiveCompatible { get; set; }
        public bool IncludesCoordinates { get; set; }
        public bool HasFoundAllFieldMatches { get; set; }
        public bool IsSchedulerEnabled { get; set; }
        public bool IsFeedMappingConfirmed { get; set; }
        public string JSONTreeFileName { get; set; }
        public string SampleJSONFIleName { get; set; }
        public string JsonTreeWithDisabledKeysFileName { get; set; }
        public long TotalEvent { get; set; }
    }
}
