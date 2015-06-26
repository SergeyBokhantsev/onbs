using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using WebApp.Models;

namespace WebApp.Controllers
{
    public class TravelPointsController : ApiController
    {
        private ONBSContext db = new ONBSContext();

        // GET api/TravelPoints
        public IEnumerable<TravelPoint> GetTravelPoints()
        {
            return db.TravelPoints.AsEnumerable();
        }

        // GET api/TravelPoints/5
        public TravelPoint GetTravelPoint(int id)
        {
            TravelPoint travelpoint = db.TravelPoints.Find(id);
            if (travelpoint == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            return travelpoint;
        }

        // PUT api/TravelPoints/5
        public HttpResponseMessage PutTravelPoint(int id, TravelPoint travelpoint)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            if (id != travelpoint.ID)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            db.Entry(travelpoint).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        // POST api/TravelPoints
        [HttpPost]
        [ActionName("add")]
        public HttpResponseMessage PostTravelPoint(int travel_id, double lat, double lon)
        {
            var travel = db.Travels.Where(t => t.ID == travel_id).FirstOrDefault();

            var travelpoint = new TravelPoint { Lat = lat, Lon = lon };

            travel.Track.Add(travelpoint);

            if (ModelState.IsValid)
            {
                db.SaveChanges();

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, travelpoint);
                response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = travelpoint.ID }));
                return response;
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }
        }

        // DELETE api/TravelPoints/5
        public HttpResponseMessage DeleteTravelPoint(int id)
        {
            TravelPoint travelpoint = db.TravelPoints.Find(id);
            if (travelpoint == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            db.TravelPoints.Remove(travelpoint);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }

            return Request.CreateResponse(HttpStatusCode.OK, travelpoint);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}