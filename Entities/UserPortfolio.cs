using CoinTradeAppMVC.Web.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoinTradeAppMVC.Entities
{
    public class UserPortfolio
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; } // Primary Key

        [Required]
        public string UserId { get; set; } // AspNetUsers tablosu ile ilişki

        [ForeignKey("UserId")]
        public AppUser User { get; set; } // AspNetUsers ile navigasyon

        [Required]
        public string Symbol { get; set; } // Örneğin: BTCUSDT

        [Required]
        [Column(TypeName = "decimal(18, 6)")]
        public decimal Amount { get; set; } // Kullanıcının aldığı miktar

        [Required]
        public decimal Price { get; set; } // Coin başına ödenen fiyat

        [Required]
        public DateTime TransactionDate { get; set; } // İşlem tarihi
    }
}
