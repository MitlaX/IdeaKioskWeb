using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IdeaKioskWeb.Models.Domain
{
    public class ApplicationUser : IdentityUser
    {
        // Propiedad adicional para vincular con el empleado
        public string NumeroEmpleado { get; set; }
    }
       public class UserRolesViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public List<string> Roles { get; set; }
    }

    public class EditUserRolesViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string SelectedRole { get; set; }  // Almacena el rol seleccionado
        public List<SelectListItem> AllRoles { get; set; } = new List<SelectListItem>();
    }

    public class ProfileViewModel
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Area { get; set; }
        public string Telefono { get; set; }
        public List<Idea> Ideas { get; set; } // Lista de ideas del usuario
        public string Rol { get; set; } // Rol del usuario
        public List<string>? Feedbacks { get; set; } // Lista de comentarios
    }
}
