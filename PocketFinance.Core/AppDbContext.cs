using Microsoft.EntityFrameworkCore;

namespace PocketFinance.Core
{
    public class AppDbContext : DbContext
    {
        public DbSet<Transacao> Transacao { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite("Data Source=financas.db");
        }
    }
}