using System.ComponentModel.DataAnnotations;

namespace CoinTradeAppMVC.ViewModels
{
    public class SendEmailForSupportViewModel
    {
        [Required(ErrorMessage = "İsim zorunludur.")]
        [Display(Name = "Name:")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Soyad zorunludur.")]
        [Display(Name = "Surname:")]
        public string Surname { get; set; }

        [Required(ErrorMessage = "Email zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz.")]
        [Display(Name = "Email:")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Konu seçimi zorunludur.")]
        [Display(Name = "Subject:")]
        public string Subject { get; set; }

        [Required(ErrorMessage = "Mesaj kısmı zorunludur.")]
        [Display(Name = "Message:")]
        public string Message { get; set; }
    }
}
