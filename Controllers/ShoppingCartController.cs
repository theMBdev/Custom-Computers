using CustomComputersGU.Models;
using CustomComputersGU.Models.Poco;
using CustomComputersGU.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace CustomComputersGU.Controllers
{
    public class ShoppingCartController : Controller
    {
        ApplicationDbContext storeDB = new ApplicationDbContext();
        //
        // GET: /ShoppingCart/
        public ActionResult Index()
        {
            var cart = ShoppingCart.GetCart(this.HttpContext);
            
            // Set up our ViewModel
            var viewModel = new ShoppingCartViewModel
            {
                CartItems = cart.GetCartItems(),
                CartTotal = cart.GetTotal()                
            };            
            // Return the view
            return View(viewModel);
        }

        // GET: /Store/AddToCart/5
        [NoDirectAccess]
        public ActionResult AddToCart(int id)
        {
            // Retrieve the product from the database
            var addedProduct = storeDB.Products
                .Single(product => product.ProductId == id);

            // Add it to the shopping cart
            if (addedProduct.UnitsInStock > 0)
            {
                var cart = ShoppingCart.GetCart(this.HttpContext);
                cart.AddToCart(addedProduct);
            }            

            // Go back to the main store page for more shopping
            return View("AddedToCart");
        }
        
        // AJAX: /ShoppingCart/RemoveFromCart/5
        [HttpPost]
        public ActionResult RemoveFromCart(int id)
        {
            // Remove the item from the cart
            var cart = ShoppingCart.GetCart(this.HttpContext);

            // Get the name of the product to display confirmation
            string productName = storeDB.Carts
                .Single(item => item.RecordId == id).Product.Name;

            // Remove from cart
            int itemCount = cart.RemoveFromCart(id);

            // Display the confirmation message
            var results = new ShoppingCartRemoveViewModel
            {
                Message = Server.HtmlEncode(productName) +
                    " has been removed from your shopping cart.",
                CartTotal = cart.GetTotal(),
                CartCount = cart.GetCount(),
                ItemCount = itemCount,
                DeleteId = id
            };
            return Json(results);
        }

        [HttpPost]
        [NoDirectAccess]
        public ActionResult UpdateCartCount(int id, int cartCount, FormCollection form)
        {
            ShoppingCartRemoveViewModel results = null;
            try
            {
                // Get the cart 
                var cart = ShoppingCart.GetCart(this.HttpContext);

                // Get the name of the product to display confirmation 
                string productName = storeDB.Carts
                    .Single(item => item.RecordId == id).Product.Name; 

                // Get the quantity in stock of the product 
                int productUnitsInStock = storeDB.Carts
                    .Single(item => item.RecordId == id).Product.UnitsInStock;

                // Update the cart count from user 
                int itemCount = cart.UpdateCartCount(id, cartCount);
                
                if(productUnitsInStock >= itemCount)
                {                    
                    //Prepare messages
                    string msg = "The quantity of " + Server.HtmlEncode(productName) +
                            " has been refreshed in your shopping cart.";
                    if (itemCount == 0) msg = Server.HtmlEncode(productName) +
                            " has been removed from your shopping cart.";
                    
                    // Display the confirmation message 
                    results = new ShoppingCartRemoveViewModel
                    {
                        Message = msg,
                        CartTotal = cart.GetTotal(),
                        CartCount = cart.GetCount(),
                        ItemCount = itemCount,
                        DeleteId = id
                    };
                }
                else
                {                    
                    //Prepare messages
                    string msg = "We do not have the amount of " + Server.HtmlEncode(productName) +
                            " you have selected please enter a lower value.";
                    
                    if (itemCount == 0) msg = Server.HtmlEncode(productName) +
                            " has been removed from your shopping cart.";
                    
                    
                    // Display the confirmation message 
                    results = new ShoppingCartRemoveViewModel
                    {
                        Message = msg,
                        CartTotal = cart.GetTotal(),
                        CartCount = cart.GetCount(),
                        ItemCount = itemCount,
                        DeleteId = id
                    };
                }                
            }
            catch
            {
                results = new ShoppingCartRemoveViewModel
                {
                    Message = "Error occurred or invalid input...",
                    CartTotal = -1,
                    CartCount = -1,
                    ItemCount = -1,
                    DeleteId = id
                };
            }            
            return Json(results);
        }
                        
        // GET: /ShoppingCart/CartSummary
        [ChildActionOnly]
        public ActionResult CartSummary()
        {
            var cart = ShoppingCart.GetCart(this.HttpContext);

            ViewData["CartCount"] = cart.GetCount();
            return PartialView("CartSummary");
        }
    }
}