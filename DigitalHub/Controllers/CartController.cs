using DigitalHub.Models;
using System;
using System.Collections.Generic;
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
            var cart = GetCart();
            return View(cart);
        }

        private Product GetProductById(int productId)
        {
            // Giả sử bạn có một dịch vụ hoặc repository để truy xuất dữ liệu sản phẩm
            var product = db.Products.FirstOrDefault(p => p.ProductID == productId);  // db.Products là ví dụ
            return product;
        }

        [HttpPost]
        public ActionResult AddToCart(int productId, int quantity)
        {
            var cart = GetCart();
            var product = db.Products.FirstOrDefault(p => p.ProductID == productId);

            if (product != null)
            {
                var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);

                if (existingItem == null)
                {
                    // Thêm sản phẩm mới vào giỏ hàng
                    cart.Items.Add(new CartItem
                    {
                        ProductId = product.ProductID,
                        ProductName = product.NamePro,
                        Price = product.DiscountPrice ?? 0m,
                        Quantity = quantity,
                        Image = product.ImagePro
                    });
                }
                else
                {
                    // Nếu sản phẩm đã có trong giỏ, chỉ cần tăng số lượng
                    existingItem.Quantity += quantity;
                }
            }
            cart.UpdateTotal();

            // Tính tổng tạm tính và tổng cộng
            decimal subtotalAmount = cart.Items.Sum(i => i.Price * i.Quantity);
            decimal totalAmount = subtotalAmount; // Giả sử không có phí vận chuyển

            return Json(new { success = true, subtotalAmount = subtotalAmount, totalAmount = totalAmount, cartCount = cart.Items.Sum(i => i.Quantity) });
        }



        [HttpPost]
        public ActionResult UpdateQuantity(int productId, int quantity)
        {
            var cart = GetCart();
            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);

            if (item != null && quantity > 0)
            {
                item.Quantity = quantity;
                cart.UpdateTotal(); // Cập nhật tổng giỏ hàng

                // Tính toán và trả về subtotal và totalAmount
                decimal subtotalAmount = cart.Items.Sum(i => i.Price * i.Quantity);
                return Json(new { success = true, subtotalAmount = subtotalAmount, totalAmount = cart.Total });
            }

            return Json(new { success = false });
        }


        [HttpPost]
        public ActionResult RemoveFromCart(int productId)
        {
            var cart = GetCart();
            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);

            if (item != null)
            {
                cart.Items.Remove(item); // Remove the item from the cart
                return Json(new { success = true });
            }

            return Json(new { success = false });
        }


        private Cart GetCart()
        {
            Cart cart = Session["Cart"] as Cart;
            if (cart == null)
            {
                cart = new Cart();
                Session["Cart"] = cart;
            }
            return cart;
        }
    }
}