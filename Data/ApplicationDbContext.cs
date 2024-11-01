#nullable disable

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using IdeaKioskWeb.Models.Domain;

namespace IdeaKioskWeb.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets para las tablas adicionales en la base de datos
        public DbSet<Empleado> Empleados { get; set; }
        public DbSet<Idea> Ideas { get; set; }
        public DbSet<Voto> Votos { get; set; }
        public DbSet<UserEmpleado> UserEmpleados { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Definir la clave primaria compuesta para la tabla UserEmpleado
            modelBuilder.Entity<UserEmpleado>()
                .HasKey(ue => new { ue.UserId, ue.NumeroEmpleado });

            // Definir las relaciones
            modelBuilder.Entity<UserEmpleado>()
                .HasOne(ue => ue.Usuario)
                .WithMany()  // Si no hay navegación inversa
                .HasForeignKey(ue => ue.UserId);

            modelBuilder.Entity<UserEmpleado>()
                .HasOne(ue => ue.Empleado)
                .WithMany()  // Si no hay navegación inversa
                .HasForeignKey(ue => ue.NumeroEmpleado);
        }

    }


}

#nullable restore