using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CustomComputersGU.Models.Poco
{
    /// <summary>
    /// This class is used to handles a product when it is been put through 
    /// the process of been ordered    
    /// Relationships:
    /// Many to 1 relationship with Product
    /// Many to 1 relationship with Order    
    /// </summary>
    public class OrderDetail
    {
        public int OrderDetailId { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        // Quantity ordered          
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        // a check box that can be checked to show an item is getting returned
        [DisplayName("Return Product")]
        public bool Returning { get; set; }               
        [DisplayName("Reason For Returning")]
        [StringLength(40)]
        public string ReasonForReturning { get; set; }
        [DisplayName("Amount Returning")]
        public int AmountReturning { get; set; }

        public virtual Product Product { get; set; }
        public virtual Order Order { get; set; }
    }
}