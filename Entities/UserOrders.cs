using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using CoinTradeAppMVC.Web.Models;

namespace CoinTradeAppMVC.Entities
{
	public class UserOrders
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
		public decimal TargetPrice { get; set; }
		[Required]
		[Column(TypeName = "decimal(18, 6)")]
		public decimal Quantity { get; set; }   // Alınacak ya da satılacak miktar
		[Required]
		public bool IsBuyOrder { get; set; } // Alış true, satış false
		[Required]
		public DateTime CreatedDate { get; set; }
		[Required]
		public bool IsCompleted { get; set; }
	}
}
