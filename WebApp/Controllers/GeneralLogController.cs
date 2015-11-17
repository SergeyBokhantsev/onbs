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
    public class GeneralLogController : ONBSApiControllerBase
    {
        private ONBSContext db = new ONBSContext();

        // POST api/GeneralLog/new
        [HttpPost]
        [ActionName("new")]
        public HttpResponseMessage NewGeneralLog(string key, string vehicle)
        {
            return Monitor(key, () =>
            {
                if (string.IsNullOrEmpty(vehicle))
                    throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.BadRequest));

                var getBodyTask = Request.Content.ReadAsStringAsync();

                if (getBodyTask.Wait(5 * 60 * 1000))
                {
                    var log = new LogEntry { Date = DateTime.UtcNow, Vehicle = vehicle, Message = getBodyTask.Result };

                    db.GeneralLogs.Add(log);
                    db.SaveChanges();

                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created);
                    response.Headers.Add("LogId", log.ID.ToString());
                    return response;
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.RequestTimeout, "Timeout reading request");
                }
            });
        }

        // PUT api/GeneralLog/new
        [HttpPut]
        [ActionName("append")]
        public HttpResponseMessage AppendGeneralLog(string key, string vehicle, int id)
        {
            return Monitor(key, () =>
            {
                if (string.IsNullOrEmpty(vehicle))
                    throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.BadRequest));

                var logEntry = db.GetLogEntry(id);

                if (logEntry.Vehicle != vehicle)
                    throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.BadRequest));

                var getBodyTask = Request.Content.ReadAsStringAsync();

                if (getBodyTask.Wait(5 * 60 * 1000))
                {
                    logEntry.Message += getBodyTask.Result;
                    db.Entry(logEntry).State = EntityState.Modified;
                    db.SaveChanges();

                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
                    return response;
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.RequestTimeout, "Timeout reading request");
                }
            });
        }

        //// GET api/GeneralLog
        //public IEnumerable<LogEntry> GetLogEntries()
        //{
        //    return db.GeneralLogs.AsEnumerable();
        //}

        //// GET api/GeneralLog/5
        //public LogEntry GetLogEntry(int id)
        //{
        //    LogEntry logentry = db.GeneralLogs.Find(id);
        //    if (logentry == null)
        //    {
        //        throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
        //    }

        //    return logentry;
        //}

        //// PUT api/GeneralLog/5
        //public HttpResponseMessage PutLogEntry(int id, LogEntry logentry)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
        //    }

        //    if (id != logentry.ID)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.BadRequest);
        //    }

        //    db.Entry(logentry).State = EntityState.Modified;

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

        //// POST api/GeneralLog
        //public HttpResponseMessage PostLogEntry(LogEntry logentry)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.GeneralLogs.Add(logentry);
        //        db.SaveChanges();

        //        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, logentry);
        //        response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = logentry.ID }));
        //        return response;
        //    }
        //    else
        //    {
        //        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
        //    }
        //}

        //// DELETE api/GeneralLog/5
        //public HttpResponseMessage DeleteLogEntry(int id)
        //{
        //    LogEntry logentry = db.GeneralLogs.Find(id);
        //    if (logentry == null)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.NotFound);
        //    }

        //    db.GeneralLogs.Remove(logentry);

        //    try
        //    {
        //        db.SaveChanges();
        //    }
        //    catch (DbUpdateConcurrencyException ex)
        //    {
        //        return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
        //    }

        //    return Request.CreateResponse(HttpStatusCode.OK, logentry);
        //}

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}