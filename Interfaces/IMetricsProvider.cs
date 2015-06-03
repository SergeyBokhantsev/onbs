using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces
{
    public interface IMetrics : IEnumerable<KeyValuePair<string, object>>
    {
        string ProviderName { get; }
    }

    public class Metrics : IMetrics
    {
        private KeyValuePair<string, object>[] metrics;

        public string ProviderName { get; private set; }

        public Metrics(string providerName, int count)
        {
            ProviderName = providerName;
            metrics = new KeyValuePair<string, object>[count];
        }

        public void Add(int index, string key, object value)
        {
            metrics[index] = new KeyValuePair<string, object>(key, value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return metrics.GetEnumerator();
        }

        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<string, object>>)metrics).GetEnumerator();
        }
    }

    public delegate void MetricsUpdatedEventHandler(object sender, IMetrics metrics);

    public interface IMetricsProvider
    {
        event MetricsUpdatedEventHandler MetricsUpdated;
    }
}
