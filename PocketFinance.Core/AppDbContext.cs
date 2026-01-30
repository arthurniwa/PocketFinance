using Microsoft.EntityFrameworkCore;

namespace PocketFinance.Core
{
    public class AppDbContext : DbContext
    {
        public DbSet<Transacao> Transacoes { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite("Data Source=../financas.db");
        }
    }
}