using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoginSystem.Models;

namespace LoginSystem.Data
{
    public class Context : DbContext
    {
        public DbSet<User> Usuarios { get; set; }

        public Context(DbContextOptions<Context> options) : base(options)
        {
            this.Database.EnsureCreated();
        }

        /// <summary>
        /// Iniciar Configurações das tabelas
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>
            ( etd =>
                {
                    etd.ToTable("tbUser");
                    etd.HasKey(p => p.UserId).HasName("UserId");
                    etd.Property(p => p.UserId).HasColumnType("int").ValueGeneratedOnAdd();
                    etd.Property(p => p.Name).IsRequired().HasMaxLength(150);
                    etd.Property(p => p.Email).IsRequired().HasMaxLength(250);
                    etd.Property(p => p.Password).IsRequired().HasMaxLength(20);
                    etd.Property(p => p.BirthData).IsRequired();
                    etd.Property(p => p.Genre).IsRequired().HasMaxLength(1);
                    etd.Property(p => p.PhoneNumber).IsRequired().HasMaxLength(15);

                }
            );
        }
    }
}
