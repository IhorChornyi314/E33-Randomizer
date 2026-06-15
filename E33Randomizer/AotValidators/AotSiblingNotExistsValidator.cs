using System.ComponentModel.DataAnnotations;

namespace E33Randomizer.AotValidators;

public class AotSiblingNotExistsValidatorAttribute<TKey> : ValidationAttribute
{
    
    protected override ValidationResult? IsValid(object? value, ValidationContext context)
    {
        if (value is null) return new ValidationResult(ResourceHelper.GetString(nameof(Assets.Resources.Validation_Null)));
        if (value is not TKey v) return new ValidationResult(ResourceHelper.GetString(nameof(Assets.Resources.Validation_NotString)));
        if (context.ObjectInstance is not ISiblingCheck<TKey> c) 
            return new ValidationResult(ResourceHelper.GetString(nameof(Assets.Resources.Validation_Sibling_NotISibling)));

        return !c.CheckFunc(v) 
            ? ValidationResult.Success 
            : new ValidationResult(ResourceHelper.GetString(nameof(Assets.Resources.Validation_Sibling_AlreadyExists)));
    }
}

public interface ISiblingCheck<in TKey>
{
    public Func<TKey, bool> CheckFunc { get; }
}