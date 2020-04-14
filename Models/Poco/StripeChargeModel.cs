using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CustomComputersGU.Models
{
    /// <summary>
    /// This class is used with the strip payment service and 
    /// handles the stripe token and amount to be charged
    /// </summary>
    public class StripeChargeModel
    {
        [Key]
        public int StripeId { get; set; }

        [Required]
        public string Token { get; set; }

        [Required]
        public double Amount { get; set; }
        
    }
}