using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Quest.Lib.DataModel;
using Quest.Common.Messages;

namespace Quest.Mobile.Controllers
{
    public class DeviceManagerController : Controller
    {
        private QuestEntities db = new QuestEntities();

        // GET: DeviceManager
        public ActionResult Index()
        {
            var devices = db.Devices.Include(d => d.DeviceRole).Include(d => d.Resource);
            return View(devices.ToList());
        }

        // GET: DeviceManager/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var device = db.Devices.Find(id);
            if (device == null)
            {
                return HttpNotFound();
            }
            return View(device);
        }

        // GET: DeviceManager/Create
        public ActionResult Create()
        {
            ViewBag.DeviceRoleID = new SelectList(db.DeviceRoles, "DeviceRoleID", "DeviceRoleName");
            ViewBag.ResourceID = new SelectList(db.ResourceCallsignViews, "ResourceID", "CallsignFleet", null);
            return View();
        }

        // POST: DeviceManager/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "DeviceID,OwnerID,ResourceID,AuthToken,DeviceIdentity,NotificationTypeID,NotificationID,LastUpdate,LastStatusUpdate,Geometry,PositionAccuracy,isEnabled,SendNearby,NearbyDistance,LoggedOnTime,LoggedOffTime,DeviceRoleID")] Device device)
        {
            if (ModelState.IsValid)
            {
                db.Devices.Add(device);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.DeviceRoleID = new SelectList(db.DeviceRoles, "DeviceRoleID", "DeviceRoleName", device.DeviceRoleID);
            ViewBag.ResourceID = new SelectList(db.ResourceCallsignViews, "ResourceID", "CallsignFleet", device.ResourceID);
            return View(device);
        }

        // GET: DeviceManager/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var device = db.Devices.Find(id);
            if (device == null)
            {
                return HttpNotFound();
            }
            ViewBag.DeviceRoleID = new SelectList(db.DeviceRoles, "DeviceRoleID", "DeviceRoleName", device.DeviceRoleID);
            ViewBag.ResourceID = new SelectList(db.ResourceCallsignViews, "ResourceID", "CallsignFleet", device.ResourceID);
            return View(device);
        }

        // POST: DeviceManager/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "DeviceID,OwnerID,ResourceID,AuthToken,DeviceIdentity,NotificationTypeID,NotificationID,LastUpdate,LastStatusUpdate,Geometry,PositionAccuracy,isEnabled,SendNearby,NearbyDistance,LoggedOnTime,LoggedOffTime,DeviceRoleID,DeviceMake,DeviceModel,DeviceProduct")] Device device)
        {
            if (ModelState.IsValid)
            {
                db.Entry(device).State = EntityState.Modified;
                db.SaveChanges();
                
                var request = new RefreshStateRequest()
                {
                    AuthToken = device.AuthToken,
                    Dummy = "",
                    RequestId = Guid.NewGuid().ToString(),
                    Timestamp = 0
                };

                // send a refresh request
                MvcApplication.MsgClientCache.BroadcastMessage(request);

                return RedirectToAction("Index");
            }
            ViewBag.DeviceRoleID = new SelectList(db.DeviceRoles, "DeviceRoleID", "DeviceRoleName", device.DeviceRoleID);
            ViewBag.ResourceID = new SelectList(db.ResourceCallsignViews, "ResourceID", "CallsignFleet", device.ResourceID);

            return View(device);
        }

        // GET: DeviceManager/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var device = db.Devices.Find(id);
            if (device == null)
            {
                return HttpNotFound();
            }
            return View(device);
        }

        // POST: DeviceManager/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var device = db.Devices.Find(id);
            db.Devices.Remove(device);
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
