using System.ComponentModel.DataAnnotations;

namespace TwitterBotApi.Models
{
	public class BotDetails
	{
		[Key]
		public int Id { get; set; }
		public string? UserName { get; set; }
		public string? EmailId { get; set; }
		public string? Password { get; set; }
		public int LoginFailure { get; set; }
	}
}
