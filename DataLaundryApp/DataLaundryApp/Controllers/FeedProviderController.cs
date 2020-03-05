using DataLaundryApp.Extentions;
using DataLaundryApp.Filters;
using DataLaundryApp.ViewModels;
using DataLaundryDAL;
using DataLaundryDAL.Constants;
using DataLaundryDAL.DTO;
using DataLaundryDAL.Helper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using DataLaundryApp.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using DataLaundryApp.Common;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading;
using System.Text;
using System.Configuration;
using System.ComponentModel;
using System.Net.NetworkInformation; 
namespace DataLaundryApp.Controllers
{
    [Authorise]
    [ExceptionHandlerAttribute]
    //[NoCache]
    public class FeedProviderController : Controller
    {
        private readonly IHostingEnvironment _hostingEnvironment;
    string Recipient1 =DataLaundryDAL.Constants.Settings.GetSMTP_R1();
    string Recipient2 =DataLaundryDAL.Constants.Settings.GetSMTP_R2();
    string Email =DataLaundryDAL.Constants.Settings.GetSMTP_Mail();
    string Credential =Settings.GetSMTP_Credential();

 
        public FeedProviderController(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        #region Feed Provider methods
        public ActionResult Index()
        {

           
            return View();
        }
        
        [HttpGet]
        public ActionResult GetFeedProviders(JQueryDataTableParamModel param)
        {

           var dataTableResponse = new DataTableResponse();
           
            //try
            //{
            string search = ""; //It's indicate blank filter

            if (!string.IsNullOrEmpty(param.sSearch))
                search = param.sSearch;

            int offset = 0;
            int pageSize = param.iDisplayLength;

            //Find page number from the logic
            if (param.iDisplayStart > 0)
            {
                offset = param.iDisplayStart / pageSize;
            }
            var dataTableRequest = new DataTableRequest();
            dataTableRequest.PageNo = offset;
            dataTableRequest.PageSize = pageSize;
            dataTableRequest.Filter = search.Trim();
            dataTableRequest.SortField = param.iSortCol_0;
            dataTableRequest.SortOrder = param.sSortDir_0;
            dataTableResponse = FeedProviderHelper.GetFeedProviders(dataTableRequest);
           // var list=new List<dataTableResponse>();
           //  var l=JsonConvert.DeserializeObject<List<FeedProvider>>(dataTableResponse.data);

            var dataList = (List<FeedProvider>)dataTableResponse.data;
           // getting invalid url 
            foreach (var item in dataList)
            {
                var name = item.Name;
                var id = item.Id;
                var IsSchedulerEnabled = item.IsSchedulerEnabled;
                var urlName = item.Source;
                var request = (HttpWebRequest)WebRequest.Create(new Uri(item.Source));
                if(IsSchedulerEnabled !=  false)
                {

                request.Method = WebRequestMethods.Http.Get;

                try
                {
                    // var response = (HttpWebResponse)request.GetResponse();
                    // if (response.StatusCode == HttpStatusCode.OK)
                    // {
                    //    continue;
                    // }
                 
                }
            catch (WebException ex)
                {
                
                // if(IsSchedulerEnabled !=  false)
                // {

                // MailMessage mail = new MailMessage();
                // SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
 
                // mail.From = new MailAddress(Email);
                // mail.To.Add(Recipient1);
                // mail.To.Add(Recipient2);
                
                // mail.Subject ="Data Laundry Feed Alert";
                // StringBuilder mailBody = new StringBuilder();
                // mailBody.AppendFormat("<h2>Data Laundry Feed Alert</h2>");
                // mailBody.AppendFormat("<td><b><span style = font-family:Arial;font-size:13pt>Dear Data Laundry Admin,</span></b></td>");
                // mailBody.AppendFormat("<br/>");
                // mailBody.AppendFormat("<p>The following feed is currently being disabled as it is not providing appropriate data anymore.</p>");
                // mailBody.AppendFormat("<p><b>Feed Name:</b> "+name );
                // mailBody.AppendFormat("<br/>");
                // mailBody.AppendFormat("<p><b>Feed URL:</b>"+" " +urlName );
                // mailBody.AppendFormat("<br/>");
                // mailBody.AppendFormat("<br/>");
                // mailBody.AppendFormat("Thanks & Regards");
                // mailBody.AppendFormat("<br/>");
                // mailBody.AppendFormat("Data Laundry");
                // mailBody.AppendFormat("<br/>");
                // mailBody.AppendFormat("<img width=70 width=70 src=https://i.ibb.co/K5xjY1n/data-laundry.png >");
                // mail.Body = mailBody.ToString();
                // mail.IsBodyHtml =  true;
                // SmtpServer.Port = 25;
                // SmtpServer.Credentials = new System.Net.NetworkCredential("datalaundryalerts", Credential);
                // SmtpServer.EnableSsl = true;
                // SmtpServer.Send(mail);
                // var vmSchedulerSettings = new vmSchedulerSettings();
                // var schedulerSettings = SchedulerHelper.SchedulerSettingsDisable(id);
               //}
                continue;
             }
             }
            }
                return Json(new 
                {
                sEcho = param.sEcho,
                iTotalRecords = dataTableResponse.totalNumberofRecord,
                iTotalDisplayRecords = dataTableResponse.filteredRecord,
                aaData = dataTableResponse.data,
                });

                
            }

        private void FillFeedDataTypes()
        {
            ViewBag.FeedDataTypeList = FeedProviderHelper.GetFeedDataTypes();
        }

        public ActionResult Create()
        {
            FillFeedDataTypes();

            return View();
        }

        [HttpPost]
        public ActionResult Create(vmFeedProvider model)
        {
            bool status = false;
            try
            {
                if (ModelState.IsValid)
                {
                    int feedProviderId = 0;
                   var request = (HttpWebRequest)WebRequest.Create(new Uri(model.Source));

                    request.Method = WebRequestMethods.Http.Get;
                    
                    var response = (HttpWebResponse)request.GetResponse();
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var FeedProvider = new FeedProvider()
                        {
                            Name = model.Name,
                            Source = model.Source,
                            IsIminConnector = model.IsIminConnector,
                            DataType = new FeedDataType()
                            {
                                Id = (int)model.FeedDataTypeId
                            },
                            EndpointUp = model.EndpointUp,
                            UsesPagingSpec = model.UsesPagingSpec,
                            IsOpenActiveCompatible = model.IsOpenActiveCompatible,
                            IncludesCoordinates = model.IncludesCoordinates
                        };

                        status = FeedProviderHelper.InsertFeedProvider(FeedProvider, out feedProviderId);

                        TempData["ResponseStatus"] = status ? "success" : "error";
                        //return RedirectToAction("AnalyzeFeed", new { id = feedProviderId });
                        return RedirectToAction("MapFeed", new { id = feedProviderId });
                    }

                    if (status)
                        TempData["ResponseMsg"] = "Record inserted successfully";
                    else
                        TempData["ResponseMsg"] = "Something went wrong. Please try again soon.";

                    return RedirectToAction("AnalyzeFeed", new { id = feedProviderId });
                }
            }
            catch (WebException ex)
            {
                TempData["ResponseStatus"] = false;
                TempData["ResponseMsg"] = "Invalid URL entered or server is unavailable.";
            }

            FillFeedDataTypes();

            return View();
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            bool status = false;
            string message = "";
            //try
            //{
            bool isDeleted = true;
            status = FeedProviderHelper.DeleteFeedProvider(id, isDeleted);

            if (status)
            {
                message = "Record updated successfully";

                #region Delete JSON files
                //string webRootPath = _hostingEnvironment.WebRootPath;
                string contentRootPath = _hostingEnvironment.ContentRootPath;
                var rootFilePath = string.Concat(contentRootPath, "/", Settings.FeedJSONFilePath);
                //DirectoryInfo hdDirectoryInWhichToSearch = new DirectoryInfo(HttpContext.Request.MapPath("~" + Settings.FeedJSONFilePath));
                DirectoryInfo hdDirectoryInWhichToSearch = new DirectoryInfo(rootFilePath);
                FileInfo[] filesInDir = hdDirectoryInWhichToSearch.GetFiles("*" + id + "*.*");

                foreach (FileInfo foundFile in filesInDir)
                    foundFile.Delete();
                #endregion
            }
            else
                message = "Something went wrong. Please try again soon.";
            //}
            //catch (Exception ex)
            //{
            //    ViewBag.ErrorMessage = ex.Message;
            //}

            // return Json(new
            // {
            //     status,
            //     message
            // }, JsonRequestBehavior.AllowGet);
            return Json(new { status, message });

        }

        public JsonResult IsNameAvailable(string Name)
        {
            bool result = FeedProviderHelper.IsNameAvailable(Name, 0);
            //return Json(result, JsonRequestBehavior.AllowGet);
            return Json(result);
        }

        [HttpPost]
        public JsonResult UpdateFeedProviderName(string name, int Id)
        {
            bool result = FeedProviderHelper.IsNameAvailable(name, Id);
            if (!result)
            {
                FeedProviderHelper.FeedProviderNameUpdate(name, Id);
                //return Json(result, JsonRequestBehavior.AllowGet);
                return Json(result);
            }
            else
            {
                //return Json(result, JsonRequestBehavior.AllowGet);
                return Json(result);
            }
        }
        #endregion Feed Provider methods

        #region Analysis methods
        public ActionResult AnalyzeFeed(int id)
        {
            FeedProvider feedProvider = null;

            //try
            //{
            feedProvider = FeedProviderHelper.GetFeedProviderDetail(id);

            if (feedProvider != null)
            {
                Tuple<FeedProvider, List<IntelligentFeedMapping>>
                
                 tuple = null;
                //Tuple<FeedProvider, List<vmJsTree>> tuple = null;
                var lstIntelligentFeedMapping = new List<IntelligentFeedMapping>();

                string finalUrl = feedProvider.Source;

                string json = GetWebContent(finalUrl);

                var token = JToken.Parse(json);

                //ProcessJson_Test(token);

                //if (feedProvider.IsOpenActiveCompatible)
                //{
                //    //analyze based on open active standard opporunity model
                //    var lstJToken = token.SelectTokens("$.items[?(@.data.type == '" + tableName + "')]").ToList();

                //    if (lstJToken != null && lstJToken.Count > 0)
                //        AnalyzeStandardOpenActiveData(feedProvider, tableName, lstJToken, lstFeedMapping);
                //}
                //else
                //{
                //    //var lstJToken = token.SelectTokens("$.items[*]").ToList();

                //    //if (lstJToken != null && lstJToken.Count > 0)
                //    //    AnalyzeFeedData(feedProvider, tableName, lstJToken, lstFeedMapping);

                //    //analyze based on intelligent mapping of keys
                //    AnalyzeFeedData_V2(feedProvider, tableName, token, lstFeedMapping);
                //}

                //analyze ordering strategy
                AnalyzeOrderingStrategy(feedProvider, token);

                //analyze based on intelligent mapping of keys
                AnalyzeFeedData_V1(feedProvider, token, out lstIntelligentFeedMapping);

                lstIntelligentFeedMapping = lstIntelligentFeedMapping.OrderBy(x => x.ParentId).ToList();

                var lstFeedMapping = lstIntelligentFeedMapping.Select(x => x.FeedMapping).ToList();

                if (!feedProvider.IsFeedMappingConfirmed)
                    FeedConfigHelper.InsertUpdateFeedMapping(lstFeedMapping);

                lstIntelligentFeedMapping = FeedConfigHelper.GetIntelligentMapping(feedProvider.Id);

                //var lstVmJsTree = CreateJsTree(lstIntelligentFeedMapping, isDisableFoundKeys: true);

                //Session[$"JsonTreeDisableFoundKeys_{id}"] = lstVmJsTree;

                //var lstFeedMappingFound = lstIntelligentFeedMapping.Where(x => !string.IsNullOrEmpty(x.FeedMapping.FeedKey) && x.ParentId == null).ToList();
                //var lstFeedMappingNotFound = lstIntelligentFeedMapping.Where(x => string.IsNullOrEmpty(x.FeedMapping.FeedKey) && x.ParentId == null).ToList();

                //tuple = new Tuple<FeedProvider, List<IntelligentFeedMapping>, List<IntelligentFeedMapping>>(feedProvider, lstFeedMappingFound, lstFeedMappingNotFound);
                tuple = new Tuple<FeedProvider, List<IntelligentFeedMapping>>(feedProvider, lstIntelligentFeedMapping);

                //var lstJsTree = CreateJsTree(lstFeedMapping);
                //tuple = new Tuple<FeedProvider, List<vmJsTree>>(feedProvider, lstJsTree);
                return View("FeedAnalysisResult", tuple);
            }
            else
            {
                ViewBag.ErrorMessage = "Feed Provider not found";
            }
            //}
            //catch (Exception ex)
            //{
            //    ViewBag.ErrorMessage = ex.Message;
            //}

            return View("FeedAnalysisResult");
        }

        /// <summary>
        /// Return the list of fields from Feed Mapping which are already analyzed
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult MapFeed(int id)
        {
            FeedProvider feedProvider = null;

            feedProvider = FeedProviderHelper.GetFeedProviderDetail(id);
       
             var urlName = feedProvider.Source;
         
                var request = (HttpWebRequest)WebRequest.Create(new Uri(urlName));
                request.Method = WebRequestMethods.Http.Get;
                try{
                     var response = (HttpWebResponse)request.GetResponse();
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
            if (feedProvider != null)
            {
                Tuple<FeedProvider, List<IntelligentFeedMapping>> tuple = null;
                var lstIntelligentFeedMapping = new List<IntelligentFeedMapping>();

                lstIntelligentFeedMapping = FeedConfigHelper.GetFeedIntelligentMapping(feedProvider.Id);

                tuple = new Tuple<FeedProvider, List<IntelligentFeedMapping>>(feedProvider, lstIntelligentFeedMapping);

                return View("FeedMappingResult", tuple);
            }
            else
            {
                ViewBag.ErrorMessage = "Feed Provider not found";
            }
                    }
            }
            catch(Exception ex){
                    ViewBag.ErrorMessage = "Mapping not found";

            }
            return View("FeedMappingResult");
        }

        /// <summary>
        /// Return the list of fields from Intelligent mapping which are going to be already analyzed
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
       
        public ActionResult AnalyzeFeed_v1(int id)
        {
            FeedProvider feedProvider = null;

            feedProvider = FeedProviderHelper.GetFeedProviderDetail(id);

            if (feedProvider != null)
            {
                Tuple<FeedProvider, List<IntelligentFeedMapping>> tuple = null;
                var lstIntelligentFeedMapping = new List<IntelligentFeedMapping>();

                string finalUrl = feedProvider.Source;

                string json = GetWebContent(finalUrl);

                var token = JToken.Parse(json);

                //var item = GetLimitedDataForAnalyze(token, finalUrl);
                var item = GetLimitedDataForAnalyze_v1(token, finalUrl);
                token["items"] = item;

                //analyze ordering strategy
                AnalyzeOrderingStrategy(feedProvider, token);

                //delete custom feed(s) from feed mapping which are already deleted from intelligent mapping
                FeedConfigHelper.DeleteCustomFeedsByFeedProvider(feedProvider.Id);

                //analyze based on intelligent mapping of keys
                //AnalyzeFeedData_V2(feedProvider, token, out lstIntelligentFeedMapping);
                AnalyzeFeedData_V3(feedProvider, token, out lstIntelligentFeedMapping);

                lstIntelligentFeedMapping = lstIntelligentFeedMapping.OrderBy(x => x.ParentId).ToList();

                var lstFeedMapping = lstIntelligentFeedMapping.Select(x => x.FeedMapping).ToList();

                if (!lstFeedMapping.Any(x => x.FeedProvider == null))
                    FeedConfigHelper.InsertUpdateFeedMapping(lstFeedMapping);

                //lstIntelligentFeedMapping = FeedConfigHelper.GetIntelligentMapping(feedProvider.Id);
                lstIntelligentFeedMapping = FeedConfigHelper.GetIntelligentMapping_v1(feedProvider.Id);

                tuple = new Tuple<FeedProvider, List<IntelligentFeedMapping>>(feedProvider, lstIntelligentFeedMapping);

                return View("FeedMappingResult", tuple);
            }
            else
            {
                ViewBag.ErrorMessage = "Feed Provider not found";
            }

            return View("FeedMappingResult");
        }

        /// <summary>
        /// Get content from given URL
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private string GetWebContent(string url)
        {
            var uri = new Uri(url);
            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = WebRequestMethods.Http.Get;
            var response = (HttpWebResponse)request.GetResponse();
            var reader = new StreamReader(response.GetResponseStream());
            string output = reader.ReadToEnd();
            response.Close();

            return output;
        }

        private void SaveAllJsonKeys(FeedProvider feedProvider, JToken token)
        {
            //save all session variables related to Json keys which can be used at other methods in application 
            //Session[$"SampleJsonItem_{feedProvider.Id}"] = token;
            HttpContext.Session.SetObject($"SampleJsonItem_{feedProvider.Id}", token);
            List<string> lstJsonKeys = token.FindTokenKeys();
            //Session[$"JsonKeyList_{feedProvider.Id}"] = lstJsonKeys;
            HttpContext.Session.SetObject($"JsonKeyList_{feedProvider.Id}", lstJsonKeys);

            //tree can be used to provide data to JsTree plugin on client side
            var lstVmJsTree = CreateJsTree(token);
            // Session[$"JsonTree_{feedProvider.Id}"] = lstVmJsTree;
            HttpContext.Session.SetObject($"JsonTree_{feedProvider.Id}", lstVmJsTree);
            var lstVmJsTree1 = CreateJsTree(token, isDisableParentKeys: true);
            //Session[$"JsonTreeDisableFoundKeys_{feedProvider.Id}"] = lstVmJsTree1;
            HttpContext.Session.SetObject($"JsonTreeDisableFoundKeys_{feedProvider.Id}", lstVmJsTree1);
            //Session["JsonTreeIntelligentMapping"] = null;

            //leaf keys can be used for applying filter conditions
            List<dynamic> lstLeafKeys = token.FindTokenKeys_LeafNodes();
            //Session[$"JsonLeafKeyList_{feedProvider.Id}"] = lstLeafKeys;
            HttpContext.Session.SetObject($"JsonLeafKeyList_{feedProvider.Id}", lstLeafKeys);
        }

