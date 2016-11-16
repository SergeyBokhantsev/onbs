using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Interfaces
{
    public interface IMetricsService
    {
        IEnumerable<IMetricsProvider> Providers { get; }
        void RegisterProvider(IMetricsProvider provider);
        void UnregisterProvider(IMetricsProvider provider);
    }

    public delegate void MetricsUpdatedEventHandler(IMetricsProvider sender, IEnumerable<IMetric> metrics);

    public interface IMetricsProvider
    {
        event MetricsUpdatedEventHandler MetricUpdated;
        string Name { get; }
        ColoredStates SummaryState { get; }
        IEnumerable<IMetric> Metrics { get; }
    }

    public interface IMetric
    {
        string Name { get; }
        ColoredStates State { get; }
    }

    public class Metric : IMetric
    {
        internal Action<IMetric> OnUpdateHandler;

        public string Name { get; set; }
        public ColoredStates State { get; set; }

        public Metric(string name)
        {
            Name = name;
            State = ColoredStates.Unknown;
        }

        protected void CallHandler()
        {
            if (null != OnUpdateHandler)
                OnUpdateHandler(this);
        }
    }

    public class GenericMetric<T> : Metric
    {
        private T value;

        public T Value 
        { 
            set
            {
                this.value = value;
                CallHandler();
            }
        }

        public GenericMetric(string name, T value)
            :base(name)
        {
            this.value = value;
        }

        public override string ToString()
        {
            return string.Concat(value);
        }
    }

    public class MetricsProvider : IMetricsProvider
    {
        public event MetricsUpdatedEventHandler MetricUpdated;

        private readonly ILogger logger;

        private bool initialized;

        private Dictionary<string, Metric> metrics;

        private List<IMetric> updateBatch = new List<IMetric>();

        public string Name { get; private set; }

        public IEnumerable<IMetric> Metrics
        {
            get 
            {
                if (!initialized)
                    throw new InvalidOperationException("This provider isn't yet initialized");

                return metrics.Values; 
            }
        }

        public ColoredStates SummaryState { get; set; }

        public MetricsProvider(ILogger logger)
        {
            if (null == logger)
                throw new ArgumentNullException("logger");

            this.logger = logger;

            SummaryState = ColoredStates.Unknown;
        }

        public void Initialize(params Metric[] metrics)
        {
            if (initialized)
                throw new InvalidOperationException("already initialized");

            if (null == metrics)
                throw new ArgumentNullException("metrics");

            this.metrics = metrics.ToDictionary(x => x.Name, x => x);

            foreach (var m in this.metrics.Values)
                m.OnUpdateHandler = MetricUpdatedHandler;

            initialized = true;
        }

        public void OpenBatch()
        {
            updateBatch.Clear();
        }

        public void CommitBatch()
        {
            var handler = MetricUpdated;

            if (null != handler)
            {
                try
                {
                    handler(this, updateBatch);
                }
                catch (Exception ex)
                {
                    logger.Log(this, ex);
                }
            }
        }

        private void MetricUpdatedHandler(IMetric source)
        {
            if (!updateBatch.Contains(source))
                updateBatch.Add(source);
        }
    }
}
