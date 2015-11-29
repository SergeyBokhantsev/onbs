using Interfaces;
using Interfaces.UI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIModels
{
    public class MultilineModel : ModelBase
    {
        private readonly ConcurrentQueue<string> lines_queue = new ConcurrentQueue<string>();

        public MultilineModel(string viewName, IHostController hc, MappedPage pageDescriptor)
            : base(viewName, hc, pageDescriptor)
        {
            SetProperty("lines_queue", lines_queue);
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
