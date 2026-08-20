using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using estoque_api.Models;
using Microsoft.EntityFrameworkCore;

namespace estoque_api.DataContext
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Produto> Produtos { get; set; }
        public DbSet<EventoProcessado> EventosProcessados {get; set;}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<EventoProcessado>()
                .HasKey(e => new { e.EventoId, e.EventoType });
        }
    }
}