using System.ComponentModel.DataAnnotations;

namespace SuperCRM.Models
{
	public class Verify
	{
		[Required]
		public string Token { get; set; }
	}
}
