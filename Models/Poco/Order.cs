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
    /// This class sets up the fields for an order and stores the entered  
    /// details of where the product has to be shipped to
    /// Relationships:
    /// 1 to Many relationship with OrderDetail
    /// </summary>    
    public partial class Order
    {
        [ScaffoldColumn(false)]
        public int OrderId { get; set; }

        [ScaffoldColumn(false)]
        public System.DateTime OrderDate { get; set; }

        // a check box that can be checked to show an item is shipped
        [ScaffoldColumn(false)]
        public bool Shipped { get; set; }
                        
        [Required(ErrorMessage = "Please Enter First Name")]
        [StringLength(30)]
        [DisplayName("First Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Please Enter Last Name")]
        [DisplayName("Last Name")]
        [StringLength(30)]
        public string LastName { get; set; }

        //Longest street name has 37 characters (Bolderwood Arboretum Ornamental Drive) 
        [Required(ErrorMessage = "Please Enter Address")]
        [StringLength(40)]
        public string Address { get; set; }
        
        [Required(ErrorMessage = "Please Enter Name of City")]
        [StringLength(40)]
        public string City { get; set; }

        //Wikapedia list of postcodes longest i seen was 14 characters 
        [Required(ErrorMessage = "Please Enter PostCode")]
        [StringLength(16)]
        public string PostalCode { get; set; }

        /*Longest country name according to the internet 50 characters 
         (United Kingdom of Great Britan and Northen Ireland)*/
        [Required(ErrorMessage = "Please Enter a Country Name")]
        [StringLength(50)]
        public string Country { get; set; }
        
        [Required(ErrorMessage = "Please Enter a Phone Number")]
        [StringLength(15)]
        public string Phone { get; set; }

        // the staff member proccessing the orders login email
        [ScaffoldColumn(false)]
        public string ServedBy { get; set; }

        // customers login email        
        [ScaffoldColumn(false)]
        [DisplayName("Customer Email")]
        public string Email { get; set; }
        
        //Total cost of the order
        [ScaffoldColumn(false)]        
        public decimal Total { get; set; }

        public List<OrderDetail> OrderDetails { get; set; }
    }
}

