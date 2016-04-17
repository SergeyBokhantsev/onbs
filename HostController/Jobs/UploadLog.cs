using Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelsClient;

namespace HostController.Jobs
{
    internal class UploadLog
    {
        public UploadLog(IHostController hc, OnlineLogger onlineLogger)
        {
            Ensure.ArgumentIsNotNull(hc);
            Ensure.ArgumentIsNotNull(onlineLogger);
            
            hc.CreateTimer(60000, ht => onlineLogger.Upload(false), true, false, "online logger timer");
        }
    }
}
