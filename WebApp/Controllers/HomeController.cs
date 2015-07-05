using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApp.Models;

namespace WebApp.Controllers
{
    public class HomeController : Controller
    {
        private const string gpointTemplate = "new google.maps.LatLng({0}, {1})";

        //
        // GET: /Home/
        public string Index()
        {
            return "stub";
        }

        public ActionResult CurrentTravel()
        {
            var db = new ONBSContext();

            var currentTravel = (from t in db.Travels
                                 orderby t.StartTime descending
                                 select t).First();

            var sortedPoints = from p in currentTravel.Track
                               orderby p.Time ascending
                               select p;

            var activePoint = sortedPoints.Last();

            var timeAgo = (int)(DateTime.Now - activePoint.Time).TotalMinutes;

            ViewBag.Popup = string.Format("Мы были здесь {0} минут назад", timeAgo);

            ViewBag.InfoHeading = string.Concat(timeAgo, " минут назад");
            ViewBag.InfoLine1 = string.Concat("Скорость: ", activePoint.Speed.ToString("0.#"));
            ViewBag.InfoLine2 = string.Concat("Начало ", currentTravel.StartTime.AddHours(3));
            ViewBag.InfoLine3 = string.Concat("Время поездки, минут: ", (int)(activePoint.Time - currentTravel.StartTime).TotalMinutes);

            if (timeAgo > 10 && activePoint.Speed < 10)
                ViewBag.InfoLine4 = "Видимо поездка закончена";

            ViewBag.MapCenter = string.Format(gpointTemplate, sortedPoints.Last().Lat, sortedPoints.Last().Lon);
            ViewBag.TravelPoints = string.Join(",", sortedPoints.Select(p => string.Format(gpointTemplate, p.Lat, p.Lon)));

            return View();
        }

    }
}
