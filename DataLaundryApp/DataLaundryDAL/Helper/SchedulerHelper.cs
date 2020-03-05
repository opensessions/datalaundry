using DataLaundryDAL.Constants;
using DataLaundryDAL.DTO;
using DataLaundryDAL.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DataLaundryDAL.Helper
{
    public class SchedulerHelper
    {
        public static List<MasterData> GetSchedulerFrequency()
        {
            var lstSchedulerFrequency = new List<MasterData>();
            var lstSqlParameter = new List<SqlParameter>();
            var dt = DBProvider.GetDataTable("GetSchedulerFrequencies", CommandType.StoredProcedure, ref lstSqlParameter);

            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    var feedDataType = new MasterData()
                    {
                        Id = Convert.ToInt32(row["Id"]),
                        Name = Convert.ToString(row["Name"])
                    };

                    lstSchedulerFrequency.Add(feedDataType);
                }
            }

            return lstSchedulerFrequency;
        }

        public static List<MasterData> GetDaysInWeek()
        {
            var lstDay = new List<MasterData>();

            for (int i = 0; i < 7; i++)
            {
                string name = "";
                switch(i)
                {
                    case 0:
                        name = "Sunday";
                        break;
                    case 1:
                        name = "Monday";
                        break;
                    case 2:
                        name = "Tuesday";
                        break;
                    case 3:
                        name = "Wednesday";
                        break;
                    case 4:
                        name = "Thursday";
                        break;
                    case 5:
                        name = "Friday";
                        break;
                    case 6:
                        name = "Saturday";
                        break;
                }

                lstDay.Add(new MasterData()
                {
                    Name = name
                });
            }
            
            return lstDay;
        }

        public static List<MasterData> GetMonths()
        {
            var lstMonth = new List<MasterData>();

            for (int i = 0; i < 12; i++)
            {
                string name = "";
                switch (i)
                {
                    case 0:
                        name = "January";
                        break;
                    case 1:
                        name = "February";
                        break;
                    case 2:
                        name = "March";
                        break;
                    case 3:
                        name = "April";
                        break;
                    case 4:
                        name = "May";
                        break;
                    case 5:
                        name = "June";
                        break;
                    case 6:
                        name = "July";
                        break;
                    case 7:
                        name = "August";
                        break;
                    case 8:
                        name = "September";
                        break;
                    case 9:
                        name = "October";
                        break;
                    case 10:
                        name = "November";
                        break;
                    case 11:
                        name = "December";
                        break;

                }

                lstMonth.Add(new MasterData()
                {
                    Name = name
                });
            }

            return lstMonth;
        }

        public static List<MasterData> GetDatesInMonths()
        {
            var lstDates = new List<MasterData>();

            for (int i = 1; i <= 31; i++)
            {
                lstDates.Add(new MasterData()
                {
                    Name = i.ToString()
                });
            }

            lstDates.Add(new MasterData()
            {
                Name = "Last"
            });

            return lstDates;
        }

        public static List<MasterData> GetWeekNumbersInMonth()
        {
            var lstWeekNumber = new List<MasterData>();

            for (int i = 1; i <= 5; i++)
            {
                string name = "";
                int weekNumber = i;

                switch (i)
                {
                    case 1:
                        name = "First";
                        break;
                    case 2:
                        name = "Second";
                        break;
                    case 3:
                        name = "Third";
                        break;
                    case 4:
                        name = "Fourth";
                        break;
                    case 5:
                        name = "Last";
                        weekNumber = -1;
                        break;
                }

                lstWeekNumber.Add(new MasterData()
                {
                    Name = name,
                    Id = weekNumber
                });
            }

            return lstWeekNumber;
        }

        public static SchedulerSettings GetSchedulerSettingsByFeedProvider(int feedProviderId)
        {
            SchedulerSettings schedulerSettings = null;

            var lstSqlParameter = new List<SqlParameter>();
            
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@FeedProviderId", SqlDbType = SqlDbType.Int, Value = feedProviderId });

            var dt = DBProvider.GetDataTable("GetSchedulerSettingsByFeedProvider", CommandType.StoredProcedure, ref lstSqlParameter);

            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    var feedProvider = new FeedProvider()
                    {
                        Id = Convert.ToInt32(row["FeedProviderId"]),
                        Name = Convert.ToString(row["Name"]),
                        Source = Convert.ToString(row["Source"]),
                        IsIminConnector = Convert.ToBoolean(row["IsIminConnector"]),
                        DataType = new FeedDataType()
                        {
                            Id = Convert.ToInt32(row["FeedDataTypeId"]),
                            Name = Convert.ToString(row["FeedDataTypeName"])
                        },
                        EndpointUp = Convert.ToBoolean(row["EndpointUp"]),
                        UsesPagingSpec = Convert.ToBoolean(row["UsesPagingSpec"]),
                        IsOpenActiveCompatible = Convert.ToBoolean(row["IsOpenActiveCompatible"]),
                        IncludesCoordinates = Convert.ToBoolean(row["IncludesCoordinates"]),
                        IsFeedMappingConfirmed = Convert.ToBoolean(row["IsFeedMappingConfirmed"])
                    };
                    
                    if(row["Id"] != DBNull.Value)
                    {
                        schedulerSettings = new SchedulerSettings()
                        {
                            Id = Convert.ToInt32(row["Id"]),
                            FeedProvider = feedProvider,
                            StartDateTime = Convert.ToDateTime(row["StartDateTime"]),
                            liStartDateTime = Convert.ToInt64(row["StartDateTimeStamp"]),
                            IsEnabled = Convert.ToBoolean(row["IsEnabled"]),
                            SchedulerFrequencyId = Convert.ToInt32(row["SchedulerFrequencyId"])
                        };
                        
                        if (row["ExpiryDateTime"] != DBNull.Value)
                        {
                            schedulerSettings.ExpiryDateTime = Convert.ToDateTime(row["ExpiryDateTime"]);
                            schedulerSettings.liExpiryDateTime = Convert.ToInt64(row["ExpiryDateTimeStamp"]);
                        }

                        if (row["RecurranceInterval"] != DBNull.Value)
                            schedulerSettings.RecurranceInterval = Convert.ToInt32(row["RecurranceInterval"]);
                        
                        if (row["RecurranceDaysInWeek"] != DBNull.Value)
                            schedulerSettings.RecurranceDaysInWeek = Convert.ToString(row["RecurranceDaysInWeek"]);

                        if (row["RecurranceMonths"] != DBNull.Value)
                            schedulerSettings.RecurranceMonths = Convert.ToString(row["RecurranceMonths"]);

                        if (row["RecurranceDatesInMonth"] != DBNull.Value)
                            schedulerSettings.RecurranceDatesInMonth = Convert.ToString(row["RecurranceDatesInMonth"]);

                        if (row["RecurranceWeekNos"] != DBNull.Value)
                            schedulerSettings.RecurranceWeekNos = Convert.ToString(row["RecurranceWeekNos"]);

                        if (row["RecurranceDaysInWeekForMonth"] != DBNull.Value)
                            schedulerSettings.RecurranceDaysInWeekForMonth = Convert.ToString(row["RecurranceDaysInWeekForMonth"]);
                    }
                    else
                    {
                        schedulerSettings = new SchedulerSettings()
                        {
                            FeedProvider = feedProvider
                        };
                    }
                    
                    break;
                }
            }

            return schedulerSettings;
        }

        public static  bool  SchedulerSettingsDisable(int feedProviderId)
        {
        int rowAffected = 0;


            var lstSqlParameter = new List<SqlParameter>();
            
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@FeedProviderId", SqlDbType = SqlDbType.Int, Value = feedProviderId });

             rowAffected = DBProvider.ExecuteNonQuery("SchedulerSettingsDisable", CommandType.StoredProcedure, ref lstSqlParameter);
             return rowAffected > 0 ;
        }
        public static bool InsertUpdateScheduleSettings(SchedulerSettings schedulerSettings)
        {
            var lstSqlParameter = new List<SqlParameter>();

            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Id", SqlDbType = SqlDbType.Int, Value = schedulerSettings.Id });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@FeedProviderId", SqlDbType = SqlDbType.Int, Value = schedulerSettings.FeedProvider.Id });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@StartDateTime", SqlDbType = SqlDbType.DateTime, Value = schedulerSettings.StartDateTime });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@ExpiryDateTime", SqlDbType = SqlDbType.DateTime, Value = (object)schedulerSettings.ExpiryDateTime ?? DBNull.Value });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@IsEnabled", SqlDbType = SqlDbType.Bit, Value = schedulerSettings.IsEnabled });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@SchedulerFrequencyId", SqlDbType = SqlDbType.Int, Value = schedulerSettings.SchedulerFrequencyId });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@RecurranceInterval", SqlDbType = SqlDbType.NVarChar, Value = (object)schedulerSettings.RecurranceInterval ?? DBNull.Value });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@RecurranceDaysInWeek", SqlDbType = SqlDbType.NVarChar, Value = (object)schedulerSettings.RecurranceDaysInWeek ?? DBNull.Value });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@RecurranceMonths", SqlDbType = SqlDbType.NVarChar, Value = (object)schedulerSettings.RecurranceMonths ?? DBNull.Value });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@RecurranceDatesInMonth", SqlDbType = SqlDbType.NVarChar, Value = (object)schedulerSettings.RecurranceDatesInMonth ?? DBNull.Value });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@RecurranceWeekNos", SqlDbType = SqlDbType.NVarChar, Value = (object)schedulerSettings.RecurranceWeekNos ?? DBNull.Value });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@RecurranceDaysInWeekForMonth", SqlDbType = SqlDbType.NVarChar, Value = (object)schedulerSettings.RecurranceDaysInWeekForMonth ?? DBNull.Value });

            int rowsAffected = DBProvider.ExecuteNonQuery("SchedulerSettings_InsertUpdate", CommandType.StoredProcedure, ref lstSqlParameter);
            return rowsAffected > 0;
        }

        public static List<SchedulerLog> GetSchedulerLogByFeedProvider(int feedProviderId)
        {
            var schedulerLogs = new List<SchedulerLog>();
            var lstSqlParameter = new List<SqlParameter>();
            var dt = DBProvider.GetDataTable("GetScheduledJobLogForFeedProvider", CommandType.StoredProcedure, ref lstSqlParameter);

            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    var schedulerJobLog = new SchedulerLog();
                    schedulerJobLog.Id = Convert.ToInt32(row["Id"]);
                    schedulerJobLog.FeedProviderId = Convert.ToInt32(row["FeedProviderId"]);
                    if(row["StartDate"] != DBNull.Value)
                        schedulerJobLog.StartDate = Convert.ToDateTime(row["StartDate"]).ToString("dd/MM/yyyy hh:mm:ss"); ;
                    if (row["EndDate"] != DBNull.Value)
                        schedulerJobLog.EndDate = Convert.ToDateTime(row["EndDate"]).ToString("dd/MM/yyyy hh:mm:ss"); ;
                    schedulerJobLog.Status = Convert.ToString(row["Status"]);

                    schedulerLogs.Add(schedulerJobLog);
                }
            }
            return schedulerLogs;
        }

        public static DataTableResponse GetSchedulerLogByFeedProvider(DataTableRequest dataTableRequest, long feedProviderId)
        {
            var dataTableResponse = new DataTableResponse();
            var schedulerLogs = new List<SchedulerLog>();

            var lstSqlParameter = new List<SqlParameter>();
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Offset", SqlDbType = SqlDbType.Int, Value = dataTableRequest.PageNo });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@pageSize", SqlDbType = SqlDbType.Int, Value = dataTableRequest.PageSize });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@sortColumnNo", SqlDbType = SqlDbType.Int, Value = dataTableRequest.SortField });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@sortDirection", SqlDbType = SqlDbType.NVarChar, Value = dataTableRequest.SortOrder });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@searchText", SqlDbType = SqlDbType.NVarChar, Value = dataTableRequest.Filter });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@feedProviderId", SqlDbType = SqlDbType.BigInt, Value = feedProviderId });

            var ds = DBProvider.GetDataSet("GetScheduledJobLogForFeedProvider", CommandType.StoredProcedure, ref lstSqlParameter);

            foreach (DataRow row in ds?.Tables[0]?.Rows)
            {
                var schedulerLog = new SchedulerLog();
                schedulerLog.Id = Convert.ToInt32(row["Id"]);
                schedulerLog.FeedProviderId = Convert.ToInt32(row["FeedProviderId"]);
                schedulerLog.Status = Convert.ToString(row["Status"]);

                //DateTimeOffset? StartDate = DateTime.SpecifyKind(Convert.ToDateTime(row["StartDate"]), DateTimeKind.Utc);
                //if (!string.IsNullOrEmpty(StartDate?.ToString()))
                //    schedulerLog.StartDate = StartDate?.UtcDateTime.ToString("yyyy-MM-dd hh:mm:ss");
                //DateTimeOffset? EndDate = DateTime.SpecifyKind(Convert.ToDateTime(row["EndDate"]), DateTimeKind.Utc);
                //if (!string.IsNullOrEmpty(EndDate?.ToString()))
                //    schedulerLog.EndDate = EndDate?.UtcDateTime.ToString("yyyy-MM-dd hh:mm:ss");

                //if (row["StartDate"] != DBNull.Value)
                //    schedulerLog.StartDate = DateTime.SpecifyKind(DateTime.Parse(row["StartDate"].ToString()), DateTimeKind.Utc).ToLocalTime().ToString("yyyy-MM-dd hh:mm:ss tt");
                //if (row["EndDate"] != DBNull.Value)
                //    schedulerLog.EndDate = DateTime.SpecifyKind(DateTime.Parse(row["EndDate"].ToString()), DateTimeKind.Utc).ToLocalTime().ToString("yyyy-MM-dd hh:mm:ss tt");

                if (row["StartDate"] != DBNull.Value)
                    schedulerLog.StartDate = DateTime.SpecifyKind(DateTime.Parse(row["StartDate"].ToString()), DateTimeKind.Utc).ToString("yyyy-MM-dd hh:mm:ss tt");
                if (row["EndDate"] != DBNull.Value)
                    schedulerLog.EndDate = DateTime.SpecifyKind(DateTime.Parse(row["EndDate"].ToString()), DateTimeKind.Utc).ToString("yyyy-MM-dd hh:mm:ss tt");
                if (row["AffectedEvents"] != DBNull.Value)
                    schedulerLog.AffectedEvents = Convert.ToInt64(row["AffectedEvents"]);

                schedulerLogs.Add(schedulerLog);
            }
            dataTableResponse.totalNumberofRecord = Convert.ToInt32(ds.Tables[1]?.Rows[0][0]);
            dataTableResponse.filteredRecord = Convert.ToInt32(ds.Tables[2]?.Rows[0][0]);
            dataTableResponse.data = schedulerLogs;
            return dataTableResponse;
        }
    }
}
