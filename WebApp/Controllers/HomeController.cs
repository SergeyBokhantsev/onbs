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

            if (currentTravel == null)
            {
                ViewBag.Message = "Нет поездок :(";
                return View("EmptyTravel");
            }

            var sortedPoints = (from p in currentTravel.Track
                               orderby p.Time ascending
                               select p).ToList();

            if (sortedPoints.Any())
            {
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

                return View("CurrentTravel");
            }
            else
                return View("EmptyTravel");
        }

        public ActionResult ShowTravel(int id)
        {
            var db = new ONBSContext();

            var currentTravel = db.Travels.Find(id);

            if (currentTravel == null)
            {
                ViewBag.Message = "Нет такой поездки :(";
                return View("EmptyTravel");
            }

            var sortedPoints = (from p in currentTravel.Track
                                orderby p.Time ascending
                                select p).ToList();

            if (sortedPoints.Any())
            {
                TravelPoint maxSpeedPoint = sortedPoints.First();

                foreach (var p in sortedPoints)
                {
                    if (p.Speed > maxSpeedPoint.Speed)
                        maxSpeedPoint = p;
                }

                ViewBag.MaxSpeedPopup = maxSpeedPoint.Speed.ToString("0.##");
                ViewBag.MaxSpeedLocation = string.Format(gpointTemplate, maxSpeedPoint.Lat, maxSpeedPoint.Lon);

                var activePoint = sortedPoints.Last();

                var timeAgo = (int)(DateTime.Now - activePoint.Time).TotalMinutes;

                ViewBag.Popup = string.Format("Мы были здесь {0} минут назад", timeAgo);

                ViewBag.InfoHeading = string.Concat(timeAgo, " минут назад");
                ViewBag.InfoLine1 = string.Concat("Скорость: ", activePoint.Speed.ToString("0.#"));
                ViewBag.InfoLine2 = string.Concat("Проехано: ", (GetDistance(sortedPoints)/1000).ToString("0.#"), " км");
                ViewBag.InfoLine3 = string.Concat("Начало ", currentTravel.StartTime.AddHours(3));
                ViewBag.InfoLine4 = string.Concat("Время поездки, минут: ", (int)(activePoint.Time - currentTravel.StartTime).TotalMinutes);

                if (timeAgo > 10 && activePoint.Speed < 10)
                    ViewBag.InfoLine5 = "Видимо поездка закончена";

                ViewBag.MapCenter = string.Format(gpointTemplate, sortedPoints.Last().Lat, sortedPoints.Last().Lon);
                ViewBag.TravelPoints = string.Join(",", sortedPoints.Select(p => string.Format(gpointTemplate, p.Lat, p.Lon)));

                return View("CurrentTravel");
            }
            else
            {
                ViewBag.Message = "Эта поездка еще не содержит точек :(";
                return View("EmptyTravel");
            }
        }

        public ActionResult TravelsList()
        {
            var db = new ONBSContext();
            var result = new TravelsList();

            result.Today = GetTravelsFor(db, DateTime.Now).ToArray();
            result.Yesterday = GetTravelsFor(db, DateTime.Now.AddDays(-1)).ToArray();

            return View(result);
        }

        private IEnumerable<LinkItem> GetTravelsFor(ONBSContext db, DateTime day)
        {
            var travels = (from t in db.Travels
                           where t.StartTime.Year == day.Year
                         && t.StartTime.Month == day.Month
                         && t.StartTime.Day == day.Day
                           orderby t.StartTime descending
                           select t).ToList();

            foreach (var travel in travels)
            {
                yield return new LinkItem { Caption = travel.Name, Action = "ShowTravel", Args = new { id = travel.ID } };
            }
        }

        private double GetDistance(List<TravelPoint> points)
        {
            double result = 0;

            if (points != null && points.Count > 1)
            {
                var prevPoint = points.First();

                for (int i=1; i< points.Count; ++i)
                {
                    result += Interfaces.GPS.Helpers.GetDistance(new Interfaces.GPS.GeoPoint(prevPoint.Lat, prevPoint.Lon), 
                                                                new Interfaces.GPS.GeoPoint(points[i].Lat, points[i].Lon));

                    prevPoint = points[i];
                }
            }

            return result;
        }
    }
}
