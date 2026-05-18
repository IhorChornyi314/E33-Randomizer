using System.ComponentModel.DataAnnotations;

namespace E33Randomizer.AotValidators;

public class AotSiblingNotExistsValidatorAttribute<TKey> : ValidationAttribute
{
    
    protected override ValidationResult? IsValid(object? value, ValidationContext context)
    {
        if (value is null) return new ValidationResult("Value is null");
        if (value is not TKey v) return new ValidationResult("Value is not string");
        if (context.ObjectInstance is not ISiblingCheck<TKey> c) return new ValidationResult("Object is not ISiblingCheck<TKey>");

        return !c.CheckFunc(v) ? ValidationResult.Success : new ValidationResult("Value already exists");
    }
}

public interface ISiblingCheck<in TKey>
{
    public Func<TKey, bool> CheckFunc { get; }
}