#pragma checksum "/Applications/08-07-2019/Jishan/#NetCore/Final/DataLaundryApp/DataLaundryApp/Views/FilterRules/Index.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "bcf5ce34ccf0ab10865ecfeb4926fd11bfb0100a"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Views_FilterRules_Index), @"mvc.1.0.view", @"/Views/FilterRules/Index.cshtml")]
[assembly:global::Microsoft.AspNetCore.Mvc.Razor.Compilation.RazorViewAttribute(@"/Views/FilterRules/Index.cshtml", typeof(AspNetCore.Views_FilterRules_Index))]
namespace AspNetCore
{
    #line hidden
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
#line 1 "/Applications/08-07-2019/Jishan/#NetCore/Final/DataLaundryApp/DataLaundryApp/Views/_ViewImports.cshtml"
using DataLaundryApp;

#line default
#line hidden
#line 2 "/Applications/08-07-2019/Jishan/#NetCore/Final/DataLaundryApp/DataLaundryApp/Views/_ViewImports.cshtml"
using DataLaundryApp.Models;

#line default
#line hidden
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"bcf5ce34ccf0ab10865ecfeb4926fd11bfb0100a", @"/Views/FilterRules/Index.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"e53a07086c94b630cf9c3f8d7b30af59307c9434", @"/Views/_ViewImports.cshtml")]
    public class Views_FilterRules_Index : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<Tuple<DataLaundryDAL.DTO.FeedProvider, int>>
    {
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
#line 2 "/Applications/08-07-2019/Jishan/#NetCore/Final/DataLaundryApp/DataLaundryApp/Views/FilterRules/Index.cshtml"
  
