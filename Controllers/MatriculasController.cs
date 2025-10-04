using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parcial_VargasMedina.Data;
using Parcial_VargasMedina.Models;

namespace Parcial_VargasMedina.Controllers
{
    [Authorize]
    public class MatriculasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public MatriculasController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var matriculas = await _context.Matriculas
                .Include(m => m.Curso)
                .Where(m => m.UsuarioId == userId)
                .OrderByDescending(m => m.FechaRegistro)
                .ToListAsync();

            return View(matriculas);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Inscribirse(int cursoId)
        {
            var userId = _userManager.GetUserId(User);
            
            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "Debe estar autenticado para inscribirse.";
                return RedirectToAction("Detalle", "Cursos", new { id = cursoId });
            }

            var curso = await _context.Cursos
                .Include(c => c.Matriculas.Where(m => m.Estado != EstadoMatricula.Cancelada))
                .FirstOrDefaultAsync(c => c.Id == cursoId);

            if (curso == null || !curso.Activo)
            {
                TempData["Error"] = "El curso no existe o no está activo.";
                return RedirectToAction("Index", "Cursos");
            }

            // Validación 1: Verificar que no esté ya matriculado
            var matriculaExistente = await _context.Matriculas
                .AnyAsync(m => m.CursoId == cursoId && m.UsuarioId == userId && m.Estado != EstadoMatricula.Cancelada);

            if (matriculaExistente)
            {
                TempData["Error"] = "Ya está inscrito en este curso.";
                return RedirectToAction("Detalle", "Cursos", new { id = cursoId });
            }

            // Validación 2: Verificar cupo disponible
            var matriculasActivas = curso.Matriculas.Count(m => m.Estado != EstadoMatricula.Cancelada);
            if (matriculasActivas >= curso.CupoMaximo)
            {
                TempData["Error"] = "No hay cupo disponible para este curso.";
                return RedirectToAction("Detalle", "Cursos", new { id = cursoId });
            }

            // Validación 3: Verificar solapamiento de horarios
            var cursosMatriculados = await _context.Matriculas
                .Include(m => m.Curso)
                .Where(m => m.UsuarioId == userId && m.Estado != EstadoMatricula.Cancelada)
                .Select(m => m.Curso)
                .ToListAsync();

            var tieneConflictoHorario = cursosMatriculados.Any(c => 
                HorariosSeSuperponen(c.HorarioInicio, c.HorarioFin, curso.HorarioInicio, curso.HorarioFin));

            if (tieneConflictoHorario)
            {
                var cursoConflicto = cursosMatriculados.First(c => 
                    HorariosSeSuperponen(c.HorarioInicio, c.HorarioFin, curso.HorarioInicio, curso.HorarioFin));
                
                TempData["Error"] = $"El horario del curso se superpone con el curso '{cursoConflicto.Nombre}' ({cursoConflicto.HorarioInicio:HH:mm} - {cursoConflicto.HorarioFin:HH:mm}).";
                return RedirectToAction("Detalle", "Cursos", new { id = cursoId });
            }

            // Si todas las validaciones pasan, crear la matrícula
            var matricula = new Matricula
            {
                CursoId = cursoId,
                UsuarioId = userId,
                FechaRegistro = DateTime.Now,
                Estado = EstadoMatricula.Pendiente
            };

            _context.Matriculas.Add(matricula);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"¡Inscripción exitosa! Te has inscrito en el curso '{curso.Nombre}'. Tu matrícula está en estado pendiente.";
            
            return RedirectToAction("Detalle", "Cursos", new { id = cursoId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancelar(int id)
        {
            var userId = _userManager.GetUserId(User);
            var matricula = await _context.Matriculas
                .Include(m => m.Curso)
                .FirstOrDefaultAsync(m => m.Id == id && m.UsuarioId == userId);

            if (matricula == null)
            {
                TempData["Error"] = "Matrícula no encontrada.";
                return RedirectToAction("Index");
            }

            if (matricula.Estado == EstadoMatricula.Cancelada)
            {
                TempData["Error"] = "Esta matrícula ya está cancelada.";
                return RedirectToAction("Index");
            }

            matricula.Estado = EstadoMatricula.Cancelada;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Matrícula en '{matricula.Curso.Nombre}' cancelada exitosamente.";
            return RedirectToAction("Index");
        }

        private static bool HorariosSeSuperponen(TimeOnly inicio1, TimeOnly fin1, TimeOnly inicio2, TimeOnly fin2)
        {
            // Verifica si hay solapamiento entre dos rangos de horarios
            return inicio1 < fin2 && inicio2 < fin1;
        }
    }
}