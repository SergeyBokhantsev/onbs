using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Interfaces;

namespace TravelsClient
{
    public class AsyncClient
    {
        private const int BUSY = 1;
        private const int FREE = 0;

        private readonly Client client;
        private int busyState;

        public AsyncClient(Uri serviceUri, string key, string vehicleId, ILogger logger)
        {
            client = new Client(serviceUri, key, vehicleId, logger);
        }

        private bool AccessAllowed()
        {
            int currentState = Interlocked.Exchange(ref busyState, BUSY);
            return currentState != BUSY;
        }

        public bool TryFindActiveTravelAsync(Action<TravelResult> callback)
        {
            if (AccessAllowed())
            {
                ThreadPool.QueueUserWorkItem(state =>
                    {
                        var result = client.FindActiveTravel();

                        Interlocked.Decrement(ref busyState);

                        if (callback != null)
                            callback(result);
                    });

                return true;
            }
            else
            {
                return false;
            }
        }

        public bool TryOpenNewTravel(string name, Action<TravelResult> callback)
        {
            if (AccessAllowed())
            {
                ThreadPool.QueueUserWorkItem(state =>
                {
                    var result = client.OpenTravel(name);

                    Interlocked.Decrement(ref busyState);

                    if (callback != null)
                        callback(result);
                });

                return true;
            }
            else
            {
                return false;
            }
        }

        public bool TryAddTravelPoints(IEnumerable<TravelPoint> points, Travel travel, Action<ActionResult> callback)
        {
            if (AccessAllowed())
            {
                ThreadPool.QueueUserWorkItem(state =>
                {
                    var result = client.AddTravelPoint(points, travel);

                    Interlocked.Decrement(ref busyState);

                    if (callback != null)
                        callback(result);
                });

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
