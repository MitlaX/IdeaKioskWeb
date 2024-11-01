using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using IdeaKioskWeb.Models;  // Importa los modelos necesarios
using System.Threading.Tasks;
using IdeaKioskWeb.Data;
using IdeaKioskWeb.Models.Domain;
using Microsoft.AspNetCore.Mvc.Rendering;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    public AccountController(ApplicationDbContext context, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _context = context;
        _signInManager = signInManager;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    // Mostrar formulario de Login
    //[HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    // Procesar Login
    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            // Usar SignInManager para autenticar al usuario
            var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, false);

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");  // Redirigir a Home si el login es exitoso
            }
            else if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "La cuenta está bloqueada.");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrectos.");
            }
        }

        return View(model);  // Si hay errores, volver a mostrar el formulario con los mensajes de error
    }

    // Mostrar formulario de Registro
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    // Procesar Registro (verifica que el empleado exista)
    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            // Verificar si el empleado existe en la tabla de empleados
            var empleadoExistente = await _context.Empleados
                .FirstOrDefaultAsync(e => e.NumeroEmpleado == model.NumEmpleado);

            if (empleadoExistente == null)
            {
                ModelState.AddModelError(string.Empty, "El número de empleado no existe.");
                return View(model);
            }

            // Crear un nuevo usuario usando ApplicationUser
            var user = new ApplicationUser
            {
                UserName = model.UserName,
                NumeroEmpleado = model.NumEmpleado, // Vincular el número de empleado al usuario
                Email = empleadoExistente.Email // Asignar el correo electrónico aquí
            };

            // Intenta crear el usuario usando UserManager
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Asignar rol "usuario" al nuevo usuario
                await _userManager.AddToRoleAsync(user, "usuario");
                // Si el usuario fue creado exitosamente, iniciar sesión y redirigir al Home
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            // Si hay errores al crear el usuario, agregarlos al ModelState
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        // Si el modelo no es válido o la creación falla, mostrar de nuevo la vista
        return View(model);
    }

    public async Task<IActionResult> Index()
    {
        // Obtener todos los usuarios
        var users = _userManager.Users.ToList();

        var userRolesViewModels = new List<UserRolesViewModel>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userRolesViewModels.Add(new UserRolesViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                Roles = roles.ToList()
            });
        }

        return View(userRolesViewModels); // Asegúrate de pasar la lista correcta a la vista
    }
    // Acción para editar roles de usuario

    [HttpGet]
    public async Task<IActionResult> Edit(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        var model = new EditUserRolesViewModel
        {
            UserId = user.Id,
            UserName = user.UserName,
            AllRoles = await _roleManager.Roles.Select(r => new SelectListItem
            {
                Value = r.Name,
                Text = r.Name
            }).ToListAsync(),
            SelectedRole = (await _userManager.GetRolesAsync(user)).FirstOrDefault()  // Obtener el rol actual del usuario
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(EditUserRolesViewModel model)
    {
        var user = await _userManager.FindByIdAsync(model.UserId);
        if (user == null)
        {
            return NotFound();
        }

        // Eliminar los roles actuales
        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);

        // Agregar el rol seleccionado
        if (!string.IsNullOrEmpty(model.SelectedRole))
        {
            await _userManager.AddToRoleAsync(user, model.SelectedRole);
        }

        return RedirectToAction("Index");
    }
    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        // Obtener el usuario autenticado
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return NotFound();
        }

        // Obtener el empleado asociado al usuario
        var empleado = await _context.Empleados.FirstOrDefaultAsync(e => e.NumeroEmpleado == user.NumeroEmpleado);

        if (empleado == null)
        {
            return NotFound();
        }
        // Obtener el rol del usuario
        var roles = await _userManager.GetRolesAsync(user);
        var rol = roles.FirstOrDefault() ?? "Sin rol";

        // Obtener las ideas asociadas al empleado
        var ideas = await _context.Ideas.Where(i => i.NumeroEmpleado == empleado.NumeroEmpleado).ToListAsync();

        // Crear un modelo que combine la información
        var profileViewModel = new ProfileViewModel
        {
            UserName = user.UserName,
            Email = user.Email,
            Nombre = empleado.Nombre,
            Apellido = empleado.Apellido,
            Area = empleado.Area,
            Telefono = empleado.Telefono,
            Ideas = ideas,
            Rol = rol // Agregar el rol al modelo
        };

        return View(profileViewModel);
    }

    // Procesar Logout
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
}
