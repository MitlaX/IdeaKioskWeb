using IdeaKioskWeb.Models.Domain;

namespace IdeaKioskWeb.Models.ViewModels
{
    public class IdeaEmpleadoViewModel
    {
        public Empleado Empleado { get; set; } = new Empleado();
        public Idea Idea { get; set; } = new Idea();
    }
}