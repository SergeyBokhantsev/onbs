using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApp.Models
{
    internal static class Extensions
    {
        public static void AssertUserKey(this ONBSContext db, string key)
        {
            var user = db.Users.Where(u => u.Key == key).FirstOrDefault();

            if (user == null)
                throw new NotAuthorizedException();
        }

        async public static Task AssertUserKeyAsync(this ONBSContext db, string key)
        {
            await Task.Run(() => db.AssertUserKey(key));
        }

        public static Travel GetTravel(this ONBSContext db, int id)
        {
            var travel = db.Travels.Where(u => u.ID == id).FirstOrDefault();

            if (travel == null)
                throw new TravelNotFoundException(id);

            return travel;
        }
    }
}
