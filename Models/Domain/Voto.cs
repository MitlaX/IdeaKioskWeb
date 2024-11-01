using System.ComponentModel.DataAnnotations;

namespace IdeaKioskWeb.Models.Domain
{
    public class Voto
    {
        [Key]
        public int Id_voto { get; set; }  // No necesita inicialización porque es un entero
        public int IdeaId { get; set; }  // No necesita inicialización porque es un entero
        public Idea Idea { get; set; } = new Idea();  // Inicializa con una nueva idea

        public string NumeroEmpleado { get; set; } = string.Empty;  // Inicia como cadena vacía
        public Empleado Empleado { get; set; }
        public DateTime FechaVoto { get; set; } = DateTime.Now;  // Inicia con la fecha actual
    }
}