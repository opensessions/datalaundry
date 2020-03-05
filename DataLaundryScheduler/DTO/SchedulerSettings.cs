using System;

namespace DataLaundryScheduler.DTO
{
    class SchedulerSettings
    {
        public int Id { get; set; }
        public FeedProvider FeedProvider { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime? ExpiryDateTime { get; set; }
        public long liStartDateTime { get; set; }
        public long? liExpiryDateTime { get; set; }
        public long CurrentUtcTimestamp { get; set; }
        public DateTime LastExecutionDateTime { get; set; }
        public string NextPageUrlAfterExecution { get; set; }
        public int? NextPageNumberAfterExecution { get; set; }
        public bool IsEnabled { get; set; }
        public int SchedulerFrequencyId { get; set; }
        public int? RecurranceInterval { get; set; }
        public string RecurranceDaysInWeek { get; set; }
        public string RecurranceMonths { get; set; }
        public string RecurranceDatesInMonth { get; set; }
        public string RecurranceWeekNos { get; set; }
        public string RecurranceDaysInWeekForMonth { get; set; }
        public bool IsStarted { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsTerminated { get; set; }
        public DateTime? SchedulerLastStartTime { get; set; }
        public long? SchedulerLastStartTimeStamp { get; set; }
    }
}
