using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using HttpServiceNamespace;
using Interfaces;
using Json;

namespace TravelsClient
{
    public class SimpleResultResultReader : IResultReader<SimpleResult>
    {
        private readonly string onErrorMessage;
        private readonly HttpStatusCode expectedCode;

        public SimpleResultResultReader(string onErrorMessage, HttpStatusCode expectedCode)
        {
            this.onErrorMessage = onErrorMessage;
            this.expectedCode = expectedCode;
        }

        public Task<SimpleResult> FromResponse(HttpStatusCode statusCode, WebHeaderCollection headers, Stream responseStream)
        {
            if (statusCode != expectedCode)
                throw new Exception(string.Format("Http Operation code is {0} but {1} expected", statusCode, expectedCode));

            return Task.FromResult(new SimpleResult(true, statusCode, null));
        }

        public SimpleResult FromException(string errorMessage, HttpStatusCode httpCode)
        {
            return new SimpleResult(false, httpCode, string.Format("{0}. Exception was: {1}", onErrorMessage, errorMessage));
        }
    }

    public class TravelResultReader : IResultReader<SimpleResult<Travel>>
    {
        private readonly string onErrorMessage;

        public TravelResultReader(string onErrorMessage)
        {
            this.onErrorMessage = onErrorMessage;
        }

        public async Task<SimpleResult<Travel>> FromResponse(HttpStatusCode statusCode, WebHeaderCollection headers, Stream responseStream)
        {
            if (statusCode == HttpStatusCode.OK || statusCode == HttpStatusCode.Created)
            {


                var travel = ParseTravel(responseStream);
                return new SimpleResult<Travel>(true, statusCode, null, travel);
            }
            else
                throw new Exception(string.Format("Unexpected HTTP status code: {0}", statusCode));
        }

