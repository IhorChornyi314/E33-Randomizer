using System.ComponentModel.DataAnnotations;

namespace E33Randomizer.AotValidators;

public class AotHasValueAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext context)
    {
        if (value is null) return new ValidationResult("Value is Required");
        if (value is not string s) return new ValidationResult("Value is not string");


        if (s.Length == 0) return new ValidationResult("Value is Required");

        return ValidationResult.Success;
    }
}