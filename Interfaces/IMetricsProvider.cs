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

        event Action<ColoredStates> SummaryStateUpdated;

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
        where T:IComparable
    {
        private T value;

        public GenericMetric(string name, T value)
            :base(name)
        {
            this.value = value;
        }

        public void Set(ColoredStates state)
        {
            if (State != state)
            {
                State = state;
                CallHandler();
            }
        }

        public void Set(T value)
        {
            if (Comparer<T>.Default.Compare(this.value, value) != 0)
            {
                this.value = value;
                CallHandler();
            }
        }

        public void Set(T value, ColoredStates state)
        {
            if (State != state || Comparer<T>.Default.Compare(this.value, value) != 0)
            {
                this.value = value;
                State = state;
                CallHandler();
            }
        }

        public override string ToString()
        {
            return string.Concat(value);
        }
    }

    public class MetricsProvider : IMetricsProvider
    {
        public event MetricsUpdatedEventHandler MetricUpdated;
        
        public event Action<ColoredStates> SummaryStateUpdated;

        private readonly ILogger logger;

        private bool initialized;

        private Dictionary<string, Metric> metrics;
        private ColoredStates summaryState;

        private bool batchStarted;
        private readonly List<IMetric> updateBatch = new List<IMetric>();
        private readonly IMetric[] singleMetric = new IMetric[1];

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

        public ColoredStates SummaryState
        {
            get
            {
                return summaryState;
            }
            set
            {
                if (value != summaryState)
                {
                    summaryState = value;

                    var handler = SummaryStateUpdated;

                    if (null != handler)
                        handler(summaryState);
                }
            }
        }

        public MetricsProvider(ILogger logger, string name)
        {
            if (null == logger)
                throw new ArgumentNullException("logger");

            if (null == name)
                throw new ArgumentNullException("name");

            this.logger = logger;
            this.Name = name;

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
            batchStarted = true;
        }

        public void CommitBatch()
        {
            batchStarted = false;
            OnMetricsUpdated(updateBatch);
        }

        private void MetricUpdatedHandler(IMetric source)
        {
            if (batchStarted)
            {
                if (!updateBatch.Contains(source))
                    updateBatch.Add(source);
            }
            else
            {
                singleMetric[0] = source;
                OnMetricsUpdated(singleMetric);
            }
        }

        private void OnMetricsUpdated(IEnumerable<IMetric> metrics)
        {
            var handler = MetricUpdated;

            if (null != handler)
            {
                try
                {
                    handler(this, metrics);
                }
                catch (Exception ex)
                {
                    logger.Log(this, ex);
                }
            }
        }
    }
}
