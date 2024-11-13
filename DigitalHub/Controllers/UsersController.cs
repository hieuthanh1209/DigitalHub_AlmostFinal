using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DigitalHub.Models;

namespace DigitalHub.Controllers
{
    public class UsersController : Controller
    {
        private DigitalHub_DBEntities database = new DigitalHub_DBEntities();

        // GET: Users
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(Customer cust)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra nếu email hoặc mật khẩu trống
                if (string.IsNullOrEmpty(cust.EmailCus))
                    ModelState.AddModelError(string.Empty, "Email không được để trống");
                if (string.IsNullOrEmpty(cust.PassCus))
                    ModelState.AddModelError(string.Empty, "Mật khẩu không được để trống");

                // Kiểm tra mật khẩu và xác nhận mật khẩu có khớp không
                if (cust.PassCus != Request.Form["ConfirmPassCus"])
                {
                    ModelState.AddModelError(string.Empty, "Mật khẩu và xác nhận mật khẩu không khớp.");
                }

                // Kiểm tra độ mạnh của mật khẩu (ví dụ: ít nhất 8 ký tự, chứa chữ và số)
                if (!IsPasswordStrong(cust.PassCus))
                {
                    ModelState.AddModelError(string.Empty, "Mật khẩu không đủ mạnh. Cần ít nhất 8 ký tự, chứa chữ và số.");
                }

                var existingCustomer = database.Customers.FirstOrDefault(k => k.EmailCus == cust.EmailCus);
                if (existingCustomer != null)
                {
                    ModelState.AddModelError(string.Empty, "Email này đã được đăng ký.");
                }

