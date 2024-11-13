using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using DigitalHub.Models;

namespace DigitalHub.Controllers
{
    public class AccountController : Controller
    {
        private DigitalHub_DBEntities db = new DigitalHub_DBEntities();

        public ActionResult AdminLogin(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        public ActionResult AdminLogin(LoginViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                if (IsValidAdminUser(model.Username, model.Password))
                {
                    FormsAuthentication.SetAuthCookie(model.Username, model.RememberMe);
                    var adminUser = db.AdminUsers.FirstOrDefault(u => u.NameUser == model.Username);
                    Session["AdminUser"] = adminUser;
                    Session["UserRole"] = adminUser.RoleUser;  // Set the role from the database (Admin or User)
                    return RedirectToLocal(returnUrl);
                }
                else
                {
                    ModelState.AddModelError("", "Invalid username or password.");
                }
            }
            return View(model);
        }

        public ActionResult Logout()
        {
            // Xóa bỏ session của admin
            Session["AdminUser"] = null;
            Session["UserRole"] = null;

            // Hủy cookie xác thực (FormsAuthentication)
            FormsAuthentication.SignOut();

            // Chuyển hướng về trang đăng nhập
            return RedirectToAction("AdminLogin", "Account");
        }


        private bool IsValidAdminUser(string username, string password)
        {
            // Directly compare the username and password without hashing
            return db.AdminUsers.Any(user => user.NameUser == username && user.PasswordUser.Trim() == password.Trim());
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            // Check if the returnUrl is valid and points to the Products Index page
            if (Url.IsLocalUrl(returnUrl) && returnUrl.Contains("/Products/Index"))
            {
                return Redirect(returnUrl);
            }
            else
            {
                // If no valid returnUrl is passed, default to Products Index page
                return RedirectToAction("Index", "Products");
            }
        }
    }

}
