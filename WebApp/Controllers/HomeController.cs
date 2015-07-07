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

        private const string markerTemplate = "var {0}_marker = new google.maps.Marker({{"
                                            + "position: {1},"
                                            + "map: map,"
                                            + "icon: '/content/images/{2}',"
                                            + "title: '{3}' }});";
        
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
                                 select t.ID).First();

            return CreateTravelMap(currentTravel, db);
        }

        public ActionResult ShowTravel(int id)
        {
            return CreateTravelMap(id, null);
        }

        private ActionResult CreateTravelMap(int id, ONBSContext _db)
        {
            var db = _db ?? new ONBSContext();

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

                ViewBag.MaxSpeedMarker = string.Format(markerTemplate,
                                                      "max_speed",
                                                      string.Format(gpointTemplate, maxSpeedPoint.Lat, maxSpeedPoint.Lon),
                                                      "speed.png",
                                                      string.Format("Максимальная скорость поездки: {0} км/ч", maxSpeedPoint.Speed.ToString("0.##")));

                var activePoint = sortedPoints.Last();

                var timeAgo = (int)(DateTime.Now - activePoint.Time).TotalMinutes;

                ViewBag.Popup = string.Format("Мы были здесь {0} минут назад", timeAgo);

                double distance;
                List<Tuple<TravelPoint, TimeSpan>> stopPoints;
                GetStatistics(sortedPoints, out distance, out stopPoints);

                ViewBag.StopMarkers = string.Concat(stopPoints.Select(p => string.Format(markerTemplate,
                    string.Concat("stop_", p.Item1.ID),
                    string.Format(gpointTemplate, p.Item1.Lat, p.Item1.Lon),
                    "stop.png",
                    string.Format("{0} - остановка {1} мин.", p.Item1.Time.ToString("HH:mm"), p.Item2.TotalMinutes.ToString("0"))
                    )));

                ViewBag.InfoHeading = string.Concat(timeAgo, " минут назад");
                ViewBag.InfoLine1 = string.Concat("Скорость: ", activePoint.Speed.ToString("0.#"));
                ViewBag.InfoLine2 = string.Concat("Проехано: ", (distance/1000).ToString("0.#"), " км");
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

        private void GetStatistics(List<TravelPoint> points, out double distance, out List<Tuple<TravelPoint, TimeSpan>> stopPoints)
        {
            distance = 0;
            stopPoints = new List<Tuple<TravelPoint, TimeSpan>>();

            if (points != null && points.Count > 1)
            {
                var prevPoint = points.First();

                for (int i=1; i< points.Count; ++i)
                {
                    distance += Interfaces.GPS.Helpers.GetDistance(new Interfaces.GPS.GeoPoint(prevPoint.Lat, prevPoint.Lon), 
                                                                new Interfaces.GPS.GeoPoint(points[i].Lat, points[i].Lon));

                    var span = points[i].Time - prevPoint.Time;
                    if (span.TotalMinutes > 2)
                        stopPoints.Add(new Tuple<TravelPoint, TimeSpan>(points[i], span));

                    prevPoint = points[i];
                }
            }
            
        }
    }
}
