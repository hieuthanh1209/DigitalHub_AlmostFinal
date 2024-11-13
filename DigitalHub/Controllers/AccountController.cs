using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using DigitalHub.Models;

namespace DigitalHub.Controllers
{
    public class AccountController : Controller
    {
        private DigitalHub_DBEntities db = new DigitalHub_DBEntities(); // Khởi tạo đối tượng kết nối tới cơ sở dữ liệu

        // Trang đăng nhập Admin
        public ActionResult AdminLogin(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl; // Gửi giá trị returnUrl sang view để điều hướng sau khi đăng nhập thành công
            return View(); // Trả về View đăng nhập
        }

        [HttpPost] // Phương thức POST để xử lý đăng nhập
        public ActionResult AdminLogin(LoginViewModel model, string returnUrl)
        {
            if (ModelState.IsValid) // Kiểm tra nếu mô hình (form) hợp lệ
            {
                // Kiểm tra tài khoản admin có hợp lệ không
                if (IsValidAdminUser(model.Username, model.Password))
                {
                    // Đặt cookie xác thực và lưu thông tin vào session
                    FormsAuthentication.SetAuthCookie(model.Username, model.RememberMe);
                    var adminUser = db.AdminUsers.FirstOrDefault(u => u.NameUser == model.Username);
                    Session["AdminUser"] = adminUser; // Lưu thông tin người dùng vào session
                    Session["UserRole"] = adminUser.RoleUser;  // Lưu vai trò người dùng (Admin hoặc User)

                    // Điều hướng về trang mà người dùng yêu cầu (returnUrl) hoặc trang mặc định nếu không có returnUrl
                    return RedirectToLocal(returnUrl);
                }
                else
                {
                    // Nếu tên đăng nhập hoặc mật khẩu không hợp lệ, thêm lỗi vào ModelState
                    ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
                }
            }
            return View(model); // Trả về View nếu có lỗi hoặc không hợp lệ
        }

        // Kiểm tra tài khoản admin hợp lệ
        private bool IsValidAdminUser(string username, string password)
        {
            // Kiểm tra xem tên đăng nhập và mật khẩu có khớp trong cơ sở dữ liệu không
            return db.AdminUsers.Any(user => user.NameUser == username && user.PasswordUser.Trim() == password.Trim());
        }

        // Điều hướng về trang được yêu cầu hoặc trang mặc định
        private ActionResult RedirectToLocal(string returnUrl)
        {
            // Kiểm tra xem returnUrl có hợp lệ và có chứa đường dẫn "/Products/Index"
            if (Url.IsLocalUrl(returnUrl) && returnUrl.Contains("/Products/Index"))
            {
                return Redirect(returnUrl); // Điều hướng về returnUrl
            }
            // Nếu không có returnUrl hợp lệ, điều hướng về trang mặc định (trang sản phẩm)
            return RedirectToAction("Index", "Products");
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

    }
}
