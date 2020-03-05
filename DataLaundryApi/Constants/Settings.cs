using System;
using System.Configuration;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace DataLaundryApi.Constants
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
        public static IConfigurationBuilder Getbuilder()
        {
             var builder= new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json");
               return builder;
        }
        public const string ErrorLogPath = "ErrorLogs/";
        public static string GetConnectionString()
        {
            //return ConfigurationManager.ConnectionStrings["ConnDataLaundryApp"].ConnectionString;
            //return "Data Source=192.168.1.168;Initial Catalog=DataLaundry_QA;User ID=sa;Password=welcome;Integrated Security=True;Max Pool Size=1000;";
            var builder = Getbuilder();
            var ConnectionString = builder.Build().GetValue<string>("ConnectionStrings:ConnDataLaundryApp");
            return ConnectionString;
        }

        public static string GetAppSetting(string key)
        {
            //return Convert.ToString(ConfigurationManager.AppSettings[key]);
            var builder=Getbuilder();
            var GetAppStringData= builder.Build().GetValue<string>("AppSettings:"+key); 
            return GetAppStringData; 
        }

        public static string MD5Hash(string input)
        {
            StringBuilder hash = new StringBuilder();
            MD5CryptoServiceProvider md5provider = new MD5CryptoServiceProvider();
            byte[] bytes = md5provider.ComputeHash(new UTF8Encoding().GetBytes(input));

            for (int i = 0; i < bytes.Length; i++)
            {
                hash.Append(bytes[i].ToString("x2"));
            }
            return hash.ToString();
        }

      
    }

}


