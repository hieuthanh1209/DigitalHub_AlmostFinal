using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DigitalHub.Attributes;
using DigitalHub.Models;

namespace DigitalHub.Controllers
{
    [AdminAuthorize]
    public class AdminController : Controller
    {
        private DigitalHub_DBEntities db = new DigitalHub_DBEntities(); // DbContext class

        // GET: Admin
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public JsonResult SearchAll(string keyword)
        {
            // Kiểm tra keyword null hoặc rỗng
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return Json(new { customers = new List<object>(), products = new List<object>(), orders = new List<object>() }, JsonRequestBehavior.AllowGet);
            }

            // Tìm kiếm thông tin khách hàng
            var customers = db.Customers
                .Where(c => c.NameCus.Contains(keyword))
                .Select(c => new { c.NameCus, c.IDCus })
                .ToList();

            // Tìm kiếm thông tin sản phẩm
            var products = db.Products
                .Where(p => p.NamePro.Contains(keyword))
                .Select(p => new { p.NamePro, p.ProductID })
                .ToList();

            // Tìm kiếm đơn hàng
            var orders = db.OrderProes
                .Where(o => o.ID.ToString().Contains(keyword))
                .Select(o => new { o.ID, CustomerName = o.Customer.NameCus })
                .ToList();

            return Json(new { customers, products, orders }, JsonRequestBehavior.AllowGet);
        }
    }
}
