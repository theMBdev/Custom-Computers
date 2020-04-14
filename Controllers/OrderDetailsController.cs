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
using Microsoft.AspNet.Identity;

namespace CustomComputersGU.Controllers
{
   
    public class OrderDetailsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: OrderDetails
        [Authorize(Roles = "SalesAssistant, StoreManager, Administrator, AssistantManager")]
        public ActionResult Index(int? id)
        {
         /* Get the total amount of items ordered
            This is passed over to the view so it knows how many tables to create */
            var totalOrder = db.OrderDetails.Max(u =>(int?) u.OrderId) ?? 0;            
            ViewBag.TotalOrders = totalOrder + 1;

            string myEmail = User.Identity.GetUserName();
            ViewBag.MyEmail = myEmail;

            var orderDetails = db.OrderDetails.Include(o => o.Order).Include(o => o.Product);
            return View(orderDetails.ToList());
        }

        // GET: OrderDetails/Details/5
        [Authorize(Roles = "SalesAssistant, StoreManager, Administrator, AssistantManager")]
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

        // Returns all orders that belong to the current user
        public ActionResult MyOrders()
        {
            string myEmail = User.Identity.GetUserName();
            //ViewBag.MyEmail = myEmail;

            var myorders = db.OrderDetails.Include(o => o.Order).Include(o => o.Product).Where(u => u.Order.Email == myEmail);
            return View(myorders.ToList());
        }

        // Returns all orders that belong to the current worker
        [Authorize(Roles = "SalesAssistant, StoreManager, Administrator, AssistantManager")]
        public ActionResult MySales()
        {
            string myEmail = User.Identity.GetUserName();
            //ViewBag.MyEmail = myEmail;

            var mysales = db.OrderDetails.Include(o => o.Order).Include(o => o.Product).Where(u => u.Order.ServedBy == myEmail);
            return View(mysales.ToList());
        }


        // GET: OrderDetails/Create
        [Authorize(Roles = "StoreManager, Administrator, AssistantManager")]
        public ActionResult Create()
        {                                                           
            ViewBag.OrderId = new SelectList(db.Orders, "OrderId", "OrderId");
            ViewBag.ProductId = new SelectList(db.Products, "ProductId", "Name");
            return View();
        }

        // POST: OrderDetails/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "OrderDetailId,OrderId,ProductId,Quantity,UnitPrice")] OrderDetail orderDetail)
        {
            if (ModelState.IsValid)
            {
                db.OrderDetails.Add(orderDetail);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.OrderId = new SelectList(db.Orders, "OrderId", "OrderId", orderDetail.OrderId);
            ViewBag.ProductId = new SelectList(db.Products, "ProductId", "Name", orderDetail.ProductId);
            return View(orderDetail);
        }


        // GET: OrderDetails/Edit/5    
        [Authorize(Roles = "StoreManager, Administrator, AssistantManager")]
        public ActionResult Edit(int? id)
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
            ViewBag.OrderId = new SelectList(db.Orders, "OrderId", "OrderId", orderDetail.OrderId);
            ViewBag.ProductId = new SelectList(db.Products, "ProductId", "Name", orderDetail.ProductId);
            return View(orderDetail);
        }

        // POST: OrderDetails/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "OrderDetailId,OrderId,ProductId,Quantity,UnitPrice,Returning")] OrderDetail orderDetail)
        {
            if (ModelState.IsValid)
            {
                db.Entry(orderDetail).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.OrderId = new SelectList(db.Orders, "OrderId", "OrderId", orderDetail.OrderId);
            ViewBag.ProductId = new SelectList(db.Products, "ProductId", "Name", orderDetail.ProductId);
            return View(orderDetail);
        }


        // GET: OrderDetails/MarkItemReturning/
        public ActionResult MarkItemReturning(int? id)
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
            ViewBag.OrderId = new SelectList(db.Orders, "OrderId", "OrderId", orderDetail.OrderId);
            ViewBag.ProductId = new SelectList(db.Products, "ProductId", "Name", orderDetail.ProductId);

            /* Auto marks check box as checked as the reason for clicking the returning
               button will most times be to say they are returning a product but they can uncheck if it was a mistake*/
            orderDetail.Returning = true;

            return View(orderDetail); 
        }

        // POST: OrderDetails/MarkItemReturning/
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MarkItemReturning([Bind(Include = "OrderDetailId,OrderId,ProductId,Quantity,UnitPrice,Returning,ReasonForReturning,AmountReturning")] OrderDetail orderDetail)
        {
            if (ModelState.IsValid)
            {
                db.Entry(orderDetail).State = EntityState.Unchanged;
                db.Entry(orderDetail).Property("Returning").IsModified = true;
                db.Entry(orderDetail).Property("ReasonForReturning").IsModified = true;
                db.Entry(orderDetail).Property("AmountReturning").IsModified = true;
                db.SaveChanges();
                if(User.IsInRole("Customer"))
                {
                    return RedirectToAction("MyOrders", "OrderDetails");
                }
                else
                {
                    return RedirectToAction("Index", "OrderDetails");
                }
                
            }
            ViewBag.OrderId = new SelectList(db.Orders, "OrderId", "OrderId", orderDetail.OrderId);
            ViewBag.ProductId = new SelectList(db.Products, "ProductId", "Name", orderDetail.ProductId);
            return View(orderDetail);
        }

        // GET: OrderDetails/Delete/5
        [Authorize(Roles = "SalesAssistant, StoreManager, Administrator, AssistantManager")]
        public ActionResult Delete(int? id)
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

        // POST: OrderDetails/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            OrderDetail orderDetail = db.OrderDetails.Find(id);
            db.OrderDetails.Remove(orderDetail);
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
