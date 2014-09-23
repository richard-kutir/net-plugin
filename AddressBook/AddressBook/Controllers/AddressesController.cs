using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using AddressBook.Models;
using RhoconnectNET;
using RhoconnectNET.Controllers;

namespace AddressBook.Controllers
{
    public class AddressesController : Controller, IRhoconnectCRUD
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        //// You must implement the next four methods
        //// These methods will be called by the Rhoconnect server
        //jsonresult rhoconnect_query_objects(string partition);
        //actionresult rhoconnect_create(string objjson, string partition);
        //actionresult rhoconnect_update(dictionary<string, object> changes, string partition);
        //actionresult rhoconnect_delete(object objid, string partition);

        // GET: Addresses
        public ActionResult Index()
        {
            return View(db.Addresses.ToList());
        }

        // GET: Addresses/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Address address = db.Addresses.Find(id);
            if (address == null)
            {
                return HttpNotFound();
            }
            return View(address);
        }

        // GET: Addresses/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Addresses/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Name,Address1,Email")] Address address)
        {
            if (ModelState.IsValid)
            {
                db.Addresses.Add(address);
                db.SaveChanges();
                // notifications to Rhoconnect server
                RhoconnectNET.Client.notify_on_create(partition(), address);
                return RedirectToAction("Index");
            }

            return View(address);
        }

        // GET: Addresses/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Address address = db.Addresses.Find(id);
            if (address == null)
            {
                return HttpNotFound();
            }
            return View(address);
        }

        // POST: Addresses/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Name,Address1,Email")] Address address)
        {
            if (ModelState.IsValid)
            {
                db.Entry(address).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(address);
        }

        // GET: Addresses/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Address address = db.Addresses.Find(id);
            if (address == null)
            {
                return HttpNotFound();
            }
            return View(address);
        }

        // POST: Addresses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Address address = db.Addresses.Find(id);
            db.Addresses.Remove(address);
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

        // RhoConnect Methods

        // This method is used to access current partition
        // in Rhoconnect notification callback
        private String partition()
        {
            // If you're using 'app' partition
            // uncomment the following line
            return "app";
        }

        public JsonResult rhoconnect_query_objects(String partition)
        {
            Console.WriteLine("Returning data");
            return Json(db.Addresses.ToDictionary(a => a.ID.ToString()));
        }

        public ActionResult rhoconnect_create(String objJson, String partition)
        {
            Address new_address = (Address)RhoconnectNET.Helpers.deserialize_json(objJson, typeof(Address));
            db.Addresses.Add(new_address);
            db.SaveChanges();
            return RhoconnectNET.Helpers.serialize_result(new_address.ID);
        }

        public ActionResult rhoconnect_update(Dictionary<string, object> changes, String partition)
        {
            int obj_id = Convert.ToInt32(changes["id"]);
            Address address_to_update = db.Addresses.First(a => a.ID == obj_id);
            // this method will update only the modified fields
            RhoconnectNET.Helpers.merge_changes(address_to_update, changes);
            db.Entry(address_to_update).State = EntityState.Modified;
            db.SaveChanges();
            return RhoconnectNET.Helpers.serialize_result(address_to_update.ID);
        }

        public ActionResult rhoconnect_delete(Object objId, String partition)
        {
            int key = Convert.ToInt32(objId);

            Address address = db.Addresses.Find(key);
            db.Addresses.Remove(address);
            db.SaveChanges();
            return RhoconnectNET.Helpers.serialize_result(key);
        }
    }
}
