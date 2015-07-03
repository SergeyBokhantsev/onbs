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

        public bool TryFindActiveTravelAsync(Action<Travel> callback)
        {
            int currentState = Interlocked.Exchange(ref busyState, BUSY);

            if (currentState == BUSY)
                return false;

            ThreadPool.QueueUserWorkItem(state =>
                {
                    var travel = client.FindActiveTravel();

                    if (callback != null)
                        callback(travel);

                    Interlocked.Decrement(ref busyState);
                });

            return true;
        }
    }
}
