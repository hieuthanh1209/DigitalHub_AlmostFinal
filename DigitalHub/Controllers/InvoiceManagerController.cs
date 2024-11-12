using System.Linq;
using System.Web.Mvc;
using DigitalHub.Models; // Adjust namespace based on your project

namespace DigitalHub.Controllers
{
    public class InvoiceManagerController : Controller
    {
        private DigitalHub_DBEntities db = new DigitalHub_DBEntities(); // Your DbContext class

        // GET: InvoiceManager/Orders
        public ActionResult Orders()
        {
            var orders = db.OrderProes.Include("Customer").Include("OrderDetails").ToList(); // Adjust based on your relationships
            return View(orders);
        }

        // GET: InvoiceManager/OrderDetails/5
        public ActionResult OrderDetails(int id)
        {
            var order = db.OrderProes.Include("OrderDetails").FirstOrDefault(o => o.ID == id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }
        // POST: InvoiceManager/DeleteOrder/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteOrder(int id)
        {
            // Find the order with the provided id
            var order = db.OrderProes.Include("OrderDetails").FirstOrDefault(o => o.ID == id);

            if (order != null)
            {
                // Delete related OrderDetail records first
                foreach (var orderDetail in order.OrderDetails.ToList())
                {
                    db.OrderDetails.Remove(orderDetail);
                }

                // After deleting the order details, remove the order itself
                db.OrderProes.Remove(order);
                db.SaveChanges();  // Commit the changes to the database

                TempData["SuccessMessage"] = "Order and its details deleted successfully.";
            }
            else
            {
                return HttpNotFound(); // If order not found
            }

            return RedirectToAction("Orders"); // Redirect to the Orders list
        }

        // POST: InvoiceManager/ConfirmPayment/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmPayment(int id)
        {
            // Find the order with the provided id
            var order = db.OrderProes.FirstOrDefault(o => o.ID == id);

            if (order != null)
            {
                // Mark the order as paid
                order.IsPaid = true;
                db.SaveChanges();  // Commit the changes to the database

                TempData["SuccessMessage"] = "Order has been confirmed as paid successfully.";
            }
            else
            {
                return HttpNotFound(); // If order not found
            }

            return RedirectToAction("OrderDetails", new { id = id }); // Redirect to the Order Details page
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UndoPayment(int id)
        {
            // Sử dụng db thay vì _context
            var order = db.OrderProes.FirstOrDefault(o => o.ID == id);
            if (order != null)
            {
                order.IsPaid = false; // Đánh dấu đơn hàng là chưa thanh toán
                db.SaveChanges(); // Lưu lại trạng thái "Chưa thanh toán"

                TempData["SuccessMessage"] = "Order payment has been undone successfully.";
            }
            else
            {
                return HttpNotFound(); // Nếu không tìm thấy đơn hàng
            }

            return RedirectToAction("OrderDetails", new { id = id }); // Quay lại trang chi tiết đơn hàng
        }
    }
}
