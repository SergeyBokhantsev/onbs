using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApp.Models;

namespace WebApp.Controllers
{
    public class LogsController : Controller
    {
        //
        // GET: /Logs/

        public ActionResult List()
        {
            var db = new ONBSContext();

            var logs = (from l in db.GeneralLogs
                                 orderby l.Date descending
                                 select new { Date = l.Date, Id = l.ID, Vehicle = l.Vehicle }).Take(20).ToArray();

            var links = logs.Select(l => new LinkItem { Caption = string.Format("{0} - {1}", l.Date, l.Vehicle), Action = "ShowLog", Args = new { id = l.Id } }).ToList();

            return View(new LinkItemsBag { Items = links });
        }

        public ActionResult ShowLog(int id)
        {
            var db = new ONBSContext();

            var log = db.GeneralLogs.FirstOrDefault(l => l.ID == id);

            return View(log);
        }
    }
}
