using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parcial_VargasMedina.Data;
using Parcial_VargasMedina.Models;
using Parcial_VargasMedina.Services;
using System.ComponentModel.DataAnnotations;

namespace Parcial_VargasMedina.Controllers
{
    public class CursosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICursosCacheService _cacheService;

        public CursosController(ApplicationDbContext context, ICursosCacheService cacheService)
        {
            _context = context;
            _cacheService = cacheService;
        }

        public async Task<IActionResult> Index(CursoFilterViewModel? filtros)
        {
            List<Curso> cursosBase;

            // Si no hay filtros, intentar obtener del cache
            if (filtros == null || EsFiltroVacio(filtros))
            {
                // Intentar obtener del cache
                var cursosFromCache = await _cacheService.GetCursosActivosAsync();

                if (cursosFromCache == null)
                {
                    // No está en cache, obtener de BD y cachear
                    cursosBase = await _context.Cursos
                        .Where(c => c.Activo)
                        .Include(c => c.Matriculas.Where(m => m.Estado != EstadoMatricula.Cancelada))
                        .ToListAsync();

                    await _cacheService.SetCursosActivosAsync(cursosBase);
                    ViewBag.FromCache = false;
                }
                else
                {
                    cursosBase = cursosFromCache;
                    ViewBag.FromCache = true;
                }
            }
            else
            {
                // Hay filtros, obtener directamente de BD
                var query = _context.Cursos.Where(c => c.Activo);

                // Validaciones server-side
                // Validar rango de créditos
                if (filtros.CreditosMin.HasValue && filtros.CreditosMax.HasValue && 
                    filtros.CreditosMin.Value > filtros.CreditosMax.Value)
                {
                    ModelState.AddModelError("CreditosRange", "Los créditos máximos deben ser mayores que los créditos mínimos.");
                }

                // Validar rango de horarios
                if (filtros.HorarioInicio.HasValue && filtros.HorarioFin.HasValue && 
                    filtros.HorarioInicio.Value >= filtros.HorarioFin.Value)
                {
                    ModelState.AddModelError("HorarioRange", "El horario de fin debe ser posterior al horario de inicio.");
                }

                // Solo aplicar filtros si no hay errores de validación
                if (ModelState.IsValid)
                {
                    if (!string.IsNullOrWhiteSpace(filtros.Nombre))
                    {
                        query = query.Where(c => c.Nombre.Contains(filtros.Nombre) || c.Codigo.Contains(filtros.Nombre));
                    }

                    if (filtros.CreditosMin.HasValue)
                    {
                        query = query.Where(c => c.Creditos >= filtros.CreditosMin.Value);
                    }

                    if (filtros.CreditosMax.HasValue)
                    {
                        query = query.Where(c => c.Creditos <= filtros.CreditosMax.Value);
                    }

                    if (filtros.HorarioInicio.HasValue)
                    {
                        query = query.Where(c => c.HorarioInicio >= filtros.HorarioInicio.Value);
                    }

                    if (filtros.HorarioFin.HasValue)
                    {
                        query = query.Where(c => c.HorarioFin <= filtros.HorarioFin.Value);
                    }
                }

                cursosBase = await query
                    .Include(c => c.Matriculas.Where(m => m.Estado != EstadoMatricula.Cancelada))
                    .ToListAsync();
            }

            var viewModel = new CatalogoCursosViewModel
            {
                Cursos = cursosBase,
                Filtros = filtros ?? new CursoFilterViewModel()
            };

            return View(viewModel);
        }

        private static bool EsFiltroVacio(CursoFilterViewModel filtros)
        {
            return string.IsNullOrWhiteSpace(filtros.Nombre) &&
                   !filtros.CreditosMin.HasValue &&
                   !filtros.CreditosMax.HasValue &&
                   !filtros.HorarioInicio.HasValue &&
                   !filtros.HorarioFin.HasValue;
        }

        public async Task<IActionResult> Detalle(int id)
        {
            var curso = await _context.Cursos
                .Include(c => c.Matriculas.Where(m => m.Estado != EstadoMatricula.Cancelada))
                .ThenInclude(m => m.Usuario)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (curso == null || !curso.Activo)
            {
                return NotFound();
            }

            // Guardar en sesión el último curso visitado
            HttpContext.Session.SetString("UltimoCurso", $"{curso.Id}|{curso.Nombre}");

            return View(curso);
        }
    }

    public class CatalogoCursosViewModel
    {
        public List<Curso> Cursos { get; set; } = new();
        public CursoFilterViewModel Filtros { get; set; } = new();
    }

    public class CursoFilterViewModel
    {
        [Display(Name = "Buscar por nombre o código")]
        public string? Nombre { get; set; }

        [Display(Name = "Créditos mínimos")]
        [Range(1, int.MaxValue, ErrorMessage = "Los créditos mínimos deben ser mayor que 0")]
        public int? CreditosMin { get; set; }

        [Display(Name = "Créditos máximos")]
        [Range(1, int.MaxValue, ErrorMessage = "Los créditos máximos deben ser mayor que 0")]
        public int? CreditosMax { get; set; }

        [Display(Name = "Horario desde")]
        [DataType(DataType.Time)]
        public TimeOnly? HorarioInicio { get; set; }

        [Display(Name = "Horario hasta")]
        [DataType(DataType.Time)]
        public TimeOnly? HorarioFin { get; set; }
    }
}