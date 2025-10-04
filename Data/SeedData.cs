using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Parcial_VargasMedina.Data;
using Parcial_VargasMedina.Models;

namespace Parcial_VargasMedina.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Asegurar que la base de datos esté creada
            await context.Database.EnsureCreatedAsync();

            // Crear rol Coordinador si no existe
            if (!await roleManager.RoleExistsAsync("Coordinador"))
            {
                await roleManager.CreateAsync(new IdentityRole("Coordinador"));
            }

            // Crear usuario coordinador si no existe
            var coordinadorEmail = "admin@portal.edu";
            var coordinador = await userManager.FindByEmailAsync(coordinadorEmail);
            
            if (coordinador == null)
            {
                coordinador = new IdentityUser
                {
                    UserName = coordinadorEmail,
                    Email = coordinadorEmail,
                    EmailConfirmed = true
                };
                
                await userManager.CreateAsync(coordinador, "Admin123!");
                await userManager.AddToRoleAsync(coordinador, "Coordinador");
            }

            // Crear cursos de ejemplo si no existen
            if (!context.Cursos.Any())
            {
                var cursos = new List<Curso>
                {
                    new Curso
                    {
                        Codigo = "PROG001",
                        Nombre = "Programación I",
                        Creditos = 4,
                        CupoMaximo = 30,
                        HorarioInicio = new TimeOnly(8, 0),
                        HorarioFin = new TimeOnly(10, 0),
                        Activo = true
                    },
                    new Curso
                    {
                        Codigo = "MAT001",
                        Nombre = "Matemáticas Discretas",
                        Creditos = 3,
                        CupoMaximo = 25,
                        HorarioInicio = new TimeOnly(10, 30),
                        HorarioFin = new TimeOnly(12, 0),
                        Activo = true
                    },
                    new Curso
                    {
                        Codigo = "BD001",
                        Nombre = "Bases de Datos",
                        Creditos = 4,
                        CupoMaximo = 20,
                        HorarioInicio = new TimeOnly(14, 0),
                        HorarioFin = new TimeOnly(16, 0),
                        Activo = true
                    },
                    new Curso
                    {
                        Codigo = "WEB001",
                        Nombre = "Desarrollo Web",
                        Creditos = 3,
                        CupoMaximo = 15,
                        HorarioInicio = new TimeOnly(16, 30),
                        HorarioFin = new TimeOnly(18, 0),
                        Activo = true
                    }
                };

                context.Cursos.AddRange(cursos);
                await context.SaveChangesAsync();
            }
        }
    }
}