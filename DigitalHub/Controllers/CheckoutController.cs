using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DigitalHub.Models;

namespace DigitalHub.Controllers
{
    public class CheckoutController : Controller
    {
        private static readonly System.Diagnostics.TraceSource logger = new System.Diagnostics.TraceSource("DigitalHubTraceSource");
        private DigitalHub_DBEntities db = new DigitalHub_DBEntities();

        // GET: Checkout (for displaying checkout form)
        public ActionResult Index()
        {
            // Lấy khách hàng từ session
            var customer = Session["TaiKhoan"] as Customer;
            if (customer == null)
            {
                // Nếu không có khách hàng trong session, điều hướng về trang đăng nhập hoặc trang chủ
                return RedirectToAction("Index", "Home");
            }

            // Lấy giỏ hàng từ bảng ShoppingCartHistory của khách hàng hiện tại
            var cartItems = db.ShoppingCartHistories
                              .Where(sc => sc.CustomerID == customer.IDCus)  // Lọc theo CustomerID
                              .Include(sc => sc.Product)  // Lấy thông tin sản phẩm
                              .Select(sc => new CartItem
                              {
                                  ProductId = sc.ProductID,
                                  ProductName = sc.Product.NamePro,
                                  Image = sc.Product.ImagePro,
                                  Price = sc.Product.DiscountPrice ?? 0m,
                                  Quantity = sc.Quantity,
                                  Category = sc.Product.Category
                              })
                              .ToList();

            // Kiểm tra nếu giỏ hàng không có sản phẩm
            if (!cartItems.Any())
            {
                // Nếu giỏ hàng rỗng, điều hướng về trang giỏ hàng
                return RedirectToAction("Index", "Cart");
            }

            // Tạo đối tượng Cart để truyền vào view
            var cart = new Cart
            {
                Items = cartItems
            };

            return View(cart); // Trả về giỏ hàng với các sản phẩm
        }

        [HttpPost]
        public ActionResult PlaceOrder(string address)
        {
            // Lấy khách hàng từ session
            var customer = Session["TaiKhoan"] as Customer;
            if (customer == null)
            {
                // Nếu không có khách hàng trong session, điều hướng về trang chủ hoặc trang lỗi
                return RedirectToAction("Index", "Home");
            }

            // Truy vấn giỏ hàng từ bảng ShoppingCartHistory của khách hàng hiện tại
            var cartItems = db.ShoppingCartHistories
                              .Where(sc => sc.CustomerID == customer.IDCus)  // Lọc theo CustomerID
                              .Include(sc => sc.Product)  // Lấy thông tin sản phẩm
                              .Select(sc => new CartItem
                              {
                                  ProductId = sc.ProductID,
                                  ProductName = sc.Product.NamePro,
                                  Image = sc.Product.ImagePro,
                                  Price = sc.Product.DiscountPrice ?? 0m,
                                  Quantity = sc.Quantity,
                                  Category = sc.Product.Category
                              })
                              .ToList();

            // Kiểm tra nếu giỏ hàng không có sản phẩm
            if (!cartItems.Any())
            {
                // Nếu giỏ hàng rỗng, điều hướng về trang giỏ hàng
                return RedirectToAction("Index", "Cart");
            }

            // Tạo đơn hàng mới
            var order = new OrderPro
            {
                IDCus = customer.IDCus,  // Lấy customer ID từ session
                DateOrder = DateTime.Now,
                AddressDeliverry = address
            };

            // Thêm đơn hàng vào cơ sở dữ liệu
            db.OrderProes.Add(order);
            db.SaveChanges();

            // Thêm chi tiết đơn hàng
            foreach (var item in cartItems)
            {
                var orderDetail = new OrderDetail
                {
                    IDOrder = order.ID,
                    IDProduct = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Price
                };
                db.OrderDetails.Add(orderDetail);
            }

            db.SaveChanges();

            // Sau khi đặt hàng, xóa giỏ hàng trong ShoppingCartHistory
            var cartHistoryItems = db.ShoppingCartHistories.Where(sc => sc.CustomerID == customer.IDCus).ToList();
            db.ShoppingCartHistories.RemoveRange(cartHistoryItems);
            db.SaveChanges();

            // Điều hướng đến trang xác nhận đơn hàng
            return RedirectToAction("OrderConfirmation", new { id = order.ID });
        }


        public ActionResult OrderConfirmation(int? id)
        {
            if (id == null)
            {
                // Handle the case where id is null
                return RedirectToAction("Login", "Users"); // Or show an error page
            }

            var order = db.OrderProes
                          .Include("Customer")
                          .Include("OrderDetails")
                          .FirstOrDefault(o => o.ID == id.Value);

            if (order == null)
            {
                return HttpNotFound();
            }

            return View(order);
        }

        [HttpPost]
        public ActionResult SelectPaymentMethod(int id, string paymentMethod)
        {
            // Find the order by ID
            var order = db.OrderProes.Find(id); // Use "db" instead of "dbContext"
            if (order != null && order.OrderDetails != null && order.OrderDetails.Any())
            {
                // Assuming the payment method is assigned to the first item in OrderDetails
                order.OrderDetails.First().PaymentMethod = paymentMethod; // Use "First()" instead of [0] for better safety

                // Save changes
                db.SaveChanges(); // Use "db" instead of "dbContext"
            }

            // Redirect to a confirmation page or appropriate action
            return RedirectToAction("PaymentConfirmation", new { id });
        }

        public ActionResult PaymentConfirmation(int id)
        {
            // You can look up the order based on the ID for display purposes
            var order = db.OrderProes
                          .Include("Customer")
                          .Include("OrderDetails")
                          .FirstOrDefault(o => o.ID == id);

            if (order == null)
            {
                return HttpNotFound();
            }

            return View(order); // You can create a "PaymentConfirmation" view to show the details.
        }


    }
}