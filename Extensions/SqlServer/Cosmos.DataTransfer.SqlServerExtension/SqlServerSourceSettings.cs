using System.ComponentModel.DataAnnotations;
using Cosmos.DataTransfer.Interfaces;
using Cosmos.DataTransfer.Interfaces.Manifest;

namespace Cosmos.DataTransfer.SqlServerExtension
{
    public class SqlServerSourceSettings : IDataExtensionSettings
    {
        [Required]
        [SensitiveValue]
        public string? ConnectionString { get; set; }

        public string? QueryText { get; set; }

        public string? FilePath { get; set; }

        // Provides a class based validation on the combination of both `QueryText`
        // and `FilePath`, as either one is required -- but both not allowed.
        // TODO: This could benefit from a class attribute or similar, to provide
        //   a custom validation instead of overloading the extension method's
        //   and replicating it.
        public void Validate() {
            var validationErrors = this.GetValidationErrors().ToList();
            var validation = this.validate();
            if (validation != null) {
                validationErrors.Add(validation);
            }

            if (validationErrors.Any())
            {
                throw new AggregateException($"Configuration for {this.GetType().Name} is invalid", validationErrors.Select(s => new Exception(s)));
            }
        }

        private string? validate() {
            if (String.IsNullOrWhiteSpace(this.QueryText) &&
                String.IsNullOrWhiteSpace(this.FilePath)) {
                return "Either `QueryText` or `FilePath` are required!";
            } else if (String.IsNullOrWhiteSpace(this.QueryText) == false &&
                String.IsNullOrWhiteSpace(this.FilePath) == false) {
                return "Both `QueryText` and `FilePath` is not allowed.";
            }
            return null;
        }
    }
}