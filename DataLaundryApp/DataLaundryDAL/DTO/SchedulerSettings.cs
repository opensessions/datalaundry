using System;

namespace DataLaundryDAL.DTO
{
    public class SchedulerSettings
    {
        public int Id { get; set; }
        public FeedProvider FeedProvider { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime? ExpiryDateTime { get; set; }
        public long liStartDateTime { get; set; }
        public long? liExpiryDateTime { get; set; }
        public DateTime LastExecutionDateTime { get; set; }
        public string NextPageUrlAfterExecution { get; set; }
        public string NextPageNumberAfterExecution { get; set; }
        public bool IsEnabled { get; set; }
        public int SchedulerFrequencyId { get; set; }
        public int? RecurranceInterval { get; set; }
        public string RecurranceDaysInWeek { get; set; }
        public string RecurranceMonths { get; set; }
        public string RecurranceDatesInMonth { get; set; }
        public string RecurranceWeekNos { get; set; }
        public string RecurranceDaysInWeekForMonth { get; set; }
    }

    public class SchedulerLog
    {
        public long Id { get; set; }
        public long FeedProviderId { get; set; }
        //public DateTime? StartDate { get; set; }
        //public DateTime? EndDate { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Status { get; set; }
        public long? AffectedEvents { get; set; }
    }
}
