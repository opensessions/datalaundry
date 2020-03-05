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

namespace DataLaundryApi.Helpers
{
    public class FeedHelper
    {
        #region Session
        public static SessionContainer GetSessions(long? afterTimestamp = null, long? afterId = null)
        {
            SessionContainer sessionContainer = new SessionContainer();

            try
            {
                var lstSqlParameter = new List<SqlParameter>();

                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@AfterTimestamp", SqlDbType = SqlDbType.BigInt, Value = (object)afterTimestamp ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@AfterId", SqlDbType = SqlDbType.BigInt, Value = (object)afterId ?? DBNull.Value });

                var dt = DBProvider.GetDataTable("GetEvents", CommandType.StoredProcedure, ref lstSqlParameter);

                if (dt != null && dt.Rows.Count > 0)
                {

                    int rowCount = dt.Rows.Count;

                    for (int i = 0; i < rowCount; i++)
                    {
                        var row = dt.Rows[i];

                        var sessionDetails = new SessionDetails()
                        {
                            State = Convert.ToString(row["State"]),
                            Modified = Convert.ToInt64(row["ModifiedDateTimestamp"]),
                            Id = Convert.ToInt64(row["Id"])
                        };

                        if (sessionDetails.State.ToLower() == "updated")
                        {
                            var session = GetSessionFromDataRow(row, sessionDetails.Id);

                            sessionDetails.Data = session;
                        }

                        sessionContainer.Items.Add(sessionDetails);

                        //build next page url in the end based on 
                        if ((i + 1) == rowCount)
                        {
                            if (sessionContainer.Next.Contains("?"))
                                sessionContainer.Next += "&";
                            else
                                sessionContainer.Next += "?";

                            sessionContainer.Next += "afterTimestamp=" + sessionDetails.Modified;

                            sessionContainer.Next += "&";

                            sessionContainer.Next += "afterId=" + sessionDetails.Id;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertErrorLogs("[DataLaundryApi] FeedHelper", "GetSessions", ex.Message, ex.InnerException?.Message, ex.StackTrace);
            }

            return sessionContainer;
        }

        private static Session GetSessionFromDataRow(DataRow row, long eventId)
        {
            var session = new Session();

            session.Name = Convert.ToString(row["Name"]);

            if (row["Description"] != DBNull.Value)
                session.Description = Convert.ToString(row["Description"]);
            
            if (row["SessionId"] != DBNull.Value)
                session.SessionId = Convert.ToString(row["SessionId"]);
            if (row["Distance"] != DBNull.Value)
                session.Distance = Convert.ToDecimal(row["Distance"]);

            if (row["Image"] != DBNull.Value)
            {
                session.Image = new Image();
                session.Image.Url = Convert.ToString(row["Image"]);
                session.Image.Thumbnail = Convert.ToString(row["ImageThumbnail"]);
            }

            if (row["StartDate"] != DBNull.Value)
                session.StartDate = DateTime.SpecifyKind(Convert.ToDateTime(row["StartDate"]), DateTimeKind.Utc);

            //session.StartDate = DateTime.ParseExact(Convert.ToString(row["StartDate"]), "dd-MM-yyyy hh:mm:ss tt", CultureInfo.CurrentCulture, DateTimeStyles.AssumeUniversal);

            if (row["EndDate"] != DBNull.Value)
                session.EndDate = DateTime.SpecifyKind(Convert.ToDateTime(row["EndDate"]), DateTimeKind.Utc);

            if (row["Duration"] != DBNull.Value)
                session.Duration = Convert.ToString(row["Duration"]);
          
      
            if (row["MaximumAttendeeCapacity"] != DBNull.Value)
            {
                //session.MaximumAttendeeCapacity = Convert.ToInt32(row["MaximumAttendeeCapacity"]);
                long maximunAttendeeCapacity = 0;
                if (long.TryParse(Convert.ToString(row["MaximumAttendeeCapacity"]), out maximunAttendeeCapacity))
                    session.MaximumAttendeeCapacity = maximunAttendeeCapacity;
            }

            if (row["RemainingAttendeeCapacity"] != DBNull.Value)
            {
                //session.RemainingAttendeeCapacity = Convert.ToInt32(row["RemainingAttendeeCapacity"]);
                long remainingAttendeeCapacity = 0;
                if (long.TryParse(Convert.ToString(row["RemainingAttendeeCapacity"]), out remainingAttendeeCapacity))
                    session.MaximumAttendeeCapacity = remainingAttendeeCapacity;
            }

            if (row["EventStatus"] != DBNull.Value)
                session.EventStatus = Convert.ToString(row["EventStatus"]);

            if (row["Category"] != DBNull.Value)
                session.Category = Convert.ToString(row["Category"]).Split(',');

            if (row["AgeRange"] != DBNull.Value)
                session.AgeRange = Convert.ToString(row["AgeRange"]);

            if (row["GenderRestriction"] != DBNull.Value)
                session.GenderRestriction = Convert.ToString(row["GenderRestriction"]);

            if (row["AttendeeInstructions"] != DBNull.Value)
                session.AttendeeInstructions = Convert.ToString(row["AttendeeInstructions"]);

            if (row["AccessibilitySupport"] != DBNull.Value)
                session.AccessibilitySupport = Convert.ToString(row["AccessibilitySupport"]).Split(',');

            if (row["IsCoached"] != DBNull.Value)
                session.IsCoached = Convert.ToBoolean(row["IsCoached"]);

            if (row["Level"] != DBNull.Value)
                session.Level = Convert.ToString(row["Level"]).Split(',');

            if (row["MeetingPoint"] != DBNull.Value)
                session.MeetingPoint = Convert.ToString(row["MeetingPoint"]);
             
             if (row["Price"] != DBNull.Value)
                session.Price = Convert.ToString(row["Price"]);
             
        

            if (row["Url"] != DBNull.Value)
                session.Url = Convert.ToString(row["Url"]);

            //get event data
            var dsOtherEventData = GetOtherSessionData(eventId);

            if (dsOtherEventData != null && dsOtherEventData.Tables.Count > 0)
            {
                //subevents
                var dtSubEvents = dsOtherEventData.Tables[0];
                var dtSuperEvent = dsOtherEventData.Tables[1];
                var dtLocation = dsOtherEventData.Tables[2];
                var dtAmenityFeature = dsOtherEventData.Tables[3];
                var dtPhysicalAcitivity = dsOtherEventData.Tables[4];
                var dtEventSchedule = dsOtherEventData.Tables[5];
                var dtOrganization = dsOtherEventData.Tables[6];
                var dtPerson = dsOtherEventData.Tables[7];
                var dtProgramme = dsOtherEventData.Tables[8];
                var dtOccurrence = dsOtherEventData.Tables[9];


                #region SubEvent
                if (dtSubEvents?.Rows?.Count > 0)
                {
                    session.SubEvent = new List<Session>();

                    foreach (DataRow childRow in dtSubEvents.Rows)
                    {
                        long subEventId = Convert.ToInt64(childRow["Id"]);
                        var subevent = GetSessionFromDataRow(childRow, subEventId);
                        session.SubEvent.Add(subevent);
                    }
                }
                #endregion SubEvent

                #region SuperEvent
                if (dtSuperEvent?.Rows?.Count > 0)
                {
                    foreach (DataRow childRow in dtSuperEvent.Rows)
                    {
                        long superEventId = Convert.ToInt64(childRow["Id"]);
                        var superEvent = GetSessionFromDataRow(childRow, superEventId);
                        session.SuperEvent = superEvent;
                        break;
                    }
                }
                #endregion SuperEvent

                #region Location
                if (dtLocation?.Rows?.Count > 0)
                {
                    session.Location = new List<Location>();

                    var locationRows = dtLocation.Select("PlaceTypeId is null");

                    if (locationRows?.Length > 0)
                    {
                        foreach (DataRow childRow in locationRows)
                        {
                            long locationId = Convert.ToInt64(childRow["Id"]);
                            //decimal Distance = Convert.ToDecimal(row["Distance"]);
                            var location = GetLocationFromDataRow(dtLocation, dtAmenityFeature, childRow, locationId);

                            session.Location.Add(location);
                        }
                    }

                }
                #endregion Location

                #region Activity
                if (dtPhysicalAcitivity?.Rows?.Count > 0)
                {
                    session.Activity = new List<Activity>();

                    foreach (DataRow childRow in dtPhysicalAcitivity.Rows)
                    {
                        var activity = new Activity();

                        if (childRow["PrefLabel"] != DBNull.Value)
                            activity.PrefLabel = Convert.ToString(childRow["PrefLabel"]);

                        if (childRow["AltLabel"] != DBNull.Value)
                            activity.AltLabel = Convert.ToString(childRow["AltLabel"]);

                        if (childRow["InScheme"] != DBNull.Value)
                            activity.InScheme = Convert.ToString(childRow["InScheme"]);

                        if (childRow["Notation"] != DBNull.Value)
                            activity.Notation = Convert.ToString(childRow["Notation"]);

                        session.Activity.Add(activity);
                    }
                }
                #endregion Activity

                #region EventSchedule
                if (dtEventSchedule?.Rows?.Count > 0)
                {
                    foreach (DataRow childRow in dtEventSchedule.Rows)
                    {
                        var eventSchedule = new EventSchedule();

                        if (childRow["StartDate"] != DBNull.Value)
                            eventSchedule.StartDate = DateTime.SpecifyKind(Convert.ToDateTime(row["StartDate"]), DateTimeKind.Utc).ToString("yyyy-MM-dd");
                        //eventSchedule.StartDate = DateTime.Parse(Convert.ToString(childRow["StartDate"]), CultureInfo.CurrentCulture, DateTimeStyles.AssumeUniversal).ToString("yyyy-MM-dd");

                        if (childRow["EndDate"] != DBNull.Value)
                            eventSchedule.EndDate = DateTime.SpecifyKind(Convert.ToDateTime(row["EndDate"]), DateTimeKind.Utc).ToString("yyyy-MM-dd");

                        if (childRow["StartTime"] != DBNull.Value)
                            eventSchedule.StartTime = Convert.ToString(childRow["StartTime"]);

                        if (childRow["EndTime"] != DBNull.Value)
                            eventSchedule.EndTime = Convert.ToString(childRow["EndTime"]);

                        if (childRow["Frequency"] != DBNull.Value)
                            eventSchedule.Frequency = Convert.ToString(childRow["Frequency"]);

                        if (childRow["ByDay"] != DBNull.Value)
                            eventSchedule.ByDay = Convert.ToString(childRow["ByDay"]).Split(',');

                        if (childRow["ByMonth"] != DBNull.Value)
                            eventSchedule.ByMonth = Convert.ToString(childRow["ByMonth"]).Split(',');

                        if (childRow["ByMonthDay"] != DBNull.Value)
                            eventSchedule.ByMonthDay = Convert.ToString(childRow["ByMonthDay"]).Split(',');

                        if (childRow["RepeatCount"] != DBNull.Value)
                            eventSchedule.RepeatCount = Convert.ToInt32(childRow["RepeatCount"]);

                        if (childRow["RepeatFrequency"] != DBNull.Value)
                            eventSchedule.RepeatFrequency = Convert.ToString(childRow["RepeatFrequency"]);

                        session.EventSchedule = eventSchedule;
                        break;
                    }
                }
                #endregion EventSchedule

                #region Organization
                if (dtOrganization?.Rows?.Count > 0)
                {
                    foreach (DataRow childRow in dtOrganization.Rows)
                    {
                        var organizer = new Person();

                        if (childRow["Name"] != DBNull.Value)
                            organizer.Name = Convert.ToString(childRow["Name"]);

                        if (childRow["Description"] != DBNull.Value)
                            organizer.Description = Convert.ToString(childRow["Description"]);

                        if (childRow["Email"] != DBNull.Value)
                            organizer.Email = Convert.ToString(childRow["Email"]);

                        if (childRow["Image"] != DBNull.Value)
                            organizer.Logo = Convert.ToString(childRow["Image"]);

                        if (childRow["Url"] != DBNull.Value)
                            organizer.Url = Convert.ToString(childRow["Url"]);

                        if (childRow["Telephone"] != DBNull.Value)
                            organizer.Telephone = Convert.ToString(childRow["Telephone"]);

                        session.Organizer = organizer;
                        break;
                    }
                }
                #endregion Organization

                #region Leader & Contributor
                if (dtPerson?.Rows?.Count > 0)
                {
                    var dtLeader = dtPerson.Select("IsLeader = 1");
                    var dtContributor = dtPerson.Select("IsLeader = 0");

                    if (dtLeader?.Length > 0)
                    {
                        foreach (DataRow childRow in dtLeader)
                        {
                            var leader = new Person();

                            if (childRow["Name"] != DBNull.Value)
                                leader.Name = Convert.ToString(childRow["Name"]);

                            if (childRow["Description"] != DBNull.Value)
                                leader.Description = Convert.ToString(childRow["Description"]);

                            if (childRow["Email"] != DBNull.Value)
                                leader.Email = Convert.ToString(childRow["Email"]);

                            if (childRow["Image"] != DBNull.Value)
                                leader.Logo = Convert.ToString(childRow["Image"]);

                            if (childRow["Url"] != DBNull.Value)
                                leader.Url = Convert.ToString(childRow["Url"]);

                            if (childRow["Telephone"] != DBNull.Value)
                                leader.Telephone = Convert.ToString(childRow["Telephone"]);

                            session.Leader = leader;
                            break;
                        }
                    }

                    if (dtContributor?.Length > 0)
                    {
                        foreach (DataRow childRow in dtContributor)
                        {
                            var contributor = new Person();

                            if (childRow["Name"] != DBNull.Value)
                                contributor.Name = Convert.ToString(childRow["Name"]);

                            if (childRow["Description"] != DBNull.Value)
                                contributor.Description = Convert.ToString(childRow["Description"]);

                            if (childRow["Email"] != DBNull.Value)
                                contributor.Email = Convert.ToString(childRow["Email"]);

                            if (childRow["Image"] != DBNull.Value)
                                contributor.Logo = Convert.ToString(childRow["Image"]);

                            if (childRow["Url"] != DBNull.Value)
                                contributor.Url = Convert.ToString(childRow["Url"]);

                            if (childRow["Telephone"] != DBNull.Value)
                                contributor.Telephone = Convert.ToString(childRow["Telephone"]);

                            session.Contributor = contributor;
                            break;
                        }
                    }
                }
                #endregion Leader & Contributor

                #region Programme
                if (dtProgramme?.Rows?.Count > 0)
                {
                    foreach (DataRow childRow in dtProgramme.Rows)
                    {
                        var programme = new Programme();

                        if (childRow["Name"] != DBNull.Value)
                            programme.Name = Convert.ToString(childRow["Name"]);

                        if (childRow["Description"] != DBNull.Value)
                            programme.Description = Convert.ToString(childRow["Description"]);

                        if (childRow["Url"] != DBNull.Value)
                            programme.Url = Convert.ToString(childRow["Url"]);

                        session.Programme = programme;
                        break;
                    }
                }
                #endregion Programme
                  #region Occurences   
                    if (dtOccurrence?.Rows?.Count > 0)
                    {
                        foreach (DataRow childRow in dtOccurrence.Rows)
                        {
                            long subEventId = 0;
                          //  dynamic occurance = new ExpandoObject();
                        var occurance = new Occurrences();                            
                         if (childRow["SubEventId"] != DBNull.Value)
                                subEventId = Convert.ToInt64(childRow["SubEventId"]);

                            if (childRow["StartDate"] != DBNull.Value)
                            {
                                DateTimeOffset? StartDate = DateTime.SpecifyKind(Convert.ToDateTime(childRow["StartDate"]), DateTimeKind.Utc);
                                if (!string.IsNullOrEmpty(StartDate?.ToString()))
                                    occurance.Start = StartDate?.UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ssK");
                            }
                            if (childRow["EndDate"] != DBNull.Value)
                            {
                                DateTimeOffset? EndDate = DateTime.SpecifyKind(Convert.ToDateTime(childRow["EndDate"]), DateTimeKind.Utc);
                                if (!string.IsNullOrEmpty(EndDate?.ToString()))
                                    occurance.End = EndDate?.UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ssK");
                            }

                            //occurance.href =  Settings.GetAppSetting("sessionDetailAPI") + objDynamicData.id + (subEventId > 0 ? ("/subevent/" + occurance.start) : "");
                         //   occurance.href = Settings.GetAppSetting("sessionDetailAPI") + objDynamicData.id + "/subevent/" + occurance.start;
                            //occurance.checkoutUrl =  Settings.GetAppSetting("checkoutUrl") + objDynamicData.id + "/subevent/" + occurance.start;

                            session.Occurrences.Add(occurance);
                        }
                    }
                    #endregion
            }


            return session;
        }

        private static Location GetLocationFromDataRow(DataTable dtLocation, DataTable dtAmenityFeature, DataRow row, long locationId)
        {
            var location = new Location();

            var locationContainsRows = dtLocation.Select($"ParentId = {locationId} and PlaceTypeId = 1");

            var locationContainedInRows = dtLocation.Select($"ParentId = {locationId} and PlaceTypeId = 2");

            var locationAmenityFeatureRows = dtAmenityFeature.Select($"PlaceId = {locationId}");

            if (row["Name"] != DBNull.Value)
                location.Name = Convert.ToString(row["Name"]);

            if (row["Description"] != DBNull.Value)
                location.Description = Convert.ToString(row["Description"]);
                
      
            if (row["Image"] != DBNull.Value)
                location.Image = Convert.ToString(row["Image"]);

            if (row["Address"] != DBNull.Value)
                location.Address = Convert.ToString(row["Address"]);

            if (row["Lat"] != DBNull.Value || row["Long"] != DBNull.Value)
            {
                location.Geo = new Geo();
                if (row["Lat"] != DBNull.Value && !string.IsNullOrEmpty(Convert.ToString(row["Lat"])))
                    location.Geo.Latitude = Convert.ToDouble(row["Lat"]);
                if (row["Long"] != DBNull.Value && !string.IsNullOrEmpty(Convert.ToString(row["Long"])))
                    location.Geo.Longitude = Convert.ToDouble(row["Long"]);
            }

            if (row["Telephone"] != DBNull.Value)
                location.Telephone = Convert.ToString(row["Telephone"]);

            if (row["FaxNumber"] != DBNull.Value)
                location.FaxNumber = Convert.ToString(row["FaxNumber"]);

            if (row["OpeningHoursSpecification"] != DBNull.Value)
            {
                location.OpeningHoursSpecification = new List<OpeningHoursSpecification>();
                var lstOpeningHoursSpecification = Newtonsoft.Json.JsonConvert.DeserializeObject<OpeningHoursSpecification[]>(row["OpeningHoursSpecification"].ToString());
                location.OpeningHoursSpecification = lstOpeningHoursSpecification.OfType<OpeningHoursSpecification>().ToList();
            }

            if (locationAmenityFeatureRows?.Length > 0)
            {
                location.AmenityFeature = new List<AmenityFeature>();
                foreach (DataRow amenityFeatureRow in locationAmenityFeatureRows)
                {
                    var amenityFeature = new AmenityFeature();

                    if (amenityFeatureRow["Type"] != DBNull.Value)
                        amenityFeature.Type = Convert.ToString(amenityFeatureRow["Type"]);

                    if (amenityFeatureRow["Name"] != DBNull.Value)
                        amenityFeature.Name = Convert.ToString(amenityFeatureRow["Name"]);

                    if (amenityFeatureRow["Value"] != DBNull.Value)
                        amenityFeature.Value = Convert.ToBoolean(amenityFeatureRow["Value"]);

                    location.AmenityFeature.Add(amenityFeature);
                }
            }

            if (locationContainsRows?.Length > 0)
            {
                foreach (DataRow containsRow in locationContainsRows)
                {
                    long containsPlaceId = Convert.ToInt64(containsRow["Id"]);

                    location.ContainsPlace = GetLocationFromDataRow(dtLocation, dtAmenityFeature, containsRow, containsPlaceId);

                    break;
                }
            }

            if (locationContainedInRows?.Length > 0)
            {
                foreach (DataRow containedInRow in locationContainedInRows)
                {
                    long containedInPlaceId = Convert.ToInt64(containedInRow["Id"]);

                    location.ContainedInPlace = GetLocationFromDataRow(dtLocation, dtAmenityFeature, containedInRow, containedInPlaceId);

                    break;
                }
            }

            return location;
        }

        public static DataSet GetOtherSessionData(long eventId, bool IsSingleOccurrence = false)
        {
            DataSet ds = null;

            try
            {
                var lstSqlParameter = new List<SqlParameter>();

                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@EventId", SqlDbType = SqlDbType.BigInt, Value = eventId });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@IsSingleOccurrence", SqlDbType = SqlDbType.Bit, Value = IsSingleOccurrence });

                ds = DBProvider.GetDataSet("GetEventData", CommandType.StoredProcedure, ref lstSqlParameter);
            }
            catch (Exception ex)
            {
                LogHelper.InsertErrorLogs("[DataLaundryApi] FeedHelper", "GetOtherSessionData", ex.Message, ex.InnerException?.Message, ex.StackTrace);
            }

            return ds;
        }

        public static SessionContainer_v1 GetSessions_v1(long? afterTimestamp = null, long? afterId = null)
        {
            SessionContainer_v1 sessionContainer = new SessionContainer_v1();

            try
            {
                var lstSqlParameter = new List<SqlParameter>();

                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@AfterTimestamp", SqlDbType = SqlDbType.BigInt, Value = (object)afterTimestamp ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@AfterId", SqlDbType = SqlDbType.BigInt, Value = (object)afterId ?? DBNull.Value });

                var dt = DBProvider.GetDataTable("GetEvents", CommandType.StoredProcedure, ref lstSqlParameter);

