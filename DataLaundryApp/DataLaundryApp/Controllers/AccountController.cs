using DataLaundryApp.Filters;
using DataLaundryApp.ViewModels;
using DataLaundryDAL.Helper;
using System;
using System.Web;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using DataLaundryApp.Models;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using DataLaundryDAL.Constants;

namespace DataLaundryApp.Controllers
{
    public class AccountController : Controller
    {
        [Authorise(isSessionOptional: true)]
        //[NoCache]
        [ExceptionHandlerAttribute]
        [HttpGet]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(vmLogin model, string returnUrl)
        {            
            try
            {
                if (ModelState.IsValid)
                {
                    var admin = UserHelper.Login(model.Email, model.Password);

                    if (admin == null || (string.IsNullOrEmpty(admin.Email) && admin.Id <= 0))
                    {
                        ModelState.AddModelError("", "Invalid email or password. Please try again!");
                        return View(model);
                    }
                    HttpContext.Session.SetString("UserID", admin.Id.ToString());
                    HttpContext.Session.SetString("Name", admin.Name.Trim());
                    HttpContext.Session.SetString("Email", admin.Email.Trim());

                    if (!string.IsNullOrEmpty(returnUrl))
                        return Redirect(returnUrl);
                    else
                        return RedirectToAction("Index", "FeedProvider");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            return View(model);
        }

        [HttpGet]
        public ActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public ActionResult SessionTimeout(string returnUrl)
        {
            ViewBag.loadUrl=Settings.GetAppSetting("GetPath");
            return View();
        }

        [HttpGet]
        public ActionResult LoginWithModalPopup()
        {
            var vmLogin = new vmLogin();
            return PartialView("LoginPopup", vmLogin);
        }
        
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult LoginWithModalPopup([FromBody]vmLogin model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var admin = UserHelper.Login(model.Email, model.Password);

                    if (admin == null || (string.IsNullOrEmpty(admin.Email) && admin.Id <= 0))
                    {
                        ModelState.AddModelError("", "Invalid email or password. Please try again!");
                       return Json(
                             new { success = false, message = "Something went wrong, please try again soon." }
                         );
                    }

                    HttpContext.Session.SetString("UserID", admin.Id.ToString());
                    HttpContext.Session.SetString("Name", admin.Name.Trim());
                    HttpContext.Session.SetString("Email", admin.Email.Trim());
		            
                    return Json(new { success = true, message="Login successful."});
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            return Json(
                     "Something went wrong, please try again soon."
                );
        }
       
    }
}