using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using IdeaKioskWeb.Data;
using IdeaKioskWeb.Models.Domain;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        // Obtener el usuario autenticado
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            // Si el usuario no está autenticado, redirigir al login
            return RedirectToAction("Login", "Account");
        }

        // Asegúrate de que esta consulta use EF Core
        var empleado = await _context.Empleados.FirstOrDefaultAsync(e => e.Email == user.Email);

        if (empleado == null)
        {
            return NotFound("Empleado no encontrado.");
        }

        return View(empleado);
    }
}
