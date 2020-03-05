using DataLaundryDAL.DTO;
using DataLaundryDAL.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLaundryDAL.Helper
{
    public class UserHelper
    {
        public static User Login(string email, string password)
        {
            User user = null;
            var lstSqlParameter = new List<SqlParameter>();

            string encPassword = CommonFunctions.EncyptData(password);

            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Email", SqlDbType = SqlDbType.NVarChar, Value = email });
            lstSqlParameter.Add(new SqlParameter() { ParameterName = "@Password", SqlDbType = SqlDbType.NVarChar, Value = encPassword });

            var dt = DBProvider.GetDataTable("ValidateAdminLogin", CommandType.StoredProcedure, ref lstSqlParameter);

            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    user = new User()
                    {
                        Id = Convert.ToInt32(row["Id"]),
                        Name = row["Name"].ToString(),
                        Email = row["Email"].ToString(),
                    };
                    break;
                }
            }
            return user;
        }
    }
}
