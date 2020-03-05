#pragma checksum "/Applications/08-07-2019/Jishan/#NetCore/Final/DataLaundryApp/DataLaundryApp/Views/FeedProvider/Create.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "3fc4c4d746b5cbc257ed96d3897ec68c4a0f5c08"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Views_FeedProvider_Create), @"mvc.1.0.view", @"/Views/FeedProvider/Create.cshtml")]
[assembly:global::Microsoft.AspNetCore.Mvc.Razor.Compilation.RazorViewAttribute(@"/Views/FeedProvider/Create.cshtml", typeof(AspNetCore.Views_FeedProvider_Create))]
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
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"3fc4c4d746b5cbc257ed96d3897ec68c4a0f5c08", @"/Views/FeedProvider/Create.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"e53a07086c94b630cf9c3f8d7b30af59307c9434", @"/Views/_ViewImports.cshtml")]
    public class Views_FeedProvider_Create : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<DataLaundryApp.ViewModels.vmFeedProvider>
    {
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
#line 2 "/Applications/08-07-2019/Jishan/#NetCore/Final/DataLaundryApp/DataLaundryApp/Views/FeedProvider/Create.cshtml"
  
    ViewBag.Title = "Add Feed Provider";
    Layout = "~/Views/Shared/_Layout.cshtml";

#line default
#line hidden
            BeginContext(145, 106, true);
            WriteLiteral("\r\n<div class=\"row wrapper border-bottom white-bg page-heading\">\r\n    <div class=\"col-lg-10\">\r\n        <h2>");
            EndContext();
            BeginContext(252, 13, false);
#line 9 "/Applications/08-07-2019/Jishan/#NetCore/Final/DataLaundryApp/DataLaundryApp/Views/FeedProvider/Create.cshtml"
       Write(ViewBag.Title);

#line default
#line hidden
            EndContext();
            BeginContext(265, 89, true);
            WriteLiteral("</h2>\r\n    </div>\r\n</div>\r\n\r\n<div class=\"wrapper wrapper-content animated fadeInRight\">\r\n");
            EndContext();
            BeginContext(395, 62, true);
            WriteLiteral("\r\n    <div class=\"ibox\">\r\n        <div class=\"ibox-content\">\r\n");
            EndContext();
#line 18 "/Applications/08-07-2019/Jishan/#NetCore/Final/DataLaundryApp/DataLaundryApp/Views/FeedProvider/Create.cshtml"
             using (Html.BeginForm("Create", "FeedProvider", FormMethod.Post, new { @id = "FeedProvider_Create", @autocomplete = "on" }))
            {
                

#line default
#line hidden
            BeginContext(628, 23, false);
#line 20 "/Applications/08-07-2019/Jishan/#NetCore/Final/DataLaundryApp/DataLaundryApp/Views/FeedProvider/Create.cshtml"
           Write(Html.AntiForgeryToken());

#line default
#line hidden
            EndContext();
            BeginContext(718, 48, true);
            WriteLiteral("                <label for=\"Name\">Name</label>\r\n");
            EndContext();
            BeginContext(768, 62, true);
            WriteLiteral("                <div class=\"form-group\">\r\n                    ");
            EndContext();
            BeginContext(831, 103, false);
#line 25 "/Applications/08-07-2019/Jishan/#NetCore/Final/DataLaundryApp/DataLaundryApp/Views/FeedProvider/Create.cshtml"
               Write(Html.TextBoxFor(m => m.Name, new { @class = "form-control m-d-xs", @maxlength = "100", @oninput = "" }));

#line default
#line hidden
            EndContext();
            BeginContext(934, 22, true);
            WriteLiteral("\r\n                    ");
            EndContext();
            BeginContext(957, 74, false);
#line 26 "/Applications/08-07-2019/Jishan/#NetCore/Final/DataLaundryApp/DataLaundryApp/Views/FeedProvider/Create.cshtml"
               Write(Html.ValidationMessageFor(m => m.Name, "", new { @class = "text-danger" }));

#line default
#line hidden
            EndContext();
            BeginContext(1031, 26, true);
            WriteLiteral("\r\n                </div>\r\n");
            EndContext();
            BeginContext(1110, 49, true);
            WriteLiteral("                <label for=\"Source\">URL</label>\r\n");
            EndContext();
            BeginContext(1161, 62, true);
            WriteLiteral("                <div class=\"form-group\">\r\n                    ");
            EndContext();
            BeginContext(1224, 90, false);
#line 33 "/Applications/08-07-2019/Jishan/#NetCore/Final/DataLaundryApp/DataLaundryApp/Views/FeedProvider/Create.cshtml"
               Write(Html.TextBoxFor(m => m.Source, new { @class = "form-control m-d-xs", @maxlength = "500" }));

#line default
#line hidden
            EndContext();
            BeginContext(1314, 22, true);
            WriteLiteral("\r\n                    ");
            EndContext();
            BeginContext(1337, 76, false);
#line 34 "/Applications/08-07-2019/Jishan/#NetCore/Final/DataLaundryApp/DataLaundryApp/Views/FeedProvider/Create.cshtml"
               Write(Html.ValidationMessageFor(m => m.Source, "", new { @class = "text-danger" }));

#line default
#line hidden
            EndContext();
            BeginContext(1413, 26, true);
            WriteLiteral("\r\n                </div>\r\n");
            EndContext();
            BeginContext(1500, 63, true);
            WriteLiteral("                <label for=\"FeedDataTypeId\">Data Type</label>\r\n");
            EndContext();
            BeginContext(1565, 62, true);
            WriteLiteral("                <div class=\"form-group\">\r\n                    ");
            EndContext();
            BeginContext(1628, 155, false);
#line 41 "/Applications/08-07-2019/Jishan/#NetCore/Final/DataLaundryApp/DataLaundryApp/Views/FeedProvider/Create.cshtml"
               Write(Html.DropDownListFor(m => m.FeedDataTypeId, new SelectList(ViewBag.FeedDataTypeList, "Id", "Name"), "-- Select --", new { @class = "form-control m-d-xs" }));

#line default
#line hidden
            EndContext();
            BeginContext(1783, 22, true);
            WriteLiteral("\r\n                    ");
            EndContext();
            BeginContext(1806, 84, false);
#line 42 "/Applications/08-07-2019/Jishan/#NetCore/Final/DataLaundryApp/DataLaundryApp/Views/FeedProvider/Create.cshtml"
               Write(Html.ValidationMessageFor(m => m.FeedDataTypeId, "", new { @class = "text-danger" }));

#line default
#line hidden
            EndContext();
            BeginContext(1890, 26, true);
            WriteLiteral("\r\n                </div>\r\n");
            EndContext();
            BeginContext(3745, 187, true);
            WriteLiteral("                <div class=\"form-group clearfix\">\r\n                    <button type=\"button\" id=\"btnSave\" class=\"btn btn-lg btn-primary pull-right\">Save</button>\r\n                </div>\r\n");
            EndContext();
#line 88 "/Applications/08-07-2019/Jishan/#NetCore/Final/DataLaundryApp/DataLaundryApp/Views/FeedProvider/Create.cshtml"
            }

#line default
#line hidden
            BeginContext(3947, 75, true);
            WriteLiteral("        </div>\r\n    </div>\r\n</div>\r\n\r\n<div class=\"jsChanges\">\r\n\r\n</div>\r\n\r\n");
            EndContext();
            DefineSection("scripts", async() => {
                BeginContext(4039, 1642, true);
                WriteLiteral(@"
    <script type=""text/javascript"">
        $(function () {


            //$(""#FeedProvider_Create"").submit(function () {
            //    console.log("" oResult "" + oResult);

            //});
            //$('#FeedProvider_Create #Name').keypress(function (e) {
            //    var regex = new RegExp(""^[a-zA-Z0-9]+$"");
            //    var str = String.fromCharCode(!e.charCode ? e.which : e.charCode);
            //    console.log(""charCode -> "" + e.charCode + "" which -> "" + e.which);
            //    if (regex.test(str)) {
            //        return true;
            //    }
            //    else {
            //        e.preventDefault();
            //        return false;
            //    }
            //});

        });
        $(document).ready(function () {
            var oResult = true;
            $(""#FeedDataTypeId"").val('1');

            $(""#btnSave"").click(function ()
            {
                if (oResult)
                {
                    if ($");
                WriteLiteral(@"(""#FeedProvider_Create"").valid())
                    {
                        $("".btn"").attr(""disabled"", true);
                        $(""#FeedProvider_Create"").submit();
                        return true;
                    }
                }
                else
                {
                    $(""#FeedProvider_Create"").valid();
                    IsName($(""#Name""));
                }
            });

            $(""#Name"").change(function () {
                var oName = $.trim($(this).val());
                if (oName != """" && oName.length > 0) {
                    $.getJSON('");
                EndContext();
                BeginContext(5682, 45, false);
#line 145 "/Applications/08-07-2019/Jishan/#NetCore/Final/DataLaundryApp/DataLaundryApp/Views/FeedProvider/Create.cshtml"
                          Write(Url.Action("IsNameAvailable", "FeedProvider"));

#line default
#line hidden
                EndContext();
                BeginContext(5727, 943, true);
                WriteLiteral(@"',
                       { Name: oName },
                       function (data) {
                           if (data)//already name exists
                           {
                               oResult = false;
                               IsName($(""#Name""));
                               return;
                           } else {
                               oResult = true;
                               return;
                           }
                       }
                       );
                }
            });
        });

        function IsName(elem) {
            if ($(elem).val().length > 0) {
                $(""#Name"").addClass(""input-validation-error"");
                $(""#Name"").removeClass(""valid"");
                $('span[data-valmsg-for=""Name""]').html('<span for=""Name"">The name already exists, please try another name.</span>');
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
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<DataLaundryApp.ViewModels.vmFeedProvider> Html { get; private set; }
    }
}
#pragma warning restore 1591
