using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IdeaKioskWeb.Models.Domain
{
    public class Idea
    {
        [Key]
        public int Id_idea { get; set; }  // Clave primaria para la idea

        // Clave foránea que se relaciona con la tabla "empleados"
        [ForeignKey("Empleado")]
        public string NumeroEmpleado { get; set; } = string.Empty;  // Clave foránea para el empleado

        public Empleado? Empleado { get; set; }  // Propiedad de navegación para acceder al empleado

        [Required]
        public string Descripcion { get; set; } = string.Empty;  // Descripción de la idea

        public string? ImagenRuta { get; set; }  // Ruta opcional de la imagen

        public string Estado { get; set; } = "En revisión";  // Estado de la idea (valor predeterminado)
        
        public string? Feedback { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;  // Fecha de creación


        public ICollection<Voto> Votos { get; set; } = new List<Voto>();  // Colección de votos para la idea
    }

    // Crea una clase ViewModel para almacenar la idea y el nombre del usuario
    public class IdeaViewModel
    {
        public Idea Idea { get; set; }
        public string UserName { get; set; }
    }

}