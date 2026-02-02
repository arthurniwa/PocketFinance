using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace PocketFinance.Core
{
    public class AppDbContext : IdentityDbContext<IdentityUser>
    {
        public DbSet<Transacao> Transacoes { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite("Data Source=../financas.db");
        }
    }
}