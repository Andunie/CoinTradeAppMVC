using System.ComponentModel.DataAnnotations;

namespace CoinTradeAppMVC.ViewModels
{
    public class TransferViewModel
    {
        [Required]
        [Display(Name = "SelectCoin")]
        public string SelectCoin { get; set; }

        [Required]
        [Display(Name = "Alıcı Cüzdan Adresi")]
        public string RecipientWalletAddress { get; set; }
        
        [Required]
        [Range(0.0001, double.MaxValue, ErrorMessage = "Geçerli bir miktar giriniz.")]
        [Display(Name = "Miktar")]
        public decimal Amount { get; set; }
    }
}
