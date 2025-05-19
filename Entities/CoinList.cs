using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CoinTradeAppMVC.Entities
{
	public class CoinList
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		[Required]
		[StringLength(25)]
		public string? CoinName { get; set; }
	}
}
