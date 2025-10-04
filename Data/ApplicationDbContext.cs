using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Parcial_VargasMedina.Models;

namespace Parcial_VargasMedina.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Curso> Cursos { get; set; }
        public DbSet<Matricula> Matriculas { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configurar restricciones para Curso
            builder.Entity<Curso>(entity =>
            {
                entity.HasIndex(c => c.Codigo).IsUnique();
                entity.Property(c => c.Codigo).IsRequired().HasMaxLength(10);
                entity.Property(c => c.Nombre).IsRequired().HasMaxLength(100);
            });

            // Configurar restricciones para Matricula
            builder.Entity<Matricula>(entity =>
            {
                // Un usuario no puede estar matriculado más de una vez en el mismo curso
                entity.HasIndex(m => new { m.CursoId, m.UsuarioId }).IsUnique()
                    .HasFilter("Estado != 2"); // No aplicar a canceladas

                entity.HasOne(m => m.Curso)
                    .WithMany(c => c.Matriculas)
                    .HasForeignKey(m => m.CursoId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(m => m.Usuario)
                    .WithMany()
                    .HasForeignKey(m => m.UsuarioId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}