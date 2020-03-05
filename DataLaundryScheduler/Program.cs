using System;
using System.Net;
using DataLaundryScheduler.Helpers;
using System.Timers;

namespace DataLaundryScheduler
{
    class Program
    {
        public static void Main(string[] args)
        {
            
            ServicePointManager.SecurityProtocol = SecurityProtocolType.SystemDefault | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 ;
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

            var timer = new Timer();
            timer.Elapsed += new ElapsedEventHandler(TimerCallBackMethod);
            timer.Interval = 5 * 60 * 1000;// every 5 mins
         // timer.Interval = 1 * 60 * 60 * 1000;// every 1 hours
         // timer.Interval = 10 * 60 * 1000;// every 10 mins
            timer.Enabled = true;
            timer.AutoReset = true;
            timer.Start();
            #region Save into Eventlog
            LogHelper.InsertEventLog(null, null, null, "[DataLaundryScheduler] SchedulerHelper", "Main", "Scheduler started");
            #endregion

            SchedulerHelper.FetchFeeds_v1();
            SchedulerHelper.DeleteFeeds();
            SchedulerHelper.DeletePastOccurredEvent();  

            Console.ReadLine();
        }
		static void TimerCallBackMethod(object sender, ElapsedEventArgs  e)
        {
            SchedulerHelper.FetchFeeds_v1();
            SchedulerHelper.DeleteFeeds();
            SchedulerHelper.DeletePastOccurredEvent(); 
        }
    }
}
