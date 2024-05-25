using System.ComponentModel.DataAnnotations;

namespace Server.API.DTO;

using System.ComponentModel.DataAnnotations;

public class RegisterDto
{
    [Required(ErrorMessage = "Username is required.")]
    [StringLength(256, ErrorMessage = "Username cannot be longer than 256 characters.")]
    [RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "Username can only contain letters and numbers.")]
    public string? UserName { get; set; }

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Email is not valid.")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Password is required.")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
    [PasswordRequirements]
    public string? Password { get; set; }
}

public class PasswordRequirementsAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value is null || string.IsNullOrWhiteSpace(value.ToString()))
        {
            return new ValidationResult("Password is required.");
        }

        var password = value.ToString();
        var errors = new List<string>();

        if (!password.Any(char.IsDigit))
        {
            errors.Add("Password must contain at least one digit.");
        }
        
        if (!password.Any(char.IsLower))
        {
            errors.Add("Password must contain at least one lowercase letter.");
        }
        
        if (!password.Any(char.IsUpper))
        {
            errors.Add("Password must contain at least one uppercase letter.");
        }
        
        /*if (!password.Any(ch => !char.IsLetterOrDigit(ch)))
        {
            errors.Add("Password must contain at least one non-alphanumeric character.");
        }*/

        if (errors.Count > 0)
        {
            return new ValidationResult(string.Join(" ", errors));
        }

        return ValidationResult.Success;
    }
}
