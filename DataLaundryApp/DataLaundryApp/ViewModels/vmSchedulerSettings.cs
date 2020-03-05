using DataLaundryDAL.DTO;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace DataLaundryApp.ViewModels
{
    public class vmSchedulerSettings
    {
        public FeedProvider FeedProvider { get; set; }

        public int Id { get; set; }
        public int FeedProviderId { get; set; }
        public string strStartDateTime { get; set; }
        public string strExpiryDateTime { get; set; }
        [Required]
        public long liStartDateTime { get; set; }
        public long? liExpiryDateTime { get; set; }
        [Required]
        public int SchedulerFrequencyId { get; set; }
        public int? RecurranceIntervalHours { get; set; }
        public int? RecurranceIntervalDays { get; set; }
        public int? RecurranceIntervalWeeks { get; set; }
        public bool IsEnabled { get; set; }
        public string RecurranceDaysInWeek { get; set; }
        public string RecurranceMonths { get; set; }
        public string RecurranceDatesInMonth { get; set; }
        public string RecurranceWeekNos { get; set; }
        public string RecurranceDaysInWeekForMonth { get; set; }
        public IList<string> SelectedDaysInWeek { get; set; }
        public IList<string> SelectedMonths { get; set; }
        public IList<string> SelectedDatesInMonth { get; set; }
        public IList<string> SelectedWeekNos { get; set; }
        public IList<string> SelectedDaysInWeekForMonth { get; set; }
        public bool IsDatesSelectedInMonth
        {
            get
            {
                return this.SchedulerFrequencyId == 5 && (this.SelectedDatesInMonth != null && this.SelectedDatesInMonth.Count > 0);
            }
        }
        public List<MasterData> RecurranceDaysInWeekSelectList { get; set; }
        public IEnumerable<SelectListItem> RecurranceMonthsSelectList { get; set; }
        public IEnumerable<SelectListItem> RecurranceDatesInMonthsSelectList { get; set; }
        public IEnumerable<SelectListItem> RecurranceWeekNosSelectList { get; set; }
        public IEnumerable<SelectListItem> RecurranceDaysInWeekForMonthSelectList { get; set; }
    }
}