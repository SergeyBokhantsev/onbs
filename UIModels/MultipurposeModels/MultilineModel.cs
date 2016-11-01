using Interfaces;
using Interfaces.UI;
using System.Collections.Concurrent;

namespace UIModels
{
    public class MultilineModel : ModelBase
    {
        private readonly ConcurrentQueue<string> lines_queue = new ConcurrentQueue<string>();

        public int FontSize
        {
            set
            {
                SetProperty("font_size", value);
            }
        }

        public MultilineModel(string viewName, IHostController hc, MappedPage pageDescriptor)
            : base(viewName, hc, pageDescriptor)
        {
            SetProperty("lines_queue", lines_queue);
            FontSize = 12000;
        }

        public void AddLine(string line)
        {
            lines_queue.Enqueue(line);
            OnPropertyChanged("lines_queue");
        }

        public void Clear()
        {
            SetProperty("clear", null);
        }
    }
}
