namespace DataLaundryDAL.DTO
{
    public class DataTableRequest
    {
        public int PageNo { get; set; }
        public int PageSize { get; set; }
        public int SortField { get; set; }
        public string SortOrder { get; set; }
        public string Filter { get; set; }
    }

    public class DataTableResponse
    {
        public object data { get; set; }
        public int totalNumberofRecord { get; set; }
        public int filteredRecord { get; set; }
    }
}
