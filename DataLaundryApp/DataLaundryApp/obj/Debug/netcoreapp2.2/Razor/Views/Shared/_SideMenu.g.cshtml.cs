#pragma checksum "/Applications/08-07-2019/Jishan/#NetCore/Final/DataLaundryApp/DataLaundryApp/Views/Shared/_SideMenu.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "7b5b9062af826f3f1e186152db516943328edbad"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(AspNetCore.Views_Shared__SideMenu), @"mvc.1.0.view", @"/Views/Shared/_SideMenu.cshtml")]
[assembly:global::Microsoft.AspNetCore.Mvc.Razor.Compilation.RazorViewAttribute(@"/Views/Shared/_SideMenu.cshtml", typeof(AspNetCore.Views_Shared__SideMenu))]
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
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"7b5b9062af826f3f1e186152db516943328edbad", @"/Views/Shared/_SideMenu.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"e53a07086c94b630cf9c3f8d7b30af59307c9434", @"/Views/_ViewImports.cshtml")]
    public class Views_Shared__SideMenu : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<dynamic>
    {
        private static readonly global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute __tagHelperAttribute_0 = new global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute("alt", new global::Microsoft.AspNetCore.Html.HtmlString("image"), global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
        private static readonly global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute __tagHelperAttribute_1 = new global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute("class", new global::Microsoft.AspNetCore.Html.HtmlString("img-circle"), global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
        private static readonly global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute __tagHelperAttribute_2 = new global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute("width", new global::Microsoft.AspNetCore.Html.HtmlString("50"), global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
        private static readonly global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute __tagHelperAttribute_3 = new global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute("height", new global::Microsoft.AspNetCore.Html.HtmlString("50"), global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
        private static readonly global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute __tagHelperAttribute_4 = new global::Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute("src", new global::Microsoft.AspNetCore.Html.HtmlString("~/img/data_laundry.png"), global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
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
        private global::Microsoft.AspNetCore.Mvc.Razor.TagHelpers.UrlResolutionTagHelper __Microsoft_AspNetCore_Mvc_Razor_TagHelpers_UrlResolutionTagHelper;
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
            BeginContext(0, 311, true);
            WriteLiteral(@"<nav class=""navbar-default navbar-static-side"" role=""navigation"">
    <div class=""sidebar-collapse"">
        <ul class=""nav metismenu"" id=""side-menu"">
            <li class=""nav-header"">
                    <div class=""dropdown profile-element"">
                        <span>
                            ");
            EndContext();
            BeginContext(311, 90, false);
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("img", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.SelfClosing, "7b5b9062af826f3f1e186152db516943328edbad5313", async() => {
            }
            );
            __Microsoft_AspNetCore_Mvc_Razor_TagHelpers_UrlResolutionTagHelper = CreateTagHelper<global::Microsoft.AspNetCore.Mvc.Razor.TagHelpers.UrlResolutionTagHelper>();
            __tagHelperExecutionContext.Add(__Microsoft_AspNetCore_Mvc_Razor_TagHelpers_UrlResolutionTagHelper);
            __tagHelperExecutionContext.AddHtmlAttribute(__tagHelperAttribute_0);
            __tagHelperExecutionContext.AddHtmlAttribute(__tagHelperAttribute_1);
            __tagHelperExecutionContext.AddHtmlAttribute(__tagHelperAttribute_2);
            __tagHelperExecutionContext.AddHtmlAttribute(__tagHelperAttribute_3);
            __tagHelperExecutionContext.AddHtmlAttribute(__tagHelperAttribute_4);
            await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
            if (!__tagHelperExecutionContext.Output.IsContentModified)
            {
                await __tagHelperExecutionContext.SetOutputContentAsync();
            }
            Write(__tagHelperExecutionContext.Output);
            __tagHelperExecutionContext = __tagHelperScopeManager.End();
            EndContext();
            BeginContext(401, 702, true);
            WriteLiteral(@"
                        </span>
                        <a data-toggle=""dropdown"" class=""dropdown-toggle"" href=""#"">
                            <span class=""clear"">
                                <span class=""block m-t-xs"">
                                    <strong class=""font-bold text-capitalize"">DataLaundry</strong>
                                </span>
                                <span class=""text-muted text-xs block text-capitalize"">Admin<b class=""caret""></b></span>
                            </span>
                        </a>
                        <ul class=""dropdown-menu animated fadeInRight m-t-xs"">                           
                            <li><a");
            EndContext();
            BeginWriteAttribute("href", " href=\"", 1103, "\"", 1142, 1);
#line 18 "/Applications/08-07-2019/Jishan/#NetCore/Final/DataLaundryApp/DataLaundryApp/Views/Shared/_SideMenu.cshtml"
WriteAttributeValue("", 1110, Url.Action("Logout", "Account"), 1110, 32, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginContext(1143, 167, true);
            WriteLiteral(">Logout</a></li>\r\n                        </ul>\r\n                    </div>\r\n                    <div class=\"logo-element\">DL</div>\r\n            </li>\r\n            <li");
            EndContext();
            BeginWriteAttribute("class", " class=\"", 1310, "\"", 1362, 1);
#line 23 "/Applications/08-07-2019/Jishan/#NetCore/Final/DataLaundryApp/DataLaundryApp/Views/Shared/_SideMenu.cshtml"
WriteAttributeValue("", 1318, Html.IsSelected(controller: "FeedProvider"), 1318, 44, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginContext(1363, 21, true);
            WriteLiteral(">\r\n                <a");
            EndContext();
            BeginWriteAttribute("href", " href=\"", 1384, "\"", 1427, 1);
#line 24 "/Applications/08-07-2019/Jishan/#NetCore/Final/DataLaundryApp/DataLaundryApp/Views/Shared/_SideMenu.cshtml"
WriteAttributeValue("", 1391, Url.Action("Index", "FeedProvider"), 1391, 36, false);

#line default
#line hidden
            EndWriteAttribute();
            BeginContext(1428, 145, true);
            WriteLiteral("><i class=\"fa fa-th-large\"></i> <span class=\"nav-label\">Feed Providers</span></a>\r\n            </li>          \r\n        </ul>\r\n    </div>\r\n</nav>");
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
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<dynamic> Html { get; private set; }
    }
}
#pragma warning restore 1591