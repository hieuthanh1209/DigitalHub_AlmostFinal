using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DigitalHub.Models;
using System.Net;

namespace DigitalHub.Controllers
{
    public class CategoriesController : Controller
    {
        DigitalHub_DBEntities db = new DigitalHub_DBEntities();
        // GET: Categories
        public ActionResult Index()
        {
            var categories = db.Categories.ToList();
            return View(categories);
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Create(Category category) 
        {
            try
            {
                db.Categories.Add(category);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch
            {
                return Content("Lỗi tạo mới Category");
            }
        }

        public ActionResult Details(int id)
        {
            var category = db.Categories.Where(c => c.ID == id).FirstOrDefault();
            return View(category);
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var category = db.Categories.Where(c => c.ID == id).FirstOrDefault();
            return View(category);
        }
        [HttpPost]
        public ActionResult Edit(Category category)
        {
            
            db.Entry(category).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var category = db.Categories.Where(c => c.ID == id).FirstOrDefault();
            if (category == null)
                return HttpNotFound();
            return View(category);
        }
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                var category = db.Categories.Where(c => c.ID == id).FirstOrDefault();
                db.Categories.Remove(category);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch
            {
                return Content("Không xoá được do có liên quan đến bảng khác");
            }
        }
    }
}