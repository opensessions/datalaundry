using DataLaundryScheduler.DTO;
using DataLaundryScheduler.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace DataLaundryScheduler.Helpers
{
    class FeedHelper
    {
        public static long? Insert(List<FeedMapping> lstFeedMapping, string feedId = null, string feedStatus = null, object modifiedDate = null, string prefix = "", long? superEventId = null)
        {
            long? eventId = null;

            if (lstFeedMapping != null && lstFeedMapping.Count > 0)
            {
                var feedProvider = lstFeedMapping[0].FeedProvider;
                //Event fields

                if (superEventId == null)
                {
                    //insert superevent first if available
                    var superEventField = GetFeedMapping(lstFeedMapping, prefix + "superEvent");
                    if (superEventField != null)
                    {
                        if (superEventField.FeedDataType == "object")
                        {
                            int? superEventMappedFieldCount = superEventField?.Childrens?.Where(x => !string.IsNullOrEmpty(x.ActualFeedKeyPath))?.Count();

                            if (superEventMappedFieldCount != null && superEventMappedFieldCount > 0)
                            {
                                superEventId = Insert(superEventField?.Childrens, prefix: "superEvent_");
                            }
                        }
                    }
                }

                object name = GetFeedKeyValue(lstFeedMapping, prefix + "name");
                object description = GetFeedKeyValue(lstFeedMapping, prefix + "description");
                object startDate = GetFeedKeyValue(lstFeedMapping, prefix + "startDate");
                object endDate = GetFeedKeyValue(lstFeedMapping, prefix + "endDate");
                object duration = GetFeedKeyValue(lstFeedMapping, prefix + "duration");
                object maximumAttendeeCapacity = GetFeedKeyValue(lstFeedMapping, prefix + "maximumAttendeeCapacity");
                object remainingAttendeeCapacity = GetFeedKeyValue(lstFeedMapping, prefix + "remainingAttendeeCapacity");
                object eventStatus = GetFeedKeyValue(lstFeedMapping, prefix + "eventStatus");
                object ageRange = GetFeedKeyValue(lstFeedMapping, prefix + "ageRange");
                object genderRestriction = GetFeedKeyValue(lstFeedMapping, prefix + "genderRestriction");
                object attendeeInstructions = GetFeedKeyValue(lstFeedMapping, prefix + "attendeeInstructions");
                object accessibilityInformation = GetFeedKeyValue(lstFeedMapping, prefix + "accessibilityInformation");
                object isCoached = GetFeedKeyValue(lstFeedMapping, prefix + "isCoached");
                object meetingPoint = GetFeedKeyValue(lstFeedMapping, prefix + "meetingPoint");
                object identifier = GetFeedKeyValue(lstFeedMapping, prefix + "identifier");
                object url = GetFeedKeyValue(lstFeedMapping, prefix + "url");

                var imageField = GetFeedMapping(lstFeedMapping, prefix + "image");
                object image = null;
                object imageThumbnail = null;

                if (imageField != null)
                {
                    if (imageField.FeedDataType == "object")
                    {
                        image = GetFeedKeyValue(imageField.Childrens, prefix + "image_url");
                        imageThumbnail = GetFeedKeyValue(imageField.Childrens, prefix + "image_thumbnail");
                    }
                    else if (imageField.FeedDataType == "string")
                    {
                        image = imageField.FeedKeyValue;
                    }
                }

                var accessibilitySupportField = GetFeedMapping(lstFeedMapping, prefix + "accessibilitySupport");
                object accessibilitySupport = null;
                //if (accessibilitySupportField != null && string.IsNullOrEmpty(accessibilitySupportField.FeedDataType) && accessibilitySupportField.FeedDataType != "null")	
                if (accessibilitySupportField != null)
                {
                    if (accessibilitySupportField.FeedDataType == "array")
                    {
                        var lstAccessibilitySupport = JsonConvert.DeserializeObject<JArray>(Convert.ToString(accessibilitySupportField.FeedKeyValue)).ToList();

                        if (lstAccessibilitySupport != null && lstAccessibilitySupport.Count > 0)
                        {
                            var lstAccessibilitySupportFinal = lstAccessibilitySupport.Select(x => x.Value<string>()).ToList();

                            if (lstAccessibilitySupportFinal != null && lstAccessibilitySupportFinal.Count > 0)
                                accessibilitySupport = string.Join(",", lstAccessibilitySupportFinal);
                        }
                    }
                    else
                    {
                        accessibilitySupport = accessibilitySupportField.FeedKeyValue;
                    }
                }

                var levelField = GetFeedMapping(lstFeedMapping, prefix + "level");
                object level = null;

                if (levelField != null && string.IsNullOrEmpty(levelField.FeedDataType) && levelField.FeedDataType != "null")
                {
                    if (levelField.FeedDataType == "array")
                    {
                        var lstLevel = JsonConvert.DeserializeObject<JArray>(Convert.ToString(levelField.FeedKeyValue))?.ToList();

                        if (lstLevel != null && lstLevel.Count > 0)
                        {
                            var lstLevelFinal = lstLevel.Select(x => x.Value<string>()).ToList();

                            if (lstLevelFinal != null && lstLevelFinal.Count > 0)
                                level = string.Join(",", lstLevelFinal);
                        }
                    }
                    else
                    {
                        level = levelField.FeedKeyValue;
                    }
                }

                var categoryField = GetFeedMapping(lstFeedMapping, prefix + "category");
                object category = null;

                if (categoryField != null)
                {
                    if (categoryField.FeedDataType == "array")
                    {
                        var lstCategory = JsonConvert.DeserializeObject<JArray>(Convert.ToString(categoryField.FeedKeyValue))?.ToList();

                        if (lstCategory != null && lstCategory.Count > 0)
                        {
                            var lstCategoryFinal = lstCategory.Select(x => x.Value<string>()).ToList();

                            if (lstCategoryFinal != null && lstCategoryFinal.Count > 0)
                                category = string.Join(",", lstCategoryFinal);
                        }
                    }
                    else
                    {
                        category = levelField.FeedKeyValue;
                    }
                }

                var lstSqlParameter = new List<SqlParameter>();

                //basic params
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@FeedProviderId", SqlDbType = SqlDbType.NVarChar, Value = feedProvider.Id });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@FeedId", SqlDbType = SqlDbType.NVarChar, Value = (object)feedId ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@State", SqlDbType = SqlDbType.NVarChar, Value = (object)feedStatus ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@ModifiedDate", SqlDbType = SqlDbType.DateTime, Value = modifiedDate ?? DBNull.Value });

                //feed data params
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Name", SqlDbType = SqlDbType.NVarChar, Value = name ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Description", SqlDbType = SqlDbType.NVarChar, Value = description ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Image", SqlDbType = SqlDbType.NVarChar, Value = image ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@ImageThumbnail", SqlDbType = SqlDbType.NVarChar, Value = imageThumbnail ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@StartDate", SqlDbType = SqlDbType.DateTime2, Value = startDate ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@EndDate", SqlDbType = SqlDbType.DateTime2, Value = endDate ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Duration", SqlDbType = SqlDbType.NVarChar, Value = duration ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@MaximumAttendeeCapacity", SqlDbType = SqlDbType.Int, Value = maximumAttendeeCapacity ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@RemainingAttendeeCapacity", SqlDbType = SqlDbType.Int, Value = remainingAttendeeCapacity ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@EventStatus", SqlDbType = SqlDbType.NVarChar, Value = eventStatus ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@SuperEventId", SqlDbType = SqlDbType.BigInt, Value = (object)superEventId ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Category", SqlDbType = SqlDbType.NVarChar, Value = category ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@AgeRange", SqlDbType = SqlDbType.NVarChar, Value = ageRange ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@GenderRestriction", SqlDbType = SqlDbType.NVarChar, Value = genderRestriction ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@AttendeeInstructions", SqlDbType = SqlDbType.NVarChar, Value = attendeeInstructions ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@AccessibilitySupport", SqlDbType = SqlDbType.NVarChar, Value = accessibilitySupport ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@AccessibilityInformation", SqlDbType = SqlDbType.NVarChar, Value = accessibilityInformation ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@IsCoached", SqlDbType = SqlDbType.Bit, Value = isCoached ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Level", SqlDbType = SqlDbType.NVarChar, Value = level ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@MeetingPoint", SqlDbType = SqlDbType.NVarChar, Value = meetingPoint ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Identifier", SqlDbType = SqlDbType.NVarChar, Value = identifier ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Url", SqlDbType = SqlDbType.NVarChar, Value = url ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@EventId", SqlDbType = SqlDbType.BigInt, Direction = ParameterDirection.Output });

                int rowsAffected = DBProvider.ExecuteNonQuery("Event_Insert", CommandType.StoredProcedure, ref lstSqlParameter);

                var eventIdParam = lstSqlParameter.Where(x => x.ParameterName == "@EventId").FirstOrDefault().Value;

                eventId = eventIdParam == DBNull.Value ? null : (long?)eventIdParam;

                if (rowsAffected > 0 && eventId != null)
                {
                    bool status = DeleteOldEventData((long)eventId);

                    //organization
                    var organizerField = GetFeedMapping(lstFeedMapping, prefix + "organizer");
                    long? organizationId = InsertOrganization((long)eventId, organizerField?.Childrens, prefix);

                    //contributor
                    var contributorField = GetFeedMapping(lstFeedMapping, prefix + "contributor");
                    long? contributorId = InsertPerson((long)eventId, contributorField?.Childrens, prefix: prefix);

                    //leader
                    var leaderField = GetFeedMapping(lstFeedMapping, prefix + "leader");
                    long? leaderId = InsertPerson((long)eventId, leaderField?.Childrens, isLeader: true, prefix: prefix);

                    //location
                    var placeField = GetFeedMapping(lstFeedMapping, prefix + "location");
                    long? placeId = InsertPlace((long)eventId, placeField?.Childrens, prefix + "location");

                    //eventSchedule
                    var eventScheduleField = GetFeedMapping(lstFeedMapping, prefix + "schedule");
                    long? eventScheduleId = InsertEventSchedule((long)eventId, eventScheduleField?.Childrens, prefix);

                    #region Programme - can be string or object
                    var programmeField = GetFeedMapping(lstFeedMapping, prefix + "programme");
                    long? programmeId = null;

                    //check if there is only one programme of string or object type
                    if (programmeField != null)
                    {
                        if (programmeField.FeedDataType == "string")
                        {
                            var lstFeedMappingProgramme = new List<FeedMapping>();

                            var feedMappingProgramme = new FeedMapping()
                            {
                                ColumnName = prefix + "programme",
                                FeedKeyValue = programmeField.FeedKeyValue
                            };

                            lstFeedMappingProgramme.Add(feedMappingProgramme);

                            programmeId = InsertProgramme((long)eventId, lstFeedMappingProgramme, prefix);
                        }
                        else if (programmeField.FeedDataType == "object")
                        {
                            programmeId = InsertProgramme((long)eventId, programmeField?.Childrens, prefix);
                        }
                    }
                    #endregion Programme - can be string or object

                    #region PhysicalActivity - can be string, object or array
                    var activityField = GetFeedMapping(lstFeedMapping, prefix + "activity");

                    if (activityField != null)
                    {
                        //check if there is only one activity of string or object type
                        if (activityField.FeedDataType == "string")
                        {
                            var lstFeedMappingActivity = new List<FeedMapping>();

                            var feedMappingActivity = new FeedMapping()
                            {
                                ColumnName = prefix + "activity_prefLabel",
                                FeedKeyValue = activityField.FeedKeyValue
                            };

                            lstFeedMappingActivity.Add(feedMappingActivity);

                            InsertPhysicalActivity((long)eventId, lstFeedMappingActivity, prefix + "activity");
                        }
                        else if (activityField.FeedDataType == "object")
                        {
                            InsertPhysicalActivity((long)eventId, activityField.Childrens, prefix + "activity");
                        }
                        else if (activityField.FeedDataType == "array")
                        {
                            int mappedChildPropertiesCount = activityField.Childrens.Where(x => !string.IsNullOrEmpty(x.ActualFeedKeyPath)).Count();

                            //if no child properties found then it is array of strings
                            if (mappedChildPropertiesCount == 0)
                            {
                                var lstActivities = JsonConvert.DeserializeObject<JArray>(Convert.ToString(activityField.FeedKeyValue))?.ToList();

                                //insert multiple physical activities
                                foreach (var activity in lstActivities)
                                {
                                    string strVal = activity.Value<string>();
                                    var lstFeedMappingActivity = new List<FeedMapping>();

                                    var feedMappingActivity = new FeedMapping()
                                    {
                                        ColumnName = prefix + "activity_prefLabel",
                                        FeedKeyValue = strVal
                                    };

                                    lstFeedMappingActivity.Add(feedMappingActivity);

                                    InsertPhysicalActivity((long)eventId, lstFeedMappingActivity, prefix + "activity");
                                }
                            }
                            else
                            {
                                //otherwise at least one of the child properties are mapped and hence it is array of objects
                                if (activityField.ChildrenRecords != null && activityField.ChildrenRecords.Count > 0)
                                {
                                    //insert multiple physical activities
                                    foreach (var record in activityField.ChildrenRecords)
                                    {
                                        InsertPhysicalActivity((long)eventId, record, prefix + "activity");
                                    }
                                }
                            }
                        }
                    }

                    #endregion PhysicalActivity - can be string, object or array

                    #region SubEvent - can be object or array
                    var subEventField = GetFeedMapping(lstFeedMapping, prefix + "subEvent");

                    if (subEventField != null)
                    {
                        if (subEventField.FeedDataType == "object")
                        {
                            if (subEventField?.Childrens != null && subEventField?.Childrens?.Count > 0)
                            {
                                Insert(subEventField?.Childrens, prefix: "subEvent_", superEventId: eventId);
                            }
                        }
                        else if (subEventField.FeedDataType == "array")
                        {
                            if (subEventField.ChildrenRecords != null && subEventField.ChildrenRecords.Count > 0)
                            {
                                //insert multiple subevents
                                foreach (var record in subEventField.ChildrenRecords)
                                {
                                    int? subEventMappedFieldCount = record?.Where(x => !string.IsNullOrEmpty(x.ActualFeedKeyPath))?.Count();

                                    if (subEventMappedFieldCount != null && subEventMappedFieldCount > 0)
                                    {
                                        Insert(record, prefix: "subEvent_", superEventId: eventId);
                                    }
                                }
                            }
                        }
                    }

                    #endregion SubEvent - can be object or array
                }
            }

            return eventId;
        }

        public static long? Insert_v1(List<FeedMapping> lstFeedMapping, string feedId = null, string feedStatus = null, object modifiedDate = null, string prefix = "", long? superEventId = null, bool isRootEvent = false, long schedulerLogId = 0)
        {
            long? eventId = null;

            try
            {
                if (lstFeedMapping != null && lstFeedMapping.Count > 0)
                {
                    var feedProvider = lstFeedMapping[0].FeedProvider;
                    //Event fields

                    #region Super Event
                    if (superEventId == null)
                    {
                        //insert superevent first if available
                        var superEventField = GetFeedMapping(lstFeedMapping, prefix + "superEvent");
                        if (superEventField != null)
                        {
                            if (superEventField.FeedDataType == "object")
                            {
                                int? superEventMappedFieldCount = superEventField?.Childrens?.Where(x => !string.IsNullOrEmpty(x.ActualFeedKeyPath))?.Count();

                                if (superEventMappedFieldCount != null && superEventMappedFieldCount > 0)
                                {
                                    superEventId = Insert_v1(superEventField?.Childrens, prefix: "superEvent_");
                                }
                            }
                        }
                    }
                    #endregion

                    #region Event
                    #region Event required Data
                    object name = GetFeedKeyValue(lstFeedMapping, prefix + "name");
                    object description = GetFeedKeyValue(lstFeedMapping, prefix + "description");
                    object startDate = GetFeedKeyValue(lstFeedMapping, prefix + "startDate");
                    object endDate = GetFeedKeyValue(lstFeedMapping, prefix + "endDate");
                    object duration = GetFeedKeyValue(lstFeedMapping, prefix + "duration");
                    object maximumAttendeeCapacity = GetFeedKeyValue(lstFeedMapping, prefix + "maximumAttendeeCapacity");
                    object remainingAttendeeCapacity = GetFeedKeyValue(lstFeedMapping, prefix + "remainingAttendeeCapacity");
                    object eventStatus = GetFeedKeyValue(lstFeedMapping, prefix + "eventStatus");
                    object ageRange = GetFeedKeyValue(lstFeedMapping, prefix + "ageRange");
                    object genderRestriction = GetFeedKeyValue(lstFeedMapping, prefix + "genderRestriction");
                    object attendeeInstructions = GetFeedKeyValue(lstFeedMapping, prefix + "attendeeInstructions");
                    object accessibilityInformation = GetFeedKeyValue(lstFeedMapping, prefix + "accessibilityInformation");
                    object isCoached = GetFeedKeyValue(lstFeedMapping, prefix + "isCoached");
                    object meetingPoint = GetFeedKeyValue(lstFeedMapping, prefix + "meetingPoint");
                    object identifier = GetFeedKeyValue(lstFeedMapping, prefix + "identifier");
                    object url = GetFeedKeyValue(lstFeedMapping, prefix + "url");

                    var imageField = GetFeedMapping(lstFeedMapping, prefix + "image");
                    object image = null;
                    object imageThumbnail = null;

                    if (imageField != null)
                    {
                        if (imageField.FeedDataType == "object")
                        {
                            image = GetFeedKeyValue(imageField.Childrens, prefix + "image_url");
                            imageThumbnail = GetFeedKeyValue(imageField.Childrens, prefix + "image_thumbnail");
                        }
                        else if (imageField.FeedDataType == "string")
                        {
                            image = imageField.FeedKeyValue;
                        }
                        else if (imageField.FeedDataType == "array")
                        {
                            int mappedChildPropertiesCount = imageField.Childrens.Where(x => !string.IsNullOrEmpty(x.ActualFeedKeyPath)).Count();

                            if (mappedChildPropertiesCount != 0)
                            {
                                if (imageField.ChildrenRecords != null && imageField.ChildrenRecords.Count > 0)
                                {
                                    foreach (var record in imageField.ChildrenRecords)
                                    {
                                        image = GetFeedKeyValue(record, prefix + "image_url");
                                        imageThumbnail = GetFeedKeyValue(record, prefix + "image_thumbnail");
                                    }
                                }
                            }
                        }
                    }

                    var accessibilitySupportField = GetFeedMapping(lstFeedMapping, prefix + "accessibilitySupport");
                    object accessibilitySupport = null;
                    if (accessibilitySupportField != null)
                    {
                        if (accessibilitySupportField.FeedDataType == "array")
                        {
                            var lstAccessibilitySupport = JsonConvert.DeserializeObject<JArray>(Convert.ToString(accessibilitySupportField.FeedKeyValue)).ToList();

                            if (lstAccessibilitySupport != null && lstAccessibilitySupport.Count > 0)
                            {
                                List<string> lstAccessibilitySupportFinal = new List<string>();
                                foreach (var acceSupport in lstAccessibilitySupport)
                                {
                                    if (acceSupport.GetType() == typeof(JValue))
                                        lstAccessibilitySupportFinal.Add(acceSupport.Value<string>());
                                    else if (acceSupport.GetType() == typeof(JObject))
                                    {
                                        var as_PrefLabel = acceSupport?.SelectToken("$.prefLabel");
                                        if (as_PrefLabel != null)
                                            lstAccessibilitySupportFinal.Add(as_PrefLabel.Value<string>());
                                    }
                                }                                
                                if (lstAccessibilitySupportFinal != null && lstAccessibilitySupportFinal.Count > 0)
                                    accessibilitySupport = string.Join(",", lstAccessibilitySupportFinal);
                            }
                        }
                        else
                        {
                            accessibilitySupport = accessibilitySupportField.FeedKeyValue;
                        }
                    }
                    var levelField = GetFeedMapping(lstFeedMapping, prefix + "level");
                    object level = null;

                    if (levelField != null && string.IsNullOrEmpty(levelField.FeedDataType) && levelField.FeedDataType != "null")
                    {
                        if (levelField.FeedDataType == "array")
                        {
                            var lstLevel = JsonConvert.DeserializeObject<JArray>(Convert.ToString(levelField.FeedKeyValue))?.ToList();

                            if (lstLevel != null && lstLevel.Count > 0)
                            {
                                var lstLevelFinal = lstLevel.Select(x => x.Value<string>()).ToList();

                                if (lstLevelFinal != null && lstLevelFinal.Count > 0)
                                    level = string.Join(",", lstLevelFinal);
                            }
                        }
                        else
                        {
                            level = levelField.FeedKeyValue;
                        }
                    }

                    var categoryField = GetFeedMapping(lstFeedMapping, prefix + "category");
                    object category = null;

                    if (categoryField != null)
                    {
                        if (categoryField.FeedDataType == "array")
                        {
                            var lstCategory = JsonConvert.DeserializeObject<JArray>(Convert.ToString(categoryField.FeedKeyValue))?.ToList();

                            if (lstCategory != null && lstCategory.Count > 0)
                            {
                                var lstCategoryFinal = lstCategory.Select(x => x.Value<string>()).ToList();

                                if (lstCategoryFinal != null && lstCategoryFinal.Count > 0)
                                    category = string.Join(",", lstCategoryFinal);
                            }
                        }
                        else
                        {
                            category = levelField.FeedKeyValue;
                        }
                    }

                    var ageRangeField = GetFeedMapping(lstFeedMapping, prefix + "ageRange");
                    int? minAge = null, maxAge = null;

                    if (ageRange != null)
                    {
                        if (ageRangeField != null)
                        {
                            if (ageRangeField.FeedDataType == "object")
                            {
                                var objAgeRange = JsonConvert.DeserializeObject<JToken>(Convert.ToString(ageRangeField.FeedKeyValue));

                                if (objAgeRange != null)
                                {
                                    var minValue = objAgeRange?.SelectToken("$.minValue");
                                    var maxValue = objAgeRange?.SelectToken("$.maxValue");

                                    minAge = minValue?.Value<string>() != null ? Convert.ToInt16(minValue) : (int?)null;
                                    maxAge = maxValue?.Value<string>() != null ? Convert.ToInt16(maxValue) : (int?)null;
                                }
                            }
                            if (ageRangeField.FeedDataType == "string")
                            {
                                var objAgeRange = Convert.ToString(ageRangeField.FeedKeyValue)?.Split('-');
                                if (objAgeRange != null)
                                {
                                    minAge = !string.IsNullOrEmpty(objAgeRange[0]) ? Convert.ToInt16(objAgeRange[0]) : (int?)null;
                                    if (objAgeRange.Length > 1)
                                        maxAge = !string.IsNullOrEmpty(objAgeRange[1]) ? Convert.ToInt16(objAgeRange[1]) : (int?)null;
                                }
                            }
                        }
                    }

                    #endregion

                    #region Event SQL Parameters

                    #region Remove [] and double quate
                    if (name != null)
                    {
                        name = name?.ToString().Replace("[", string.Empty).Replace("]", string.Empty);
                        name = name?.ToString().Replace("\"", string.Empty).Replace("\"", string.Empty);
                    }
                    if (description != null)
                    {
                        description = description?.ToString().Replace("[", "").Replace("]", string.Empty);
                        description = description?.ToString().Replace("\"", string.Empty).Replace("\"", string.Empty);
                    }
                    #endregion
                    var lstSqlParameter = new List<SqlParameter>();

                    //basic params
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@FeedProviderId", SqlDbType = SqlDbType.NVarChar, Value = feedProvider.Id });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@FeedId", SqlDbType = SqlDbType.NVarChar, Value = (object)feedId ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@State", SqlDbType = SqlDbType.NVarChar, Value = (object)feedStatus ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@ModifiedDate", SqlDbType = SqlDbType.DateTime, Value = modifiedDate ?? DBNull.Value });

                    //feed data params
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Name", SqlDbType = SqlDbType.NVarChar, Value = name ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Description", SqlDbType = SqlDbType.NVarChar, Value = description ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Image", SqlDbType = SqlDbType.NVarChar, Value = image ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@ImageThumbnail", SqlDbType = SqlDbType.NVarChar, Value = imageThumbnail ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@StartDate", SqlDbType = SqlDbType.DateTime2, Value = startDate ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@EndDate", SqlDbType = SqlDbType.DateTime2, Value = endDate ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Duration", SqlDbType = SqlDbType.NVarChar, Value = duration ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@MaximumAttendeeCapacity", SqlDbType = SqlDbType.NVarChar, Value = maximumAttendeeCapacity ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@RemainingAttendeeCapacity", SqlDbType = SqlDbType.NVarChar, Value = remainingAttendeeCapacity ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@EventStatus", SqlDbType = SqlDbType.NVarChar, Value = eventStatus ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@SuperEventId", SqlDbType = SqlDbType.BigInt, Value = (object)superEventId ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Category", SqlDbType = SqlDbType.NVarChar, Value = category ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@AgeRange", SqlDbType = SqlDbType.NVarChar, Value = ageRange ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@GenderRestriction", SqlDbType = SqlDbType.NVarChar, Value = genderRestriction ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@AttendeeInstructions", SqlDbType = SqlDbType.NVarChar, Value = attendeeInstructions ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@AccessibilitySupport", SqlDbType = SqlDbType.NVarChar, Value = accessibilitySupport ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@AccessibilityInformation", SqlDbType = SqlDbType.NVarChar, Value = accessibilityInformation ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@IsCoached", SqlDbType = SqlDbType.Bit, Value = isCoached ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Level", SqlDbType = SqlDbType.NVarChar, Value = level ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@MeetingPoint", SqlDbType = SqlDbType.NVarChar, Value = meetingPoint ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Identifier", SqlDbType = SqlDbType.NVarChar, Value = identifier ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Url", SqlDbType = SqlDbType.NVarChar, Value = url ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@MinAge", SqlDbType = SqlDbType.Int, Value = minAge });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@MaxAge", SqlDbType = SqlDbType.Int, Value = maxAge });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@EventId", SqlDbType = SqlDbType.BigInt, Direction = ParameterDirection.Output });
                    #endregion

                    int rowsAffected = DBProvider.ExecuteNonQuery("Event_Insert", CommandType.StoredProcedure, ref lstSqlParameter);

                    var eventIdParam = lstSqlParameter.Where(x => x.ParameterName == "@EventId").FirstOrDefault().Value;

                    eventId = eventIdParam == DBNull.Value ? null : (long?)eventIdParam;

                    if (rowsAffected > 0 && eventId != null)
                    {
                        bool status = DeleteOldEventData((long)eventId);

                        //organization
                        var organizerField = GetFeedMapping(lstFeedMapping, prefix + "organizer");
                        long? organizationId = InsertOrganization((long)eventId, organizerField?.Childrens, prefix, feedProviderId: feedProvider.Id);

                        //contributor
                        var contributorField = GetFeedMapping(lstFeedMapping, prefix + "contributor");
                        long? contributorId = InsertPerson((long)eventId, contributorField?.Childrens, prefix: prefix, feedProviderId: feedProvider.Id);

                        //location                        
                        var placeField = GetFeedMapping(lstFeedMapping, prefix + "location");
                        if (placeField.FeedDataType == "string")
                        {
                            var address = placeField?.Childrens.FirstOrDefault(x => x.ColumnName == prefix + "location" + "_address");
                            address.FeedDataType = "string";
                            address.FeedKeyValue = placeField.FeedKeyValue;
                        }
                        long? placeId = InsertPlace((long)eventId, placeField?.Childrens, prefix + "location", feedProviderId: feedProvider.Id);
                        
                        #region Leader - can be object or array
                        var leaderField = GetFeedMapping(lstFeedMapping, prefix + "leader");

                        if (leaderField != null)
                        {
                            if (leaderField.FeedDataType == "object")
                                InsertPerson((long)eventId, leaderField?.Childrens, isLeader: true, prefix: prefix, feedProviderId: feedProvider.Id);
                            else if (leaderField.FeedDataType == "array")
                            {
                                int mappedChildPropertiesCount = leaderField.Childrens.Where(x => !string.IsNullOrEmpty(x.ActualFeedKeyPath)).Count();
                                //if no child properties found then it is array of strings
                                if (mappedChildPropertiesCount != 0)
                                {
                                    //otherwise at least one of the child properties are mapped and hence it is array of objects
                                    if (leaderField.ChildrenRecords != null && leaderField.ChildrenRecords.Count > 0)
                                    {
                                        //insert multiple leader
                                        foreach (var record in leaderField.ChildrenRecords)
                                        {
                                            InsertPerson((long)eventId, record, isLeader: true, prefix: prefix, feedProviderId: feedProvider.Id);
                                        }
                                    }
                                }
                            }
                        }
                        #endregion Leader - can be object or array

                        #region EventSchedule - can be string, object or array
                        var eventScheduleField = GetFeedMapping(lstFeedMapping, prefix + "schedule");

                        if (eventScheduleField != null)
                        {
                            if (eventScheduleField.FeedDataType == "object")
                                InsertEventSchedule((long)eventId, eventScheduleField?.Childrens, prefix, feedProviderId: feedProvider.Id);
                            else if (eventScheduleField.FeedDataType == "array")
                            {
                                int mappedChildPropertiesCount = eventScheduleField.Childrens.Where(x => !string.IsNullOrEmpty(x.ActualFeedKeyPath)).Count();

                                //if no child properties found then it is array of strings
                                if (mappedChildPropertiesCount != 0)
                                {
                                    //otherwise at least one of the child properties are mapped and hence it is array of objects
                                    if (eventScheduleField.ChildrenRecords != null && eventScheduleField.ChildrenRecords.Count > 0)
                                    {
                                        //insert multiple event schedule
                                        foreach (var record in eventScheduleField.ChildrenRecords)
                                        {
                                            InsertEventSchedule((long)eventId, record, prefix, feedProviderId: feedProvider.Id);
                                        }
                                    }
                                }
                            }
                        }
                        #endregion EventSchedule - can be string, object or array

                        #region Programme - can be string or object
                        var programmeField = GetFeedMapping(lstFeedMapping, prefix + "programme");
                        long? programmeId = null;

                        //check if there is only one programme of string or object type
                        if (programmeField != null)
                        {
                            if (programmeField.FeedDataType == "string")
                            {
                                var lstFeedMappingProgramme = new List<FeedMapping>();

                                var feedMappingProgramme = new FeedMapping()
                                {
                                    ColumnName = prefix + "programme",
                                    FeedKeyValue = programmeField.FeedKeyValue
                                };

                                lstFeedMappingProgramme.Add(feedMappingProgramme);

                                programmeId = InsertProgramme((long)eventId, lstFeedMappingProgramme, prefix, feedProviderId: feedProvider.Id);
                            }
                            else if (programmeField.FeedDataType == "object")
                            {
                                programmeId = InsertProgramme((long)eventId, programmeField?.Childrens, prefix, feedProviderId: feedProvider.Id);
                            }
                        }
                        #endregion Programme - can be string or object

                        #region PhysicalActivity - can be string, object or array
                        var activityField = GetFeedMapping(lstFeedMapping, prefix + "activity");

                        if (activityField != null)
                        {
                            //check if there is only one activity of string or object type
                            if (activityField.FeedDataType == "string")
                            {
                                var lstFeedMappingActivity = new List<FeedMapping>();

                                var feedMappingActivity = new FeedMapping()
                                {
                                    ColumnName = prefix + "activity_prefLabel",
                                    FeedKeyValue = activityField.FeedKeyValue
                                };

                                lstFeedMappingActivity.Add(feedMappingActivity);

                                InsertPhysicalActivity((long)eventId, lstFeedMappingActivity, prefix + "activity", feedProviderId: feedProvider.Id);
                            }
                            else if (activityField.FeedDataType == "object")
                            {
                                InsertPhysicalActivity((long)eventId, activityField.Childrens, prefix + "activity", feedProviderId: feedProvider.Id);
                            }
                            else if (activityField.FeedDataType == "array")
                            {
                                int mappedChildPropertiesCount = activityField.Childrens.Where(x => !string.IsNullOrEmpty(x.ActualFeedKeyPath)).Count();

                                //if no child properties found then it is array of strings
                                if (mappedChildPropertiesCount == 0)
                                {
                                    var lstActivities = JsonConvert.DeserializeObject<JArray>(Convert.ToString(activityField.FeedKeyValue))?.ToList();

                                    //insert multiple physical activities
                                    foreach (var activity in lstActivities)
                                    {
                                        string strVal = activity.Value<string>();
                                        var lstFeedMappingActivity = new List<FeedMapping>();

                                        var feedMappingActivity = new FeedMapping()
                                        {
                                            ColumnName = prefix + "activity_prefLabel",
                                            FeedKeyValue = strVal
                                        };

                                        lstFeedMappingActivity.Add(feedMappingActivity);

                                        InsertPhysicalActivity((long)eventId, lstFeedMappingActivity, prefix + "activity", feedProviderId: feedProvider.Id);
                                    }
                                }
                                else
                                {
                                    //otherwise at least one of the child properties are mapped and hence it is array of objects
                                    if (activityField.ChildrenRecords != null && activityField.ChildrenRecords.Count > 0)
                                    {
                                        //insert multiple physical activities
                                        foreach (var record in activityField.ChildrenRecords)
                                        {
                                            InsertPhysicalActivity((long)eventId, record, prefix + "activity", feedProviderId: feedProvider.Id);
                                        }
                                    }
                                }
                            }
                        }
                        #endregion PhysicalActivity - can be string, object or array

                        #region Offer - can be object or array
                        var offer = GetFeedMapping(lstFeedMapping, prefix + "offer");
                        long? OfferID = null;
                        if (offer != null)
                        {
                            if (offer.FeedDataType == "object")
                            {
                                OfferID = InsertOffer((long)eventId, offer.Childrens, prefix + "offer", feedProviderId: feedProvider.Id);
                            }
                            else if (offer.FeedDataType == "array")
                            {
                                int mappedChildPropertiesCount = offer.Childrens.Where(x => !string.IsNullOrEmpty(x.ActualFeedKeyPath)).Count();

                                //if no child properties found then it is array of strings
                                if (mappedChildPropertiesCount == 0)
                                {
                                    var lstOffers = JsonConvert.DeserializeObject<JArray>(Convert.ToString(offer.FeedKeyValue))?.ToList();

                                    //insert multiple offer
                                    foreach (var singleoffer in lstOffers)
                                    {
                                        string strVal = singleoffer.Value<string>();
                                        var lstFeedMappingOffer = new List<FeedMapping>();

                                        var feedMappingActivity = new FeedMapping()
                                        {
                                            ColumnName = prefix + "offer_price",
                                            FeedKeyValue = strVal
                                        };

                                        lstFeedMappingOffer.Add(feedMappingActivity);

                                        InsertOffer((long)eventId, lstFeedMappingOffer, prefix + "offer", feedProviderId: feedProvider.Id);
                                    }
                                }
                                else
                                {
                                    //otherwise at least one of the child properties are mapped and hence it is array of objects
                                    if (offer.ChildrenRecords != null && offer.ChildrenRecords.Count > 0)
                                    {
                                        //insert multiple offer
                                        foreach (var record in offer.ChildrenRecords)
                                        {
                                            InsertOffer((long)eventId, record, prefix + "offer", feedProviderId: feedProvider.Id);
                                        }
                                    }
                                }
                            }
                        }
                        #endregion

                        #region facilityUse
                        var facilityUse = GetFeedMapping(lstFeedMapping, prefix + "facilityUse");
                        if (facilityUse != null)
                        {
                            if (facilityUse.FeedDataType == "object")
                            {
                                InsertFacilityUse((long)eventId, facilityUse.Childrens, prefix + "facilityUse", feedProvider.Id);
                            }
                            else if (facilityUse.FeedDataType == "array")
                            {
                                if (facilityUse.ChildrenRecords != null && facilityUse.ChildrenRecords.Count > 0)
                                {
                                    //insert multiple offer
                                    foreach (var record in facilityUse.ChildrenRecords)
                                    {
                                        InsertFacilityUse((long)eventId, record, prefix + "facilityUse", feedProvider.Id);
                                    }
                                }                               
                            }
                        }
                        #endregion

                        #region  Slot - can be object or array Model V2
                        var slot = GetFeedMapping(lstFeedMapping, prefix + "slot");
                        if (slot != null)
                        {
                            if (slot.FeedDataType == "object")
                            {
                                InsertSlot((long)eventId, OfferID, slot.Childrens, prefix + "slot", feedProvider.Id);
                            }
                            else if (slot.FeedDataType == "array")
                            {
                                if (slot.ChildrenRecords != null && slot.ChildrenRecords.Count > 0)
                                {
                                    //insert multiple offer
                                    foreach (var record in slot.ChildrenRecords)
                                    {
                                        InsertSlot((long)eventId, OfferID, record, prefix + "slot", feedProvider.Id);
                                    }
                                }                                
                            }
                        }
                        #endregion                         

                        #region SubEvent - can be object or array
                        var subEventField = GetFeedMapping(lstFeedMapping, prefix + "subEvent");

                        if (subEventField != null)
                        {
                            if (subEventField.FeedDataType == "object")
                            {
                                if (subEventField?.Childrens != null && subEventField?.Childrens?.Count > 0)
                                {
                                    Insert_v1(subEventField?.Childrens, prefix: "subEvent_", superEventId: eventId);
                                }
                            }
                            else if (subEventField.FeedDataType == "array")
                            {
                                if (subEventField.ChildrenRecords != null && subEventField.ChildrenRecords.Count > 0)
                                {
                                    //insert multiple subevents
                                    foreach (var record in subEventField.ChildrenRecords)
                                    {
                                        int? subEventMappedFieldCount = record?.Where(x => !string.IsNullOrEmpty(x.ActualFeedKeyPath))?.Count();

                                        if (subEventMappedFieldCount != null && subEventMappedFieldCount > 0)
                                        {
                                            Insert_v1(record, prefix: "subEvent_", superEventId: eventId);
                                        }
                                    }
                                }
                            }
                        }
                        #endregion SubEvent - can be object or array

                        #region Occurrence
                        if (isRootEvent == true)
                            InsertEventOccurrence((long)eventId, feedProvider.Id);
                        #endregion
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertErrorLogs("[DataLaundryScheduler] FeedHelper", "Insert_v1", ex.Message, ex.InnerException?.Message, ex.StackTrace, lstFeedMapping.FirstOrDefault().FeedProvider.Id);
                #region Update to scheduler Log   
                if (schedulerLogId > 0)
                {
                    long logId;
                    LogHelper.InsertUpdateSchedulerLog(schedulerLogId, lstFeedMapping.FirstOrDefault().FeedProvider.Id, null, DateTime.Now, LogHelper.stringValueOf(LogStatus.ErrorOccurred), out logId, "[DataLaundryScheduler] FeedHelper - Insert_v1");
                }
                #endregion
            }
            return eventId;
        }

        public static bool DeleteOldEventData(long eventId)
        {
            var lstSqlParameter = new List<SqlParameter>();

            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@EventId", SqlDbType = SqlDbType.BigInt, Value = eventId });

            int rowsAffected = DBProvider.ExecuteNonQuery("Event_DeleteOldData", CommandType.StoredProcedure, ref lstSqlParameter);            
            return rowsAffected > 0;
        }

        private static long? InsertPhysicalActivity(long eventId, List<FeedMapping> lstFeedMapping, string prefix = "activity", long? feedProviderId = null)
        {
            long? physicalActivityId = null;
            var lstSqlParameter = new List<SqlParameter>();
            try
            {
                object prefLabel = GetFeedKeyValue(lstFeedMapping, prefix + "_prefLabel");
                object altLabel = GetFeedKeyValue(lstFeedMapping, prefix + "_altLabel");
                object inScheme = GetFeedKeyValue(lstFeedMapping, prefix + "_inScheme");
                object notation = GetFeedKeyValue(lstFeedMapping, prefix + "_notation");
                object image = GetFeedKeyValue(lstFeedMapping, prefix + "_image"); // Model v2
                object description = GetFeedKeyValue(lstFeedMapping, prefix + "_description"); // Model v2
                long? broaderActivityId = null;
                long? narrowerActivityId = null;

                var broaderField = GetFeedMapping(lstFeedMapping, prefix + "_broader");
                var narrowerField = GetFeedMapping(lstFeedMapping, prefix + "_narrower");

                if (broaderField != null)
                {
                    var lstFeedMappingBroader = broaderField?.Childrens?.Where(x => !string.IsNullOrEmpty(x.ActualFeedKeyPath))?.ToList();

                    if (lstFeedMappingBroader != null && lstFeedMappingBroader.Count > 0)
                    {
                        broaderActivityId = InsertPhysicalActivity(eventId, lstFeedMappingBroader, prefix: prefix + "_broader");
                    }
                }
                if (narrowerField != null)
                {
                    var lstFeedMappingNarrower = narrowerField?.Childrens?.Where(x => !string.IsNullOrEmpty(x.ActualFeedKeyPath))?.ToList();

                    if (lstFeedMappingNarrower != null && lstFeedMappingNarrower.Count > 0)
                    {
                        narrowerActivityId = InsertPhysicalActivity(eventId, lstFeedMappingNarrower, prefix: prefix + "_narrower");
                    }
                }
                if (prefLabel != null)
                {
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@EventId", SqlDbType = SqlDbType.BigInt, Value = eventId });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@PrefLabel", SqlDbType = SqlDbType.NVarChar, Value = prefLabel ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@AltLabel", SqlDbType = SqlDbType.NVarChar, Value = altLabel ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@InScheme", SqlDbType = SqlDbType.NVarChar, Value = inScheme ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Notation", SqlDbType = SqlDbType.NVarChar, Value = notation ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@BroaderId", SqlDbType = SqlDbType.BigInt, Value = (object)broaderActivityId ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@NarrowerId", SqlDbType = SqlDbType.BigInt, Value = (object)narrowerActivityId ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Image", SqlDbType = SqlDbType.NVarChar, Value = (object)image ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Description", SqlDbType = SqlDbType.NVarChar, Value = (object)description ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@PhysicalActivityId", SqlDbType = SqlDbType.BigInt, Direction = ParameterDirection.Output });

                    int rowsAffected = DBProvider.ExecuteNonQuery("PhysicalActivity_Insert", CommandType.StoredProcedure, ref lstSqlParameter);

                    var physicalActivityIdParam = lstSqlParameter.Where(x => x.ParameterName == "@PhysicalActivityId").FirstOrDefault().Value;

                    physicalActivityId = physicalActivityIdParam == DBNull.Value ? null : (long?)physicalActivityIdParam;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertErrorLogs("[DataLaundryScheduler] FeedHelper", "InsertPhysicalActivity", ex.Message, ex.InnerException?.Message, ex.StackTrace, feedProviderId);
            }
            return physicalActivityId;
        }
        //Model V2
        private static long? InsertOffer(long eventId, List<FeedMapping> lstFeedMapping, string prefix = "offer", long? feedProviderId = null, long? SlotId = null)
        {
            long? OfferId = null;
            var lstSqlParameter = new List<SqlParameter>();
            try
            {
                object name = GetFeedKeyValue(lstFeedMapping, prefix + "_name");
                object price = GetFeedKeyValue(lstFeedMapping, prefix + "_price");
                object priceCurrency = GetFeedKeyValue(lstFeedMapping, prefix + "_priceCurrency");
                object identifier = GetFeedKeyValue(lstFeedMapping, prefix + "_identifier");
                object description = GetFeedKeyValue(lstFeedMapping, prefix + "_description");

                if (price != null)
                {
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@EventId", SqlDbType = SqlDbType.BigInt, Value = eventId });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@SlotId", SqlDbType = SqlDbType.BigInt, Value = SlotId });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Identifier", SqlDbType = SqlDbType.NVarChar, Value = (object)identifier ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Name", SqlDbType = SqlDbType.NVarChar, Value = (object)name ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Price", SqlDbType = SqlDbType.NVarChar, Value = (object)price ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@PriceCurrency", SqlDbType = SqlDbType.NVarChar, Value = (object)priceCurrency ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Description", SqlDbType = SqlDbType.NVarChar, Value = (object)description ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@OfferId", SqlDbType = SqlDbType.BigInt, Direction = ParameterDirection.Output });

                    int rowsAffected = DBProvider.ExecuteNonQuery("Offer_Insert", CommandType.StoredProcedure, ref lstSqlParameter);

                    var offerIdParam = lstSqlParameter.Where(x => x.ParameterName == "@OfferId").FirstOrDefault().Value;

                    OfferId = offerIdParam == DBNull.Value ? null : (long?)offerIdParam;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertErrorLogs("[DataLaundryScheduler] FeedHelper", "InsertOffer", ex.Message, ex.InnerException?.Message, ex.StackTrace, feedProviderId);
            }
            return OfferId;
        }
        //Model V2
        private static long? InsertSlot(long eventId, long? offerId, List<FeedMapping> lstFeedMapping, string prefix = "slot", long? feedProviderId = null)
        {
            long? slotId = null;
            try
            {
                var lstSqlParameter = new List<SqlParameter>();
                object identifier = GetFeedKeyValue(lstFeedMapping, prefix + "_identifier");
                object startDate = GetFeedKeyValue(lstFeedMapping, prefix + "_startDate");
                object endDate = GetFeedKeyValue(lstFeedMapping, prefix + "_endDate");
                object duration = GetFeedKeyValue(lstFeedMapping, prefix + "_duration");
                object remainingUses = GetFeedKeyValue(lstFeedMapping, prefix + "_remainingUses");
                object maximumUses = GetFeedKeyValue(lstFeedMapping, prefix + "_maximumUses");
                var offerFeedMapping = GetFeedMapping(lstFeedMapping, prefix + "_offer");

                if (startDate != null)
                {
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@EventId", SqlDbType = SqlDbType.BigInt, Value = eventId });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Identifier", SqlDbType = SqlDbType.NVarChar, Value = (object)identifier ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@StartDate", SqlDbType = SqlDbType.DateTime, Value = (object)startDate ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@EndDate", SqlDbType = SqlDbType.DateTime, Value = (object)endDate ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Duration", SqlDbType = SqlDbType.NVarChar, Value = (object)duration ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@OfferID", SqlDbType = SqlDbType.BigInt, Value = offerId });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@RemainingUses", SqlDbType = SqlDbType.NVarChar, Value = (object)remainingUses ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@MaximumUses", SqlDbType = SqlDbType.NVarChar, Value = (object)maximumUses ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@slotId", SqlDbType = SqlDbType.BigInt, Direction = ParameterDirection.Output });

                    int rowsAffected = DBProvider.ExecuteNonQuery("Slot_Insert", CommandType.StoredProcedure, ref lstSqlParameter);

                    var slotIdParam = lstSqlParameter.Where(x => x.ParameterName == "@slotId").FirstOrDefault().Value;

                    slotId = slotIdParam == DBNull.Value ? null : (long?)slotIdParam;
                }
                if (offerFeedMapping != null)
                {
                    if (offerFeedMapping.FeedDataType == "object")
                    {
                        InsertOffer((long)eventId, offerFeedMapping.Childrens, prefix: prefix + "_offer", feedProviderId, slotId);
                    }
                    else if (offerFeedMapping.FeedDataType == "array")
                    {
                        if (offerFeedMapping.ChildrenRecords != null && offerFeedMapping.ChildrenRecords.Count > 0)
                        {
                            //insert multiple offer
                            foreach (var record in offerFeedMapping.ChildrenRecords)
                            {
                                InsertOffer((long)eventId, record, prefix: prefix + "_offer", feedProviderId, slotId);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertErrorLogs("[DataLaundryScheduler] FeedHelper", "InsertSlot", ex.Message, ex.InnerException?.Message, ex.StackTrace, feedProviderId);
            }
            return slotId;
        }

        //Model V2
        private static long? InsertFacilityUse(long eventId, List<FeedMapping> lstFeedMapping, string prefix = "facilityUse", long? feedProviderId = null)
        {
            long? facilityUsedId = null;
            try
            {
                object Type = GetFeedKeyValue(lstFeedMapping, prefix + "_type");
                object Identifier = GetFeedKeyValue(lstFeedMapping, prefix + "_identifier");
                object URL = GetFeedKeyValue(lstFeedMapping, prefix + "_url");
                object Name = GetFeedKeyValue(lstFeedMapping, prefix + "_name");
                object Description = GetFeedKeyValue(lstFeedMapping, prefix + "_description");
                object Provider = GetFeedKeyValue(lstFeedMapping, prefix + "_provider");
                object Image = GetFeedKeyValue(lstFeedMapping, prefix + "_image");

                var individualFacilityUse = GetFeedMapping(lstFeedMapping, prefix + "_individualFacilityUse");

                if (Name != null)
                {
                    var lstSqlParameter = new List<SqlParameter>();
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@EventId", SqlDbType = SqlDbType.BigInt, Value = eventId });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@ParentId", SqlDbType = SqlDbType.BigInt, Value = null });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@TypeID", SqlDbType = SqlDbType.Int, Value = 1 });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@url", SqlDbType = SqlDbType.NVarChar, Value = (object)URL ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@identifier", SqlDbType = SqlDbType.NVarChar, Value = (object)Identifier ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@name", SqlDbType = SqlDbType.NVarChar, Value = (object)Name ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@description", SqlDbType = SqlDbType.NVarChar, Value = (object)Description ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@provider", SqlDbType = SqlDbType.NVarChar, Value = (object)Provider ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@image", SqlDbType = SqlDbType.NVarChar, Value = (object)Image ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@facilityUsedId", SqlDbType = SqlDbType.BigInt, Direction = ParameterDirection.Output });

                    int rowsAffected = DBProvider.ExecuteNonQuery("FacilityUse_Insert", CommandType.StoredProcedure, ref lstSqlParameter);
                    var facilityUsedIdParam = lstSqlParameter.Where(x => x.ParameterName == "@facilityUsedId").FirstOrDefault().Value;
                    facilityUsedId = facilityUsedIdParam == DBNull.Value ? null : (long?)facilityUsedIdParam;
                }
                if (individualFacilityUse != null && facilityUsedId != null)
                {
                    if (individualFacilityUse.FeedDataType == "array")
                    {
                        if (individualFacilityUse.ChildrenRecords.Count > 0)
                        {
                            foreach (var record in individualFacilityUse.ChildrenRecords)
                            {
                                IndividualFacilityUse((long)eventId, record, prefix: prefix + "_individualFacilityUse", feedProviderId, facilityUsedId);
                            }
                        }
                    }
                    else if (individualFacilityUse.FeedDataType == "object")
                    {
                        var lstFeedMappingIndualFacilityUse = individualFacilityUse.Childrens?.Where(x => !string.IsNullOrEmpty(x.ActualFeedKeyPath))?.ToList();
                        if (lstFeedMappingIndualFacilityUse != null && lstFeedMappingIndualFacilityUse.Count > 0)
                        {
                            IndividualFacilityUse((long)eventId, lstFeedMappingIndualFacilityUse, prefix: prefix + "_individualFacilityUse", feedProviderId, facilityUsedId);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                LogHelper.InsertErrorLogs("[DataLaundryScheduler] FeedHelper", "InsertFacilityUse", ex.Message, ex.InnerException?.Message, ex.StackTrace, feedProviderId);
            }
            return facilityUsedId;

        }

        //Model  V2
        private static long? IndividualFacilityUse(long eventId, List<FeedMapping> lstFeedMapping, string prefix = "individualFacilityUse", long? feedProviderId = null, long? parentId = null)
        {
            long? facilityUsedId = null;
            try
            {
                object Type = GetFeedKeyValue(lstFeedMapping, prefix + "_type");
                object Identifier = GetFeedKeyValue(lstFeedMapping, prefix + "_identifier");
                object URL = GetFeedKeyValue(lstFeedMapping, prefix + "_url");
                object Name = GetFeedKeyValue(lstFeedMapping, prefix + "_name");
                object Description = GetFeedKeyValue(lstFeedMapping, prefix + "_description");
                object Provider = GetFeedKeyValue(lstFeedMapping, prefix + "_provider");
                object Image = GetFeedKeyValue(lstFeedMapping, prefix + "_image");

                if (Name != null && parentId != null)
                {
                    var lstSqlParameter = new List<SqlParameter>();
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@EventId", SqlDbType = SqlDbType.BigInt, Value = eventId });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@ParentId", SqlDbType = SqlDbType.BigInt, Value = parentId });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@TypeID", SqlDbType = SqlDbType.Int, Value = 2 });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@url", SqlDbType = SqlDbType.NVarChar, Value = (object)URL ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@identifier", SqlDbType = SqlDbType.NVarChar, Value = (object)Identifier ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@name", SqlDbType = SqlDbType.NVarChar, Value = (object)Name ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@description", SqlDbType = SqlDbType.NVarChar, Value = (object)Description ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@provider", SqlDbType = SqlDbType.NVarChar, Value = (object)Provider ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@image", SqlDbType = SqlDbType.NVarChar, Value = (object)Image ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@facilityUsedId", SqlDbType = SqlDbType.BigInt, Direction = ParameterDirection.Output });

                    int rowsAffected = DBProvider.ExecuteNonQuery("FacilityUse_Insert", CommandType.StoredProcedure, ref lstSqlParameter);
                    var facilityUsedIdParam = lstSqlParameter.Where(x => x.ParameterName == "@facilityUsedId").FirstOrDefault().Value;
                    facilityUsedId = facilityUsedIdParam == DBNull.Value ? null : (long?)facilityUsedIdParam;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertErrorLogs("[DataLaundryScheduler] FeedHelper", "IndividualFacilityUse", ex.Message, ex.InnerException?.Message, ex.StackTrace, feedProviderId);
            }
            return facilityUsedId;
        }

        private static long? InsertPlace(long eventId, List<FeedMapping> lstFeedMapping, string prefix = "location", long? parentId = null, int? placeTypeId = null, long? feedProviderId = null)
        {
            long? placeId = null;
            var lstSqlParameter = new List<SqlParameter>();
            try
            {
                object name = GetFeedKeyValue(lstFeedMapping, prefix + "_name");
                object description = GetFeedKeyValue(lstFeedMapping, prefix + "_description");
                object image = GetFeedKeyValue(lstFeedMapping, prefix + "_image");
                object url = GetFeedKeyValue(lstFeedMapping, prefix + "_url");

                var locationAddressField = GetFeedMapping(lstFeedMapping, prefix + "_address");
                object address = null;
                var locationGeoField = GetFeedMapping(lstFeedMapping, prefix + "_geo");
                object latitude = null;
                object longitude = null;
                object streetAddress = null;
                object addressLocality = null;
                object postalCode = null;
                object region = null;
                object telephone = GetFeedKeyValue(lstFeedMapping, prefix + "_telephone");
                object faxNumber = GetFeedKeyValue(lstFeedMapping, prefix + "_faxNumber");
                var containsPlace = GetFeedMapping(lstFeedMapping, prefix + "_containsPlace");
                var containedInPlace = GetFeedMapping(lstFeedMapping, prefix + "_containedInPlace");
                var openingHoursSpecification = GetFeedKeyValue(lstFeedMapping, prefix + "_openingHoursSpecification");// Model V2                
                long? containsPlaceId = null;
                long? containedInPlaceId = null;

                if (locationAddressField?.FeedDataType == "string")
                {
                    address = locationAddressField.FeedKeyValue;
                }
                else if (locationAddressField?.FeedDataType == "object")
                {
                    streetAddress = GetFeedKeyValue(locationAddressField?.Childrens, prefix + "_address_streetAddress");
                    addressLocality = GetFeedKeyValue(locationAddressField?.Childrens, prefix + "_address_addressLocality");
                    postalCode = GetFeedKeyValue(locationAddressField?.Childrens, prefix + "_address_postalCode");
                    region = GetFeedKeyValue(locationAddressField?.Childrens, prefix + "_address_region");

                    address = Convert.ToString(streetAddress) + ' ' + Convert.ToString(addressLocality) + ' ' + Convert.ToString(postalCode) + ' ' + Convert.ToString(region);
                }

                //check if geo coordinates available
                if (locationGeoField != null && !string.IsNullOrEmpty(locationGeoField.ActualFeedKeyPath))
                {
                    latitude = GetFeedKeyValue(locationGeoField?.Childrens, prefix + "_geo_latitude");
                    longitude = GetFeedKeyValue(locationGeoField?.Childrens, prefix + "_geo_longitude");
                }

                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@EventId", SqlDbType = SqlDbType.BigInt, Value = eventId });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@ParentId", SqlDbType = SqlDbType.BigInt, Value = (object)parentId ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@PlaceTypeId", SqlDbType = SqlDbType.BigInt, Value = (object)placeTypeId ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Name", SqlDbType = SqlDbType.NVarChar, Value = name ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Description", SqlDbType = SqlDbType.NVarChar, Value = description ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Image", SqlDbType = SqlDbType.NVarChar, Value = image ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Url", SqlDbType = SqlDbType.NVarChar, Value = url ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Address", SqlDbType = SqlDbType.NVarChar, Value = address ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Lat", SqlDbType = SqlDbType.NVarChar, Value = latitude ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Long", SqlDbType = SqlDbType.NVarChar, Value = longitude ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Telephone", SqlDbType = SqlDbType.NVarChar, Value = telephone ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@FaxNumber", SqlDbType = SqlDbType.NVarChar, Value = faxNumber ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@StreetAddress", SqlDbType = SqlDbType.NVarChar, Value = streetAddress ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@AddressLocality", SqlDbType = SqlDbType.NVarChar, Value = addressLocality ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@PostalCode", SqlDbType = SqlDbType.NVarChar, Value = postalCode ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Region", SqlDbType = SqlDbType.NVarChar, Value = region ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@OpeningHoursSpecification", SqlDbType = SqlDbType.NVarChar, Value = openingHoursSpecification ?? DBNull.Value });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@PlaceId", SqlDbType = SqlDbType.BigInt, Direction = ParameterDirection.Output });

                int rowsAffected = DBProvider.ExecuteNonQuery("Place_Insert", CommandType.StoredProcedure, ref lstSqlParameter);

                var placeIdParam = lstSqlParameter.Where(x => x.ParameterName == "@PlaceId").FirstOrDefault().Value;

                placeId = placeIdParam == DBNull.Value ? null : (long?)placeIdParam;

                #region AmenityFeature - can be array
                if (placeId != null && placeId > 0)
                {
                    if (containsPlace != null)
                    {
                        var lstFeedMappingContainsPlace = containsPlace.Childrens?.Where(x => !string.IsNullOrEmpty(x.ActualFeedKeyPath))?.ToList();

                        if (lstFeedMappingContainsPlace != null && lstFeedMappingContainsPlace.Count > 0)
                        {
                            int containsPlaceTypeId = 1;
                            containsPlaceId = InsertPlace(eventId, lstFeedMappingContainsPlace, prefix: prefix + "_containsPlace", parentId: placeId, placeTypeId: containsPlaceTypeId, feedProviderId: feedProviderId);
                        }
                    }

                    if (containedInPlace != null)
                    {
                        var lstFeedMappingContainedInPlace = containedInPlace?.Childrens?.Where(x => !string.IsNullOrEmpty(x.ActualFeedKeyPath))?.ToList();

                        if (lstFeedMappingContainedInPlace != null && lstFeedMappingContainedInPlace.Count > 0)
                        {
                            int containedInPlaceTypeId = 2;
                            containedInPlaceId = InsertPlace(eventId, lstFeedMappingContainedInPlace, prefix: prefix + "_containedInPlace", parentId: placeId, placeTypeId: containedInPlaceTypeId, feedProviderId: feedProviderId);
                        }
                    }

                    var amenityFeatureField = GetFeedMapping(lstFeedMapping, prefix + "_amenityFeature");

                    if (amenityFeatureField != null && amenityFeatureField.FeedDataType == "array")
                    {
                        if (amenityFeatureField.ChildrenRecords.Count > 0)
                        {
                            //insert multiple physical activities
                            foreach (var record in amenityFeatureField.ChildrenRecords)
                            {
                                InsertAmenityFeature((long)eventId, (long)placeId, record, prefix, feedProviderId);
                            }
                        }
                    }
                }
                #endregion AmenityFeature - can be array
            }
            catch (Exception ex)
            {
                LogHelper.InsertErrorLogs("[DataLaundryScheduler] FeedHelper", "InsertPlace", ex.Message, ex.InnerException?.Message, ex.StackTrace, feedProviderId);
            }
            return placeId;
        }

        private static long? InsertAmenityFeature(long eventId, long placeId, List<FeedMapping> lstFeedMapping, string prefix = "location", long? feedProviderId = null)
        {
            long? amenityFeatureId = null;
            var lstSqlParameter = new List<SqlParameter>();
            try
            {
                object type = GetFeedKeyValue(lstFeedMapping, prefix + "_amenityFeature_type");
                object name = GetFeedKeyValue(lstFeedMapping, prefix + "_amenityFeature_name");
                object value = GetFeedKeyValue(lstFeedMapping, prefix + "_amenityFeature_value");
                if (type != null && name != null)
                {
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@EventId", SqlDbType = SqlDbType.BigInt, Value = eventId });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@PlaceId", SqlDbType = SqlDbType.BigInt, Value = placeId });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Type", SqlDbType = SqlDbType.NVarChar, Value = type ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Name", SqlDbType = SqlDbType.NVarChar, Value = name ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Value", SqlDbType = SqlDbType.Bit, Value = value ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@AmenityFeatureId", SqlDbType = SqlDbType.BigInt, Direction = ParameterDirection.Output });

                    int rowsAffected = DBProvider.ExecuteNonQuery("AmenityFeature_Insert", CommandType.StoredProcedure, ref lstSqlParameter);

                    var amenityFeatureIdParam = lstSqlParameter.Where(x => x.ParameterName == "@AmenityFeatureId").FirstOrDefault().Value;

                    amenityFeatureId = amenityFeatureIdParam == DBNull.Value ? null : (long?)amenityFeatureIdParam;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertErrorLogs("[DataLaundryScheduler] FeedHelper", "InsertAmenityFeature", ex.Message, ex.InnerException?.Message, ex.StackTrace, feedProviderId);
            }
            return amenityFeatureId;
        }

        private static long? InsertOrganization(long eventId, List<FeedMapping> lstFeedMapping, string prefix = "", long? feedProviderId = null)
        {
            long? organizationId = null;
            var lstSqlParameter = new List<SqlParameter>();
            try
            {
                object name = GetFeedKeyValue(lstFeedMapping, prefix + "organizer_name");
                object url = GetFeedKeyValue(lstFeedMapping, prefix + "organizer_url");
                object description = GetFeedKeyValue(lstFeedMapping, prefix + "organizer_description");
                object email = GetFeedKeyValue(lstFeedMapping, prefix + "organizer_email");
                object telephone = GetFeedKeyValue(lstFeedMapping, prefix + "organizer_telephone");
                //object logo = GetFeedKeyValue(lstFeedMapping, prefix + "organizer_logo");

                var logoField = GetFeedMapping(lstFeedMapping, prefix + "organizer_logo");
                object logo = null;

                if (logoField != null)
                {
                    if (logoField.FeedDataType == "object")
                    {
                        logo = GetFeedKeyValue(logoField.Childrens, prefix + "organizer_logo_url");
                    }
                    else if (logoField.FeedDataType == "string")
                    {
                        logo = logoField.FeedKeyValue;
                    }
                    else if (logoField.FeedDataType == "array")
                    {
                        int mappedChildPropertiesCount = logoField.Childrens.Where(x => !string.IsNullOrEmpty(x.ActualFeedKeyPath)).Count();

                        if (mappedChildPropertiesCount != 0)
                        {
                            if (logoField.ChildrenRecords != null && logoField.ChildrenRecords.Count > 0)
                            {
                                foreach (var record in logoField.ChildrenRecords)
                                {
                                    logo = GetFeedKeyValue(logoField.Childrens, prefix + "organizer_logo_url");
                                }
                            }
                        }
                    }
                }

                if (name != null)
                {
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@EventId", SqlDbType = SqlDbType.BigInt, Value = eventId });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Name", SqlDbType = SqlDbType.NVarChar, Value = name });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Url", SqlDbType = SqlDbType.NVarChar, Value = url ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Description", SqlDbType = SqlDbType.NVarChar, Value = description ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Email", SqlDbType = SqlDbType.NVarChar, Value = email ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Image", SqlDbType = SqlDbType.NVarChar, Value = logo ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Telephone", SqlDbType = SqlDbType.NVarChar, Value = telephone ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@OrganizationId", SqlDbType = SqlDbType.BigInt, Direction = ParameterDirection.Output });

                    int rowsAffected = DBProvider.ExecuteNonQuery("Organization_Insert", CommandType.StoredProcedure, ref lstSqlParameter);

                    var organizationIdParam = lstSqlParameter.Where(x => x.ParameterName == "@OrganizationId").FirstOrDefault().Value;

                    organizationId = organizationIdParam == DBNull.Value ? null : (long?)organizationIdParam;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertErrorLogs("[DataLaundryScheduler] FeedHelper", "InsertOrganization", ex.Message, ex.InnerException?.Message, ex.StackTrace, feedProviderId);
            }
            return organizationId;
        }

        private static long? InsertPerson(long eventId, List<FeedMapping> lstFeedMapping, bool isLeader = false, string prefix = "", long? feedProviderId = null)
        {
            long? personId = null;
            var lstSqlParameter = new List<SqlParameter>();
            try
            {
                prefix += isLeader ? "leader" : "contributor";

                object name = GetFeedKeyValue(lstFeedMapping, prefix + "_name");
                object url = GetFeedKeyValue(lstFeedMapping, prefix + "_url");
                object description = GetFeedKeyValue(lstFeedMapping, prefix + "_description");
                object email = GetFeedKeyValue(lstFeedMapping, prefix + "_email");
                object logo = GetFeedKeyValue(lstFeedMapping, prefix + "_logo");
                object telephone = GetFeedKeyValue(lstFeedMapping, prefix + "_telephone");

                if (name != null)
                {
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@EventId", SqlDbType = SqlDbType.BigInt, Value = eventId });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@IsLeader", SqlDbType = SqlDbType.Bit, Value = isLeader });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Name", SqlDbType = SqlDbType.NVarChar, Value = name });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Url", SqlDbType = SqlDbType.NVarChar, Value = url ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Description", SqlDbType = SqlDbType.NVarChar, Value = description ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Email", SqlDbType = SqlDbType.NVarChar, Value = email ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Image", SqlDbType = SqlDbType.NVarChar, Value = logo ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Telephone", SqlDbType = SqlDbType.NVarChar, Value = telephone ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@PersonId", SqlDbType = SqlDbType.BigInt, Direction = ParameterDirection.Output });

                    int rowsAffected = DBProvider.ExecuteNonQuery("Person_Insert", CommandType.StoredProcedure, ref lstSqlParameter);

                    var personIdParam = lstSqlParameter.Where(x => x.ParameterName == "@PersonId").FirstOrDefault().Value;

                    personId = personIdParam == DBNull.Value ? null : (long?)personIdParam;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertErrorLogs("[DataLaundryScheduler] FeedHelper", "InsertPerson", ex.Message, ex.InnerException?.Message, ex.StackTrace, feedProviderId);
            }
            return personId;
        }

        private static long? InsertProgramme(long eventId, List<FeedMapping> lstFeedMapping, string prefix = "", long? feedProviderId = null)
        {
            long? programmeId = null;
            var lstSqlParameter = new List<SqlParameter>();
            try
            {
                object name = GetFeedKeyValue(lstFeedMapping, prefix + "programme_name");
                object description = GetFeedKeyValue(lstFeedMapping, prefix + "programme_description");
                object url = GetFeedKeyValue(lstFeedMapping, prefix + "programme_url");

                if (name != null)
                {
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@EventId", SqlDbType = SqlDbType.BigInt, Value = eventId });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Name", SqlDbType = SqlDbType.NVarChar, Value = name });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Url", SqlDbType = SqlDbType.NVarChar, Value = url ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Description", SqlDbType = SqlDbType.NVarChar, Value = description ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Image", SqlDbType = SqlDbType.NVarChar, Value = DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@ProgrammeId", SqlDbType = SqlDbType.BigInt, Direction = ParameterDirection.Output });

                    int rowsAffected = DBProvider.ExecuteNonQuery("Programme_Insert", CommandType.StoredProcedure, ref lstSqlParameter);

                    var programmeIdParam = lstSqlParameter.Where(x => x.ParameterName == "@ProgrammeId").FirstOrDefault().Value;

                    programmeId = programmeIdParam == DBNull.Value ? null : (long?)programmeIdParam;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertErrorLogs("[DataLaundryScheduler] FeedHelper", "InsertProgramme", ex.Message, ex.InnerException?.Message, ex.StackTrace, feedProviderId);
            }
            return programmeId;
        }

        private static long? InsertEventSchedule(long eventId, List<FeedMapping> lstFeedMapping, string prefix = "", long? feedProviderId = null)
        {
            long? eventScheduleId = null;
            var lstSqlParameter = new List<SqlParameter>();
            try
            {
                object startDate = GetFeedKeyValue(lstFeedMapping, prefix + "schedule_startDate");
                object endDate = GetFeedKeyValue(lstFeedMapping, prefix + "schedule_endDate");
                object startTime = GetFeedKeyValue(lstFeedMapping, prefix + "schedule_startTime");
                object endTime = GetFeedKeyValue(lstFeedMapping, prefix + "schedule_endTime");
                object frequency = GetFeedKeyValue(lstFeedMapping, prefix + "schedule_frequency");

                var byDayField = GetFeedMapping(lstFeedMapping, prefix + "schedule_byDay");
                object byDay = null;

                if (byDayField != null)
                {
                    if (byDayField.FeedDataType == "array")
                    {
                        var lstByDay = JsonConvert.DeserializeObject<JArray>(Convert.ToString(byDayField.FeedKeyValue))?.ToList();

                        if (lstByDay != null && lstByDay.Count > 0)
                        {
                            var lstByDayFinal = lstByDay.Select(x => x.Value<string>()).ToList();

                            if (lstByDayFinal != null && lstByDayFinal.Count > 0)
                                byDay = string.Join(",", lstByDayFinal);
                        }
                    }
                    else
                    {
                        byDay = byDayField.FeedKeyValue;
                    }
                }

                var byMonthField = GetFeedMapping(lstFeedMapping, prefix + "schedule_byMonth");
                object byMonth = null;

                if (byMonthField != null)
                {
                    if (byMonthField.FeedDataType == "array")
                    {
                        var lstByMonth = JsonConvert.DeserializeObject<JArray>(Convert.ToString(byMonthField.FeedKeyValue))?.ToList();

                        if (lstByMonth != null && lstByMonth.Count > 0)
                        {
                            var lstByMonthFinal = lstByMonth.Select(x => x.Value<string>()).ToList();

                            if (lstByMonthFinal != null && lstByMonthFinal.Count > 0)
                                byMonth = string.Join(",", lstByMonthFinal);
                        }
                    }
                    else
                    {
                        byMonth = byMonthField.FeedKeyValue;
                    }
                }

                var byMonthDayField = GetFeedMapping(lstFeedMapping, prefix + "schedule_byMonthDay");
                object byMonthDay = null;

                if (byMonthDayField != null)
                {
                    if (byMonthDayField.FeedDataType == "array")
                    {
                        var lstByMonthDay = JsonConvert.DeserializeObject<JArray>(Convert.ToString(byMonthDayField.FeedKeyValue))?.ToList();

                        if (lstByMonthDay != null && lstByMonthDay.Count > 0)
                        {
                            var lstByMonthDayFinal = lstByMonthDay.Select(x => x.Value<string>()).ToList();

                            if (lstByMonthDayFinal != null && lstByMonthDayFinal.Count > 0)
                                byMonthDay = string.Join(",", lstByMonthDayFinal);
                        }
                    }
                    else
                    {
                        byMonthDay = byMonthDayField.FeedKeyValue;
                    }
                }

                object repeatCount = GetFeedKeyValue(lstFeedMapping, prefix + "schedule_repeatCount");
                object repeatFrequency = GetFeedKeyValue(lstFeedMapping, prefix + "schedule_repeatFrequency");

                var exceptDateField = GetFeedMapping(lstFeedMapping, prefix + "schedule_exceptDate");
                object exceptDate = null;

                if (exceptDateField != null)
                {
                    if (exceptDateField.FeedDataType == "array")
                    {
                        var lstExceptDate = JsonConvert.DeserializeObject<JArray>(Convert.ToString(exceptDateField.FeedKeyValue))?.ToList();

                        if (lstExceptDate != null && lstExceptDate.Count > 0)
                        {
                            var lstExceptDateFinal = lstExceptDate.Select(x => x.Value<string>()).ToList();

                            if (lstExceptDateFinal != null && lstExceptDateFinal.Count > 0)
                                exceptDate = string.Join(",", lstExceptDateFinal);
                        }
                    }
                    else
                        exceptDate = exceptDateField.FeedKeyValue;
                }

                if (startDate != null)
                {
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@EventId", SqlDbType = SqlDbType.BigInt, Value = eventId });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@StartDate", SqlDbType = SqlDbType.Date, Value = startDate });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@EndDate", SqlDbType = SqlDbType.Date, Value = endDate ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@StartTime", SqlDbType = SqlDbType.Time, Value = startTime ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@EndTime", SqlDbType = SqlDbType.Time, Value = endTime ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Frequency", SqlDbType = SqlDbType.NVarChar, Value = frequency ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@ByDay", SqlDbType = SqlDbType.NVarChar, Value = byDay ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@ByMonth", SqlDbType = SqlDbType.NVarChar, Value = byMonth ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@ByMonthDay", SqlDbType = SqlDbType.NVarChar, Value = byMonthDay ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@RepeatCount", SqlDbType = SqlDbType.Int, Value = repeatCount ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@RepeatFrequency", SqlDbType = SqlDbType.NVarChar, Value = repeatFrequency ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@ExceptDate", SqlDbType = SqlDbType.NVarChar, Value = exceptDate ?? DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@EventScheduleId", SqlDbType = SqlDbType.BigInt, Direction = ParameterDirection.Output });

                    int rowsAffected = DBProvider.ExecuteNonQuery("EventSchedule_Insert_v1", CommandType.StoredProcedure, ref lstSqlParameter);

                    var eventScheduleIdParam = lstSqlParameter.Where(x => x.ParameterName == "@EventScheduleId").FirstOrDefault().Value;

                    eventScheduleId = eventScheduleIdParam == DBNull.Value ? null : (long?)eventScheduleIdParam;
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertErrorLogs("[DataLaundryScheduler] FeedHelper", "InsertEventSchedule", ex.Message, ex.InnerException?.Message, ex.StackTrace, feedProviderId);
            }
            return eventScheduleId;
        }

        public static bool InsertCustomFeed(long eventId, List<FeedMapping> lstFeedMapping, long? feedProviderId = null)
        {
            int count = 0;
            try
            {
                if (lstFeedMapping != null && lstFeedMapping.Count > 0)
                {
                    foreach (var feedMapping in lstFeedMapping)
                    {
                        if (feedMapping.IsCustomFeedKey)
                        {
                            var lstSqlParameter = new List<SqlParameter>();

                            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@EventId", SqlDbType = SqlDbType.BigInt, Value = eventId });
                            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@ColumnName", SqlDbType = SqlDbType.NVarChar, Value = feedMapping.ColumnName });
                            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Value", SqlDbType = SqlDbType.NVarChar, Value = feedMapping.FeedKeyValue ?? DBNull.Value });

                            count += DBProvider.ExecuteNonQuery("CustomFeedData_Insert", CommandType.StoredProcedure, ref lstSqlParameter);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertErrorLogs("[DataLaundryScheduler] FeedHelper", "InsertEventSchedule", ex.Message, ex.InnerException?.Message, ex.StackTrace, feedProviderId);
            }
            return count > 0;
        }

        private static object GetFeedKeyValue(List<FeedMapping> lstFeedMapping, string columnName)
        {
            object selectedObject = null;
            var selectedFeedMapping = lstFeedMapping?.Where(x => x.ColumnName == columnName).FirstOrDefault();

            if (selectedFeedMapping != null)
                selectedObject = selectedFeedMapping.FeedKeyValue;

            return selectedObject;
        }

        private static FeedMapping GetFeedMapping(List<FeedMapping> lstFeedMapping, string columnName)
        {
            var selectedFeedMapping = lstFeedMapping?.Where(x => x.ColumnName == columnName).FirstOrDefault();

            return selectedFeedMapping;
        }

        private static bool InsertEventOccurrence(long eventId, long? feedProviderId = null)
        {
            long? rowsAffected = 0;
            try
            {
                var lstSqlParameter = new List<SqlParameter>();

                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@EventId", SqlDbType = SqlDbType.BigInt, Value = eventId });
                lstSqlParameter.Add(new SqlParameter() { ParameterName = "@OccCount", SqlDbType = SqlDbType.BigInt, Value = Convert.ToInt32(Settings.GetAppSettingOccurrenceCount() ?? "12") });

                //rowsAffected = DBProvider.ExecuteNonQuery("EventOccurrence_Insert", CommandType.StoredProcedure, ref lstSqlParameter);
                rowsAffected = DBProvider.ExecuteNonQuery("EventOccurrence_Insert_v1", CommandType.StoredProcedure, ref lstSqlParameter);
            }
            catch (Exception ex)
            {
                LogHelper.InsertErrorLogs("[DataLaundryScheduler] FeedHelper", "InsertEventOccurrence", ex.Message, ex.InnerException?.Message, ex.StackTrace, feedProviderId);
            }
            return rowsAffected > 0;
        }
        public static  bool  SchedulerSettingsDisable(int feedProviderId)
        {
        int rowAffected = 0;


            var lstSqlParameter = new List<SqlParameter>();
            
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@FeedProviderId", SqlDbType = SqlDbType.Int, Value = feedProviderId });

             rowAffected = DBProvider.ExecuteNonQuery("SchedulerSettingsDisable", CommandType.StoredProcedure, ref lstSqlParameter);
             return rowAffected > 0 ;
        }
    }
}
