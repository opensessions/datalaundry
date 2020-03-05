using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DataLaundryApi.Models
{
    #region EventContainer
    public class EventContainer
    {
        [JsonProperty(PropertyName = "count")]
        public int count { get; set; }

        [JsonProperty(PropertyName = "limit")]
        public long? limit { get; set; }

        [JsonProperty(PropertyName = "page")]
        public long? page { get; set; }

        public List<string> items { get; set; }
    }

    //objDynamicData.offers = new List<ExpandoObject>();
    //objDynamicData.cost = "Contact organiser";
    //objDynamicData.tags = Array.Empty<string>();
    //objDynamicData.weekdays = Array.Empty<int>();
    //objDynamicData.categories = new ExpandoObject();
    //objDynamicData.categories.disabilitySupport = Array.Empty<string>();
    //objDynamicData.occurrences = new List<ExpandoObject>();
    //objDynamicData.contact = new ExpandoObject();
    //objDynamicData.contact.pointOfContact = new ExpandoObject();
    //objDynamicData.contact.organisation = new ExpandoObject();
    public class Items
    {
        public List<string> offers { get; set; }
        public string cost { get; set; }
        public string[] tags { get; set; }
        public int[] weekdays { get; set; }

        public Categories categories { get; set; }
        public List<Occurrences> occurrences { get; set; }
        public Contact contact { get; set; }
        public Restrictions restrictions { get; set; }

        public string description { get; set; }

    }   
    #endregion
}