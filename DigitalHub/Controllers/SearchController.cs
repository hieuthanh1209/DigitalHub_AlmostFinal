using DigitalHub.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace DigitalHub.Controllers
{
    public class SearchController : Controller
    {
        private DigitalHub_DBEntities db = new DigitalHub_DBEntities();

        // Action tìm kiếm
        public ActionResult Index(string query)
        {
            // Kiểm tra nếu query không null hoặc rỗng
            if (string.IsNullOrEmpty(query))
            {
                return View(new List<Product>());
            }

            // Tìm kiếm sản phẩm trong cơ sở dữ liệu
            var results = db.Products
                            .Where(p => p.NamePro.Contains(query))
                            .ToList();

            // Trả kết quả về view
            return View(results);
        }
    }
}