using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel;

namespace PocketFinance.Core
{
    public class AppDbContext : IdentityDbContext<IdentityUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Transacao> Transacoes { get ; set ; } = null!;

        public DbSet<Meta> Metas { get ; set ; }

        public DbSet<Conta> Contas { get ; set ; } = null!;
    }
}