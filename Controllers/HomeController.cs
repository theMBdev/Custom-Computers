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

    public class HomeController : Controller
    {        
        ApplicationDbContext storeDB = new ApplicationDbContext();



    public ActionResult Index()
        {
            // Get most popular products
            var products = GetTopSellingProducts(5);

            return View(products);
        }

        private ActionResult View(List<Product> products, List<Category> categories)
        {
            throw new NotImplementedException();
        }

        private List<Product> GetTopSellingProducts(int count)
        {
            /* Group the order details by product and return
               the products with the highest count */
            return storeDB.Products
                .OrderByDescending(a => a.OrderDetails.Count())
                .Take(count)
                .ToList();
        }



        // Creates the content of the return form which will be downloaded as a pdf file
        [NoDirectAccess]
        public ActionResult DownloadPdfReturnForm()
        {
            try
            {
                var model = new ReturnAddressForPdf();
                 
                // The text of the return form
                var content =
                "<h2>Return To<h2>" +
                "<p>FreePost RHHW-EKID-EWJS <br/> Custom Computers <br/> 10 Silicon Valley <br/> Glasgow <br/> Scotland <br/> G1 117 <br/> United Kingdom </p>";                

                model.PDFContent = content;                

                //Use ViewAsPdf Class to generate pdf using ReturnFormPdf.cshtml view
                return new Rotativa.ViewAsPdf("ReturnFormPdf", model) { FileName = "ReturnFormCustomComputers.pdf" };
            }
            catch (Exception ex)
            {

                throw;
            }
        }



    }
}