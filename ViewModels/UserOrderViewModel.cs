using Microsoft.CodeAnalysis.Elfie.Model;
using System.ComponentModel.DataAnnotations;

namespace CoinTradeAppMVC.ViewModels
{
    public class UserOrderViewModel
    {
        [Required]
        public int Id { get; set; }
        [Required(ErrorMessage = "Please provide symbol name")]
        [StringLength(10, ErrorMessage = "Coin Symbol cannot exceed 10 characters.")]
        [Display(Name = "Symbol")]
        public string CoinSymbol { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Target Price must be greater than 0.")]
        [Display(Name = "Target Price")]
        public decimal TargetPrice { get; set; }

        [Required(ErrorMessage = "Please provide quantity")]
        [Range(0.0001, double.MaxValue, ErrorMessage = "Quantity must be greater than 0.")]
        [Display(Name = "Quantity")]
        public decimal Quantity { get; set; }

        [Required(ErrorMessage = "Please choose your action")]
        [Display(Name = "Action")]
        public bool IsBuyOrder { get; set; } // true: Alım, false: Satış

		[Required]
		public bool IsCompleted { get; set; } // Gerçekleşti mi?
	}
}
