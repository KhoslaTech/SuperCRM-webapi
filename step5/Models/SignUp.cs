using System.ComponentModel.DataAnnotations;

namespace SuperCRM.Models
{
	public enum AccountType
	{
		Individual,
		Team,
	}

	[CustomValidation(typeof(SignUp), nameof(SignUp.IsValid))]
	public class SignUp
	{
		[Required]
		[MaxLength(60)]
		public string Name { get; set; }

		[Required]
		[EmailAddress]
		[MaxLength(100)]
		public string Email { get; set; }

		[Required]
		[StringLength(512, MinimumLength = 6, ErrorMessage = "{0} must be between {1} and {2} characters.")]
		public string Password { get; set; }

		public AccountType Type { get; set; }

		public bool RememberMe { get; set; }

		[MaxLength(128)]
		public string BusinessName { get; set; }

		public static ValidationResult IsValid(SignUp model, ValidationContext context)
		{
			if (model.Type == AccountType.Team &&
			    string.IsNullOrWhiteSpace(model.BusinessName))
				return new ValidationResult("BusinessName is required");

			return ValidationResult.Success;
		}
	}
}
