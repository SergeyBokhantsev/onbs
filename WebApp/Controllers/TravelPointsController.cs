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
    public class TravelPointsController : TravelApiControllerBase
    {
        [HttpPost]
        [ActionName("add")]
        public HttpResponseMessage AddTravelPoint(string key, int travel_id, double lat, double lon, double speed, int type, string description)
        {
            return Monitor(key, () =>
                {
                    var travel = db.GetTravel(travel_id);

                    var point = new TravelPoint { Description = description, Lat = lat, Lon = lon, Speed = speed, Time = DateTime.Now, Type = (TravelPointTypes)type };

                    travel.Track.Add(point);
                    travel.EndTime = point.Time;

                    db.SaveChanges();

                    return Request.CreateResponse(HttpStatusCode.OK);
                });
        }

        [HttpPost]
        [ActionName("addmany")]
        public HttpResponseMessage AddTravelPoint(string key, int travel_id)
        {
            return Monitor(key, () =>
            {
                var travel = db.GetTravel(travel_id);

                DateTime maxTime = new DateTime();

                var readTask = Request.Content.ReadAsStringAsync();
                if (readTask.Wait(5 * 60 * 1000))
                {
                    try
                    {
                        foreach (var rawPoint in readTask.Result.Split('|'))
                        {
                            var tp = new TravelPoint();

                            foreach (var rawItem in rawPoint.Split(';'))
                            {
                                var subItems = rawItem.Split('=');
                                var name = subItems[0];
                                var value = subItems[1];

                                switch (name)
                                {
                                    case "lat":
                                        tp.Lat = double.Parse(value);
                                        break;

                                    case "lon":
                                        tp.Lon = double.Parse(value);
                                        break;

                                    case "speed":
                                        tp.Speed = double.Parse(value);
                                        break;

                                    case "description":
                                        tp.Description = value;
                                        break;

                                    case "type":
                                        tp.Type = (TravelPointTypes)int.Parse(value);
                                        break;

                                    case "time":
                                        tp.Time = DateTime.Parse(value);
                                        if (tp.Time > maxTime)
                                            maxTime = tp.Time;
                                        break;
                                }
                            }

                            travel.Track.Add(tp);
                        }
                    }
                    catch (Exception ex)
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, string.Concat("Unable to parse body: ", ex.Message));
                    }

                    if (travel.EndTime < maxTime)
                        travel.EndTime = maxTime;

                    db.SaveChanges();

                    return Request.CreateResponse(HttpStatusCode.OK);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.RequestTimeout, "Timeout reading request");
                }
            });
        }

        //// GET api/TravelPoints
        //public IEnumerable<TravelPoint> GetTravelPoints()
        //{
        //    return db.TravelPoints.AsEnumerable();
        //}

        //// GET api/TravelPoints/5
        //public TravelPoint GetTravelPoint(int id)
        //{
        //    TravelPoint travelpoint = db.TravelPoints.Find(id);
        //    if (travelpoint == null)
        //    {
        //        throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
        //    }

        //    return travelpoint;
        //}

        //// PUT api/TravelPoints/5
        //public HttpResponseMessage PutTravelPoint(int id, [FromBody] IEnumerable<TravelPoint> travelpoints)
        //{
        //    TravelPoint travelpoint = null;

        //    if (!ModelState.IsValid)
        //    {
        //        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
        //    }

        //    if (id != travelpoint.ID)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.BadRequest);
        //    }

        //    db.Entry(travelpoint).State = EntityState.Modified;

        //    try
        //    {
        //        db.SaveChanges();
        //    }
        //    catch (DbUpdateConcurrencyException ex)
        //    {
        //        return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
        //    }

        //    return Request.CreateResponse(HttpStatusCode.OK);
        //}

        //public HttpResponseMessage PostTravelPoint(string key, int travelId, [FromBody] IEnumerable<TravelPoint> points)
        //{
        //    return Monitor(key, () =>
        //    {
        //        var travel = db.GetTravel(travelId);

        //        foreach (var p in points)
        //            travel.Track.Add(p);

        //        db.SaveChanges();

        //        return Request.CreateResponse(HttpStatusCode.Created);
        //    });
        //}

        //async Task<HttpResponseMessage> Foo()
        //{
        //    var body = await Request.Content.ReadAsStringAsync();
        //    return Request.CreateResponse(HttpStatusCode.Created);
        //}

        //// POST api/TravelPoints
        //[HttpPost]
        //[ActionName("add")]
        //public HttpResponseMessage PostTravelPoint(int travel_id, double lat, double lon, double speed, string description)
        //{
        //    var travel = db.Travels.Where(t => t.ID == travel_id).FirstOrDefault();

        //    var travelpoint = new TravelPoint { Lat = lat, Lon = lon, Speed = speed, Description = description, Time = DateTime.Now };

        //    travel.EndTime = DateTime.Now;
        //    travel.Track.Add(travelpoint);

        //    if (ModelState.IsValid)
        //    {
        //        db.SaveChanges();

        //        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, travelpoint);
        //        response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = travelpoint.ID }));
        //        return response;
        //    }
        //    else
        //    {
        //        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
        //    }
        //}

        //// DELETE api/TravelPoints/5
        //public HttpResponseMessage DeleteTravelPoint(int id)
        //{
        //    TravelPoint travelpoint = db.TravelPoints.Find(id);
        //    if (travelpoint == null)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.NotFound);
        //    }

        //    db.TravelPoints.Remove(travelpoint);

        //    try
        //    {
        //        db.SaveChanges();
        //    }
        //    catch (DbUpdateConcurrencyException ex)
        //    {
        //        return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
        //    }

        //    return Request.CreateResponse(HttpStatusCode.OK, travelpoint);
        //}
    }
}