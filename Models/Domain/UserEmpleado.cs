using System.ComponentModel.DataAnnotations.Schema;

namespace IdeaKioskWeb.Models.Domain
{
    public class UserEmpleado
    {

        // Clave foránea que apunta a AspNetUsers
        public string UserId { get; set; } // Clave primaria parte 1

        // Clave foránea que apunta a Empleados
        public string NumeroEmpleado { get; set; } // Clave primaria parte 2

        // Relación con ApplicationUser (AspNetUsers)
        public ApplicationUser Usuario { get; set; }

        // Relación con Empleado
        public Empleado Empleado { get; set; }
    }
}
