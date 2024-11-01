using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IdeaKioskWeb.Data;
using IdeaKioskWeb.Models.Domain;
using IdeaKioskWeb.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using OfficeOpenXml;
using System.Net.Mail;
using System.Net;
using System.Text;


[Authorize]  // Asegurar que solo los usuarios autenticados puedan acceder a estas acciones
public class IdeasController : Controller
{

    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly UserManager<ApplicationUser> _userManager;  // Declarar UserManager

    public IdeasController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
        _userManager = userManager;  // Inyectar UserManager
    }
    // Acción GET para mostrar la vista de registrar una idea
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Unauthorized("Usuario no autenticado.");
        }

        var empleadoExistente = await _context.Empleados
            .FirstOrDefaultAsync(e => e.NumeroEmpleado == user.NumeroEmpleado);

        if (empleadoExistente == null)
        {
            return NotFound("Empleado no encontrado.");
        }

        var model = new IdeaEmpleadoViewModel
        {
            Empleado = empleadoExistente,
            Idea = new Idea()
        };

        return View(model);
    }
    // Acción para mostrar la vista de registrar una idea
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(IdeaEmpleadoViewModel model, IFormFile? imagenArchivo)
    {

        if (ModelState.IsValid)
        {

            // Obtener el usuario autenticado
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized("Usuario no autenticado.");
            }

            // Obtener el empleado basado en el número de empleado del usuario autenticado
            var empleadoExistente = await _context.Empleados
                .FirstOrDefaultAsync(e => e.NumeroEmpleado == user.NumeroEmpleado);  // Obtener empleado por NumeroEmpleado


            // Asignar el número de empleado al campo NumeroEmpleado de la idea
            model.Idea.NumeroEmpleado = empleadoExistente.NumeroEmpleado;

            // Verificaer si el archivo se ha proporcionado y no está vacío
            //if (imagenArchivo.Length == 0)
            //{
            //    Console.WriteLine("Noeee se proporcionó un archivo o el archivo está vacío.");
            //    ModelState.AddModelError("ImagenRuta", "Debe proporcionar una imagen válida.");
            //    return View(model);
            //}


            //Si el archivo está presente, continuar con la lógica
            //Console.WriteLine($"Archivo recibido: {imagenArchivo.FileName}, tamaño: {imagenArchivo.Length}");


            // Si se subió una imagen, guardarla
            if (imagenArchivo != null && imagenArchivo.Length > 0)
            {
               

                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);  // Crear la carpeta si no existe
                }

                // Generar un nombre de archivo único
                var uniqueFileName = Path.GetFileNameWithoutExtension(imagenArchivo.FileName) + "_" + Path.GetRandomFileName() + Path.GetExtension(imagenArchivo.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                try
                {
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imagenArchivo.CopyToAsync(fileStream);
                    }

                    // Comprobar si el archivo se ha guardado correctamente
                    if (System.IO.File.Exists(filePath))
                    {
                        Console.WriteLine("Archivo guardado correctamente en: " + filePath);
                    }
                    else
                    {
                        Console.WriteLine("Error: el archivo no se guardó correctamente.");
                    }

                    // Asignar la ruta de la imagen al modelo de Idea
                    model.Idea.ImagenRuta = "/uploads/" + uniqueFileName;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error al guardar la imagen: " + ex.Message);
                }
            }
            else
            {
                Console.WriteLine("No se proporcionó un archivo o el archivo está vacío.");
            }

       
            

            // Guardar la idea en la base de datos
            _context.Ideas.Add(model.Idea);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }
        // Si el modelo no es válido, aquí puedes inspeccionar los errores en la consola del servidor
        foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
        {
            Console.WriteLine(error.ErrorMessage);  // Solo visible en la consola del servidor
        }

        return View(model);
    }

    // Acción para mostrar la vista de votación
    public async Task<IActionResult> Votar(string sortOrder, string filterMonth)
    {
        // Obtener todas las ideas y la cantidad de votos
        var ideas = await _context.Ideas
      .Include(i => i.Votos)
      .ToListAsync();

        // Filtrar por mes si se ha seleccionado
        if (!string.IsNullOrEmpty(filterMonth))
        {
            var selectedMonth = DateTime.Parse(filterMonth);
            ideas = ideas.Where(i => i.FechaCreacion.Month == selectedMonth.Month && i.FechaCreacion.Year == selectedMonth.Year).ToList();
        }

        // Ordenar según el tipo de orden seleccionado
        switch (sortOrder)
        {
            case "MostVoted":
                ideas = ideas.OrderByDescending(i => i.Votos.Count).ToList();
                break;
            case "MostRecent":
                ideas = ideas.OrderByDescending(i => i.FechaCreacion).ToList();
                break;
        }
        // Almacenar los valores en ViewBag para que se puedan usar en la vista
        ViewBag.SortOrder = sortOrder;
        ViewBag.FilterMonth = filterMonth;

        // Cargar los usuarios relacionados
        var userIds = ideas.Select(idea => idea.NumeroEmpleado).Distinct().ToList();
        var usuarios = await _context.Users
            .Where(u => userIds.Contains(u.NumeroEmpleado))
            .ToListAsync();

        // Crear un modelo que incluya las ideas y sus respectivos nombres de usuario
        var ideaViewModels = ideas.Select(idea => new IdeaViewModel
        {
            Idea = idea,
            UserName = usuarios.FirstOrDefault(u => u.NumeroEmpleado == idea.NumeroEmpleado)?.UserName ?? "Desconocido"
        }).ToList();

        return View(ideaViewModels);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddFeedback(int id, string feedback)
    {
        var idea = await _context.Ideas.FindAsync(id);
        if (idea == null)
        {
            return NotFound();
        }

        // Asignar el feedback de la idea
        idea.Feedback = feedback;

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Feedback agregado exitosamente.";
        return RedirectToAction(nameof(Admin));
    }

    // Acción para manejar el voto por una idea (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Votar(int id)
    {
        // Buscar la idea en la base de datos
        var idea = await _context.Ideas.Include(i => i.Votos).FirstOrDefaultAsync(i => i.Id_idea == id);
        if (idea == null)
        {
            return NotFound();  // Si no se encuentra la idea, retornar un error 404
        }

        // Obtener el usuario autenticado
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Unauthorized("Usuario no autenticado.");
        }

        // Obtener el empleado relacionado al usuario autenticado utilizando el NumeroEmpleado
        var empleado = await _context.Empleados.FirstOrDefaultAsync(e => e.NumeroEmpleado == user.NumeroEmpleado);

        if (empleado == null)
        {
            return BadRequest("El empleado no existe.");  // Si el empleado no existe, retornar un error
        }

        // Verificar si el empleado ya votó por esta idea
        var votoExistente = idea.Votos.FirstOrDefault(v => v.NumeroEmpleado == empleado.NumeroEmpleado);

        if (votoExistente != null)
        {
            // Si el voto ya existe, quitarlo
            idea.Votos.Remove(votoExistente);
            TempData["Message"] = "Tu voto ha sido retirado.";
        }
        else
        {
            // Si no existe, añadir un nuevo voto
            var nuevoVoto = new Voto
            {
                IdeaId = id,
                FechaVoto = DateTime.UtcNow,
                NumeroEmpleado = empleado.NumeroEmpleado  // Asignar el número de empleado válido
            };

            idea.Votos.Add(nuevoVoto);
            TempData["Message"] = "Tu voto ha sido registrado.";
        }

        // Guardar los cambios en la base de datos
        await _context.SaveChangesAsync();

        // Redirigir a la página de votación después de votar o quitar el voto
        return RedirectToAction(nameof(Votar));
    }


    [HttpGet]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Admin(string estado, string empleado, string descripcion, DateTime? fechaInicio, DateTime? fechaFin)
    {
        var ideasQuery = _context.Ideas.Include(i => i.Empleado).AsQueryable();

        // Filtrar por estado si está seleccionado
        if (!string.IsNullOrEmpty(estado))
        {
            ideasQuery = ideasQuery.Where(i => i.Estado == estado);
        }

        // Filtrar por empleado (nombre o apellido)
        if (!string.IsNullOrEmpty(empleado))
        {
            ideasQuery = ideasQuery.Where(i => i.Empleado.Nombre.Contains(empleado) || i.Empleado.Apellido.Contains(empleado));
        }

        // Filtrar por descripción (palabras clave)
        if (!string.IsNullOrEmpty(descripcion))
        {
            ideasQuery = ideasQuery.Where(i => i.Descripcion.Contains(descripcion));
        }

        // Filtrar por fecha de creación (rango)
        if (fechaInicio.HasValue)
        {
            DateTime fechaInicioUtc = fechaInicio.Value.ToUniversalTime();
            ideasQuery = ideasQuery.Where(i => i.FechaCreacion >= fechaInicioUtc);
        }
        if (fechaFin.HasValue)
        {
            DateTime fechaFinUtc = fechaFin.Value.ToUniversalTime();
            ideasQuery = ideasQuery.Where(i => i.FechaCreacion <= fechaFinUtc);
        }

        // Obtener los resultados filtrados
        var ideas = await ideasQuery.ToListAsync();

        return View(ideas);
    }
    // Acción para exportar las ideas a Excel
    [HttpGet]
    // Método para generar el archivo Excel y retornar un MemoryStream
    private async Task<MemoryStream> GenerateExcelReport(List<Idea> ideas)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        var stream = new MemoryStream();

        using (var package = new ExcelPackage(stream))
        {
            var worksheet = package.Workbook.Worksheets.Add("Ideas");
            worksheet.Cells["A1"].Value = "ID";
            worksheet.Cells["B1"].Value = "Descripción";
            worksheet.Cells["C1"].Value = "Estado";
            worksheet.Cells["D1"].Value = "Fecha de Creación";
            worksheet.Cells["E1"].Value = "Número de Empleado";

            int row = 2;
            foreach (var idea in ideas)
            {
                worksheet.Cells[row, 1].Value = idea.Id_idea;
                worksheet.Cells[row, 2].Value = idea.Descripcion;
                worksheet.Cells[row, 3].Value = idea.Estado;
                worksheet.Cells[row, 4].Value = idea.FechaCreacion.ToString("yyyy-MM-dd");
                worksheet.Cells[row, 5].Value = idea.NumeroEmpleado;
                row++;
            }

            worksheet.Cells.AutoFitColumns();
            await package.SaveAsync();
        }

        stream.Position = 0;
        return stream;
    }

    public async Task<IActionResult> ExportToExcel()
    {
        // Configurar el contexto de la licencia (No comercial)
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        var ideas = await _context.Ideas.ToListAsync();

        // Usar EPPlus para crear el archivo Excel
        using (var package = new ExcelPackage())
        {
            var worksheet = package.Workbook.Worksheets.Add("Ideas");
            worksheet.Cells["A1"].Value = "ID";
            worksheet.Cells["B1"].Value = "Descripción";
            worksheet.Cells["C1"].Value = "Estado";
            worksheet.Cells["D1"].Value = "Fecha de Creación";
            worksheet.Cells["E1"].Value = "Número de Empleado";

            int row = 2;
            foreach (var idea in ideas)
            {
                worksheet.Cells[row, 1].Value = idea.Id_idea;
                worksheet.Cells[row, 2].Value = idea.Descripcion;
                worksheet.Cells[row, 3].Value = idea.Estado;
                worksheet.Cells[row, 4].Value = idea.FechaCreacion.ToString("yyyy-MM-dd");
                worksheet.Cells[row, 5].Value = idea.NumeroEmpleado;
                row++;
            }

            worksheet.Cells.AutoFitColumns();

            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            string excelName = $"Ideas_{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx";

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarIdea(int id)
    {
        // Buscar la idea por ID
        var idea = await _context.Ideas.FindAsync(id);

        if (idea == null)
        {
            return NotFound();  // Si no se encuentra la idea, retornar un error 404
        }

        // Eliminar la idea
        _context.Ideas.Remove(idea);
        await _context.SaveChangesAsync();

        // Redirigir al menú de administración después de eliminar
        TempData["SuccessMessage"] = "La idea ha sido eliminada con éxito.";
        return RedirectToAction(nameof(Admin));
    }
    // Acción para cambiar el estado de una idea (Aprobada, Rechazada, En revisión)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstado(int id, string estado)
    {
        

        // Buscar la idea en la base de datos
        var idea = await _context.Ideas.FindAsync(id);
        if (idea == null)
        {
            return NotFound();  // Si no se encuentra la idea, retornar un error 404
        }
        
        // Actualizar el estado de la idea
        idea.Estado = estado;
        TempData["SuccessMessage"] = "El estado de la idea se ha actualizado correctamente.";
        await _context.SaveChangesAsync();

        // Redirigir a la página de administración después de actualizar el estado
        return RedirectToAction(nameof(Admin));
    }
    // Acción para enviar el reporte de ideas por correo
    [HttpPost]
    public async Task<IActionResult> SendReport()
    {
        Console.WriteLine("Método SendReport ha sido llamado.");
        try
        {
            // Obtener todas las ideas para el reporte
            var ideas = await _context.Ideas.ToListAsync();

            if (ideas == null || ideas.Count == 0)
            {
                return BadRequest("No hay ideas para enviar en el reporte.");
            }

            // Lógica para enviar el correo
            var reportContent = GenerateReportContent(ideas);
            var recipientEmail = "L20112177@itcj.edu.mx"; // Cambia esto por la dirección de correo del destinatario
            var subject = "Reporte de Ideas";

            Console.WriteLine("Preparando el envío del correo...");

            // Crear el archivo Excel
            var excelFileStream = await GenerateExcelReport(ideas);

            // Configurar el cliente SMTP
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("ideakioskweb@gmail.com", "bbgtjnqwvudhvzub"),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("ideakioskweb@gmail.com"),
                Subject = subject,
                Body = reportContent,
                IsBodyHtml = true,
            };

            mailMessage.To.Add(recipientEmail);
            mailMessage.Attachments.Add(new Attachment(excelFileStream, "Reporte_Ideas.xlsx"));

            // Enviar el correo
            await smtpClient.SendMailAsync(mailMessage);

            TempData["SuccessMessage"] = "Reporte enviado exitosamente.";
            return RedirectToAction(nameof(Admin));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al enviar el reporte: {ex.Message}");
            TempData["ErrorMessage"] = "Error al enviar el reporte."; // Establece un mensaje de error
            return RedirectToAction(nameof(Admin));
        }
    }



    private string GenerateReportContent(List<Idea> ideas)
    {
        var reportBuilder = new StringBuilder();
        reportBuilder.AppendLine("<h1>Reporte de Ideas</h1>");
        reportBuilder.AppendLine("<hr/>");
        reportBuilder.AppendLine($"<p>Total de ideas: {ideas.Count}</p>");

        reportBuilder.AppendLine("<p>Ideas más votadas:</p>");
        var topIdeas = ideas.OrderByDescending(i => i.Votos.Count).Take(3);
        foreach (var idea in topIdeas)
        {
            reportBuilder.AppendLine($"<p><strong>ID:</strong> {idea.Id_idea}, <strong>Descripción:</strong> {idea.Descripcion}, <strong>Estado:</strong> {idea.Estado}, <strong>Fecha:</strong> {idea.FechaCreacion}</p>");
        }

        return reportBuilder.ToString();
    }

}
