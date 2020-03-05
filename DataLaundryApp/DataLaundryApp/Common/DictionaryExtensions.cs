using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Specialized;

namespace DataLaundryApp.Common
{
    public static class DictionaryExtensions
    {
        public static NameValueCollection ToNameValueCollection<tValue>(this IDictionary<string, tValue> dictionary)
        {
            var collection = new NameValueCollection();
            foreach (var pair in dictionary)
                collection.Add(pair.Key, pair.Value.ToString());
            return collection;
        }
    }
}