using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UniTodo.Modules.Auth.Models;

namespace UniTodo.Modules.Auth.DB
{
    public class AuthDbContext : IdentityDbContext<ApplicationUser>
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<RefreshToken>()
            .HasIndex(rt => rt.TokenHash)
.IsUnique();

            builder.Entity<RefreshToken>()
            .Property(rt => rt.IsRevoked)
            .IsConcurrencyToken();

        }
    }
}
