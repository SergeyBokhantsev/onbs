using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class ONBSContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Travel> Travels { get; set; }
        public DbSet<TravelPoint> TravelPoints { get; set; }
        public DbSet<LogEntry> GeneralLogs { get; set; }

        public ONBSContext()
            : base(System.Configuration.ConfigurationManager.AppSettings["DBConnectionString"])
        {
        }

        public ONBSContext(string connectionString)
            : base(connectionString)
        {
        }
    }
}