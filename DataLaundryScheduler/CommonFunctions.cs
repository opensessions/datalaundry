using System;
using System.Collections.Generic;
using System.Dynamic;

namespace DataLaundryScheduler
{
    class CommonFunctions
    {
        public static void AddProperty(ExpandoObject expando, string propertyName, object propertyValue)
        {
            // ExpandoObject supports IDictionary so we can extend it like this
            var expandoDict = expando as IDictionary<string, object>;

            if (expandoDict.ContainsKey(propertyName))
                expandoDict[propertyName] = propertyValue;
            else
                expandoDict.Add(propertyName, propertyValue);
        }

        public static bool IsPropertyExists(dynamic objDynamic, string name)
        {
            if (objDynamic is ExpandoObject)
                return ((IDictionary<string, object>)objDynamic).ContainsKey(name);

            return objDynamic.GetType().GetProperty(name) != null;
        }

        public static object GetPropertyValue(ExpandoObject expando, string propertyName)
        {
            var expandoDict = expando as IDictionary<string, object>;

            if (expandoDict.ContainsKey(propertyName))
                return expandoDict[propertyName];

            return null;
        }

        public static string ReplaceFirstOccurrence(string source, string find, string replace)
        {
            int Place = source.IndexOf(find);
            string result = source.Remove(Place, find.Length).Insert(Place, replace);
            return result;
        }

        public static string ReplaceLastOccurrence(string source, string find, string replace)
        {
            int Place = source.LastIndexOf(find);
            string result = source.Remove(Place, find.Length).Insert(Place, replace);
            return result;
        }

        public static int IndexOfNthOccurrence(string source, string find, int positionToFind)
        {
            int finalIndex = -1; // initially set out of bound as no index found yet

            int matchCount = 0; // initially set as no occurrence found
            int startIndexMarker = 0; // start finding from first index itself

            while ((startIndexMarker = source.IndexOf(find, startIndexMarker)) != -1 && matchCount < positionToFind)
            {
                // as match found update counter to find next occurrence after this index only
                startIndexMarker++;
                // update counter after char occurrence found til our position
                matchCount++;
            }

            //if match count is same as our position then set as index found in the string
            if (matchCount == positionToFind)
                finalIndex = startIndexMarker;

            return finalIndex;
        }

        public static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            try
            {
                dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            }
            catch { }
            return dtDateTime;
        }
    }
}
