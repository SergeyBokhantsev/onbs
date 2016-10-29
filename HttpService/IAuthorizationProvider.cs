using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HttpServiceNamespace
{
    public interface IAuthorizationProvider
    {
        Task Authorize(HttpWebRequest request);
    }
}
