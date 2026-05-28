using System.ComponentModel.DataAnnotations;

namespace CreditSim.Core.Models
{
    /// <summary>
    /// Custom validation attribute that enforces creditHistory must be "good" or "bad".
    /// Mirrors: body('creditHistory').isIn(['good', 'bad'])
    /// </summary>
    public class CreditHistoryValidationAttribute : ValidationAttribute
    {
        public CreditHistoryValidationAttribute()
            : base("Credit history must be either \"good\" or \"bad\"") { }

        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            var str = value as string;
            if (str == "good" || str == "bad")
                return ValidationResult.Success!;

            return new ValidationResult(ErrorMessage);
        }
    }
}
