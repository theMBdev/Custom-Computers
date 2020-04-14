using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CustomComputersGU.Models.Poco
{
    /// <summary>
    /// This class is the blueprint for a product    
    /// Relationships:
    /// Many to 1 relationship with Category
    /// 1 to many relationship with OrderDetail  
    /// </summary>
    public class Product
    {
        public int ProductId { get; set; }

        [Required(ErrorMessage = "An Product Name is required")]
        [StringLength(100)]
        [DisplayName("Product Name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, 100000.00,
            ErrorMessage = "Price must be between 0.01 and 100,000.00")]
        public decimal Price { get; set; }

        [DisplayName("Product Art URL")]
        [StringLength(1024)]
        public string ArtUrl { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        [DisplayName("Units in Stock")]
        [Range(0, 1000,
            ErrorMessage = "Stock must be between 0 and 1000")]
        public int UnitsInStock { get; set; }

        [DisplayName("Category")]
        public int CategoryId { get; set; }
                
        // Navigational Properties
        public virtual Category Category { get; set; }
        public virtual List<OrderDetail> OrderDetails { get; set; }
    }
}