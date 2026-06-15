using System.ComponentModel.DataAnnotations;

namespace E33Randomizer.AotValidators;

public class AotMinLengthAttribute : ValidationAttribute
{
    public int Min { get; }
    public AotMinLengthAttribute(int min) => Min = min;

    protected override ValidationResult? IsValid(object? value, ValidationContext context)
    {
        if (value is null) return new ValidationResult(ResourceHelper.GetString(nameof(Assets.Resources.Validation_Null)));
        if (value is not string s) return new ValidationResult(ResourceHelper.GetString(nameof(Assets.Resources.Validation_NotString)));


        if (s.Length < Min)
        {
            return new ValidationResult(ResourceHelper.GetStringFormatted(nameof(Assets.Resources.Validation_MinLength_Length), Min));
        }

        return ValidationResult.Success;
    }
}