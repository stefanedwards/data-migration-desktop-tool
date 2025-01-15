using System.ComponentModel.DataAnnotations;

namespace Cosmos.DataTransfer.Interfaces;
public abstract class ClassValidatorAttribute : Attribute 
{ 
    public virtual IEnumerable<ValidationResult> ValidateObject(object obj) {
        throw new NotImplementedException();
    }
}
