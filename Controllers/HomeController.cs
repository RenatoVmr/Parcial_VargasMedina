using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parcial_VargasMedina.Data;
using Parcial_VargasMedina.Models;

namespace Parcial_VargasMedina.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var viewModel = new HomeViewModel();
        
        // Obtener el último curso visitado desde la sesión
        var ultimoCursoSession = HttpContext.Session.GetString("UltimoCurso");
        if (!string.IsNullOrEmpty(ultimoCursoSession))
        {
            var partes = ultimoCursoSession.Split('|');
            if (partes.Length == 2 && int.TryParse(partes[0], out int cursoId))
            {
                var curso = await _context.Cursos
                    .FirstOrDefaultAsync(c => c.Id == cursoId && c.Activo);
                
                if (curso != null)
                {
                    viewModel.UltimoCursoVisitado = curso;
                }
            }
        }
        
        return View(viewModel);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

public class HomeViewModel
{
    public Curso? UltimoCursoVisitado { get; set; }
}