    ViewBag.Title = "Filter Rule";
    Layout = "~/Views/Shared/_Layout.cshtml";

#line default
#line hidden
            BeginContext(142, 106, true);
            WriteLiteral("\r\n<div class=\"row wrapper border-bottom white-bg page-heading\">\r\n    <div class=\"col-lg-10\">\r\n        <h2>");
            EndContext();
            BeginContext(249, 13, false);
#line 9 "/Applications/08-07-2019/Jishan/#NetCore/Final/DataLaundryApp/DataLaundryApp/Views/FilterRules/Index.cshtml"
       Write(ViewBag.Title);

#line default
#line hidden
            EndContext();
            BeginContext(262, 89, true);
            WriteLiteral("</h2>\r\n    </div>\r\n</div>\r\n\r\n<div class=\"wrapper wrapper-content animated fadeInRight\">\r\n");
            EndContext();
#line 14 "/Applications/08-07-2019/Jishan/#NetCore/Final/DataLaundryApp/DataLaundryApp/Views/FilterRules/Index.cshtml"
     if (Model.Item1 != null && Model.Item1.Id > 0)
    {

#line default
#line hidden
            BeginContext(411, 82, true);
            WriteLiteral("        <div class=\"row\">\r\n            <div class=\"col-sm-12\">\r\n\r\n                ");
            EndContext();
            BeginContext(494, 37, false);
#line 19 "/Applications/08-07-2019/Jishan/#NetCore/Final/DataLaundryApp/DataLaundryApp/Views/FilterRules/Index.cshtml"
           Write(Html.Partial("_Details", Model.Item1));

#line default
#line hidden
            EndContext();
            BeginContext(531, 1721, true);
            WriteLiteral(@"

            </div>
        </div>
        <div class=""row"">
            <div class=""col-lg-12"">
                <div class=""ibox"">
                    <div class=""ibox-title"">
                        <div class=""row"">
                            <div class=""col-sm-12 b-r pull-right"">
                                <button id=""btnAddNew"" class=""btn btn-primary"" onclick=""OpenAddRule();""><i class=""fa fa-plus""></i> Add Rule</button>&nbsp;&nbsp;
                                <button id=""btnAddNew"" class=""btn btn-primary"" onclick=""AutoFlush(this);""><i class=""fa fa-recycle""></i> Flush Data</button>
                            </div>
                        </div>
                    </div>
                    <div class=""ibox-content"">
                        <div class=""row"">
                            <div class=""col-sm-12 b-r"">
                                <div class=""table-responsive"">
                                    <table id=""FilterRulesDT"" class=""table table-striped no-margins""");
            WriteLiteral(@">
                                        <thead>
                                            <tr>
                                                <th>Name</th>
                                                <th>IsEnable</th>
                                                <th>Action</th>
                                            </tr>
                                        </thead>
                                        <tbody></tbody>
                                    </table>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
");
            EndContext();
#line 55 "/Applications/08-07-2019/Jishan/#NetCore/Final/DataLaundryApp/DataLaundryApp/Views/FilterRules/Index.cshtml"
    }
    else
    {

#line default
#line hidden
            BeginContext(2276, 101, true);
            WriteLiteral("        <div class=\"alert alert-danger\">\r\n                Filter Rule not found\r\n            </div>\r\n");
            EndContext();
#line 61 "/Applications/08-07-2019/Jishan/#NetCore/Final/DataLaundryApp/DataLaundryApp/Views/FilterRules/Index.cshtml"
    }

#line default
#line hidden
            BeginContext(2384, 97, true);
            WriteLiteral("    <div id=\"SessionTimeoutDialog\" class=\"modal\" style=\"overflow: hidden;\">\r\n    </div>\r\n</div>\r\n");
            EndContext();
            DefineSection("scripts", async() => {
                BeginContext(2498, 567, true);
                WriteLiteral(@"
    <script>
        var oTable;
        $(document).ready(function () {
            $("".navbar-minimalize"").trigger(""click"");
            oTable = $('#FilterRulesDT').dataTable({
                ""dom"": '<""top""lf>rt<""bottom""ip><""clear"">',
                ""bPaginate"": true,
                ""bFilter"": true,
                ""autoWidth"": true,
                ""bServerSide"": true,
                ""bProcessing"": true,
                ""pageLength"": 10,
                ""lengthMenu"": [[10, 25, 50, 100], [10, 25, 50, 100]],
                ""sAjaxSource"": """);
                EndContext();
                BeginContext(3066, 83, false);
#line 79 "/Applications/08-07-2019/Jishan/#NetCore/Final/DataLaundryApp/DataLaundryApp/Views/FilterRules/Index.cshtml"
                           Write(Url.Action("GetAllRuleByFeedID", "FilterRules",new { FeedProviderId = Model.Item2}));

#line default
#line hidden
                EndContext();
                BeginContext(3149, 3577, true);
                WriteLiteral(@""",
                ""fnServerData"": function (sSource, aoData, fnCallback) {
                    $.getJSON(sSource, aoData, function (json) {
                        fnCallback(json)
                    }).fail(function (jqxhr, textStatus, error) {
                        if (jqxhr.status == 401)
                            window.location.href = '/Account/Login';
                        else
                            toastr.error(""Something went wrong. Please try again soon."");
                    });
                },
                ""aoColumns"": [
                    { 'mData': 'RuleName' },
                    {
                        ""mData"": function (source) {
                            return source;
                        },
                        ""mRender"": function (resObj) {
                            var html = '';
                            if (resObj.IsEnable) {
                                html = '<input type=""checkbox"" onclick=""UpdateRule(this,' + resObj.Id + ',");
                WriteLiteral(@"' + resObj.IsEnable + ')"" checked="""" title=""Click to disable rule."">';
                            }
                            else {
                                html = '<input type=""checkbox"" onclick=""UpdateRule(this,' + resObj.Id + ',' + resObj.IsEnable + ')"" title=""Click to enable rule."">';
                            }
                            return html;
                        },
                    },
                    {
                        ""mData"": function (source) {
                            return source;
                        },
                        ""mRender"": function (resObj) {
                            var html = '';
                            html += '<a href=""javascript:void(0);"" class=""btn btn-block btn-danger btn-rounded"" onclick=""Delete(' + resObj.Id + ')"">Delete</a>';
                            // html += '<a href=""javascript:void(0);"" class=""btn btn-block btn-primary btn-rounded"" onclick=""Edit(' + resObj.Id + ')"">Edit</a>';
                     ");
                WriteLiteral(@"       html += '<a href=""javascript:void(0);"" class=""btn btn-block btn-success btn-rounded"" onclick=""Details(' + resObj.Id + ')"">Details</a>';
                            return html;
                        },
                        ""bSortable"": false,
                        'searchable': false
                    }
                ]
            });

            $('#SessionTimeoutDialog').dialog({
                autoOpen: false,
                width: 410,
                left: 465,
                resizable: false,
                title: 'Login',
                modal: true,
                open: function (event, ui) {
                    changeDialogPosition();
                    $('#frmLoginPopup').validate({
                        rules: {
                            Email: {
                                required: true
                            },
                            Password: {
                                required: true
                            }
      ");
                WriteLiteral(@"                  },
                        messages: {
                            Email: {
                                required: ""This field is required.""
                            },
                            Password: {
                                required: ""This field is required.""
                            }
                        }
                    });
                }
            });
        });

        function OpenAddRule() {
            location.href = '");
                EndContext();
                BeginContext(6727, 56, false);
#line 156 "/Applications/08-07-2019/Jishan/#NetCore/Final/DataLaundryApp/DataLaundryApp/Views/FilterRules/Index.cshtml"
                        Write(Url.Action("Create","FilterRules",new { id=Model.Item2}));

#line default
#line hidden
                EndContext();
                BeginContext(6783, 483, true);
                WriteLiteral(@"';
        }

        function Delete(id) {
            swal({
                title: 'Are you sure to delete this rule?',
                text: ""You will not be able to recover"",
                type: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#3085d6',
                cancelButtonColor: '#d33',
                confirmButtonText: 'Yes'
            }).then(function () {
                $.ajax({
                    url: '");
                EndContext();
                BeginContext(7267, 35, false);
#line 170 "/Applications/08-07-2019/Jishan/#NetCore/Final/DataLaundryApp/DataLaundryApp/Views/FilterRules/Index.cshtml"
                     Write(Url.Action("Delete", "FilterRules"));

#line default
#line hidden
                EndContext();
                BeginContext(7302, 617, true);
                WriteLiteral(@"',
                    data: { ""id"": id },
                    type: 'POST',
                    success: function (data, textStatus, jQxhr) {
                        hideLoader();
                        if (data.status == true)
                            oTable.fnDraw();
                        else
                            toastr.error(data.message);
                    },
                    error: function (jqXhr, textStatus, errorThrown) {
                        hideLoader();
                        if (jqXhr.status == 401) {
                            $(""#SessionTimeoutDialog"").load('");
                EndContext();
                BeginContext(7921, 44, false);
#line 183 "/Applications/08-07-2019/Jishan/#NetCore/Final/DataLaundryApp/DataLaundryApp/Views/FilterRules/Index.cshtml"
                                                         Write(Url.Action("LoginWithModalPopup", "Account"));

#line default
#line hidden
                EndContext();
                BeginContext(7966, 609, true);
                WriteLiteral(@"/', function (response, status, xhr) {
                                $(""#SessionTimeoutDialog"").dialog(""open"");
                            });
                        }
                        else
                            toastr.error(""Something went wrong. Please try again soon."");
                    }
                });
            });
        }

        function changeDialogPosition() {
            $('.modal').dialog(""option"", ""position"", { my: ""center top"", at: ""center top+50"", of: window });
        }

        function Details(RuleId) {
            window.location.href = """);
                EndContext();
                BeginContext(8576, 46, false);
#line 199 "/Applications/08-07-2019/Jishan/#NetCore/Final/DataLaundryApp/DataLaundryApp/Views/FilterRules/Index.cshtml"
                               Write(Url.Action("GetRuleDetailByID", "FilterRules"));

#line default
#line hidden
                EndContext();
                BeginContext(8622, 547, true);
                WriteLiteral(@"/"" + RuleId;
        }
        function UpdateRule(elem, id, IsEnable) {
            try {
                swal({
                    title: 'Are you sure to ' + (IsEnable ? 'disable' : 'enable') + ' this rule?',
                    type: 'warning',
                    showCancelButton: true,
                    confirmButtonColor: '#3085d6',
                    cancelButtonColor: '#d33',
                    confirmButtonText: 'Yes'
                }).then(function () {
                    $.ajax({
                        url: '");
                EndContext();
                BeginContext(9170, 35, false);
#line 212 "/Applications/08-07-2019/Jishan/#NetCore/Final/DataLaundryApp/DataLaundryApp/Views/FilterRules/Index.cshtml"
                         Write(Url.Action("Update", "FilterRules"));

#line default
#line hidden
                EndContext();
                BeginContext(9205, 706, true);
                WriteLiteral(@"',
                        data: { ""id"": id, IsEnable: (IsEnable ? false : true) },
                        type: 'POST',
                        success: function (data, textStatus, jQxhr) {
                            hideLoader();
                            if (data.status == true)
                                oTable.fnDraw();
                            else
                                toastr.error(data.message);
                        },
                        error: function (jqXhr, textStatus, errorThrown) {
                            hideLoader();
                            if (jqXhr.status == 401) {
                                $(""#SessionTimeoutDialog"").load('");
                EndContext();
                BeginContext(9913, 44, false);
#line 225 "/Applications/08-07-2019/Jishan/#NetCore/Final/DataLaundryApp/DataLaundryApp/Views/FilterRules/Index.cshtml"
                                                             Write(Url.Action("LoginWithModalPopup", "Account"));

#line default
#line hidden
                EndContext();
                BeginContext(9958, 1392, true);
                WriteLiteral(@"/', function (response, status, xhr) {
                                    $(""#SessionTimeoutDialog"").dialog(""open"");
                                });
                            }
                            else
                                toastr.error(""Something went wrong. Please try again soon."");
                        }
                    });
                },
                function (dismiss)
                {
                    if ($(elem).is("":checked"")) {
                        $(elem).prop(""checked"", false);
                    }
                    else {
                        $(elem).prop(""checked"", true);
                    }
                });
            }
            catch (e) {
                console.log(""Error : UpdateRule : "" + e.message);
            }
        }

        function AutoFlush(elem)
        {
            try
            {
                swal({
                    title: 'Flush data?',
                    text: 'Are you sure to");
                WriteLiteral(@" all rule applied in existing  data?',
                    type: 'warning',
                    showCancelButton: true,
                    confirmButtonColor: '#3085d6',
                    cancelButtonColor: '#d33',
                    confirmButtonText: 'Yes'
                }).then(function () {
                    $.ajax({
                        url: '");
                EndContext();
                BeginContext(11351, 38, false);
#line 263 "/Applications/08-07-2019/Jishan/#NetCore/Final/DataLaundryApp/DataLaundryApp/Views/FilterRules/Index.cshtml"
                         Write(Url.Action("AutoFlush", "FilterRules"));

#line default
#line hidden
                EndContext();
                BeginContext(11389, 54, true);
                WriteLiteral("\',\r\n                        data: { \"FeedProviderID\": ");
                EndContext();
                BeginContext(11444, 11, false);
#line 264 "/Applications/08-07-2019/Jishan/#NetCore/Final/DataLaundryApp/DataLaundryApp/Views/FilterRules/Index.cshtml"
                                             Write(Model.Item2);

#line default
#line hidden
                EndContext();
                BeginContext(11455, 1870, true);
                WriteLiteral(@"},
                        type: 'POST',
                        success: function (data, textStatus, jQxhr)
                        {         
                            debugger;
                            if(data.Item1)
                            {
                                swal(""Flush data"", ""Changed data successfully."", ""success"");
                            }
                            else if(data.Item1 === false && data.Item2 === 1)
                            {
                                swal({
                                    title: ""Flush data"",
                                    text: ""Please create rule first or Enable existing rule."",
                                    type: ""warning"",
                                    showCancelButton: false,
                                    confirmButtonClass: ""btn-danger"",
                                    confirmButtonText: ""OK"",
                                });
                            }
                ");
                WriteLiteral(@"            else
                            {   
                                swal({
                                    title: ""Flush data"",
                                    text: ""Something went wrong."",
                                    type: ""warning"",
                                    showCancelButton: false,
                                    confirmButtonClass: ""btn-danger"",
                                    confirmButtonText: ""OK"",
                                });
                            }
                            hideLoader();
                        },
                        error: function (jqXhr, textStatus, errorThrown) {
                            hideLoader();
                            if (jqXhr.status == 401) {
                                $(""#SessionTimeoutDialog"").load('");
                EndContext();
                BeginContext(13327, 44, false);
#line 300 "/Applications/08-07-2019/Jishan/#NetCore/Final/DataLaundryApp/DataLaundryApp/Views/FilterRules/Index.cshtml"
                                                             Write(Url.Action("LoginWithModalPopup", "Account"));

#line default
#line hidden
                EndContext();
                BeginContext(13372, 606, true);
                WriteLiteral(@"/', function (response, status, xhr) {
                                    $(""#SessionTimeoutDialog"").dialog(""open"");
                                });
                            }
                            else
                                toastr.error(""Something went wrong. Please try again soon."");
                        }
                    });
                },
                function (dismiss) {
                });
            }
            catch (e)
            {
                console.log(""Error : AutoFlush "" + e.message);
            }
        }
    </script>
");
                EndContext();
            }
            );
        }
        #pragma warning restore 1998
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.ViewFeatures.IModelExpressionProvider ModelExpressionProvider { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IUrlHelper Url { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IViewComponentHelper Component { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IJsonHelper Json { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<Tuple<DataLaundryDAL.DTO.FeedProvider, int>> Html { get; private set; }
    }
}
#pragma warning restore 1591
