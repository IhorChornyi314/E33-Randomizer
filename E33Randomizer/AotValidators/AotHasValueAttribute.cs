using System.ComponentModel.DataAnnotations;

namespace E33Randomizer.AotValidators;

public class AotHasValueAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext context)
    {
        if (value is null) return new ValidationResult(ResourceHelper.GetString(nameof(Assets.Resources.Validation_Required)));
        if (value is not string s) return new ValidationResult(ResourceHelper.GetString(nameof(Assets.Resources.Validation_Required)));


        if (s.Length == 0) return new ValidationResult(ResourceHelper.GetString(nameof(Assets.Resources.Validation_Required)));

        return ValidationResult.Success;
    }
}