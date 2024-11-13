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
            var admins = db.AdminUsers.ToList();
            return View(admins);
        }

        // GET: Admin/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(AdminUser admin)
        {
            if (ModelState.IsValid)
            {
                db.AdminUsers.Add(admin); // Password stored as plain text
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(admin);
        }

        // GET: Admin/Edit/5
        public ActionResult Edit(int id)
        {
            var admin = db.AdminUsers.Find(id);
            if (admin == null) return HttpNotFound();
            return View(admin);
        }

        // POST: Admin/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(AdminUser admin)
        {
            if (ModelState.IsValid)
            {
                db.Entry(admin).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(admin);
        }

        // GET: Admin/Details/5
        public ActionResult Details(int id)
        {
            var admin = db.AdminUsers.Find(id);
            if (admin == null) return HttpNotFound();
            return View(admin);
        }

        // GET: Admin/Delete/5
        public ActionResult Delete(int id)
        {
            var admin = db.AdminUsers.Find(id);
            if (admin == null) return HttpNotFound();
            return View(admin);
        }

        // POST: Admin/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var admin = db.AdminUsers.Find(id);
            db.AdminUsers.Remove(admin);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public JsonResult SearchAll(string keyword)
        {
            // Kiểm tra keyword null hoặc rỗng
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return Json(new { products = new List<object>(), orders = new List<object>() }, JsonRequestBehavior.AllowGet);
            }

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

            return Json(new { products, orders }, JsonRequestBehavior.AllowGet);
        }

    }
}
