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
    public class TravelsController : TravelApiControllerBase
    {
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

        // DELETE api/Travels/5
        public HttpResponseMessage DeleteTravel(int id, string key)
        {
            return Monitor(key, () =>
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

                    return Request.CreateResponse(HttpStatusCode.NoContent, travel);
                });
        }
    }
}