using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Threading.Tasks;
using DataLaundryDAL.Utilities;
using DataLaundryDAL.Helper;

namespace DataLaundryApp.BackgroundJob
{
    public class ScheduledJob : IJob
    {
        private readonly IConfiguration configuration;

        private readonly ILogger<ScheduledJob> logger;

        public ScheduledJob(IConfiguration configuration, ILogger<ScheduledJob> logger)
        {
            this.logger = logger;
            this.configuration = configuration;
        }
       
        public async Task Execute(IJobExecutionContext context)
        {
            this.logger.LogWarning($"Hello from scheduled task start {DateTime.Now.ToLongTimeString()}");
            try
            {
                /*Delete unwanted data in database child table*/
                DBProvider.ExecuteNonQuery("spr_DeleteUnWantedData", System.Data.CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                LogHelper.InsertErrorLogs("[DataLaundryApp] : EventJob ", "Execute", ex.Message, "", ex.StackTrace);
            }
            this.logger.LogWarning($"Hello from scheduled task end {DateTime.Now.ToLongTimeString()}");
            
            await Task.CompletedTask;
        }
    }
}