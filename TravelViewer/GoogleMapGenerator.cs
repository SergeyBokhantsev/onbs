using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApp.Models;

namespace TravelViewer
{
    internal static class GoogleMapGenerator
    {
        private static int Comparer(TravelPoint t1, TravelPoint t2)
        {
            if (t1.Time == t2.Time)
                return 0;
            else return t1.Time > t2.Time ? 1 : -1;
        }

        public static string CreateHtml(string templatePath, IEnumerable<TravelPoint> points)
        {
            var sortedPoints = points.ToList();

            if (sortedPoints.Count == 0)
                return null;

            sortedPoints.Sort(Comparer);

            const string pointTemplate = "new google.maps.LatLng({0}, {1})";

            var mapCenter = string.Format(pointTemplate, sortedPoints.First().Lat, sortedPoints.First().Lon);

            var pointsJS = string.Join(",", sortedPoints.Select(p => string.Format(pointTemplate, p.Lat, p.Lon)));

            var htmlTemplate = File.ReadAllText(templatePath);

            return htmlTemplate.Replace("{{TRAVEL_POINTS}}", pointsJS)
                .Replace("{{MAP_CENTER}}", mapCenter);
        }
    }
}
