//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DigitalHub.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class ShoppingCart
    {
        public int ID { get; set; }
        public string SessionID { get; set; }
        public Nullable<int> CustomerID { get; set; }
        public int ProductID { get; set; }
        public int Quantity { get; set; }
        public System.DateTime DateAdded { get; set; }
    
        public virtual Customer Customer { get; set; }
        public virtual Product Product { get; set; }
    }
}
