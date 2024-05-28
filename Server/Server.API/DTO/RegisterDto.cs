using System.ComponentModel.DataAnnotations;

namespace Server.API.DTO;

using System.ComponentModel.DataAnnotations;

public class RegisterDto
{
    [Required(ErrorMessage = "Brugernavn er påkrævet.")]
    [StringLength(256, ErrorMessage = "Brugernavnet må ikke være længere end 256 tegn.")]
    [RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "Brugernavnet må kun indeholde bogstaver og tal.")]
    public string? UserName { get; set; }

    [Required(ErrorMessage = "E-mail er påkrævet.")]
    [EmailAddress(ErrorMessage = "Ugyldig e-mail adresse.")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Adgangskode er påkrævet.")]
    [MinLength(8, ErrorMessage = "Adgangskoden skal være mindst 8 tegn lang.")]
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
