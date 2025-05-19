using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using CoinTradeAppMVC.Web.Models;

namespace CoinTradeAppMVC.Entities
{
    public class UserMargin
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public AppUser User { get; set; }
        
        [Required]
        public string Symbol { get; set; }
        [Required]
        public decimal EntryPrice { get; set; }
        [Required]
        public decimal ExitPrice { get; set; }
        [Required]
		[Column(TypeName = "decimal(18, 4)")]
		public decimal OriginalAmount { get; set; }
        [Required]
		[Column(TypeName = "decimal(18, 4)")]
		public decimal BorrowAmount { get; set; }
        [Required]
        public bool IsLongMargin { get; set; }
		[Required]
		public DateTime CreatedDate { get; set; }
		[Required]
		public bool IsCompleted { get; set; }
	}
}
