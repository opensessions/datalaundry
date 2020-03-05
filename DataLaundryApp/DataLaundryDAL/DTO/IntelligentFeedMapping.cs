namespace DataLaundryDAL.DTO
{
    public class IntelligentFeedMapping : IntelligentMapping
    {
        public FeedMapping FeedMapping { get; set; }
        public IntelligentFeedMapping()
        {
            FeedMapping = new FeedMapping();
        }
    }
}