        public SimpleResult<Travel> FromException(string errorMessage, HttpStatusCode statusCode)
        {
            return new SimpleResult<Travel>(false,
                                    statusCode,
                                    string.Format("{0}. Exception was: {1}", onErrorMessage, errorMessage),
                                    null);
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
        private const string ADD_TRAVEL_POINT_GENERIC_ERROR_MESSAGE = "Error while adding travel point(s)";

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
        private readonly HttpService client;

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

            client = new HttpService(null, null) 
            { 
                HttpGetTimeout = new TimeSpan(0, 0, 30), 
                HttpPostTimeout = new TimeSpan(0, 0, 30) 
            };
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

        public async Task<SimpleResult<Travel>> FindActiveTravelAsync()
        {
            var uri = CreateUri("Travels/active", KeyParam, VehicleParam);
            var args = ExecuteArguments<SimpleResult<Travel>>.CreateGET(uri, new TravelResultReader(FIND_OPENED_TRAVEL_GENERIC_ERROR_MESSAGE));

            return await client.ExecuteAsync(args);
        }

        public async Task<SimpleResult<Travel>> GetTravelAsync(int id)
        {
            var uri = CreateUri("Travels/get", KeyParam, CreateArg(ARG_ID, id.ToString()));
            var args = ExecuteArguments<SimpleResult<Travel>>.CreateGET(uri, new TravelResultReader(GET_TRAVEL_GENERIC_ERROR_MESSAGE));

            return await client.ExecuteAsync(args);
        }

        public async Task<SimpleResult<Travel>> OpenTravelAsync(string name)
        {
            if (string.IsNullOrEmpty(name))
                return new SimpleResult<Travel>(false, HttpStatusCode.Unused, "Name is not provided", null);

            var uri = CreateUri("Travels/open", KeyParam, VehicleParam, CreateArg(ARG_NAME, name));

            var args = ExecuteArguments<SimpleResult<Travel>>.Create(uri, "POST", new TravelResultReader(OPEN_TRAVEL_GENERIC_ERROR_MESSAGE), null);

            return await client.ExecuteAsync(args);
        }

        public async Task<SimpleResult> CloseTravelAsync(Travel travel)
        {
            if (travel == null)
                return new SimpleResult(false, HttpStatusCode.Unused, "Travel is not provided.");

            var uri = CreateUri("Travels/close", KeyParam, CreateArg(ARG_ID, travel.ID.ToString()));

            var args = ExecuteArguments<SimpleResult>.Create(uri, "POST", new SimpleResultResultReader(CLOSE_TRAVEL_GENERIC_ERROR_MESSAGE, HttpStatusCode.OK), null);

            return await client.ExecuteAsync(args);
        }

        public async Task<SimpleResult> RenameTravelAsync(Travel travel)
        {
            if (travel == null)
                return new SimpleResult(false, HttpStatusCode.Unused, "Travel is not provided.");

            if (string.IsNullOrEmpty(travel.Name))
                return new SimpleResult(false, HttpStatusCode.Unused, "Travel is was invalid");

            var tId = CreateArg(ARG_ID, travel.ID.ToString());
            var name = CreateArg(ARG_NAME, travel.Name);
            var uri = CreateUri("Travels/rename", KeyParam, tId, name);

            var args = ExecuteArguments<SimpleResult>.Create(uri, "POST", new SimpleResultResultReader(RENAME_TRAVEL_GENERIC_ERROR_MESSAGE, HttpStatusCode.OK), null);

            return await client.ExecuteAsync(args);
        }

        public async Task<SimpleResult> DeleteTravelAsync(Travel travel)
        {
            if (travel == null)
                return new SimpleResult(false, HttpStatusCode.Unused, "Travel is not provided.");

            var uri = CreateUri("Travels/delete", KeyParam, CreateArg(ARG_ID, travel.ID.ToString()));

            var args = ExecuteArguments<SimpleResult>.Create(uri, "DELETE", new SimpleResultResultReader(DELETE_TRAVEL_GENERIC_ERROR_MESSAGE, HttpStatusCode.NoContent), null);

            return await client.ExecuteAsync(args);
        }

        public async Task<SimpleResult> AddTravelPointAsync(TravelPoint tp, Travel travel)
        {
            if (travel == null)
                return new SimpleResult(false, HttpStatusCode.Unused, "Travel is not provided.");

            if (tp == null)
                return new SimpleResult(false, HttpStatusCode.Unused, "Travel point is not provided.");

            var tId = CreateArg(ARG_TRAVELID, travel.ID.ToString());
            var lat = CreateArg(ARG_LAT, tp.Lat.ToString());
            var lon = CreateArg(ARG_LON, tp.Lon.ToString());
            var speed = CreateArg(ARG_SPEED, tp.Speed.ToString());
            var type = CreateArg(ARG_TYPE, ((int)tp.Type).ToString());
            var descr = CreateArg(ARG_DESCRIPTION, tp.Description);

            var uri = CreateUri("TravelPoints/add", KeyParam, VehicleParam, tId, lat, lon, speed, type, descr);

            var args = ExecuteArguments<SimpleResult>.Create(uri, "POST", new SimpleResultResultReader(ADD_TRAVEL_POINT_GENERIC_ERROR_MESSAGE, HttpStatusCode.OK), null);

            return await client.ExecuteAsync(args);
        }

        public async Task<SimpleResult> AddTravelPointAsync(IEnumerable<TravelPoint> tps, Travel travel)
        {
            if (travel == null)
                return new SimpleResult(false, HttpStatusCode.Unused, "Travel is not provided.");

            if (tps == null)
                return new SimpleResult(false, HttpStatusCode.Unused, "Travel points is not provided.");

            var tId = CreateArg(ARG_TRAVELID, travel.ID.ToString());

            StringBuilder body = new StringBuilder();

            foreach (var tp in tps)
            {
                var lat = string.Concat(ARG_LAT, "=", tp.Lat);
                var lon = string.Concat(ARG_LON, "=", tp.Lon);
                var speed = string.Concat(ARG_SPEED, "=", tp.Speed);
                var type = string.Concat(ARG_TYPE, "=", (int)tp.Type);
                var time = string.Concat(ARG_TIME, "=", tp.Time.ToFileTimeUtc());
                var descr = string.Concat(ARG_DESCRIPTION, "=", tp.Description);

                body.Append(string.Join(";", lat, lon, speed, type, time, descr));
                body.Append("|");
            }

            if (body.Length > 0)
                body.Remove(body.Length - 1, 1);

            var uri = CreateUri("TravelPoints/addmany", KeyParam, tId);

            var args = ExecuteArguments<SimpleResult>.Create(uri, "POST", new SimpleResultResultReader(ADD_TRAVEL_POINT_GENERIC_ERROR_MESSAGE, HttpStatusCode.OK), new StringRequestWriter(body.ToString()));

            return await client.ExecuteAsync(args);
        }

        public void Cancel()
        {
            client.Abort();
        }
    }
}
