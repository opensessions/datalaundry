#pragma checksum "/Applications/08-07-2019/Jishan/#NetCore/Final/DataLaundryApp/DataLaundryApp/Views/FeedProvider/EditFieldMapping.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "67487cb0eb0c13fc69a340010c8edf01fc2d9042"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Views_FeedProvider_EditFieldMapping), @"mvc.1.0.view", @"/Views/FeedProvider/EditFieldMapping.cshtml")]
[assembly:global::Microsoft.AspNetCore.Mvc.Razor.Compilation.RazorViewAttribute(@"/Views/FeedProvider/EditFieldMapping.cshtml", typeof(AspNetCore.Views_FeedProvider_EditFieldMapping))]
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
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"67487cb0eb0c13fc69a340010c8edf01fc2d9042", @"/Views/FeedProvider/EditFieldMapping.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"e53a07086c94b630cf9c3f8d7b30af59307c9434", @"/Views/_ViewImports.cshtml")]
    public class Views_FeedProvider_EditFieldMapping : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<DataLaundryApp.ViewModels.vmFeedMapping>
    {
        private static readonly global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute __tagHelperAttribute_0 = new global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute("id", new global::Microsoft.AspNetCore.Html.HtmlString("frmEditFieldMapping"), global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
        #line hidden
        #pragma warning disable 0169
        private string __tagHelperStringValueBuffer;
        #pragma warning restore 0169
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperExecutionContext __tagHelperExecutionContext;
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperRunner __tagHelperRunner = new global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperRunner();
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperScopeManager __backed__tagHelperScopeManager = null;
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperScopeManager __tagHelperScopeManager
        {
            get
            {
                if (__backed__tagHelperScopeManager == null)
                {
                    __backed__tagHelperScopeManager = new global::Microsoft.AspNetCore.Razor.Runtime.TagHelpers.TagHelperScopeManager(StartTagHelperWritingScope, EndTagHelperWritingScope);
                }
                return __backed__tagHelperScopeManager;
            }
        }
        private global::Microsoft.AspNetCore.Mvc.TagHelpers.FormTagHelper __Microsoft_AspNetCore_Mvc_TagHelpers_FormTagHelper;
        private global::Microsoft.AspNetCore.Mvc.TagHelpers.RenderAtEndOfFormTagHelper __Microsoft_AspNetCore_Mvc_TagHelpers_RenderAtEndOfFormTagHelper;
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
#line 2 "/Applications/08-07-2019/Jishan/#NetCore/Final/DataLaundryApp/DataLaundryApp/Views/FeedProvider/EditFieldMapping.cshtml"
  
    ViewBag.Title = "Edit Feed Mapping";
    Layout = null;

    string jsonTree = "";
    if (ViewBag.JsonTree != null)
    {
        jsonTree = ViewBag.JsonTree;
    }


#line default
#line hidden
            BeginContext(235, 2, true);
            WriteLiteral("\r\n");
            EndContext();
            BeginContext(237, 1794, false);
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("form", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.StartTagAndEndTag, "67487cb0eb0c13fc69a340010c8edf01fc2d90424203", async() => {
                BeginContext(269, 6, true);
                WriteLiteral("\r\n    ");
                EndContext();
                BeginContext(276, 23, false);
#line 15 "/Applications/08-07-2019/Jishan/#NetCore/Final/DataLaundryApp/DataLaundryApp/Views/FeedProvider/EditFieldMapping.cshtml"
Write(Html.AntiForgeryToken());

#line default
#line hidden
                EndContext();
                BeginContext(299, 6, true);
                WriteLiteral("\r\n    ");
                EndContext();
                BeginContext(306, 25, false);
#line 16 "/Applications/08-07-2019/Jishan/#NetCore/Final/DataLaundryApp/DataLaundryApp/Views/FeedProvider/EditFieldMapping.cshtml"
Write(Html.HiddenFor(m => m.Id));

#line default
#line hidden
                EndContext();
                BeginContext(331, 6, true);
                WriteLiteral("\r\n    ");
                EndContext();
                BeginContext(338, 37, false);
#line 17 "/Applications/08-07-2019/Jishan/#NetCore/Final/DataLaundryApp/DataLaundryApp/Views/FeedProvider/EditFieldMapping.cshtml"
Write(Html.HiddenFor(m => m.FeedProviderId));

#line default
#line hidden
                EndContext();
                BeginContext(375, 6, true);
                WriteLiteral("\r\n    ");
                EndContext();
                BeginContext(382, 32, false);
#line 18 "/Applications/08-07-2019/Jishan/#NetCore/Final/DataLaundryApp/DataLaundryApp/Views/FeedProvider/EditFieldMapping.cshtml"
Write(Html.HiddenFor(m => m.TableName));

#line default
#line hidden
                EndContext();
                BeginContext(414, 6, true);
                WriteLiteral("\r\n    ");
                EndContext();
                BeginContext(421, 33, false);
#line 19 "/Applications/08-07-2019/Jishan/#NetCore/Final/DataLaundryApp/DataLaundryApp/Views/FeedProvider/EditFieldMapping.cshtml"
Write(Html.HiddenFor(m => m.ColumnName));

#line default
#line hidden
                EndContext();
                BeginContext(454, 48, true);
                WriteLiteral("\r\n    <input type=\"hidden\" id=\"ActualColumnName\"");
                EndContext();
                BeginWriteAttribute("value", " value=\"", 502, "\"", 535, 1);
#line 20 "/Applications/08-07-2019/Jishan/#NetCore/Final/DataLaundryApp/DataLaundryApp/Views/FeedProvider/EditFieldMapping.cshtml"
WriteAttributeValue("", 510, ViewBag.ActualColumnName, 510, 25, false);

#line default
#line hidden
                EndWriteAttribute();
                BeginContext(536, 118, true);
                WriteLiteral("/>\r\n    <input type=\"hidden\" id=\"FeedKeyBeforeChange\"/>\r\n    \r\n    <div class=\"row\">\r\n        <div class=\"col-md-6\">\r\n");
                EndContext();
                BeginContext(704, 12, true);
                WriteLiteral("            ");
                EndContext();
                BeginContext(717, 24, false);
#line 26 "/Applications/08-07-2019/Jishan/#NetCore/Final/DataLaundryApp/DataLaundryApp/Views/FeedProvider/EditFieldMapping.cshtml"
       Write(Html.Label("Table Name"));

#line default
#line hidden
                EndContext();
                BeginContext(741, 56, true);
                WriteLiteral("\r\n            <div class=\"form-group\">\r\n                ");
                EndContext();
                BeginContext(798, 33, false);
#line 28 "/Applications/08-07-2019/Jishan/#NetCore/Final/DataLaundryApp/DataLaundryApp/Views/FeedProvider/EditFieldMapping.cshtml"
           Write(Html.DisplayFor(m => m.TableName));

#line default
#line hidden
                EndContext();
                BeginContext(831, 72, true);
                WriteLiteral("\r\n            </div>\r\n        </div>\r\n\r\n        <div class=\"col-md-6\">\r\n");
                EndContext();
                BeginContext(954, 12, true);
                WriteLiteral("            ");
                EndContext();
                BeginContext(967, 25, false);
#line 34 "/Applications/08-07-2019/Jishan/#NetCore/Final/DataLaundryApp/DataLaundryApp/Views/FeedProvider/EditFieldMapping.cshtml"
       Write(Html.Label("Column Name"));

#line default
#line hidden
                EndContext();
                BeginContext(992, 56, true);
                WriteLiteral("\r\n            <div class=\"form-group\">\r\n                ");
                EndContext();
                BeginContext(1049, 34, false);
#line 36 "/Applications/08-07-2019/Jishan/#NetCore/Final/DataLaundryApp/DataLaundryApp/Views/FeedProvider/EditFieldMapping.cshtml"
           Write(Html.DisplayFor(m => m.ColumnName));

#line default
#line hidden
                EndContext();
                BeginContext(1083, 60, true);
                WriteLiteral("\r\n            </div>\r\n        </div>\r\n    </div>\r\n    \r\n    ");
                EndContext();
                BeginContext(1144, 34, false);
#line 41 "/Applications/08-07-2019/Jishan/#NetCore/Final/DataLaundryApp/DataLaundryApp/Views/FeedProvider/EditFieldMapping.cshtml"
Write(Html.HiddenFor(m => m.FeedKeyPath));

#line default
#line hidden
                EndContext();
                BeginContext(1178, 6, true);
                WriteLiteral("\r\n    ");
                EndContext();
                BeginContext(1185, 37, false);
#line 42 "/Applications/08-07-2019/Jishan/#NetCore/Final/DataLaundryApp/DataLaundryApp/Views/FeedProvider/EditFieldMapping.cshtml"
Write(Html.HiddenFor(m => m.ColumnDataType));

#line default
#line hidden
                EndContext();
                BeginContext(1222, 6, true);
                WriteLiteral("\r\n    ");
                EndContext();
                BeginContext(1229, 42, false);
#line 43 "/Applications/08-07-2019/Jishan/#NetCore/Final/DataLaundryApp/DataLaundryApp/Views/FeedProvider/EditFieldMapping.cshtml"
Write(Html.HiddenFor(m => m.EffectToInteMapping));

#line default
#line hidden
                EndContext();
                BeginContext(1271, 2, true);
                WriteLiteral("\r\n");
                EndContext();
                BeginContext(1313, 4, true);
                WriteLiteral("    ");
                EndContext();
                BeginContext(1318, 22, false);
#line 45 "/Applications/08-07-2019/Jishan/#NetCore/Final/DataLaundryApp/DataLaundryApp/Views/FeedProvider/EditFieldMapping.cshtml"
Write(Html.Label("Feed Key"));

#line default
#line hidden
                EndContext();
                BeginContext(1340, 106, true);
                WriteLiteral("\r\n\r\n    <div id=\"FeedKeyTree\" class=\"form-group\" style=\"max-height:200px; overflow-y:auto;\"></div>\r\n    \r\n");
                EndContext();
                BeginContext(1682, 342, true);
                WriteLiteral(@"
    <div>
        <i>Notes:</i>
        <ol>
            <li>
                You can only select key within selected feed key mapping of parent in the tree view.
            </li>
            <li>
                You can select any feed key mapping if this feed mapping has no parent.
            </li>
        </ol>
    </div>
");
                EndContext();
            }
            );
            __Microsoft_AspNetCore_Mvc_TagHelpers_FormTagHelper = CreateTagHelper<global::Microsoft.AspNetCore.Mvc.TagHelpers.FormTagHelper>();
            __tagHelperExecutionContext.Add(__Microsoft_AspNetCore_Mvc_TagHelpers_FormTagHelper);
            __Microsoft_AspNetCore_Mvc_TagHelpers_RenderAtEndOfFormTagHelper = CreateTagHelper<global::Microsoft.AspNetCore.Mvc.TagHelpers.RenderAtEndOfFormTagHelper>();
            __tagHelperExecutionContext.Add(__Microsoft_AspNetCore_Mvc_TagHelpers_RenderAtEndOfFormTagHelper);
            __tagHelperExecutionContext.AddHtmlAttribute(__tagHelperAttribute_0);
            await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
            if (!__tagHelperExecutionContext.Output.IsContentModified)
            {
                await __tagHelperExecutionContext.SetOutputContentAsync();
            }
            Write(__tagHelperExecutionContext.Output);
            __tagHelperExecutionContext = __tagHelperScopeManager.End();
            EndContext();
            BeginContext(2031, 59, true);
            WriteLiteral("\r\n\r\n<script>\r\n    $(function () {\r\n        var jsonTree = \'");
            EndContext();
            BeginContext(2091, 18, false);
#line 69 "/Applications/08-07-2019/Jishan/#NetCore/Final/DataLaundryApp/DataLaundryApp/Views/FeedProvider/EditFieldMapping.cshtml"
                   Write(Html.Raw(jsonTree));

#line default
#line hidden
            EndContext();
            BeginContext(2109, 563, true);
            WriteLiteral(@"';

        var jsonTreeObject = JSON.parse(jsonTree);
        $('#FeedKeyTree').jstree({
            'core': {
                ""check_callback"": true,
                'data': jsonTreeObject
            }
        });

        $(""#FeedKeyTree"").bind(""ready.jstree"", function (evt, data) {
            changeDialogPosition();
        });

        $(""#FeedKeyTree"").bind(""select_node.jstree"", function (evt, data) {
            //selected node object: data.node;
            $(""#FeedKeyPath"").val(data.node.id);
        });

    });
</script>

");
            EndContext();
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
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<DataLaundryApp.ViewModels.vmFeedMapping> Html { get; private set; }
    }
}
#pragma warning restore 1591
