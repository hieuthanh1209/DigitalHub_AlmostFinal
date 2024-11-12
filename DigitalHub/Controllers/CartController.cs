using DigitalHub.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DigitalHub.Controllers
{
    public class CartController : Controller
    {
        private DigitalHub_DBEntities db = new DigitalHub_DBEntities();

        // GET: Cart
        public ActionResult Index()
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("Login", "Users");
            }

            var cart = GetCart(); // Lấy giỏ hàng từ cơ sở dữ liệu
            return View(cart);
        }

        [HttpPost]
        public ActionResult AddToCart(int productId, int quantity)
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("Login", "Users");
            }

            var currentCustomer = (Customer)Session["TaiKhoan"];
            var product = db.Products.FirstOrDefault(p => p.ProductID == productId);

            if (product != null)
            {
                // Kiểm tra sản phẩm trong ShoppingCartHistory của khách hàng hiện tại
                var cartHistory = db.ShoppingCartHistories
                                    .FirstOrDefault(sc => sc.CustomerID == currentCustomer.IDCus && sc.ProductID == productId);

                if (cartHistory == null)
                {
                    // Thêm sản phẩm mới vào ShoppingCartHistory
                    cartHistory = new ShoppingCartHistory
                    {
                        CustomerID = currentCustomer.IDCus,
                        ProductID = product.ProductID,
                        Quantity = quantity,
                        DateAdded = DateTime.Now
                    };
                    db.ShoppingCartHistories.Add(cartHistory);
                }
                else
                {
                    // Nếu sản phẩm đã có, tăng số lượng
                    cartHistory.Quantity += quantity;
                }

                db.SaveChanges(); // Lưu thay đổi vào cơ sở dữ liệu
            }

            var cart = GetCart();
            decimal subtotalAmount = cart.Items.Sum(i => i.Price * i.Quantity);
            decimal totalAmount = subtotalAmount;

            return Json(new { success = true, subtotalAmount = subtotalAmount, totalAmount = totalAmount, cartCount = cart.Items.Sum(i => i.Quantity) });
        }

        [HttpPost]
        public ActionResult RemoveFromCart(int productId)
        {
            var currentCustomer = (Customer)Session["TaiKhoan"];

            // Tìm và xóa sản phẩm trong ShoppingCartHistory
            var cartHistory = db.ShoppingCartHistories
                                .FirstOrDefault(sc => sc.CustomerID == currentCustomer.IDCus && sc.ProductID == productId);

            if (cartHistory != null)
            {
                db.ShoppingCartHistories.Remove(cartHistory);
                db.SaveChanges();

                var cart = GetCart();
                return Json(new { success = true, subtotalAmount = cart.Total, totalAmount = cart.Total, cartCount = cart.Items.Sum(i => i.Quantity) });
            }

            return Json(new { success = false });
        }

        [HttpPost]
        public ActionResult UpdateQuantity(int productId, int quantity)
        {
            var currentCustomer = (Customer)Session["TaiKhoan"];

            var cartHistory = db.ShoppingCartHistories
                                .FirstOrDefault(sc => sc.CustomerID == currentCustomer.IDCus && sc.ProductID == productId);

            if (cartHistory != null)
            {
                cartHistory.Quantity = quantity;
                db.SaveChanges();

                var cart = GetCart();
                return Json(new { success = true, subtotalAmount = cart.Total, totalAmount = cart.Total, cartCount = cart.Items.Sum(i => i.Quantity) });
            }

            return Json(new { success = false });
        }

        private bool IsUserLoggedIn()
        {
            return Session["TaiKhoan"] != null;
        }

        private Cart GetCart()
        {
            var currentCustomer = (Customer)Session["TaiKhoan"];

            if (currentCustomer == null)
            {
                return new Cart();
            }

            var cartItems = db.ShoppingCartHistories
                              .Where(sc => sc.CustomerID == currentCustomer.IDCus)
                              .Select(sc => new CartItem
                              {
                                  ProductId = sc.ProductID,
                                  ProductName = db.Products
                                                 .Where(p => p.ProductID == sc.ProductID)
                                                 .Select(p => p.NamePro)
                                                 .FirstOrDefault(), // Truy vấn riêng để lấy tên sản phẩm
                                  Image = db.Products
                                            .Where(p => p.ProductID == sc.ProductID)
                                            .Select(p => p.ImagePro)
                                            .FirstOrDefault(), // Truy vấn riêng để lấy hình ảnh sản phẩm
                                  Price = db.Products
                                            .Where(p => p.ProductID == sc.ProductID)
                                            .Select(p => p.DiscountPrice)
                                            .FirstOrDefault() ?? 0m, // Truy vấn riêng để lấy giá, nếu null gán giá mặc định
                                  Quantity = sc.Quantity,
                                  Category = db.Products
                                               .Where(p => p.ProductID == sc.ProductID)
                                               .Select(p => p.Category)
                                               .FirstOrDefault() // Truy vấn riêng để lấy danh mục sản phẩm
                              })
                              .ToList();

            var cart = new Cart
            {
                Items = cartItems
            };

            cart.UpdateTotal();
            return cart;
        }
    }
}
