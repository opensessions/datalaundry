using DataLaundryDAL.Constants;
using DataLaundryDAL.Utilities;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;

namespace DataLaundryDAL
{
    public class CommonFunctions
    {
        private static string EncryptionDecryptionKey = "$$#D@+@|_@undry##D@+@|_@undry#$$";

        private static IHttpContextAccessor HttpContextAccessor;          
        public static void Configure(IHttpContextAccessor httpContextAccessor)
        {
            HttpContextAccessor = httpContextAccessor;
        }
        public static void AddProperty(ExpandoObject expando, string propertyName, object propertyValue)
        {
            // ExpandoObject supports IDictionary so we can extend it like this
            var expandoDict = expando as IDictionary<string, object>;

            if (expandoDict.ContainsKey(propertyName))
                expandoDict[propertyName] = propertyValue;
            else
                expandoDict.Add(propertyName, propertyValue);
        }

        public static bool IsPropertyExists(dynamic objDynamic, string name)
        {
            if (objDynamic is ExpandoObject)
                return ((IDictionary<string, object>)objDynamic).ContainsKey(name);

            return objDynamic.GetType().GetProperty(name) != null;
        }

        public static object GetPropertyValue(ExpandoObject expando, string propertyName)
        {
            var expandoDict = expando as IDictionary<string, object>;

            if (expandoDict.ContainsKey(propertyName))
                return expandoDict[propertyName];

            return null;
        }

      
        public static string GetCustomJsonPath(string jsonPath, string replacementChars = " > ")
        {
            if (!string.IsNullOrEmpty(jsonPath))
            {
                string customJsonPath = Regex.Replace(jsonPath, @"\[(\d+)\]\.", replacementChars);
                customJsonPath = customJsonPath.Replace(".", replacementChars);

                return customJsonPath;
            }
            return "";
        }

        public static string FullyQualifiedAppUrl()
        {
             string appPath = string.Empty;
            var httpRequestBase =  HttpContextAccessor.HttpContext.Request;
            if (httpRequestBase != null)
            {
                //Formatting the fully qualified website url/name
               // appPath = httpRequestBase.ApplicationPath;
              appPath = string.Format("{0}://{1}",
                            httpRequestBase.Scheme,
                            httpRequestBase.Host
                           );
            }

            if (!appPath.EndsWith("/"))
            {
                appPath += "/";
            }
            return appPath;
        }

        public static string ReplaceFirstOccurrence(string Source, string Find, string Replace)
        {
            int Place = Source.IndexOf(Find);
            string result = Source.Remove(Place, Find.Length).Insert(Place, Replace);
            return result;
        }

        public static string ReplaceLastOccurrence(string Source, string Find, string Replace)
        {
            int Place = Source.LastIndexOf(Find);
            string result = Source.Remove(Place, Find.Length).Insert(Place, Replace);
            return result;
        }

        public static string StringToBase64(string Text)
        {
            string base64string = string.Empty;

            byte[] Objbyte = Encoding.UTF8.GetBytes(Text);
            // convert the byte array to a Base64 string

            base64string = Convert.ToBase64String(Objbyte);
            return base64string;
        }

        public static string Base64ToString(string Base64Text)
        {
            string text = String.Empty;

            byte[] ObjByte = Convert.FromBase64String(Base64Text);

            text = Encoding.UTF8.GetString(ObjByte);

            return text;
        }

        #region Encryption and Decryption
        public static string EncyptData(string Text)
        {
            EncryptDecrypt service = new EncryptDecrypt();
            var key = Encoding.ASCII.GetBytes(EncryptionDecryptionKey);

            var plainText = Text;
            var encryptedData = service.EncryptStringToBytes(key, plainText);
            string StrPwdFromByte = ByteArrayToString(encryptedData);
            return StrPwdFromByte;
        }
        public static string DecryptData(string Text)
        {
            EncryptDecrypt service = new EncryptDecrypt();
            var key = Encoding.ASCII.GetBytes(EncryptionDecryptionKey);
            Byte[] BytePassword = StringToByteArray(Text);
            var decryptedData = service.DecryptBytesToString(key, BytePassword);
            return (decryptedData);
        }
        #endregion

        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        public static byte[] StringToByteArray(String hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }
        public static DateTime ParseStringToDateTime(string strDateTime, string format = "dd/MM/yyyy")
        {
            if (string.IsNullOrEmpty(strDateTime))
                return DateTime.MinValue;
            try
            {
                return DateTime.ParseExact(strDateTime, format, CultureInfo.InvariantCulture, DateTimeStyles.None);
            }
            catch (Exception)
            {
                return DateTime.MinValue;
            }
        }

        public static string ParseDateTimeToString(DateTime dt, string format = "dd/MM/yyyy")
        {
            try
            {
                return dt.ToString(format, CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                // ignored
            }

            return null;
        }
        public static string ConvertToLocalTimeAndFormat(long UnixTimeStamp, string format = "")
        {
            //var o = HttpContext.Current.Session["tzo"];
            var o=0;
            var tzo = o == null ? 0 : Convert.ToDouble(o);


            DateTime dt = new DateTime();
            if (UnixTimeStamp > 0)
            {
                dt = UnixTimeStampToDateTime(UnixTimeStamp);
            }

            dt = dt.AddMinutes(-1 * tzo);

            if (string.IsNullOrEmpty(format))
            {
                format = Settings.DefaultDateTimeFormat;
            }

            var s = dt.ToString(format);

            if (tzo == 0)
                s += " ";

            return s;
        }

        public static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
    }
}
