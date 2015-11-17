using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using WebApp.Models;

namespace WebApp.Controllers
{
    public abstract class ONBSApiControllerBase : ApiController
    {
        protected ONBSContext db = new ONBSContext();

        protected HttpResponseMessage Monitor(string key, Func<HttpResponseMessage> action)
        {
            try
            {
                db.AssertUserKey(key);
                return action();
            }
            catch (NotAuthorizedException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Forbidden, ex);
            }
            catch (TravelNotFoundException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }
            catch (LogEntryNotFoundException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
