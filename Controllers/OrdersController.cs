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
using CustomComputersGU.ViewModels;
using Rotativa;

namespace CustomComputersGU.Controllers
{
    [Authorize(Roles ="SalesAssistant, StoreManager, Administrator, AssistantManager")]
    public class OrdersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
          
        // GET: Orders
        public ActionResult Index()
        {  
            return View(db.Orders.ToList());
        }          

        // GET: Orders/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            OrderDetail orderDetail = db.OrderDetails.Find(id);
            if (orderDetail == null)
            {
                return HttpNotFound();
            }
            return View(orderDetail);
        }

        // GET: Orders/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Orders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Order order)
        {
            if (ModelState.IsValid)
            {
                db.Orders.Add(order);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(order);
        }

        // GET: Orders/MarkOrderAsShipped/
        public ActionResult MarkOrderAsShipped(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            /* Auto marks check box as checked as the reason for clicking the marked as shipped
               button will most times be to check the check box so this means the user just has to 
               click save*/
            order.Shipped = true;
                        
            return View(order);
        }

        // POST: Orders/MarkOrderAsShipped/
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MarkOrderAsShipped([Bind(Include = "OrderId,OrderDate,Shipped,FirstName,LastName,Address,City,PostalCode,Country,Phone,Email,Total")] Order order)
        {
            if (ModelState.IsValid)
            {
                // So when a staff member marks the order as shipped the other attributes are not changed to default values
                db.Entry(order).State = EntityState.Unchanged;
                //Only change one value
                db.Entry(order).Property("Shipped").IsModified = true;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(order);
        }

        // GET: Orders/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        
        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Order order = db.Orders.Find(id);
            db.Orders.Remove(order);
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
