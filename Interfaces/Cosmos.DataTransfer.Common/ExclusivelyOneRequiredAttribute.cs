using System.ComponentModel.DataAnnotations;
using Cosmos.DataTransfer.Interfaces;

namespace Cosmos.DataTransfer.Common;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ExclusivelyOneRequiredAttribute : ClassValidatorAttribute 
{
    private string[] properties;
    public ExclusivelyOneRequiredAttribute(params string[] properties)
    {
        this.properties = properties;
    }

    /// <summary>
    /// Gets or sets a flag indicating whether the attribute should allow empty strings.
    /// </summary>
    public bool AllowEmptyStrings { get; set; }

    public override IEnumerable<ValidationResult> ValidateObject(object obj) 
    {
        // Validate the individual properties as in ValidationExtensions.GetValidationErrors,
        // and pick out any [Required] errors, that are matched by this.properties.

        var context = new ValidationContext(obj, serviceProvider: null, items: null);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(obj, context, results, true);
        //results = results.SkipWhile(x => x)
        // It ends here.
        throw new NotImplementedException();


        // And continue with testing the combination of all properties.
        // Note: It is *not* a requirement for the properties in 
        // ExclusivelyOneRequiredAttribute to be decorated with the RequiredAttribute.
        var t = obj.GetType();
        var nonblank = new List<string>();

        foreach (var propertyname in this.properties) {
            var propinfo = t.GetProperty(propertyname) ?? 
                throw new ArgumentException($"Property `{propertyname}` does not exist in {obj.GetType().Name}.", propertyname);
            var value = propinfo.GetValue(obj);

            if (value is null) {
                continue;
            }
            var stringValue = value as string;
            if (stringValue != null && !AllowEmptyStrings &&
                stringValue.Trim().Length != 0) {
                nonblank.Add(propertyname);
            }
        }

        if (nonblank.Count == 0) {
            results.Add(new ValidationResult(
                "None of the required properties were provided. " + 
                $"{obj.GetType().Name} requires exactly one of the properties: " + 
                string.Join(", ", this.properties), this.properties));
        } else if (nonblank.Count > 1) {
            results.Add(new ValidationResult(
                "Multiple properties were given where exactly one is required. " +
                "Offending properties (reduce to one):" +
                string.Join(", ", nonblank), nonblank));
        }
        
        return results;
    }
}
