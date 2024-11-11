using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DigitalHub.Models;
using System.IO;
using DigitalHub.Filters;

namespace DigitalHub.Controllers
{
    public class HomeController : Controller
    {
        private DigitalHub_DBEntities db = new DigitalHub_DBEntities();

        
        public ActionResult Index()
        {
            var products = db.Products.Include(p => p.Category1);
            return View(products.ToList());
        }

        public ActionResult Laptop()
        {
            var products = db.Products.Include(p => p.Category1)
                                      .Where(p => p.Category1.IDCate == "C007");
            return View(products.ToList());
        }

        public ActionResult LinhKien()
        {
            var products = db.Products.Include(p => p.Category1)
                                      .Where(p => p.Category1.IDCate == "C004");
            return View(products.ToList());
        }

        public ActionResult BanPhimChuot()
        {
            var products = db.Products.Include(p => p.Category1)
                                      .Where(p => p.Category1.IDCate == "C005");
            return View(products.ToList());
        }

        public ActionResult TaiNghe()
        {
            var products = db.Products.Include(p => p.Category1)
                                      .Where(p => p.Category1.IDCate == "C006");
            return View(products.ToList());
        }

        // GET: Products/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }

            // Gọi phương thức lưu lịch sử xem sản phẩm
            SaveProductViewHistory(product.ProductID);

            return View(product);
        }
        // Phương thức lưu lịch sử xem sản phẩm
        private void SaveProductViewHistory(int productId)
        {
            if (Session["TaiKhoan"] != null)
            {
                var currentCustomer = (Customer)Session["TaiKhoan"];

                // Kiểm tra xem đã tồn tại bản ghi cho sản phẩm này chưa
                var existingRecord = db.ProductViewHistories.FirstOrDefault(v => v.CustomerID == currentCustomer.IDCus && v.ProductID == productId);

                if (existingRecord != null)
                {
                    // Cập nhật thời gian xem mới nhất
                    existingRecord.ViewDate = DateTime.Now;
                    db.Entry(existingRecord).State = EntityState.Modified;
                }
                else
                {
                    var viewHistory = new ProductViewHistory
                    {
                        CustomerID = currentCustomer.IDCus,
                        ProductID = productId,
                        ViewDate = DateTime.Now
                    };
                    db.ProductViewHistories.Add(viewHistory);
                }

                db.SaveChanges();
            }
        }
    }
}