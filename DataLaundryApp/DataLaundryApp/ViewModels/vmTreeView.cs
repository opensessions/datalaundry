using Newtonsoft.Json;
using System.Collections.Generic;

namespace DataLaundryApp.ViewModels
{
    public class vmJsTree
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        [JsonProperty(PropertyName = "icon")]
        public string Icon { get; set; }

        [JsonProperty(PropertyName = "state")]
        public vmJsTreeState State { get; set; }

        [JsonProperty(PropertyName = "li_attr")]
        public object LiAttributes { get; set; }

        [JsonProperty(PropertyName = "a_attr")]
        public object AAttributes { get; set; }

        [JsonProperty(PropertyName = "children")]
        public List<vmJsTree> Children { get; set; }

        public vmJsTree()
        {
            Children = new List<vmJsTree>();
        }
    }
    

    public class vmJsTreeState
    {
        [JsonProperty(PropertyName = "opened")]
        public bool Opened { get; set; }
        [JsonProperty(PropertyName = "disabled")]
        public bool Disabled { get; set; }
        [JsonProperty(PropertyName = "selected")]
        public bool Selected { get; set; }
    }
}