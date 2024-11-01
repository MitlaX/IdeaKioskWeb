using System.ComponentModel.DataAnnotations;

namespace IdeaKioskWeb.Models.Domain
{
    public class Empleado
    {
        [Key]
        public string NumeroEmpleado { get; set; }

        [Required]
        public string Nombre { get; set; }

        [Required]
        public string Apellido { get; set; }

        [Required]
        public string Area { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public string Telefono { get; set; }

        // Relación de uno a muchos (un empleado puede tener muchas ideas)
        public ICollection<Idea> Ideas { get; set; } = new List<Idea>();
        // Relación de uno a muchos (un empleado puede emitir muchos votos)
        public ICollection<Voto> Votos { get; set; } = new List<Voto>();
    }
}