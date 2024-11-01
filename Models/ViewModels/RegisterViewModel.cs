using System.ComponentModel.DataAnnotations;

public class RegisterViewModel
{
    [Required]
    [Display(Name = "Nombre de Usuario")]
    public string UserName { get; set; }

    [Required]
    [Display(Name = "Número de Empleado")]
    public string NumEmpleado { get; set; }  // Campo para el número de empleado

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Confirmar contraseña")]
    [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
    public string ConfirmPassword { get; set; }
}