        private void SaveAllJsonKeys_v1(FeedProvider feedProvider, JToken token)
        {
            //string webRootPath = _hostingEnvironment.WebRootPath;
            string contentRootPath = _hostingEnvironment.ContentRootPath;
            var rootFilePath = string.Concat(contentRootPath, "/", Settings.FeedJSONFilePath);
            string filePath = "";

            #region Save data into text file

            //save json tree format according to matched field
            var lstVmJsTree = CreateJsTree(token);
            //Session[$"JsonTree_{feedProvider.Id}"] = lstVmJsTree;
            HttpContext.Session.SetObject($"JsonTree_{feedProvider.Id}", lstVmJsTree);
            var jsonTreeFilename = "JsonTree_" + feedProvider.Id + ".json";
            filePath = Path.Combine(rootFilePath, jsonTreeFilename);
            // write JSON directly to a file
            using (StreamWriter file = System.IO.File.CreateText(filePath))
            using (JsonTextWriter writer = new JsonTextWriter(file))
            {
                JArray jObj = JArray.FromObject(lstVmJsTree);
                jObj.WriteTo(writer);
            }

            //save sample json
            //Session[$"SampleJsonItem_{feedProvider.Id}"] = token;
            HttpContext.Session.SetObject($"SampleJsonItem_{feedProvider.Id}", token);
            var sampleJSONFileName = "SampleJsonItem_" + feedProvider.Id + ".json";
            filePath = Path.Combine(rootFilePath, sampleJSONFileName);
            // write JSON directly to a file
            using (StreamWriter file = System.IO.File.CreateText(filePath))
            using (JsonTextWriter writer = new JsonTextWriter(file))
            {
                JObject jObj = JObject.FromObject(token);
                jObj.WriteTo(writer);
            }

            //save json for disabled keys           
            var lstVmJsTreeForDisabledKey = CreateJsTree(token, isDisableParentKeys: true);
            //Session[$"JsonTreeDisableFoundKeys_{feedProvider.Id}"] = lstVmJsTreeForDisabledKey;
            HttpContext.Session.SetObject($"JsonTreeDisableFoundKeys_{feedProvider.Id}", lstVmJsTreeForDisabledKey);
            var JsonTreeDisableKeyFileName = "JsonTreeDisableFoundKeys_" + feedProvider.Id + ".json";
            filePath = Path.Combine(rootFilePath, JsonTreeDisableKeyFileName);
            // write JSON directly to a file
            using (StreamWriter file = System.IO.File.CreateText(filePath))
            using (JsonTextWriter writer = new JsonTextWriter(file))
            {
                JArray jObj = JArray.FromObject(lstVmJsTreeForDisabledKey);
                jObj.WriteTo(writer);
            }

            FeedProviderHelper.UpdateFeedProvider_JSONFileName(feedProvider.Id, jsonTreeFilename, sampleJSONFileName, JsonTreeDisableKeyFileName);
            #endregion
        }

        /// <summary>
        /// Create JS tree according to matched fields and Save matched JSON keys into a file/session
        /// </summary>
        /// <param name="feedProvider"></param>
        /// <param name="token"></param>
        private void SaveAllJsonKeys_v2(FeedProvider feedProvider, JToken token)
        {
            //var rootFilePath = HttpContext.Request.MapPath("~" + Settings.FeedJSONFilePath);
            //string webRootPath = _hostingEnvironment.WebRootPath;
            string contentRootPath = _hostingEnvironment.ContentRootPath;
            var rootFilePath = string.Concat(contentRootPath, "/", Settings.FeedJSONFilePath);
            string filePath = "";
            JToken newToken = null;

            #region save sample json
            var sampleJSONFileName = "SampleJsonItem_" + feedProvider.Id + ".json";
            //newToken = CreateUpdateSampleJSON(token, ((Session[$"SampleJsonItem_{feedProvider.Id}"] as JToken)?.SelectToken($"items[0]") ?? null));
            newToken = CreateUpdateSampleJSON(token, ((HttpContext.Session.GetObject<JToken>($"SampleJsonItem_{feedProvider.Id}") as JToken)?.SelectToken($"items[0]") ?? null));
            #region Create Root Token
            JObject actualToken = new JObject();
            foreach (JProperty sourceProp in token.Parent.Parent.Parent.Children<JProperty>())
            {
                if (sourceProp.Name != "items")
                    actualToken.Add(new JProperty(sourceProp.Name, sourceProp.Value));
                else
                {
                    JArray array = new JArray();
                    array.Add(newToken);
                    actualToken.Add(new JProperty(sourceProp.Name, array));
                }
            }
            #endregion
            //Session[$"SampleJsonItem_{feedProvider.Id}"] = actualToken;
            HttpContext.Session.SetObject($"SampleJsonItem_{feedProvider.Id}", actualToken);
            filePath = Path.Combine(rootFilePath, sampleJSONFileName);
            using (StreamWriter file = System.IO.File.CreateText(filePath))
            using (JsonTextWriter writer = new JsonTextWriter(file))
            {
                JObject jObj = JObject.FromObject(actualToken);
                jObj.WriteTo(writer);
            }
            #endregion

            #region save json tree format according to matched field

            #region commented( Insert/update JSON tree node)
            //var jsonTreeFilename = "JsonTree_" + feedProvider.Id + ".json";

            //#region Insert/update tree node
            //if (Session[$"JsonTree_{feedProvider.Id}"] != null)
            //{
            //    lstVmJsTree = Session[$"JsonTree_{feedProvider.Id}"] as List<vmJsTree>;
            //    lstVmJsTree = CreateUpdateJsTree(newToken, lstVmJsTree);
            //}
            //else if (!string.IsNullOrEmpty(jsonTreeFilename) &&
            //    System.IO.File.Exists(Path.Combine(HttpContext.Request.MapPath("~" + Settings.FeedJSONFilePath), jsonTreeFilename)))
            //{
            //    using (StreamReader r = new StreamReader(Path.Combine(HttpContext.Request.MapPath("~" + Settings.FeedJSONFilePath), jsonTreeFilename)))
            //    {
            //        string json = r.ReadToEnd();
            //        lstVmJsTree = JsonConvert.DeserializeObject<List<vmJsTree>>(json);
            //        lstVmJsTree = CreateUpdateJsTree(newToken, lstVmJsTree);
            //    }
            //}
            //else
            //    lstVmJsTree = CreateJsTree(newToken);
            //#endregion

            //Session[$"JsonTree_{feedProvider.Id}"] = lstVmJsTree;
            //filePath = Path.Combine(rootFilePath, jsonTreeFilename);
            //using (StreamWriter file = System.IO.File.CreateText(filePath))
            //using (JsonTextWriter writer = new JsonTextWriter(file))
            //{
            //    JArray jObj = JArray.FromObject(lstVmJsTree);
            //    jObj.WriteTo(writer);
            //}
            #endregion

            var lstVmJsTree = CreateJsTree(newToken);
            //Session[$"JsonTree_{feedProvider.Id}"] = lstVmJsTree;
            HttpContext.Session.SetObject($"JsonTree_{feedProvider.Id}", lstVmJsTree);
            var jsonTreeFilename = "JsonTree_" + feedProvider.Id + ".json";
            filePath = Path.Combine(rootFilePath, jsonTreeFilename);
            using (StreamWriter file = System.IO.File.CreateText(filePath))
            using (JsonTextWriter writer = new JsonTextWriter(file))
            {
                JArray jObj = JArray.FromObject(lstVmJsTree);
                jObj.WriteTo(writer);
            }

            #endregion            

            #region save json for disabled keys 
            var lstVmJsTreeForDisabledKey = CreateJsTree(newToken, isDisableParentKeys: true);
            //Session[$"JsonTreeDisableFoundKeys_{feedProvider.Id}"] = lstVmJsTreeForDisabledKey;
            HttpContext.Session.SetObject($"JsonTreeDisableFoundKeys_{feedProvider.Id}", lstVmJsTreeForDisabledKey);
            var JsonTreeDisableKeyFileName = "JsonTreeDisableFoundKeys_" + feedProvider.Id + ".json";
            filePath = Path.Combine(rootFilePath, JsonTreeDisableKeyFileName);
            using (StreamWriter file = System.IO.File.CreateText(filePath))
            using (JsonTextWriter writer = new JsonTextWriter(file))
            {
                JArray jObj = JArray.FromObject(lstVmJsTreeForDisabledKey);
                jObj.WriteTo(writer);
            }
            #endregion

            FeedProviderHelper.UpdateFeedProvider_JSONFileName(feedProvider.Id, jsonTreeFilename, sampleJSONFileName, JsonTreeDisableKeyFileName);
        }

        private void AnalyzeStandardOpenActiveData(FeedProvider feedProvider, List<JToken> lstJToken, List<FeedMapping> lstFeedMapping = null)
        {
            var lstExistingFeedMapping = FeedConfigHelper.GetFeedMappingByTableName(feedProvider.Id);

            if (lstExistingFeedMapping != null && lstExistingFeedMapping.Count > 0)
            {
                for (int i = 0; i < lstJToken.Count; i++)
                {
                    var state = lstJToken[i].SelectToken("$.state");

                    if (state != null)
                    {
                        string strState = state.Value<string>();

                        //skip analyzing deleted feeds
                        if (strState == "deleted")
                        {
                            if (i != lstJToken.Count - 1)
                                continue;
                        }
                    }

                    SaveAllJsonKeys(feedProvider, lstJToken[i]);

                    dynamic objDynamicData = new ExpandoObject();

                    for (int j = 0; j < lstExistingFeedMapping.Count; j++)
                    {
                        long id = lstExistingFeedMapping[j].Id;
                        long? parentId = lstExistingFeedMapping[j].ParentId;

                        string tableName = lstExistingFeedMapping[j].TableName;
                        string columnName = lstExistingFeedMapping[j].ColumnName;
                        string columnDataType = "";
                        string feedKey = lstExistingFeedMapping[j].FeedKey;
                        string constraints = lstExistingFeedMapping[j].Constraint;
                        string customFeedKeyPath = lstExistingFeedMapping[j].FeedKeyPath;
                        string actualFeedKeyPath = lstExistingFeedMapping[j].ActualFeedKeyPath;
                        bool isFound = false;
                        bool hasMatchedConstraint = true;

                        //if constraints are available then first check constraints and then find the key match
                        if (!string.IsNullOrEmpty(constraints))
                            hasMatchedConstraint = CheckConstraints(lstJToken[i], constraints);

                        if (hasMatchedConstraint)
                        {
                            var currJToken = lstJToken[i];

                            //get parent feed key
                            var parentFeedMapping = lstFeedMapping.Where(x => x.Id == parentId).FirstOrDefault();

                            //if parent feed key found then only search inside that parent only
                            if (parentFeedMapping != null)
                                currJToken = currJToken.SelectToken("$." + parentFeedMapping.ActualFeedKeyPath);

                            isFound = FindFeedKeyMatch(currJToken, columnName, feedKey, objDynamicData, ref customFeedKeyPath, ref actualFeedKeyPath, ref columnDataType, checkByHierarchy: parentFeedMapping != null);
                        }

                        #region Feed Key Mapping
                        if (!isFound)
                            feedKey = "";

                        if (lstFeedMapping != null)
                        {
                            var idx = lstFeedMapping.FindIndex(x => x.ColumnName == columnName && x.ParentId == parentId);

                            if (idx >= 0)
                            {
                                if (!string.IsNullOrEmpty(feedKey))
                                {
                                    lstFeedMapping[idx].FeedKey = feedKey;
                                    lstFeedMapping[idx].FeedKeyPath = customFeedKeyPath;
                                }
                            }
                            else
                            {
                                var feedMapping = new FeedMapping()
                                {
                                    FeedProvider = feedProvider,
                                    TableName = tableName,
                                    ColumnName = columnName,
                                    ParentId = parentId,
                                    Constraint = constraints
                                };

                                if (!string.IsNullOrEmpty(feedKey))
                                {
                                    feedMapping.FeedKey = feedKey;
                                    feedMapping.FeedKeyPath = customFeedKeyPath;
                                    feedMapping.ActualFeedKeyPath = actualFeedKeyPath;
                                }

                                lstFeedMapping.Add(feedMapping);
                            }
                        }
                        #endregion Feed Key Mapping
                    }

                    break;
                }
            }
        }

        private void AnalyzeFeedData(FeedProvider feedProvider, List<JToken> lstJToken, List<FeedMapping> lstFeedMapping = null)
        {
            var dtIntelligentMapping = FeedConfigHelper.GetIntelligentMappingByTableName();

            if (dtIntelligentMapping != null && dtIntelligentMapping.Rows.Count > 0)
            {
                for (int i = 0; i < lstJToken.Count; i++)
                {
                    if (i == 0)
                    {
                        HttpContext.Session.SetObject($"SampleJsonItem_{feedProvider.Id}", lstJToken[i]);
                    }

                    dynamic objDynamicData = new ExpandoObject();

                    for (int j = 0; j < dtIntelligentMapping.Rows.Count; j++)
                    {
                        var row = dtIntelligentMapping.Rows[j];

                        bool isFound = false;
                        string columnName = Convert.ToString(row["ColumnName"]);
                        string tableName = Convert.ToString(row["TableName"]);
                        string possibleMatches = Convert.ToString(row["PossibleMatches"]);
                        string customCriteria = Convert.ToString(row["CustomCriteria"]);
                        string columnDataType = "";
                        string feedKey = "";
                        string customFeedKeyPath = "";
                        string actualFeedKeyPath = "";

                        if (!string.IsNullOrEmpty(possibleMatches))
                        {
                            var possibleMatchesParts = possibleMatches.Split(',');

                            for (int k = 0; k < possibleMatchesParts.Length; k++)
                            {
                                if (!string.IsNullOrEmpty(possibleMatchesParts[k]))
                                {
                                    //if constraints are available then first check constraints and then find the key match
                                    if (!string.IsNullOrEmpty(customCriteria))
                                    {
                                        bool hasMatchedConstraints = CheckConstraints(lstJToken[i].Parent.Parent, customCriteria);

                                        if (hasMatchedConstraints)
                                            isFound = FindFeedKeyMatch(lstJToken[i], columnName, possibleMatchesParts[k], objDynamicData, ref customFeedKeyPath, ref actualFeedKeyPath, ref columnDataType);
                                    }
                                    else
                                    {
                                        isFound = FindFeedKeyMatch(lstJToken[i], columnName, possibleMatchesParts[k], objDynamicData, ref customFeedKeyPath, ref actualFeedKeyPath, ref columnDataType);
                                    }
                                }

                                if (isFound)
                                {
                                    feedKey = possibleMatchesParts[k];
                                    break;
                                }
                            }
                        }

                        #region Feed Key Mapping
                        var feedMapping = new FeedMapping()
                        {
                            FeedProvider = feedProvider,
                            TableName = tableName,
                            ColumnName = columnName
                        };

                        if (!string.IsNullOrEmpty(feedKey))
                        {
                            feedMapping.FeedKey = feedKey;
                            feedMapping.FeedKeyPath = customFeedKeyPath;
                        }

                        if (lstFeedMapping != null)
                            lstFeedMapping.Add(feedMapping);
                        #endregion Feed Key Mapping
                    }
                    break;
                }
            }

            return;
        }

        /// <summary>
        /// Update feed provider's ordering stratergy such as whether it uses url slug or not and the parameter which are coming into next url
        /// </summary>
        /// <param name="feedProvider"></param>
        /// <param name="token"></param>
        private void AnalyzeOrderingStrategy(FeedProvider feedProvider, JToken token)
        {
            if (feedProvider != null && token != null)
            {
                bool isUsesChangeNumber = false, isUsesTimeStamp = false, isUtcTimestamp = false, isUsesUrlSlug = false;

                var next = token.SelectToken("$.next");
                long? datetimestamp;

                if (next != null)
                {
                    string strNext = next.Value<string>();

                    //check if querystring params available
                    if (strNext.IndexOf('?') > -1)
                    {
                        //var parameters = HttpUtility.ParseQueryString(strNext.Substring(strNext.IndexOf("?")));
                        var parameters = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(strNext.Substring(strNext.IndexOf("?")));

                        var objNameCollection = parameters.ToNameValueCollection();
                        string afterChangeNumber = objNameCollection.Get("afterChangeNumber");
                        string afterTimestamp = objNameCollection.Get("afterTimestamp");
                        if (!string.IsNullOrEmpty(afterTimestamp))
                        {
                            isUsesTimeStamp = true;

                            long tempDatetimestamp = 0;

                            if (long.TryParse(afterTimestamp, out tempDatetimestamp))
                            {
                                isUtcTimestamp = true;
                            }
                        }
                        else if (!string.IsNullOrEmpty(afterChangeNumber))
                        {
                            isUsesChangeNumber = true;
                        }
                    }
                    else
                    {
                        //next page url only supports slugs (params after /)
                        if (strNext.IndexOf("/") > -1)
                        {
                            isUsesUrlSlug = true;

                            string parameter = strNext.Substring(strNext.LastIndexOf("/"));

                            long tempDatetimestamp = 0;

                            if (!string.IsNullOrEmpty(parameter))
                            {
                                isUsesTimeStamp = true;

                                if (long.TryParse(parameter, out tempDatetimestamp))
                                {
                                    //timestamp found
                                    datetimestamp = tempDatetimestamp;

                                    isUtcTimestamp = true;
                                }
                            }
                        }
                    }
                }

                FeedProviderHelper.UpdateFeedProvider_OrderingStrategy(feedProvider.Id, isUsesTimeStamp, isUtcTimestamp, isUsesChangeNumber, isUsesUrlSlug);
            }

        }

