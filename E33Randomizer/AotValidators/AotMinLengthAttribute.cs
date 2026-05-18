using System.ComponentModel.DataAnnotations;

namespace E33Randomizer.AotValidators;

public class AotMinLengthAttribute : ValidationAttribute
{
    public int Min { get; }
    public AotMinLengthAttribute(int min) => Min = min;

    protected override ValidationResult? IsValid(object? value, ValidationContext context)
    {
        if (value is null) return new ValidationResult("Value is null");
        if (value is not string s) return new ValidationResult("Value is not string");


        if (s.Length < Min)
        {
            return new ValidationResult($"Length must be at least {Min}.");
        }

        return ValidationResult.Success;
    }
}