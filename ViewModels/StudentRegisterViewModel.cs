using System.ComponentModel.DataAnnotations;

namespace CampusCrew.ViewModels;

public class StudentRegisterViewModel
{
    // SECTION 1 — ACCOUNT INFORMATION
    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(80, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 80 characters.")]
    [Display(Name = "Full Name")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter a valid email address.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [StringLength(120, ErrorMessage = "Email address cannot exceed 120 characters.")]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

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

    // SECTION 2 — ACADEMIC INFORMATION
    [Required(ErrorMessage = "Please select or enter your college/university.")]
    [StringLength(120, ErrorMessage = "College name cannot exceed 120 characters.")]
    [Display(Name = "College / University")]
    public string College { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter your course or degree.")]
    [StringLength(80, ErrorMessage = "Course name cannot exceed 80 characters.")]
    [Display(Name = "Course / Degree")]
    public string Course { get; set; } = string.Empty;

    [StringLength(80, ErrorMessage = "Department cannot exceed 80 characters.")]
    [Display(Name = "Department / Specialization")]
    public string Department { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select your academic year.")]
    [Range(1, 5, ErrorMessage = "Please select a valid academic year (1-5).")]
    [Display(Name = "Academic Year")]
    public int Year { get; set; } = 1;

    // SECTION 3 — CAMPUS / PROFILE INFORMATION
    [Required(ErrorMessage = "Please enter your city/location.")]
    [StringLength(80, ErrorMessage = "City/Location cannot exceed 80 characters.")]
    [Display(Name = "City / Location")]
    public string Location { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Bio cannot exceed 500 characters.")]
    [Display(Name = "Short Bio")]
    public string Bio { get; set; } = string.Empty;

    // SECTION 4 — SKILLS
    [Display(Name = "Select Skills")]
    public List<string> SelectedSkills { get; set; } = new();

    public List<string> AvailableSkills { get; set; } = new();
}
