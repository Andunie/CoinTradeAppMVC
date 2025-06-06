﻿using System.ComponentModel.DataAnnotations;

namespace CoinTradeAppMVC.ViewModels
{
    public class SignInViewModel
    {
        public SignInViewModel() 
        {

        }

        public SignInViewModel(string email, string password)
        {
            Email = email;
            Password = password;
        }
        [Required(ErrorMessage = "Email alanı boş bırakılamaz")]
        [EmailAddress(ErrorMessage = "Email formatı yanlıştır.")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Şifre alanı boş bırakılamaz")]
        [Display(Name = "Password")]
        public string Password { get; set; }
        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }
    }
}
