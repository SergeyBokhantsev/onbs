using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using WebApp.Models;

namespace WebApp.Controllers
{
    public class TravelsController : ONBSApiControllerBase
    {
        // GET api/Travels/active
        [HttpGet]
        [ActionName("active")]
        public HttpResponseMessage GetActiveTravel(string key, string vehicle)
        {
            return Monitor(key, () =>
            {
                if (string.IsNullOrEmpty(vehicle))
                    throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.BadRequest));

                var travel = db.Travels.Where(t => t.Vehicle == vehicle && !t.Closed).OrderByDescending(t => t.EndTime).FirstOrDefault();

                if (travel == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }
                else
                {
                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
                    response.Content = new StringContent(JsonConvert.SerializeObject(travel));
                    return response;
                }
            });
        }

        // GET api/Travels/5
        [HttpGet]
        public HttpResponseMessage GetTravel(int id)
        {
            return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Key not provided");
        }

        // GET api/Travels/5
        [HttpGet]
        [ActionName("get")]
        public HttpResponseMessage GetTravel(int id, string key)
        {
            return Monitor(key, () =>
            {
                var travel = db.GetTravel(id);
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(JsonConvert.SerializeObject(travel));
                return response;
            });
        }

        // POST api/Travels/open
        [HttpPost]
        [ActionName("open")]
        public HttpResponseMessage OpenTravel(string key, string vehicle, string name)
        {
            return Monitor(key, () =>
                {
                    if (string.IsNullOrEmpty(vehicle) || string.IsNullOrEmpty(name))
                        throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.BadRequest));

                    var travel = new Travel(name, vehicle);

                    db.Travels.Add(travel);
                    db.SaveChanges();

                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created);
                    response.Content = new StringContent(JsonConvert.SerializeObject(travel));
                    return response;
                });
        }

        // POST api/Travels/close/5
        [HttpPost]
        [ActionName("close")]
        public HttpResponseMessage CloseTravel(int id, string key)
        {
            return Monitor(key, () =>
            {
                db.GetTravel(id).Closed = true;

                try
                {
                    db.SaveChanges();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
                }

                return Request.CreateResponse(HttpStatusCode.OK);
            });
        }

        // POST api/Travels/close/5
        [HttpPost]
        [ActionName("rename")]
        public HttpResponseMessage CloseTravel(int id, string key, string name)
        {
            return Monitor(key, () =>
            {
                db.GetTravel(id).Name = name;

                try
                {
                    db.SaveChanges();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
                }

                return Request.CreateResponse(HttpStatusCode.OK);
            });
        }

        // DELETE api/Travels/5
        [HttpDelete]
        [ActionName("delete")]
        public HttpResponseMessage DeleteTravel(int id, string key)
        {
            return Monitor(key, () =>
                {
                    Travel travel = db.GetTravel(id);

                    var track = travel.Track.ToArray();

                    foreach (var point in track)
                        db.TravelPoints.Remove(point);

                    db.Travels.Remove(travel);

                    try
                    {
                        db.SaveChanges();
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
                    }

                    return Request.CreateResponse(HttpStatusCode.NoContent, travel);
                });
        }
    }
}