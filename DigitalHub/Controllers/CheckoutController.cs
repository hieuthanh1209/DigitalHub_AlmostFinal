using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
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
            var cart = Session["Cart"] as Cart;
            if (cart == null || !cart.Items.Any())
            {
                // Redirect to cart if it's empty
                return RedirectToAction("Index", "Cart");
            }

            return View(cart); // Pass the cart to the view for displaying
        }

        // POST: Checkout (for placing the order)
        [HttpPost]
        public ActionResult PlaceOrder(string address)
        {
            var cart = Session["Cart"] as Cart;
            if (cart == null || !cart.Items.Any())
            {
                // Redirect to cart if empty
                return RedirectToAction("Index", "Cart");
            }

            // Fetch actual customer ID from user session
            var customer = Session["TaiKhoan"] as Customer;
            if (customer == null)
            {
                // Handle the case where customer is not found in session
                return RedirectToAction("Login", "Users"); // Or show an error page
            }

            // Create a new order
            var order = new OrderPro
            {
                IDCus = customer.IDCus, // Fetch actual customer ID from user session, set to null if not found
                DateOrder = DateTime.Now,
                AddressDeliverry = address
            };
            db.OrderProes.Add(order);
            db.SaveChanges();

            // Add order details
            foreach (var item in cart.Items)
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

            // Clear the cart after order is placed
            Session["Cart"] = null;

            // Redirect after placing the order
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