using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using HttpClient;
using Interfaces;
using Json;

namespace TravelsClient
{
    public abstract class TravelClientResult
    {
        public bool Success { get; protected set; }
        public string Error { get; protected set; }

        protected TravelClientResult()
        {
        }

        protected void CreateErrorText(ClientResponse response, string error)
        {
            error = error ?? "Unknown error";
            var responseMessage = response != null ? response.Error : "No response";
            Error = string.Format("{0}. Http response was: {1}", error, responseMessage);
        }
    }

    public class ActionResult : TravelClientResult
    {
        public ActionResult(ClientResponse response, string errorMessage, HttpStatusCode desiredStatus = HttpStatusCode.OK)
        {
            if (response != null && response.Status == desiredStatus)
            {
                Success = true;
            }
            else
            {
                Success = false;
                CreateErrorText(response, errorMessage);
            }
        }
    }

    public class TravelResult : TravelClientResult
    {
        public Travel Travel { get; private set; }

        public TravelResult(string error)
        {
            Success = false;
            Error = error;
        }

        public TravelResult(ClientResponse response, string genericErrorMessage, params HttpStatusCode[] desiredStatuses)
        {
            if (response != null && desiredStatuses != null && desiredStatuses.Contains(response.Status))
            {
                try
                {
                    if (response.Status != HttpStatusCode.NotFound)
                    {
                        using (var stream = response.GetStream())
                        {
                            Travel = ParseTravel(stream);
                        }
                    }

                    Success = true;
                }
                catch (Exception ex)
                {
                    Success = false;
                    CreateErrorText(response, string.Format("Unable to parse Travel. {0}", ex.Message));
                }
            }
            else
            {
                Success = false;
                CreateErrorText(response, genericErrorMessage);
            }
        }

        private Travel ParseTravel(Stream stream)
        {
            var jsonObj = new JsonParser().Parse(stream);
            var id = JPath.GetFieldValue<int>(jsonObj, "ID");
            var name = JPath.GetFieldValue<string>(jsonObj, "Name");
            var closed = JPath.GetFieldValue<bool>(jsonObj, "Closed");
            var startTime = JPath.GetFieldValue<string>(jsonObj, "StartTime");
            var endTime = JPath.GetFieldValue<string>(jsonObj, "EndTime");
            return new Travel { ID = id, Name = name, Closed = closed, StartTime = DateTime.Parse(startTime), EndTime = DateTime.Parse(endTime) };
        }
    }

    public class Client
    {
        private const string FIND_OPENED_TRAVEL_GENERIC_ERROR_MESSAGE = "Error while searching active travel.";
        private const string GET_TRAVEL_GENERIC_ERROR_MESSAGE = "Error while getting travel.";
        private const string OPEN_TRAVEL_GENERIC_ERROR_MESSAGE = "Error while creating new travel.";
        private const string CLOSE_TRAVEL_GENERIC_ERROR_MESSAGE = "Error while closing travel.";
        private const string RENAME_TRAVEL_GENERIC_ERROR_MESSAGE = "Error while renaming travel.";
        private const string DELETE_TRAVEL_GENERIC_ERROR_MESSAGE = "Error while deleting travel.";
        private const string ADD_TRAVEL_POINT_GENERIC_ERROR_MESSAGE = "Error while adding travel point(s).";

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

        public TravelResult FindActiveTravel()
        {
            var url = CreateUri("Travels/active", KeyParam, VehicleParam);
            using (var response = client.Get(url, 3, 3000))
            {
                return new TravelResult(response, FIND_OPENED_TRAVEL_GENERIC_ERROR_MESSAGE, HttpStatusCode.OK, HttpStatusCode.NotFound);
            }
        }

        public async Task<TravelResult> FindActiveTravelAsync()
        {
            return await Task.Run<TravelResult>((Func<TravelResult>)FindActiveTravel);
        }

        public TravelResult GetTravel(int id)
        {
            var url = CreateUri("Travels/get", KeyParam, CreateArg(ARG_ID, id.ToString()));
            using (var response = client.Get(url, 3, 3000))
            {
                return new TravelResult(response, GET_TRAVEL_GENERIC_ERROR_MESSAGE, HttpStatusCode.OK);
            }
        }

