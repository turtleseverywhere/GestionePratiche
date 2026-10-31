using Microsoft.EntityFrameworkCore;
using sistema_gestione_pratiche.DB.Entities;

namespace sistema_gestione_pratiche.DB.Contexts
{
    public class DBContext : DbContext
    {
        public DBContext(DbContextOptions<DBContext> options)
            : base(options)
        {
        }

        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Pratica> Pratiche { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}