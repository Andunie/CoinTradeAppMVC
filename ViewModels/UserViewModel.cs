using Humanizer.Localisation;
using System.ComponentModel.DataAnnotations;

namespace CoinTradeAppMVC.ViewModels
{
	public class UserViewModel
	{
        public string? UserName { get; set; } 
        public string? Email { get; set; }
		public string? PhoneNumber { get; set; } // Null olabilir.

		[Required(ErrorMessage = "Password can not be empty")]
		[Display(Name = "Password")]
		public string? Password { get; set; }
		
		[Required(ErrorMessage = "Password can not be empty")]
		[Display(Name = "New Password :")]
		public string? NewPassword { get; set; }
		
		[Required(ErrorMessage = "New Password can not be empty")]
		[Display(Name = "New Password Confirm :")]
		public string? NewPasswordConfirm { get; set; }
    }
}
