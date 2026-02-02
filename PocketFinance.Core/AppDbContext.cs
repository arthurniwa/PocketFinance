using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace PocketFinance.Core
{
    public class AppDbContext : IdentityDbContext<IdentityUser>
    {
        public DbSet<Transacao> Transacoes { get; set; } = null!;

        public DbSet<Meta> Metas { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite("Data Source=../PocketFinance.Web/financas.db");
        }
    }
}