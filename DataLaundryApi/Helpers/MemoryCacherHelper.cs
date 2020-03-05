using System;
using System.Configuration;
using System.Runtime.Caching;

namespace DataLaundryApi.Helpers
{
    public class MemoryCacheHelper
    {
        //public static string IsCacheEnabled = ConfigurationManager.AppSettings["IsCacheEnabled"].ToString();
        public static string IsCacheEnabled = Constants.Settings.GetAppSetting("IsCacheEnabled");
        public static object GetValue(string key)
        {
            if (IsCacheEnabled == "1")
            {
                MemoryCache memoryCache = MemoryCache.Default;
                return memoryCache.Get(key);
            }
            else
                return null;
        }

        public static bool Add(string key, object value, DateTimeOffset absExpiration)
        {
            if (IsCacheEnabled == "1")
            {
                MemoryCache memoryCache = MemoryCache.Default;
                return memoryCache.Add(key, value, absExpiration);
            }
            return false;
        }

        public static void Delete(string key)
        {
            if (IsCacheEnabled == "1")
            {
                MemoryCache memoryCache = MemoryCache.Default;
                if (memoryCache.Contains(key))
                {
                    memoryCache.Remove(key);
                }
            }
        }
    }
}