using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using CustomComputersGU.Models;
using CustomComputersGU.Models.Poco;
using System.Collections;
using Rotativa;

namespace CustomComputersGU.Controllers
{
    
    public class StoreManagerController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        [Authorize(Roles = "Administrator,AssistantManager,StoreManager, StoresManager")]
        public ActionResult LowProductsToPdf()
        {
            var model = db.Products.Where(unit => unit.UnitsInStock <= 5).ToList();

            return new Rotativa.ViewAsPdf("LowProductsToPdf", model) { FileName = "LowProducts.pdf" };

            //return new ViewAsPdf(model);
        }
          

        // GET: StoreManager
        [Authorize(Roles = "Administrator,AssistantManager,StoreManager, StoresManager")]
        public ActionResult Index()
        {
            var products = db.Products.Include(p => p.Category);
            
            return View(products.ToList());
        }

        // GET: StoreManager
        // If stock of an item is 5 or lower it will be listed on this page
        [Authorize(Roles = "Administrator,AssistantManager,StoreManager, StoresManager")]
        public ActionResult LowStock()
        {
            var products = db.Products.Where(unit => unit.UnitsInStock <= 5).ToList();
         
            return View(products.ToList());
        }

        [Authorize(Roles = "Administrator,AssistantManager,StoreManager, StoresManager")]
        public ActionResult ReturningProducts()
        {           
            var returningProducts = db.OrderDetails.Where(unit => unit.Returning == true).ToList();

            return View(returningProducts.ToList());
        }


        [Authorize(Roles = "Administrator,AssistantManager,StoreManager, StoresManager, SalesAssistant")]        
        public ActionResult StorePanel()
        {
            return View();            
        }

        // GET: StoreManager/Details/5
        [NoDirectAccess]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        

        // GET: StoreManager/Create
        [Authorize(Roles = "Administrator,AssistantManager,StoreManager, StoresManager")]
        public ActionResult Create()
        {
            ViewBag.CategoryId = new SelectList(db.Categories, "CategoryId", "Name");
            return View();
        }

        // POST: StoreManager/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ProductId,Name,Price,ArtUrl,UnitsInStock,Description,CategoryId")] Product product)
        {
            if (ModelState.IsValid)
            {                
                db.Products.Add(product);
                if (product.ArtUrl == null)
                {
                    product.ArtUrl = "/Content/Images/LogoPlaceHolder.png";
                }
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CategoryId = new SelectList(db.Categories, "CategoryId", "Name", product.CategoryId);
            return View(product);
        }

        // GET: StoreManager/Edit/5
        [Authorize(Roles = "Administrator,AssistantManager,StoreManager, StoresManager")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            ViewBag.CategoryId = new SelectList(db.Categories, "CategoryId", "Name", product.CategoryId);
            return View(product);
        }

        // POST: StoreManager/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ProductId,Name,Price,ArtUrl,UnitsInStock,Description,CategoryId")] Product product)
        {
            if (ModelState.IsValid)
            {
                db.Entry(product).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CategoryId = new SelectList(db.Categories, "CategoryId", "Name", product.CategoryId);
            return View(product);
        }
        
        // GET: StoreManager/Delete/5
        [Authorize(Roles = "Administrator,AssistantManager,StoreManager, StoresManager")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // POST: StoreManager/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Product product = db.Products.Find(id);
            db.Products.Remove(product);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
