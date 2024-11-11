using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DigitalHub.Models
{
    public class Cart
    {
        public List<CartItem> Items { get; set; } = new List<CartItem>();
        public decimal Total { get; private set; }
        public void UpdateTotal()
        {
            Total = Items.Sum(i => i.Price * i.Quantity);
        }
    }
}