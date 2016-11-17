using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces;
using Interfaces.UI;
using Interfaces.UI.Models;

namespace UIModels
{
    public class MetricsModel : ModelBase
    {
        private readonly IMetricsProvider metricsProvider;

        private readonly Dictionary<IMetric, int> indexMap = new Dictionary<IMetric, int>();

        private readonly TextGrigDataModel grid;

        public MetricsModel(string viewName, IHostController hc, MappedPage pageDescriptor, object arg)
            :base(viewName, hc, pageDescriptor)
        {
            metricsProvider = arg as IMetricsProvider;

            if (null == metricsProvider)
                throw new ArgumentException("Argument is not of IMetricsProvider type");

            metricsProvider.MetricUpdated += MetricUpdated;
            metricsProvider.SummaryStateUpdated += UpdateTitle;

            UpdateTitle(metricsProvider.SummaryState);

            grid = new TextGrigDataModel("Metric", "Value", "State");

            int index = 0;
            foreach(var m in metricsProvider.Metrics)
            {
                grid.AddRow(m.Name, m.ToString(), GetStateText(m.State));
                indexMap.Add(m, index++);
            }

            SetProperty("grid", grid);
        }

        private void UpdateTitle(ColoredStates state)
        {
            SetProperty(ModelNames.PageTitle, string.Concat(metricsProvider.Name, " - ", GetStateText(state)));
        }

        private void MetricUpdated(IMetricsProvider sender, IEnumerable<IMetric> metrics)
        {
            foreach (var m in metrics)
                grid.Set(indexMap[m], 1, m.ToString());
        }

        private string GetStateText(ColoredStates state)
        {
            switch (state)
            {
                case ColoredStates.Normal:
                    return "OK";
                case ColoredStates.Red:
                    return "ERROR";
                case ColoredStates.Yellow:
                    return "WARN";
                case ColoredStates.Blue:
                    return "Blue";
                case ColoredStates.DarkGrey:
                    return "Dark Grey";
                case ColoredStates.Green:
                    return "Green";
                case ColoredStates.Grey:
                    return "Grey";
                case ColoredStates.Unknown:
                    return "Unknown state";
                default:
                    return state.ToString();
            }
        }
    }
}
