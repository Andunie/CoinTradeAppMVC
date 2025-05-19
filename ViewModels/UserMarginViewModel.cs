using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CoinTradeAppMVC.ViewModels
{
    public class UserMarginViewModel
    {
        [Required]
        public int Id { get; set; }
        [Required(ErrorMessage = "Please provide symbol name")]
        [StringLength(10, ErrorMessage = "Coin Symbol cannot exceed 10 characters.")]
        [Display(Name = "Symbol")]
        public string CoinSymbol { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Entry Price must be greater than 0.")]
        [Display(Name = "Entry Price")]
        public decimal EntryPrice { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Exit Price must be greater than 0.")]
        [Display(Name = "Exit Price")]
        public decimal ExitPrice { get; set; }

        [Required(ErrorMessage = "Please provide original amount")]
        [Range(0.0001, double.MaxValue, ErrorMessage = "Original amount must be greater than 0.")]
        [Display(Name = "Original Amount")]
        public decimal OriginalAmount{ get; set; }

        [Required(ErrorMessage = "Please provide borrow amount")]
        [Range(0.0001, double.MaxValue, ErrorMessage = "Borrow amount must be greater than 0.")]
        [Display(Name = "Borrow Amount")]
        public decimal BorrowAmount { get; set; }

        [Required]
        public decimal ProfitOrLoss { get; set; }   // Hesaplanan kâr / zârar

        [Required]
        public bool IsLongMargin { get; set; }  // True : Long/Buy, False : Short/Sell


        [Required]
        public bool IsCompleted { get; set; } // Gerçekleşti mi
    }
}