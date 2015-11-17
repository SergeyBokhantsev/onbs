using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using WebApp.Models;

namespace TravelViewer
{
    internal class Client : IDisposable
    {
        private ONBSContext db;

        public Client(string connectionString)
        {
            db = new ONBSContext(connectionString);
        }

        async public Task<IEnumerable<Tuple<int, string>>> GetTravelInfos()
        {
            return await Task.Run<IEnumerable<Tuple<int, string>>>(() =>
                {
                    var inf = (from travel in db.Travels select new { ID = travel.ID, NAME = travel.Name }).ToArray();
                    return inf.Select(i => new Tuple<int, string>(i.ID, i.NAME));
                });
        }

        async public Task<Travel> GetTravel(int id)
        {
            return await Task.Run<Travel>(() => db.Travels.Find(id));
        }

        public void ReloadEntity<TEntity>(TEntity entity)
        where TEntity : class
        {
            db.Entry(entity).Reload();
        }

        public void ReloadNavigationProperty<TEntity, TElement>(
        TEntity entity,
        Expression<Func<TEntity, ICollection<TElement>>> navigationProperty)
            where TEntity : class
            where TElement : class
        {
            db.Entry(entity).Collection<TElement>(navigationProperty).Query();
        }

        public void Dispose()
        {
            db.Dispose();
        }
    }
}
