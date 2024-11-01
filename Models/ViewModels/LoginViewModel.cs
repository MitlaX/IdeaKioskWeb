using System.ComponentModel.DataAnnotations;

public class LoginViewModel
{
    [Required]
    public string UserName { get; set; }  // Cambiado a UserName en lugar de Email

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [Display(Name = "Recordarme")]
    public bool RememberMe { get; set; }
}