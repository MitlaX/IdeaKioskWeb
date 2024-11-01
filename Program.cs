using IdeaKioskWeb.Data;
using IdeaKioskWeb.Models.Domain; // Importar el modelo ApplicationUser
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Configurar el ApplicationDbContext con PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configurar Identity para usar ApplicationUser y ApplicationDbContext
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()  // Cambiar IdentityUser a ApplicationUser
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();


// Configurar las cookies de autenticación
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;  // Cookies solo accesibles por HTTP, mayor seguridad
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);  // Duración de la cookie
    options.LoginPath = "/Account/Login";  // Ruta para el login
    options.AccessDeniedPath = "/Account/AccessDenied";  // Ruta para acceso denegado
    options.SlidingExpiration = true;  // Renovar la cookie si el usuario navega
});

builder.Services.AddControllersWithViews();  // Añadir soporte para controladores y vistas



var app = builder.Build();

// Middleware
app.UseHttpsRedirection();  // Redirección a HTTPS
app.UseStaticFiles();  // Permite servir archivos estáticos

app.UseRouting();

app.UseAuthentication();  // Habilitar autenticación
app.UseAuthorization();   // Habilitar autorización

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