                if (ModelState.IsValid)
                {
                    // Lưu thông tin vào cơ sở dữ liệu mà không mã hóa mật khẩu
                    database.Customers.Add(cust);
                    database.SaveChanges();

                    // Thêm thông báo thành công và chuyển hướng đến trang đăng nhập
                    TempData["ThongBao"] = "Tạo tài khoản thành công! Vui lòng đăng nhập.";
                    return RedirectToAction("Login");
                }
            }
            return View();
        }

        // Kiểm tra độ mạnh của mật khẩu
        private bool IsPasswordStrong(string password)
        {
            return password.Length >= 8 && password.Any(char.IsDigit) && password.Any(char.IsLetter);
        }
        // GET: Login
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(Customer cust)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(cust.EmailCus))
                    ModelState.AddModelError(string.Empty, "Email không được để trống");
                if (string.IsNullOrEmpty(cust.PassCus))
                    ModelState.AddModelError(string.Empty, "Mật khẩu không được để trống");

                var khachhang = database.Customers.FirstOrDefault(k => k.EmailCus == cust.EmailCus && k.PassCus == cust.PassCus);
                if (khachhang != null)
                {
                    Session["TaiKhoan"] = khachhang;
                        return RedirectToAction("Index", "Home"); // Redirect regular users to the homepage
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng");
                }
            }
            return View();
        }

        private bool IsUserAdmin()
        {
            return (bool?)Session["IsAdmin"] ?? false;
        }


        // GET: Logout
        [HttpGet]
        public ActionResult Logout()
        {
            // Xóa session
            Session.Clear();
            Session.Abandon();

            // Xóa cookie xác thực nếu có
            if (Request.Cookies["UserCookie"] != null)
            {
                var cookie = new HttpCookie("UserCookie")
                {
                    Expires = DateTime.Now.AddDays(-1)
                };
                Response.Cookies.Add(cookie);
            }

            TempData["ThongBao"] = "Đăng xuất thành công!";
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public ActionResult CustomerInfo()
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("Login");
            }

            var currentCustomer = (Customer)Session["TaiKhoan"];
            var customer = database.Customers.Find(currentCustomer.IDCus);
            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CustomerInfo(Customer updatedCustomer)
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("Login");
            }

            if (ModelState.IsValid)
            {
                var customerInDb = database.Customers.Find(updatedCustomer.IDCus);
                if (customerInDb != null)
                {
                    customerInDb.NameCus = updatedCustomer.NameCus;
                    customerInDb.PhoneCus = updatedCustomer.PhoneCus;
                    customerInDb.AddressCus = updatedCustomer.AddressCus;
                    customerInDb.Gender = updatedCustomer.Gender;
                    customerInDb.BirthDate = updatedCustomer.BirthDate;

                    database.SaveChanges();

                    // Cập nhật lại session
                    Session["TaiKhoan"] = customerInDb;

                    TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
                    return RedirectToAction("CustomerInfo");
                }
                else
                {
                    ModelState.AddModelError("", "Không tìm thấy thông tin khách hàng.");
                }
            }
            return View(updatedCustomer);
        }
        public ActionResult OrderHistory()
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("Login");
            }

            var currentCustomer = (Customer)Session["TaiKhoan"];

            // Lấy danh sách đơn hàng của khách hàng, bao gồm chi tiết đơn hàng và sản phẩm
            var orders = database.OrderProes
                .Where(o => o.IDCus == currentCustomer.IDCus)
                .OrderByDescending(o => o.DateOrder)
                .Include(o => o.OrderDetails.Select(od => od.Product))
                .ToList();

            return View(orders);
        }

        private bool IsUserLoggedIn()
        {
            return Session["TaiKhoan"] != null;
        }
        public ActionResult ViewHistory()
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("Login");
            }

            var currentCustomer = (Customer)Session["TaiKhoan"];

            // Lấy danh sách sản phẩm đã xem
            var viewedProducts = database.ProductViewHistories
                .Where(v => v.CustomerID == currentCustomer.IDCus)
                .OrderByDescending(v => v.ViewDate)
                .Select(v => v.Product)
                .Distinct()
                .ToList();

            return View(viewedProducts);
        }

        [HttpGet]
        public ActionResult ChangePassword()
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("Login");
            }

            // Lấy thông tin khách hàng từ session
            var currentCustomer = (Customer)Session["TaiKhoan"];

            return View(currentCustomer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("Login");
            }

            // Lấy thông tin khách hàng từ session
            var currentCustomer = (Customer)Session["TaiKhoan"];
            var customerInDb = database.Customers.Find(currentCustomer.IDCus);

            // Kiểm tra nếu thông tin khách hàng không tồn tại
            if (customerInDb == null)
            {
                ModelState.AddModelError("", "Không tìm thấy thông tin khách hàng.");
                return View(currentCustomer);
            }

            // Kiểm tra mật khẩu hiện tại trong cơ sở dữ liệu
            var CheckMK = database.Customers.FirstOrDefault(k => k.EmailCus == currentCustomer.EmailCus && k.PassCus == currentPassword);

            // Kiểm tra mật khẩu hiện tại
            if (string.IsNullOrEmpty(currentPassword))
            {
                ModelState.AddModelError("CurrentPassword", "Vui lòng nhập mật khẩu hiện tại.");
            }
            else if (CheckMK == null)
            {
                ModelState.AddModelError("CurrentPassword", "Mật khẩu hiện tại không đúng.");
            }

            // Kiểm tra mật khẩu mới
            if (string.IsNullOrEmpty(newPassword))
            {
                ModelState.AddModelError("NewPassword", "Vui lòng nhập mật khẩu mới.");
            }
            else if (!IsPasswordStrong(newPassword))
            {
                ModelState.AddModelError("NewPassword", "Mật khẩu mới không đủ mạnh. Cần ít nhất 8 ký tự, chứa chữ và số.");
            }

            // Kiểm tra xác nhận mật khẩu mới
            if (string.IsNullOrEmpty(confirmPassword))
            {
                ModelState.AddModelError("ConfirmPassword", "Vui lòng xác nhận mật khẩu mới.");
            }
            else if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "Mật khẩu mới và xác nhận mật khẩu không khớp.");
            }

            // Nếu có lỗi, trả lại view với thông báo lỗi
            if (!ModelState.IsValid)
            {
                return View(customerInDb);
            }

            // Nếu không có lỗi, cập nhật mật khẩu mới
            customerInDb.PassCus = newPassword;
            database.SaveChanges();

            // Cập nhật lại session với thông tin khách hàng mới
            Session["TaiKhoan"] = customerInDb;

            TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
            return RedirectToAction("ChangePassword");
        }
        [HttpPost]
        public JsonResult CheckCurrentPassword(string password)
        {
            // Lấy thông tin khách hàng từ session
            var currentCustomer = (Customer)Session["TaiKhoan"];
            if (currentCustomer == null)
            {
                return Json(new { valid = false });
            }

            // Kiểm tra mật khẩu cũ
            var customerInDb = database.Customers.FirstOrDefault(c => c.IDCus == currentCustomer.IDCus && c.PassCus == password);
            bool isValid = customerInDb != null;
            return Json(new { valid = isValid });
        }
        public ActionResult AddToWishlist(int productId)
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("Login");
            }

            var currentCustomer = (Customer)Session["TaiKhoan"];
            var existingItem = database.Wishlists.FirstOrDefault(w => w.CustomerID == currentCustomer.IDCus && w.ProductID == productId);

            if (existingItem == null)
            {
                var wishlistItem = new Wishlist
                {
                    CustomerID = currentCustomer.IDCus,
                    ProductID = productId
                };

                database.Wishlists.Add(wishlistItem);
                database.SaveChanges();
                TempData["SuccessMessage"] = "Sản phẩm đã được thêm vào danh sách yêu thích.";
            }
            else
            {
                TempData["ErrorMessage"] = "Sản phẩm này đã có trong danh sách yêu thích của bạn.";
            }

            return RedirectToAction("Wishlist");
        }
        public ActionResult RemoveFromWishlist(int productId)
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("Login");
            }

            var currentCustomer = (Customer)Session["TaiKhoan"];
            var wishlistItem = database.Wishlists.FirstOrDefault(w => w.CustomerID == currentCustomer.IDCus && w.ProductID == productId);

            if (wishlistItem != null)
            {
                database.Wishlists.Remove(wishlistItem);
                database.SaveChanges();
                TempData["SuccessMessage"] = "Sản phẩm đã được xóa khỏi danh sách yêu thích.";
            }

            return RedirectToAction("Wishlist");
        }
        public ActionResult Wishlist()
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("Login");
            }

            var currentCustomer = (Customer)Session["TaiKhoan"];
            var wishlist = database.Wishlists
                .Where(w => w.CustomerID == currentCustomer.IDCus)
                .Include(w => w.Product)
                .ToList();

            return View(wishlist);
        }
    }
}
