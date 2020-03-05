using DataLaundryDAL.DTO;
using DataLaundryDAL.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DataLaundryDAL.Helper
{
    public class FeedHelper
    {
        public static bool Insert(dynamic objDynamicData, string tableName)
        {
            int rowsAffected = 0;
            var lstSqlParameter = new List<SqlParameter>();
            string commandText = "";

            switch (tableName)
            {
                case "Event":
                    commandText = $"{tableName}_Insert";

                    var name = CommonFunctions.IsPropertyExists(objDynamicData, "name") ? ((object)objDynamicData.name ?? DBNull.Value) : DBNull.Value;
                    var description = CommonFunctions.IsPropertyExists(objDynamicData, "description") ? ((object)objDynamicData.description ?? DBNull.Value) : DBNull.Value;
                    var startDate = CommonFunctions.IsPropertyExists(objDynamicData, "startDate") ? ((object)objDynamicData.startDate ?? DBNull.Value) : DBNull.Value;
                    var endDate = CommonFunctions.IsPropertyExists(objDynamicData, "endDate") ? ((object)objDynamicData.endDate ?? DBNull.Value) : DBNull.Value;
                    var duration = CommonFunctions.IsPropertyExists(objDynamicData, "duration") ? ((object)objDynamicData.duration ?? DBNull.Value) : DBNull.Value;
                    var maximumAttendeeCapacity = CommonFunctions.IsPropertyExists(objDynamicData, "maximumAttendeeCapacity") ? ((object)objDynamicData.maximumAttendeeCapacity ?? DBNull.Value) : DBNull.Value;
                    var remainingAttendeeCapacity = CommonFunctions.IsPropertyExists(objDynamicData, "remainingAttendeeCapacity") ? ((object)objDynamicData.remainingAttendeeCapacity ?? DBNull.Value) : DBNull.Value;
                    var eventStatus = CommonFunctions.IsPropertyExists(objDynamicData, "eventStatus") ? ((object)objDynamicData.eventStatus ?? DBNull.Value) : DBNull.Value;
                    var ageRange = CommonFunctions.IsPropertyExists(objDynamicData, "ageRange") ? ((object)objDynamicData.ageRange ?? DBNull.Value) : DBNull.Value;
                    var genderRestriction = CommonFunctions.IsPropertyExists(objDynamicData, "genderRestriction") ? ((object)objDynamicData.genderRestriction ?? DBNull.Value) : DBNull.Value;
                    var programme = CommonFunctions.IsPropertyExists(objDynamicData, "programme") ? ((object)objDynamicData.programme ?? DBNull.Value) : DBNull.Value;
                    var attendeeInstructions = CommonFunctions.IsPropertyExists(objDynamicData, "attendeeInstructions") ? ((object)objDynamicData.attendeeInstructions ?? DBNull.Value) : DBNull.Value;
                    var isCoached = CommonFunctions.IsPropertyExists(objDynamicData, "isCoached") ? ((object)objDynamicData.isCoached ?? DBNull.Value) : DBNull.Value;
                    var level = CommonFunctions.IsPropertyExists(objDynamicData, "level") ? ((object)objDynamicData.level ?? DBNull.Value) : DBNull.Value;
                    var meetingPoint = CommonFunctions.IsPropertyExists(objDynamicData, "meetingPoint") ? ((object)objDynamicData.meetingPoint ?? DBNull.Value) : DBNull.Value;
                    var identifier = CommonFunctions.IsPropertyExists(objDynamicData, "identifier") ? ((object)objDynamicData.identifier ?? DBNull.Value) : DBNull.Value;
                    var url = CommonFunctions.IsPropertyExists(objDynamicData, "url") ? ((object)objDynamicData.url ?? DBNull.Value) : DBNull.Value;

                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Name", SqlDbType = SqlDbType.NVarChar, Value = name });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Description", SqlDbType = SqlDbType.NVarChar, Value = description });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Image", SqlDbType = SqlDbType.NVarChar, Value = DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@StartDate", SqlDbType = SqlDbType.DateTime, Value = startDate });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@EndDate", SqlDbType = SqlDbType.DateTime, Value = endDate });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Duration", SqlDbType = SqlDbType.NVarChar, Value = duration });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Location", SqlDbType = SqlDbType.NVarChar, Value = DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Organizer", SqlDbType = SqlDbType.NVarChar, Value = DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Contributor", SqlDbType = SqlDbType.NVarChar, Value = DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@MaximumAttendeeCapacity", SqlDbType = SqlDbType.Int, Value = maximumAttendeeCapacity });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@RemainingAttendeeCapacity", SqlDbType = SqlDbType.Int, Value = remainingAttendeeCapacity });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@EventStatus", SqlDbType = SqlDbType.NVarChar, Value = eventStatus });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@SuperEvent", SqlDbType = SqlDbType.NVarChar, Value = DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@SubEvent", SqlDbType = SqlDbType.NVarChar, Value = DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Schedule", SqlDbType = SqlDbType.NVarChar, Value = DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Activity", SqlDbType = SqlDbType.NVarChar, Value = DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Category", SqlDbType = SqlDbType.NVarChar, Value = DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@AgeRange", SqlDbType = SqlDbType.NVarChar, Value = ageRange });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@GenderRestriction", SqlDbType = SqlDbType.NVarChar, Value = genderRestriction });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Programme", SqlDbType = SqlDbType.NVarChar, Value = programme });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@AttendeeInstructions", SqlDbType = SqlDbType.NVarChar, Value = attendeeInstructions });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Leader", SqlDbType = SqlDbType.NVarChar, Value = DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@AccessibilitySupport", SqlDbType = SqlDbType.NVarChar, Value = DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@AccessibilityInformation", SqlDbType = SqlDbType.NVarChar, Value = DBNull.Value });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@IsCoached", SqlDbType = SqlDbType.NVarChar, Value = isCoached });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Level", SqlDbType = SqlDbType.NVarChar, Value = level });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@MeetingPoint", SqlDbType = SqlDbType.NVarChar, Value = meetingPoint });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Identifier", SqlDbType = SqlDbType.NVarChar, Value = identifier });
                    lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Url", SqlDbType = SqlDbType.NVarChar, Value = url });

                    break;
                default:
                    break;
            }

            if (!string.IsNullOrEmpty(commandText) && lstSqlParameter.Count > 0)
                rowsAffected = DBProvider.ExecuteNonQuery(commandText, CommandType.StoredProcedure, ref lstSqlParameter);

            return rowsAffected > 0;
        }
    }
}
