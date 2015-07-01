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
        private const string ARG_ID = "id";
        private const string ARG_KEY = "key";
        private const string ARG_VEHICLE = "vehicle";
        private const string ARG_NAME = "name";
        private const string ARG_TRAVELID = "travel_id";
        private const string ARG_LAT = "lat";
        private const string ARG_LON = "lon";
        private const string ARG_SPEED = "speed";
        private const string ARG_DESCRIPTION = "description";
        private const string ARG_TYPE = "type";
        private const string ARG_TIME = "time";

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
                return CreateArg(ARG_KEY, key);
            }
        }

        private string VehicleParam
        {
            get
            {
                return CreateArg(ARG_VEHICLE, vehicleId);
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

        private string CreateArg(string name, string value)
        {
            return string.Concat(name, "=", HttpUtility.UrlEncode(value));
        }

        private Uri CreateUri(string relativeUrl, params string[] args)
        {
            var argLine = args.Any() ? string.Concat("?", string.Join("&", args)) : string.Empty;
            return new Uri(serviceUri, string.Format("{0}{1}{2}", apiPrefix, relativeUrl, argLine));
        }

        private Travel ParseTravel(Stream stream)
        {
            var jsonObj = new JsonParser().Parse(stream);
            var id = JPath.GetFieldValue<int>(jsonObj, "ID");
            var name = JPath.GetFieldValue<string>(jsonObj, "Name");
            var closed = JPath.GetFieldValue<bool>(jsonObj, "Closed");
            //TODO: get start date
            return new Travel { ID = id, Name = name, Closed = closed };
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

        public Travel GetTravel(int id)
        {
            var url = CreateUri("Travels/get", KeyParam, CreateArg(ARG_ID, id.ToString()));
            var response = client.Get(url);

            if (response.Status == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
            else if (response.Status != System.Net.HttpStatusCode.OK)
            {
                throw new Exception(string.Format("Unable to get travel: {0}", response.Error));
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

        public Travel OpenTravel(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("travel name");

            var url = CreateUri("Travels/open", KeyParam, VehicleParam, CreateArg(ARG_NAME, name));
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

        public void CloseTravel(Travel travel)
        {
            if (travel == null)
                throw new ArgumentNullException("travel");

            var url = CreateUri("Travels/close", KeyParam, CreateArg(ARG_ID, travel.ID.ToString()));
            var response = client.Post(url, null);

            if (response.Status != System.Net.HttpStatusCode.OK)
            {
                throw new Exception(string.Format("Unable to close travel: {0}", response.Error));
            }
        }

        public void RenameTravel(Travel travel)
        {
            if (travel == null)
                throw new ArgumentNullException("travel");

            if (string.IsNullOrEmpty(travel.Name))
                throw new ArgumentNullException("travel name");

            var tId = CreateArg(ARG_ID, travel.ID.ToString());
            var name = CreateArg(ARG_NAME, travel.Name);

            var url = CreateUri("Travels/rename", KeyParam, tId, name);

            var response = client.Post(url, null);

            if (response.Status != System.Net.HttpStatusCode.OK)
            {
                throw new Exception(string.Format("Unable to rename travel: {0}", response.Error));
            }
        }

        public void DeleteTravel(Travel travel)
        {
            if (travel == null)
                throw new ArgumentNullException("travel");

            var url = CreateUri("Travels/delete", KeyParam, CreateArg(ARG_ID, travel.ID.ToString()));
            var response = client.Delete(url);

            if (response.Status != System.Net.HttpStatusCode.NoContent)
            {
                throw new Exception(string.Format("Unable to delete travel: {0}", response.Error));
            }
        }

        public void AddTravelPoint(TravelPoint tp, Travel travel)
        {
            if (tp == null)
                throw new ArgumentNullException("travel point");

            var tId = CreateArg(ARG_TRAVELID, travel.ID.ToString());
            var lat = CreateArg(ARG_LAT, tp.Lat.ToString());
            var lon = CreateArg(ARG_LON, tp.Lon.ToString());
            var speed = CreateArg(ARG_SPEED, tp.Speed.ToString());
            var type = CreateArg(ARG_TYPE, ((int)tp.Type).ToString());
            var descr = CreateArg(ARG_DESCRIPTION, tp.Description);

            var url = CreateUri("TravelPoints/add", KeyParam, VehicleParam, tId, lat, lon, speed, type, descr);

            var response = client.Post(url, null);

            if (response.Status != System.Net.HttpStatusCode.OK)
            {
                throw new Exception(string.Format("Unable to add travel point: {0}", response.Error));
            }
        }

        public void AddTravelPoint(IEnumerable<TravelPoint> tps, Travel travel)
        {
            if (tps == null)
                throw new ArgumentNullException("travel points");

            var tId = CreateArg(ARG_TRAVELID, travel.ID.ToString());

            StringBuilder body = new StringBuilder();

            foreach (var tp in tps)
            {
                var lat = string.Concat(ARG_LAT, "=", tp.Lat);
                var lon = string.Concat(ARG_LON, "=", tp.Lon);
                var speed = string.Concat(ARG_SPEED, "=", tp.Speed);
                var type = string.Concat(ARG_TYPE, "=", (int)tp.Type);
                var time = string.Concat(ARG_TIME, "=", tp.Time.ToUniversalTime());
                var descr = string.Concat(ARG_DESCRIPTION, "=", tp.Description);

                body.Append(string.Join(";", lat, lon, speed, type, time, descr));
                body.Append("|");
            }

            if (body.Length > 0)
                body.Remove(body.Length - 1, 1);

            var url = CreateUri("TravelPoints/addmany", KeyParam, tId);

            var response = client.Post(url, body.ToString());

            if (response.Status != System.Net.HttpStatusCode.OK)
            {
                throw new Exception(string.Format("Unable to add travel point: {0}", response.Error));
            }
        }
    }
}
