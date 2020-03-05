using DataLaundryApi.Constants;
using DataLaundryApi.Models;
using DataLaundryApi.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Xml;
using System.Collections;
using System.Reflection;
namespace DataLaundryApi.Constants
{
    public static class Helper
    {
          #region Distinct
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>
            (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
            {
                HashSet<TKey> seenKeys = new HashSet<TKey>();
                foreach (TSource element in source)
                {
                    if (seenKeys.Add(keySelector(element)))
                    {
                        yield return element;
                 
                    }
                }
            }
        #endregion
      public static void AddProperty(ExpandoObject expando, string propertyName, object propertyValue)
            {
                //Take use of the IDictionary implementation
                var expandoDict = expando as IDictionary<string,object>;
                if (expandoDict.ContainsKey(propertyName))
                    expandoDict[propertyName] = propertyValue;
                else
                    expandoDict.Add(propertyName, propertyValue);
            }
    }
}