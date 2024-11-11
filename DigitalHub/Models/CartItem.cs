using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DigitalHub.Models
{
    public class CartItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string Image { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice => Price * Quantity;
        public string Category { get; set; }
    }
}