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
    public class TravelsController : ApiController
    {
        private ONBSContext db = new ONBSContext();

        private void AssertKey(string key)
        {
            var user = db.Users.Where(u => u.Key == key).FirstOrDefault();

            if (user == null)
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.Forbidden));
        }

        // GET api/Travels
        public IEnumerable<Travel> GetTravels()
        {
            return db.Travels.AsEnumerable();
        }

        // GET api/Travels/5
        public Travel GetTravel(int id, string key)
        {
            AssertKey(key);

            Travel travel = db.Travels.Find(id);
            if (travel == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            return travel;
        }

        // PUT api/Travels/5
        public HttpResponseMessage PutTravel(int id, Travel travel)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            if (id != travel.ID)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            db.Entry(travel).State = EntityState.Modified;

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

        // POST api/Travels
        [HttpPost]
        [ActionName("open")]
        public HttpResponseMessage PostTravel(string name)
        {
            Travel travel = new Travel { StartTime = DateTime.Now, Name = name };

            if (ModelState.IsValid)
            {
                db.Travels.Add(travel);
                db.SaveChanges();

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created);
                response.Content = new StringContent(travel.ID.ToString());
                response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = travel.ID }));
                return response;
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }
        }

        // DELETE api/Travels/5
        public HttpResponseMessage DeleteTravel(int id)
        {
            Travel travel = db.Travels.Find(id);
            if (travel == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            db.Travels.Remove(travel);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }

            return Request.CreateResponse(HttpStatusCode.OK, travel);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}