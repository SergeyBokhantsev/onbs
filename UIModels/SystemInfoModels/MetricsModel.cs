using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces;

namespace UIModels.SystemInfoModels
{
    public class MetricsModel : MultilineModel
    {
        private readonly IMetricsProvider metricsProvider;

        private string template

        public MetricsModel(string viewName, IHostController hc, MappedPage pageDescriptor, object arg)
            :base(viewName, hc, pageDescriptor)
        {
            metricsProvider = arg as IMetricsProvider;

            if (null == metricsProvider)
                throw new ArgumentException("Argument is not of IMetricsProvider type");

            metricsProvider.Metrics

            metricsProvider.MetricUpdated += metricsProvider_MetricUpdated;
        }

        private void MetricUpdated(IMetricsProvider sender, IEnumerable<IMetric> metrics)
        {
            throw new NotImplementedException();
        }
    }
}
