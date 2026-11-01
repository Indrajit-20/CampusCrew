using System.ComponentModel.DataAnnotations;

namespace CampusCrew.ViewModels;

public class AdminLoginViewModel
{
    [Required(ErrorMessage = "Admin email address is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid administrator email address.")]
    [Display(Name = "Admin Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;
}
