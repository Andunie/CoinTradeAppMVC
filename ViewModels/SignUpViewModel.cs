using System.ComponentModel.DataAnnotations;

namespace CoinTradeAppMVC.Web.ViewModels
{
    public class SignUpViewModel
    {
        public SignUpViewModel()
        {
                
        }

        public SignUpViewModel(string userName, string email, string phone, string password) 
        {
            UserName = userName;
            Email = email;
            Phone = phone;
            Password = password;
        }
        [Required(ErrorMessage ="Please provide your username")]
        [Display(Name = "Username")]
        public string UserName { get; set; }

		[EmailAddress(ErrorMessage = "Please provide your email")]
		[Display(Name = "Email")]
        public string Email { get; set; }

		[Required(ErrorMessage = "Please provide your phone number")]
		[Display(Name = "Phone")]
        public string Phone { get; set; }

		[Required(ErrorMessage = "Please provide your password")]
		[Display(Name = "Password")] 
        public string Password { get; set; }

        [Compare(nameof(Password),ErrorMessage ="Passwords do not match")]
		[Required(ErrorMessage = "Please provide your password confirm")]
		[Display(Name = "Confirm Password")]
        public string PasswordConfirm { get; set; }
    }
}
