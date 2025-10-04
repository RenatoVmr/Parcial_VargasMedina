using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parcial_VargasMedina.Data;
using Parcial_VargasMedina.Models;
using Parcial_VargasMedina.Services;

namespace Parcial_VargasMedina.Controllers
{
    [Authorize(Roles = "Coordinador")]
    public class CoordinadorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICursosCacheService _cacheService;

        public CoordinadorController(ApplicationDbContext context, ICursosCacheService cacheService)
        {
            _context = context;
            _cacheService = cacheService;
        }

        // Dashboard principal del coordinador
        public async Task<IActionResult> Index()
        {
            var viewModel = new CoordinadorDashboardViewModel
            {
                TotalCursos = await _context.Cursos.CountAsync(),
                CursosActivos = await _context.Cursos.CountAsync(c => c.Activo),
                TotalMatriculas = await _context.Matriculas.CountAsync(),
                MatriculasAprobadas = await _context.Matriculas.CountAsync(m => m.Estado == EstadoMatricula.Confirmada)
            };

            return View(viewModel);
        }

        // CRUD de Cursos
        public async Task<IActionResult> Cursos()
        {
            var cursos = await _context.Cursos
                .Include(c => c.Matriculas)
                .OrderBy(c => c.Codigo)
                .ToListAsync();

            return View(cursos);
        }

        // Crear curso - GET
        public IActionResult CrearCurso()
        {
            return View();
        }

        // Crear curso - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearCurso(Curso curso)
        {
            if (ModelState.IsValid)
            {
                // Validar que no exista otro curso con el mismo código
                var cursoExistente = await _context.Cursos
                    .AnyAsync(c => c.Codigo == curso.Codigo);

                if (cursoExistente)
                {
                    ModelState.AddModelError("Codigo", "Ya existe un curso con este código.");
                    return View(curso);
                }

                // Validar horarios
                if (curso.HorarioInicio >= curso.HorarioFin)
                {
                    ModelState.AddModelError("HorarioFin", "El horario de fin debe ser posterior al horario de inicio.");
                    return View(curso);
                }

                _context.Add(curso);
                await _context.SaveChangesAsync();

                // Invalidar cache al agregar curso
                await _cacheService.InvalidarCacheAsync();

                TempData["SuccessMessage"] = "Curso creado exitosamente.";
                return RedirectToAction(nameof(Cursos));
            }
            return View(curso);
        }

        // Editar curso - GET
        public async Task<IActionResult> EditarCurso(int id)
        {
            var curso = await _context.Cursos.FindAsync(id);
            if (curso == null)
            {
                return NotFound();
            }
            return View(curso);
        }

        // Editar curso - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarCurso(int id, Curso curso)
        {
            if (id != curso.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Validar que no exista otro curso con el mismo código (excepto el actual)
                    var cursoExistente = await _context.Cursos
                        .AnyAsync(c => c.Codigo == curso.Codigo && c.Id != id);

                    if (cursoExistente)
                    {
                        ModelState.AddModelError("Codigo", "Ya existe otro curso con este código.");
                        return View(curso);
                    }

                    // Validar horarios
                    if (curso.HorarioInicio >= curso.HorarioFin)
                    {
                        ModelState.AddModelError("HorarioFin", "El horario de fin debe ser posterior al horario de inicio.");
                        return View(curso);
                    }

                    _context.Update(curso);
                    await _context.SaveChangesAsync();

                    // Invalidar cache al actualizar curso
                    await _cacheService.InvalidarCacheAsync();

                    TempData["SuccessMessage"] = "Curso actualizado exitosamente.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CursoExists(curso.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Cursos));
            }
            return View(curso);
        }

        // Eliminar curso - GET
        public async Task<IActionResult> EliminarCurso(int id)
        {
            var curso = await _context.Cursos
                .Include(c => c.Matriculas)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (curso == null)
            {
                return NotFound();
            }

            return View(curso);
        }

        // Eliminar curso - POST
        [HttpPost, ActionName("EliminarCurso")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarCursoConfirmado(int id)
        {
            var curso = await _context.Cursos
                .Include(c => c.Matriculas)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (curso != null)
            {
                // Verificar si tiene matrículas activas
                var tieneMatriculasActivas = curso.Matriculas
                    .Any(m => m.Estado == EstadoMatricula.Confirmada || m.Estado == EstadoMatricula.Pendiente);

                if (tieneMatriculasActivas)
                {
                    TempData["ErrorMessage"] = "No se puede eliminar el curso porque tiene matrículas activas.";
                    return RedirectToAction(nameof(Cursos));
                }

                _context.Cursos.Remove(curso);
                await _context.SaveChangesAsync();

                // Invalidar cache al eliminar curso
                await _cacheService.InvalidarCacheAsync();

                TempData["SuccessMessage"] = "Curso eliminado exitosamente.";
            }

            return RedirectToAction(nameof(Cursos));
        }

        // Gestión de Matrículas
        public async Task<IActionResult> Matriculas()
        {
            var matriculas = await _context.Matriculas
                .Include(m => m.Usuario)
                .Include(m => m.Curso)
                .OrderByDescending(m => m.FechaRegistro)
                .ToListAsync();

            return View(matriculas);
        }

        // Cambiar estado de matrícula
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstadoMatricula(int id, EstadoMatricula nuevoEstado)
        {
            var matricula = await _context.Matriculas.FindAsync(id);
            
            if (matricula == null)
            {
                return NotFound();
            }

            matricula.Estado = nuevoEstado;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Estado de matrícula cambiado a {nuevoEstado}.";
            return RedirectToAction(nameof(Matriculas));
        }

        private bool CursoExists(int id)
        {
            return _context.Cursos.Any(e => e.Id == id);
        }
    }

    public class CoordinadorDashboardViewModel
    {
        public int TotalCursos { get; set; }
        public int CursosActivos { get; set; }
        public int TotalMatriculas { get; set; }
        public int MatriculasAprobadas { get; set; }
    }
}