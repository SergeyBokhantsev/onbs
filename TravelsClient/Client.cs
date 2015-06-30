using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Interfaces;
using Json;

namespace TravelsClient
{
    public class Client
    {
        private const string apiPrefix = "/api/";

        private readonly Uri serviceUri;
        private readonly string key;
        private readonly string vehicleId;

        private readonly ILogger logger;
        private readonly HttpClient.Client client;

        private string KeyParam
        {
            get
            {
                return string.Concat("key=", key);
            }
        }

        private string VehicleParam
        {
            get
            {
                return string.Concat("vehicle=", key);
            }
        }

        public Client(Uri serviceUri, string key, string vehicleId, ILogger logger)
        {
            this.key = key;
            this.vehicleId = vehicleId;
            this.serviceUri = serviceUri;

            this.logger = logger;

            client = new HttpClient.Client();
            client.ClientException += exc => logger.Log(this, exc);
        }

        private Uri CreateUri(string relativeUrl, params string[] args)
        {
            var argLine = args.Any() ? string.Concat("?", string.Join("&", args)) : string.Empty;
            return new Uri(serviceUri, string.Format("{0}{1}{2}", apiPrefix, relativeUrl, HttpUtility.UrlEncode(argLine)));
        }

        private Travel ParseTravel(Stream stream)
        {
            var jsonObj = new JsonParser().Parse(stream);
            var id = JPath.GetFieldValue<int>(jsonObj, "ID");
            var name = JPath.GetFieldValue<string>(jsonObj, "Name");
            //TODO: get start date
            return new Travel { ID = id, Name = name };
        }

        public Travel FindActiveTravel()
        {
            var url = CreateUri("Travels/active", KeyParam, VehicleParam);
            var response = client.Get(url);

            if (response.Status == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
            else if (response.Status != System.Net.HttpStatusCode.OK)
            {
                logger.Log(this, string.Format("Server error while finding active travel. Status was {0}", response.Status), LogLevels.Warning);
                return null;
            }
            else
            {
                try
                {
                    using (var stream = response.GetStream())
                    {
                        return ParseTravel(stream);
                    }
                }
                catch (Exception ex)
                {
                    logger.Log(this, ex);
                    return null;
                }
            }
        }

        public Travel CreateNewTravel(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("travel name");

            var url = CreateUri("Travels/open", KeyParam, VehicleParam, string.Concat("name=", name));
            var response = client.Post(url, null);

            if (response.Status != System.Net.HttpStatusCode.Created)
            {
                throw new Exception(string.Format("Unable to create new travel: {0}", response.Error));
            }

            try
            {
                using (var stream = response.GetStream())
                {
                    return ParseTravel(stream);
                }
            }
            catch (Exception ex)
            {
                logger.Log(this, ex);
                return null;
            }
        }

        public void DeleteTravel(Travel travel)
        {
            if (travel == null)
                throw new ArgumentNullException("travel");

            var url = CreateUri(string.Concat("Travels/", travel.ID), KeyParam);
            var response = client.Delete(url);

            if (response.Status != System.Net.HttpStatusCode.NoContent)
            {
                throw new Exception(string.Format("Unable to delete travel: {0}", response.Error));
            }
        }

        public void AddTravelPoint(TravelPoint tp, int travelId)
        {
            if (tp == null)
                throw new ArgumentNullException("travel point");

            var tId = string.Concat("travel=", travelId);
            var lat = string.Concat("lat=", tp.Lat);
            var lon = string.Concat("lon=", tp.Lon);
            var speed = string.Concat("speed=", tp.Speed);
            var type = string.Concat("type=", tp.Type);

            var url = CreateUri("TravelPoints/add", KeyParam, VehicleParam, tId, lat, lon, speed, type);

            var response = client.Post(url, null);

            if (response.Status != System.Net.HttpStatusCode.Created)
            {
                throw new Exception(string.Format("Unable to add travel point: {0}", response.Error));
            }
        }

        public void AddTravelPoint(IEnumerable<TravelPoint> tps, int travelId)
        {
            if (tps == null)
                throw new ArgumentNullException("travel points");

            StringBuilder body = new StringBuilder();

            foreach (var tp in tps)
            {
                var lat = string.Concat("lat=", tp.Lat);
                var lon = string.Concat("lon=", tp.Lon);
                var speed = string.Concat("speed=", tp.Speed);
                var type = string.Concat("type=", tp.Type);

                body.AppendLine(string.Join(";", 
                    "lat=", lat, 
                    "lon=", lon,
                    "speed=", speed, 
                    "type=", type));
            }

            var tId = string.Concat("travel=", travelId);

            var url = CreateUri("TravelPoints/addmany", KeyParam, VehicleParam, tId);

            var response = client.Post(url, body.ToString());

            if (response.Status != System.Net.HttpStatusCode.Created)
            {
                throw new Exception(string.Format("Unable to add travel point: {0}", response.Error));
            }
        }

        public void RenameTravel(Travel travel)
        {
            if (travel == null)
                throw new ArgumentNullException("travel");

            if (string.IsNullOrEmpty(travel.Name))
                throw new ArgumentNullException("travel name");

            var tId = string.Concat("travel=", travel.ID);
            var name = string.Concat("name=", travel.Name);

            var url = CreateUri("Travels/rename", KeyParam, tId, name);

            var response = client.Post(url, null);

            if (response.Status != System.Net.HttpStatusCode.OK)
            {
                throw new Exception(string.Format("Unable to rename travel: {0}", response.Error));
            }
        }
    }
}
