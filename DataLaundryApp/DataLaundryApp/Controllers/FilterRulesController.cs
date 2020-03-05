using DataLaundryApp.Filters;
using DataLaundryApp.ViewModels;
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
using Microsoft.AspNetCore.Mvc;
using DataLaundryApp.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.Net.NetworkInformation; 
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
namespace DataLaundryApp.Controllers
{
    [Authorise]
    [ExceptionHandlerAttribute]
    //[NoCache]
    public class FilterRulesController : Controller
    {
         private readonly IHostingEnvironment _hostingEnvironment;

        public FilterRulesController(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        // GET: FilterRules  
        #region Displaydata
        [ActionName("Page")]
        public ActionResult Index(int id)
        {
            var FeedData = FeedProviderHelper.GetFeedProviderDetail(id);
          var urlName = FeedData.Source;
        //   var oTuple = new Tuple<FeedProvider, int>(FeedData, id);
             var oTuple =new Tuple<FeedProvider, int>(null,0);

         
                var request = (HttpWebRequest)WebRequest.Create(new Uri(urlName));
                request.Method = WebRequestMethods.Http.Get;
                try{
                    var response = (HttpWebResponse)request.GetResponse();
                    if (response.StatusCode == HttpStatusCode.OK)
                    {

              oTuple = new Tuple<FeedProvider, int>(FeedData, id);
                    }
                }
                catch(Exception ex){
                            ViewBag.ErrorMessage = "Filter rules not found.";

                }
                        return View("Index",oTuple);

        }
        public ActionResult GetAllRuleByFeedID(JQueryDataTableParamModel param, int FeedProviderId)
        {
            var dataTableResponse = new DataTableResponse();

            string search = ""; //It's indicate blank filter

            if (!string.IsNullOrEmpty(param.sSearch))
                search = param.sSearch;

            int offset = 0;
            int pageSize = param.iDisplayLength;


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

            dataTableResponse = FilterRuleHelper.GetAllRuleByFeedID(dataTableRequest, FeedProviderId);

            return Json(new{
                sEcho=param.sEcho,
                iTotalRecords = dataTableResponse.totalNumberofRecord,
                iTotalDisplayRecords = dataTableResponse.filteredRecord,
                aaData = dataTableResponse.data
            });
        }

        #endregion

        #region Delete
        [HttpPost]
        public ActionResult Delete(int id)
        {
            bool status = false;
            string message = "";

            status = FilterRuleHelper.DeleteRule(id);

            if (status)
                message = "Record deleted successfully.";
            else
                message = "Something went wrong. Please try again soon.";

            return Json(new{status,message});
        }
        #endregion

        #region Insert
        public ActionResult Create(int id)
        {
            FeedProvider feedProvider = null;
            vmFilterRule rules = new vmFilterRule();

            feedProvider = FeedProviderHelper.GetFeedProviderDetail(id);
           
            var FieldNameList = FillFieldName(id);
            ViewBag.FieldNameList = FieldNameList;           

            ViewBag.RuleOperator = FilterRuleHelper.GetALLRuleOperator();
            ViewBag.Operator = FilterRuleHelper.GetAllOperator();
            ViewBag.OperationType = FilterRuleHelper.GetAllOperationType();
            var lstIntelligentMapping = FeedConfigHelper.GetFeedIntelligentMapping(id);
            if (feedProvider != null)
            {
                Tuple<FeedProvider, vmFilterRule> tuple = new Tuple<FeedProvider, vmFilterRule>(feedProvider, rules);
                return View(tuple);
            }
            else
            {
                ViewBag.ErrorMessage = "Feed Provider not found";
            }
            return View();
        }

        [HttpPost]
        //[ValidateInput(false)]
        public ActionResult Create(FilterRule Model)
        {

            try
            {
                if (ModelState.IsValid)
                {
                    bool status = FilterRuleHelper.InsertRule(Model);
                    if (status)
                    {
                        TempData["ResponseMsg"] = "Rule created succesfully.";
                        TempData["ResponseStatus"] = "success";
                    }
                    else
                    {
                        TempData["ResponseMsg"] = "Something went wrong.";
                        TempData["ResponseStatus"] = "error";
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Rule Name Is Required.");
                }
            }
            catch (Exception ex)
            {
                TempData["ResponseStatus"] = false;
                TempData["ResponseMsg"] = ex.Message;
            }
            return RedirectToAction("Page", new { id = Model.Id });
        }
        #endregion

        #region Update
        public ActionResult Update(int id, bool IsEnable)
        {

            bool status = false;
            string message = "";

            status = FilterRuleHelper.UpdateRule(id, IsEnable);

            if (status)
                message = "Record update successfully.";
            else
                message = "Something went wrong. Please try again soon.";
           
            return Json(new {status,message});
        }
        #endregion

        #region GetRuleDetail
        public ActionResult GetRuleDetailByID(int id)
        {
            var oFilterRule = FilterRuleHelper.GetRuleDetailByID(id);
            oFilterRule.OperationTypeMaster = FilterRuleHelper.GetAllOperationType();
            return View(oFilterRule);
        }
        #endregion

        #region CommanFunction
        private List<JSTree> FillFieldName(int id)
        {

            var feedMapping = FeedConfigHelper.GetFeedMappingByTableName(id).ToList();
            var feedMappingNotMaped = feedMapping.Where(x => string.IsNullOrEmpty(x.FeedKey)).ToList();

            #region processGetFeedMappingRows
            var parentFeedMapping = feedMapping.Where(x => x.ParentId == null).ToList();
            var feedMappingTree = processGetFeedMappingRows(feedMapping, parentFeedMapping);
            #endregion         

            return feedMappingTree;
        }

        private List<vmJsTree> FillFieldName(int id, string jsonFileName)
        {
            List<vmJsTree> lstVmJsTree = null;
            string contentRootPath = _hostingEnvironment.ContentRootPath;
            var rootFilePath = string.Concat(contentRootPath,"/",Settings.FeedJSONFilePath);
            // if (Session[$"JsonTreeDisableFoundKeys_{id}"] != null)
            //     lstVmJsTree = Session[$"JsonTreeDisableFoundKeys_{id}"] as List<vmJsTree>;
            // else if (!string.IsNullOrEmpty(jsonFileName))
             if (!string.IsNullOrEmpty(jsonFileName))
            {
                using (StreamReader r = new StreamReader(Path.Combine(rootFilePath, jsonFileName)))
                {
                    string json = r.ReadToEnd();
                    lstVmJsTree = JsonConvert.DeserializeObject<List<vmJsTree>>(json);
                }
            }
            if (lstVmJsTree != null)
            {
                for (int i = 0; i < lstVmJsTree.Count; i++)
                {
                    var vmJsTreeFinal = new vmJsTree();
                    FindNodeInTree(lstVmJsTree[i], "", vmJsTreeFinal, isDisableParentKeys: true);
                    lstVmJsTree[i] = vmJsTreeFinal;
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

        private static List<JSTree> processGetFeedMappingRows(List<FeedMapping> lstOrgFeedMapping, List<FeedMapping> lstParentFeedMapping)
        {
            List<JSTree> lstJsTree = new List<JSTree>();

            foreach (var row in lstParentFeedMapping)
            {
                var feedmappingTree = new JSTree()
                {
                    Id = row.Id.ToString(),
                    Text = (string.IsNullOrEmpty(row.FeedKey) ? row.ColumnName : row.FeedKey + "(" + row.ColumnDataType + ")"),
                    ColumnDataType = row.ColumnDataType,
                    IsMatch = (string.IsNullOrEmpty(row.FeedKey) ? false : true),
                };
                var childRows = lstOrgFeedMapping.Where(x => x.ParentId == row.Id).ToList();
                if (childRows?.Count() > 0)
                    feedmappingTree.Children = processGetFeedMappingRows(lstOrgFeedMapping, childRows);

                lstJsTree.Add(feedmappingTree);
            }
            return lstJsTree;
        }
        #endregion`

        #region Auto Flush
        public ActionResult AutoFlush(int FeedProviderID)
        {
            var oResult = FilterRuleHelper.GetAllJsonEventData_V1(FeedProviderID);            
            return Json(oResult);
        }
        #endregion

        #region GetSampleData in Operation - Value Assign
        public ActionResult SampleData(int id, Int64 FeedMappingID, string jsonFileName)
        {
            if (FeedMappingID > 0)
            {
                 string contentRootPath = _hostingEnvironment.ContentRootPath;
                 var rootFilePath = string.Concat(contentRootPath,"/",Settings.FeedJSONFilePath);
                JToken jToken = null;
                if (!string.IsNullOrEmpty(jsonFileName))
                {
                    dynamic data = null;
                    using (StreamReader r = new StreamReader(Path.Combine(rootFilePath, jsonFileName)))
                    {
                        string json = r.ReadToEnd();
                        data = JsonConvert.DeserializeObject<dynamic>(json);
                    }
                    jToken = data as JToken;                    
                }

                if (jToken != null)
                {
                    //DbCall getFeed
                    var lstFeedSampleData = new List<FeedSampleData>();
                    var oResult = MemoryCacheHelper.GetValue("GetSampleData_" + id);
                    if (oResult == null)
                    {
                        lstFeedSampleData = FilterRuleHelper.GetSampleData(id);
                        MemoryCacheHelper.Add("GetSampleData_" + id, lstFeedSampleData, DateTimeOffset.UtcNow.AddHours(1));
                    }
                    else
                    {
                        lstFeedSampleData = oResult as List<FeedSampleData>;
                    }

                    string jsonPath = lstFeedSampleData.FirstOrDefault(x => x.ID == FeedMappingID).ActualFeedKeyPath;
                    if (!string.IsNullOrEmpty(jsonPath))
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
                        else
                            return Content(JsonConvert.SerializeObject("", Formatting.Indented), "application/json");
                    }
                    else
                    {
                        return Content(
                                   JsonConvert.SerializeObject("", Formatting.Indented)
                               , "application/json");

                    }
                }
            }
            return Json("");
        }
        #endregion
    }
}