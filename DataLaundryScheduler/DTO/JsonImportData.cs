using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLaundryScheduler.DTO
{
    #region For AutoFlush
    public class JsonImportData
    {
        public long JsonID { get; set; }

        public long EventID { get; set; }

        public long FeedProviderID { get; set; }

        public string FeedID { get; set; }

        public string JsonData { get; set; }
    }
    #endregion
}
