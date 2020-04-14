using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CustomComputersGU.Models.Poco
{
    /// <summary>
    /// This class is used with ShoppingCart
    /// Relationships:
    /// Many to 1 relationship with Product
    /// </summary>
    public class Cart
    {
        [Key]
        public int RecordId { get; set; }
        public string CartId { get; set; }
        public int ProductId { get; set; }

        [Required(AllowEmptyStrings = true, ErrorMessage = " ")]
        [Range(0, 10, ErrorMessage = "Quantity must be between 0 and 10")]
        [DisplayName("Quantity")]
        public int Count { get; set; }
        public DateTime DateCreated { get; set; }

        public virtual Product Product { get; set; }
    }
}