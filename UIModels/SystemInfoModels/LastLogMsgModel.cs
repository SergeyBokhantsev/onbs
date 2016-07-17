using Interfaces;
using Interfaces.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIModels
{
    public class LastLogMsgModel : MultilineModel
    {
        public LastLogMsgModel(string viewName, IHostController hc, MappedPage pageDescriptor)
            : base(viewName, hc, pageDescriptor)
        {
            hc.Logger.LogEvent += LogEvent;

            Disposing += (s, e) => hc.Logger.LogEvent -= LogEvent;
        }

        void LogEvent(object caller, string message, LogLevels level)
        {
            if (level < LogLevels.Debug && null != caller)
                AddLine(string.Format("{0}| {1}", caller.GetType().Name, message));
        }
    }
}
