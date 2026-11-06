using Microsoft.EntityFrameworkCore;
using x1_lavorazione_pratiche.DB.Entities;

namespace x1_lavorazione_pratiche.DB.Contexts
{
    public class DBContext : DbContext
    {
        public DBContext(DbContextOptions<DBContext> options)
            : base(options)
        {
        }

        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Pratica> Pratiche { get; set; }
        public DbSet<PraticaHistory> PraticheHistory { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}