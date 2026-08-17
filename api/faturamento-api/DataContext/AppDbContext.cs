using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using faturamento_api.Models;
using Microsoft.EntityFrameworkCore;

namespace faturamento_api.DataContext
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<NotaFiscal> NotasFiscais { get; set; }
        public DbSet<ItemNotaFiscal> ItensNotasFiscais { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<NotaFiscal>()
                .Property(notaFiscal => notaFiscal.Numero)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<NotaFiscal>()
                .HasIndex(notaFiscal => notaFiscal.Numero)
                .IsUnique();
        }
    }
}