        public TravelResult OpenTravel(string name)
        {
            if (string.IsNullOrEmpty(name))
                return new TravelResult(null, "Name is not provided");

            var url = CreateUri("Travels/open", KeyParam, VehicleParam, CreateArg(ARG_NAME, name));
            using (var response = client.Post(url, null, 3, 3000))
            {
                return new TravelResult(response, OPEN_TRAVEL_GENERIC_ERROR_MESSAGE, HttpStatusCode.Created);
            }
        }

        public async Task<TravelResult> OpenTravelAsync(string name)
        {
            return await Task.Run<TravelResult>(() => OpenTravel(name));
        }

        public ActionResult CloseTravel(Travel travel)
        {
            if (travel == null)
                return new ActionResult(null, "Travel is not provided.");

            var url = CreateUri("Travels/close", KeyParam, CreateArg(ARG_ID, travel.ID.ToString()));
            using (var response = client.Post(url, null, 3, 3000))
            {
                return new ActionResult(response, CLOSE_TRAVEL_GENERIC_ERROR_MESSAGE);
            }
        }

        public ActionResult RenameTravel(Travel travel)
        {
            if (travel == null)
                return new ActionResult(null, "Travel is not provided.");

            if (string.IsNullOrEmpty(travel.Name))
                return new ActionResult(null, "Travel is was invalid");

            var tId = CreateArg(ARG_ID, travel.ID.ToString());
            var name = CreateArg(ARG_NAME, travel.Name);
            var url = CreateUri("Travels/rename", KeyParam, tId, name);
            using (var response = client.Post(url, null, 3, 3000))
            {
                return new ActionResult(response, RENAME_TRAVEL_GENERIC_ERROR_MESSAGE);
            }
        }

        public ActionResult DeleteTravel(Travel travel)
        {
            if (travel == null)
                return new ActionResult(null, "Travel is not provided.");

            var url = CreateUri("Travels/delete", KeyParam, CreateArg(ARG_ID, travel.ID.ToString()));
            using (var response = client.Delete(url))
            {
                return new ActionResult(response, DELETE_TRAVEL_GENERIC_ERROR_MESSAGE, HttpStatusCode.NoContent);
            }
        }

        public ActionResult AddTravelPoint(TravelPoint tp, Travel travel)
        {
            if (travel == null)
                return new ActionResult(null, "Travel is not provided.");

            if (tp == null)
                return new ActionResult(null, "Travel point is not provided.");

            try
            {
                var tId = CreateArg(ARG_TRAVELID, travel.ID.ToString());
                var lat = CreateArg(ARG_LAT, tp.Lat.ToString());
                var lon = CreateArg(ARG_LON, tp.Lon.ToString());
                var speed = CreateArg(ARG_SPEED, tp.Speed.ToString());
                var type = CreateArg(ARG_TYPE, ((int)tp.Type).ToString());
                var descr = CreateArg(ARG_DESCRIPTION, tp.Description);

                var url = CreateUri("TravelPoints/add", KeyParam, VehicleParam, tId, lat, lon, speed, type, descr);
                using (var response = client.Post(url, null, 3, 3000))
                {
                    return new ActionResult(response, ADD_TRAVEL_POINT_GENERIC_ERROR_MESSAGE);
                }
            }
            catch (Exception ex)
            {
                return new ActionResult(null, ex.Message);
            }
        }

        public ActionResult AddTravelPoint(IEnumerable<TravelPoint> tps, Travel travel)
        {
            if (travel == null)
                return new ActionResult(null, "Travel is not provided.");

            if (tps == null)
                return new ActionResult(null, "Travel points is not provided.");

            try
            {
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
                using (var response = client.Post(url, body.ToString(), 3, 3000))
                {
                    return new ActionResult(response, ADD_TRAVEL_POINT_GENERIC_ERROR_MESSAGE);
                }
            }
            catch (Exception ex)
            {
                return new ActionResult(null, ex.Message);
            }
        }

        public async Task<ActionResult> AddTravelPointAsync(IEnumerable<TravelPoint> tps, Travel travel)
        {
            return await Task.Run<ActionResult>(() => AddTravelPoint(tps, travel));
        }
    }
}