        private void AnalyzeFeedData_V1(FeedProvider feedProvider, JToken token, out List<IntelligentFeedMapping> lstIntelligentFeedMapping)
        {
            //get all intelligent mapping keys
            lstIntelligentFeedMapping = FeedConfigHelper.GetIntelligentMapping(feedProvider.Id);

            if (lstIntelligentFeedMapping != null && lstIntelligentFeedMapping.Count > 0)
            {
                //get arrays by level
                var lstTuple = token.FindArraysByLevel();
                var lstJToken = lstTuple.Select(x => x.Item1).ToList();
                bool hasFoundAnyKeys = false;

                //loop through all json arrays of feed
                for (int i = 0; i < lstJToken.Count; i++)
                {
                    //break if already found any keys in previous iterations
                    if (hasFoundAnyKeys)
                        break;

                    int totalChilds = lstJToken[i].Count();

                    if (totalChilds > 0)
                    {
                        //loop through all objects of json array
                        for (int k = 0; k < totalChilds; k++)
                        {
                            var jsonObject = lstJToken[i].SelectToken("$.[" + k + "]");

                            var state = lstJToken[i].SelectToken("$.[" + k + "].state");

                            if (state != null)
                            {
                                string strState = state.Value<string>();

                                //skip analyzing deleted feeds and continue till at least one valid feed found
                                if (strState == "deleted")
                                {
                                    if (k != totalChilds - 1)
                                        continue;
                                }
                            }

                            dynamic objDynamicData = new ExpandoObject();

                            //loop through all configured keys needs to be picked according to intelligent mapping
                            #region loop through intelligent mapping
                            for (int j = 0; j < lstIntelligentFeedMapping.Count; j++)
                            {
                                var intelligentFeedMapping = lstIntelligentFeedMapping[j];

                                //var currJToken = lstJToken[i];
                                var currJToken = jsonObject;

                                long id = intelligentFeedMapping.Id;
                                long? parentId = intelligentFeedMapping.ParentId;
                                string tableName = intelligentFeedMapping.TableName;
                                string columnName = intelligentFeedMapping.ColumnName;

                                string possibleMatches = intelligentFeedMapping.PossibleMatches;
                                string possibleHierarchies = intelligentFeedMapping.PossibleHierarchies;
                                string customCriteria = intelligentFeedMapping.CustomCriteria;
                                long feedMappingId = intelligentFeedMapping.FeedMapping.Id;
                                long? feedMappingParentId = intelligentFeedMapping.FeedMapping.ParentId;
                                string customFeedKeyPath = intelligentFeedMapping.FeedMapping.FeedKeyPath;
                                string actualFeedKeyPath = intelligentFeedMapping.FeedMapping.ActualFeedKeyPath;
                                string columnDataType = intelligentFeedMapping.FeedMapping.ColumnDataType;
                                bool isCustomFeedKey = intelligentFeedMapping.FeedMapping.IsCustomFeedKey;
                                string foundKey = "";
                                bool isFound = false;
                                bool hasMatchedConstraint = true;

                                if (parentId != null)
                                {
                                    var parentFeedMapping = lstIntelligentFeedMapping.Where(x => x.Id == parentId).FirstOrDefault();

                                    //if parent feed key found and no hierarchy set then only search inside that parent only
                                    if (parentFeedMapping != null
                                        && !string.IsNullOrEmpty(parentFeedMapping.FeedMapping.FeedKey)
                                        && string.IsNullOrEmpty(possibleHierarchies))
                                        currJToken = currJToken.Root.SelectToken("$." + parentFeedMapping.FeedMapping.ActualFeedKeyPath);
                                    else
                                        currJToken = null;
                                }

                                if (currJToken != null)
                                {
                                    //if constraints are available then first check constraints and then find the key match
                                    //if (!string.IsNullOrEmpty(customCriteria))
                                    //    hasMatchedConstraint = CheckConstraints(lstJToken[i].Root, customCriteria);
                                    if (!string.IsNullOrEmpty(customCriteria))
                                        hasMatchedConstraint = CheckConstraints(jsonObject.Root, customCriteria);

                                    //analyse according to various intelligent mapping criterias
                                    if (hasMatchedConstraint)
                                        isFound = ProcessFeedKey(currJToken, k, columnName, possibleMatches, possibleHierarchies, objDynamicData, ref customFeedKeyPath, ref actualFeedKeyPath, ref columnDataType);

                                    if (isFound)
                                    {
                                        hasFoundAnyKeys = true;
                                        foundKey = customFeedKeyPath.Substring(customFeedKeyPath.LastIndexOf('>') + 1).Trim();
                                    }
                                }

                                #region Feed Key Mapping
                                if (lstIntelligentFeedMapping != null)
                                {
                                    var idx = lstIntelligentFeedMapping.FindIndex(x => x.ColumnName == columnName && x.ParentId == parentId && x.FeedMapping.IsCustomFeedKey == isCustomFeedKey);

                                    if (idx >= 0)
                                    {
                                        if (!isCustomFeedKey || (isCustomFeedKey && !string.IsNullOrEmpty(foundKey)))
                                        {
                                            lstIntelligentFeedMapping[idx].FeedMapping.FeedProvider = feedProvider;
                                            lstIntelligentFeedMapping[idx].FeedMapping.Id = id;
                                            lstIntelligentFeedMapping[idx].FeedMapping.ParentId = parentId;
                                            lstIntelligentFeedMapping[idx].FeedMapping.TableName = lstIntelligentFeedMapping[idx].TableName;
                                            lstIntelligentFeedMapping[idx].FeedMapping.ColumnName = lstIntelligentFeedMapping[idx].ColumnName;
                                            lstIntelligentFeedMapping[idx].FeedMapping.IsCustomFeedKey = isCustomFeedKey;

                                            if (!string.IsNullOrEmpty(foundKey))
                                            {
                                                lstIntelligentFeedMapping[idx].FeedMapping.ColumnDataType = columnDataType;
                                                lstIntelligentFeedMapping[idx].FeedMapping.FeedKey = foundKey;
                                                lstIntelligentFeedMapping[idx].FeedMapping.FeedKeyPath = customFeedKeyPath;
                                                lstIntelligentFeedMapping[idx].FeedMapping.ActualFeedKeyPath = actualFeedKeyPath;
                                                lstIntelligentFeedMapping[idx].FeedMapping.FeedKeyValue = JsonConvert.SerializeObject(CommonFunctions.GetPropertyValue(objDynamicData, columnName), Formatting.Indented);
                                            }
                                        }
                                        else
                                        {
                                            lstIntelligentFeedMapping.RemoveAt(idx);
                                        }
                                    }
                                    else
                                    {
                                        var intelligentFeedMappingNew = new IntelligentFeedMapping()
                                        {
                                            Id = id,
                                            ParentId = parentId,
                                            TableName = tableName,
                                            ColumnName = columnName
                                        };

                                        intelligentFeedMappingNew.FeedMapping.IsCustomFeedKey = isCustomFeedKey;
                                        intelligentFeedMappingNew.FeedMapping.FeedProvider = feedProvider;
                                        intelligentFeedMappingNew.FeedMapping.Id = id;
                                        intelligentFeedMappingNew.FeedMapping.ParentId = parentId;
                                        intelligentFeedMappingNew.FeedMapping.TableName = tableName;
                                        intelligentFeedMappingNew.FeedMapping.ColumnName = columnName;
                                        intelligentFeedMappingNew.FeedMapping.IsCustomFeedKey = isCustomFeedKey;

                                        if (!string.IsNullOrEmpty(foundKey))
                                        {
                                            intelligentFeedMappingNew.FeedMapping.ColumnDataType = columnDataType;
                                            intelligentFeedMappingNew.FeedMapping.FeedKey = foundKey;
                                            intelligentFeedMappingNew.FeedMapping.FeedKeyPath = customFeedKeyPath;
                                            intelligentFeedMappingNew.FeedMapping.ActualFeedKeyPath = actualFeedKeyPath;
                                            intelligentFeedMappingNew.FeedMapping.FeedKeyValue = JsonConvert.SerializeObject(CommonFunctions.GetPropertyValue(objDynamicData, columnName), Formatting.Indented);
                                        }

                                        lstIntelligentFeedMapping.Add(intelligentFeedMappingNew);
                                    }
                                }
                                #endregion Feed Key Mapping
                            }
                            #endregion

                            //break and save json keys in session if any keys found or if it is last iteration
                            if (hasFoundAnyKeys || k == totalChilds - 1)
                            {
                                SaveAllJsonKeys(feedProvider, jsonObject);
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void AnalyzeFeedData_V2(FeedProvider feedProvider, JToken token, out List<IntelligentFeedMapping> lstIntelligentFeedMapping)
        {
            //get all intelligent mapping keys
            //lstIntelligentFeedMapping = FeedConfigHelper.GetIntelligentMapping(feedProvider.Id);
            lstIntelligentFeedMapping = FeedConfigHelper.GetIntelligentMapping_v1(feedProvider.Id);

            if (lstIntelligentFeedMapping != null && lstIntelligentFeedMapping.Count > 0)
            {
                //get arrays by level
                var lstTuple = token.FindArraysByLevel();
                var lstJToken = lstTuple.Select(x => x.Item1).ToList();
                bool hasFoundAnyKeys = false;

                //loop through all json arrays of feed
                for (int i = 0; i < lstJToken.Count; i++)
                {
                    //break if already found any keys in previous iterations
                    if (hasFoundAnyKeys)
                        break;

                    int totalChilds = lstJToken[i].Count();

                    if (totalChilds > 0)
                    {
                        //loop through all objects of json array
                        for (int k = 0; k < totalChilds; k++)
                        {
                            var jsonObject = lstJToken[i].SelectToken("$.[" + k + "]");

                            var state = lstJToken[i].SelectToken("$.[" + k + "].state");

                            if (state != null)
                            {
                                string strState = state.Value<string>();

                                //skip analyzing deleted feeds and continue till at least one valid feed found
                                if (strState == "deleted")
                                {
                                    if (k != totalChilds - 1)
                                        continue;
                                }
                            }

                            dynamic objDynamicData = new ExpandoObject();

                            //loop through all configured keys needs to be picked according to intelligent mapping
                            #region loop through intelligent mapping
                            for (int j = 0; j < lstIntelligentFeedMapping.Count; j++)
                            {
                                var intelligentFeedMapping = lstIntelligentFeedMapping[j];

                                //var currJToken = lstJToken[i];
                                var currJToken = jsonObject;

                                long id = intelligentFeedMapping.Id;
                                long? parentId = intelligentFeedMapping.ParentId;
                                string tableName = intelligentFeedMapping.TableName;
                                string columnName = intelligentFeedMapping.ColumnName;

                                string possibleMatches = intelligentFeedMapping.PossibleMatches;
                                string possibleHierarchies = intelligentFeedMapping.PossibleHierarchies;
                                string customCriteria = intelligentFeedMapping.CustomCriteria;
                                long feedMappingId = intelligentFeedMapping.FeedMapping.Id;
                                long? feedMappingParentId = intelligentFeedMapping.FeedMapping.ParentId;
                                string customFeedKeyPath = intelligentFeedMapping.FeedMapping.FeedKeyPath;
                                string actualFeedKeyPath = intelligentFeedMapping.FeedMapping.ActualFeedKeyPath;
                                string columnDataType = intelligentFeedMapping.FeedMapping.ColumnDataType;
                                //bool isCustomFeedKey = intelligentFeedMapping.FeedMapping.IsCustomFeedKey;
                                bool isCustomFeedKey = intelligentFeedMapping.IsCustomFeedKey;
                                long? position = intelligentFeedMapping.Position;
                                string foundKey = "";
                                bool isFound = false;
                                bool hasMatchedConstraint = true;

                                if (parentId != null)
                                {
                                    var parentFeedMapping = lstIntelligentFeedMapping.Where(x => x.Id == parentId).FirstOrDefault();

                                    //if parent feed key found and no hierarchy set then only search inside that parent only
                                    if (parentFeedMapping != null
                                        && !string.IsNullOrEmpty(parentFeedMapping.FeedMapping.FeedKey)
                                        && string.IsNullOrEmpty(possibleHierarchies))
                                        currJToken = currJToken.Root.SelectToken("$." + parentFeedMapping.FeedMapping.ActualFeedKeyPath);
                                    else
                                        currJToken = null;
                                }

                                if (currJToken != null)
                                {
                                    //if constraints are available then first check constraints and then find the key match
                                    //if (!string.IsNullOrEmpty(customCriteria))
                                    //    hasMatchedConstraint = CheckConstraints(lstJToken[i].Root, customCriteria);
                                    if (!string.IsNullOrEmpty(customCriteria))
                                        hasMatchedConstraint = CheckConstraints(jsonObject.Root, customCriteria);

                                    //analyse according to various intelligent mapping criterias
                                    if (hasMatchedConstraint)
                                        isFound = ProcessFeedKey(currJToken, k, columnName, possibleMatches, possibleHierarchies, objDynamicData, ref customFeedKeyPath, ref actualFeedKeyPath, ref columnDataType);

                                    if (isFound)
                                    {
                                        hasFoundAnyKeys = true;
                                        foundKey = customFeedKeyPath.Substring(customFeedKeyPath.LastIndexOf('>') + 1).Trim();
                                    }
                                }

                                #region Feed Key Mapping
                                if (lstIntelligentFeedMapping != null)
                                {
                                    //var idx = lstIntelligentFeedMapping.FindIndex(x => x.ColumnName == columnName && x.ParentId == parentId && x.FeedMapping.IsCustomFeedKey == isCustomFeedKey);
                                    var idx = lstIntelligentFeedMapping.FindIndex(x => x.ColumnName == columnName && x.ParentId == parentId && x.IsCustomFeedKey == isCustomFeedKey);

                                    if (idx >= 0)
                                    {
                                        if (!isCustomFeedKey || (isCustomFeedKey && !string.IsNullOrEmpty(foundKey)))
                                        {
                                            lstIntelligentFeedMapping[idx].FeedMapping.FeedProvider = feedProvider;
                                            lstIntelligentFeedMapping[idx].FeedMapping.Id = id;
                                            lstIntelligentFeedMapping[idx].FeedMapping.ParentId = parentId;
                                            lstIntelligentFeedMapping[idx].FeedMapping.TableName = lstIntelligentFeedMapping[idx].TableName;
                                            lstIntelligentFeedMapping[idx].FeedMapping.ColumnName = lstIntelligentFeedMapping[idx].ColumnName;
                                            lstIntelligentFeedMapping[idx].FeedMapping.IsCustomFeedKey = isCustomFeedKey;

                                            if (!string.IsNullOrEmpty(foundKey))
                                            {
                                                lstIntelligentFeedMapping[idx].FeedMapping.ColumnDataType = columnDataType;
                                                lstIntelligentFeedMapping[idx].FeedMapping.FeedKey = foundKey;
                                                lstIntelligentFeedMapping[idx].FeedMapping.FeedKeyPath = customFeedKeyPath;
                                                lstIntelligentFeedMapping[idx].FeedMapping.ActualFeedKeyPath = actualFeedKeyPath;
                                                lstIntelligentFeedMapping[idx].FeedMapping.FeedKeyValue = JsonConvert.SerializeObject(CommonFunctions.GetPropertyValue(objDynamicData, columnName), Formatting.Indented);
                                                lstIntelligentFeedMapping[idx].FeedMapping.Position = position;
                                            }
                                            //lstIntelligentFeedMapping[idx].FeedMapping.Position = position;
                                        }
                                        else
                                        {
                                            lstIntelligentFeedMapping.RemoveAt(idx);
                                        }
                                    }
                                    else
                                    {
                                        var intelligentFeedMappingNew = new IntelligentFeedMapping()
                                        {
                                            Id = id,
                                            ParentId = parentId,
                                            TableName = tableName,
                                            ColumnName = columnName
                                        };

                                        intelligentFeedMappingNew.FeedMapping.IsCustomFeedKey = isCustomFeedKey;
                                        intelligentFeedMappingNew.FeedMapping.FeedProvider = feedProvider;
                                        intelligentFeedMappingNew.FeedMapping.Id = id;
                                        intelligentFeedMappingNew.FeedMapping.ParentId = parentId;
                                        intelligentFeedMappingNew.FeedMapping.TableName = tableName;
                                        intelligentFeedMappingNew.FeedMapping.ColumnName = columnName;
                                        intelligentFeedMappingNew.FeedMapping.IsCustomFeedKey = isCustomFeedKey;

                                        if (!string.IsNullOrEmpty(foundKey))
                                        {
                                            intelligentFeedMappingNew.FeedMapping.ColumnDataType = columnDataType;
                                            intelligentFeedMappingNew.FeedMapping.FeedKey = foundKey;
                                            intelligentFeedMappingNew.FeedMapping.FeedKeyPath = customFeedKeyPath;
                                            intelligentFeedMappingNew.FeedMapping.ActualFeedKeyPath = actualFeedKeyPath;
                                            intelligentFeedMappingNew.FeedMapping.FeedKeyValue = JsonConvert.SerializeObject(CommonFunctions.GetPropertyValue(objDynamicData, columnName), Formatting.Indented);
                                            intelligentFeedMappingNew.FeedMapping.Position = position;
                                        }
                                        //intelligentFeedMappingNew.FeedMapping.Position = position;

                                        lstIntelligentFeedMapping.Add(intelligentFeedMappingNew);
                                    }
                                }
                                #endregion Feed Key Mapping
                            }
                            #endregion

                            //break and save json keys in session if any keys found or if it is last iteration
                            if (hasFoundAnyKeys || k == totalChilds - 1)
                            {
                                //SaveAllJsonKeys(feedProvider, jsonObject);
                                SaveAllJsonKeys_v1(feedProvider, jsonObject);
                                break;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Return the list of fields which are mapped or not-mapped
        /// </summary>
        /// <param name="feedProvider"></param>
        /// <param name="token"></param>
        /// <param name="lstIntelligentFeedMapping"></param>
        private void AnalyzeFeedData_V3(FeedProvider feedProvider, JToken token, out List<IntelligentFeedMapping> lstIntelligentFeedMapping)
        {
            //get all intelligent mapping keys
            //lstIntelligentFeedMapping = FeedConfigHelper.GetIntelligentMapping_v1(feedProvider.Id);
            lstIntelligentFeedMapping = FeedConfigHelper.GetIntelligentMappingForAnalyze();

            if (lstIntelligentFeedMapping != null && lstIntelligentFeedMapping.Count > 0)
            {
                //get arrays by level
                var lstTuple = token.FindArraysByLevel();
                var lstJToken = lstTuple.Select(x => x.Item1).ToList();
                bool hasFoundAnyKeys = false;

                //loop through all json arrays of feed
                for (int i = 0; i < lstJToken.Count; i++)
                {
                    //break if already found any keys in previous iterations
                    if (hasFoundAnyKeys)
                        break;

                    int totalChilds = lstJToken[i].Count();

                    if (totalChilds > 0)
                    {
                        #region through all objects of json array                        
                        for (int k = 0; k < totalChilds; k++)
                        {
                            var jsonObject = lstJToken[i].SelectToken("$.[" + k + "]");

                            var state = lstJToken[i].SelectToken("$.[" + k + "].state");

                            if (state != null)
                            {
                                string strState = state.Value<string>();

                                //skip analyzing deleted feeds and continue till at least one valid feed found
                                if (strState == "deleted")
                                {
                                    //if (k != totalChilds - 1)
                                    continue;
                                }
                            }

                            dynamic objDynamicData = new ExpandoObject();

                            //loop through all configured keys needs to be picked according to intelligent mapping
                            #region loop through intelligent mapping
                            for (int j = 0; j < lstIntelligentFeedMapping.Count; j++)
                            {
                                var intelligentFeedMapping = lstIntelligentFeedMapping[j];

                                var currJToken = jsonObject;

                                long id = intelligentFeedMapping.Id;
                                long? parentId = intelligentFeedMapping.ParentId;
                                string tableName = intelligentFeedMapping.TableName;
                                string columnName = intelligentFeedMapping.ColumnName;
                                
                                string possibleMatches = intelligentFeedMapping.PossibleMatches;
                                string possibleHierarchies = intelligentFeedMapping.PossibleHierarchies;
                                string customCriteria = intelligentFeedMapping.CustomCriteria;
                                long feedMappingId = intelligentFeedMapping.FeedMapping.Id;
                                long? feedMappingParentId = intelligentFeedMapping.FeedMapping.ParentId;
                                string customFeedKeyPath = intelligentFeedMapping.FeedMapping.FeedKeyPath;
                                string actualFeedKeyPath = intelligentFeedMapping.FeedMapping.ActualFeedKeyPath;
                                string columnDataType = intelligentFeedMapping.FeedMapping.ColumnDataType;
                                bool isCustomFeedKey = intelligentFeedMapping.IsCustomFeedKey;
                                long? position = intelligentFeedMapping.Position;
                                string foundKey = "";
                                bool isFound = false;
                                bool hasMatchedConstraint = true;

                                if (parentId != null)
                                {
                                    var parentFeedMapping = lstIntelligentFeedMapping.Where(x => x.Id == parentId).FirstOrDefault();

                                    //if parent feed key found and no hierarchy set then only search inside that parent only
                                    //if (parentFeedMapping != null
                                    //    && !string.IsNullOrEmpty(parentFeedMapping?.FeedMapping.FeedKey)
                                    //    && string.IsNullOrEmpty(possibleHierarchies))
                                    //    currJToken = currJToken.Root.SelectToken("$." + parentFeedMapping.FeedMapping.ActualFeedKeyPath);
                                    //else
                                    //    currJToken = null;
                                    if (parentFeedMapping != null
                                        && !string.IsNullOrEmpty(parentFeedMapping?.FeedMapping.FeedKey))
                                    {
                                        if (string.IsNullOrEmpty(possibleHierarchies))
                                            currJToken = currJToken.Root.SelectToken("$." + parentFeedMapping.FeedMapping.ActualFeedKeyPath);
                                    }
                                    else
                                        currJToken = null;
                                }

                                if (currJToken != null)
                                {
                                    if (!string.IsNullOrEmpty(customCriteria))
                                        hasMatchedConstraint = CheckConstraints(jsonObject.Root, customCriteria);

                                    //analyse according to various intelligent mapping criterias
                                    if (hasMatchedConstraint)
                                        isFound = ProcessFeedKey(currJToken, k, columnName, possibleMatches, possibleHierarchies, objDynamicData, ref customFeedKeyPath, ref actualFeedKeyPath, ref columnDataType);

                                    if (isFound)
                                    {
                                        hasFoundAnyKeys = true;
                                        foundKey = customFeedKeyPath.Substring(customFeedKeyPath.LastIndexOf('>') + 1).Trim();
                                    }
                                    else
                                    {
                                    }
                                }

                                #region Feed Key Mapping
                                if (lstIntelligentFeedMapping != null)
                                {
                                    var idx = lstIntelligentFeedMapping.FindIndex(x => x.ColumnName == columnName && x.ParentId == parentId && x.IsCustomFeedKey == isCustomFeedKey);

                                    if (idx >= 0)
                                    {
                                        if (!isCustomFeedKey || (isCustomFeedKey && !string.IsNullOrEmpty(foundKey)))
                                        {
                                            lstIntelligentFeedMapping[idx].FeedMapping.FeedProvider = feedProvider;
                                            lstIntelligentFeedMapping[idx].FeedMapping.Id = id;
                                            lstIntelligentFeedMapping[idx].FeedMapping.ParentId = parentId;
                                            lstIntelligentFeedMapping[idx].FeedMapping.TableName = lstIntelligentFeedMapping[idx].TableName;
                                            lstIntelligentFeedMapping[idx].FeedMapping.ColumnName = lstIntelligentFeedMapping[idx].ColumnName;
                                            lstIntelligentFeedMapping[idx].FeedMapping.IsCustomFeedKey = isCustomFeedKey;

                                            if (!string.IsNullOrEmpty(foundKey))
                                            {
                                                lstIntelligentFeedMapping[idx].FeedMapping.ColumnDataType = columnDataType;
                                                lstIntelligentFeedMapping[idx].FeedMapping.FeedKey = foundKey;
                                                lstIntelligentFeedMapping[idx].FeedMapping.FeedKeyPath = customFeedKeyPath;
                                                lstIntelligentFeedMapping[idx].FeedMapping.ActualFeedKeyPath = actualFeedKeyPath;
                                                lstIntelligentFeedMapping[idx].FeedMapping.FeedKeyValue = JsonConvert.SerializeObject(CommonFunctions.GetPropertyValue(objDynamicData, columnName), Formatting.Indented);
                                                lstIntelligentFeedMapping[idx].FeedMapping.Position = position;
                                            }
                                        }
                                        else
                                        {
                                            lstIntelligentFeedMapping.RemoveAt(idx);
                                        }
                                    }
                                    else
                                    {
                                        var intelligentFeedMappingNew = new IntelligentFeedMapping()
                                        {
                                            Id = id,
                                            ParentId = parentId,
                                            TableName = tableName,
                                            ColumnName = columnName
                                        };

                                        intelligentFeedMappingNew.FeedMapping.IsCustomFeedKey = isCustomFeedKey;
                                        intelligentFeedMappingNew.FeedMapping.FeedProvider = feedProvider;
                                        intelligentFeedMappingNew.FeedMapping.Id = id;
                                        intelligentFeedMappingNew.FeedMapping.ParentId = parentId;
                                        intelligentFeedMappingNew.FeedMapping.TableName = tableName;
                                        intelligentFeedMappingNew.FeedMapping.ColumnName = columnName;
                                        intelligentFeedMappingNew.FeedMapping.IsCustomFeedKey = isCustomFeedKey;

                                        if (!string.IsNullOrEmpty(foundKey))
                                        {
                                            intelligentFeedMappingNew.FeedMapping.ColumnDataType = columnDataType;
                                            intelligentFeedMappingNew.FeedMapping.FeedKey = foundKey;
                                            intelligentFeedMappingNew.FeedMapping.FeedKeyPath = customFeedKeyPath;
                                            intelligentFeedMappingNew.FeedMapping.ActualFeedKeyPath = actualFeedKeyPath;
                                            intelligentFeedMappingNew.FeedMapping.FeedKeyValue = JsonConvert.SerializeObject(CommonFunctions.GetPropertyValue(objDynamicData, columnName), Formatting.Indented);
                                            intelligentFeedMappingNew.FeedMapping.Position = position;
                                        }

                                        lstIntelligentFeedMapping.Add(intelligentFeedMappingNew);
                                    }
                                }
                                #endregion Feed Key Mapping
                            }
                            #endregion

                            //break and save json keys in session if any keys found or if it is last iteration                            
                            if (hasFoundAnyKeys && state.Value<string>() != "deleted")
                            {
                                SaveAllJsonKeys_v2(feedProvider, jsonObject);
                                //break;
                            }
                        }
                        #endregion                        
                    }
                }
            }
        }

        //public JToken GetAllTokensUsingNextPageURL(JToken token, JToken lstJToken = null, string nextPageURL = null)
        //{
        //    var nextUrl = token.SelectToken("$.next");
        //    var children = lstJToken?.Children().ToArray();
        //    JArray jarrayObj = new JArray();
        //    jarrayObj.Add(lstJToken.Children());

        //    if (nextUrl != null)
        //    {
        //        while (jarrayObj.Children().Count() != Settings.FeedTraverseLength)
        //        {
        //            string json = GetWebContent(Convert.ToString(nextUrl));

        //            var newToken = JToken.Parse(json).SelectToken("$.items");
        //            if (newToken.Children().Count() > 0)
        //            {
        //                jarrayObj.Add(newToken.Children());
        //            }
        //            GetAllTokensUsingNextPageURL(token, jarrayObj as JToken, Convert.ToString(nextUrl));
        //        }
        //    }
        //    return lstJToken;
        //}

        #region Get Limited Data for Analyze
        public JToken GetLimitedDataForAnalyze_old(JToken token, JToken lstItems = null, string nextPageURL = null)
        {
            JArray jarrayObj = new JArray();
            JToken result = null;
            if (token != null)
            {
                if (!string.IsNullOrEmpty(nextPageURL))
                {
                    #region get items from next page url
                    var nextUrl = token.SelectToken("$.next");
                    jarrayObj.Add(lstItems?.Children());

                    if (nextUrl != null)
                    {
                        //result = GetAllTokensUsingNextPageURL(token, lstItems, nextPageURL);                        
                        if (jarrayObj.Children().Count() == Settings.FeedTraverseLength)
                            return jarrayObj as JToken;

                        if (jarrayObj.Children().Count() < Settings.FeedTraverseLength)
                        {
                            string json = GetWebContent(Convert.ToString(nextUrl));

                            var newToken = JToken.Parse(json);
                            var newItems = newToken.SelectToken("$.items");
                            //if (newItems.Children().Count() > 0)
                            //{
                            //    jarrayObj.Add(newItems.Children());
                            //}
                            for (var i = 0; i < (Settings.FeedTraverseLength - jarrayObj.Children().Count()) && i < newItems.Children().Count(); i++)
                            {
                                jarrayObj.Add(newItems[i]);
                            }
                            //lstItems = jarrayObj as JToken;
                            result = GetLimitedDataForAnalyze_old(newToken, jarrayObj as JToken, Convert.ToString(nextUrl));
                        }
                    }
                    #endregion
                }
                else
                {
                    #region get items for first time
                    var items = token.SelectToken("$.items");
                    if (items.Count() > 0)
                    {
                        if (items.Count() < Settings.FeedTraverseLength)
                        {
                            var nextUrl = token.SelectToken("$.next");
                            jarrayObj.Add(items?.Children());

                            if (nextUrl != null)
                            {
                                // lstItems = jarrayObj as JToken;
                                result = GetLimitedDataForAnalyze_old(token, jarrayObj as JToken, Convert.ToString(nextUrl));
                                //result = GetAllTokensUsingNextPageURL(token, jarrayObj as JToken, nextPageURL);
                            }
                        }
                        else
                        {
                            //result = items.Take(Settings.FeedTraverseLength) as JArray;
                            for (int i = 0; i < Settings.FeedTraverseLength; i++)
                            {
                                jarrayObj.Add(items[i]);
                            }
                            result = jarrayObj;
                        }
                    }
                    #endregion
                }
            }
            return result;
        }

        public JToken GetLimitedDataForAnalyze(JToken token, string baseURL, JToken lstItems = null, string nextPageURL = null)
        {
            JArray jarrayObj = new JArray();
            JToken result = null;
            if (token != null)
            {
                if (lstItems != null)
                {
                    #region get items from next page url
                    var nextUrl = token.SelectToken("$.next");
                    jarrayObj.Add(lstItems?.Children());

                    if (nextUrl != null)
                    {
                        nextUrl = GetNextPageURL(Convert.ToString(nextUrl), baseURL);
                        if (jarrayObj.Children().Count() == Settings.FeedTraverseLength)
                            return jarrayObj as JToken;

                        if (jarrayObj.Children().Count() < Settings.FeedTraverseLength)
                        {
                            string json = GetWebContent(Convert.ToString(nextUrl));

                            var newToken = JToken.Parse(json);
                            var newItems = newToken.SelectToken("$.items");
                            var inTakeCount = Settings.FeedTraverseLength - jarrayObj.Children().Count();
                            //for (var i = 0; i < (Settings.FeedTraverseLength - jarrayObj.Children().Count()) && i < newItems.Children().Count(); i++)
                            for (var i = 0; i < inTakeCount && i < newItems.Children().Count(); i++)
                            {
                                jarrayObj.Add(newItems[i]);
                            }
                            //result = GetLimitedDataForAnalyze(newToken, jarrayObj as JToken, Convert.ToString(nextUrl));
                            result = GetLimitedDataForAnalyze(newToken, baseURL, jarrayObj as JToken);
                        }
                    }
                    else
                        result = jarrayObj;
                    #endregion
                }
                else
                {
                    #region get items from source url for first time
                    var items = token.SelectToken("$.items");
                    if (items.Count() > 0)
                    {
                        if (items.Count() < Settings.FeedTraverseLength)
                        {
                            var nextUrl = token.SelectToken("$.next");
                            jarrayObj.Add(items?.Children());

                            if (nextUrl != null)
                                result = GetLimitedDataForAnalyze(token, baseURL, jarrayObj as JToken);
                            else
                                result = jarrayObj;
                        }
                        else
                        {
                            //result = items.Take(Settings.FeedTraverseLength);
                            for (int i = 0; i < Settings.FeedTraverseLength; i++)
                            {
                                jarrayObj.Add(items[i]);
                            }
                            result = jarrayObj;
                        }
                    }
                    #endregion
                }
            }
            return result;
        }

        /// <summary>
        /// Get limited items to traverse for analyzing
        /// </summary>
        /// <param name="token"></param>
        /// <param name="lstItems"></param>
        /// <param name="nextPageURL"></param>
        /// <returns></returns>
        public JToken GetLimitedDataForAnalyze_v1(JToken token, string baseURL, JToken lstItems = null, string nextPageURL = null)
        {
            JArray jarrayObj = new JArray();
            JToken result = null;
            if (token != null)
            {
                if (lstItems != null)
                {
                    #region get items from next page url
                    var nextUrl = token.SelectToken("$.next");
                    jarrayObj.Add(lstItems?.Children());

                    if (nextUrl != null)
                    {
                        nextUrl = GetNextPageURL(Convert.ToString(nextUrl), baseURL);

                        if (jarrayObj.Children().Count() == Settings.FeedTraverseLength)
                            return jarrayObj as JToken;

                        if (jarrayObj.Children().Count() < Settings.FeedTraverseLength)
                        {
                            string json = GetWebContent(Convert.ToString(nextUrl));

                            var newToken = JToken.Parse(json);
                            var newItems = newToken.SelectToken("$.items");
                            var inTakeCount = Settings.FeedTraverseLength - jarrayObj.Children().Count();

                            if (newItems.Count() > 0)
                            {
                                for (var i = 0; i < inTakeCount && i < newItems.Children().Count(); i++)
                                {
                                    var state = newItems.SelectToken("$.[" + i + "].state");
                                    if (state != null)
                                    {
                                        string strState = state.Value<string>();
                                        if (strState != "deleted")
                                            jarrayObj.Add(newItems[i]);
                                    }
                                    //jarrayObj.Add(newItems[i]);
                                }
                                result = GetLimitedDataForAnalyze_v1(newToken, baseURL, jarrayObj as JToken);
                            }
                            else
                                result = jarrayObj;
                        }
                    }
                    else
                        result = jarrayObj;
                    #endregion
                }
                else
                {
                    #region get items from source url for first time
                    var items = token.SelectToken("$.items");
                    var nextUrl = token.SelectToken("$.next");
                    if (items.Count() > 0)
                    {
                        //for (int i = 0; i < (items.Count() < Settings.FeedTraverseLength ? items.Count() : Settings.FeedTraverseLength); i++)
                        //for (int i = 0; i < items.Count() && jarrayObj.Count() < Settings.FeedTraverseLength; i++)
                        //for (int i = 0; i < items.Count() && jarrayObj.Count() < Settings.FeedTraverseLength; i++)
                        for (int i = 0; i < items.Count() && jarrayObj.Count() < Settings.FeedTraverseLength; i++)
                        {
                            var state = items.SelectToken("$.[" + i + "].state");
                            if (state != null)
                            {
                                string strState = state.Value<string>();
                                if (strState != "deleted")
                                    jarrayObj.Add(items[i]);
                            }
                        }
                        if (jarrayObj.Count() < Settings.FeedTraverseLength && nextUrl != null)
                            result = GetLimitedDataForAnalyze_v1(token, baseURL, jarrayObj as JToken);
                        else
                            result = jarrayObj;


                        //if (items.Count() < Settings.FeedTraverseLength)
                        //{
                        //    //var nextUrl = token.SelectToken("$.next");
                        //    //jarrayObj.Add(items?.Children());
                        //    for (int i = 0; i < items.Count(); i++)
                        //    {
                        //        var state = items.SelectToken("$.[" + i + "].state");
                        //        if (state != null)
                        //        {
                        //            string strState = state.Value<string>();
                        //            if (strState != "deleted")
                        //                jarrayObj.Add(items[i]);
                        //        }
                        //    }
                        //    if (nextUrl != null)
                        //        result = GetLimitedDataForAnalyze_v1(token, baseURL, jarrayObj as JToken);
                        //    else
                        //        result = jarrayObj;
                        //}
                        //else
                        //{
                        //    //result = items.Take(Settings.FeedTraverseLength);
                        //    for (int i = 0; i < Settings.FeedTraverseLength; i++)
                        //    {
                        //        var state = items.SelectToken("$.[" + i + "].state");
                        //        if (state != null)
                        //        {
                        //            string strState = state.Value<string>();
                        //            if (strState != "deleted")
                        //                jarrayObj.Add(items[i]);
                        //        }
                        //        //jarrayObj.Add(items[i]);
                        //    }
                        //    if (jarrayObj.Count() < Settings.FeedTraverseLength)
                        //        result = GetLimitedDataForAnalyze_v1(token, baseURL, jarrayObj as JToken);
                        //    else
                        //        result = jarrayObj;
                        //    //result = jarrayObj;
                        //}
                    }
                    #endregion
                }
            }
            return result;
        }
        #endregion

        private string GetNextPageURL(string nextURL, string baseURL)
        {
            string finalURL = "";
            if (nextURL.IndexOf("http") > -1)
            {
                finalURL = nextURL;
            }
            else
            {
                if (nextURL.IndexOf('?') > -1)
                {
                    string urlWithQuery = nextURL.Substring(nextURL.IndexOf('?'));
                    finalURL = baseURL + urlWithQuery;
                }
                else if (nextURL.IndexOf('/') > -1)
                {
                    string urlWithQuery = nextURL.Substring(nextURL.LastIndexOf('/'));
                    finalURL = baseURL + urlWithQuery;
                }
                else
                {
                    finalURL = baseURL;
                }
            }
            return finalURL;
        }

        /// <summary>
        /// Process the feed key from Possible Matches/Hierarchies from Intelligent Mapping
        /// It will break when one of possible matches get found
        /// </summary>
        /// <param name="token"></param>
        /// <param name="index"></param>
        /// <param name="columnName"></param>
        /// <param name="possibleMatches"></param>
        /// <param name="possibleHierarchies"></param>
        /// <param name="objDynamicData"></param>
        /// <param name="customFeedKeyPath"></param>
        /// <param name="actualFeedKeyPath"></param>
        /// <param name="columnDataType"></param>
        /// <returns></returns>
        private bool ProcessFeedKey(JToken token, int index, string columnName, string possibleMatches, string possibleHierarchies, dynamic objDynamicData, ref string customFeedKeyPath, ref string actualFeedKeyPath, ref string columnDataType)
        {
            bool isFound = false;
            string[] possibleMatchesParts = null;
            string[] possibleHierarchiesParts = null;

            if (!string.IsNullOrEmpty(possibleMatches))
                possibleMatchesParts = possibleMatches.Split(',');

            if (!string.IsNullOrEmpty(possibleHierarchies))
                possibleHierarchiesParts = possibleHierarchies.Split(',');

            if (possibleMatchesParts != null && possibleMatchesParts.Length > 0)
            {
                //find possible matches inside each hierarchies and break if first match found
                if (possibleHierarchiesParts != null && possibleHierarchiesParts.Length > 0)
                {
                    for (int i = 0; i < possibleHierarchiesParts.Length; i++)
                    {
                        //break if already found in previous iterations
                        if (isFound)
                            break;

                        if (!string.IsNullOrEmpty(possibleHierarchiesParts[i]))
                        {
                            for (int j = 0; j < possibleMatchesParts.Length; j++)
                            {
                                if (!string.IsNullOrEmpty(possibleMatchesParts[j]))
                                {
                                    //possibleHierarchiesParts[i] = "items[].data.Location[].address";
                                    string newFeedKey = "";
                                    string currenthierarchy = possibleHierarchiesParts[i].Trim();

                                    if (!string.IsNullOrEmpty(actualFeedKeyPath))
                                    {
                                        currenthierarchy = System.Text.RegularExpressions.Regex.Replace(actualFeedKeyPath, @"\[(\d+)\]", "[]");
                                        currenthierarchy = currenthierarchy.Substring(0, currenthierarchy.LastIndexOf('.') + 1);
                                    }

                                    newFeedKey = "$." + currenthierarchy + possibleMatchesParts[j].Trim();
                                    newFeedKey = CommonFunctions.ReplaceFirstOccurrence(newFeedKey, "[]", $"[{index}]");
                                    newFeedKey = newFeedKey.Replace("[]", "[0]");

                                    isFound = FindFeedKeyMatch(token, columnName, newFeedKey, objDynamicData, ref customFeedKeyPath, ref actualFeedKeyPath, ref columnDataType, checkByHierarchy: true);

                                    if (isFound)
                                        break;
                                }
                            }
                        }
                    }
                }

                //if no hierarchies defined or no key found under any hierarchies then try to find key by level
                if (!isFound)
                {
                    for (int j = 0; j < possibleMatchesParts.Length; j++)
                    {
                        if (!string.IsNullOrEmpty(possibleMatchesParts[j]))
                        {
                            isFound = FindFeedKeyMatch(token, columnName, possibleMatchesParts[j].Trim(), objDynamicData, ref customFeedKeyPath, ref actualFeedKeyPath, ref columnDataType);

                            if (isFound)
                                break;
                        }
                    }
                }
            }

            return isFound;
        }

        /// <summary>
        /// find the actual feed ket path from JSON item
        /// </summary>
        /// <param name="jToken"></param>
        /// <param name="columnName"></param>
        /// <param name="feedKey"></param>
        /// <param name="objDynamicData"></param>
        /// <param name="customFeedKeyPath"></param>
        /// <param name="actualFeedKeyPath"></param>
        /// <param name="columnDataType"></param>
        /// <param name="checkByHierarchy"></param>
        /// <returns></returns>
        private bool FindFeedKeyMatch(JToken jToken, string columnName, string feedKey, dynamic objDynamicData, ref string customFeedKeyPath, ref string actualFeedKeyPath, ref string columnDataType, bool checkByHierarchy = false)
        {
            bool isFound = false;
            customFeedKeyPath = string.Empty;
            actualFeedKeyPath = string.Empty;

            if (jToken != null && objDynamicData != null && !string.IsNullOrEmpty(columnName) && !string.IsNullOrEmpty(feedKey))
            {
                try
                {
                    JToken jValue = null;
                    if (checkByHierarchy)
                    {
                        //get value by jsonPath
                        jValue = jToken.Root.SelectToken(feedKey);
                    }
                    else
                    {
                        //get value by only json key name by level
                        var tuple = jToken.FindTokens_V2ByLevel(feedKey).FirstOrDefault();
                        jValue = tuple?.Item1;
                    }

                    if (jValue != null)
                    {
                        customFeedKeyPath = CommonFunctions.GetCustomJsonPath(jValue.Path);
                        actualFeedKeyPath = jValue.Path;

                        columnDataType = GetTokenType(jValue);

                        //add property in dynamic object
                        switch ((int)jValue.Type)
                        {
                            case 1:
                                //object
                                var jObject = jValue.Value<JObject>();
                                CommonFunctions.AddProperty(objDynamicData, columnName, jObject);
                                break;
                            case 2:
                                //array
                                var jArray = jValue.Value<JArray>();
                                CommonFunctions.AddProperty(objDynamicData, columnName, jArray);
                                break;
                            case 6:
                                //integer
                                int integer = jValue.Value<int>();
                                CommonFunctions.AddProperty(objDynamicData, columnName, integer);
                                break;
                            case 7:
                                //float
                                float floatVal = jValue.Value<float>();
                                CommonFunctions.AddProperty(objDynamicData, columnName, floatVal);
                                break;
                            case 8:
                                //string
                                string strVal = jValue.Value<string>();
                                CommonFunctions.AddProperty(objDynamicData, columnName, strVal);
                                break;
                            case 9:
                                //bool
                                bool boolVal = jValue.Value<bool>();
                                CommonFunctions.AddProperty(objDynamicData, columnName, boolVal);
                                break;
                            case 10:
                                //Null
                                CommonFunctions.AddProperty(objDynamicData, columnName, null);
                                break;
                            case 12:
                                //date
                                var date = jValue.Value<DateTime>();
                                CommonFunctions.AddProperty(objDynamicData, columnName, date);
                                break;
                            default:
                                break;
                        }

                        //mark as found if there is any property in dynamic object
                        if (CommonFunctions.IsPropertyExists(objDynamicData, columnName))
                            isFound = true;
                    }
                }
                catch (Exception ex)
                {

                }
            }

            return isFound;
        }

        private string GetTokenType(JToken token)
        {
            string columnDataType = "undefined";

            if (token != null)
            {
                switch ((int)token.Type)
                {
                    case 1:
                        //object
                        columnDataType = "object";
                        break;
                    case 2:
                        //array
                        columnDataType = "array";
                        break;
                    case 6:
                        //integer
                        columnDataType = "integer";
                        break;
                    case 7:
                        //float
                        columnDataType = "float";
                        break;
                    case 8:
                        //string
                        columnDataType = "string";
                        break;
                    case 9:
                        //bool
                        columnDataType = "bool";
                        break;
                    case 10:
                        //Null
                        columnDataType = "undefined";
                        break;
                    case 12:
                        //date
                        columnDataType = "date";
                        break;
                    case 17:
                        //timespan
                        columnDataType = "timespan";
                        break;
                    default:
                        columnDataType = "undefined";
                        break;
                }
            }

            return columnDataType;
        }

        private bool CheckConstraints(JToken jToken, string constraints)
        {
            bool hasMatchedConstraint = false;

            //string value1 = "Event";
            //string value2 = "";
            //string compareOperator = "equal";
            //string targetJsonKeyPath = "items[0].data.type";

            var constraintParts = constraints.Split(new string[] { "||" }, StringSplitOptions.None);

            if (constraintParts.Length > 1)
            {
                try
                {
                    string targetJsonKeyPath = constraintParts[0];
                    string compareOperator = constraintParts[1];
                    string value1 = constraintParts[2];
                    string value2 = "";

                    if (constraintParts.Length > 3)
                        value2 = constraintParts[3];

                    var jtokenTarget = jToken.Root.SelectToken("$." + targetJsonKeyPath);

                    if (jtokenTarget != null)
                    {
                        if (jtokenTarget.Type != JTokenType.Object
                            && jtokenTarget.Type != JTokenType.Array)
                        {
                            #region Filter value
                            switch ((int)jtokenTarget.Type)
                            {
                                case 6:
                                    //integer
                                    int integer = jtokenTarget.Value<int>();

                                    switch (compareOperator)
                                    {
                                        case "equal":
                                            if (integer == Convert.ToInt32(value1))
                                                hasMatchedConstraint = true;
                                            break;
                                        case "not equal":
                                            if (integer != Convert.ToInt32(value1))
                                                hasMatchedConstraint = true;
                                            break;
                                        case "less":
                                            if (integer < Convert.ToInt32(value1))
                                                hasMatchedConstraint = true;
                                            break;
                                        case "less or equal to":
                                            if (integer <= Convert.ToInt32(value1))
                                                hasMatchedConstraint = true;
                                            break;
                                        case "greater":
                                            if (integer > Convert.ToInt32(value1))
                                                hasMatchedConstraint = true;
                                            break;
                                        case "greater or equal to":
                                            if (integer >= Convert.ToInt32(value1))
                                                hasMatchedConstraint = true;
                                            break;
                                        default:
                                            break;
                                    }
                                    break;
                                case 8:
                                    //string
                                    string strVal = jtokenTarget.Value<string>();

                                    switch (compareOperator)
                                    {
                                        case "equal":
                                            if (strVal == value1)
                                                hasMatchedConstraint = true;
                                            break;
                                        case "not equal":
                                            if (strVal != value1)
                                                hasMatchedConstraint = true;
                                            break;
                                        case "contains":
                                            if (strVal.Contains(value1))
                                                hasMatchedConstraint = true;
                                            break;
                                        case "begin with":
                                            if (strVal.StartsWith(value1))
                                                hasMatchedConstraint = true;
                                            break;
                                        case "doesn't begin with":
                                            if (!strVal.StartsWith(value1))
                                                hasMatchedConstraint = true;
                                            break;
                                        case "ends with":
                                            if (!strVal.EndsWith(value1))
                                                hasMatchedConstraint = true;
                                            break;
                                        case "doesn't ends with":
                                            if (!strVal.EndsWith(value1))
                                                hasMatchedConstraint = true;
                                            break;
                                        case "is null":
                                            if (strVal == null)
                                                hasMatchedConstraint = true;
                                            break;
                                        case "is not null":
                                            if (strVal != null)
                                                hasMatchedConstraint = true;
                                            break;
                                        case "is empty":
                                            if (strVal == "")
                                                hasMatchedConstraint = true;
                                            break;
                                        case "is not empty":
                                            if (strVal != "")
                                                hasMatchedConstraint = true;
                                            break;
                                        default:
                                            break;
                                    }

                                    break;
                                default:
                                    break;
                            }
                            #endregion Filter value
                        }
                    }
                }
                catch (Exception ex)
                {

                }
            }

            return hasMatchedConstraint;
        }
        #endregion Analysis methods

        #region JsTree methods
        private List<vmJsTree> CreateJsTree(List<IntelligentFeedMapping> lstFeedMappingAll, List<IntelligentFeedMapping> lstIntelligentMapping = null, bool isRoot = true, bool isDisableFoundKeys = false)
        {
            var lstVmJsTree = new List<vmJsTree>();

            List<IntelligentFeedMapping> lstIntelligentMappingFinal = null;

            if (isRoot)
                lstIntelligentMappingFinal = lstFeedMappingAll.Where(x => x.ParentId == null).ToList();
            else
                lstIntelligentMappingFinal = lstIntelligentMapping;

            if (lstIntelligentMappingFinal != null && lstIntelligentMappingFinal.Count > 0)
            {
                foreach (var intelligentMapping in lstIntelligentMappingFinal)
                {
                    var treeView = new vmJsTree()
                    {
                        Id = intelligentMapping.Id.ToString(),
                        Text = intelligentMapping.ColumnName
                    };

                    dynamic obj = new ExpandoObject();
                    obj.ParentId = intelligentMapping.ParentId;

                    treeView.LiAttributes = obj;

                    treeView.State = null;

                    var lstFeedmappingChildren = lstFeedMappingAll.Where(x => x.ParentId == intelligentMapping.Id).ToList();

                    //disable if feed key found based on param
                    if (!string.IsNullOrEmpty(intelligentMapping.FeedMapping.FeedKey))
                    {
                        treeView.State = new vmJsTreeState()
                        {
                            Disabled = isDisableFoundKeys
                        };
                    }

                    var lstChildren = CreateJsTree(lstFeedMappingAll, lstFeedmappingChildren, isRoot: false, isDisableFoundKeys: isDisableFoundKeys);
                    treeView.Children = lstChildren;

                    if (treeView.Children.Count == 0)
                        treeView.Icon = "jstree-file";

                    lstVmJsTree.Add(treeView);
                }
            }

            return lstVmJsTree;
        }

        /// <summary>
        /// Create JS tree aacording to the purpose e.g disabling the feed key or disabling the object etc or set selected the matched property
        /// </summary>
        /// <param name="containerToken"></param>
        /// <param name="isDisableParentKeys"></param>
        /// <returns></returns>
        private List<vmJsTree> CreateJsTree(JToken containerToken, bool isDisableParentKeys = false)
        {
            var lstVmJsTree = new List<vmJsTree>();

            if (containerToken.Type == JTokenType.Object)
            {
                foreach (JProperty child in containerToken.Children<JProperty>())
                {
                    string customJsonPath = CommonFunctions.GetCustomJsonPath(child.Path);
                    string columnDataType = GetTokenType(child.Value);

                    var treeView = new vmJsTree()
                    {
                        Id = customJsonPath + "||" + child.Path + "||" + columnDataType,
                        Text = child.Name + " (" + columnDataType + ")"
                    };

                    //dynamic liAttr = new ExpandoObject();
                    //var jValue = containerToken.Root.SelectToken("$." + child.Path);
                    //if (jValue != null)
                    //{ 
                    //    liAttr.FeedKeyValue = JsonConvert.SerializeObject(jValue, Formatting.Indented);

                    //    treeView.LiAttributes = JsonConvert.SerializeObject(liAttr);
                    //}

                    var lstChildren = CreateJsTree(child.Value, isDisableParentKeys);
                    treeView.Children = lstChildren;

                    if (treeView.Children.Count == 0)
                    {
                        treeView.Icon = "jstree-file";
                    }
                    else
                    {
                        if (isDisableParentKeys)
                        {
                            treeView.State = new vmJsTreeState()
                            {
                                Disabled = isDisableParentKeys
                            };
                        }
                    }

                    lstVmJsTree.Add(treeView);
                }
            }
            else if (containerToken.Type == JTokenType.Array)
            {
                foreach (JToken child in containerToken.Children())
                {
                    lstVmJsTree = CreateJsTree(child, isDisableParentKeys);
                    break;
                }
            }

            return lstVmJsTree;
        }

        private List<vmJsTree> CreateUpdateJsTree(JToken containerToken, List<vmJsTree> jsTree, bool isDisableParentKeys = false)
        {
            var lstVmJsTree = new List<vmJsTree>();
            if (jsTree != null)
                lstVmJsTree = jsTree;

            if (containerToken.Type == JTokenType.Object)
            {
                foreach (JProperty child in containerToken.Children<JProperty>())
                {
                    string customJsonPath = CommonFunctions.GetCustomJsonPath(child.Path);
                    string columnDataType = GetTokenType(child.Value);

                    var treeView = new vmJsTree()
                    {
                        Id = customJsonPath + "||" + child.Path + "||" + columnDataType,
                        Text = child.Name + " (" + columnDataType + ")"
                    };


                    var lstChildren = CreateUpdateJsTree(child.Value, jsTree?.FirstOrDefault(x => x.Text == treeView.Text)?.Children, isDisableParentKeys);
                    treeView.Children = lstChildren;

                    if (treeView.Children.Count == 0)
                    {
                        treeView.Icon = "jstree-file";
                    }
                    else
                    {
                        if (isDisableParentKeys)
                        {
                            treeView.State = new vmJsTreeState()
                            {
                                Disabled = isDisableParentKeys
                            };
                        }
                    }

                    if (jsTree == null || (jsTree != null && !jsTree.Any(x => x.Text == treeView.Text)))
                    {
                        lstVmJsTree.Add(treeView);
                    }
                }
            }
            else if (containerToken.Type == JTokenType.Array)
            {
                foreach (JToken child in containerToken.Children())
                {
                    CreateUpdateJsTree(child, jsTree, isDisableParentKeys);
                    break;
                }
            }

            return lstVmJsTree;
        }

        private List<vmJsTree> CreateJsTree_v1(JToken containerToken, bool isDisableParentKeys = false, bool isForCustomKey = false)
        {
            var lstVmJsTree = new List<vmJsTree>();

            if (containerToken.Type == JTokenType.Object)
            {
                foreach (JProperty child in containerToken.Children<JProperty>())
                {
                    string customJsonPath = CommonFunctions.GetCustomJsonPath(child.Path);
                    string columnDataType = GetTokenType(child.Value);

                    var treeView = new vmJsTree()
                    {
                        Id = customJsonPath + "||" + child.Path + "||" + columnDataType,
                        Text = child.Name + " (" + columnDataType + ")"
                    };

                    var lstChildren = CreateJsTree_v1(child.Value, isDisableParentKeys, isForCustomKey);
                    treeView.Children = lstChildren;

                    if (treeView.Children.Count == 0)
                    {
                        treeView.Icon = "jstree-file";
                    }
                    else
                    {
                        if (isDisableParentKeys)
                        {
                            treeView.State = new vmJsTreeState()
                            {
                                Disabled = isDisableParentKeys
                            };
                        }
                    }
                    if (isForCustomKey)
                    {
                        treeView.State = new vmJsTreeState()
                        {
                            Disabled = !treeView.Text.StartsWith("beta:") ? true : false
                        };
                    }
                    lstVmJsTree.Add(treeView);
                }
            }
            else if (containerToken.Type == JTokenType.Array)
            {
                foreach (JToken child in containerToken.Children())
                {
                    CreateJsTree_v1(child, isDisableParentKeys, isForCustomKey);
                    break;
                }
            }

            return lstVmJsTree;
        }

        private void FindNodeInTree(vmJsTree jsTree, string text, vmJsTree jsTreeFinal, bool isDisableParentKeys = false)
        {
            if (jsTree != null)
            {
                if (string.IsNullOrEmpty(jsTreeFinal.Id))
                {
                    jsTreeFinal.Id = jsTree.Id;
                    jsTreeFinal.Text = jsTree.Text;
                    jsTreeFinal.Icon = jsTree.Icon;
                    jsTreeFinal.LiAttributes = jsTree.LiAttributes;
                    jsTreeFinal.AAttributes = jsTree.AAttributes;
                }

                jsTreeFinal.State = new vmJsTreeState();

                if (isDisableParentKeys && jsTree.Children != null && jsTree.Children.Count > 0)
                {
                    jsTreeFinal.State.Disabled = true;
                }

                if (!string.IsNullOrEmpty(text))
                {
                    var idParts = jsTree.Id.Split(new string[] { "||" }, StringSplitOptions.None);
                    if (idParts.Length > 0)
                    {
                        if (idParts[0] == text)
                        {
                            jsTreeFinal.State.Selected = true;
                            jsTreeFinal.State.Opened = true;
                        }
                    }
                }

                if (jsTree.Children != null)
                {
                    jsTreeFinal.Children = new List<vmJsTree>();

                    for (int i = 0; i < jsTree.Children.Count; i++)
                    {
                        var jsTreeTemp = new vmJsTree();
                        jsTreeFinal.Children.Add(jsTreeTemp);

                        FindNodeInTree(jsTree.Children[i], text, jsTreeFinal.Children[i], isDisableParentKeys: isDisableParentKeys);
                    }
                }
            }
        }

        private void FindNodeInTree_v1(vmJsTree jsTree, string text, vmJsTree jsTreeFinal, bool isDisableParentKeys = false, bool isForCustomKey = false)
        {
            if (jsTree != null)
            {
                if (string.IsNullOrEmpty(jsTreeFinal.Id))
                {
                    jsTreeFinal.Id = jsTree.Id;
                    jsTreeFinal.Text = jsTree.Text;
                    jsTreeFinal.Icon = jsTree.Icon;
                    jsTreeFinal.LiAttributes = jsTree.LiAttributes;
                    jsTreeFinal.AAttributes = jsTree.AAttributes;
                }

                jsTreeFinal.State = new vmJsTreeState();

                if (isDisableParentKeys && jsTree.Children != null && jsTree.Children.Count > 0)
                {
                    jsTreeFinal.State.Disabled = true;
                }
                jsTreeFinal.State.Disabled = (isForCustomKey && !jsTreeFinal.Text.StartsWith("beta:")) ? true : jsTreeFinal.State.Disabled;

                if (!string.IsNullOrEmpty(text))
                {
                    var idParts = jsTree.Id.Split(new string[] { "||" }, StringSplitOptions.None);
                    if (idParts.Length > 0)
                    {
                        if (idParts[0] == text)
                        {
                            jsTreeFinal.State.Selected = true;
                            jsTreeFinal.State.Opened = true;
                        }
                    }
                }

                if (jsTree.Children != null)
                {
                    jsTreeFinal.Children = new List<vmJsTree>();

                    for (int i = 0; i < jsTree.Children.Count; i++)
                    {
                        var jsTreeTemp = new vmJsTree();
                        jsTreeFinal.Children.Add(jsTreeTemp);

                        var keyName = jsTreeFinal.Text.Split('(')[0].Trim();


                        isForCustomKey = jsTreeFinal.Text.StartsWith("beta") ? false : isForCustomKey;
                        FindNodeInTree_v1(jsTree.Children[i], text, jsTreeFinal.Children[i], isDisableParentKeys: isDisableParentKeys, isForCustomKey: isForCustomKey);
                    }
                }
            }
        }

        private void FindNodeInTree(vmJsTree jsTree, string text, vmJsTree jsTreeFinal, string parentKeyPath, bool isEnableChildKeys = false)
        {
            if (jsTree != null)
            {
                if (string.IsNullOrEmpty(jsTreeFinal.Id))
                {
                    jsTreeFinal.Id = jsTree.Id;
                    jsTreeFinal.Text = jsTree.Text;
                    jsTreeFinal.Icon = jsTree.Icon;
                    jsTreeFinal.LiAttributes = jsTree.LiAttributes;
                    jsTreeFinal.AAttributes = jsTree.AAttributes;
                }

                jsTreeFinal.State = new vmJsTreeState();

                //disable all keys if parent key path is given
                if (!isEnableChildKeys && !string.IsNullOrEmpty(parentKeyPath))
                    jsTreeFinal.State.Disabled = true;

                if (!string.IsNullOrEmpty(text))
                {
                    var idParts = jsTree.Id.Split(new string[] { "||" }, StringSplitOptions.None);
                    if (idParts.Length > 0)
                    {
                        if (idParts[0] == text)
                        {
                            jsTreeFinal.State.Selected = true;
                            jsTreeFinal.State.Opened = true;
                        }
                    }
                }

                if (jsTree.Children != null && jsTree.Children.Count > 0)
                {
                    //enable only chlid keys based on parent key's path
                    if (!string.IsNullOrEmpty(parentKeyPath))
                    {
                        if (isEnableChildKeys)
                        {
                            jsTreeFinal.State.Disabled = false;
                        }
                        else
                        {
                            var idParts = jsTree.Id.Split(new string[] { "||" }, StringSplitOptions.None);
                            if (idParts.Length > 0)
                            {
                                if (idParts[0] == parentKeyPath)
                                {
                                    isEnableChildKeys = true;
                                }
                            }
                        }
                    }

                    jsTreeFinal.Children = new List<vmJsTree>();

                    for (int i = 0; i < jsTree.Children.Count; i++)
                    {
                        var jsTreeTemp = new vmJsTree();
                        jsTreeFinal.Children.Add(jsTreeTemp);

                        FindNodeInTree(jsTree.Children[i], text, jsTreeFinal.Children[i], parentKeyPath, isEnableChildKeys);
                    }

                    //after enabling childrens reset this variable if parent key path is given
                    if (!string.IsNullOrEmpty(parentKeyPath))
                        isEnableChildKeys = false;
                }
            }
        }

        #endregion JsTree methods

        #region View Sample Data
        public ActionResult ViewItemJson(int id, string jsonFileName)
        {
            // if (Session[$"SampleJsonItem_{id}"] != null)
            // {
            //     return Content(
            //         JsonConvert.SerializeObject(Session[$"SampleJsonItem_{id}"], Formatting.Indented)
            //     , "application/json");
            // }
            string contentRootPath = _hostingEnvironment.ContentRootPath;
            var rootFilePath = string.Concat(contentRootPath, "/", Settings.FeedJSONFilePath);
            if (!string.IsNullOrEmpty(jsonFileName))
            {
                dynamic data = "";
                using (StreamReader r = new StreamReader(Path.Combine(rootFilePath, jsonFileName)))
                {
                    string json = r.ReadToEnd();
                    data = JsonConvert.DeserializeObject<dynamic>(json);
                }
                return Content(
                    JsonConvert.SerializeObject(data, Formatting.Indented)
                , "application/json");

            }
            else
            {
                return Json("Something went wrong, please try again soon.");
            }
        }

        public ActionResult ViewFeedKeyJson(int id, string jsonPath)
        {
            if (HttpContext.Session.GetObject<JToken>($"SampleJsonItem_{id}") != null)
            {
                if (!string.IsNullOrEmpty(jsonPath))
                {
                    var jToken = HttpContext.Session.GetObject<JToken>($"SampleJsonItem_{id}");

                    var jValue = jToken.Root.SelectToken("$." + jsonPath);

                    if (jValue != null)
                    {
                        var val = jValue.Value<object>();
                        return Content(
                            JsonConvert.SerializeObject(val, Formatting.Indented)
                        , "application/json");
                    }
                }

                return Content("", "application/json");
            }
            return Json("Something went wrong, please try again soon.");
        }

        public ActionResult ViewFeedKeyJson_v1(int id, string jsonPath, string jsonFileName)
        {
            if (!string.IsNullOrEmpty(jsonPath))
            {
                JToken jToken = null;
                // if (Session[$"SampleJsonItem_{id}"] != null)
                //     jToken = Session[$"SampleJsonItem_{id}"] as JToken;
                // else if (!string.IsNullOrEmpty(jsonFileName))
                string contentRootPath = _hostingEnvironment.ContentRootPath;
                var rootFilePath = string.Concat(contentRootPath, "/", Settings.FeedJSONFilePath);
                if (!string.IsNullOrEmpty(jsonFileName))
                {
                    dynamic data = null;
                    using (StreamReader r = new StreamReader(Path.Combine(rootFilePath, jsonFileName)))
                    {
                        string json = r.ReadToEnd();
                        data = JsonConvert.DeserializeObject<dynamic>(json);
                    }
                    jToken = data as JToken;
                    jsonPath = jsonPath.Substring(jsonPath.LastIndexOf(']') + 1);
                }

                if (jToken != null)
                {
                    jsonPath = jsonPath.Substring(jsonPath.LastIndexOf(']') + 1);
                    var jValue = jToken.Root.SelectTokens("$." + jsonPath).FirstOrDefault();
                    if (jValue != null)
                    {
                        var val = jValue.Value<object>();
                        return Content(
                            JsonConvert.SerializeObject(val, Formatting.Indented)
                        , "application/json");
                    }
                }
            }

            // return Json(
            //         "Something went wrong, please try again soon."
            //     , JsonRequestBehavior.AllowGet);
            return Json("Something went wrong, please try again soon.");
        }

        #endregion View Sample Data       

        public ActionResult EditFieldMapping(int id, string tableName, string columnName, string actualColumnName, string feedKeyPath, string parentFeedKeyPath, string jsonFileName)
        {
            var lstVmJsTree = new List<vmJsTree>();
            string contentRootPath = _hostingEnvironment.ContentRootPath;
            var rootFilePath = string.Concat(contentRootPath, "/", Settings.FeedJSONFilePath);
            // if (Session[$"JsonTree_{id}"] != null)
            //     lstVmJsTree = Session[$"JsonTree_{id}"] as List<vmJsTree>;
            // else if (!string.IsNullOrEmpty(jsonFileName))
            if (!string.IsNullOrEmpty(jsonFileName))
            {
                using (StreamReader r = new StreamReader(Path.Combine(rootFilePath, jsonFileName)))
                {
                    string json = r.ReadToEnd();
                    lstVmJsTree = JsonConvert.DeserializeObject<List<vmJsTree>>(json);
                }
            }
            else
            {
                #region analyze again if above conditions get failed
                var feedProvider = FeedProviderHelper.GetFeedProviderDetail(id);
                var lstIntelligentFeedMapping = new List<IntelligentFeedMapping>();
                #region It's for only saving JSON tree into session
                string finalUrl = feedProvider.Source;

                string json = GetWebContent(finalUrl);

                var token = JToken.Parse(json);

                //analyze based on intelligent mapping of keys                
                AnalyzeFeedData_V2(feedProvider, token, out lstIntelligentFeedMapping);
                #endregion
                // if (Session[$"JsonTree_{id}"] != null)
                //     lstVmJsTree = Session[$"JsonTree_{id}"] as List<vmJsTree>;
                #endregion
            }

            if (lstVmJsTree != null)
            {
                for (int i = 0; i < lstVmJsTree.Count; i++)
                {
                    var vmJsTreeFinal = new vmJsTree();
                    FindNodeInTree(lstVmJsTree[i], feedKeyPath, vmJsTreeFinal, parentFeedKeyPath);
                    lstVmJsTree[i] = vmJsTreeFinal;
                }

                string strTree = JsonConvert.SerializeObject(lstVmJsTree);

                ViewBag.JsonTree = strTree;
            }

            var vmFeedMapping = new vmFeedMapping()
            {
                TableName = tableName,
                ColumnName = columnName,
                FeedKeyPath = feedKeyPath
            };

            ViewBag.ActualColumnName = columnName;

            if (!string.IsNullOrEmpty(vmFeedMapping.ColumnName))
                vmFeedMapping.ColumnName = vmFeedMapping.ColumnName.IndexOf('_') > -1 ? vmFeedMapping.ColumnName.Substring(vmFeedMapping.ColumnName.LastIndexOf('_') + 1) : vmFeedMapping.ColumnName;

            return View(vmFeedMapping);
        }

        #region Custom Feed Key Mapping
        public ActionResult CreateCustomFeedKeyMapping(int id, string jsonFileName)
        {
            List<vmJsTree> lstVmJsTree = null;
            var vmCustomFeedMapping = new vmCustomFeedMapping();

            // if (Session[$"JsonTreeDisableFoundKeys_{id}"] != null)
            //     lstVmJsTree = Session[$"JsonTreeDisableFoundKeys_{id}"] as List<vmJsTree>;
            // else if (!string.IsNullOrEmpty(jsonFileName))
            string contentRootPath = _hostingEnvironment.ContentRootPath;
            var rootFilePath = string.Concat(contentRootPath, "/", Settings.FeedJSONFilePath);
            if (!string.IsNullOrEmpty(jsonFileName))
            {
                using (StreamReader r = new StreamReader(Path.Combine(rootFilePath, jsonFileName)))
                {
                    string json = r.ReadToEnd();
                    lstVmJsTree = JsonConvert.DeserializeObject<List<vmJsTree>>(json);
                }
            }
            else
            {
                #region analyze again if above conditions get failed
                var feedProvider = FeedProviderHelper.GetFeedProviderDetail(id);
                var lstIntelligentFeedMapping = new List<IntelligentFeedMapping>();
                #region It's for only saving JSON tree into session
                string finalUrl = feedProvider.Source;

                string json = GetWebContent(finalUrl);

                var token = JToken.Parse(json);

                //analyze based on intelligent mapping of keys                
                AnalyzeFeedData_V2(feedProvider, token, out lstIntelligentFeedMapping);
                #endregion
                // if (Session[$"JsonTreeDisableFoundKeys_{id}"] != null)
                //     lstVmJsTree = Session[$"JsonTreeDisableFoundKeys_{id}"] as List<vmJsTree>;
                #endregion
            }
            if (lstVmJsTree != null)
            {
                for (int i = 0; i < lstVmJsTree.Count; i++)
                {
                    var vmJsTreeFinal = new vmJsTree();
                    FindNodeInTree(lstVmJsTree[i], "", vmJsTreeFinal, isDisableParentKeys: true);
                    //FindNodeInTree_v1(lstVmJsTree[i], "", vmJsTreeFinal, isDisableParentKeys: true, isForCustomKey: true);
                    lstVmJsTree[i] = vmJsTreeFinal;
                }
            }
            string strTree = JsonConvert.SerializeObject(lstVmJsTree);
            ViewBag.JsonTree = strTree;

            return PartialView("CreateEditCustomFeedKeyMapping", vmCustomFeedMapping);
        }

        [HttpPost]
        public ActionResult CreateCustomFeedKeyMapping(vmCustomFeedMapping model)
        {
            bool status = false;
            string message = "";

            if (ModelState.IsValid)
            {
                var feedMapping = new FeedMapping()
                {
                    Id = model.Id,
                    FeedProvider = new FeedProvider()
                    {
                        Id = model.FeedProviderId
                    },
                    TableName = "Custom",
                    ColumnName = model.CustomKeyName,
                    IsCustomFeedKey = true
                };


                var feedKeyPathParts = model.FeedKeyPath.Split(new string[] { "||" }, StringSplitOptions.None);
                feedMapping.FeedKeyPath = feedKeyPathParts[0];

                if (feedKeyPathParts[0].IndexOf(">") > -1)
                    feedMapping.FeedKey = feedKeyPathParts[0].Substring(feedKeyPathParts[0].LastIndexOf(">") + 1).Trim();
                else
                    feedMapping.FeedKey = feedKeyPathParts[0];

                if (feedKeyPathParts.Length > 1)
                    feedMapping.ActualFeedKeyPath = feedKeyPathParts[1];

                //try
                //{
                long feedMappingId;
                status = FeedConfigHelper.InsertUpdateFeedMapping(feedMapping, out feedMappingId);
                //}
                //catch (Exception ex)
                //{
                //    ViewBag.ErrorMessage = ex.Message;
                //    message = ex.Message;
                //}

                if (status)
                {
                    //Session[$"JsonTreeDisableFoundKeys_{model.FeedProviderId}"] = null;
                    TempData["ResponseStatus"] = status ? "success" : "error";
                    TempData["ResponseMsg"] = "Record inserted successfully";
                }
                else
                {
                    message = "Something went wrong. Please try again soon.";
                }
            }
            return Json(new { status, message });
        }

        [HttpPost]
        public ActionResult CreateCustomFeedKeyMapping_v1(vmCustomFeedMapping model)
        {
            bool status = false;
            string message = "";

            if (ModelState.IsValid)
            {
                #region Insert/Update Intelligent Mapping
                var intelligentMapping = new IntelligentMapping()
                {
                    TableName = "Custom",
                    ColumnName = model.CustomKeyName,
                    IsCustomFeedKey = true,
                };

                if (!string.IsNullOrEmpty(model.FeedKeyPath))
                {
                    var feedKeyPathParts = model.FeedKeyPath?.Split(new string[] { "||" }, StringSplitOptions.None);
                    if (feedKeyPathParts.Length > 1)
                    {
                        intelligentMapping.PossibleMatches = feedKeyPathParts[0]?.Substring(feedKeyPathParts[0].LastIndexOf(">") + 1).Trim();
                        intelligentMapping.PossibleHierarchies = Regex.Replace(feedKeyPathParts[1], "(\\[.*\\])|(\".*\")|('.*')|(\\(.*\\))", "[]");
                    }
                }
                status = FeedConfigHelper.InsertUpdateIntelligentMapping_v1(intelligentMapping);
                #endregion

                #region Insert/Update Feedmapping
                var feedMapping = new FeedMapping()
                {
                    Id = model.Id,
                    FeedProvider = new FeedProvider()
                    {
                        Id = model.FeedProviderId
                    },
                    TableName = "Custom",
                    ColumnName = model.CustomKeyName,
                    IsCustomFeedKey = true,
                };
                if (!string.IsNullOrEmpty(model.FeedKeyPath))
                {
                    var feedKeyPathParts = model.FeedKeyPath?.Split(new string[] { "||" }, StringSplitOptions.None);
                    if (feedKeyPathParts.Length > 1)
                    {
                        feedMapping.FeedKeyPath = feedKeyPathParts[0];
                        feedMapping.FeedKey = feedKeyPathParts[0].IndexOf(">") > -1 ? feedKeyPathParts[0].Substring(feedKeyPathParts[0].LastIndexOf(">") + 1).Trim() : feedKeyPathParts[0];
                        feedMapping.ActualFeedKeyPath = feedKeyPathParts[1];
                        feedMapping.ColumnDataType = feedKeyPathParts[2];
                    }
                }

                long feedMappingId;
                status = FeedConfigHelper.InsertUpdateFeedMapping(feedMapping, out feedMappingId);
                #endregion                

                TempData["ResponseStatus"] = status ? "success" : "error";
                if (status)
                    TempData["ResponseMsg"] = "Record inserted successfully";
                else
                    message = "Something went wrong. Please try again soon.";
            }


            return Json(new { status, message });
        }

        public ActionResult EditCustomFeedKeyMapping(long id, int feedProviderId, string jsonFileName)
        {
            List<vmJsTree> lstVmJsTree = null;
            var vmCustomFeedMapping = new vmCustomFeedMapping();
            string contentRootPath = _hostingEnvironment.ContentRootPath;
            var rootFilePath = string.Concat(contentRootPath, "/", Settings.FeedJSONFilePath);
            //try
            //{
            // if (Session[$"JsonTreeDisableFoundKeys_{feedProviderId}"] != null)
            //     lstVmJsTree = Session[$"JsonTreeDisableFoundKeys_{feedProviderId}"] as List<vmJsTree>;
            if (!string.IsNullOrEmpty(jsonFileName))
            {
                using (StreamReader r = new StreamReader(Path.Combine(rootFilePath, jsonFileName)))
                {
                    string json = r.ReadToEnd();
                    lstVmJsTree = JsonConvert.DeserializeObject<List<vmJsTree>>(json);
                }
            }
            else
            {
                #region analyze again if above conditions get failed
                var feedProvider = FeedProviderHelper.GetFeedProviderDetail(feedProviderId);
                var lstIntelligentFeedMapping = new List<IntelligentFeedMapping>();
                #region It's for only saving JSON tree into session
                string finalUrl = feedProvider.Source;

                string json = GetWebContent(finalUrl);

                var token = JToken.Parse(json);

                //analyze based on intelligent mapping of keys                
                AnalyzeFeedData_V2(feedProvider, token, out lstIntelligentFeedMapping);
                #endregion
                // if (Session[$"JsonTreeDisableFoundKeys_{id}"] != null)
                //     lstVmJsTree = Session[$"JsonTreeDisableFoundKeys_{id}"] as List<vmJsTree>;
                #endregion
            }

            var feedMapping = FeedConfigHelper.GetFeedMappingDetail(id);
            if (feedMapping != null)
            {
                vmCustomFeedMapping = new vmCustomFeedMapping()
                {
                    Id = id,
                    FeedProviderId = feedProviderId,
                    CustomKeyName = feedMapping.ColumnName,
                    FeedKeyPath = feedMapping.FeedKeyPath + "||" + feedMapping.ActualFeedKeyPath
                };
                if (lstVmJsTree != null)
                {
                    for (int i = 0; i < lstVmJsTree.Count; i++)
                    {
                        var vmJsTreeFinal = new vmJsTree();
                        FindNodeInTree(lstVmJsTree[i], feedMapping.FeedKeyPath.ToString(), vmJsTreeFinal, isDisableParentKeys: true);
                        //FindNodeInTree_v1(lstVmJsTree[i], feedMapping.FeedKeyPath.ToString(), vmJsTreeFinal, isDisableParentKeys: true, isForCustomKey: true);
                        lstVmJsTree[i] = vmJsTreeFinal;
                    }

                    string strTree = JsonConvert.SerializeObject(lstVmJsTree);
                    ViewBag.JsonTree = strTree;
                }
            }
            //}
            //catch (Exception ex)
            //{
            //    ViewBag.ErrorMessage = ex.Message;
            //}

            return PartialView("CreateEditCustomFeedKeyMapping", vmCustomFeedMapping);
        }

        [HttpPost]
        public ActionResult EditCustomFeedKeyMapping(vmCustomFeedMapping model)
        {
            bool status = false;
            string message = "";

            if (ModelState.IsValid)
            {
                var feedMapping = new FeedMapping()
                {
                    Id = model.Id,
                    FeedProvider = new FeedProvider()
                    {
                        Id = model.FeedProviderId
                    },
                    TableName = "Custom",
                    ColumnName = model.CustomKeyName,
                    IsCustomFeedKey = true
                };


                var feedKeyPathParts = model.FeedKeyPath.Split(new string[] { "||" }, StringSplitOptions.None);
                feedMapping.FeedKeyPath = feedKeyPathParts[0];

                if (feedKeyPathParts[0].IndexOf(">") > -1)
                    feedMapping.FeedKey = feedKeyPathParts[0].Substring(feedKeyPathParts[0].LastIndexOf(">") + 1).Trim();
                else
                    feedMapping.FeedKey = feedKeyPathParts[0];

                if (feedKeyPathParts.Length > 1)
                    feedMapping.ActualFeedKeyPath = feedKeyPathParts[1];

                //try
                //{
                long feedMappingId;
                status = FeedConfigHelper.InsertUpdateFeedMapping(feedMapping, out feedMappingId);
                //}
                //catch (Exception ex)
                //{
                //    ViewBag.ErrorMessage = ex.Message;
                //    message = ex.Message;
                //}

                if (status)
                {
                    //Session[$"JsonTreeDisableFoundKeys_{model.FeedProviderId}"] = null;
                    TempData["ResponseStatus"] = status ? "success" : "error";
                    TempData["ResponseMsg"] = "Record updated successfully";
                }
                else
                {
                    message = "Something went wrong. Please try again soon.";
                }
            }
            return Json(new { status, message });
        }

        [HttpPost]
        public ActionResult EditCustomFeedKeyMapping_v1(vmCustomFeedMapping model)
        {
            bool status = false;
            string message = "";

            if (ModelState.IsValid)
            {
                #region Insert/Update Intelligent Mapping
                var intelligentMapping = new IntelligentMapping()
                {
                    TableName = "Custom",
                    ColumnName = model.CustomKeyName,
                    IsCustomFeedKey = true,
                };

                if (!string.IsNullOrEmpty(model.FeedKeyPath))
                {
                    var feedKeyPathParts = model.FeedKeyPath?.Split(new string[] { "||" }, StringSplitOptions.None);
                    if (feedKeyPathParts.Length > 1)
                    {
                        intelligentMapping.PossibleMatches = feedKeyPathParts[0]?.Substring(feedKeyPathParts[0].LastIndexOf(">") + 1).Trim();
                        intelligentMapping.PossibleHierarchies = Regex.Replace(feedKeyPathParts[1], "(\\[.*\\])|(\".*\")|('.*')|(\\(.*\\))", "[]");
                    }
                }
                status = FeedConfigHelper.InsertUpdateIntelligentMapping_v1(intelligentMapping);
                #endregion

                #region Insert/Update Feedmapping
                var feedMapping = new FeedMapping()
                {
                    Id = model.Id,
                    FeedProvider = new FeedProvider()
                    {
                        Id = model.FeedProviderId
                    },
                    TableName = "Custom",
                    ColumnName = model.CustomKeyName,
                    IsCustomFeedKey = true,
                };
                if (!string.IsNullOrEmpty(model.FeedKeyPath))
                {
                    var feedKeyPathParts = model.FeedKeyPath?.Split(new string[] { "||" }, StringSplitOptions.None);
                    if (feedKeyPathParts.Length > 1)
                    {
                        feedMapping.FeedKeyPath = feedKeyPathParts[0];
                        feedMapping.FeedKey = feedKeyPathParts[0].IndexOf(">") > -1 ? feedKeyPathParts[0].Substring(feedKeyPathParts[0].LastIndexOf(">") + 1).Trim() : feedKeyPathParts[0];
                        feedMapping.ActualFeedKeyPath = feedKeyPathParts[1];
                        feedMapping.ColumnDataType = feedKeyPathParts[2];
                    }
                }

                long feedMappingId;
                status = FeedConfigHelper.InsertUpdateFeedMapping(feedMapping, out feedMappingId);
                #endregion                                

                TempData["ResponseStatus"] = status ? "success" : "error";
                if (status)
                    TempData["ResponseMsg"] = "Record updated successfully";
                else
                    message = "Something went wrong. Please try again soon.";
            }


            return Json(new { status, message });
        }

        [HttpPost]
        public ActionResult CheckFeedColumnName(int feedProviderId, string columnName, long? id)
        {
            try
            {
                int count = 0;
                //if (!string.IsNullOrEmpty(columnName) && columnName.LastIndexOf('_') > -1)
                //    columnName = columnName.Substring(columnName.LastIndexOf('_')).Trim();

                count = FeedConfigHelper.ValidateFeedMappingColumnName(feedProviderId, columnName, id);


                return Json((count == 0));
            }
            catch (Exception ex)
            {
                return Json(false);
            }
        }

        [HttpPost]
        public ActionResult DeleteCustomFeedKeyMapping(long id, bool effectToIntemapping = false)
        {
            bool status = false;
            string message = "";

            //try
            //{
            status = FeedConfigHelper.DeleteFeedMapping(id, effectToIntemapping: effectToIntemapping);
            //}
            //catch (Exception ex)
            //{
            //    message = "Something went wrong. Please try again soon.";
            //}

            if (status)
            {
                message = "Record deleted successfully";

                //Session["JsonTreeIntelligentMapping"] = null;
                TempData["ResponseStatus"] = status ? "success" : "error";
                TempData["ResponseMsg"] = message;
            }
            else
            {
                message = "Something went wrong. Please try again soon.";
            }
            return Json(new { status, message });
        }

        #endregion Custom Feed Key Mapping

        [HttpPost]
        //public ActionResult ConfirmFeedAnalysis(List<vmFeedMapping> lstFeedMapping,int feedProviderId,bool IsFeedMappingChanged)
        public ActionResult ConfirmFeedAnalysis(ConfirmFeedAnalysisModel model, IFormCollection formdata)
        {
            bool status = false;
            string message = "";

            if (model.IsFeedMappingChanged)
            {
                var lstFeedMappingFinal = new List<FeedMapping>();
                var lstIntelligentMapping = new List<IntelligentMapping>();

                foreach (var feedMapping in model.lstFeedMapping)
                {
                    #region Feed Mapping
                    var feedMappingFinal = new FeedMapping()
                    {
                        FeedProvider = new FeedProvider()
                        {
                            Id = feedMapping.FeedProviderId
                        },
                        ParentId = feedMapping.FeedMappingParentId,
                        TableName = feedMapping.TableName,
                        ColumnName = feedMapping.ColumnName,
                        FeedKey = feedMapping.FeedKeyPath?.Substring(feedMapping.FeedKeyPath.LastIndexOf(">") + 1).Trim(),
                        FeedKeyPath = feedMapping.FeedKeyPath,
                        ActualFeedKeyPath = feedMapping.ActualFeedKeyPath,
                        Constraint = feedMapping.Constraint,
                        ColumnDataType = feedMapping.ColumnDataType,
                        Position = feedMapping.Position,
                        IsCustomFeedKey = feedMapping.IsCustomFeedKey
                    };
                    if (!string.IsNullOrEmpty(feedMapping.FeedKeyPath))
                    {
                        var feedKeyPathParts = feedMapping.FeedKeyPath?.Split(new string[] { "||" }, StringSplitOptions.None);
                        feedMappingFinal.FeedKeyPath = feedKeyPathParts[0];

                        if (feedKeyPathParts.Length > 1)
                            feedMappingFinal.ActualFeedKeyPath = feedKeyPathParts[1];
                    }
                    #endregion

                    #region Intelligent Mapping
                    if (feedMapping.EffectToInteMapping)
                    {
                        var intelligentMapping = new IntelligentMapping()
                        {
                            ParentId = feedMapping.ParentId,
                            TableName = feedMapping.TableName,
                            ColumnName = feedMapping.ColumnName,
                            PossibleMatches = feedMappingFinal.FeedKey
                        };
                        if (!string.IsNullOrEmpty(feedMapping.ActualFeedKeyPath))
                        {
                            //add it to list of possible hierarchies
                            string hierachy = System.Text.RegularExpressions.Regex.Replace(feedMapping.ActualFeedKeyPath, @"\[(\d+)\]", "[]");
                            hierachy = hierachy.Substring(0, hierachy.LastIndexOf('.') + 1);
                            intelligentMapping.PossibleHierarchies = hierachy;
                        }
                        else
                        {
                            //remove it from list of possible hierarchies
                            var existingFeedmapping = FeedConfigHelper.GetFeedMappingDetailByTableColumnName(feedMapping.TableName, feedMapping.ColumnName, model.feedProviderId);
                            if (!string.IsNullOrEmpty(existingFeedmapping?.FeedKey))
                            {
                                intelligentMapping.PossibleMatches = existingFeedmapping.FeedKey;
                                if (!string.IsNullOrEmpty(existingFeedmapping?.ActualFeedKeyPath))
                                {
                                    string hierachy = System.Text.RegularExpressions.Regex.Replace(existingFeedmapping.ActualFeedKeyPath, @"\[(\d+)\]", "[]");
                                    hierachy = hierachy.Substring(0, hierachy.LastIndexOf('.') + 1);
                                    intelligentMapping.PossibleHierarchies = hierachy;
                                }
                                intelligentMapping.RemoveFeedKey = true;
                            }
                        }
                        lstIntelligentMapping.Add(intelligentMapping);
                    }
                    #endregion

                    lstFeedMappingFinal.Add(feedMappingFinal);
                }
                status = FeedConfigHelper.InsertUpdateFeedMapping(lstFeedMappingFinal);
                //bool status1 = FeedConfigHelper.InsertUpdateIntelligentMapping(lstIntelligentMapping);
                bool status1 = FeedConfigHelper.InsertUpdateIntelligentMapping_v1(lstIntelligentMapping);
            }
            else
            {
                status = !model.IsFeedMappingChanged;
            }
            if (status)
            {
                FeedProviderHelper.UpdateFeedProvider_ConfirmFeedMapping(model.feedProviderId, isFeedMappingConfirmed: true);

                TempData["ResponseMsg"] = "Records updated successfully";
                TempData["ResponseStatus"] = "success";

                message = "Records updated successfully";
            }
            else
            {
                message = "Something went wrong. Please try again soon.";
            }


            return Json(new { status, message });
        }

        #region Scheduler settings methods
        //[ResponseCache(NoStore =true, Location = ResponseCacheLocation.None)]
        public ActionResult SchedulerSettings(int id,string Source)
        {
            var vmSchedulerSettings = new vmSchedulerSettings();
            string message = "";
            //try
           // {
           
             var schedulerSettings = SchedulerHelper.GetSchedulerSettingsByFeedProvider(id);
            var urlName = schedulerSettings.FeedProvider.Source;
                var request = (HttpWebRequest)WebRequest.Create(new Uri(urlName));
               request.Method = WebRequestMethods.Http.Get;
               try
               {
               var response = (HttpWebResponse)request.GetResponse();
                 if (response.StatusCode == HttpStatusCode.OK)
                   {
                        if (schedulerSettings != null)
                     {
                        vmSchedulerSettings = new vmSchedulerSettings()
                     {
                        FeedProvider = schedulerSettings.FeedProvider
                     };

                int schedulerFrequencyId = schedulerSettings.SchedulerFrequencyId;

                FillSchedulerFrequency(ref schedulerFrequencyId);

                if (schedulerSettings.Id > 0)
                {
                    vmSchedulerSettings.Id = schedulerSettings.Id;
                    vmSchedulerSettings.liStartDateTime = schedulerSettings.liStartDateTime;
                    vmSchedulerSettings.liExpiryDateTime = schedulerSettings.liExpiryDateTime;
                    vmSchedulerSettings.IsEnabled = schedulerSettings.IsEnabled;

                    switch (schedulerFrequencyId)
                    {
                        case 2:
                            vmSchedulerSettings.RecurranceIntervalHours = schedulerSettings.RecurranceInterval;
                            break;
                        case 3:
                            vmSchedulerSettings.RecurranceIntervalDays = schedulerSettings.RecurranceInterval;
                            break;
                        case 4:
                            vmSchedulerSettings.RecurranceIntervalWeeks = schedulerSettings.RecurranceInterval;
                            break;
                    }

                    vmSchedulerSettings.SelectedMonths = schedulerSettings.RecurranceMonths?.Split(',');
                    vmSchedulerSettings.SelectedDatesInMonth = schedulerSettings.RecurranceDatesInMonth?.Split(',');
                    vmSchedulerSettings.SelectedWeekNos = schedulerSettings.RecurranceWeekNos?.Split(',');
                    vmSchedulerSettings.SelectedDaysInWeekForMonth = schedulerSettings.RecurranceDaysInWeekForMonth?.Split(',');
                }

                vmSchedulerSettings.SchedulerFrequencyId = schedulerFrequencyId;

                vmSchedulerSettings.RecurranceDaysInWeekSelectList = FillDaysInWeek(schedulerSettings.RecurranceDaysInWeek);
                vmSchedulerSettings.RecurranceMonthsSelectList = FillMonths(schedulerSettings.RecurranceMonths);
                vmSchedulerSettings.RecurranceDatesInMonthsSelectList = FillDatesInMonth(schedulerSettings.RecurranceDatesInMonth);
                vmSchedulerSettings.RecurranceWeekNosSelectList = FillWeekNumbersInMonth(schedulerSettings.RecurranceWeekNos);
                vmSchedulerSettings.RecurranceDaysInWeekForMonthSelectList = FillDaysInWeekForMonth(schedulerSettings.RecurranceDaysInWeekForMonth);
         }
                   
            // catch (Exception ex)
            // {
            //     ViewBag.ErrorMessage = ex.Message;
            // }

            
       }
                 
    }
           catch (WebException ex)
               {
                   ViewBag.ErrorMessage = "The Provided URL is faulty and not providing appropriate data anymore so scheduler settings are currently disabled for this url please check with url.";
                                    
                }
               
             return View(vmSchedulerSettings);
        }

        private void FillSchedulerFrequency(ref int schedulerFrequencyId)
        {
            var list = SchedulerHelper.GetSchedulerFrequency();

            //get default value
            if (schedulerFrequencyId <= 0)
            {
                schedulerFrequencyId = list.Where(x => x.Name.ToLower() == "One time".ToLower())
                                            .Select(x => x.Id).FirstOrDefault();
            }

            ViewBag.lstSchedulerFrequency = list;

            //var lstSchedulerFrequency = SchedulerHelper.GetSchedulerFrequency();
            //ViewBag.lstSchedulerFrequency = new SelectList(lstSchedulerFrequency, "Id", "Name", SelectedValue);
        }

        private List<MasterData> FillDaysInWeek(string recurranceDaysInWeeks = null)
        {
            var list = SchedulerHelper.GetDaysInWeek();

            if (!string.IsNullOrEmpty(recurranceDaysInWeeks))
            {
                var recurranceDaysInWeekParts = recurranceDaysInWeeks.Split(',');

                foreach (var recurranceDaysInWeek in recurranceDaysInWeekParts)
                {
                    list.Where(x => x.Name == recurranceDaysInWeek.Trim())
                        .ToList()
                        .ForEach(x => x.IsSelected = true);
                }
            }
            return list;
        }

        private IEnumerable<SelectListItem> FillDaysInWeekForMonth(string recurranceDaysInWeeks = null)
        {
            var list = SchedulerHelper.GetDaysInWeek();

            var recurranceDaysInWeekParts = new List<string>();

            if (!string.IsNullOrEmpty(recurranceDaysInWeeks))
            {
                recurranceDaysInWeekParts = recurranceDaysInWeeks.Split(',').ToList();

                //foreach (var recurranceDaysInWeek in recurranceDaysInWeekParts)
                //{
                //    list.Where(x => x.Name == recurranceDaysInWeek.Trim())
                //        .ToList()
                //        .ForEach(x => x.IsSelected = true);
                //}
            }

            var selectList = new SelectList(list, "Name", "Name", recurranceDaysInWeekParts);

            return selectList;
        }

        private IEnumerable<SelectListItem> FillMonths(string recurranceMonths = null)
        {
            var list = SchedulerHelper.GetMonths();

            List<string> recurranceMonthParts = new List<string>();

            if (!string.IsNullOrEmpty(recurranceMonths))
            {
                recurranceMonthParts = recurranceMonths.Split(',').ToList();

                //foreach (var recurranceMonth in recurranceDaysInWeekParts)
                //{
                //    list.Where(x => x.Name == recurranceMonth.Trim())
                //        .ToList()
                //        .ForEach(x => x.IsSelected = true);
                //}
            }

            var selectList = new SelectList(list, "Name", "Name", recurranceMonthParts);

            return selectList;
        }

        private IEnumerable<SelectListItem> FillDatesInMonth(string recurranceDatesInMonth = null)
        {
            var list = SchedulerHelper.GetDatesInMonths();

            List<string> recurranceDatesInMonthParts = new List<string>();

            if (!string.IsNullOrEmpty(recurranceDatesInMonth))
            {
                recurranceDatesInMonthParts = recurranceDatesInMonth.Split(',').ToList();

                //foreach (var recurranceDateInMonth in recurranceDatesInMonthParts)
                //{
                //    list.Where(x => x.Name == recurranceDateInMonth.Trim())
                //        .ToList()
                //        .ForEach(x => x.IsSelected = true);
                //}
            }

            var selectList = new SelectList(list, "Name", "Name", recurranceDatesInMonthParts);

            return selectList;
        }

        private IEnumerable<SelectListItem> FillWeekNumbersInMonth(string recurranceWeekNumbersInMonth = null)
        {
            var list = SchedulerHelper.GetWeekNumbersInMonth();

            var recurranceWeekNumbersInMonthParts = new List<string>();

            if (!string.IsNullOrEmpty(recurranceWeekNumbersInMonth))
            {
                recurranceWeekNumbersInMonthParts = recurranceWeekNumbersInMonth.Split(',').ToList();

                //foreach (var recurranceWeekNumberInMonth in recurranceWeekNumbersInMonthParts)
                //{
                //    list.Where(x => x.Name == recurranceWeekNumberInMonth.Trim())
                //        .ToList()
                //        .ForEach(x => x.IsSelected = true);
                //}
            }

            var selectList = new SelectList(list, "Id", "Name", recurranceWeekNumbersInMonthParts);

            return selectList;
        }

        [HttpPost]
        public ActionResult EditSchedulerSettings(vmSchedulerSettings model)
        {
            bool status = false;
            string message = "";

            if (ModelState.IsValid)
            {
                //try
                //{
                var schedulerSettings = new SchedulerSettings()
                {
                    Id = model.Id,
                    FeedProvider = new FeedProvider()
                    {
                        Id = model.FeedProviderId
                    },
                    IsEnabled = model.IsEnabled,
                    SchedulerFrequencyId = model.SchedulerFrequencyId
                };

                schedulerSettings.StartDateTime = DateTimeOffset.FromUnixTimeSeconds(model.liStartDateTime).DateTime;

                if (model.liExpiryDateTime != null && model.liExpiryDateTime > 0)
                    schedulerSettings.ExpiryDateTime = DateTimeOffset.FromUnixTimeSeconds((long)model.liExpiryDateTime).DateTime;

                if (model.SchedulerFrequencyId == 2)
                {
                    //hourly
                    schedulerSettings.RecurranceInterval = (int)model.RecurranceIntervalHours;
                }
                else if (model.SchedulerFrequencyId == 3)
                {
                    //daily
                    schedulerSettings.RecurranceInterval = (int)model.RecurranceIntervalDays;
                }
                else if (model.SchedulerFrequencyId == 4)
                {
                    //weekly
                    schedulerSettings.RecurranceInterval = (int)model.RecurranceIntervalWeeks;
                    schedulerSettings.RecurranceDaysInWeek = string.Join(",", model.SelectedDaysInWeek);
                }
                else if (model.SchedulerFrequencyId == 5)
                {
                    //monthly
                    schedulerSettings.RecurranceMonths = string.Join(",", model.SelectedMonths);

                    if (model.IsDatesSelectedInMonth)
                    {
                        schedulerSettings.RecurranceDatesInMonth = string.Join(",", model.SelectedDatesInMonth);
                    }
                    else
                    {
                        schedulerSettings.RecurranceWeekNos = string.Join(",", model.SelectedWeekNos);
                        schedulerSettings.RecurranceDaysInWeekForMonth = string.Join(",", model.SelectedDaysInWeekForMonth);
                    }
                }

                status = SchedulerHelper.InsertUpdateScheduleSettings(schedulerSettings);
                //}
                //catch (Exception ex)
                //{
                //    ViewBag.ErrorMessage = ex.Message;
                //    message = ex.Message;
                //}

                if (status)
                {
                    TempData["ResponseStatus"] = status ? "success" : "error";
                    TempData["ResponseMsg"] = "Record updated successfully";
                }
                else
                {
                    message = "Something went wrong. Please try again soon.";
                }
            }
            else
            {
                message = "Something went wrong. Please try again soon.";
            }
            var errorList = ModelState.Values.SelectMany(m => m.Errors)
                                 .Select(e => e.ErrorMessage)
                                 .ToList();
            return Json(new { status, message });
        }

        public ActionResult ViewSchedulerLog(long id)
        {
            ViewBag.FeedProviderId = id;
            return View();
        }

        public ActionResult GetSchedulerLogFeedProviders(JQueryDataTableParamModel param, long feedProviderId)
        {
            var dataTableResponse = new DataTableResponse();

            string search = ""; //It's indicate blank filter

            if (!string.IsNullOrEmpty(param.sSearch))
                search = param.sSearch;

            int offset = 0;
            int pageSize = param.iDisplayLength;

            //Find page number from the logic
            if (param.iDisplayStart > 0)
            {
                offset = param.iDisplayStart / pageSize;
            }

            var dataTableRequest = new DataTableRequest();
            dataTableRequest.PageNo = offset;
            dataTableRequest.PageSize = pageSize;
            dataTableRequest.Filter = search;
            dataTableRequest.SortField = param.iSortCol_0;
            dataTableRequest.SortOrder = param.sSortDir_0;

            dataTableResponse = SchedulerHelper.GetSchedulerLogByFeedProvider(dataTableRequest, feedProviderId);

            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = dataTableResponse.totalNumberofRecord,
                iTotalDisplayRecords = dataTableResponse.filteredRecord,
                aaData = dataTableResponse.data
            });
        }

        #endregion Scheduler settings methods

        //[HttpPost]
        public ActionResult ActivateDeactivateFeedKey(long id, bool isActive)
        {
            bool status = false;
            string message = "";

            status = FeedConfigHelper.ActivateDeactivateFeedKey(id, isActive);

            if (status)
            {
                message = "Record updated successfully";
                TempData["ResponseStatus"] = status ? "success" : "error";
                TempData["ResponseMsg"] = message;
            }
            else
            {
                message = "Something went wrong. Please try again soon.";
            }
            return Json(new { status, message });
        }

        #region Sample JSON data        

        /// <summary>
        /// Add/Update Properties and their value into Sample Jtoken object which are found by their hierarchy
        /// </summary>
        /// <param name="newToken"></param>
        /// <param name="oldToken"></param>
        /// <returns></returns>
        private JToken CreateUpdateSampleJSON(JToken newToken, JToken oldToken = null)
        {
            JToken token = null;

            if (oldToken == null)
                token = newToken;
            else
            {
                if (oldToken.Type == JTokenType.Object)
                {
                    JObject target = JObject.Parse(JsonConvert.SerializeObject(oldToken));
                    if (newToken.Type == JTokenType.Object)
                    {
                        foreach (JProperty sourceProp in newToken.Children<JProperty>())
                        {
                            JProperty targetProp = target.Property(sourceProp.Name);
                            target[sourceProp.Name] = CreateUpdateSampleJSON(sourceProp?.Value, targetProp?.Value);
                        }
                    }
                    else if (newToken.Type == JTokenType.Array)
                    {
                        foreach (JToken child in newToken.Children())
                        {
                            JProperty targetProp = target.Property(child.ToString());
                            CreateUpdateSampleJSON(child, targetProp?.Value);
                            break;
                        }
                    }
                    token = target as JToken;
                }
                else if (oldToken.Type == JTokenType.Array)
                {
                    if (oldToken.Count() > 0)
                    {
                        for (int i = 0; i < oldToken.Count(); i++)
                        {
                            JToken child = oldToken[i] as JToken;
                            if (child.Type == JTokenType.Object)
                            {
                                JObject target = JObject.Parse(JsonConvert.SerializeObject(child));
                                if (newToken.Type == JTokenType.Object)
                                {
                                    foreach (JProperty sourceProp in newToken.Children<JProperty>())
                                    {
                                        JProperty targetProp = target.Property(sourceProp.Name);
                                        target[sourceProp.Name] = CreateUpdateSampleJSON(sourceProp?.Value, targetProp?.Value);
                                    }
                                    oldToken[i] = target as JToken;
                                }
                                else if (newToken.Type == JTokenType.Array)
                                {
                                    foreach (JToken newToken_child in newToken.Children())
                                    {
                                        oldToken[i] = CreateUpdateSampleJSON(newToken_child, child);
                                        break;
                                    }
                                }
                            }
                            token = oldToken;
                        }
                    }
                    else
                        token = newToken;
                }
                else
                    token = (newToken.Type == JTokenType.Object || newToken.Type == JTokenType.Array) ? newToken : oldToken;
            }
            return token;
        }
        #endregion        

        #region Post Data From AJax
        public class ConfirmFeedAnalysisModel
        {
            public List<vmFeedMapping> lstFeedMapping { get; set; }
            public int feedProviderId { get; set; }
            public bool IsFeedMappingChanged { get; set; }
        }
        #endregion

    }
}

