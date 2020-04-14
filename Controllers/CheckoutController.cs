using CustomComputersGU.Models;
using CustomComputersGU.Models.Poco;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CustomComputersGU.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        ApplicationDbContext storeDB = new ApplicationDbContext();

        // GET: /Checkout/AddressAndPayment
        [NoDirectAccess]
        public ActionResult AddressAndPayment()
        {
            return View();
        }
                
        // POST: /Checkout/AddressAndPayment
        [HttpPost]
        public ActionResult AddressAndPayment(FormCollection values)
        {
            var order = new Order();
            TryUpdateModel(order);

            try
            {                
                
                /*Assigns the users identity to email vairable if user is a customer 
                  and if the user is a worker it assigns there identity to serverd by vairable
                  worker manualy types in the customers email so that the order will display in the customers orders */
                if (User.IsInRole("Customer"))
                {
                    order.Email = User.Identity.Name;
                }
                else
                {
                    order.ServedBy = User.Identity.Name;
                }

                order.OrderDate = DateTime.Now;

                var cart = ShoppingCart.GetCart(this.HttpContext);
                decimal TotCart = cart.GetTotal();
                
                TempData["orderTotal"] = TotCart;
                TempData["orderData"] = order;
                return RedirectToAction("Charge");

                //return RedirectToAction("Complete", new { id = order.OrderId });
                
            }
            catch
            {
                //Invalid - redisplay with errors
                return View(order);
            }
        }

        //Process order
        [NoDirectAccess]
        public Order ProcessOrder(Order order)
        {

            //Save Order
            storeDB.Orders.Add(order);
            storeDB.SaveChanges();

            //Process the order 
            var cart = ShoppingCart.GetCart(this.HttpContext);
            cart.CreateOrder(order);            
            
            return (order);
        }
     
        [NoDirectAccess]
        public ActionResult Charge()
        {            
            return View(new StripeChargeModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Charge(StripeChargeModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
              
            var chargeId = await ProcessPayment(model);            

            //Get order using tempdata
            var order = new Order();
            if (TempData.ContainsKey("orderData"))
                order = (Order)TempData["orderData"];

            //Process the order
            ProcessOrder(order);

            //return View("Index");
            if(User.IsInRole("Customer"))
            {
                return View("Index");
            }
            else
            {
                return View("StaffCheckout");
            }
        }
        [NoDirectAccess]
        private /*static*/ async Task<string> ProcessPayment(StripeChargeModel model)
        {
            
            decimal orderTotal = 0;

            if (TempData.ContainsKey("orderTotal"))
                 orderTotal = (decimal)TempData["orderTotal"];
           
            return await Task.Run(() =>
            {
                var myCharge = new StripeChargeCreateOptions
                {
                    // convert the amount of orderTotal to pennies
                    Amount = (int)orderTotal * 100,
                    Currency = "gbp",
                    Description = "Custom Computers Charge",
                    SourceTokenOrExistingSourceId = model.Token
                };

                var chargeService = new StripeChargeService("sk_test_d3aJplQiuJAzSj4AQJwPGIk7");
                var stripeCharge = chargeService.Create(myCharge);

                return stripeCharge.Id;
                //return RedirectToAction("Charge", stripeCharge.Id);
            });
        }

    }
}