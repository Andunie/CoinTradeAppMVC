using System.ComponentModel.DataAnnotations;

namespace CoinTradeAppMVC.ViewModels
{
	public class ResetPasswordViewModel
	{
		[Required(ErrorMessage = "Şifre alanı boş bırakılamaz.")]
		[Display(Name = "New Password")]
		public string Password { get; set; } = null!;

		[Compare(nameof(Password), ErrorMessage = "Şifre aynı değildir.")]
		[Required(ErrorMessage = "Şifre tekrar alanı boş bırakılamaz")]
		[Display(Name = "New Password Again")]
		public string PasswordConfirm { get; set; } = null!;
	}
}
