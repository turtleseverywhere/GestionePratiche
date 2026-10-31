using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using x1_lavorazione_pratiche.DB.Entities.AuthEntities;

namespace x1_lavorazione_pratiche.DB.Contexts
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