                if (dt != null && dt.Rows.Count > 0)
                {
                    int rowCount = dt.Rows.Count;

                    for (int i = 0; i < rowCount; i++)
                    {
                        var row = dt.Rows[i];

                        var sessionDetails = new SessionDetails_v1()
                        {
                            State = Convert.ToString(row["State"]),
                            Modified = Convert.ToInt64(row["ModifiedDateTimestamp"]),
                            Id = Convert.ToInt64(row["Id"])
                        };

                        if (sessionDetails.State.ToLower() == "updated")
                        {
                            var session = GetSessionFromDataRow_v1(row, sessionDetails.Id);
                            sessionDetails.Sessions = session;
                        }

                        sessionContainer.Items.Add(sessionDetails);

                        //build next page url in the end based on 
                        if ((i + 1) == rowCount)
                        {
                            if (sessionContainer.Next.Contains("?"))
                                sessionContainer.Next += "&";
                            else
                                sessionContainer.Next += "?";

                            sessionContainer.Next += "afterTimestamp=" + sessionDetails.Modified;

                            sessionContainer.Next += "&";

                            sessionContainer.Next += "afterId=" + sessionDetails.Id;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertErrorLogs("[DataLaundryApi] FeedHelper", "GetSessions_v1", ex.Message, ex.InnerException?.Message, ex.StackTrace);
            }

            return sessionContainer;
        }

        private static ExpandoObject GetSessionFromDataRow_v1(DataRow row, long eventId)
        {
            dynamic session = new ExpandoObject() as IDictionary<string, Object>;

            session.Name = Convert.ToString(row["Name"]);
            session.Identifier = Convert.ToString(row["Identifier"]);
            session.FeedId = Convert.ToString(row["FeedId"]);
            if (row["Description"] != DBNull.Value)
                session.Description = Convert.ToString(row["Description"]);
            else
                session.Description = "";

            if (row["Image"] != DBNull.Value)
            {
                session.Image = new Image();
                session.Image.Url = Convert.ToString(row["Image"]);
                session.Image.Thumbnail = Convert.ToString(row["ImageThumbnail"]);
            }
            else
            {
                session.Image = new Image();
                session.Image.Url = "";
                session.Image.Thumbnail = "";
            }

            if (row["StartDate"] != DBNull.Value)
                session.StartDate = DateTime.SpecifyKind(Convert.ToDateTime(row["StartDate"]), DateTimeKind.Utc);
            else
                session.StartDate = "";

            if (row["EndDate"] != DBNull.Value)
                session.EndDate = DateTime.SpecifyKind(Convert.ToDateTime(row["EndDate"]), DateTimeKind.Utc);
            else
                session.EndDate = "";

            if (row["Duration"] != DBNull.Value)
                session.Duration = Convert.ToString(row["Duration"]);
            else
                session.Duration = "";

            if (row["MaximumAttendeeCapacity"] != DBNull.Value)
            {
                long maximunAttendeeCapacity = 0;
                session.MaximumAttendeeCapacity = long.TryParse(Convert.ToString(row["MaximumAttendeeCapacity"]), out maximunAttendeeCapacity)
                                                    ? maximunAttendeeCapacity
                                                    : (IsValidJson(Convert.ToString(row["MaximumAttendeeCapacity"]))
                                                        ? (JsonConvert.DeserializeObject(Convert.ToString(row["MaximumAttendeeCapacity"])) as JToken).SelectToken("$.value")
                                                        : Convert.ToString(row["MaximumAttendeeCapacity"]));
            }
            else
            {
                session.MaximumAttendeeCapacity = 0;
            }

            if (row["RemainingAttendeeCapacity"] != DBNull.Value)
            {
                long remainingAttendeeCapacity = 0;
                session.RemainingAttendeeCapacity = long.TryParse(Convert.ToString(row["RemainingAttendeeCapacity"]), out remainingAttendeeCapacity)
                                                    ? remainingAttendeeCapacity
                                                    : (IsValidJson(Convert.ToString(row["RemainingAttendeeCapacity"]))
                                                        ? (JsonConvert.DeserializeObject(Convert.ToString(row["RemainingAttendeeCapacity"])) as JToken).SelectToken("$.value")
                                                        : Convert.ToString(row["RemainingAttendeeCapacity"]));
            }
            else
            {
                session.RemainingAttendeeCapacity = 0;
            }


            if (row["EventStatus"] != DBNull.Value)
                session.EventStatus = Convert.ToString(row["EventStatus"]);
            else
                session.EventStatus = "";

            if (row["Category"] != DBNull.Value)
                session.Category = Convert.ToString(row["Category"]).Split(',');
            else
                session.Category = new string[] { };

            if (row["AgeRange"] != DBNull.Value)
                session.AgeRange = Convert.ToString(row["AgeRange"]);
            else
                session.AgeRange = "";

            if (row["GenderRestriction"] != DBNull.Value)
                session.GenderRestriction = Convert.ToString(row["GenderRestriction"]);
            else
                session.GenderRestriction = "";

            if (row["AttendeeInstructions"] != DBNull.Value)
                session.AttendeeInstructions = Convert.ToString(row["AttendeeInstructions"]);
            else
                session.AttendeeInstructions = "";

            if (row["AccessibilitySupport"] != DBNull.Value)
                session.AccessibilitySupport = Convert.ToString(row["AccessibilitySupport"]).Split(',');
            else
                session.AccessibilitySupport = "";

            if (row["IsCoached"] != DBNull.Value)
                session.IsCoached = Convert.ToBoolean(row["IsCoached"]);
            else
                session.IsCoached = false;

            if (row["Level"] != DBNull.Value)
                session.Level = Convert.ToString(row["Level"]).Split(',');
            else
                session.Level = new string[] { };

            if (row["MeetingPoint"] != DBNull.Value)
                session.MeetingPoint = Convert.ToString(row["MeetingPoint"]);
            else
                session.MeetingPoint = "";

            if (row["Url"] != DBNull.Value)
                session.Url = Convert.ToString(row["Url"]);
            else
                session.Url = "";

            //get event data
            var dsOtherEventData = GetOtherSessionData(eventId);

            if (dsOtherEventData != null && dsOtherEventData.Tables.Count > 0)
            {
                //subevents
                var dtSubEvents = dsOtherEventData.Tables[0];
                var dtSuperEvent = dsOtherEventData.Tables[1];
                var dtLocation = dsOtherEventData.Tables[2];
                var dtAmenityFeature = dsOtherEventData.Tables[3];
                var dtPhysicalAcitivity = dsOtherEventData.Tables[4];
                var dtEventSchedule = dsOtherEventData.Tables[5];
                var dtOrganization = dsOtherEventData.Tables[6];
                var dtPerson = dsOtherEventData.Tables[7];
                var dtProgramme = dsOtherEventData.Tables[8];
                var dtOccurrence = dsOtherEventData.Tables[9];
                var dtCustomFeed = dsOtherEventData.Tables[10];
                var dtOffer = dsOtherEventData.Tables[11];
                var dtSlot = dsOtherEventData.Tables[12];
                var dtFacilityUse = dsOtherEventData.Tables[13];

                #region SubEvent
                if (dtSubEvents?.Rows?.Count > 0)
                {
                    session.SubEvent = new List<ExpandoObject>();

                    foreach (DataRow childRow in dtSubEvents.Rows)
                    {
                        long subEventId = Convert.ToInt64(childRow["Id"]);
                        var subevent = GetSessionFromDataRow_v1(childRow, subEventId);
                        session.SubEvent.Add(subevent);
                    }
                }
                else
                {
                    session.SubEvent = new List<ExpandoObject>();
                }
                #endregion SubEvent

                #region SuperEvent
                if (dtSuperEvent?.Rows?.Count > 0)
                {
                    foreach (DataRow childRow in dtSuperEvent.Rows)
                    {
                        long superEventId = Convert.ToInt64(childRow["Id"]);
                        var superEvent = GetSessionFromDataRow_v1(childRow, superEventId);
                        session.SuperEvent = superEvent;
                        break;
                    }
                }
                #endregion SuperEvent

                #region Location
                if (dtLocation?.Rows?.Count > 0)
                {
                    session.Location = new List<Location>();

                    var locationRows = dtLocation.Select("PlaceTypeId is null");

                    if (locationRows?.Length > 0)
                    {
                        foreach (DataRow childRow in locationRows)
                        {
                            long locationId = Convert.ToInt64(childRow["Id"]);

                            var location = GetLocationFromDataRow(dtLocation, dtAmenityFeature, childRow, locationId);

                            session.Location.Add(location);
                        }
                    }

                }
                else
                {
                    session.Location = new List<Location>();
                    session.Location.Add(new Location());
                }
                #endregion Location

                #region Activity
                if (dtPhysicalAcitivity?.Rows?.Count > 0)
                {
                    session.Activity = new List<Activity>();

                    foreach (DataRow childRow in dtPhysicalAcitivity.Rows)
                    {
                        var activity = new Activity();

                        if (childRow["PrefLabel"] != DBNull.Value)
                            activity.PrefLabel = Convert.ToString(childRow["PrefLabel"]);

                        if (childRow["AltLabel"] != DBNull.Value)
                            activity.AltLabel = Convert.ToString(childRow["AltLabel"]);

                        if (childRow["InScheme"] != DBNull.Value)
                            activity.InScheme = Convert.ToString(childRow["InScheme"]);

                        if (childRow["Notation"] != DBNull.Value)
                            activity.Notation = Convert.ToString(childRow["Notation"]);

                        if (childRow["Image"] != DBNull.Value)
                            activity.Image = Convert.ToString(childRow["Image"]);

                        if (childRow["Description"] != DBNull.Value)
                            activity.Description = Convert.ToString(childRow["Description"]);

                        session.Activity.Add(activity);
                    }
                }
                else
                {
                    session.Activity = new List<Activity>();
                    session.Activity.Add(new Activity());
                }
                #endregion Activity

                #region EventSchedule
                if (dtEventSchedule?.Rows?.Count > 0)
                {
                    foreach (DataRow childRow in dtEventSchedule.Rows)
                    {
                        var eventSchedule = new EventSchedule();

                        if (childRow["StartDate"] != DBNull.Value)
                            eventSchedule.StartDate = DateTime.SpecifyKind(Convert.ToDateTime(childRow["StartDate"]), DateTimeKind.Utc).ToString("yyyy-MM-dd");

                        if (childRow["EndDate"] != DBNull.Value)
                            eventSchedule.EndDate = DateTime.SpecifyKind(Convert.ToDateTime(childRow["EndDate"]), DateTimeKind.Utc).ToString("yyyy-MM-dd");

                        if (childRow["StartTime"] != DBNull.Value)
                            eventSchedule.StartTime = Convert.ToString(childRow["StartTime"]);

                        if (childRow["EndTime"] != DBNull.Value)
                            eventSchedule.EndTime = Convert.ToString(childRow["EndTime"]);

                        if (childRow["Frequency"] != DBNull.Value)
                            eventSchedule.Frequency = Convert.ToString(childRow["Frequency"]);

                        if (childRow["ByDay"] != DBNull.Value)
                            eventSchedule.ByDay = Convert.ToString(childRow["ByDay"]).Split(',');

                        if (childRow["ByMonth"] != DBNull.Value)
                            eventSchedule.ByMonth = Convert.ToString(childRow["ByMonth"]).Split(',');

                        if (childRow["ByMonthDay"] != DBNull.Value)
                            eventSchedule.ByMonthDay = Convert.ToString(childRow["ByMonthDay"]).Split(',');

                        if (childRow["RepeatCount"] != DBNull.Value)
                            eventSchedule.RepeatCount = Convert.ToInt32(childRow["RepeatCount"]);

                        if (childRow["RepeatFrequency"] != DBNull.Value)
                            eventSchedule.RepeatFrequency = Convert.ToString(childRow["RepeatFrequency"]);

                        session.EventSchedule = eventSchedule;
                        break;
                    }
                }
                else
                {
                    session.EventSchedule = new EventSchedule();
                }
                #endregion EventSchedule

                #region Organization
                if (dtOrganization?.Rows?.Count > 0)
                {
                    foreach (DataRow childRow in dtOrganization.Rows)
                    {
                        var organizer = new Person();

                        if (childRow["Name"] != DBNull.Value)
                            organizer.Name = Convert.ToString(childRow["Name"]);

                        if (childRow["Description"] != DBNull.Value)
                            organizer.Description = Convert.ToString(childRow["Description"]);

                        if (childRow["Email"] != DBNull.Value)
                            organizer.Email = Convert.ToString(childRow["Email"]);

                        if (childRow["Image"] != DBNull.Value)
                            organizer.Logo = Convert.ToString(childRow["Image"]);

                        if (childRow["Url"] != DBNull.Value)
                            organizer.Url = Convert.ToString(childRow["Url"]);

                        if (childRow["Telephone"] != DBNull.Value)
                            organizer.Telephone = Convert.ToString(childRow["Telephone"]);

                        session.Organizer = organizer;
                        break;
                    }
                }
                else
                {
                    session.Organizer = new Person();
                }
                #endregion Organization

                #region Leader & Contributor
                if (dtPerson?.Rows?.Count > 0)
                {
                    var dtLeader = dtPerson.Select("IsLeader = 1");
                    var dtContributor = dtPerson.Select("IsLeader = 0");

                    if (dtLeader?.Length > 0)
                    {
                        foreach (DataRow childRow in dtLeader)
                        {
                            var leader = new Person();

                            if (childRow["Name"] != DBNull.Value)
                                leader.Name = Convert.ToString(childRow["Name"]);

                            if (childRow["Description"] != DBNull.Value)
                                leader.Description = Convert.ToString(childRow["Description"]);

                            if (childRow["Email"] != DBNull.Value)
                                leader.Email = Convert.ToString(childRow["Email"]);

                            if (childRow["Image"] != DBNull.Value)
                                leader.Logo = Convert.ToString(childRow["Image"]);

                            if (childRow["Url"] != DBNull.Value)
                                leader.Url = Convert.ToString(childRow["Url"]);

                            if (childRow["Telephone"] != DBNull.Value)
                                leader.Telephone = Convert.ToString(childRow["Telephone"]);

                            session.Leader = leader;
                            break;
                        }
                    }
                    else
                    {
                        session.Leader = new Person();
                    }

                    if (dtContributor?.Length > 0)
                    {
                        foreach (DataRow childRow in dtContributor)
                        {
                            var contributor = new Person();

                            if (childRow["Name"] != DBNull.Value)
                                contributor.Name = Convert.ToString(childRow["Name"]);

                            if (childRow["Description"] != DBNull.Value)
                                contributor.Description = Convert.ToString(childRow["Description"]);

                            if (childRow["Email"] != DBNull.Value)
                                contributor.Email = Convert.ToString(childRow["Email"]);

                            if (childRow["Image"] != DBNull.Value)
                                contributor.Logo = Convert.ToString(childRow["Image"]);

                            if (childRow["Url"] != DBNull.Value)
                                contributor.Url = Convert.ToString(childRow["Url"]);

                            if (childRow["Telephone"] != DBNull.Value)
                                contributor.Telephone = Convert.ToString(childRow["Telephone"]);

                            session.Contributor = contributor;
                            break;
                        }
                    }
                    else
                    {
                        session.Contributor = new Person();
                    }
                }
                else
                {
                    session.Leader = new Person();
                    session.Contributor = new Person();
                }
                #endregion Leader & Contributor

                #region Programme
                if (dtProgramme?.Rows?.Count > 0)
                {
                    foreach (DataRow childRow in dtProgramme.Rows)
                    {
                        var programme = new Programme();

                        if (childRow["Name"] != DBNull.Value)
                            programme.Name = Convert.ToString(childRow["Name"]);

                        if (childRow["Description"] != DBNull.Value)
                            programme.Description = Convert.ToString(childRow["Description"]);

                        if (childRow["Url"] != DBNull.Value)
                            programme.Url = Convert.ToString(childRow["Url"]);

                        session.Programme = programme;
                        break;
                    }
                }
                else
                {
                    session.Programme = new Programme();
                }
                #endregion Programme

                #region dtOffer
                if (dtOffer != null && dtOffer.Rows.Count > 0)
                {
                    session.Offer = new List<Offer>();

                    foreach (DataRow offerrow in dtOffer.Select("SlotId IS NULL"))
                    {
                        var oOffer = new Offer()
                        {
                            Name = Convert.ToString(offerrow["Name"]),
                            Price = Convert.ToString(offerrow["Price"]),
                            Description = Convert.ToString(offerrow["Description"]),
                            PriceCurrency = Convert.ToString(offerrow["PriceCurrency"])
                        };
                        session.Offer.Add(oOffer);
                    }
                }
                else
                {
                    session.Offer = new Offer();
                }
                #endregion

                #region Slot
                if (dtSlot != null && dtSlot?.Rows?.Count > 0)
                {
                    session.Slot = new List<Slot>();
                    foreach (DataRow slotrow in dtSlot.Rows)
                    {
                        var oSlot = new Slot()
                        {
                            Identifier = Convert.ToString(slotrow["Identifier"]),
                            StartDate = Convert.ToString(slotrow["StartDate"]),
                            EndDate = Convert.ToString(slotrow["EndDate"]),
                            Duration = Convert.ToString(slotrow["Duration"]),
                            OfferID = Convert.ToString(slotrow["OfferID"]),
                            RemainingUses = Convert.ToString(slotrow["RemainingUses"]),
                            MaximumUses = Convert.ToString(slotrow["MaximumUses"])
                        };

                        if (oSlot.OfferID != null)
                        {
                            var oSlotOffer = dtOffer.Select(" SlotId = " + slotrow["Id"].ToString());
                            if (oSlotOffer != null && oSlotOffer.Length > 0)
                            {
                                foreach (DataRow offerrow in oSlotOffer)
                                {
                                    var oOffer = new Offer()
                                    {
                                        Name = Convert.ToString(offerrow["Name"]),
                                        Price = Convert.ToString(offerrow["Price"]),
                                        Description = Convert.ToString(offerrow["Description"]),
                                        PriceCurrency = Convert.ToString(offerrow["PriceCurrency"])
                                    };
                                    oSlot.Offers.Add(oOffer);
                                }
                            }
                        }
                        session.Slot.Add(oSlot);
                    }
                }
                else
                {
                    session.Slot = new List<Slot>();
                }
                #endregion

                #region FacilityUse
                if (dtFacilityUse != null && dtFacilityUse.Rows.Count > 0)
                {
                    session.FacilityUse = new List<FacilityUse>();
                    foreach (DataRow drFacility in dtFacilityUse.Select("ParentId IS NULL"))
                    {
                        var oFacilityUse = new FacilityUse();
                        oFacilityUse.Description = Convert.ToString(drFacility["Description"]);
                        oFacilityUse.Name = Convert.ToString(drFacility["Name"]);
                        oFacilityUse.URL = Convert.ToString(drFacility["URL"]);
                        oFacilityUse.Type = "FacilityUse";
                        oFacilityUse.Provider = new Provider();
                        if (!string.IsNullOrEmpty(Convert.ToString(drFacility["Provider"])))
                            oFacilityUse.Provider = JsonConvert.DeserializeObject<Provider>(Convert.ToString(drFacility["Provider"]));

                        #region IndividualFacilityUse
                        var IndividualFacilityUse = dtFacilityUse.Select("ParentId = " + drFacility["Id"]);
                        if (IndividualFacilityUse != null && IndividualFacilityUse.Length > 0)
                        {
                            oFacilityUse.IndividualFacilityUse = new List<FacilityUse>();
                            foreach (DataRow oChildFacility in IndividualFacilityUse)
                            {
                                var oIndividualFacilityUse = new FacilityUse();
                                oIndividualFacilityUse.Description = Convert.ToString(oChildFacility["Description"]);
                                oIndividualFacilityUse.Name = Convert.ToString(oChildFacility["Name"]);
                                oIndividualFacilityUse.URL = Convert.ToString(oChildFacility["URL"]);
                                oIndividualFacilityUse.Type = "IndividualFacilityUse";
                                oIndividualFacilityUse.Provider = new Provider();
                                if (!string.IsNullOrEmpty(Convert.ToString(oChildFacility["Provider"])))
                                    oIndividualFacilityUse.Provider = JsonConvert.DeserializeObject<Provider>(Convert.ToString(oChildFacility["Provider"]));

                                oFacilityUse.IndividualFacilityUse.Add(oIndividualFacilityUse);
                            }
                        }
                        #endregion
                        session.FacilityUse.Add(oFacilityUse);
                    }
                }
                else
                    session.FacilityUse = new List<FacilityUse>();

                #endregion

                #region dtOccurrence
                if (dtOccurrence != null && dtOccurrence.Rows.Count > 0)
                {
                    session.Occurrence = new List<Occurrences>();
                    foreach (DataRow Occurrencerow in dtOccurrence.Rows)
                    {
                        var oOccurrence = new Occurrences();
                        if (Occurrencerow["StartDate"] != DBNull.Value)
                        {
                            DateTimeOffset? StartDate = DateTime.SpecifyKind(Convert.ToDateTime(Occurrencerow["StartDate"]), DateTimeKind.Utc);
                            if (!string.IsNullOrEmpty(StartDate?.ToString()))
                                oOccurrence.Start = StartDate?.UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ssK");
                        }
                        if (Occurrencerow["EndDate"] != DBNull.Value)
                        {
                            DateTimeOffset? EndDate = DateTime.SpecifyKind(Convert.ToDateTime(Occurrencerow["EndDate"]), DateTimeKind.Utc);
                            if (!string.IsNullOrEmpty(EndDate?.ToString()))
                                oOccurrence.End = EndDate?.UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ssK");
                        }
                        oOccurrence.Href = Settings.GetAppSetting("sessionDetailAPI") + eventId + "/subevent/" + oOccurrence.Start;
                        session.Occurrence.Add(oOccurrence);
                    }
                }

                #endregion

                #region Custom Feed
                if (dtCustomFeed?.Rows?.Count > 0)
                {
                    var dictionary = (IDictionary<string, object>)session;
                    foreach (DataRow childRow in dtCustomFeed.Rows)
                    {
                        if (childRow["ColumnName"] != DBNull.Value)
                            dictionary.Add("beta:" + Convert.ToString(childRow["ColumnName"]), Convert.ToString(childRow["Value"]));
                    }
                }
                #endregion
            }

            return session;
        }
        #endregion

        #region Activity
        public static List<string> GetAllActivities()
        {
            List<string> activities = new List<string>();
            try
            {
                var dt = DBProvider.GetDataTable("GetAllActivities", CommandType.StoredProcedure);

                if (dt != null && dt.Rows.Count > 0)
                {
                    int rowCount = dt.Rows.Count;

                    for (int i = 0; i < rowCount; i++)
                    {
                        var row = dt.Rows[i];
                        var activity = Convert.ToString(row["PrefLabel"]);
                        activities.Add(activity);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertErrorLogs("[DataLaundryApi] FeedHelper", "GetAllActivities", ex.Message, ex.InnerException?.Message, ex.StackTrace);
            }
            return activities;
        }

        public static List<string> GetActivities(double? latitude, double? longitude, double? radius,
                                            string source = null, string kind = null, string tag = null, string excludeTag = null,
                                            string disabilitySupport = null, string weekdays = null, double? minCost = null,
                                            double? maxCost = null, string gender = null, long? minTime = null,
                                            long? maxTime = null, long? minAge = null, long? maxAge = null,
                                            string from = null, string to = null)
        {
            List<string> activities = new List<string>();
            try
            {
                var lstSqlParameter = new List<SqlParameter>();
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@lat", SqlDbType = SqlDbType.Decimal, Value = (object)latitude ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@long", SqlDbType = SqlDbType.Decimal, Value = (object)longitude ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@radius", SqlDbType = SqlDbType.Decimal, Value = (object)radius ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@disabilitySupport", SqlDbType = SqlDbType.NVarChar, Value = (object)disabilitySupport ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@gender", SqlDbType = SqlDbType.NVarChar, Value = (object)gender ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@minTime", SqlDbType = SqlDbType.BigInt, Value = (object)minTime ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@maxTime", SqlDbType = SqlDbType.BigInt, Value = (object)maxTime ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@minAge", SqlDbType = SqlDbType.BigInt, Value = (object)minAge ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@maxAge", SqlDbType = SqlDbType.BigInt, Value = (object)maxAge ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@weekdays", SqlDbType = SqlDbType.NVarChar, Value = (object)weekdays ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@from", SqlDbType = SqlDbType.NVarChar, Value = (object)from ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@to", SqlDbType = SqlDbType.NVarChar, Value = (object)to ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@source", SqlDbType = SqlDbType.NVarChar, Value = (object)source ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@kind", SqlDbType = SqlDbType.NVarChar, Value = (object)kind ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@tag", SqlDbType = SqlDbType.NVarChar, Value = (object)tag ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@excludeTag", SqlDbType = SqlDbType.NVarChar, Value = (object)excludeTag ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@minCost", SqlDbType = SqlDbType.Decimal, Value = (object)minCost ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@maxCost", SqlDbType = SqlDbType.Decimal, Value = (object)maxCost ?? DBNull.Value });

                //var dt = DBProvider.GetDataTable("GetActivities", CommandType.StoredProcedure, ref lstSqlParameter);
                var dt = DBProvider.GetDataTable("GetActivities_v1", CommandType.StoredProcedure, ref lstSqlParameter);

                if (dt != null && dt.Rows.Count > 0)
                {
                    int rowCount = dt.Rows.Count;

                    for (int i = 0; i < rowCount; i++)
                    {
                        var row = dt.Rows[i];
                        var activity = Convert.ToString(row["PrefLabel"]);
                        activities.Add(activity);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertErrorLogs("[DataLaundryApi] FeedHelper", "GetActivities", ex.Message, ex.InnerException?.Message, ex.StackTrace);
            }
            return activities;
        }
            

     public static List<SessionDetails> GetEventAddressWise(string activityName = null, string location = null,string Organizer = null,decimal? latitude = null, decimal? longitude = null,string sessionid=null)
        {
            List<SessionDetails> sessions = new List<SessionDetails>();

            //List<SessionDetails> distinct = sessions.GroupBy(x => x.Data.Location).Select(g => g.First()).ToList();
            try
            {
                var lstSqlParameter = new List<SqlParameter>();

                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@activityName", SqlDbType = SqlDbType.NVarChar, Value = (object)activityName ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@location", SqlDbType = SqlDbType.NVarChar, Value = (object)location ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@lat", SqlDbType = SqlDbType.Decimal, Value = (object)latitude ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@long", SqlDbType = SqlDbType.Decimal, Value = (object)longitude ?? DBNull.Value });
                var dt = DBProvider.GetDataTable("GetSimilarFilteredEvents_v1", CommandType.StoredProcedure, ref lstSqlParameter);

                if (dt != null && dt.Rows.Count > 0)
                {
                    int rowCount = dt.Rows.Count;

                    for (int i = 0; i < rowCount; i++)
                    {
                        var row = dt.Rows[i];

                        var sessionDetails = new SessionDetails()
                        {
                            State = Convert.ToString(row["State"]),
                            Modified = Convert.ToInt64(row["ModifiedDateTimestamp"]),
                            Id = Convert.ToInt64(row["Id"])
                        };

                        if (sessionDetails.State.ToLower() == "updated")
                        {
                            var session = GetSessionFromDataRow(row, sessionDetails.Id);
                            TimeSpan ts = XmlConvert.ToTimeSpan(row["Duration"].ToString());
                            session.Duration = Convert.ToInt32(ts.TotalMinutes).ToString();
                                       

                            sessionDetails.Data = session;
                        }

                        if(sessions.Any(xa=>xa.Id == sessionDetails.Id)   == false){
                            if(sessionDetails.Data.SessionId.ToLower() != sessionid.ToLower()){
                                sessions.Add(sessionDetails);
                            }
                         
                         
                         }
                         
                    }
                }
               
            
            }
            catch (Exception ex)
            {
               // LogHelper.InsertErrorLogs("[DataLaundryApi] FeedHelper", "GetEventActivityWise", ex.Message, ex.InnerException?.Message, ex.StackTrace);
            }


            System.Collections.IList list = sessions;
           // for (int i = 0; i < list.Count;i++)
            // {x.Data.Location[0].Address
                //List<SessionDetails> listAddress = sessions.Select(x => x.Data).Where(XmlAttribute);
                IEnumerable<SessionDetails> x =  
from p in sessions  
where p.Data.Location[0].Address == location  && p.Data.Activity[0].PrefLabel.Contains(activityName) && p.Data.Organizer.Name == Organizer
select p; 
        //  x=  x.DistinctBy(sessions.Id);
            var    listAddress = x.ToList();
                            

//}
                return listAddress;
   //  List<SessionDetails> list1 = sessions.Select(o => o.Data.Location).Distinct();
   
        }
        public static List<SessionDetails> GetEventActivityWise(string activityName = null, string location = null, string latitude = null, string longitude = null)
        {
            List<SessionDetails> sessions = new List<SessionDetails>();
           
           // List<SessionDetails> distinct = sessions.GroupBy(x => x.Data.Location).Select(g => g.First()).ToList();
            try
            {
                var lstSqlParameter = new List<SqlParameter>();

                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@activityName", SqlDbType = SqlDbType.NVarChar, Value = (object)activityName ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@location", SqlDbType = SqlDbType.NVarChar, Value = (object)location ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@lat", SqlDbType = SqlDbType.NVarChar, Value = (object)latitude ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@long", SqlDbType = SqlDbType.NVarChar, Value = (object)longitude ?? DBNull.Value });

                var dt = DBProvider.GetDataTable("GetEventActivityWise", CommandType.StoredProcedure, ref lstSqlParameter);

                if (dt != null && dt.Rows.Count > 0)
                {
                    int rowCount = dt.Rows.Count;

                    for (int i = 0; i < rowCount; i++)
                    {
                        var row = dt.Rows[i];

                        var sessionDetails = new SessionDetails()
                        {
                            State = Convert.ToString(row["State"]),
                            Modified = Convert.ToInt64(row["ModifiedDateTimestamp"]),
                            Id = Convert.ToInt64(row["Id"])
                        };
        
                        if (sessionDetails.State.ToLower() == "updated")
                        {
                            var session = GetSessionFromDataRow(row, sessionDetails.Id);
                            sessionDetails.Data = session;
                        }
                        sessions.Add(sessionDetails);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertErrorLogs("[DataLaundryApi] FeedHelper", "GetEventActivityWise", ex.Message, ex.InnerException?.Message, ex.StackTrace);
            }


            System.Collections.IList list = sessions;
           // for (int i = 0; i < list.Count;i++)
            // {
              //  List<SessionDetails> distinct = sessions.GroupBy(x => x.Data.Location[0].Address).Select(g => g.First()).ToList();
                
               // sessions = distinct.ToList();

                //}
                return sessions;
   //  List<SessionDetails> list1 = sessions.Select(o => o.Data.Location).Distinct();
   
        }
        #endregion

        #region Disabilities         
        public static List<string> GetDisabilities(double? latitude, double? longitude, double? radius,
                                            string source = null, string kind = null, string tag = null, string excludeTag = null,
                                            string activity = null, string weekdays = null, double? minCost = null,
                                            double? maxCost = null, string gender = null, long? minTime = null,
                                            long? maxTime = null, long? minAge = null, long? maxAge = null,
                                            string from = null, string to = null)
        {
            List<string> disabilities = new List<string>();
            try
            {
                var lstSqlParameter = new List<SqlParameter>();

                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@lat", SqlDbType = SqlDbType.Decimal, Value = (object)latitude ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@long", SqlDbType = SqlDbType.Decimal, Value = (object)longitude ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@radius", SqlDbType = SqlDbType.Decimal, Value = (object)radius ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@activity", SqlDbType = SqlDbType.NVarChar, Value = (object)activity ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@gender", SqlDbType = SqlDbType.NVarChar, Value = (object)gender ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@minTime", SqlDbType = SqlDbType.BigInt, Value = (object)minTime ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@maxTime", SqlDbType = SqlDbType.BigInt, Value = (object)maxTime ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@minAge", SqlDbType = SqlDbType.BigInt, Value = (object)minAge ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@maxAge", SqlDbType = SqlDbType.BigInt, Value = (object)maxAge ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@weekdays", SqlDbType = SqlDbType.NVarChar, Value = (object)weekdays ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@from", SqlDbType = SqlDbType.NVarChar, Value = (object)from ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@to", SqlDbType = SqlDbType.NVarChar, Value = (object)to ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@source", SqlDbType = SqlDbType.NVarChar, Value = (object)source ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@kind", SqlDbType = SqlDbType.NVarChar, Value = (object)kind ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@tag", SqlDbType = SqlDbType.NVarChar, Value = (object)tag ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@excludeTag", SqlDbType = SqlDbType.NVarChar, Value = (object)excludeTag ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@minCost", SqlDbType = SqlDbType.Decimal, Value = (object)minCost ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@maxCost", SqlDbType = SqlDbType.Decimal, Value = (object)maxCost ?? DBNull.Value });

                //var dt = DBProvider.GetDataTable("GetDisabilities", CommandType.StoredProcedure, ref lstSqlParameter);
                var dt = DBProvider.GetDataTable("GetDisabilities_v1", CommandType.StoredProcedure, ref lstSqlParameter);

                if (dt != null && dt.Rows.Count > 0)
                {
                    int rowCount = dt.Rows.Count;

                    for (int i = 0; i < rowCount; i++)
                    {
                        var row = dt.Rows[i];
                        var disability = Convert.ToString(row["AccessibilitySupport"]);
                        if (!string.IsNullOrEmpty(disability))
                            disabilities.Add(disability);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertErrorLogs("[DataLaundryApi] FeedHelper", "GetDisabilities", ex.Message, ex.InnerException?.Message, ex.StackTrace);
            }
            return disabilities;
        }
        #endregion

        #region Events with filteration      
        //private static OpportunityDetail GetOpportunityFromDataRow(DataRow row, long eventId)
        //{
        //    var opportunity = new OpportunityDetail();
        //    try
        //    {
        //        opportunity.Description = Convert.ToString(row["Description"]);

        //        if (row["Category"] != DBNull.Value)
        //            opportunity.Tags = row["Category"].ToString().Split(',');

        //        //if (row["AgeRange"] != DBNull.Value)
        //        //{
        //        //    string[] ages = Convert.ToString(row["AgeRange"]).Split('-');
        //        //    opportunity.Restrictions.MinAge = Convert.ToInt16(ages[0].Trim());
        //        //    if (ages.Length > 1)
        //        //        opportunity.Restrictions.MaxAge = Convert.ToInt16(ages[1].Trim());
        //        //}

        //        if (row["MinAge"] != DBNull.Value)
        //            opportunity.Restrictions.MinAge = Convert.ToInt32(row["MinAge"]);

        //        if (row["MaxAge"] != DBNull.Value)
        //            opportunity.Restrictions.MaxAge = Convert.ToInt32(row["MaxAge"]);

        //        if (row["GenderRestriction"] != DBNull.Value)
        //            opportunity.Restrictions.Gender = Convert.ToString(row["GenderRestriction"]);

        //        if (row["WeekDays"] != DBNull.Value)
        //            opportunity.Weekdays = row["WeekDays"].ToString().Split(',').Select(n => Convert.ToInt32(n)).ToArray();

        //        opportunity.CheckoutUrl = "/" + opportunity.Id;

        //        if (row["StartDate"] != DBNull.Value)
        //        {
        //            var objOccurence = new Occurrences();
        //            DateTimeOffset? StartDate = DateTime.SpecifyKind(Convert.ToDateTime(row["StartDate"]), DateTimeKind.Utc);

        //            if (!string.IsNullOrEmpty(StartDate?.ToString()))
        //                objOccurence.Start = StartDate?.UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ssK");

        //            if (row["EndDate"] != DBNull.Value)
        //            {
        //                DateTimeOffset? EndDate = DateTime.SpecifyKind(Convert.ToDateTime(row["EndDate"]), DateTimeKind.Utc);
        //                if (!string.IsNullOrEmpty(EndDate?.ToString()))
        //                    objOccurence.End = EndDate?.UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ssK");
        //            }
        //            objOccurence.CheckoutUrl = Settings.GetAppSetting("sessionDetailAPI") + opportunity.Id;

        //            opportunity.Occurrences.Add(objOccurence);
        //        }

        //        if (row["Duration"] != DBNull.Value)
        //        {
        //            TimeSpan ts = XmlConvert.ToTimeSpan(row["Duration"].ToString());
        //            opportunity.DurationMins = Convert.ToInt16(ts.TotalMinutes);
        //        }

        //        opportunity.Title = Convert.ToString(row["Name"]);

        //        if (row["AccessibilitySupport"] != DBNull.Value)
        //            opportunity.Categories.DisabilitySupport = Convert.ToString(row["AccessibilitySupport"]).Split(',');

        //        opportunity.Id = Convert.ToString(row["Id"]);

        //        if (row["Image"] != DBNull.Value)
        //            opportunity.Image = Convert.ToString(row["Image"]);

        //        opportunity.Href = Settings.GetAppSetting("sessionDetailAPI") + opportunity.Id;

        //        #region get event data
        //        var dsOtherEventData = GetOtherSessionData(eventId);

        //        if (dsOtherEventData != null && dsOtherEventData.Tables.Count > 0)
        //        {
        //            var dtSubEvents = dsOtherEventData.Tables[0];
        //            var dtSuperEvent = dsOtherEventData.Tables[1];
        //            var dtLocation = dsOtherEventData.Tables[2];
        //            var dtPhysicalAcitivity = dsOtherEventData.Tables[4];
        //            var dtEventSchedule = dsOtherEventData.Tables[5];
        //            var dtOrganization = dsOtherEventData.Tables[6];

        //            #region Location
        //            if (dtLocation?.Rows?.Count > 0)
        //            {
        //                foreach (DataRow childRow in dtLocation.Rows)
        //                {
        //                    if (childRow["Lat"] != DBNull.Value)
        //                        opportunity.Location.Coordinates.Lat = Convert.ToDecimal(childRow["Lat"]);
        //                    if (childRow["Long"] != DBNull.Value)
        //                        opportunity.Location.Coordinates.Lng = Convert.ToDecimal(childRow["Long"]);
        //                    if (childRow["Address"] != DBNull.Value)
        //                        opportunity.Location.Address = Convert.ToString(childRow["Address"]);
        //                    break;
        //                }
        //            }
        //            #endregion Location

        //            #region Activity
        //            if (dtPhysicalAcitivity?.Rows?.Count > 0)
        //            {
        //                List<string> items = new List<string>();
        //                foreach (DataRow childRow in dtPhysicalAcitivity.Rows)
        //                    items.Add(Convert.ToString(childRow["PrefLabel"]));

        //                opportunity.Categories.Activities = items.ToArray();
        //            }
        //            #endregion Activity

        //            #region Organization
        //            if (dtOrganization?.Rows?.Count > 0)
        //            {
        //                foreach (DataRow childRow in dtOrganization.Rows)
        //                {
        //                    if (childRow["Name"] != DBNull.Value)
        //                        opportunity.Contact.PointOfContact.Name = Convert.ToString(childRow["Name"]);
        //                    if (childRow["Email"] != DBNull.Value)
        //                        opportunity.Contact.PointOfContact.Email = Convert.ToString(childRow["Email"]);
        //                    if (childRow["Url"] != DBNull.Value)
        //                        opportunity.Contact.Organisation.Website = Convert.ToString(childRow["Url"]);
        //                    if (childRow["Telephone"] != DBNull.Value)
        //                        opportunity.Contact.Organisation.Telephone = Convert.ToString(childRow["Telephone"]);
        //                    break;
        //                }
        //            }
        //            #endregion Organization

        //            #region SubEvent
        //            if (dtSubEvents?.Rows?.Count > 0)
        //            {
        //                opportunity.Occurrences = new List<Occurrences>();

        //                foreach (DataRow childRow in dtSubEvents.Rows)
        //                {
        //                    long subEventId = Convert.ToInt64(childRow["Id"]);
        //                    var subevent = GetSessionFromDataRow(childRow, subEventId);

        //                    var occurance = new Occurrences();
        //                    occurance.Start = subevent.StartDate?.ToString();
        //                    occurance.End = subevent.EndDate?.ToString();
        //                    occurance.CheckoutUrl = subevent.Url?.ToString();

        //                    opportunity.Occurrences.Add(occurance);
        //                }
        //            }
        //            #endregion SubEvent

        //            #region EventSchedule
        //            //if (dtEventSchedule?.Rows?.Count > 0)
        //            //{
        //            //    foreach (DataRow childRow in dtEventSchedule.Rows)
        //            //    {
        //            //        var eventSchedule = new EventSchedule();

        //            //        if (childRow["StartDate"] != DBNull.Value)
        //            //            eventSchedule.StartDate = DateTime.SpecifyKind(Convert.ToDateTime(row["StartDate"]), DateTimeKind.Utc).ToString("yyyy-MM-dd");
        //            //        //eventSchedule.StartDate = DateTime.Parse(Convert.ToString(childRow["StartDate"]), CultureInfo.CurrentCulture, DateTimeStyles.AssumeUniversal).ToString("yyyy-MM-dd");

        //            //        if (childRow["EndDate"] != DBNull.Value)
        //            //            eventSchedule.EndDate = DateTime.SpecifyKind(Convert.ToDateTime(row["EndDate"]), DateTimeKind.Utc).ToString("yyyy-MM-dd");

        //            //        if (childRow["StartTime"] != DBNull.Value)
        //            //            eventSchedule.StartTime = Convert.ToString(childRow["StartTime"]);

        //            //        if (childRow["EndTime"] != DBNull.Value)
        //            //            eventSchedule.EndTime = Convert.ToString(childRow["EndTime"]);

        //            //        if (childRow["Frequency"] != DBNull.Value)
        //            //            eventSchedule.Frequency = Convert.ToString(childRow["Frequency"]);

        //            //        if (childRow["ByDay"] != DBNull.Value)
        //            //            eventSchedule.ByDay = Convert.ToString(childRow["ByDay"]).Split(',');

        //            //        if (childRow["ByMonth"] != DBNull.Value)
        //            //            eventSchedule.ByMonth = Convert.ToString(childRow["ByMonth"]).Split(',');

        //            //        if (childRow["ByMonthDay"] != DBNull.Value)
        //            //            eventSchedule.ByMonthDay = Convert.ToString(childRow["ByMonthDay"]).Split(',');

        //            //        if (childRow["RepeatCount"] != DBNull.Value)
        //            //            eventSchedule.RepeatCount = Convert.ToInt32(childRow["RepeatCount"]);

        //            //        if (childRow["RepeatFrequency"] != DBNull.Value)
        //            //            eventSchedule.RepeatFrequency = Convert.ToString(childRow["RepeatFrequency"]);

        //            //        opportunity.EventSchedule = eventSchedule;
        //            //        break;
        //            //    }
        //            //}
        //            #endregion EventSchedule
        //        }
        //        #endregion
        //    }
        //    catch (Exception ex)
        //    {
        //        LogHelper.InsertErrorLogs("[DataLaundryApi] FeedHelper", "GetOpportunityFromDataRow", ex.Message, ex.InnerException?.Message, ex.StackTrace);
        //    }
        //    return opportunity;
        //}

        #region By Opportunity Model
        private static OpportunityDetail GetOpportunitySessionFromDataRow(DataRow row, long eventId)
        {
            var opportunity = new OpportunityDetail();
            try
            {
                opportunity.Description = Convert.ToString(row["Description"]);

                if (row["Category"] != DBNull.Value)
                    opportunity.Tags = row["Category"].ToString().Split(',').Where(x => !string.IsNullOrEmpty(x)).ToArray(); ;

                //if (row["AgeRange"] != DBNull.Value)
                //{
                //    string[] ages = Convert.ToString(row["AgeRange"]).Split('-');
                //    opportunity.Restrictions.MinAge = Convert.ToInt16(ages[0].Trim());
                //    if (ages.Length > 1)
                //        opportunity.Restrictions.MaxAge = Convert.ToInt16(ages[1].Trim());
                //}

                if (row["MinAge"] != DBNull.Value)
                    opportunity.Restrictions.MinAge = Convert.ToInt32(row["MinAge"]);

                if (row["MaxAge"] != DBNull.Value)
                    opportunity.Restrictions.MaxAge = Convert.ToInt32(row["MaxAge"]);

                if (row["GenderRestriction"] != DBNull.Value)
                    opportunity.Restrictions.Gender = Convert.ToString(row["GenderRestriction"]);

                if (row["WeekDays"] != DBNull.Value)
                    opportunity.Weekdays = row["WeekDays"].ToString().Split(',').Select(n => Convert.ToInt32(n)).ToArray();

                if (row["Duration"] != DBNull.Value)
                {
                    TimeSpan ts = XmlConvert.ToTimeSpan(row["Duration"].ToString());
                    opportunity.DurationMins = Convert.ToInt32(ts.TotalMinutes);
                }

                opportunity.Title = Convert.ToString(row["Name"]);

                if (row["AccessibilitySupport"] != DBNull.Value)
                    opportunity.Categories.DisabilitySupport = Convert.ToString(row["AccessibilitySupport"]).Split(',');

                if (row["Activity"] != DBNull.Value)
                    opportunity.Categories.Activities = Convert.ToString(row["Activity"]).Split(',');

                //opportunity.Id = Convert.ToString(row["Id"]);
                opportunity.Id = Convert.ToString(row["FeedName"]) + "-" + Convert.ToString(row["FeedId"]);

                if (row["Image"] != DBNull.Value)
                    opportunity.Image = Convert.ToString(row["Image"]);

                opportunity.Href = Settings.GetAppSetting("sessionDetailAPI") + opportunity.Id;
                //opportunity.CheckoutUrl = Settings.GetAppSetting("checkoutUrl") + opportunity.Id;

                #region get event data
                var dsOtherEventData = GetOtherSessionData(eventId);

                if (dsOtherEventData != null && dsOtherEventData.Tables.Count > 0)
                {
                    //var dtPhysicalAcitivity = dsOtherEventData.Tables[4];
                    var dtLocation = dsOtherEventData.Tables[2];
                    var dtOrganization = dsOtherEventData.Tables[6];
                    var dtSubEvents = dsOtherEventData.Tables[0];
                    var dtEventSchedule = dsOtherEventData.Tables[5];
                    var dtOccurrence = dsOtherEventData.Tables[9];

                    #region Activity
                    //if (dtPhysicalAcitivity?.Rows?.Count > 0)
                    //{
                    //    List<string> items = new List<string>();
                    //    foreach (DataRow childRow in dtPhysicalAcitivity.Rows)
                    //        items.Add(Convert.ToString(childRow["PrefLabel"]));

                    //    opportunity.Categories.Activities = items.ToArray();
                    //}
                    #endregion Activity
                    #region Location
                    if (dtLocation?.Rows?.Count > 0)
                    {
                        foreach (DataRow childRow in dtLocation.Rows)
                        {
                            if (childRow["Lat"] != DBNull.Value)
                                opportunity.Location.Coordinates.Lat = Convert.ToDecimal(childRow["Lat"]);
                            if (childRow["Long"] != DBNull.Value)
                                opportunity.Location.Coordinates.Lng = Convert.ToDecimal(childRow["Long"]);
                            if (childRow["Address"] != DBNull.Value)
                                opportunity.Location.Address = Convert.ToString(childRow["Address"]);
                            if (row["Distance"] != DBNull.Value)
                                opportunity.Location.Distance = Convert.ToDecimal(row["Distance"]);
                            break;
                        }
                    }
                    #endregion Location 
                    #region Organization
                    if (dtOrganization?.Rows?.Count > 0)
                    {
                        foreach (DataRow childRow in dtOrganization.Rows)
                        {
                            if (childRow["Name"] != DBNull.Value)
                                opportunity.Contact.PointOfContact.Name = Convert.ToString(childRow["Name"]);
                            if (childRow["Email"] != DBNull.Value)
                                opportunity.Contact.PointOfContact.Email = Convert.ToString(childRow["Email"]);
                            if (childRow["Url"] != DBNull.Value)
                                opportunity.Contact.Organisation.Website = Convert.ToString(childRow["Url"]);
                            if (childRow["Telephone"] != DBNull.Value)
                                opportunity.Contact.Organisation.Telephone = Convert.ToString(childRow["Telephone"]);
                            break;
                        }
                    }
                    #endregion Organization
                    #region Occurences   
                    if (dtOccurrence?.Rows?.Count > 0)
                    {
                        foreach (DataRow childRow in dtOccurrence.Rows)
                        {
                            long subEventId = 0;
                            var occurance = new Occurrences();

                            if (childRow["SubEventId"] != DBNull.Value)
                                subEventId = Convert.ToInt64(childRow["SubEventId"]);

                            if (childRow["StartDate"] != DBNull.Value)
                            {
                                DateTimeOffset? StartDate = DateTime.SpecifyKind(Convert.ToDateTime(childRow["StartDate"]), DateTimeKind.Utc);
                                if (!string.IsNullOrEmpty(StartDate?.ToString()))
                                    occurance.Start = StartDate?.UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ssK");
                            }
                            if (childRow["EndDate"] != DBNull.Value)
                            {
                                DateTimeOffset? EndDate = DateTime.SpecifyKind(Convert.ToDateTime(childRow["EndDate"]), DateTimeKind.Utc);
                                if (!string.IsNullOrEmpty(EndDate?.ToString()))
                                    occurance.End = EndDate?.UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ssK");
                            }

                            //occurance.Href =  Settings.GetAppSetting("sessionDetailAPI") + opportunity.Id + (subEventId > 0 ? ("/subevent/" + occurance.Start) : "");
                            occurance.Href = Settings.GetAppSetting("sessionDetailAPI") + opportunity.Id + "/subevent/" + occurance.Start;
                            //occurance.CheckoutUrl =  Settings.GetAppSetting("checkoutUrl") + opportunity.Id + "/subevent/" + occurance.Start;

                            opportunity.Occurrences.Add(occurance);
                        }
                    }
                    #endregion
                }
                #endregion
            }
            catch (Exception ex)
            {
                LogHelper.InsertErrorLogs("[DataLaundryApi] FeedHelper", "GetOpportunitySessionFromDataRow", ex.Message, ex.InnerException?.Message, ex.StackTrace);
            }
            return opportunity;
        }
        public static OpportunityContainer GetEvents(double? latitude, double? longitude, double? radius,
                                            string source = null, string kind = null, string tag = null, string excludeTag = null,
                                            string activity = null, string disabilitySupport = null, string weekdays = null,
                                            double? minCost = null, double? maxCost = null, string gender = null,
                                            string sortMode = null, long? minTime = null, long? maxTime = null,
                                            long? minAge = null, long? maxAge = null, long? page = 1,
                                            long? limit = 100, string from = null, string to = null)
        {
            OpportunityContainer eventContainer = new OpportunityContainer();
            eventContainer.Count = 0;
            eventContainer.Limit = limit;
            eventContainer.Page = page;

            try
            {
                var lstSqlParameter = new List<SqlParameter>();
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@lat", SqlDbType = SqlDbType.Decimal, Value = (object)latitude ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@long", SqlDbType = SqlDbType.Decimal, Value = (object)longitude ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@radius", SqlDbType = SqlDbType.Decimal, Value = (object)radius ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@source", SqlDbType = SqlDbType.NVarChar, Value = (object)source ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@kind", SqlDbType = SqlDbType.NVarChar, Value = (object)kind ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@tag", SqlDbType = SqlDbType.NVarChar, Value = (object)tag ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@excludeTag", SqlDbType = SqlDbType.NVarChar, Value = (object)excludeTag ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@activity", SqlDbType = SqlDbType.NVarChar, Value = (object)activity ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@disabilitySupport", SqlDbType = SqlDbType.NVarChar, Value = (object)disabilitySupport ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@weekdays", SqlDbType = SqlDbType.NVarChar, Value = (object)weekdays ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@minCost", SqlDbType = SqlDbType.Decimal, Value = (object)minCost ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@maxCost", SqlDbType = SqlDbType.Decimal, Value = (object)maxCost ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@gender", SqlDbType = SqlDbType.NVarChar, Value = (object)gender ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@sortMode", SqlDbType = SqlDbType.NVarChar, Value = (object)sortMode ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@minTime", SqlDbType = SqlDbType.BigInt, Value = (object)minTime ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@maxTime", SqlDbType = SqlDbType.BigInt, Value = (object)maxTime ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@minAge", SqlDbType = SqlDbType.BigInt, Value = (object)minAge ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@maxAge", SqlDbType = SqlDbType.BigInt, Value = (object)maxAge ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@page", SqlDbType = SqlDbType.BigInt, Value = (object)page ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@limit", SqlDbType = SqlDbType.BigInt, Value = (object)limit ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@from", SqlDbType = SqlDbType.NVarChar, Value = (object)from ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@to", SqlDbType = SqlDbType.NVarChar, Value = (object)to ?? DBNull.Value });

                //var ds = DBProvider.GetDataSet("GetFilteredEvents", CommandType.StoredProcedure, ref lstSqlParameter);
                var ds = DBProvider.GetDataSet("GetFilteredEvents_v1", CommandType.StoredProcedure, ref lstSqlParameter);
                //var ds = DBProvider.GetDataSet("GetFilteredEvents_v2", CommandType.StoredProcedure, ref lstSqlParameter);

                if (ds != null && ds.Tables.Count > 0)
                {
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        DataTable dt_Events = ds.Tables[0];
                        int rowCount = dt_Events.Rows.Count;

                        eventContainer.Count = Convert.ToInt32(ds.Tables[1].Rows.Count > 0 ? ds.Tables[1].Rows[0]["TotalCount"].ToString() : "0");

                        for (int i = 0; i < rowCount; i++)
                        {
                            var row = dt_Events.Rows[i];
                            var opportunity = GetOpportunitySessionFromDataRow(row, Convert.ToInt64(row["Id"]));
                            eventContainer.Items.Add(opportunity);

                            #region build next page url in the end based on 
                            if ((i + 1) == rowCount && eventContainer.Count > limit && rowCount >= limit && rowCount <= eventContainer.Count)
                            {
                                if (string.IsNullOrEmpty(eventContainer.Next))
                                    eventContainer.Next = Settings.GetAppSetting("sessionAPI");

                                eventContainer.Next += eventContainer.Next.Contains("?") ? "&" : "?";

                                #region required params
                                eventContainer.Next += "lat=" + latitude;
                                eventContainer.Next += "&";
                                eventContainer.Next += "lng=" + longitude;
                                eventContainer.Next += "&";
                                eventContainer.Next += "radius=" + radius;
                                eventContainer.Next += "&";
                                eventContainer.Next += "page=" + (page + 1);
                                eventContainer.Next += "&";
                                eventContainer.Next += "limit=" + limit;
                                #endregion

                                #region Optional Params
                                eventContainer.Next += GetUrlFromGivenString(eventContainer.Next, source, "source");
                                eventContainer.Next += GetUrlFromGivenString(eventContainer.Next, activity, "activity");
                                eventContainer.Next += GetUrlFromGivenString(eventContainer.Next, disabilitySupport, "disabilitySupport");
                                eventContainer.Next += GetUrlFromGivenString(eventContainer.Next, weekdays, "weekdays");
                                eventContainer.Next += GetUrlFromGivenString(eventContainer.Next, kind, "kind");
                                eventContainer.Next += GetUrlFromGivenString(eventContainer.Next, tag, "tag");
                                eventContainer.Next += GetUrlFromGivenString(eventContainer.Next, excludeTag, "excludeTag");
                                eventContainer.Next += GetUrlFromGivenString(eventContainer.Next, sortMode, "sortMode");
                                eventContainer.Next += GetUrlFromGivenString(eventContainer.Next, gender, "gender");
                                eventContainer.Next += GetUrlFromGivenString(eventContainer.Next, from, "from");
                                eventContainer.Next += GetUrlFromGivenString(eventContainer.Next, to, "to");
                                eventContainer.Next += GetUrlFromGivenString(eventContainer.Next, minTime?.ToString(), "minTime");
                                eventContainer.Next += GetUrlFromGivenString(eventContainer.Next, maxTime?.ToString(), "maxTime");
                                eventContainer.Next += GetUrlFromGivenString(eventContainer.Next, minAge?.ToString(), "minAge");
                                eventContainer.Next += GetUrlFromGivenString(eventContainer.Next, maxAge?.ToString(), "maxAge");
                                eventContainer.Next += GetUrlFromGivenString(eventContainer.Next, minCost?.ToString(), "minCost");
                                eventContainer.Next += GetUrlFromGivenString(eventContainer.Next, maxCost?.ToString(), "maxCost");
                                #endregion
                            }
                            #endregion

                            #region build prev page url in the end based on 
                            if ((i + 1) == rowCount && eventContainer.Count > limit && page > 1)
                            {
                                if (string.IsNullOrEmpty(eventContainer.Prev))
                                    eventContainer.Prev = Settings.GetAppSetting("sessionAPI");

                                eventContainer.Prev += eventContainer.Prev.Contains("?") ? "&" : "?";

                                #region required params
                                eventContainer.Prev += "lat=" + latitude;
                                eventContainer.Prev += "&";
                                eventContainer.Prev += "lng=" + longitude;
                                eventContainer.Prev += "&";
                                eventContainer.Prev += "radius=" + radius;
                                eventContainer.Prev += "&";
                                eventContainer.Prev += "page=" + (page - 1);
                                eventContainer.Prev += "&";
                                eventContainer.Prev += "limit=" + limit;
                                #endregion

                                #region Optional Params
                                eventContainer.Prev += GetUrlFromGivenString(eventContainer.Prev, source, "source");
                                eventContainer.Prev += GetUrlFromGivenString(eventContainer.Prev, activity, "activity");
                                eventContainer.Prev += GetUrlFromGivenString(eventContainer.Prev, disabilitySupport, "disabilitySupport");
                                eventContainer.Prev += GetUrlFromGivenString(eventContainer.Prev, weekdays, "weekdays");
                                eventContainer.Prev += GetUrlFromGivenString(eventContainer.Prev, kind, "kind");
                                eventContainer.Prev += GetUrlFromGivenString(eventContainer.Prev, tag, "tag");
                                eventContainer.Prev += GetUrlFromGivenString(eventContainer.Prev, excludeTag, "excludeTag");
                                eventContainer.Prev += GetUrlFromGivenString(eventContainer.Prev, sortMode, "sortMode");
                                eventContainer.Prev += GetUrlFromGivenString(eventContainer.Prev, gender, "gender");
                                eventContainer.Prev += GetUrlFromGivenString(eventContainer.Prev, from, "from");
                                eventContainer.Prev += GetUrlFromGivenString(eventContainer.Prev, to, "to");
                                eventContainer.Prev += GetUrlFromGivenString(eventContainer.Prev, minTime?.ToString(), "minTime");
                                eventContainer.Prev += GetUrlFromGivenString(eventContainer.Prev, maxTime?.ToString(), "maxTime");
                                eventContainer.Prev += GetUrlFromGivenString(eventContainer.Prev, minAge?.ToString(), "minAge");
                                eventContainer.Prev += GetUrlFromGivenString(eventContainer.Prev, maxAge?.ToString(), "maxAge");
                                eventContainer.Prev += GetUrlFromGivenString(eventContainer.Prev, minCost?.ToString(), "minCost");
                                eventContainer.Prev += GetUrlFromGivenString(eventContainer.Prev, maxCost?.ToString(), "maxCost");
                                #endregion
                            }
                            #endregion
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertErrorLogs("[DataLaundryApi] FeedHelper", "GetEvents", ex.Message, ex.InnerException?.Message, ex.StackTrace);
            }
            return eventContainer;
        }
        public static OpportunityDetail GetEventDetailsById(long? eventId = 0)
        {
            OpportunityDetail detail = new OpportunityDetail();
            try
            {
                var lstSqlParameter = new List<SqlParameter>();
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@eventId", SqlDbType = SqlDbType.BigInt, Value = (object)eventId ?? DBNull.Value });

                var ds = DBProvider.GetDataSet("GetEventById", CommandType.StoredProcedure, ref lstSqlParameter);

                if (ds.Tables[0].Rows.Count > 0)
                {
                    var row = ds.Tables[0].Rows[0];
                    detail = GetOpportunitySessionFromDataRow(row, Convert.ToInt64(row["Id"])); ;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertErrorLogs("[DataLaundryApi] FeedHelper", "GetEventDetailsById", ex.Message, ex.InnerException?.Message, ex.StackTrace);
            }
            return detail;
        }
        public static OpportunityDetail GetSubEventDetailsById(long? eventId, string startDate)
        {
            OpportunityDetail detail = new OpportunityDetail();
            try
            {
                var lstSqlParameter = new List<SqlParameter>();
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@eventId", SqlDbType = SqlDbType.BigInt, Value = (object)eventId ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@startDate", SqlDbType = SqlDbType.NVarChar, Value = (object)@startDate ?? DBNull.Value });

                var ds = DBProvider.GetDataSet("GetSubEventByEventId", CommandType.StoredProcedure, ref lstSqlParameter);

                if (ds.Tables[0].Rows.Count > 0)
                {
                    var row = ds.Tables[0].Rows[0];
                    detail = GetOpportunitySessionFromDataRow(row, Convert.ToInt64(row["Id"])); ;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertErrorLogs("[DataLaundryApi] FeedHelper", "GetSubEventDetailsById", ex.Message, ex.InnerException?.Message, ex.StackTrace);
            }
            return detail;
        }
        public static OpportunityDetail GetEventDetailsBySessionId(string sessionId, string feedName = null)
        {
            OpportunityDetail detail = new OpportunityDetail();
            try
            {
                var lstSqlParameter = new List<SqlParameter>();
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@sessionId", SqlDbType = SqlDbType.NVarChar, Value = (object)sessionId ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@FeedName", SqlDbType = SqlDbType.NVarChar, Value = (object)feedName ?? DBNull.Value });

                var ds = DBProvider.GetDataSet("GetEventBySessionId", CommandType.StoredProcedure, ref lstSqlParameter);

                if (ds.Tables[0].Rows.Count > 0)
                {
                    var row = ds.Tables[0].Rows[0];
                    detail = GetOpportunitySessionFromDataRow(row, Convert.ToInt64(row["Id"]));
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertErrorLogs("[DataLaundryApi] FeedHelper", "GetEventDetailsBySessionId", ex.Message, ex.InnerException?.Message, ex.StackTrace);
            }
            return detail;
        }
        public static OpportunityDetail GetSubEventDetailsBySessionId(string sessionId, string startDate, string feedName = null)
        {
            OpportunityDetail detail = new OpportunityDetail();
            try
            {
                var lstSqlParameter = new List<SqlParameter>();
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@sessionId", SqlDbType = SqlDbType.NVarChar, Value = (object)sessionId ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@FeedName", SqlDbType = SqlDbType.NVarChar, Value = (object)feedName ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@startDate", SqlDbType = SqlDbType.NVarChar, Value = (object)@startDate ?? DBNull.Value });

                var ds = DBProvider.GetDataSet("GetSubEventBySessionId", CommandType.StoredProcedure, ref lstSqlParameter);

                if (ds.Tables[0].Rows.Count > 0)
                {
                    var row = ds.Tables[0].Rows[0];
                    detail = GetOpportunitySessionFromDataRow(row, Convert.ToInt64(row["Id"]));
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertErrorLogs("[DataLaundryApi] FeedHelper", "GetSubEventDetailsBySessionId", ex.Message, ex.InnerException?.Message, ex.StackTrace);
            }
            return detail;
        }
        #endregion

        #region By Dynamic
        private static ExpandoObject GetOpportunityFromDatarowDynamically(DataRow row, long eventId)
        {
            dynamic objDynamicData = new ExpandoObject();
            try
            {
                objDynamicData.id = Convert.ToString(row["Id"]);
                objDynamicData.description = Convert.ToString(row["Description"]);
                objDynamicData.title = Convert.ToString(row["Name"]);
                objDynamicData.href = Settings.GetAppSetting("sessionDetailAPI") + objDynamicData.id;

                if (row["Category"] != DBNull.Value)
                    objDynamicData.tags = row["Category"].ToString().Split(',');
                else
                    objDynamicData.tags = new string[] { };

                if (row["WeekDays"] != DBNull.Value)
                    objDynamicData.weekdays = row["WeekDays"].ToString().Split(',').Select(n => Convert.ToInt32(n)).ToArray();
                else
                    objDynamicData.weekdays = new string[] { };

                if (row["AgeRange"] != DBNull.Value)
                {
                    if (!((IDictionary<string, Object>)objDynamicData).ContainsKey("restrictions"))
                        objDynamicData.restrictions = new ExpandoObject();

                    string[] ages = Convert.ToString(row["AgeRange"]).Split('-');
                    objDynamicData.restrictions.minAge = Convert.ToInt16(ages[0].Trim());
                    if (ages.Length > 1)
                        objDynamicData.restrictions.maxAge = Convert.ToInt16(ages[1].Trim());
                }
                else
                {
                    objDynamicData.restrictions = new ExpandoObject();
                    objDynamicData.restrictions.minAge = 0;
                    objDynamicData.restrictions.maxAge = 0;
                }

                if (row["GenderRestriction"] != DBNull.Value)
                {
                    if (!((IDictionary<string, Object>)objDynamicData).ContainsKey("restrictions"))
                        objDynamicData.restrictions = new ExpandoObject();
                    objDynamicData.restrictions.gender = Convert.ToString(row["GenderRestriction"]);
                }
                else
                {
                    objDynamicData.restrictions = new ExpandoObject();
                    objDynamicData.restrictions.gender = "";
                }

                if (row["Image"] != DBNull.Value)
                    objDynamicData.image = Convert.ToString(row["Image"]);
                else
                    objDynamicData.image = "";

                if (row["Duration"] != DBNull.Value)
                {
                    TimeSpan ts = XmlConvert.ToTimeSpan(row["Duration"].ToString());
                    objDynamicData.durationMins = Convert.ToInt16(ts.TotalMinutes);
                }
                else
                {
                    objDynamicData.durationMins = 0;
                }

                if (row["AccessibilitySupport"] != DBNull.Value)
                {
                    if (!((IDictionary<string, Object>)objDynamicData).ContainsKey("categories"))
                        objDynamicData.categories = new ExpandoObject();
                    objDynamicData.categories.disabilitySupport = Convert.ToString(row["AccessibilitySupport"]).Split(',');
                }
                else
                {
                    objDynamicData.categories = new ExpandoObject();
                    objDynamicData.categories.disabilitySupport = new string[] { };
                }
                #region get event data
                var dsOtherEventData = GetOtherSessionData(eventId);

                if (dsOtherEventData != null && dsOtherEventData.Tables.Count > 0)
                {
                    var dtSubEvents = dsOtherEventData.Tables[0];
                    var dtSuperEvent = dsOtherEventData.Tables[1];
                    var dtLocation = dsOtherEventData.Tables[2];
                    var dtPhysicalAcitivity = dsOtherEventData.Tables[4];
                    var dtEventSchedule = dsOtherEventData.Tables[5];
                    var dtOrganization = dsOtherEventData.Tables[6];

                    #region Location
                    if (dtLocation?.Rows?.Count > 0)
                    {
                        objDynamicData.location = new ExpandoObject();
                        objDynamicData.location.coordinates = new ExpandoObject();
                        foreach (DataRow childRow in dtLocation.Rows)
                        {
                            if (childRow["Lat"] != DBNull.Value)
                                objDynamicData.location.coordinates.lat = Convert.ToDecimal(childRow["Lat"]);
                            else
                                objDynamicData.location.coordinates.lat = 0;

                            if (childRow["Long"] != DBNull.Value)
                                objDynamicData.location.coordinates.lng = Convert.ToDecimal(childRow["Long"]);
                            else
                                objDynamicData.location.coordinates.lng = 0;

                            if (childRow["Address"] != DBNull.Value)
                                objDynamicData.location.address = Convert.ToString(childRow["Address"]);
                            else
                                objDynamicData.location.address = "";

                            if (row["Distance"] != DBNull.Value)
                                objDynamicData.location.distance = Convert.ToDecimal(row["Distance"]);
                            else
                                objDynamicData.location.distance = 0;
                            break;
                        }
                    }
                    else
                    {
                        #region ForErrorHandling AllowNullProperty
                        objDynamicData.location = new ExpandoObject();
                        objDynamicData.location.coordinates = new ExpandoObject();
                        objDynamicData.location.coordinates.lat = 0;
                        objDynamicData.location.coordinates.lng = 0;
                        objDynamicData.location.address = "";
                        objDynamicData.location.distance = 0;
                        #endregion
                    }
                    #endregion Location

                    #region Activity
                    if (dtPhysicalAcitivity?.Rows?.Count > 0)
                    {
                        List<string> items = new List<string>();
                        foreach (DataRow childRow in dtPhysicalAcitivity.Rows)
                            items.Add(Convert.ToString(childRow["PrefLabel"]));

                        if (!((IDictionary<string, Object>)objDynamicData).ContainsKey("categories"))
                            objDynamicData.categories = new ExpandoObject();

                        objDynamicData.categories.activities = items.ToArray();
                    }
                    else
                    {
                        objDynamicData.categories = new ExpandoObject();
                        objDynamicData.categories.activities = new string[] { };
                    }
                    #endregion Activity

                    #region Organization
                    if (dtOrganization?.Rows?.Count > 0)
                    {
                        objDynamicData.contact = new ExpandoObject();
                        objDynamicData.contact.pointOfContact = new ExpandoObject();
                        objDynamicData.contact.organisation = new ExpandoObject();
                        foreach (DataRow childRow in dtOrganization.Rows)
                        {
                            if (childRow["Name"] != DBNull.Value)
                                objDynamicData.contact.pointOfContact.name = Convert.ToString(childRow["Name"]);
                            else
                                objDynamicData.contact.pointOfContact.name = "";

                            if (childRow["Email"] != DBNull.Value)
                                objDynamicData.contact.pointOfContact.email = Convert.ToString(childRow["Email"]);
                            else
                                objDynamicData.contact.pointOfContact.email = "";

                            if (childRow["Url"] != DBNull.Value)
                                objDynamicData.contact.organisation.website = Convert.ToString(childRow["Url"]);
                            else
                                objDynamicData.contact.organisation.website = "";

                            if (childRow["Telephone"] != DBNull.Value)
                                objDynamicData.contact.organisation.telephone = Convert.ToString(childRow["Telephone"]);
                            else
                                objDynamicData.contact.organisation.telephone = "";
                            break;
                        }
                    }
                    else
                    {
                        #region ForErrorhandling AllowNullProperty
                        objDynamicData.contact = new ExpandoObject();
                        objDynamicData.contact.pointOfContact = new ExpandoObject();
                        objDynamicData.contact.organisation = new ExpandoObject();
                        objDynamicData.contact.pointOfContact.name = "";
                        objDynamicData.contact.pointOfContact.email = "";
                        objDynamicData.contact.organisation.website = "";
                        objDynamicData.contact.organisation.telephone = "";
                        #endregion
                    }
                    #endregion Organization

                    #region SubEvent
                    if (dtSubEvents?.Rows?.Count > 0)
                    {
                        objDynamicData.occurrences = new List<ExpandoObject>();
                        foreach (DataRow childRow in dtSubEvents.Rows)
                        {
                            long subEventId = Convert.ToInt64(childRow["Id"]);
                            var subevent = GetSessionFromDataRow(childRow, subEventId);

                            dynamic occurance = new ExpandoObject();
                            if (!string.IsNullOrEmpty(subevent.StartDate?.ToString()))
                                occurance.start = subevent.StartDate?.UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ssK");
                            //occurance.start = DateTime.SpecifyKind(Convert.ToDateTime(subevent.StartDate.ToString()), DateTimeKind.Utc).ToString("yyyy-MM-dd'T'HH:mm:ssK");
                            if (!string.IsNullOrEmpty(subevent.EndDate?.ToString()))
                                occurance.end = subevent.EndDate?.UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ssK");
                            //if (!string.IsNullOrEmpty(subevent.Url))
                            //    occurance.checkoutUrl = subevent.Url?.ToString();
                            occurance.href = Settings.GetAppSetting("sessionDetailAPI") + objDynamicData.id + "/subevent/" + occurance.start;

                            objDynamicData.occurrences.Add(occurance);
                        }
                    }
                    else
                    {
                        objDynamicData.occurrences = new List<ExpandoObject>();
                    }
                    #endregion SubEvent
                }
                #endregion

            }
            catch (Exception ex)
            {
                LogHelper.InsertErrorLogs("[DataLaundryApi] FeedHelper", "GetOpportunityFromDatarowDynamically", ex.Message, ex.InnerException?.Message, ex.StackTrace);
            }
            return objDynamicData;
        }
        private static ExpandoObject GetOpportunityFromDatarowDynamically_v1(DataRow row, long eventId, bool IsSingleEventOccurrence)
{
            dynamic objDynamicData = new ExpandoObject();
            //var objItems = new Items();
            try
            {
                //objItems.offers = new List<string>();
                //objItems.cost = "Contact organiser";
                //objItems.tags = new string[] { };
                //objItems.weekdays = new int[] { };
                //objItems.categories = new Categories();
                //objItems.categories.DisabilitySupport = new string[] { };
                //objItems.occurrences = new List<Occurrences>();
                //objItems.contact = new Contact();
                //objItems.contact.PointOfContact = new ContactPointOfContact();
                //objItems.contact.Organisation = new ContactOrganisation();
                //objItems.restrictions = new Restrictions();

                objDynamicData.offers = new List<ExpandoObject>();
                objDynamicData.cost = "Contact organiser";
                objDynamicData.tags = Array.Empty<string>();
                objDynamicData.weekdays = Array.Empty<int>();
                objDynamicData.categories = new ExpandoObject();
                objDynamicData.categories.disabilitySupport = Array.Empty<string>();
                objDynamicData.occurrences = new List<ExpandoObject>();
                objDynamicData.contact = new ExpandoObject();
                objDynamicData.contact.pointOfContact = new ExpandoObject();
                objDynamicData.contact.organisation = new ExpandoObject();

                objDynamicData.description = Convert.ToString(row["Description"]);

                // objItems.description = Convert.ToString(row["Description"]);

                if (row["Category"] != DBNull.Value)
                {
                    objDynamicData.tags = row["Category"].ToString().Split(',').Where(x => !string.IsNullOrEmpty(x)).ToArray();

                    //  objItems.tags = row["Category"].ToString().Split(',').Where(x => !string.IsNullOrEmpty(x)).ToArray();
                }


                //if (row["AgeRange"] != DBNull.Value)
                //{
                //    if (!((IDictionary<string, Object>)objDynamicData).ContainsKey("restrictions"))
                //        objDynamicData.restrictions = new ExpandoObject();
                //    string[] ages = Convert.ToString(row["AgeRange"]).Split('-');
                //    objDynamicData.restrictions.minAge = Convert.ToInt32(ages[0].Trim());
                //    if (ages.Length > 1)
                //        objDynamicData.restrictions.maxAge = Convert.ToInt32(ages[1].Trim());
                //}

                if (row["MinAge"] != DBNull.Value)
                {
                    if (!((IDictionary<string, Object>)objDynamicData).ContainsKey("restrictions"))
                    {
                        objDynamicData.restrictions = new ExpandoObject();
                        objDynamicData.restrictions.minAge = Convert.ToInt32(row["MinAge"]);

                        //  objItems.restrictions.MinAge= Convert.ToInt32(row["MinAge"]);
                    }
                }
                else
                {
                    objDynamicData.restrictions = new ExpandoObject();
                    objDynamicData.restrictions.minAge = 0;
                }

                if (row["MaxAge"] != DBNull.Value)
                {
                    if (!((IDictionary<string, Object>)objDynamicData).ContainsKey("restrictions"))
                    {
                        objDynamicData.restrictions = new ExpandoObject();
                        objDynamicData.restrictions.maxAge = Convert.ToInt32(row["MaxAge"]);

                        //objItems.
                    }
                }
                else
                {
                    objDynamicData.restrictions = new ExpandoObject();
                    objDynamicData.restrictions.maxAge = 0;
                }

                if (row["GenderRestriction"] != DBNull.Value)
                {
                    if (!((IDictionary<string, Object>)objDynamicData).ContainsKey("restrictions"))
                        objDynamicData.restrictions = new ExpandoObject();
                    objDynamicData.restrictions.gender = Convert.ToString(row["GenderRestriction"]);
                }
                else
                {
                    objDynamicData.restrictions = new ExpandoObject();
                    objDynamicData.restrictions.gender = "";
                }

                if (row["WeekDays"] != DBNull.Value)
                    objDynamicData.weekdays = row["WeekDays"].ToString().Split(',').Select(n => Convert.ToInt32(n)).ToArray();

                if (row["Duration"] != DBNull.Value)
                {
                    TimeSpan ts = XmlConvert.ToTimeSpan(row["Duration"].ToString());
                    objDynamicData.durationMins = Convert.ToInt32(ts.TotalMinutes);
                }
                else
                {
                    objDynamicData.durationMins = 0;
                }

                objDynamicData.title = Convert.ToString(row["Name"]);

                if (row["AccessibilitySupport"] != DBNull.Value)
                    objDynamicData.categories.disabilitySupport = Convert.ToString(row["AccessibilitySupport"]).Split(',');
                else
                    objDynamicData.categories.disabilitySupport = new string[] { };

                if (row["Activity"] != DBNull.Value)
                    objDynamicData.categories.activities = Convert.ToString(row["Activity"]).Split(',');
                else
                    objDynamicData.categories.activities = new string[] { };

                objDynamicData.id = Convert.ToString(row["FeedName"]) + "-" + Convert.ToString(row["FeedId"]);

                if (row["Image"] != DBNull.Value)
                    objDynamicData.image = Convert.ToString(row["Image"]);
                else
                    objDynamicData.image = "";

                objDynamicData.href = Settings.GetAppSetting("sessionDetailAPI") + objDynamicData.id;
                //objDynamicData.checkoutUrl = Settings.GetAppSetting("checkoutUrl") + objDynamicData.id;

                #region get event data
                var dsOtherEventData = new DataSet();
                if (!IsSingleEventOccurrence)
                    dsOtherEventData = GetOtherSessionData(eventId);
                else
                    dsOtherEventData = GetOtherSessionData(eventId, true);

                if (dsOtherEventData != null && dsOtherEventData.Tables.Count > 0)
                {
                    //var dtPhysicalAcitivity = dsOtherEventData.Tables[4];
                    var dtLocation = dsOtherEventData.Tables[2];
                    var dtOrganization = dsOtherEventData.Tables[6];
                    var dtSubEvents = dsOtherEventData.Tables[0];
                    var dtEventSchedule = dsOtherEventData.Tables[5];
                    var dtOccurrence = dsOtherEventData.Tables[9];
                    var dtOffer = dsOtherEventData.Tables[11];

                    #region Activity
                    //if (dtPhysicalAcitivity?.Rows?.Count > 0)
                    //{
                    //    List<string> items = new List<string>();
                    //    foreach (DataRow childRow in dtPhysicalAcitivity.Rows)
                    //        items.Add(Convert.ToString(childRow["PrefLabel"]));

                    //    if (!((IDictionary<string, Object>)objDynamicData).ContainsKey("categories"))
                    //        objDynamicData.categories = new ExpandoObject();

                    //    objDynamicData.categories.activities = items.ToArray();
                    //}
                    #endregion Activity

                    #region Location
                    if (dtLocation?.Rows?.Count > 0)
                    {
                        objDynamicData.location = new ExpandoObject();
                        objDynamicData.location.coordinates = new ExpandoObject();
                        foreach (DataRow childRow in dtLocation.Rows)
                        {
                            if (childRow["Lat"] != DBNull.Value)
                                objDynamicData.location.coordinates.lat = Convert.ToDecimal(childRow["Lat"]);
                            else
                                objDynamicData.location.coordinates.lat = 0;

                            if (childRow["Long"] != DBNull.Value)
                                objDynamicData.location.coordinates.lng = Convert.ToDecimal(childRow["Long"]);
                            else
                                objDynamicData.location.coordinates.lng = 0;
                         
                            if (childRow["Address"] != DBNull.Value)
                                objDynamicData.location.address = Convert.ToString(childRow["Address"]);
                            else
                                objDynamicData.location.address = "";

                            if (row["Distance"] != DBNull.Value)
                                objDynamicData.location.distance = Convert.ToDecimal(row["Distance"]);
                            else
                                objDynamicData.location.distance = 0;
                            break;
                        }
                    }
                    else
                    {
                        #region ForErrorHandling AllowProperties
                        objDynamicData.location = new ExpandoObject();
                        objDynamicData.location.coordinates = new ExpandoObject();
                        objDynamicData.location.coordinates.lat = 0;
                        objDynamicData.location.coordinates.lng = 0;
                        objDynamicData.location.address = "";
                        objDynamicData.location.distance = 0;
                        #endregion
                    }
                    #endregion Location

                    #region Organization
                    if (dtOrganization?.Rows?.Count > 0)
                    {
                        //objDynamicData.contact = new ExpandoObject();
                        //objDynamicData.contact.pointOfContact = new ExpandoObject();
                        //objDynamicData.contact.organisation = new ExpandoObject();
                        foreach (DataRow childRow in dtOrganization.Rows)
                        {
                            if (childRow["Name"] != DBNull.Value)
                                objDynamicData.contact.pointOfContact.name = Convert.ToString(childRow["Name"]);
                            else
                                objDynamicData.contact.pointOfContact.name = "";

                            if (childRow["Email"] != DBNull.Value)
                                objDynamicData.contact.pointOfContact.email = Convert.ToString(childRow["Email"]);
                            else
                                objDynamicData.contact.pointOfContact.email = "";

                            if (childRow["Url"] != DBNull.Value)
                                objDynamicData.contact.organisation.website = Convert.ToString(childRow["Url"]);
                            else
                                objDynamicData.contact.organisation.website = "";

                            if (childRow["Telephone"] != DBNull.Value)
                                objDynamicData.contact.organisation.telephone = Convert.ToString(childRow["Telephone"]);
                            else
                                objDynamicData.contact.organisation.telephone = "";
                            break;
                        }
                    }
                    else
                    {
                        #region ForErrorHandling AllowProperties
                        objDynamicData.contact.pointOfContact.name = "";
                        objDynamicData.contact.pointOfContact.email = "";
                        objDynamicData.contact.organisation.website = "";
                        objDynamicData.contact.organisation.telephone = "";
                        #endregion
                    }
                    #endregion Organization

                    #region Occurences   
                    if (dtOccurrence?.Rows?.Count > 0)
                    {
                        foreach (DataRow childRow in dtOccurrence.Rows)
                        {
                            long subEventId = 0;
                            dynamic occurance = new ExpandoObject();

                            if (childRow["SubEventId"] != DBNull.Value)
                                subEventId = Convert.ToInt64(childRow["SubEventId"]);

                            if (childRow["StartDate"] != DBNull.Value)
                            {
                                DateTimeOffset? StartDate = DateTime.SpecifyKind(Convert.ToDateTime(childRow["StartDate"]), DateTimeKind.Utc);
                                if (!string.IsNullOrEmpty(StartDate?.ToString()))
                                    occurance.start = StartDate?.UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ssK");
                            }
                            if (childRow["EndDate"] != DBNull.Value)
                            {
                                DateTimeOffset? EndDate = DateTime.SpecifyKind(Convert.ToDateTime(childRow["EndDate"]), DateTimeKind.Utc);
                                if (!string.IsNullOrEmpty(EndDate?.ToString()))
                                    occurance.end = EndDate?.UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ssK");
                            }

                            //occurance.href =  Settings.GetAppSetting("sessionDetailAPI") + objDynamicData.id + (subEventId > 0 ? ("/subevent/" + occurance.start) : "");
                            occurance.href = Settings.GetAppSetting("sessionDetailAPI") + objDynamicData.id + "/subevent/" + occurance.start;
                            //occurance.checkoutUrl =  Settings.GetAppSetting("checkoutUrl") + objDynamicData.id + "/subevent/" + occurance.start;

                            objDynamicData.occurrences.Add(occurance);
                        }
                    }
                    #endregion

                    #region dtOffer
                    if (dtOffer != null && dtOffer.Rows.Count > 0)
                    {
                        foreach (DataRow childRow in dtOffer.Rows)
                        {
                            dynamic offer = new ExpandoObject();
                            offer.type = "Offer";
                            offer.name = Convert.ToString(childRow["Name"]);
                            offer.price = Convert.ToString(childRow["Price"]);
                            offer.priceCurrency = Convert.ToString(childRow["PriceCurrency"]);
                            //offer.identifier = Convert.ToString(childRow["Identifier"]);
                            objDynamicData.offers.Add(offer);
                        }

                        //objDynamicData.cost = objDynamicData.offers[0].price;
                        IEnumerable<dynamic> MaxPrice = (dynamic)objDynamicData.offers;
                        var oPrice = MaxPrice.Select(x => Convert.ToDecimal(x.price)).ToList();
                        objDynamicData.cost = oPrice.Max();

                    }
                    #endregion
                }
                #endregion
         
            }
            catch (Exception ex)
            {
                LogHelper.InsertErrorLogs("[DataLaundryApi] FeedHelper", "GetOpportunityFromDatarowDynamically_v1", ex.Message, ex.InnerException?.Message, ex.StackTrace);
            }


            return objDynamicData;
        }
        #region New Method Avoid Internal call added 28-01-2019
        public static dynamic GetEventsDynamically_V1(double? latitude, double? longitude, double? radius,
                                            string source = null, string kind = null, string tag = null, string excludeTag = null,
                                            string activity = null, string disabilitySupport = null, string weekdays = null,
                                            double? minCost = null, double? maxCost = null, string gender = null,
                                            string sortMode = null, long? minTime = null, long? maxTime = null,
                                            long? minAge = null, long? maxAge = null
                                            , long? page = 1,
                                            long? limit = 100, string from = null, string to = null)
        {
            dynamic eventContainer = new ExpandoObject();
            List<dynamic> eventContainer2 = new List<dynamic>();

            eventContainer.count = 0;
            eventContainer.limit = limit;
            eventContainer.page = page;
            eventContainer.items = new List<dynamic>();
            ArrayList objDynamicData1 = new ArrayList();

            try
            {
                var lstSqlParameter = new List<SqlParameter>();
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@lat", SqlDbType = SqlDbType.Decimal, Value = (object)latitude ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@long", SqlDbType = SqlDbType.Decimal, Value = (object)longitude ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@radius", SqlDbType = SqlDbType.Decimal, Value = (object)radius ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@source", SqlDbType = SqlDbType.NVarChar, Value = (object)source ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@kind", SqlDbType = SqlDbType.NVarChar, Value = (object)kind ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@tag", SqlDbType = SqlDbType.NVarChar, Value = (object)tag ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@excludeTag", SqlDbType = SqlDbType.NVarChar, Value = (object)excludeTag ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@activity", SqlDbType = SqlDbType.NVarChar, Value = (object)activity ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@disabilitySupport", SqlDbType = SqlDbType.NVarChar, Value = (object)disabilitySupport ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@weekdays", SqlDbType = SqlDbType.NVarChar, Value = (object)weekdays ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@minCost", SqlDbType = SqlDbType.Decimal, Value = (object)minCost ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@maxCost", SqlDbType = SqlDbType.Decimal, Value = (object)maxCost ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@gender", SqlDbType = SqlDbType.NVarChar, Value = (object)gender ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@sortMode", SqlDbType = SqlDbType.NVarChar, Value = (object)sortMode ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@minTime", SqlDbType = SqlDbType.BigInt, Value = (object)minTime ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@maxTime", SqlDbType = SqlDbType.BigInt, Value = (object)maxTime ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@minAge", SqlDbType = SqlDbType.BigInt, Value = (object)minAge ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@maxAge", SqlDbType = SqlDbType.BigInt, Value = (object)maxAge ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@page", SqlDbType = SqlDbType.BigInt, Value = (object)page ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@limit", SqlDbType = SqlDbType.BigInt, Value = (object)limit ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@from", SqlDbType = SqlDbType.NVarChar, Value = (object)from ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@to", SqlDbType = SqlDbType.NVarChar, Value = (object)to ?? DBNull.Value });

                var ds = DBProvider.GetDataSet("GetFilteredEvents_v1", CommandType.StoredProcedure, ref lstSqlParameter);

                if (ds != null && ds.Tables.Count > 0)
                {
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        DataTable dt_Events = ds.Tables[0];
                        int rowCount = dt_Events.Rows.Count;

                        eventContainer.count = Convert.ToInt32(ds.Tables[1].Rows.Count > 0 ? ds.Tables[1].Rows[0]["TotalCount"].ToString() : "0");

                        var lstEventID = (from ID in dt_Events.AsEnumerable() select ID["id"]).Distinct().ToList();

                        string EventIDs = string.Join(",", lstEventID);

                        lstSqlParameter = new List<SqlParameter>();
                        lstSqlParameter.Add(new SqlParameter() { ParameterName = "@EventIDs", Value = EventIDs, SqlDbType = SqlDbType.NVarChar });

                        var dsMultiEventData = DBProvider.GetDataSet("GetMultiEventData", CommandType.StoredProcedure, ref lstSqlParameter);

                        #region All ChildData
                        var dtLocation = dsMultiEventData.Tables[0];
                        var dtOrganization = dsMultiEventData.Tables[1];
                        var dtSubEvents = dsMultiEventData.Tables[2];
                        var dtEventSchedule = dsMultiEventData.Tables[3];
                        var dtOccurrence = dsMultiEventData.Tables[4];
                        var dtOffer = dsMultiEventData.Tables[5];
                        #endregion
                        IEnumerable<dynamic> strList = new List<dynamic>();
                            //List<string> names = new List<string>();
                        for (int i = 0; i < rowCount; i++)
                        {

                            var row = dt_Events.Rows[i];
                            var eventid = Convert.ToInt64(row["Id"]);
                            var opportunity = new ExpandoObject();
                            dynamic objDynamicData = new ExpandoObject();
                            objDynamicData.offers = new List<ExpandoObject>();
                            objDynamicData.cost = "Contact organiser";
                            objDynamicData.tags = Array.Empty<string>();
                            objDynamicData.weekdays = Array.Empty<int>();
                            objDynamicData.categories = new ExpandoObject();
                            objDynamicData.categories.disabilitySupport = Array.Empty<string>();
                            objDynamicData.occurrences = new List<ExpandoObject>();
                            objDynamicData.contact = new ExpandoObject();
                            objDynamicData.contact.pointOfContact = new ExpandoObject();
                            objDynamicData.contact.organisation = new ExpandoObject();

                            objDynamicData.description = Convert.ToString(row["Description"]);


                            if (row["Category"] != DBNull.Value)
                            {
                                objDynamicData.tags = row["Category"].ToString().Split(',').Where(x => !string.IsNullOrEmpty(x)).ToArray();
                            }

                            if (row["MinAge"] != DBNull.Value)
                            {
                                if (!((IDictionary<string, Object>)objDynamicData).ContainsKey("restrictions"))
                                {
                                    objDynamicData.restrictions = new ExpandoObject();
                                    objDynamicData.restrictions.minAge = Convert.ToInt32(row["MinAge"]);
                                }
                            }
                            else
                            {
                                objDynamicData.restrictions = new ExpandoObject();
                                objDynamicData.restrictions.minAge = 0;
                            }

                            if (row["MaxAge"] != DBNull.Value)
                            {
                                if (!((IDictionary<string, Object>)objDynamicData).ContainsKey("restrictions"))
                                {
                                    objDynamicData.restrictions = new ExpandoObject();
                                    objDynamicData.restrictions.maxAge = Convert.ToInt32(row["MaxAge"]);
                                }
                            }
                            else
                            {
                                objDynamicData.restrictions = new ExpandoObject();
                                objDynamicData.restrictions.maxAge = 0;
                            }

                            if (row["GenderRestriction"] != DBNull.Value)
                            {
                                if (!((IDictionary<string, Object>)objDynamicData).ContainsKey("restrictions"))
                                    objDynamicData.restrictions = new ExpandoObject();
                                objDynamicData.restrictions.gender = Convert.ToString(row["GenderRestriction"]);
                            }
                            else
                            {
                                objDynamicData.restrictions = new ExpandoObject();
                                objDynamicData.restrictions.gender = "";
                            }

                            if (row["WeekDays"] != DBNull.Value)
                                objDynamicData.weekdays = row["WeekDays"].ToString().Split(',').Select(n => Convert.ToInt32(n)).ToArray();

                            if (row["Duration"] != DBNull.Value)
                            {
                                TimeSpan ts = XmlConvert.ToTimeSpan(row["Duration"].ToString());
                                objDynamicData.durationMins = Convert.ToInt32(ts.TotalMinutes);
                            }
                            else
                            {
                                objDynamicData.durationMins = 0;
                            }

                            objDynamicData.title = Convert.ToString(row["Name"]);

                            if (row["AccessibilitySupport"] != DBNull.Value)
                                objDynamicData.categories.disabilitySupport = Convert.ToString(row["AccessibilitySupport"]).Split(',');
                            else
                                objDynamicData.categories.disabilitySupport = new string[] { };

                            if (row["Activity"] != DBNull.Value)
                                objDynamicData.categories.activities = Convert.ToString(row["Activity"]).Split(',');
                            else
                                objDynamicData.categories.activities = new string[] { };

                            objDynamicData.id = Convert.ToString(row["FeedName"]) + "-" + Convert.ToString(row["FeedId"]);

                            if (row["Image"] != DBNull.Value)
                                objDynamicData.image = Convert.ToString(row["Image"]);
                            else
                                objDynamicData.image = "";

                            objDynamicData.href = Settings.GetAppSetting("sessionDetailAPI") + objDynamicData.id;

                            #region Location
                            var oLocationRow = dtLocation.Select("EventId =" + eventid);
                          
                            if (oLocationRow != null && oLocationRow.Count() > 0)
                            {
                                objDynamicData.location = new ExpandoObject();
                                objDynamicData.location.coordinates = new ExpandoObject();
                                foreach (DataRow childRow in oLocationRow)
                                {
                                    if (childRow["Lat"] != DBNull.Value)
                                        objDynamicData.location.coordinates.lat = Convert.ToDecimal(childRow["Lat"]);
                                    else
                                        objDynamicData.location.coordinates.lat = 0;

                                    if (childRow["Long"] != DBNull.Value)
                                        objDynamicData.location.coordinates.lng = Convert.ToDecimal(childRow["Long"]);
                                    else
                                        objDynamicData.location.coordinates.lng = 0;


                                    if (childRow["Address"] != DBNull.Value){
                                        objDynamicData.location.address = Convert.ToString(childRow["Address"]);

                                        objDynamicData1.Add(objDynamicData.location.address);
                                        //eventContainer= eventContainer.items.GroupBy(objDynamicData1[i]).First();

                                }
                                else
                                        objDynamicData.location.address = "";

                                    if (row["Distance"] != DBNull.Value)
                                        objDynamicData.location.distance = Convert.ToDecimal(row["Distance"]);
                                    else
                                        objDynamicData.location.distance = 0;
                             
                                    break;

                                }

                            }
                            else
                            {
                                #region ForErrorHandling AllowProperties
                                objDynamicData.location = new ExpandoObject();
                                objDynamicData.location.coordinates = new ExpandoObject();
                                objDynamicData.location.coordinates.lat = 0;
                                objDynamicData.location.coordinates.lng = 0;
                                objDynamicData.location.address = "";
                                objDynamicData.location.distance = 0;
                                #endregion
                            }
                            #endregion Location

                            #region Organization
                            var oOrganizationRow = dtOrganization.Select("EventId =" + eventid);
                            if (oOrganizationRow != null && oOrganizationRow.Count() > 0)
                            {
                                foreach (DataRow childRow in oOrganizationRow)
                                {
                                    if (childRow["Name"] != DBNull.Value)
                                        objDynamicData.contact.pointOfContact.name = Convert.ToString(childRow["Name"]);
                                    else
                                        objDynamicData.contact.pointOfContact.name = "";

                                    if (childRow["Email"] != DBNull.Value)
                                        objDynamicData.contact.pointOfContact.email = Convert.ToString(childRow["Email"]);
                                    else
                                        objDynamicData.contact.pointOfContact.email = "";

                                    if (childRow["Url"] != DBNull.Value)
                                        objDynamicData.contact.organisation.website = Convert.ToString(childRow["Url"]);
                                    else
                                        objDynamicData.contact.organisation.name = "";
                                      if (childRow["Name"] != DBNull.Value)
                                        objDynamicData.contact.organisation.name = Convert.ToString(childRow["Name"]);
                                    else
                                        objDynamicData.contact.organisation.website = "";
                                    if (childRow["Telephone"] != DBNull.Value)
                                        objDynamicData.contact.organisation.telephone = Convert.ToString(childRow["Telephone"]);
                                    else
                                        objDynamicData.contact.organisation.telephone = "";
                                    break;
                                }
                            }
                            else
                            {
                                #region ForErrorHandling AllowProperties
                                objDynamicData.contact.pointOfContact.name = "";
                                objDynamicData.contact.pointOfContact.email = "";
                                objDynamicData.contact.organisation.website = "";
                                  objDynamicData.contact.organisation.name = "";
                                objDynamicData.contact.organisation.telephone = "";
                                #endregion
                            }
                            #endregion Organization

                            #region Occurences 
                            var oOccurrenceRow = dtOccurrence.Select("EventId =" + eventid);
                            if (oOccurrenceRow != null && oOccurrenceRow.Count() > 0)
                            {
                                foreach (DataRow childRow in oOccurrenceRow)
                                {
                                    long subEventId = 0;
                                    dynamic occurance = new ExpandoObject();

                                    if (childRow["SubEventId"] != DBNull.Value)
                                        subEventId = Convert.ToInt64(childRow["SubEventId"]);

                                    if (childRow["StartDate"] != DBNull.Value)
                                    {
                                        DateTimeOffset? StartDate = DateTime.SpecifyKind(Convert.ToDateTime(childRow["StartDate"]), DateTimeKind.Utc);
                                        if (!string.IsNullOrEmpty(StartDate?.ToString()))
                                            occurance.start = StartDate?.UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ssK");
                                    }
                                    if (childRow["EndDate"] != DBNull.Value)
                                    {
                                        DateTimeOffset? EndDate = DateTime.SpecifyKind(Convert.ToDateTime(childRow["EndDate"]), DateTimeKind.Utc);
                                        if (!string.IsNullOrEmpty(EndDate?.ToString()))
                                            occurance.end = EndDate?.UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ssK");
                                    }

                                    //occurance.href =  Settings.GetAppSetting("sessionDetailAPI") + objDynamicData.id + (subEventId > 0 ? ("/subevent/" + occurance.start) : "");
                                    occurance.href = Settings.GetAppSetting("sessionDetailAPI") + objDynamicData.id + "/subevent/" + occurance.start;
                                    //occurance.checkoutUrl =  Settings.GetAppSetting("checkoutUrl") + objDynamicData.id + "/subevent/" + occurance.start;

                                    objDynamicData.occurrences.Add(occurance);
                                    break;
                                }
                            }
                            #endregion

                            #region offer
                            var oOffer = dtOffer.Select("EventId =" + eventid);
                            if (oOffer != null && oOffer.Count() > 0)
                            {
                                foreach (DataRow childRow in oOffer)
                                {
                                    dynamic offers = new ExpandoObject();
                                    //offers.Id = Convert.ToInt64(childRow["Id"]);
                                    offers.type = "Offer";
                                    //offers.identifier = Convert.ToString(childRow["Identifier"]);
                                    offers.name = Convert.ToString(childRow["Name"]);
                                    offers.price = Convert.ToString(childRow["Price"]);
                                    offers.priceCurrency = Convert.ToString(childRow["PriceCurrency"]);

                                    objDynamicData.offers.Add(offers);
                                }
                                //objDynamicData.cost = objDynamicData.offers[0].price;
                                IEnumerable<dynamic> MaxPrice = (dynamic)objDynamicData.offers;
                                var oPrice = MaxPrice.Select(x => Convert.ToDecimal(x.price)).ToList();
                                objDynamicData.cost = oPrice.Max();


                            }
                            //objDynamicData.offers
                            #endregion

                            opportunity = objDynamicData;

                            eventContainer.items.Add(opportunity);

                            #region build next page url in the end based on 
                            if ((i + 1) == rowCount && eventContainer.count > limit && rowCount >= limit && rowCount <= eventContainer.count)
                            {
                                if (!((IDictionary<string, Object>)eventContainer).ContainsKey("next"))
                                    eventContainer.next = Settings.GetAppSetting("opportunityAPI");

                                eventContainer.next += eventContainer.next.Contains("?") ? "&" : "?";

                                #region required params
                                eventContainer.next += "lat=" + latitude;
                                eventContainer.next += "&";
                                eventContainer.next += "lng=" + longitude;
                                eventContainer.next += "&";
                                eventContainer.next += "radius=" + radius;
                                eventContainer.next += "&";
                                eventContainer.next += "page=" + (page + 1);
                                eventContainer.next += "&";
                                eventContainer.next += "limit=" + limit;
                                #endregion

                                #region Optional Params
                                eventContainer.next += GetUrlFromGivenString(eventContainer.next, source, "source");
                                eventContainer.next += GetUrlFromGivenString(eventContainer.next, activity, "activity");
                                eventContainer.next += GetUrlFromGivenString(eventContainer.next, disabilitySupport, "disabilitySupport");
                                eventContainer.next += GetUrlFromGivenString(eventContainer.next, weekdays, "weekdays");
                                eventContainer.next += GetUrlFromGivenString(eventContainer.next, kind, "kind");
                                eventContainer.next += GetUrlFromGivenString(eventContainer.next, tag, "tag");
                                eventContainer.next += GetUrlFromGivenString(eventContainer.next, excludeTag, "excludeTag");
                                eventContainer.next += GetUrlFromGivenString(eventContainer.next, sortMode, "sortMode");
                                eventContainer.next += GetUrlFromGivenString(eventContainer.next, gender, "gender");
                                eventContainer.next += GetUrlFromGivenString(eventContainer.next, from, "from");
                                eventContainer.next += GetUrlFromGivenString(eventContainer.next, to, "to");
                                eventContainer.next += GetUrlFromGivenString(eventContainer.next, minTime?.ToString(), "minTime");
                                eventContainer.next += GetUrlFromGivenString(eventContainer.next, maxTime?.ToString(), "maxTime");
                                eventContainer.next += GetUrlFromGivenString(eventContainer.next, minAge?.ToString(), "minAge");
                                eventContainer.next += GetUrlFromGivenString(eventContainer.next, maxAge?.ToString(), "maxAge");
                                eventContainer.next += GetUrlFromGivenString(eventContainer.next, minCost?.ToString(), "minCost");
                                eventContainer.next += GetUrlFromGivenString(eventContainer.next, maxCost?.ToString(), "maxCost");
                                #endregion
                            }
                            #endregion

                            #region build prev page url in the end based on 
                            if ((i + 1) == rowCount && eventContainer.count > limit && page > 1)
                            {
                                if (!((IDictionary<string, Object>)eventContainer).ContainsKey("prev"))
                                    eventContainer.prev = Settings.GetAppSetting("opportunityAPI");

                                eventContainer.prev += eventContainer.prev.Contains("?") ? "&" : "?";

                                #region required params
                                eventContainer.prev += "lat=" + latitude;
                                eventContainer.prev += "&";
                                eventContainer.prev += "lng=" + longitude;
                                eventContainer.prev += "&";
                                eventContainer.prev += "radius=" + radius;
                                eventContainer.prev += "&";
                                eventContainer.prev += "page=" + (page - 1);
                                eventContainer.prev += "&";
                                eventContainer.prev += "limit=" + limit;
                                #endregion

                                #region Optional Params
                                eventContainer.prev += GetUrlFromGivenString(eventContainer.prev, source, "source");
                                eventContainer.prev += GetUrlFromGivenString(eventContainer.prev, activity, "activity");
                                eventContainer.prev += GetUrlFromGivenString(eventContainer.prev, disabilitySupport, "disabilitySupport");
                                eventContainer.prev += GetUrlFromGivenString(eventContainer.prev, weekdays, "weekdays");
                                eventContainer.prev += GetUrlFromGivenString(eventContainer.prev, kind, "kind");
                                eventContainer.prev += GetUrlFromGivenString(eventContainer.prev, tag, "tag");
                                eventContainer.prev += GetUrlFromGivenString(eventContainer.prev, excludeTag, "excludeTag");
                                eventContainer.prev += GetUrlFromGivenString(eventContainer.prev, sortMode, "sortMode");
                                eventContainer.prev += GetUrlFromGivenString(eventContainer.prev, gender, "gender");
                                eventContainer.prev += GetUrlFromGivenString(eventContainer.prev, from, "from");
                                eventContainer.prev += GetUrlFromGivenString(eventContainer.prev, to, "to");
                                eventContainer.prev += GetUrlFromGivenString(eventContainer.prev, minTime?.ToString(), "minTime");
                                eventContainer.prev += GetUrlFromGivenString(eventContainer.prev, maxTime?.ToString(), "maxTime");
                                eventContainer.prev += GetUrlFromGivenString(eventContainer.prev, minAge?.ToString(), "minAge");
                                eventContainer.prev += GetUrlFromGivenString(eventContainer.prev, maxAge?.ToString(), "maxAge");
                                eventContainer.prev += GetUrlFromGivenString(eventContainer.prev, minCost?.ToString(), "minCost");
                                eventContainer.prev += GetUrlFromGivenString(eventContainer.prev, maxCost?.ToString(), "maxCost");
                                #endregion
                            }
                       

                            #endregion
                            
                            
                    //eventContainer = eventContainer.items.DistinctBy(eventContainer.items.location.address).ToList();

                        }
                        var distinctItems = objDynamicData1.Cast<object>().Distinct();
                      //var a = eventContainer.items;
                  //strList  = Uniquerecords.GroupBy(objDynamicData1);
         
         List<dynamic> strList2 = eventContainer.items;
        eventContainer.items = null;
       

    
    

         eventContainer2 = strList2.GroupBy(x => new{x.location.address , x.contact.organisation.name}).Select(g => g.First()).Take(50).ToList(); 
            var  count =    strList2.GroupBy(x=>x.location.address).Select(g => g.Count()).ToList();


         eventContainer.items = eventContainer2 ;
        //eventContainer.count = count;
         
// List<SessionDetails> distinct = sessions.GroupBy(x => x.Data.Location).Select(g => g.First()).ToList();

                      
                    }
                } 
            }
            catch (Exception ex)
            {
                var oParameter = string.Concat("Error occured following parameter ",
                                        "latitude = ", latitude,
                                        " ,longitude = ", longitude,
                                        " ,radius = ", radius,
                                        " ,source = ", source,
                                        " ,kind = ", kind,
                                        " ,tag = ", tag,
                                        " ,excludeTag = ", excludeTag,
                                        " ,activity = ", activity,
                                        " ,disabilitySupport = ", disabilitySupport,
                                        " ,weekdays = ", weekdays,
                                        " ,minCost = ", minCost,
                                        " ,maxCost = ", maxCost,
                                        " ,gender = ", gender,
                                        " ,sortMode = ", sortMode,
                                        " ,minTime = ", minTime,
                                        " ,maxTime = ", maxTime,
                                        " ,minAge = ", minAge,
                                        " ,maxAge = ", maxAge,
                                        " ,page = ", page,
                                        " ,limit = ", limit,
                                        " ,from = ", from,
                                        " ,to = ", to);
                LogHelper.InsertErrorLogs("[DataLaundryApi] FeedHelper", "GetEventsDynamically_V1", ex.Message, string.Concat(ex.InnerException?.Message, oParameter), ex.StackTrace);
            }
             
           

            return eventContainer;
        }

        #endregion
           public static dynamic GetEventsSimilarDynamically(double? latitude, double? longitude, double? radius,
                                            string source = null, string kind = null, string tag = null, string excludeTag = null,
                                            string activity = null, string disabilitySupport = null, string weekdays = null,
                                            double? minCost = null, double? maxCost = null, string gender = null,
                                            string sortMode = null, long? minTime = null, long? maxTime = null,
                                            long? minAge = null, long? maxAge = null
                                            , long? page = 1,
                                            long? limit = 100, string from = null, string to = null)
        {
            dynamic eventContainer = new ExpandoObject();
            List<dynamic> eventContainer2 = new List<dynamic>();

            eventContainer.count = 0;
            eventContainer.limit = limit;
            eventContainer.page = page;
            eventContainer.items = new List<dynamic>();
            ArrayList objDynamicData1 = new ArrayList();

            try
            {
                var lstSqlParameter = new List<SqlParameter>();
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@lat", SqlDbType = SqlDbType.Decimal, Value = (object)latitude ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@long", SqlDbType = SqlDbType.Decimal, Value = (object)longitude ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@radius", SqlDbType = SqlDbType.Decimal, Value = (object)radius ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@source", SqlDbType = SqlDbType.NVarChar, Value = (object)source ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@kind", SqlDbType = SqlDbType.NVarChar, Value = (object)kind ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@tag", SqlDbType = SqlDbType.NVarChar, Value = (object)tag ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@excludeTag", SqlDbType = SqlDbType.NVarChar, Value = (object)excludeTag ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@activity", SqlDbType = SqlDbType.NVarChar, Value = (object)activity ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@disabilitySupport", SqlDbType = SqlDbType.NVarChar, Value = (object)disabilitySupport ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@weekdays", SqlDbType = SqlDbType.NVarChar, Value = (object)weekdays ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@minCost", SqlDbType = SqlDbType.Decimal, Value = (object)minCost ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@maxCost", SqlDbType = SqlDbType.Decimal, Value = (object)maxCost ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@gender", SqlDbType = SqlDbType.NVarChar, Value = (object)gender ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@sortMode", SqlDbType = SqlDbType.NVarChar, Value = (object)sortMode ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@minTime", SqlDbType = SqlDbType.BigInt, Value = (object)minTime ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@maxTime", SqlDbType = SqlDbType.BigInt, Value = (object)maxTime ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@minAge", SqlDbType = SqlDbType.BigInt, Value = (object)minAge ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@maxAge", SqlDbType = SqlDbType.BigInt, Value = (object)maxAge ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@page", SqlDbType = SqlDbType.BigInt, Value = (object)page ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@limit", SqlDbType = SqlDbType.BigInt, Value = (object)limit ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@from", SqlDbType = SqlDbType.NVarChar, Value = (object)from ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@to", SqlDbType = SqlDbType.NVarChar, Value = (object)to ?? DBNull.Value });

                var ds = DBProvider.GetDataSet("GetFilteredEvents_v1", CommandType.StoredProcedure, ref lstSqlParameter);

                if (ds != null && ds.Tables.Count > 0)
                {
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        DataTable dt_Events = ds.Tables[0];
                        int rowCount = dt_Events.Rows.Count;

                        eventContainer.count = Convert.ToInt32(ds.Tables[1].Rows.Count > 0 ? ds.Tables[1].Rows[0]["TotalCount"].ToString() : "0");

                        var lstEventID = (from ID in dt_Events.AsEnumerable() select ID["id"]).Distinct().ToList();

                        string EventIDs = string.Join(",", lstEventID);

                        lstSqlParameter = new List<SqlParameter>();
                        lstSqlParameter.Add(new SqlParameter() { ParameterName = "@EventIDs", Value = EventIDs, SqlDbType = SqlDbType.NVarChar });

                        var dsMultiEventData = DBProvider.GetDataSet("GetMultiEventData", CommandType.StoredProcedure, ref lstSqlParameter);

                        #region All ChildData
                        var dtLocation = dsMultiEventData.Tables[0];
                        var dtOrganization = dsMultiEventData.Tables[1];
                        var dtSubEvents = dsMultiEventData.Tables[2];
                        var dtEventSchedule = dsMultiEventData.Tables[3];
                        var dtOccurrence = dsMultiEventData.Tables[4];
                        var dtOffer = dsMultiEventData.Tables[5];
                        #endregion
                        IEnumerable<dynamic> strList = new List<dynamic>();
                            //List<string> names = new List<string>();
                        for (int i = 0; i < rowCount; i++)
                        {

                            var row = dt_Events.Rows[i];
                            var eventid = Convert.ToInt64(row["Id"]);
                            var opportunity = new ExpandoObject();
                            dynamic objDynamicData = new ExpandoObject();
                            objDynamicData.offers = new List<ExpandoObject>();
                            objDynamicData.cost = "Contact organiser";
                            objDynamicData.tags = Array.Empty<string>();
                            objDynamicData.weekdays = Array.Empty<int>();
                            objDynamicData.categories = new ExpandoObject();
                            objDynamicData.categories.disabilitySupport = Array.Empty<string>();
                            objDynamicData.occurrences = new List<ExpandoObject>();
                            objDynamicData.contact = new ExpandoObject();
                            objDynamicData.contact.pointOfContact = new ExpandoObject();
                            objDynamicData.contact.organisation = new ExpandoObject();

                            objDynamicData.description = Convert.ToString(row["Description"]);


                            if (row["Category"] != DBNull.Value)
                            {
                                objDynamicData.tags = row["Category"].ToString().Split(',').Where(x => !string.IsNullOrEmpty(x)).ToArray();
                            }

                            if (row["MinAge"] != DBNull.Value)
                            {
                                if (!((IDictionary<string, Object>)objDynamicData).ContainsKey("restrictions"))
                                {
                                    objDynamicData.restrictions = new ExpandoObject();
                                    objDynamicData.restrictions.minAge = Convert.ToInt32(row["MinAge"]);
                                }
                            }
                            else
                            {
                                objDynamicData.restrictions = new ExpandoObject();
                                objDynamicData.restrictions.minAge = 0;
                            }

                            if (row["MaxAge"] != DBNull.Value)
                            {
                                if (!((IDictionary<string, Object>)objDynamicData).ContainsKey("restrictions"))
                                {
                                    objDynamicData.restrictions = new ExpandoObject();
                                    objDynamicData.restrictions.maxAge = Convert.ToInt32(row["MaxAge"]);
                                }
                            }
                            else
                            {
                                objDynamicData.restrictions = new ExpandoObject();
                                objDynamicData.restrictions.maxAge = 0;
                            }

                            if (row["GenderRestriction"] != DBNull.Value)
                            {
                                if (!((IDictionary<string, Object>)objDynamicData).ContainsKey("restrictions"))
                                    objDynamicData.restrictions = new ExpandoObject();
                                objDynamicData.restrictions.gender = Convert.ToString(row["GenderRestriction"]);
                            }
                            else
                            {
                                objDynamicData.restrictions = new ExpandoObject();
                                objDynamicData.restrictions.gender = "";
                            }

                            if (row["WeekDays"] != DBNull.Value)
                                objDynamicData.weekdays = row["WeekDays"].ToString().Split(',').Select(n => Convert.ToInt32(n)).ToArray();

                            if (row["Duration"] != DBNull.Value)
                            {
                                TimeSpan ts = XmlConvert.ToTimeSpan(row["Duration"].ToString());
                                objDynamicData.durationMins = Convert.ToInt32(ts.TotalMinutes);
                            }
                            else
                            {
                                objDynamicData.durationMins = 0;
                            }

                            objDynamicData.title = Convert.ToString(row["Name"]);

                            if (row["AccessibilitySupport"] != DBNull.Value)
                                objDynamicData.categories.disabilitySupport = Convert.ToString(row["AccessibilitySupport"]).Split(',');
                            else
                                objDynamicData.categories.disabilitySupport = new string[] { };

                            if (row["Activity"] != DBNull.Value)
                                objDynamicData.categories.activities = Convert.ToString(row["Activity"]).Split(',');
                            else
                                objDynamicData.categories.activities = new string[] { };

                            objDynamicData.id = Convert.ToString(row["FeedName"]) + "-" + Convert.ToString(row["FeedId"]);

                            if (row["Image"] != DBNull.Value)
                                objDynamicData.image = Convert.ToString(row["Image"]);
                            else
                                objDynamicData.image = "";

                            objDynamicData.href = Settings.GetAppSetting("sessionDetailAPI") + objDynamicData.id;

                            #region Location
                            var oLocationRow = dtLocation.Select("EventId =" + eventid);
                          
                            if (oLocationRow != null && oLocationRow.Count() > 0)
                            {
                                objDynamicData.location = new ExpandoObject();
                                objDynamicData.location.coordinates = new ExpandoObject();
                                foreach (DataRow childRow in oLocationRow)
                                {
                                    if (childRow["Lat"] != DBNull.Value)
                                        objDynamicData.location.coordinates.lat = Convert.ToDecimal(childRow["Lat"]);
                                    else
                                        objDynamicData.location.coordinates.lat = 0;

                                    if (childRow["Long"] != DBNull.Value)
                                        objDynamicData.location.coordinates.lng = Convert.ToDecimal(childRow["Long"]);
                                    else
                                        objDynamicData.location.coordinates.lng = 0;


                                    if (childRow["Address"] != DBNull.Value){
                                        objDynamicData.location.address = Convert.ToString(childRow["Address"]);

                                        objDynamicData1.Add(objDynamicData.location.address);
                                        //eventContainer= eventContainer.items.GroupBy(objDynamicData1[i]).First();

                                }
                                else
                                        objDynamicData.location.address = "";

                                    if (row["Distance"] != DBNull.Value)
                                        objDynamicData.location.distance = Convert.ToDecimal(row["Distance"]);
                                    else
                                        objDynamicData.location.distance = 0;
                             
                                    break;

                                }

                            }
                            else
                            {
                                #region ForErrorHandling AllowProperties
                                objDynamicData.location = new ExpandoObject();
                                objDynamicData.location.coordinates = new ExpandoObject();
                                objDynamicData.location.coordinates.lat = 0;
                                objDynamicData.location.coordinates.lng = 0;
                                objDynamicData.location.address = "";
                                objDynamicData.location.distance = 0;
                                #endregion
                            }
                            #endregion Location

                            #region Organization
                            var oOrganizationRow = dtOrganization.Select("EventId =" + eventid);
                            if (oOrganizationRow != null && oOrganizationRow.Count() > 0)
                            {
                                foreach (DataRow childRow in oOrganizationRow)
                                {
                                    if (childRow["Name"] != DBNull.Value)
                                        objDynamicData.contact.pointOfContact.name = Convert.ToString(childRow["Name"]);
                                    else
                                        objDynamicData.contact.pointOfContact.name = "";

                                    if (childRow["Email"] != DBNull.Value)
                                        objDynamicData.contact.pointOfContact.email = Convert.ToString(childRow["Email"]);
                                    else
                                        objDynamicData.contact.pointOfContact.email = "";

                                    if (childRow["Url"] != DBNull.Value)
                                        objDynamicData.contact.organisation.website = Convert.ToString(childRow["Url"]);
                                    else
                                        objDynamicData.contact.organisation.name = "";
                                      if (childRow["Name"] != DBNull.Value)
                                        objDynamicData.contact.organisation.name = Convert.ToString(childRow["Name"]);
                                    else
                                        objDynamicData.contact.organisation.website = "";
                                    if (childRow["Telephone"] != DBNull.Value)
                                        objDynamicData.contact.organisation.telephone = Convert.ToString(childRow["Telephone"]);
                                    else
                                        objDynamicData.contact.organisation.telephone = "";
                                    break;
                                }
                            }
                            else
                            {
                                #region ForErrorHandling AllowProperties
                                objDynamicData.contact.pointOfContact.name = "";
                                objDynamicData.contact.pointOfContact.email = "";
                                objDynamicData.contact.organisation.website = "";
                                  objDynamicData.contact.organisation.name = "";
                                objDynamicData.contact.organisation.telephone = "";
                                #endregion
                            }
                            #endregion Organization

                            #region Occurences 
                            var oOccurrenceRow = dtOccurrence.Select("EventId =" + eventid);
                            if (oOccurrenceRow != null && oOccurrenceRow.Count() > 0)
                            {
                                foreach (DataRow childRow in oOccurrenceRow)
                                {
                                    long subEventId = 0;
                                    dynamic occurance = new ExpandoObject();

                                    if (childRow["SubEventId"] != DBNull.Value)
                                        subEventId = Convert.ToInt64(childRow["SubEventId"]);

                                    if (childRow["StartDate"] != DBNull.Value)
                                    {
                                        DateTimeOffset? StartDate = DateTime.SpecifyKind(Convert.ToDateTime(childRow["StartDate"]), DateTimeKind.Utc);
                                        if (!string.IsNullOrEmpty(StartDate?.ToString()))
                                            occurance.start = StartDate?.UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ssK");
                                    }
                                    if (childRow["EndDate"] != DBNull.Value)
                                    {
                                        DateTimeOffset? EndDate = DateTime.SpecifyKind(Convert.ToDateTime(childRow["EndDate"]), DateTimeKind.Utc);
                                        if (!string.IsNullOrEmpty(EndDate?.ToString()))
                                            occurance.end = EndDate?.UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ssK");
                                    }

                                    //occurance.href =  Settings.GetAppSetting("sessionDetailAPI") + objDynamicData.id + (subEventId > 0 ? ("/subevent/" + occurance.start) : "");
                                    occurance.href = Settings.GetAppSetting("sessionDetailAPI") + objDynamicData.id + "/subevent/" + occurance.start;
                                    //occurance.checkoutUrl =  Settings.GetAppSetting("checkoutUrl") + objDynamicData.id + "/subevent/" + occurance.start;

                                    objDynamicData.occurrences.Add(occurance);
                                    break;
                                }
                            }
                            #endregion

                            #region offer
                            var oOffer = dtOffer.Select("EventId =" + eventid);
                            if (oOffer != null && oOffer.Count() > 0)
                            {
                                foreach (DataRow childRow in oOffer)
                                {
                                    dynamic offers = new ExpandoObject();
                                    //offers.Id = Convert.ToInt64(childRow["Id"]);
                                    offers.type = "Offer";
                                    //offers.identifier = Convert.ToString(childRow["Identifier"]);
                                    offers.name = Convert.ToString(childRow["Name"]);
                                    offers.price = Convert.ToString(childRow["Price"]);
                                    offers.priceCurrency = Convert.ToString(childRow["PriceCurrency"]);

                                    objDynamicData.offers.Add(offers);
                                }
                                //objDynamicData.cost = objDynamicData.offers[0].price;
                                IEnumerable<dynamic> MaxPrice = (dynamic)objDynamicData.offers;
                                var oPrice = MaxPrice.Select(x => Convert.ToDecimal(x.price)).ToList();
                                objDynamicData.cost = oPrice.Max();


                            }
                            //objDynamicData.offers
                            #endregion

                            opportunity = objDynamicData;

                            eventContainer.items.Add(opportunity);

                            #region build next page url in the end based on 
                            if ((i + 1) == rowCount && eventContainer.count > limit && rowCount >= limit && rowCount <= eventContainer.count)
                            {
                                if (!((IDictionary<string, Object>)eventContainer).ContainsKey("next"))
                                    eventContainer.next = Settings.GetAppSetting("opportunityAPI");

                                eventContainer.next += eventContainer.next.Contains("?") ? "&" : "?";

                                #region required params
                                eventContainer.next += "lat=" + latitude;
                                eventContainer.next += "&";
                                eventContainer.next += "lng=" + longitude;
                                eventContainer.next += "&";
                                eventContainer.next += "radius=" + radius;
                                eventContainer.next += "&";
                                eventContainer.next += "page=" + (page + 1);
                                eventContainer.next += "&";
                                eventContainer.next += "limit=" + limit;
                                #endregion

                                #region Optional Params
                                eventContainer.next += GetUrlFromGivenString(eventContainer.next, source, "source");
                                eventContainer.next += GetUrlFromGivenString(eventContainer.next, activity, "activity");
                                eventContainer.next += GetUrlFromGivenString(eventContainer.next, disabilitySupport, "disabilitySupport");
                                eventContainer.next += GetUrlFromGivenString(eventContainer.next, weekdays, "weekdays");
                                eventContainer.next += GetUrlFromGivenString(eventContainer.next, kind, "kind");
                                eventContainer.next += GetUrlFromGivenString(eventContainer.next, tag, "tag");
                                eventContainer.next += GetUrlFromGivenString(eventContainer.next, excludeTag, "excludeTag");
                                eventContainer.next += GetUrlFromGivenString(eventContainer.next, sortMode, "sortMode");
                                eventContainer.next += GetUrlFromGivenString(eventContainer.next, gender, "gender");
                                eventContainer.next += GetUrlFromGivenString(eventContainer.next, from, "from");
                                eventContainer.next += GetUrlFromGivenString(eventContainer.next, to, "to");
                                eventContainer.next += GetUrlFromGivenString(eventContainer.next, minTime?.ToString(), "minTime");
                                eventContainer.next += GetUrlFromGivenString(eventContainer.next, maxTime?.ToString(), "maxTime");
                                eventContainer.next += GetUrlFromGivenString(eventContainer.next, minAge?.ToString(), "minAge");
                                eventContainer.next += GetUrlFromGivenString(eventContainer.next, maxAge?.ToString(), "maxAge");
                                eventContainer.next += GetUrlFromGivenString(eventContainer.next, minCost?.ToString(), "minCost");
                                eventContainer.next += GetUrlFromGivenString(eventContainer.next, maxCost?.ToString(), "maxCost");
                                #endregion
                            }
                            #endregion

                            #region build prev page url in the end based on 
                            if ((i + 1) == rowCount && eventContainer.count > limit && page > 1)
                            {
                                if (!((IDictionary<string, Object>)eventContainer).ContainsKey("prev"))
                                    eventContainer.prev = Settings.GetAppSetting("opportunityAPI");

                                eventContainer.prev += eventContainer.prev.Contains("?") ? "&" : "?";

                                #region required params
                                eventContainer.prev += "lat=" + latitude;
                                eventContainer.prev += "&";
                                eventContainer.prev += "lng=" + longitude;
                                eventContainer.prev += "&";
                                eventContainer.prev += "radius=" + radius;
                                eventContainer.prev += "&";
                                eventContainer.prev += "page=" + (page - 1);
                                eventContainer.prev += "&";
                                eventContainer.prev += "limit=" + limit;
                                #endregion

                                #region Optional Params
                                eventContainer.prev += GetUrlFromGivenString(eventContainer.prev, source, "source");
                                eventContainer.prev += GetUrlFromGivenString(eventContainer.prev, activity, "activity");
                                eventContainer.prev += GetUrlFromGivenString(eventContainer.prev, disabilitySupport, "disabilitySupport");
                                eventContainer.prev += GetUrlFromGivenString(eventContainer.prev, weekdays, "weekdays");
                                eventContainer.prev += GetUrlFromGivenString(eventContainer.prev, kind, "kind");
                                eventContainer.prev += GetUrlFromGivenString(eventContainer.prev, tag, "tag");
                                eventContainer.prev += GetUrlFromGivenString(eventContainer.prev, excludeTag, "excludeTag");
                                eventContainer.prev += GetUrlFromGivenString(eventContainer.prev, sortMode, "sortMode");
                                eventContainer.prev += GetUrlFromGivenString(eventContainer.prev, gender, "gender");
                                eventContainer.prev += GetUrlFromGivenString(eventContainer.prev, from, "from");
                                eventContainer.prev += GetUrlFromGivenString(eventContainer.prev, to, "to");
                                eventContainer.prev += GetUrlFromGivenString(eventContainer.prev, minTime?.ToString(), "minTime");
                                eventContainer.prev += GetUrlFromGivenString(eventContainer.prev, maxTime?.ToString(), "maxTime");
                                eventContainer.prev += GetUrlFromGivenString(eventContainer.prev, minAge?.ToString(), "minAge");
                                eventContainer.prev += GetUrlFromGivenString(eventContainer.prev, maxAge?.ToString(), "maxAge");
                                eventContainer.prev += GetUrlFromGivenString(eventContainer.prev, minCost?.ToString(), "minCost");
                                eventContainer.prev += GetUrlFromGivenString(eventContainer.prev, maxCost?.ToString(), "maxCost");
                                #endregion
                            }
                       

                            #endregion
                            
                            
                    //eventContainer = eventContainer.items.DistinctBy(eventContainer.items.location.address).ToList();

                        }
                        var distinctItems = objDynamicData1.Cast<object>().Distinct();
                      //var a = eventContainer.items;
                  //strList  = Uniquerecords.GroupBy(objDynamicData1);
         
         List<dynamic> strList2 = eventContainer.items;
        eventContainer.items = null;
       

    
    

         eventContainer2 = strList2.GroupBy(x => new{x.location.address , x.contact.organisation.name}).Select(g => g.First()).Take(50).ToList(); 
            var  count =    strList2.GroupBy(x=>x.location.address).Select(g => g.Count()).ToList();


         eventContainer.items = eventContainer2 ;
        //eventContainer.count = count;
         
// List<SessionDetails> distinct = sessions.GroupBy(x => x.Data.Location).Select(g => g.First()).ToList();

                      
                    }
                } 
            }
            catch (Exception ex)
            {
                var oParameter = string.Concat("Error occured following parameter ",
                                        "latitude = ", latitude,
                                        " ,longitude = ", longitude,
                                        " ,radius = ", radius,
                                        " ,source = ", source,
                                        " ,kind = ", kind,
                                        " ,tag = ", tag,
                                        " ,excludeTag = ", excludeTag,
                                        " ,activity = ", activity,
                                        " ,disabilitySupport = ", disabilitySupport,
                                        " ,weekdays = ", weekdays,
                                        " ,minCost = ", minCost,
                                        " ,maxCost = ", maxCost,
                                        " ,gender = ", gender,
                                        " ,sortMode = ", sortMode,
                                        " ,minTime = ", minTime,
                                        " ,maxTime = ", maxTime,
                                        " ,minAge = ", minAge,
                                        " ,maxAge = ", maxAge,
                                        " ,page = ", page,
                                        " ,limit = ", limit,
                                        " ,from = ", from,
                                        " ,to = ", to);
                LogHelper.InsertErrorLogs("[DataLaundryApi] FeedHelper", "GetEventsDynamically_V1", ex.Message, string.Concat(ex.InnerException?.Message, oParameter), ex.StackTrace);
            }
             
           

            return eventContainer;
        }
        #region  Similar event-- olson
  public static dynamic GetSimilarEventsDynamically_V1(double? latitude, double? longitude, double? radius,
                                            string source = null, string kind = null, string tag = null, string excludeTag = null,
                                            string activity = null, string disabilitySupport = null, string weekdays = null,
                                            double? minCost = null, double? maxCost = null, string gender = null,
                                            string sortMode = null, long? minTime = null, long? maxTime = null,
                                            long? minAge = null, long? maxAge = null, long? page = 1,
                                            long? limit = 50, string from = null, string to = null)
        {
            dynamic eventContainer = new ExpandoObject();
            List<dynamic> eventContainer2 = new List<dynamic>();

            eventContainer.count = 0;
            eventContainer.limit = limit;
            eventContainer.page = page;
            eventContainer.items = new List<dynamic>();
            ArrayList objDynamicData1 = new ArrayList();

            try
            {
                var lstSqlParameter = new List<SqlParameter>();
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@lat", SqlDbType = SqlDbType.Decimal, Value = (object)latitude ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@long", SqlDbType = SqlDbType.Decimal, Value = (object)longitude ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@radius", SqlDbType = SqlDbType.Decimal, Value = (object)radius ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@source", SqlDbType = SqlDbType.NVarChar, Value = (object)source ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@kind", SqlDbType = SqlDbType.NVarChar, Value = (object)kind ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@tag", SqlDbType = SqlDbType.NVarChar, Value = (object)tag ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@excludeTag", SqlDbType = SqlDbType.NVarChar, Value = (object)excludeTag ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@activity", SqlDbType = SqlDbType.NVarChar, Value = (object)activity ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@disabilitySupport", SqlDbType = SqlDbType.NVarChar, Value = (object)disabilitySupport ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@weekdays", SqlDbType = SqlDbType.NVarChar, Value = (object)weekdays ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@minCost", SqlDbType = SqlDbType.Decimal, Value = (object)minCost ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@maxCost", SqlDbType = SqlDbType.Decimal, Value = (object)maxCost ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@gender", SqlDbType = SqlDbType.NVarChar, Value = (object)gender ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@sortMode", SqlDbType = SqlDbType.NVarChar, Value = (object)sortMode ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@minTime", SqlDbType = SqlDbType.BigInt, Value = (object)minTime ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@maxTime", SqlDbType = SqlDbType.BigInt, Value = (object)maxTime ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@minAge", SqlDbType = SqlDbType.BigInt, Value = (object)minAge ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@maxAge", SqlDbType = SqlDbType.BigInt, Value = (object)maxAge ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@page", SqlDbType = SqlDbType.BigInt, Value = (object)page ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@limit", SqlDbType = SqlDbType.BigInt, Value = (object)limit ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@from", SqlDbType = SqlDbType.NVarChar, Value = (object)from ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@to", SqlDbType = SqlDbType.NVarChar, Value = (object)to ?? DBNull.Value });

                var ds = DBProvider.GetDataSet("GetFilteredEvents_v1", CommandType.StoredProcedure, ref lstSqlParameter);

                if (ds != null && ds.Tables.Count > 0)
                {
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        DataTable dt_Events = ds.Tables[0];
                        int rowCount = dt_Events.Rows.Count;

                        eventContainer.count = Convert.ToInt32(ds.Tables[1].Rows.Count > 0 ? ds.Tables[1].Rows[0]["TotalCount"].ToString() : "0");

                        var lstEventID = (from ID in dt_Events.AsEnumerable() select ID["id"]).Distinct().ToList();

                        string EventIDs = string.Join(",", lstEventID);

                        lstSqlParameter = new List<SqlParameter>();
                        lstSqlParameter.Add(new SqlParameter() { ParameterName = "@EventIDs", Value = EventIDs, SqlDbType = SqlDbType.NVarChar });

                        var dsMultiEventData = DBProvider.GetDataSet("GetMultiEventData", CommandType.StoredProcedure, ref lstSqlParameter);

                        #region All ChildData
                        var dtLocation = dsMultiEventData.Tables[0];
                        var dtOrganization = dsMultiEventData.Tables[1];
                        var dtSubEvents = dsMultiEventData.Tables[2];
                        var dtEventSchedule = dsMultiEventData.Tables[3];
                        var dtOccurrence = dsMultiEventData.Tables[4];
                        var dtOffer = dsMultiEventData.Tables[5];
                        #endregion
                        IEnumerable<dynamic> strList = new List<dynamic>();
                            //List<string> names = new List<string>();
                        for (int i = 0; i < rowCount; i++)
                        {

                            var row = dt_Events.Rows[i];
                            var eventid = Convert.ToInt64(row["Id"]);
                            var opportunity = new ExpandoObject();
                            dynamic objDynamicData = new ExpandoObject();
                            objDynamicData.offers = new List<ExpandoObject>();
                            objDynamicData.cost = "Contact organiser";
                            objDynamicData.tags = Array.Empty<string>();
                            objDynamicData.weekdays = Array.Empty<int>();
                            objDynamicData.categories = new ExpandoObject();
                            objDynamicData.categories.disabilitySupport = Array.Empty<string>();
                            objDynamicData.occurrences = new List<ExpandoObject>();
                            objDynamicData.contact = new ExpandoObject();
                            objDynamicData.contact.pointOfContact = new ExpandoObject();
                            objDynamicData.contact.organisation = new ExpandoObject();

                            objDynamicData.description = Convert.ToString(row["Description"]);


                            if (row["Category"] != DBNull.Value)
                            {
                                objDynamicData.tags = row["Category"].ToString().Split(',').Where(x => !string.IsNullOrEmpty(x)).ToArray();
                            }

                            if (row["MinAge"] != DBNull.Value)
                            {
                                if (!((IDictionary<string, Object>)objDynamicData).ContainsKey("restrictions"))
                                {
                                    objDynamicData.restrictions = new ExpandoObject();
                                    objDynamicData.restrictions.minAge = Convert.ToInt32(row["MinAge"]);
                                }
                            }
                            else
                            {
                                objDynamicData.restrictions = new ExpandoObject();
                                objDynamicData.restrictions.minAge = 0;
                            }

                            if (row["MaxAge"] != DBNull.Value)
                            {
                                if (!((IDictionary<string, Object>)objDynamicData).ContainsKey("restrictions"))
                                {
                                    objDynamicData.restrictions = new ExpandoObject();
                                    objDynamicData.restrictions.maxAge = Convert.ToInt32(row["MaxAge"]);
                                }
                            }
                            else
                            {
                                objDynamicData.restrictions = new ExpandoObject();
                                objDynamicData.restrictions.maxAge = 0;
                            }

                            if (row["GenderRestriction"] != DBNull.Value)
                            {
                                if (!((IDictionary<string, Object>)objDynamicData).ContainsKey("restrictions"))
                                    objDynamicData.restrictions = new ExpandoObject();
                                objDynamicData.restrictions.gender = Convert.ToString(row["GenderRestriction"]);
                            }
                            else
                            {
                                objDynamicData.restrictions = new ExpandoObject();
                                objDynamicData.restrictions.gender = "";
                            }

                            if (row["WeekDays"] != DBNull.Value)
                                objDynamicData.weekdays = row["WeekDays"].ToString().Split(',').Select(n => Convert.ToInt32(n)).ToArray();

                            if (row["Duration"] != DBNull.Value)
                            {
                                TimeSpan ts = XmlConvert.ToTimeSpan(row["Duration"].ToString());
                                objDynamicData.durationMins = Convert.ToInt32(ts.TotalMinutes);
                            }
                            else
                            {
                                objDynamicData.durationMins = 0;
                            }

                            objDynamicData.title = Convert.ToString(row["Name"]);

                            if (row["AccessibilitySupport"] != DBNull.Value)
                                objDynamicData.categories.disabilitySupport = Convert.ToString(row["AccessibilitySupport"]).Split(',');
                            else
                                objDynamicData.categories.disabilitySupport = new string[] { };

                            if (row["Activity"] != DBNull.Value)
                                objDynamicData.categories.activities = Convert.ToString(row["Activity"]).Split(',');
                            else
                                objDynamicData.categories.activities = new string[] { };

                            objDynamicData.id = Convert.ToString(row["FeedName"]) + "-" + Convert.ToString(row["FeedId"]);

                            if (row["Image"] != DBNull.Value)
                                objDynamicData.image = Convert.ToString(row["Image"]);
                            else
                                objDynamicData.image = "";

                            objDynamicData.href = Settings.GetAppSetting("sessionDetailAPI") + objDynamicData.id;

                            #region Location
                            var oLocationRow = dtLocation.Select("EventId =" + eventid);
                          
                            if (oLocationRow != null && oLocationRow.Count() > 0)
                            {
                                objDynamicData.location = new ExpandoObject();
                                objDynamicData.location.coordinates = new ExpandoObject();
                                foreach (DataRow childRow in oLocationRow)
                                {
                                    if (childRow["Lat"] != DBNull.Value)
                                        objDynamicData.location.coordinates.lat = Convert.ToDecimal(childRow["Lat"]);
                                    else
                                        objDynamicData.location.coordinates.lat = 0;

                                    if (childRow["Long"] != DBNull.Value)
                                        objDynamicData.location.coordinates.lng = Convert.ToDecimal(childRow["Long"]);
                                    else
                                        objDynamicData.location.coordinates.lng = 0;


                                    if (childRow["Address"] != DBNull.Value){
                                        objDynamicData.location.address = Convert.ToString(childRow["Address"]);

                                        objDynamicData1.Add(objDynamicData.location.address);
                                        //eventContainer= eventContainer.items.GroupBy(objDynamicData1[i]).First();

                                }
                                else
                                        objDynamicData.location.address = "";

                                    if (row["Distance"] != DBNull.Value)
                                        objDynamicData.location.distance = Convert.ToDecimal(row["Distance"]);
                                    else
                                        objDynamicData.location.distance = 0;
                             
                                    break;

                                }

                            }
                            else
                            {
                                #region ForErrorHandling AllowProperties
                                objDynamicData.location = new ExpandoObject();
                                objDynamicData.location.coordinates = new ExpandoObject();
                                objDynamicData.location.coordinates.lat = 0;
                                objDynamicData.location.coordinates.lng = 0;
                                objDynamicData.location.address = "";
                                objDynamicData.location.distance = 0;
                                #endregion
                            }
                            #endregion Location

                            #region Organization
                            var oOrganizationRow = dtOrganization.Select("EventId =" + eventid);
                            if (oOrganizationRow != null && oOrganizationRow.Count() > 0)
                            {
                                foreach (DataRow childRow in oOrganizationRow)
                                {
                                    if (childRow["Name"] != DBNull.Value)
                                        objDynamicData.contact.pointOfContact.name = Convert.ToString(childRow["Name"]);
                                    else
                                        objDynamicData.contact.pointOfContact.name = "";

                                    if (childRow["Email"] != DBNull.Value)
                                        objDynamicData.contact.pointOfContact.email = Convert.ToString(childRow["Email"]);
                                    else
                                        objDynamicData.contact.pointOfContact.email = "";

                                    if (childRow["Url"] != DBNull.Value)
                                        objDynamicData.contact.organisation.website = Convert.ToString(childRow["Url"]);
                                    else
                                        objDynamicData.contact.organisation.website = "";

                                    if (childRow["Telephone"] != DBNull.Value)
                                        objDynamicData.contact.organisation.telephone = Convert.ToString(childRow["Telephone"]);
                                    else
                                        objDynamicData.contact.organisation.telephone = "";
                                    break;
                                }
                            }
                            else
                            {
                                #region ForErrorHandling AllowProperties
                                objDynamicData.contact.pointOfContact.name = "";
                                objDynamicData.contact.pointOfContact.email = "";
                                objDynamicData.contact.organisation.website = "";
                                objDynamicData.contact.organisation.telephone = "";
                                #endregion
                            }
                            #endregion Organization

                            #region Occurences 
                            var oOccurrenceRow = dtOccurrence.Select("EventId =" + eventid);
                            if (oOccurrenceRow != null && oOccurrenceRow.Count() > 0)
                            {
                                foreach (DataRow childRow in oOccurrenceRow)
                                {
                                    long subEventId = 0;
                                    dynamic occurance = new ExpandoObject();

                                    if (childRow["SubEventId"] != DBNull.Value)
                                        subEventId = Convert.ToInt64(childRow["SubEventId"]);

                                    if (childRow["StartDate"] != DBNull.Value)
                                    {
                                        DateTimeOffset? StartDate = DateTime.SpecifyKind(Convert.ToDateTime(childRow["StartDate"]), DateTimeKind.Utc);
                                        if (!string.IsNullOrEmpty(StartDate?.ToString()))
                                            occurance.start = StartDate?.UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ssK");
                                    }
                                    if (childRow["EndDate"] != DBNull.Value)
                                    {
                                        DateTimeOffset? EndDate = DateTime.SpecifyKind(Convert.ToDateTime(childRow["EndDate"]), DateTimeKind.Utc);
                                        if (!string.IsNullOrEmpty(EndDate?.ToString()))
                                            occurance.end = EndDate?.UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ssK");
                                    }

                                    //occurance.href =  Settings.GetAppSetting("sessionDetailAPI") + objDynamicData.id + (subEventId > 0 ? ("/subevent/" + occurance.start) : "");
                                    occurance.href = Settings.GetAppSetting("sessionDetailAPI") + objDynamicData.id + "/subevent/" + occurance.start;
                                    //occurance.checkoutUrl =  Settings.GetAppSetting("checkoutUrl") + objDynamicData.id + "/subevent/" + occurance.start;

                                    objDynamicData.occurrences.Add(occurance);
                                    break;
                                }
                            }
                            #endregion

                            #region offer
                            var oOffer = dtOffer.Select("EventId =" + eventid);
                            if (oOffer != null && oOffer.Count() > 0)
                            {
                                foreach (DataRow childRow in oOffer)
                                {
                                    dynamic offers = new ExpandoObject();
                                    //offers.Id = Convert.ToInt64(childRow["Id"]);
                                    offers.type = "Offer";
                                    //offers.identifier = Convert.ToString(childRow["Identifier"]);
                                    offers.name = Convert.ToString(childRow["Name"]);
                                    offers.price = Convert.ToString(childRow["Price"]);
                                    offers.priceCurrency = Convert.ToString(childRow["PriceCurrency"]);

                                    objDynamicData.offers.Add(offers);
                                }
                                //objDynamicData.cost = objDynamicData.offers[0].price;
                                IEnumerable<dynamic> MaxPrice = (dynamic)objDynamicData.offers;
                                var oPrice = MaxPrice.Select(x => Convert.ToDecimal(x.price)).ToList();
                                objDynamicData.cost = oPrice.Max();


                            }
                            //objDynamicData.offers
                            #endregion

                            opportunity = objDynamicData;

                            eventContainer.items.Add(opportunity);

                            #region build next page url in the end based on 
                            if ((i + 1) == rowCount && eventContainer.count > limit && rowCount >= limit && rowCount <= eventContainer.count)
                            {
                                if (!((IDictionary<string, Object>)eventContainer).ContainsKey("next"))
                                    eventContainer.next = Settings.GetAppSetting("opportunityAPI");

                                eventContainer.next += eventContainer.next.Contains("?") ? "&" : "?";

                                #region required params
                                eventContainer.next += "lat=" + latitude;
                                eventContainer.next += "&";
                                eventContainer.next += "lng=" + longitude;
                                eventContainer.next += "&";
                                eventContainer.next += "radius=" + radius;
                                eventContainer.next += "&";
                                eventContainer.next += "page=" + (page + 1);
                                eventContainer.next += "&";
                                eventContainer.next += "limit=" + limit;
                                #endregion

                                #region Optional Params
                                eventContainer.next += GetUrlFromGivenString(eventContainer.next, source, "source");
                                eventContainer.next += GetUrlFromGivenString(eventContainer.next, activity, "activity");
                                eventContainer.next += GetUrlFromGivenString(eventContainer.next, disabilitySupport, "disabilitySupport");
                                eventContainer.next += GetUrlFromGivenString(eventContainer.next, weekdays, "weekdays");
                                eventContainer.next += GetUrlFromGivenString(eventContainer.next, kind, "kind");
                                eventContainer.next += GetUrlFromGivenString(eventContainer.next, tag, "tag");
                                eventContainer.next += GetUrlFromGivenString(eventContainer.next, excludeTag, "excludeTag");
                                eventContainer.next += GetUrlFromGivenString(eventContainer.next, sortMode, "sortMode");
                                eventContainer.next += GetUrlFromGivenString(eventContainer.next, gender, "gender");
                                eventContainer.next += GetUrlFromGivenString(eventContainer.next, from, "from");
                                eventContainer.next += GetUrlFromGivenString(eventContainer.next, to, "to");
                                eventContainer.next += GetUrlFromGivenString(eventContainer.next, minTime?.ToString(), "minTime");
                                eventContainer.next += GetUrlFromGivenString(eventContainer.next, maxTime?.ToString(), "maxTime");
                                eventContainer.next += GetUrlFromGivenString(eventContainer.next, minAge?.ToString(), "minAge");
                                eventContainer.next += GetUrlFromGivenString(eventContainer.next, maxAge?.ToString(), "maxAge");
                                eventContainer.next += GetUrlFromGivenString(eventContainer.next, minCost?.ToString(), "minCost");
                                eventContainer.next += GetUrlFromGivenString(eventContainer.next, maxCost?.ToString(), "maxCost");
                                #endregion
                            }
                            #endregion

                            #region build prev page url in the end based on 
                            if ((i + 1) == rowCount && eventContainer.count > limit && page > 1)
                            {
                                if (!((IDictionary<string, Object>)eventContainer).ContainsKey("prev"))
                                    eventContainer.prev = Settings.GetAppSetting("opportunityAPI");

                                eventContainer.prev += eventContainer.prev.Contains("?") ? "&" : "?";

                                #region required params
                                eventContainer.prev += "lat=" + latitude;
                                eventContainer.prev += "&";
                                eventContainer.prev += "lng=" + longitude;
                                eventContainer.prev += "&";
                                eventContainer.prev += "radius=" + radius;
                                eventContainer.prev += "&";
                                eventContainer.prev += "page=" + (page - 1);
                                eventContainer.prev += "&";
                                eventContainer.prev += "limit=" + limit;
                                #endregion

                                #region Optional Params
                                eventContainer.prev += GetUrlFromGivenString(eventContainer.prev, source, "source");
                                eventContainer.prev += GetUrlFromGivenString(eventContainer.prev, activity, "activity");
                                eventContainer.prev += GetUrlFromGivenString(eventContainer.prev, disabilitySupport, "disabilitySupport");
                                eventContainer.prev += GetUrlFromGivenString(eventContainer.prev, weekdays, "weekdays");
                                eventContainer.prev += GetUrlFromGivenString(eventContainer.prev, kind, "kind");
                                eventContainer.prev += GetUrlFromGivenString(eventContainer.prev, tag, "tag");
                                eventContainer.prev += GetUrlFromGivenString(eventContainer.prev, excludeTag, "excludeTag");
                                eventContainer.prev += GetUrlFromGivenString(eventContainer.prev, sortMode, "sortMode");
                                eventContainer.prev += GetUrlFromGivenString(eventContainer.prev, gender, "gender");
                                eventContainer.prev += GetUrlFromGivenString(eventContainer.prev, from, "from");
                                eventContainer.prev += GetUrlFromGivenString(eventContainer.prev, to, "to");
                                eventContainer.prev += GetUrlFromGivenString(eventContainer.prev, minTime?.ToString(), "minTime");
                                eventContainer.prev += GetUrlFromGivenString(eventContainer.prev, maxTime?.ToString(), "maxTime");
                                eventContainer.prev += GetUrlFromGivenString(eventContainer.prev, minAge?.ToString(), "minAge");
                                eventContainer.prev += GetUrlFromGivenString(eventContainer.prev, maxAge?.ToString(), "maxAge");
                                eventContainer.prev += GetUrlFromGivenString(eventContainer.prev, minCost?.ToString(), "minCost");
                                eventContainer.prev += GetUrlFromGivenString(eventContainer.prev, maxCost?.ToString(), "maxCost");
                                #endregion
                            }
                       

                            #endregion
                            
                            
                    //eventContainer = eventContainer.items.DistinctBy(eventContainer.items.location.address).ToList();

                        }
                        var distinctItems = objDynamicData1.Cast<object>().Distinct();
                      //var a = eventContainer.items;
                        //strList  = Uniquerecords.GroupBy(objDynamicData1);
                        List<dynamic> strList2 = eventContainer.items;
                        eventContainer.items = null;
       

    
       
     
         eventContainer2 = strList2.GroupBy(x=>x.location.address).Select(g => g.First()).ToList();
                var  count = strList2.GroupBy(x=>x.location.address).Select(g => g.Count()).ToList();

        eventContainer.items = eventContainer2;

         
// List<SessionDetails> distinct = sessions.GroupBy(x => x.Data.Location).Select(g => g.First()).ToList();

                      
                    }
                } 
            }
            catch (Exception ex)
            {
                var oParameter = string.Concat("Error occured following parameter ",
                                        "latitude = ", latitude,
                                        " ,longitude = ", longitude,
                                        " ,radius = ", radius,
                                        " ,source = ", source,
                                        " ,kind = ", kind,
                                        " ,tag = ", tag,
                                        " ,excludeTag = ", excludeTag,
                                        " ,activity = ", activity,
                                        " ,disabilitySupport = ", disabilitySupport,
                                        " ,weekdays = ", weekdays,
                                        " ,minCost = ", minCost,
                                        " ,maxCost = ", maxCost,
                                        " ,gender = ", gender,
                                        " ,sortMode = ", sortMode,
                                        " ,minTime = ", minTime,
                                        " ,maxTime = ", maxTime,
                                        " ,minAge = ", minAge,
                                        " ,maxAge = ", maxAge,
                                        " ,page = ", page,
                                        " ,limit = ", limit,
                                        " ,from = ", from,
                                        " ,to = ", to);
                LogHelper.InsertErrorLogs("[DataLaundryApi] FeedHelper", "GetEventsDynamically_V1", ex.Message, string.Concat(ex.InnerException?.Message, oParameter), ex.StackTrace);
            }
             
           

            return eventContainer;
        }

        #endregion
        public static ExpandoObject GetEventsDynamically(double? latitude, double? longitude, double? radius,
                                            string source = null, string kind = null, string tag = null, string excludeTag = null,
                                            string activity = null, string disabilitySupport = null, string weekdays = null,
                                            double? minCost = null, double? maxCost = null, string gender = null,
                                            string sortMode = null, long? minTime = null, long? maxTime = null,
                                            long? minAge = null, long? maxAge = null, long? page = 1,
                                            long? limit = 70, string from = null, string to = null)
        {
            //var eventContainer1 = new EventContainer();
            //eventContainer1.count = 0;
            //eventContainer1.limit = limit;
            //eventContainer1.page = page;

            dynamic eventContainer = new ExpandoObject();
            eventContainer.count = 0;
            eventContainer.limit = limit;
            eventContainer.page = page;
            eventContainer.items = new List<dynamic>();
            try
            {
                var lstSqlParameter = new List<SqlParameter>();
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@lat", SqlDbType = SqlDbType.Decimal, Value = (object)latitude ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@long", SqlDbType = SqlDbType.Decimal, Value = (object)longitude ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@radius", SqlDbType = SqlDbType.Decimal, Value = (object)radius ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@source", SqlDbType = SqlDbType.NVarChar, Value = (object)source ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@kind", SqlDbType = SqlDbType.NVarChar, Value = (object)kind ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@tag", SqlDbType = SqlDbType.NVarChar, Value = (object)tag ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@excludeTag", SqlDbType = SqlDbType.NVarChar, Value = (object)excludeTag ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@activity", SqlDbType = SqlDbType.NVarChar, Value = (object)activity ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@disabilitySupport", SqlDbType = SqlDbType.NVarChar, Value = (object)disabilitySupport ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@weekdays", SqlDbType = SqlDbType.NVarChar, Value = (object)weekdays ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@minCost", SqlDbType = SqlDbType.Decimal, Value = (object)minCost ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@maxCost", SqlDbType = SqlDbType.Decimal, Value = (object)maxCost ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@gender", SqlDbType = SqlDbType.NVarChar, Value = (object)gender ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@sortMode", SqlDbType = SqlDbType.NVarChar, Value = (object)sortMode ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@minTime", SqlDbType = SqlDbType.BigInt, Value = (object)minTime ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@maxTime", SqlDbType = SqlDbType.BigInt, Value = (object)maxTime ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@minAge", SqlDbType = SqlDbType.BigInt, Value = (object)minAge ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@maxAge", SqlDbType = SqlDbType.BigInt, Value = (object)maxAge ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@page", SqlDbType = SqlDbType.BigInt, Value = (object)page ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@limit", SqlDbType = SqlDbType.BigInt, Value = (object)limit ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@from", SqlDbType = SqlDbType.NVarChar, Value = (object)from ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@to", SqlDbType = SqlDbType.NVarChar, Value = (object)to ?? DBNull.Value });

                //var ds = DBProvider.GetDataSet("GetFilteredEvents", CommandType.StoredProcedure, ref lstSqlParameter);
                var ds = DBProvider.GetDataSet("GetFilteredEvents_v1", CommandType.StoredProcedure, ref lstSqlParameter);
                //var ds = DBProvider.GetDataSet("GetFilteredEvents_v2", CommandType.StoredProcedure, ref lstSqlParameter);

                if (ds != null && ds.Tables.Count > 0)
                {
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        DataTable dt_Events = ds.Tables[0];
                        int rowCount = dt_Events.Rows.Count;

                        eventContainer.count = Convert.ToInt32(ds.Tables[1].Rows.Count > 0 ? ds.Tables[1].Rows[0]["TotalCount"].ToString() : "0");

                        var lstEventID = (from ID in dt_Events.AsEnumerable() select ID["id"]).Distinct().ToList();

                        string EventIDs = string.Join(",", lstEventID);

                        for (int i = 0; i < rowCount; i++)
                        {
                            var row = dt_Events.Rows[i];
                            //var opportunity = GetOpportunityFromDatarowDynamically(row, Convert.ToInt64(row["Id"]));
                            var opportunity = GetOpportunityFromDatarowDynamically_v1(row, Convert.ToInt64(row["Id"]), true);

                            eventContainer.items.Add(opportunity);

                            #region build next page url in the end based on 
                            if ((i + 1) == rowCount && eventContainer.count > limit && rowCount >= limit && rowCount <= eventContainer.count)
                            {
                                if (!((IDictionary<string, Object>)eventContainer).ContainsKey("next"))
                                    eventContainer.next = Settings.GetAppSetting("opportunityAPI");

                                eventContainer.next += eventContainer.next.Contains("?") ? "&" : "?";

                                #region required params
                                eventContainer.next += "lat=" + latitude;
                                eventContainer.next += "&";
                                eventContainer.next += "lng=" + longitude;
                                eventContainer.next += "&";
                                eventContainer.next += "radius=" + radius;
                                eventContainer.next += "&";
                                eventContainer.next += "page=" + (page + 1);
                                eventContainer.next += "&";
                                eventContainer.next += "limit=" + limit;
                                #endregion

                                #region Optional Params
                                eventContainer.next += GetUrlFromGivenString(eventContainer.next, source, "source");
                                eventContainer.next += GetUrlFromGivenString(eventContainer.next, activity, "activity");
                                eventContainer.next += GetUrlFromGivenString(eventContainer.next, disabilitySupport, "disabilitySupport");
                                eventContainer.next += GetUrlFromGivenString(eventContainer.next, weekdays, "weekdays");
                                eventContainer.next += GetUrlFromGivenString(eventContainer.next, kind, "kind");
                                eventContainer.next += GetUrlFromGivenString(eventContainer.next, tag, "tag");
                                eventContainer.next += GetUrlFromGivenString(eventContainer.next, excludeTag, "excludeTag");
                                eventContainer.next += GetUrlFromGivenString(eventContainer.next, sortMode, "sortMode");
                                eventContainer.next += GetUrlFromGivenString(eventContainer.next, gender, "gender");
                                eventContainer.next += GetUrlFromGivenString(eventContainer.next, from, "from");
                                eventContainer.next += GetUrlFromGivenString(eventContainer.next, to, "to");
                                eventContainer.next += GetUrlFromGivenString(eventContainer.next, minTime?.ToString(), "minTime");
                                eventContainer.next += GetUrlFromGivenString(eventContainer.next, maxTime?.ToString(), "maxTime");
                                eventContainer.next += GetUrlFromGivenString(eventContainer.next, minAge?.ToString(), "minAge");
                                eventContainer.next += GetUrlFromGivenString(eventContainer.next, maxAge?.ToString(), "maxAge");
                                eventContainer.next += GetUrlFromGivenString(eventContainer.next, minCost?.ToString(), "minCost");
                                eventContainer.next += GetUrlFromGivenString(eventContainer.next, maxCost?.ToString(), "maxCost");
                                #endregion
                            }
                            #endregion

                            #region build prev page url in the end based on 
                            if ((i + 1) == rowCount && eventContainer.count > limit && page > 1)
                            {
                                if (!((IDictionary<string, Object>)eventContainer).ContainsKey("prev"))
                                    eventContainer.prev = Settings.GetAppSetting("opportunityAPI");

                                eventContainer.prev += eventContainer.prev.Contains("?") ? "&" : "?";

                                #region required params
                                eventContainer.prev += "lat=" + latitude;
                                eventContainer.prev += "&";
                                eventContainer.prev += "lng=" + longitude;
                                eventContainer.prev += "&";
                                eventContainer.prev += "radius=" + radius;
                                eventContainer.prev += "&";
                                eventContainer.prev += "page=" + (page - 1);
                                eventContainer.prev += "&";
                                eventContainer.prev += "limit=" + limit;
                                #endregion

                                #region Optional Params
                                eventContainer.prev += GetUrlFromGivenString(eventContainer.prev, source, "source");
                                eventContainer.prev += GetUrlFromGivenString(eventContainer.prev, activity, "activity");
                                eventContainer.prev += GetUrlFromGivenString(eventContainer.prev, disabilitySupport, "disabilitySupport");
                                eventContainer.prev += GetUrlFromGivenString(eventContainer.prev, weekdays, "weekdays");
                                eventContainer.prev += GetUrlFromGivenString(eventContainer.prev, kind, "kind");
                                eventContainer.prev += GetUrlFromGivenString(eventContainer.prev, tag, "tag");
                                eventContainer.prev += GetUrlFromGivenString(eventContainer.prev, excludeTag, "excludeTag");
                                eventContainer.prev += GetUrlFromGivenString(eventContainer.prev, sortMode, "sortMode");
                                eventContainer.prev += GetUrlFromGivenString(eventContainer.prev, gender, "gender");
                                eventContainer.prev += GetUrlFromGivenString(eventContainer.prev, from, "from");
                                eventContainer.prev += GetUrlFromGivenString(eventContainer.prev, to, "to");
                                eventContainer.prev += GetUrlFromGivenString(eventContainer.prev, minTime?.ToString(), "minTime");
                                eventContainer.prev += GetUrlFromGivenString(eventContainer.prev, maxTime?.ToString(), "maxTime");
                                eventContainer.prev += GetUrlFromGivenString(eventContainer.prev, minAge?.ToString(), "minAge");
                                eventContainer.prev += GetUrlFromGivenString(eventContainer.prev, maxAge?.ToString(), "maxAge");
                                eventContainer.prev += GetUrlFromGivenString(eventContainer.prev, minCost?.ToString(), "minCost");
                                eventContainer.prev += GetUrlFromGivenString(eventContainer.prev, maxCost?.ToString(), "maxCost");
                                #endregion
                            }
                            #endregion
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var oParameter = string.Concat("Error occured following parameter ",
                                        "latitude = ", latitude,
                                        " ,longitude = ", longitude,
                                        " ,radius = ", radius,
                                        " ,source = ", source,
                                        " ,kind = ", kind,
                                        " ,tag = ", tag,
                                        " ,excludeTag = ", excludeTag,
                                        " ,activity = ", activity,
                                        " ,disabilitySupport = ", disabilitySupport,
                                        " ,weekdays = ", weekdays,
                                        " ,minCost = ", minCost,
                                        " ,maxCost = ", maxCost,
                                        " ,gender = ", gender,
                                        " ,sortMode = ", sortMode,
                                        " ,minTime = ", minTime,
                                        " ,maxTime = ", maxTime,
                                        " ,minAge = ", minAge,
                                        " ,maxAge = ", maxAge,
                                        " ,page = ", page,
                                        " ,limit = ", limit,
                                        " ,from = ", from,
                                        " ,to = ", to);
                LogHelper.InsertErrorLogs("[DataLaundryApi] FeedHelper", "GetEventsDynamically", ex.Message, oParameter, ex.StackTrace);
            }
            return eventContainer;
        }

        public static ExpandoObject GetEventDetailsByIdDynamically(long? eventId = 0)
        {
            ExpandoObject detail = new ExpandoObject();
            try
            {
                var lstSqlParameter = new List<SqlParameter>();
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@eventId", SqlDbType = SqlDbType.BigInt, Value = (object)eventId ?? DBNull.Value });

                var ds = DBProvider.GetDataSet("GetEventById", CommandType.StoredProcedure, ref lstSqlParameter);

                if (ds.Tables[0].Rows.Count > 0)
                {
                    var row = ds.Tables[0].Rows[0];
                    detail = GetOpportunityFromDatarowDynamically_v1(row, Convert.ToInt64(row["Id"]), false);
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertErrorLogs("[DataLaundryApi] FeedHelper", "GetEventDetailsByIdDynamically", ex.Message, ex.InnerException?.Message, ex.StackTrace);
            }
            return detail;
        }
        public static ExpandoObject GetSubEventDetailsByIdDynamically(long? eventId, string startDate)
        {       
            ExpandoObject detail = new ExpandoObject();
            
            try
            {
                var lstSqlParameter = new List<SqlParameter>();
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@eventId", SqlDbType = SqlDbType.BigInt, Value = (object)eventId ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@startDate", SqlDbType = SqlDbType.NVarChar, Value = (object)@startDate ?? DBNull.Value });

                var ds = DBProvider.GetDataSet("GetSubEventByEventId", CommandType.StoredProcedure, ref lstSqlParameter);

                if (ds.Tables[0].Rows.Count > 0)
                {
                    var row = ds.Tables[0].Rows[0];
                    detail = GetOpportunityFromDatarowDynamically_v1(row, Convert.ToInt64(row["Id"]), false);
                }

            }
            catch (Exception ex)
            {
                LogHelper.InsertErrorLogs("[DataLaundryApi] FeedHelper", "GetSubEventDetailsByIdDynamically", ex.Message, ex.InnerException?.Message, ex.StackTrace);
            }
            return detail;
        }
        public static ExpandoObject GetEventDetailsBySessionIdDynamically(string sessionId, string feedName = null)
        {
            dynamic detail = new ExpandoObject() as IDictionary<string, Object>;;
            dynamic sessions = new ExpandoObject();

            try
            {
                var lstSqlParameter = new List<SqlParameter>();
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@sessionId", SqlDbType = SqlDbType.NVarChar, Value = (object)sessionId ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@FeedName", SqlDbType = SqlDbType.NVarChar, Value = (object)feedName ?? DBNull.Value });

                var ds = DBProvider.GetDataSet("GetEventBySessionId", CommandType.StoredProcedure, ref lstSqlParameter);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    var row = ds.Tables[0].Rows[0];
                    detail = GetOpportunityFromDatarowDynamically_v1(row, Convert.ToInt64(row["Id"]), false);
                }
                else
                {
                    dynamic objDynamicData = new ExpandoObject();

                    objDynamicData.offers = new List<ExpandoObject>();
                    objDynamicData.cost = "Contact organiser";
                    objDynamicData.tags = Array.Empty<string>();
                    objDynamicData.weekdays = Array.Empty<int>();
                    objDynamicData.categories = new ExpandoObject();
                    objDynamicData.categories.disabilitySupport = Array.Empty<string>();
                    objDynamicData.occurrences = new List<ExpandoObject>();
                    objDynamicData.contact = new ExpandoObject();
                    objDynamicData.contact.pointOfContact = new ExpandoObject();
                    objDynamicData.contact.organisation = new ExpandoObject();

                    objDynamicData.description = "";
                    objDynamicData.categories.disabilitySupport = new string[] { };
                    var a= objDynamicData.categories.activities = new string[] { };
                    objDynamicData.restrictions = new ExpandoObject();
                    objDynamicData.restrictions.minAge = 0;
                    objDynamicData.restrictions.maxAge = 0;
                    objDynamicData.restrictions = new ExpandoObject();
                    objDynamicData.restrictions.gender = "";
                    objDynamicData.durationMins = 0;
                    objDynamicData.title = "";
                    objDynamicData.id = "";
                    objDynamicData.image = "";
                    objDynamicData.href = "";
                    objDynamicData.location = new ExpandoObject();
                    objDynamicData.location.coordinates = new ExpandoObject();
                    objDynamicData.location.coordinates.lat = 0;
                    objDynamicData.location.coordinates.lng = 0;
                    objDynamicData.location.address = "";
                    objDynamicData.location.distance = 0;
                    objDynamicData.contact.pointOfContact.name = "";
                    objDynamicData.contact.pointOfContact.email = "";
                    objDynamicData.contact.pointOfContact.email = "";

                    objDynamicData.contact.organisation.website = "";
                    objDynamicData.contact.organisation.telephone = "";
                    objDynamicData.contact.organisation.name = "";


                }
                var DyObjectsList = new List<dynamic>();
                    // DyObjectsList = detail;
                    // List<dynamic> strList2 =detail;
                   //  var distinctItems = detail.Cast<object>();
             

                var activityName= detail.categories.activities[0];
                var Address = detail.location.address;
                var Organizer = detail.contact.pointOfContact.name;
                var latitude = detail.location.coordinates.lat;
                var longitude = detail.location.coordinates.lng;
                var sessionid = detail.id;

                sessions = FeedHelper.GetEventAddressWise(activityName,Address,Organizer,latitude,longitude,sessionid);   
               
               
                var expandoDict = detail as IDictionary<string,object>;
                if (expandoDict.ContainsKey("restrictions") && sessions.Count > 1){
                    
                    expandoDict["addresswisedata"] = sessions;
                   
                }
                else
                    expandoDict.Add("addresswisedata", null);

                detail = expandoDict;
                
               
            }
            catch (Exception ex)
            {
                LogHelper.InsertErrorLogs("[DataLaundryApi] FeedHelper", "GetEventDetailsBySessionIdDynamically", ex.Message, ex.InnerException?.Message, ex.StackTrace);
            }
            return detail;
        }
          
        public static ExpandoObject GetSubEventDetailsBySessionIdDynamically(string sessionId, string startDate, string feedName = null)
        {
            ExpandoObject detail = new ExpandoObject();
            try
            {
                var lstSqlParameter = new List<SqlParameter>();
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@sessionId", SqlDbType = SqlDbType.NVarChar, Value = (object)sessionId ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@FeedName", SqlDbType = SqlDbType.NVarChar, Value = (object)feedName ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@startDate", SqlDbType = SqlDbType.NVarChar, Value = (object)@startDate ?? DBNull.Value });

                var ds = DBProvider.GetDataSet("GetSubEventBySessionId", CommandType.StoredProcedure, ref lstSqlParameter);

                if (ds.Tables[0].Rows.Count > 0)
                {
                    var row = ds.Tables[0].Rows[0];
                    detail = GetOpportunityFromDatarowDynamically_v1(row, Convert.ToInt64(row["Id"]), false);
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertErrorLogs("[DataLaundryApi] FeedHelper", "GetSubEventDetailsBySessionIdDynamically", ex.Message, ex.InnerException?.Message, ex.StackTrace);
            }
            return detail;
        }
       
        #endregion

        #region Organisation
        private static OrganisationDetail GetOrganisationFromDataRow(DataRow row, long eventId)
        {
            var organisation = new OrganisationDetail();
            try
            {
                organisation.Id = Convert.ToString(row["Id"]);

                if (row["Name"] != DBNull.Value)
                    organisation.Title = Convert.ToString(row["Name"]);

                if (row["Description"] != DBNull.Value)
                    organisation.Description = Convert.ToString(row["Description"]);

                if (row["Image"] != DBNull.Value)
                    organisation.Image = Convert.ToString(row["Image"]);

                #region Event
                if (row["Category"] != DBNull.Value)
                    organisation.Tags = row["Category"].ToString().Split(',');

                if (row["AgeRange"] != DBNull.Value)
                {
                    string[] ages = Convert.ToString(row["AgeRange"]).Split('-');
                    organisation.Restrictions.MinAge = Convert.ToInt16(ages[0].Trim());
                    if (ages.Length > 1)
                        organisation.Restrictions.MaxAge = Convert.ToInt16(ages[1].Trim());
                }

                if (row["GenderRestriction"] != DBNull.Value)
                    organisation.Restrictions.Gender = Convert.ToString(row["GenderRestriction"]);

                if (row["AccessibilitySupport"] != DBNull.Value)
                    organisation.Categories.DisabilitySupport = Convert.ToString(row["AccessibilitySupport"]).Split(',');

                if (row["Activity"] != DBNull.Value)
                    organisation.Categories.Activities = Convert.ToString(row["Activity"]).Split(',');
                #endregion
                #region Location
                if (row["Lat"] != DBNull.Value)
                    organisation.Location.Coordinates.Lat = Convert.ToDecimal(row["Lat"]);

                if (row["Long"] != DBNull.Value)
                    organisation.Location.Coordinates.Lng = Convert.ToDecimal(row["Long"]);

                if (row["Address"] != DBNull.Value)
                    organisation.Location.Address = Convert.ToString(row["Address"]);
                #endregion
                #region Organisation Contact
                if (row["Name"] != DBNull.Value)
                    organisation.Contact.PointOfContact.Name = Convert.ToString(row["Name"]);
                if (row["Email"] != DBNull.Value)
                    organisation.Contact.PointOfContact.Email = Convert.ToString(row["Email"]);
                if (row["Url"] != DBNull.Value)
                    organisation.Contact.Organisation.Website = Convert.ToString(row["Url"]);
                if (row["Telephone"] != DBNull.Value)
                    organisation.Contact.Organisation.Telephone = Convert.ToString(row["Telephone"]);
                #endregion

                organisation.Href = Settings.GetAppSetting("organisationDetailAPI") + organisation.Id;
            }
            catch (Exception ex)
            {
                LogHelper.InsertErrorLogs("[DataLaundryApi] FeedHelper", "GetOrganisationFromDataRow", ex.Message, ex.InnerException?.Message, ex.StackTrace);
            }
            return organisation;
        }
        public static OrganisationContainer GetOrganisations(double? latitude, double? longitude, double? radius,
                                            string activity = null, string disabilitySupport = null, string gender = null,
                                            long? minAge = null, long? maxAge = null, long? page = 1, long? limit = 50,
                                            string from = null, string to = null, string source = null,
                                            string tag = null, string excludeTag = null)
        {
            OrganisationContainer organisationContainer = new OrganisationContainer();
            organisationContainer.Count = 0;
            organisationContainer.Limit = limit;
            organisationContainer.Page = page;

            try
            {
                var lstSqlParameter = new List<SqlParameter>();
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@lat", SqlDbType = SqlDbType.Decimal, Value = (object)latitude ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@long", SqlDbType = SqlDbType.Decimal, Value = (object)longitude ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@radius", SqlDbType = SqlDbType.Decimal, Value = (object)radius ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@activity", SqlDbType = SqlDbType.NVarChar, Value = (object)activity ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@disabilitySupport", SqlDbType = SqlDbType.NVarChar, Value = (object)disabilitySupport ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@gender", SqlDbType = SqlDbType.NVarChar, Value = (object)gender ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@source", SqlDbType = SqlDbType.NVarChar, Value = (object)source ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@tag", SqlDbType = SqlDbType.NVarChar, Value = (object)tag ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@excludeTag", SqlDbType = SqlDbType.NVarChar, Value = (object)excludeTag ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@minAge", SqlDbType = SqlDbType.BigInt, Value = (object)minAge ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@maxAge", SqlDbType = SqlDbType.BigInt, Value = (object)maxAge ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@page", SqlDbType = SqlDbType.BigInt, Value = (object)page ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@limit", SqlDbType = SqlDbType.BigInt, Value = (object)limit ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@from", SqlDbType = SqlDbType.NVarChar, Value = (object)from ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@to", SqlDbType = SqlDbType.NVarChar, Value = (object)to ?? DBNull.Value });

                //var ds = DBProvider.GetDataSet("GetOrganisations", CommandType.StoredProcedure, ref lstSqlParameter);
                var ds = DBProvider.GetDataSet("GetOrganisations_v1", CommandType.StoredProcedure, ref lstSqlParameter);

                if (ds != null && ds.Tables.Count > 0)
                {
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        DataTable dt_Events = ds.Tables[0];
                        int rowCount = dt_Events.Rows.Count;

                        organisationContainer.Count = Convert.ToInt32(ds.Tables[1].Rows.Count > 0 ? ds.Tables[1].Rows[0]["TotalCount"].ToString() : "0");

                        for (int i = 0; i < rowCount; i++)
                        {
                            var row = dt_Events.Rows[i];
                            var organisation = GetOrganisationFromDataRow(row, Convert.ToInt64(row["Id"]));
                            organisationContainer.Items.Add(organisation);

                            #region build next page url in the end based on 
                            if ((i + 1) == rowCount && organisationContainer.Count > limit && rowCount >= limit && rowCount <= organisationContainer.Count)
                            {
                                if (string.IsNullOrEmpty(organisationContainer.Next))
                                    organisationContainer.Next = Settings.GetAppSetting("organisationAPI");

                                organisationContainer.Next += organisationContainer.Next.Contains("?") ? "&" : "?";

                                #region required params
                                organisationContainer.Next += "lat=" + latitude;
                                organisationContainer.Next += "&";
                                organisationContainer.Next += "lng=" + longitude;
                                organisationContainer.Next += "&";
                                organisationContainer.Next += "radius=" + radius;
                                organisationContainer.Next += "&";
                                organisationContainer.Next += "page=" + (page + 1);
                                organisationContainer.Next += "&";
                                organisationContainer.Next += "limit=" + limit;
                                #endregion

                                #region Optional Params
                                organisationContainer.Next += GetUrlFromGivenString(organisationContainer.Next, source, "source");
                                organisationContainer.Next += GetUrlFromGivenString(organisationContainer.Next, activity, "activity");
                                organisationContainer.Next += GetUrlFromGivenString(organisationContainer.Next, disabilitySupport, "disabilitySupport");
                                organisationContainer.Next += GetUrlFromGivenString(organisationContainer.Next, tag, "tag");
                                organisationContainer.Next += GetUrlFromGivenString(organisationContainer.Next, excludeTag, "excludeTag");
                                organisationContainer.Next += GetUrlFromGivenString(organisationContainer.Next, gender, "gender");
                                organisationContainer.Next += GetUrlFromGivenString(organisationContainer.Next, to, "to");
                                organisationContainer.Next += GetUrlFromGivenString(organisationContainer.Next, minAge?.ToString(), "minAge");
                                organisationContainer.Next += GetUrlFromGivenString(organisationContainer.Next, maxAge?.ToString(), "maxAge");
                                #endregion
                            }
                            #endregion

                            #region build prev page url in the end based on 
                            if ((i + 1) == rowCount && organisationContainer.Count > limit && page > 1)
                            {
                                if (string.IsNullOrEmpty(organisationContainer.Prev))
                                    organisationContainer.Prev = Settings.GetAppSetting("organisationAPI");

                                organisationContainer.Prev += organisationContainer.Prev.Contains("?") ? "&" : "?";

                                #region required params
                                organisationContainer.Prev += "lat=" + latitude;
                                organisationContainer.Prev += "&";
                                organisationContainer.Prev += "lng=" + longitude;
                                organisationContainer.Prev += "&";
                                organisationContainer.Prev += "radius=" + radius;
                                organisationContainer.Prev += "&";
                                organisationContainer.Prev += "page=" + (page - 1);
                                organisationContainer.Prev += "&";
                                organisationContainer.Prev += "limit=" + limit;
                                #endregion

                                #region Optional Params
                                organisationContainer.Prev += GetUrlFromGivenString(organisationContainer.Prev, source, "source");
                                organisationContainer.Prev += GetUrlFromGivenString(organisationContainer.Prev, activity, "activity");
                                organisationContainer.Prev += GetUrlFromGivenString(organisationContainer.Prev, disabilitySupport, "disabilitySupport");
                                organisationContainer.Prev += GetUrlFromGivenString(organisationContainer.Prev, tag, "tag");
                                organisationContainer.Prev += GetUrlFromGivenString(organisationContainer.Prev, excludeTag, "excludeTag");
                                organisationContainer.Prev += GetUrlFromGivenString(organisationContainer.Prev, gender, "gender");
                                organisationContainer.Prev += GetUrlFromGivenString(organisationContainer.Prev, to, "to");
                                organisationContainer.Prev += GetUrlFromGivenString(organisationContainer.Prev, minAge?.ToString(), "minAge");
                                organisationContainer.Prev += GetUrlFromGivenString(organisationContainer.Prev, maxAge?.ToString(), "maxAge");
                                #endregion
                            }
                            #endregion
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertErrorLogs("[DataLaundryApi] FeedHelper", "GetOrganisations", ex.Message, ex.InnerException?.Message, ex.StackTrace);
            }
            return organisationContainer;
        }
        public static OrganisationDetail GetOrganisationDetailsById(long? organisationId = 0)
        {
            OrganisationDetail detail = new OrganisationDetail();
            try
            {
                var lstSqlParameter = new List<SqlParameter>();
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@organisationId", SqlDbType = SqlDbType.BigInt, Value = (object)organisationId ?? DBNull.Value });

                var ds = DBProvider.GetDataSet("GetOrganisationById", CommandType.StoredProcedure, ref lstSqlParameter);

                if (ds.Tables[0].Rows.Count > 0)
                {
                    var row = ds.Tables[0].Rows[0];
                    detail = GetOrganisationFromDataRow(row, Convert.ToInt64(row["Id"]));
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertErrorLogs("[DataLaundryApi] FeedHelper", "GetOrganisationDetailsById", ex.Message, ex.InnerException?.Message, ex.StackTrace);
            }
            return detail;
        }
        #endregion

        public static LocationContainer GetLocations(double? latitude, double? longitude, double? radius,
                                            string activity = null, string disabilitySupport = null, string gender = null,
                                            long? minTime = null, long? maxTime = null, long? minAge = null, long? maxAge = null,
                                            string weekdays = null, long? page = 1, long? limit = 50, string from = null,
                                            string to = null, string source = null, string tag = null, string excludeTag = null,
                                            double? minCost = null, double? maxCost = null)
        {
            LocationContainer locationContainer = new LocationContainer();
            locationContainer.Count = 0;
            locationContainer.Limit = limit;
            locationContainer.Page = page;
            try
            {
                var lstSqlParameter = new List<SqlParameter>();
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@lat", SqlDbType = SqlDbType.Decimal, Value = (object)latitude ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@long", SqlDbType = SqlDbType.Decimal, Value = (object)longitude ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@radius", SqlDbType = SqlDbType.Decimal, Value = (object)radius ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@source", SqlDbType = SqlDbType.NVarChar, Value = (object)source ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@tag", SqlDbType = SqlDbType.NVarChar, Value = (object)tag ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@excludeTag", SqlDbType = SqlDbType.NVarChar, Value = (object)excludeTag ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@activity", SqlDbType = SqlDbType.NVarChar, Value = (object)activity ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@disabilitySupport", SqlDbType = SqlDbType.NVarChar, Value = (object)disabilitySupport ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@weekdays", SqlDbType = SqlDbType.NVarChar, Value = (object)weekdays ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@minCost", SqlDbType = SqlDbType.Decimal, Value = (object)minCost ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@maxCost", SqlDbType = SqlDbType.Decimal, Value = (object)maxCost ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@gender", SqlDbType = SqlDbType.NVarChar, Value = (object)gender ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@minTime", SqlDbType = SqlDbType.BigInt, Value = (object)minTime ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@maxTime", SqlDbType = SqlDbType.BigInt, Value = (object)maxTime ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@minAge", SqlDbType = SqlDbType.BigInt, Value = (object)minAge ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@maxAge", SqlDbType = SqlDbType.BigInt, Value = (object)maxAge ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@page", SqlDbType = SqlDbType.BigInt, Value = (object)page ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@limit", SqlDbType = SqlDbType.BigInt, Value = (object)limit ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@from", SqlDbType = SqlDbType.NVarChar, Value = (object)from ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@to", SqlDbType = SqlDbType.NVarChar, Value = (object)to ?? DBNull.Value });

                //var ds = DBProvider.GetDataSet("GetLocations", CommandType.StoredProcedure, ref lstSqlParameter);
                var ds = DBProvider.GetDataSet("GetLocations_v1", CommandType.StoredProcedure, ref lstSqlParameter);

                if (ds != null && ds.Tables.Count > 0)
                {
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        DataTable dt_Events = ds.Tables[0];
                        int rowCount = dt_Events.Rows.Count;

                        locationContainer.Count = Convert.ToInt32(ds.Tables[1].Rows.Count > 0 ? ds.Tables[1].Rows[0]["TotalCount"].ToString() : "0");

                        for (int i = 0; i < rowCount; i++)
                        {
                            #region Location Detail
                            var row = dt_Events.Rows[i];

                            var location = new LocationDetail();
                            if (row["Lat"] != DBNull.Value)
                                location.Coordinates.Lat = Convert.ToDecimal(row["Lat"]);
                            if (row["Long"] != DBNull.Value)
                                location.Coordinates.Lng = Convert.ToDecimal(row["Long"]);
                            if (row["Distance"] != DBNull.Value)
                                location.Distance = Convert.ToInt32(row["Distance"]);
                            if (row["TotalEvents"] != DBNull.Value)
                            {
                                location.Sessions.Count = Convert.ToInt32(row["TotalEvents"]);

                                if (string.IsNullOrEmpty(location.Sessions.Href))
                                    location.Sessions.Href = Settings.GetAppSetting("sessionAPI");

                                location.Sessions.Href += location.Sessions.Href.Contains("?") ? "&" : "?";

                                #region required params
                                location.Sessions.Href += "lat=" + latitude;
                                location.Sessions.Href += "&";
                                location.Sessions.Href += "lng=" + longitude;
                                location.Sessions.Href += "&";
                                location.Sessions.Href += "radius=" + radius;
                                location.Sessions.Href += "&";
                                location.Sessions.Href += "page=" + page;
                                location.Sessions.Href += "&";
                                location.Sessions.Href += "limit=" + limit;
                                #endregion

                                #region Optional Params
                                location.Sessions.Href += GetUrlFromGivenString(location.Sessions.Href, source, "source");
                                location.Sessions.Href += GetUrlFromGivenString(location.Sessions.Href, activity, "activity");
                                location.Sessions.Href += GetUrlFromGivenString(location.Sessions.Href, disabilitySupport, "disabilitySupport");
                                location.Sessions.Href += GetUrlFromGivenString(location.Sessions.Href, weekdays, "weekdays");
                                location.Sessions.Href += GetUrlFromGivenString(location.Sessions.Href, tag, "tag");
                                location.Sessions.Href += GetUrlFromGivenString(location.Sessions.Href, excludeTag, "excludeTag");
                                location.Sessions.Href += GetUrlFromGivenString(location.Sessions.Href, gender, "gender");
                                location.Sessions.Href += GetUrlFromGivenString(location.Sessions.Href, from, "from");
                                location.Sessions.Href += GetUrlFromGivenString(location.Sessions.Href, to, "to");
                                location.Sessions.Href += GetUrlFromGivenString(location.Sessions.Href, minTime?.ToString(), "minTime");
                                location.Sessions.Href += GetUrlFromGivenString(location.Sessions.Href, maxTime?.ToString(), "maxTime");
                                location.Sessions.Href += GetUrlFromGivenString(location.Sessions.Href, minAge?.ToString(), "minAge");
                                location.Sessions.Href += GetUrlFromGivenString(location.Sessions.Href, maxAge?.ToString(), "maxAge");
                                location.Sessions.Href += GetUrlFromGivenString(location.Sessions.Href, minCost?.ToString(), "minCost");
                                location.Sessions.Href += GetUrlFromGivenString(location.Sessions.Href, maxCost?.ToString(), "maxCost");
                                #endregion
                            }
                            if (row["TotalOrganisations"] != DBNull.Value)
                            {
                                location.Organisations.Count = Convert.ToInt32(row["TotalOrganisations"]);

                                if (string.IsNullOrEmpty(location.Organisations.Href))
                                    location.Organisations.Href = Settings.GetAppSetting("organisationAPI");

                                location.Organisations.Href += location.Organisations.Href.Contains("?") ? "&" : "?";

                                #region required params
                                location.Organisations.Href += "lat=" + latitude;
                                location.Organisations.Href += "&";
                                location.Organisations.Href += "lng=" + longitude;
                                location.Organisations.Href += "&";
                                location.Organisations.Href += "radius=" + radius;
                                location.Organisations.Href += "&";
                                location.Organisations.Href += "page=" + page;
                                location.Organisations.Href += "&";
                                location.Organisations.Href += "limit=" + limit;
                                #endregion

                                #region Optional Params
                                location.Organisations.Href += GetUrlFromGivenString(location.Organisations.Href, source, "source");
                                location.Organisations.Href += GetUrlFromGivenString(location.Organisations.Href, activity, "activity");
                                location.Organisations.Href += GetUrlFromGivenString(location.Organisations.Href, disabilitySupport, "disabilitySupport");
                                location.Organisations.Href += GetUrlFromGivenString(location.Organisations.Href, weekdays, "weekdays");
                                location.Organisations.Href += GetUrlFromGivenString(location.Organisations.Href, tag, "tag");
                                location.Organisations.Href += GetUrlFromGivenString(location.Organisations.Href, excludeTag, "excludeTag");
                                location.Organisations.Href += GetUrlFromGivenString(location.Organisations.Href, gender, "gender");
                                location.Organisations.Href += GetUrlFromGivenString(location.Organisations.Href, from, "from");
                                location.Organisations.Href += GetUrlFromGivenString(location.Organisations.Href, to, "to");
                                location.Organisations.Href += GetUrlFromGivenString(location.Organisations.Href, minTime?.ToString(), "minTime");
                                location.Organisations.Href += GetUrlFromGivenString(location.Organisations.Href, maxTime?.ToString(), "maxTime");
                                location.Organisations.Href += GetUrlFromGivenString(location.Organisations.Href, minAge?.ToString(), "minAge");
                                location.Organisations.Href += GetUrlFromGivenString(location.Organisations.Href, maxAge?.ToString(), "maxAge");
                                location.Organisations.Href += GetUrlFromGivenString(location.Organisations.Href, minCost?.ToString(), "minCost");
                                location.Organisations.Href += GetUrlFromGivenString(location.Organisations.Href, maxCost?.ToString(), "maxCost");
                                #endregion
                            }


                            #endregion

                            locationContainer.Items.Add(location);

                            #region build next page url in the end based on 
                            if ((i + 1) == rowCount && locationContainer.Count > limit && rowCount >= limit && rowCount <= locationContainer.Count)
                            {
                                if (string.IsNullOrEmpty(locationContainer.Next))
                                    locationContainer.Next = Settings.GetAppSetting("locationAPI");

                                locationContainer.Next += locationContainer.Next.Contains("?") ? "&" : "?";

                                #region required params
                                locationContainer.Next += "lat=" + latitude;
                                locationContainer.Next += "&";
                                locationContainer.Next += "lng=" + longitude;
                                locationContainer.Next += "&";
                                locationContainer.Next += "radius=" + radius;
                                locationContainer.Next += "&";
                                locationContainer.Next += "page=" + (page + 1);
                                locationContainer.Next += "&";
                                locationContainer.Next += "limit=" + limit;
                                #endregion

                                #region Optional Params
                                locationContainer.Next += GetUrlFromGivenString(locationContainer.Next, source, "source");
                                locationContainer.Next += GetUrlFromGivenString(locationContainer.Next, activity, "activity");
                                locationContainer.Next += GetUrlFromGivenString(locationContainer.Next, disabilitySupport, "disabilitySupport");
                                locationContainer.Next += GetUrlFromGivenString(locationContainer.Next, weekdays, "weekdays");
                                locationContainer.Next += GetUrlFromGivenString(locationContainer.Next, tag, "tag");
                                locationContainer.Next += GetUrlFromGivenString(locationContainer.Next, excludeTag, "excludeTag");
                                locationContainer.Next += GetUrlFromGivenString(locationContainer.Next, gender, "gender");
                                locationContainer.Next += GetUrlFromGivenString(locationContainer.Next, from, "from");
                                locationContainer.Next += GetUrlFromGivenString(locationContainer.Next, to, "to");
                                locationContainer.Next += GetUrlFromGivenString(locationContainer.Next, minTime?.ToString(), "minTime");
                                locationContainer.Next += GetUrlFromGivenString(locationContainer.Next, maxTime?.ToString(), "maxTime");
                                locationContainer.Next += GetUrlFromGivenString(locationContainer.Next, minAge?.ToString(), "minAge");
                                locationContainer.Next += GetUrlFromGivenString(locationContainer.Next, maxAge?.ToString(), "maxAge");
                                locationContainer.Next += GetUrlFromGivenString(locationContainer.Next, minCost?.ToString(), "minCost");
                                locationContainer.Next += GetUrlFromGivenString(locationContainer.Next, maxCost?.ToString(), "maxCost");
                                #endregion
                            }
                            #endregion

                            #region build prev page url in the end based on 
                            if ((i + 1) == rowCount && locationContainer.Count > limit && page > 1)
                            {
                                if (string.IsNullOrEmpty(locationContainer.Prev))
                                    locationContainer.Prev = Settings.GetAppSetting("locationAPI");

                                locationContainer.Prev += locationContainer.Prev.Contains("?") ? "&" : "?";

                                #region required params
                                locationContainer.Prev += "lat=" + latitude;
                                locationContainer.Prev += "&";
                                locationContainer.Prev += "lng=" + longitude;
                                locationContainer.Prev += "&";
                                locationContainer.Prev += "radius=" + radius;
                                locationContainer.Prev += "&";
                                locationContainer.Prev += "page=" + (page - 1);
                                locationContainer.Prev += "&";
                                locationContainer.Prev += "limit=" + limit;
                                #endregion

                                #region Optional Params
                                locationContainer.Prev += GetUrlFromGivenString(locationContainer.Prev, source, "source");
                                locationContainer.Prev += GetUrlFromGivenString(locationContainer.Prev, activity, "activity");
                                locationContainer.Prev += GetUrlFromGivenString(locationContainer.Prev, disabilitySupport, "disabilitySupport");
                                locationContainer.Prev += GetUrlFromGivenString(locationContainer.Prev, weekdays, "weekdays");
                                locationContainer.Prev += GetUrlFromGivenString(locationContainer.Prev, tag, "tag");
                                locationContainer.Prev += GetUrlFromGivenString(locationContainer.Prev, excludeTag, "excludeTag");
                                locationContainer.Prev += GetUrlFromGivenString(locationContainer.Prev, gender, "gender");
                                locationContainer.Prev += GetUrlFromGivenString(locationContainer.Prev, from, "from");
                                locationContainer.Prev += GetUrlFromGivenString(locationContainer.Prev, to, "to");
                                locationContainer.Prev += GetUrlFromGivenString(locationContainer.Prev, minTime?.ToString(), "minTime");
                                locationContainer.Prev += GetUrlFromGivenString(locationContainer.Prev, maxTime?.ToString(), "maxTime");
                                locationContainer.Prev += GetUrlFromGivenString(locationContainer.Prev, minAge?.ToString(), "minAge");
                                locationContainer.Prev += GetUrlFromGivenString(locationContainer.Prev, maxAge?.ToString(), "maxAge");
                                locationContainer.Prev += GetUrlFromGivenString(locationContainer.Prev, minCost?.ToString(), "minCost");
                                locationContainer.Prev += GetUrlFromGivenString(locationContainer.Prev, maxCost?.ToString(), "maxCost");
                                #endregion
                            }
                            #endregion
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertErrorLogs("[DataLaundryApi] FeedHelper", "GetLocations", ex.Message, ex.InnerException?.Message, ex.StackTrace);
            }
            return locationContainer;
        }
        #endregion

        public static string GetUrlFromGivenString(string baseUrl, string param, string requestParamName)
        {
            string resultedUrl = null;
            if (!string.IsNullOrEmpty(param))
            {
                if (param.Split(',').Length > 0)
                {
                    var result = param.Split(',');
                    foreach (var data in result)
                    {
                        resultedUrl += "&";
                        resultedUrl += requestParamName + "=" + data;
                    }
                }
                else
                {
                    resultedUrl += requestParamName + "=" + param;
                    resultedUrl += "&";
                }
            }
            return resultedUrl;
        }
        public static bool IsValidJson(string stringValue)
        {
            if (string.IsNullOrWhiteSpace(stringValue))
                return false;

            var value = stringValue.Trim();

            if ((value.StartsWith("{") && value.EndsWith("}")) || //For object
                (value.StartsWith("[") && value.EndsWith("]"))) //For array
            {
                try
                {
                    var obj = JToken.Parse(value);
                    return true;
                }
                catch (JsonReaderException)
                {
                    return false;
                }
            }
            return false;
        }
    }
}
