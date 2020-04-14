using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CustomComputersGU.Models.Poco
{
    /// <summary>
    /// This class is the blueprint for a category    
    /// Relationships:
    /// 1 to Many relationship with Product
    /// </summary>
    public partial class Category
    {
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "An Category Name is required")]
        [StringLength(160)]
        public string Name { get; set; }

        [Required(ErrorMessage = "An Category Decription is required")]
        [StringLength(1000000)]        
        public string Description { get; set; }

        [DisplayName("Product Art URL")]
        [StringLength(1024)]
        public string ArtUrl { get; set; }

        public List<Product> Products { get; set; }        
    }
}