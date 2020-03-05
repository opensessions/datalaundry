using DataLaundryApi.App_Filter;
using DataLaundryApi.Helpers;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DataLaundryApi.Controllers
{
    //[RoutePrefix("api/v1")]  
    [Route("rpde/v1")]
    [AuthorizationHandler(true)]   
    [ApiController]
    public class SearchController : ControllerBase
    {
        private static AppSettings _MySettings;
        private  double defaultLatitude = 51.5595;
        private  double defaultLongitude = -0.3167933; 
        private   double defaultRadius = 30;

         public SearchController(IOptions<AppSettings> AppSettings)
        {          
            _MySettings=AppSettings.Value;
            if(_MySettings!=null)
            {
                defaultLatitude=Convert.ToDouble(_MySettings.defaultLatitude.ToString());
                defaultLongitude=Convert.ToDouble(_MySettings.defaultLongitude.ToString());
                defaultRadius=Convert.ToDouble(_MySettings.defaultRadius.ToString());
            }            
        }
        
        #region allactivities
        [Route("search/allactivities")]
        public ActionResult GetAllActivities()
        {
            var oRequestCode = new Random().Next(0, int.MaxValue);
            var oRequestTime = DateTime.Now;
            LogHelper.InsertServiceLogs("search/allactivities (GetAllActivities) (" + oRequestCode + ")", null, oRequestTime);
            var activities = new List<string>();
            var oResult = MemoryCacheHelper.GetValue("GetAllActivities");
            if (oResult == null)
            {
                activities = FeedHelper.GetAllActivities();
                MemoryCacheHelper.Add("GetAllActivities", activities, DateTimeOffset.UtcNow.AddHours(2));
            }
            else
            {
                activities = oResult as List<string>;
            }
            LogHelper.InsertServiceLogs("search/allactivities (GetAllActivities) - Response (" + oRequestCode + ")", null, oRequestTime, DateTime.Now);
            return Ok(new { items = activities });
        }
        #endregion

        #region GetActivities
        [Route("search/activities"), HttpGet]
        public ActionResult GetActivities(double? lat, double? lng, double? radius, [FromQuery] string[] source,
                                                [FromQuery] string[] kind, [FromQuery] string[] tag, [FromQuery] string[] excludeTag,
                                                [FromQuery] string[] disabilitySupport, [FromQuery] string[] weekdays, string gender = null,
                                                double? minCost = null, double? maxCost = null, string from = null, string to = null,
                                                long? minTime = null, long? maxTime = null, long? minAge = null, long? maxAge = null)
        {
            #region Variable declaration or assignment
            lat = lat ?? defaultLatitude;
            lng = lng ?? defaultLongitude;
            radius = radius ?? defaultRadius;
            string sources = null;
            string kinds = null;
            string tags = null;
            string excludeTags = null;
            string disabilitySupports = null;
            string days = null;

            if (source.Length > 0)
                sources = string.Join(",", source);
            if (kind.Length > 0)
                kinds = string.Join(",", kind);
            if (tag.Length > 0)
                tags = string.Join(",", tag);
            if (excludeTag.Length > 0)
                excludeTags = string.Join(",", excludeTag);
            if (disabilitySupport.Length > 0)
                disabilitySupports = string.Join(",", disabilitySupport);
            if (weekdays.Length > 0)
                days = string.Join(",", weekdays);
            #endregion
            var oRequestCode = new Random().Next(0, int.MaxValue);
            var oRequestTime = DateTime.Now;
            string Model = string.Concat("{lat:", lat, ",lng:", lng, ",radius:", radius, ",sources:", sources, ",kinds:", kinds, ",tags:", tags, ",excludeTags:", excludeTags, ",disabilitySupports:", disabilitySupports, ",days:", days, ",gender:", gender, ",minCost:", minCost, ",maxCost:", maxCost, ",from:", from, ",to:", to, ",minTime:", minTime, ",maxTime:", maxTime, ",minAge:", minAge, ",maxAge:", maxAge, "}");
            var oResult = MemoryCacheHelper.GetValue(Model + "-Activities");
            var activities = new List<string>();
            LogHelper.InsertServiceLogs("search/activities (GetActivities) (" + oRequestCode + ")", Model, oRequestTime);
            if (oResult == null)
            {
                activities = FeedHelper.GetActivities(lat, lng, radius, sources, kinds, tags,
                                            excludeTags, disabilitySupports, days,
                                            minCost, maxCost, gender, minTime, maxTime,
                                            minAge, maxAge, from, to);
                MemoryCacheHelper.Add(Model + "-Activities", activities, DateTimeOffset.UtcNow.AddHours(2));
            }
            else
            {
                activities = oResult as List<string>;
            }
            LogHelper.InsertServiceLogs("search/activities (GetActivities) - Response (" + oRequestCode + ")", Model, oRequestTime, DateTime.Now);
            return Ok(new { items = activities });
        }
       
        #endregion

        #region disabilities
        [Route("search/disabilities"), HttpGet]
        public ActionResult GetDisabilities(double? lat, double? lng, double? radius, [FromQuery] string[] source,
                                                [FromQuery] string[] kind, [FromQuery] string[] tag, [FromQuery] string[] excludeTag,
                                                [FromQuery] string[] activity, [FromQuery] string[] weekdays, string gender = null,
                                                double? minCost = null, double? maxCost = null, string from = null, string to = null,
                                                long? minTime = null, long? maxTime = null, long? minAge = null, long? maxAge = null)
        {
            #region Variable declaration or assignment
            lat = lat ?? defaultLatitude;
            lng = lng ?? defaultLongitude;
            radius = radius ?? defaultRadius;
            string sources = null;
            string kinds = null;
            string tags = null;
            string excludeTags = null;
            string activities = null;
            string days = null;
            if (source.Length > 0)
                sources = string.Join(",", source);
            if (kind.Length > 0)
                kinds = string.Join(",", kind);
            if (tag.Length > 0)
                tags = string.Join(",", tag);
            if (excludeTag.Length > 0)
                excludeTags = string.Join(",", excludeTag);
            if (activity.Length > 0)
                activities = string.Join(",", activity);
            if (weekdays.Length > 0)
                days = string.Join(",", weekdays);
            #endregion
            var oRequestCode = new Random().Next(0, int.MaxValue);
            var oRequestTime = DateTime.Now;
            string Model = string.Concat("{lat:", lat, ",lng:", lng, ",radius:", radius, ",sources:", sources, ",kinds:", kinds, ",tags:", tags, ",excludeTags:", excludeTags, ",activities:", activities, ",days:", days, ",gender:", gender, ",minCost:", minCost, ",maxCost:", maxCost, ",from:", from, ",to:", to, ",minTime:", minTime, ",maxTime:", maxTime, ",minAge:", minAge, ",maxAge:", maxAge, "}");
            var disabilities = new List<string>();
            var oResult = MemoryCacheHelper.GetValue(Model + "-Disabilities");
            LogHelper.InsertServiceLogs("search/disabilities (GetDisabilities) (" + oRequestCode + ")", Model, oRequestTime);
            if (oResult == null)
            {
                disabilities = FeedHelper.GetDisabilities(lat, lng, radius, sources, kinds, tags,
                                           excludeTags, activities, days,
                                           minCost, maxCost, gender, minTime, maxTime,
                                           minAge, maxAge, from, to);

                MemoryCacheHelper.Add(Model + "-Disabilities", disabilities, DateTimeOffset.UtcNow.AddHours(2));
            }
            else
            {
                disabilities = oResult as List<string>;
            }
            LogHelper.InsertServiceLogs("search/disabilities (GetDisabilities) - Response (" + oRequestCode + ")", Model, oRequestTime, DateTime.Now);
            return Ok(new { items = disabilities });
        }       
        #endregion

        [Route("search/sessions"), HttpGet]
        public ActionResult GetSessions(double? lat, double? lng, double? radius,
                                            [FromQuery] string[] source, [FromQuery] string[] kind, [FromQuery] string[] tag, [FromQuery] string[] excludeTag,
                                            [FromQuery] string[] activity, [FromQuery] string[] disabilitySupport, [FromQuery] string[] weekdays,
                                            double? minCost = null, double? maxCost = null, string gender = null, string sortMode = null,
                                            long? minTime = null, long? maxTime = null, long? minAge = null, long? maxAge = null,
                                            long? page = 1, long? limit = 50, string from = null, string to = null)
        {
            #region Variable declaration or assignment
            lat = lat ?? defaultLatitude;
            lng = lng ?? defaultLongitude;
            radius = radius ?? defaultRadius;
            string sources = null;
            string kinds = null;
            string tags = null;
            string excludeTags = null;
            string activities = null;
            string disabilitySupports = null;
            string days = null;

            if (source.Length > 0)
                sources = string.Join(",", source);
            if (kind.Length > 0)
                kinds = string.Join(",", kind);
            if (tag.Length > 0)
                tags = string.Join(",", tag);
            if (excludeTag.Length > 0)
                excludeTags = string.Join(",", excludeTag);
            if (activity.Length > 0)
                activities = string.Join(",", activity);
            if (disabilitySupport.Length > 0)
                disabilitySupports = string.Join(",", disabilitySupport);
            if (weekdays.Length > 0)
                days = string.Join(",", weekdays);
            if (page == null || page == 0)
                page = 1;
            if (limit == null || limit == 0)
                limit = 50;
            #endregion
            string Model = string.Concat("{lat:", lat, ",lng:", lng, ",radius:", radius, ",sources:", sources, ",kinds:", kinds, ",tags:", tags, ",excludeTags:", excludeTags, ",activitys:", activities, ",disabilitySupports:", disabilitySupports, ",days:", days, ",gender:", gender, ",minCost:", minCost, ",maxCost:", maxCost, ",from:", from, ",to:", to, ",page:", page, ",limit:", limit, ",minTime:", minTime, ",maxTime:", maxTime, ",minAge:", minAge, ",maxAge:", maxAge, "}");
            LogHelper.InsertServiceLogs("search/sessions (GetSessions)", Model);
            var events = FeedHelper.GetEvents(lat, lng, radius, sources, kinds, tags,
                                            excludeTags, activities, disabilitySupports, days,
                                            minCost, maxCost, gender, sortMode, minTime, maxTime,
                                            minAge, maxAge, page, limit, from, to);
            return Ok(events);
        }

        #region opportunities Sample API
        [Route("search/opportunities"), HttpGet]
        public ActionResult GetEvents(double? lat, double? lng, double? radius,
                                           [FromQuery] string[] source, [FromQuery] string[] kind, [FromQuery] string[] tag, [FromQuery] string[] excludeTag,
                                           [FromQuery] string[] activity, [FromQuery] string[] disabilitySupport, [FromQuery] string[] weekdays,
                                           double? minCost = null, double? maxCost = null, string gender = null, string sortMode = null,
                                           long? minTime = null, long? maxTime = null, long? minAge = null, long? maxAge = null,
                                           long? page = 1, long? limit = 50, string from = null, string to = null)
        {
            #region Variable declaration or assignment
            lat = lat ?? defaultLatitude;
            lng = lng ?? defaultLongitude;
            radius = radius ?? defaultRadius;
            string sources = null;
            string kinds = null;
            string tags = null;
            string excludeTags = null;
            string activities = null;
            string disabilitySupports = null;
            string days = null;

            if (source.Length > 0)
                sources = string.Join(",", source);
            if (kind.Length > 0)
                kinds = string.Join(",", kind);
            if (tag.Length > 0)
                tags = string.Join(",", tag);
            if (excludeTag.Length > 0)
                excludeTags = string.Join(",", excludeTag);
            if (activity.Length > 0)
                activities = string.Join(",", activity);
            if (disabilitySupport.Length > 0)
                disabilitySupports = string.Join(",", disabilitySupport);
            if (weekdays.Length > 0)
                days = string.Join(",", weekdays);
            if (page == null || page == 0)
                page = 1;
            if (limit == null || limit == 0)
                limit = 100;
            #endregion
            string Model = string.Concat("{lat:", lat, ",lng:", lng, ",radius:", radius, ",sources:", sources, ",kinds:", kinds, ",tags:", tags, ",excludeTags:", excludeTags, ",activitys:", activities, ",disabilitySupports:", disabilitySupports, ",days:", days, ",gender:", gender, ",minCost:", minCost, ",maxCost:", maxCost, ",from:", from, ",to:", to, ",page:", page, ",limit:", limit, ",minTime:", minTime, ",maxTime:", maxTime, ",minAge:", minAge, ",maxAge:", maxAge, "}");
            var oRequestCode = new Random().Next(0, int.MaxValue);
            var oRequestTime = DateTime.Now;
            LogHelper.InsertServiceLogs("search/opportunities (GetEvents) (" + oRequestCode + ")", Model, oRequestTime);

            //var events = FeedHelper.GetEventsDynamically(lat, lng, radius, sources, kinds, tags,
            //                                excludeTags, activities, disabilitySupports, days,
            //                                minCost, maxCost, gender, sortMode, minTime, maxTime,
            //                                minAge, maxAge, page, limit, from, to);

            #region Added for avoid individual call for database 28-01-2019
            var events = FeedHelper.GetEventsDynamically_V1(lat, lng, radius, sources, kinds, tags,
                                            excludeTags, activities, disabilitySupports, days,
                                            minCost, maxCost, gender, sortMode, minTime, maxTime,
                                            minAge, maxAge, page, limit, from, to);
            #endregion
            LogHelper.InsertServiceLogs("search/opportunities (GetEvents) - Response (" + oRequestCode + ")", Model, oRequestTime, DateTime.Now);
            return Ok(events);
        }       
        #endregion        

        #region session - sessionId
        [Route("session/{sessionId}"), HttpGet]
        public ActionResult GetEventDetails(string sessionId)
        {
            string Model = string.Concat("{sessionId:", sessionId, "}");
            LogHelper.InsertServiceLogs("search/sessionId (GetEventDetails)", Model);

            //var events = FeedHelper.GetEventDetailsBySessionIdDynamically(sessionId?.Split('-')[1], sessionId?.Split('-')[0]);            
            var events = FeedHelper.GetEventDetailsBySessionIdDynamically(sessionId?.Substring(sessionId.IndexOf("-") + 1).Trim(), sessionId?.Substring(0, sessionId.IndexOf("-")).Trim());    
            return Ok(events);
        }      
        #endregion

        #region session/{sessionId}/subevent/{startDate}
        [Route("session/{sessionId}/subevent/{startDate}"), HttpGet]
        public ActionResult GetSubEventDetails(string sessionId, string startDate)
        {
            string Model = string.Concat("{sessionId:", sessionId, "startDate:", startDate, "}");
            LogHelper.InsertServiceLogs("search/sessionId/subevent/startDate (GetSubEventDetails)", Model);
            //var events = FeedHelper.GetSubEventDetailsBySessionIdDynamically(sessionId?.Split('-')[0], startDate, sessionId?.Split('-')[1]);
            var events = FeedHelper.GetSubEventDetailsBySessionIdDynamically(sessionId?.Substring(sessionId.IndexOf("-") + 1).Trim(), startDate, sessionId?.Substring(0, sessionId.IndexOf("-")).Trim());         
            return Ok(events);
        }
      
        #endregion

        [Route("search/organisations"), HttpGet]
        public ActionResult GetOrganisations(double? lat, double? lng, double? radius,
                                            [FromQuery] string[] activity, [FromQuery] string[] disabilitySupport,
                                            [FromQuery] string[] source, [FromQuery] string[] tag, [FromQuery] string[] excludeTag,
                                            string gender = null, long? minAge = null, long? maxAge = null,
                                            long? page = 1, long? limit = 50, string from = null, string to = null)
        {
            #region Variable declaration or assignment
            lat = lat ?? defaultLatitude;
            lng = lng ?? defaultLongitude;
            radius = radius ?? defaultRadius;
            string sources = null;
            string tags = null;
            string excludeTags = null;
            string activities = null;
            string disabilitySupports = null;

            if (source.Length > 0)
                sources = string.Join(",", source);
            if (tag.Length > 0)
                tags = string.Join(",", tag);
            if (excludeTag.Length > 0)
                excludeTags = string.Join(",", excludeTag);
            if (activity.Length > 0)
                activities = string.Join(",", activity);
            if (disabilitySupport.Length > 0)
                disabilitySupports = string.Join(",", disabilitySupport);
            if (page == null || page == 0)
                page = 1;
            if (limit == null || limit == 0)
                limit = 50;
            #endregion
            string Model = string.Concat("{lat:", lat, " ,lng:", lng, " ,radius:", radius, " , sources:", sources, " , tags:", tags, " , excludeTags:", excludeTags, " , activities:", activities, " ,page:", page, " , limit:", limit, "}");

            LogHelper.InsertServiceLogs("search/organisations (GetOrganisations)", Model);

            var organisations = FeedHelper.GetOrganisations(lat, lng, radius, activities,
                                            disabilitySupports, gender, minAge, maxAge,
                                            page, limit, from, to, sources, tags, excludeTags);
            
            return Ok(organisations);
        }

        [Route("organisation/{organisationId}"), HttpGet]
        public ActionResult GetOrganisationDetails(long? organisationId)
        {
            string Model = string.Concat("{organisationId:", organisationId, "}");
            var oRequestCode = new Random().Next(0, int.MaxValue);
            var oRequestTime = DateTime.Now;
            LogHelper.InsertServiceLogs("search/organisationId (GetOrganisationDetails)(" + oRequestCode + ")", Model, oRequestTime);
            var organisation = FeedHelper.GetOrganisationDetailsById(organisationId);
            LogHelper.InsertServiceLogs("search/organisationId (GetOrganisationDetails)- Response(" + oRequestCode + ")", Model, oRequestTime, DateTime.Now);          
            return Ok(organisation);
        }

        #region search/locations
        [Route("search/locations"), HttpGet]
        public ActionResult GetLocations(double? lat, double? lng, double? radius, [FromQuery] string[] activity,
                                            [FromQuery] string[] disabilitySupport, [FromQuery] string[] weekdays,
                                            [FromQuery] string[] source, [FromQuery] string[] tag, [FromQuery] string[] excludeTag,
                                            string gender = null, long? minTime = null, long? maxTime = null, long? minAge = null,
                                            long? maxAge = null, long? page = 1, long? limit = 50, string from = null,
                                            string to = null, double? minCost = null, double? maxCost = null)
        {
            #region Variable declaration or assignment
            lat = lat ?? defaultLatitude;
            lng = lng ?? defaultLongitude;
            radius = radius ?? defaultRadius;
            var oRadius = radius?.ToString("f2");
            string sources = null;
            string tags = null;
            string excludeTags = null;
            string activities = null;
            string disabilitySupports = null;
            string days = null;

            if (source.Length > 0)
                sources = string.Join(",", source);
            if (tag.Length > 0)
                tags = string.Join(",", tag);
            if (excludeTag.Length > 0)
                excludeTags = string.Join(",", excludeTag);
            if (activity.Length > 0)
                activities = string.Join(",", activity);
            if (disabilitySupport.Length > 0)
                disabilitySupports = string.Join(",", disabilitySupport);
            if (weekdays.Length > 0)
                days = string.Join(",", weekdays);
            if (page == null || page == 0)
                page = 1;
            if (limit == null || limit == 0)
                limit = 50;
            #endregion
            string Model = string.Concat("{lat:", lat, ",lng:", lng, ",radius:", radius, ",sources:", sources, ",tags:", tags, ",excludeTags:", excludeTags, ",activitys:", activities, ",disabilitySupports:", disabilitySupports, ",days:", days, ",gender:", gender, ",minCost:", minCost, ",maxCost:", maxCost, ",from:", from, ",to:", to, ",page:", page, ",limit:", limit, ",minTime:", minTime, ",maxTime:", maxTime, ",minAge:", minAge, ",maxAge:", maxAge, "}");
            var oRequestCode = new Random().Next(0, int.MaxValue);
            var oRequestTime = DateTime.Now;
            LogHelper.InsertServiceLogs("search/locations (GetLocations) (" + oRequestCode + ")", Model, oRequestTime);
            var events = FeedHelper.GetLocations(lat, lng, radius, activities, disabilitySupports,
                                            gender, minTime, maxTime, minAge, maxAge, days, page, limit,
                                            from, to, sources, tags, excludeTags, minCost, maxCost);
            LogHelper.InsertServiceLogs("search/locations (GetLocations) - Response (" + oRequestCode + ")", Model, oRequestTime, DateTime.Now);
            return Ok(events);
        }        
        #endregion
[Route("search/eventaddresswise")]
        public ActionResult GetEventAddressWise(string activityName = null, string location = null ,string organizer = null)
        {
            string Model = string.Concat("{activityName:", activityName, " , location:", organizer," , location:", organizer);
            LogHelper.InsertServiceLogs("search/list (GetEventActivityWise)", Model);
            var sessions = FeedHelper.GetEventAddressWise(activityName, location,organizer);           
            return Ok( new { activities = sessions });
        }
        [Route("search/list")]
        public ActionResult GetEventActivityWise(string activityName = null, string location = null, string latitude = null, string longitude = null)
        {
            string Model = string.Concat("{activityName:", activityName, " , location:", location, " , latitude:", latitude, " , longitude:", longitude);
            LogHelper.InsertServiceLogs("search/list (GetEventActivityWise)", Model);
            var sessions = FeedHelper.GetEventActivityWise(activityName, location, latitude, longitude);           
            return Ok( new { activities = sessions });
        }
    }
}
