using System;
using System.Configuration;
using Newtonsoft.Json;
using DataLaundryDAL.Helper;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using System.IO;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace DataLaundryDAL.Constants
{
    public class Settings
    {
         public static IConfigurationBuilder Getbuilder()
        {
            var builder = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile("appsettings.json");
            return builder;
        }
        
        private static IConfigurationBuilder builder = Getbuilder();
        private static IConfiguration _Configuration;

        public static void Configure(IConfiguration configuration)
        {
           try
           {
            _Configuration = configuration;
           }catch(Exception ex)
           {
               string msg="";
               msg=ex.Message;
           }
        }        

        public const string ErrorLogPath = "ErrorLogs/";
        public const string DefaultDateTimeFormat = "dd/MM/yyyy hh:mm tt";
        public const string DefaultDateFormat = "dd/MM/yyyy";
        //public static string FeedJSONFilePath = ConfigurationManager.AppSettings["FeedJSONFilePath"].ToString();
        //public static string FeedJSONFilePath = _Configuration["AppSettings:FeedJSONFilePath"].ToString();
        public static string FeedJSONFilePath = builder.Build().GetValue<string>("AppSettings:FeedJSONFilePath");
        //public static int FeedTraverseLength = Convert.ToInt32(ConfigurationManager.AppSettings["FeedTraverseLength"].ToString());
        public static int FeedTraverseLength = Convert.ToInt32(builder.Build().GetValue<string>("AppSettings:FeedTraverseLength"));
           
        // var conn = builder.Build().GetValue<string>("ConnectionStrings:connAwakenedMindCompanyAdmin");
        // public static int FeedTraverseLength=50;

        public static string GetConnectionString()
        {
            //return ConfigurationManager.ConnectionStrings["ConnDataLaundryApp"].ConnectionString;
           // return  _Configuration["ConnectionStrings:ConnDataLaundryApp"].ToString();
           //return "Data Source=192.168.1.168;Initial Catalog=DataLaundry_QA_CORE;User ID=sa;Password=welcome;Integrated Security=False;Max Pool Size=1000;";
           return builder.Build().GetValue<string>("AppSettings:ConnDataLaundryApp");
        }
        
        public static string GetAppSetting(string key)
        {
            //return Convert.ToString(ConfigurationManager.AppSettings[key]);
            //return Convert.ToString(_Configuration["AppSettings:"+key]);
            return builder.Build().GetValue<string>("AppSettings:"+key);
        }

        public static TimeSpan ConvertToTimestamp(DateTime value)
        {
            //create Timespan by subtracting the value provided from
            //the Unix Epoch
            TimeSpan span = (value - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime());

            //return the total seconds (which is a UNIX timestamp)
            return span;
        }
         public static string GetSMTP_R1()
        {
            return builder.Build().GetValue<string>("AppSettings:Recipient1");
        }

        public static string GetSMTP_R2()
        {
            return builder.Build().GetValue<string>("AppSettings:Recipient2");
        }

        public static string GetSMTP_Mail()
        {
            return builder.Build().GetValue<string>("AppSettings:Email");
        }

        public static string GetSMTP_Credential()
        {
            return builder.Build().GetValue<string>("AppSettings:Credential");
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
                    LogHelper.InsertErrorLogs("[DataLaundryAPP] Settings", "IsValidJson", ex.Message, ex.InnerException?.Message, ex.StackTrace);
                    return false;
                }
                catch (Exception ex) //some other exception
                {
                    LogHelper.InsertErrorLogs("[DataLaundryAPP] Settings", "IsValidJson", ex.Message, ex.InnerException?.Message, ex.StackTrace);
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
