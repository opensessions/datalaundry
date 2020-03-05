using DataLaundryScheduler.Helpers;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Configuration;
using System.IO;


namespace DataLaundryScheduler
{
    public class Settings
    {
        public static IConfiguration _Configuration;
        private static IConfigurationBuilder builder;
        public static void Configure(IConfigurationBuilder Builder)
        {
            builder = Builder;
        }
        public Settings(IConfiguration configuration)
        {
            _Configuration = configuration;
        }
        public static IConfigurationRoot Getbuilder()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false)
                .Build();
            return builder;
        }

        public static string ErrorLogPath = "ErrorLogs/";

        public static string GetConnectionString()
        {
            //return ConfigurationManager.ConnectionStrings["ConnDataLaundryApp"].ConnectionString;
            var builder = Getbuilder();
            // var ConnectionString = builder.Build().GetValue<string>("ConnectionStrings:ConnDataLaundryApp");
            // return ConnectionString;
            //return "Data Source=192.168.1.168;Initial Catalog=DataLaundry_QA_CORE;User ID=sa;Password=welcome;Integrated Security=True;Max Pool Size=1000;";
            return builder["ConnectionStrings:ConnDataLaundryApp"];
        }

        public static string GetAppSetting(string Key)
        {
            //return Convert.ToString(ConfigurationManager.AppSettings[Key]);
            var builder = Getbuilder();
            // var GetAppStringData= builder.Build().GetValue<string>("AppSettings:"+Key); 
            // return GetAppStringData; 
            return builder["AppSettings:" + Key];
        }
        public static string GetAppCommandTimeout()
        {
            var builder = Getbuilder();
            return builder["AppSettings:CommandTimeout"];
        }
        public static string GetAppSettingDaysBefore()
        {
            var builder = Getbuilder();
            return builder["AppSettings:DaysBefore"];
        }
         public static string SMTP_Email()
        {
            var builder = Getbuilder();
            return builder["AppSettings:Email"];
        }
         public static string SMTP_REC1()
        {
            var builder = Getbuilder();
            return builder["AppSettings:Recipient1"];
        }
         public static string SMTP_REC2()
        {
            var builder = Getbuilder();
            return builder["AppSettings:Recipient2"];
        }
 public static string SMTP_Cred()
        {
            var builder = Getbuilder();
            return builder["AppSettings:Credential"];
        }
        public static string GetAppSettingOccurrenceCount()
        {
            var builder = Getbuilder();
            return builder["AppSettings:OccurrenceCount"];
        }
        public static TimeSpan ConvertToTimestamp(DateTime value)
        {
            //create Timespan by subtracting the value provided from
            //the Unix Epoch
            TimeSpan span = (value - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime());

            //return the total seconds (which is a UNIX timestamp)
            return span;
        }

        public static bool IsValidJson(string strInput)
        {
            strInput = strInput.Trim();
            if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || //For object
                (strInput.StartsWith("[") && strInput.EndsWith("]"))) //For array
            {
                try
                {
                    var obj = JToken.Parse(strInput);
                    return true;
                }
                catch (JsonReaderException ex)
                {
                    LogHelper.InsertErrorLogs("[DataLaundryScheduler] Settings", "IsValidJson", ex.Message, ex.InnerException?.Message, ex.StackTrace);
                    return false;
                }
                catch (Exception ex) //some other exception
                {
                    LogHelper.InsertErrorLogs("[DataLaundryScheduler] Settings", "IsValidJson", ex.Message, ex.InnerException?.Message, ex.StackTrace);
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

    }

}
