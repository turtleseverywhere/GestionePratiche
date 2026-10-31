using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using sistema_gestione_pratiche.DB.Entities.AuthEntities;

namespace sistema_gestione_pratiche.DB.Contexts
{
    public class AuthDBContext : IdentityDbContext<User, Role, string>
    {
        public AuthDBContext(DbContextOptions<AuthDBContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}