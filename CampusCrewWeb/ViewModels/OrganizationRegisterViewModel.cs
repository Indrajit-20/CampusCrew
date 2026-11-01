using System.ComponentModel.DataAnnotations;

namespace CampusCrew.ViewModels;

public class OrganizationRegisterViewModel
{
    // SECTION 1 — ORGANIZATION INFORMATION
    [Required(ErrorMessage = "Organization or club name is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Organization name must be between 2 and 100 characters.")]
    [Display(Name = "Organization / Club Name")]
    public string OrganizationName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select organization type.")]
    [StringLength(50)]
    [Display(Name = "Organization Type")]
    public string OrganizationType { get; set; } = "Student Club";

    [Required(ErrorMessage = "Please enter college or university.")]
    [StringLength(120)]
    [Display(Name = "College / University")]
    public string College { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter city or location.")]
    [StringLength(80)]
    [Display(Name = "City / Location")]
    public string Location { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please provide a short description of the organization.")]
    [StringLength(800, MinimumLength = 10, ErrorMessage = "Description should be at least 10 characters.")]
    [Display(Name = "Organization Description")]
    public string Description { get; set; } = string.Empty;

    // SECTION 2 — CONTACT INFORMATION
    [Required(ErrorMessage = "Contact person name is required.")]
    [StringLength(80)]
    [Display(Name = "Contact Person")]
    public string ContactPerson { get; set; } = string.Empty;

    [Required(ErrorMessage = "Official email address is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid official email address.")]
    [StringLength(120)]
    [Display(Name = "Official Email")]
    public string OfficialEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Contact number is required.")]
    [Phone(ErrorMessage = "Please enter a valid contact phone number.")]
    [StringLength(20)]
    [Display(Name = "Contact Number")]
    public string ContactNumber { get; set; } = string.Empty;

    // SECTION 3 — ACCOUNT INFORMATION
    [Required(ErrorMessage = "Password is required.")]
    [DataType(DataType.Password)]
    [StringLength(40, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters.")]
    [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).+$", ErrorMessage = "Password must contain at least one letter and one number.")]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please confirm your password.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    [Display(Name = "Confirm Password")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
