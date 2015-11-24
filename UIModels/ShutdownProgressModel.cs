using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces;
using Interfaces.UI;

namespace UIModels
{
    public class ShutdownProgressModel : ModelBase
    {
        private readonly ConcurrentQueue<string> lines_queue = new ConcurrentQueue<string>();

        public ShutdownProgressModel(string viewName, IHostController hc, MappedPage pageDescriptor)
            : base(viewName, hc, pageDescriptor)
        {
            SetProperty(ModelNames.PageTitle, "Shutdown in progress");
            SetProperty("lines_queue", lines_queue);
        }

        public void AddLine(string line)
        {
            lines_queue.Enqueue(line);
            OnPropertyChanged("lines_queue");
        }
    }
}
