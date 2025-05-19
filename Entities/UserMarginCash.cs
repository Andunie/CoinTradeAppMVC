using CoinTradeAppMVC.Web.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CoinTradeAppMVC.Entities
{
    public class UserMarginCash
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public AppUser User { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Balance { get; set; }

        [Required]
        public DateTime LastUpdated { get; set; }
        [Required]
        [StringLength(64)]
        public string WalletAddress { get; set; }
    }
}
