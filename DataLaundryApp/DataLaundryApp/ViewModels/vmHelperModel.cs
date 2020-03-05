using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DataLaundryDAL.DTO;

namespace DataLaundryApp.ViewModels
{
    public class vmHelperModel
    {
        public bool hasMatchesFound { get; set; }
        public List<IntelligentFeedMapping> lstIntelligentFeedMappingAll { get; set; }
        public List<IntelligentFeedMapping>  lstIntelligentFeedMapping { get; set; }
        public bool isRoot  { get; set; }
        public bool isParentMappingDone   { get; set; }

        public vmHelperModel()
        {
            lstIntelligentFeedMapping=new List<IntelligentFeedMapping>();
            isRoot=true;
            isParentMappingDone=false;
        }
    }

}
