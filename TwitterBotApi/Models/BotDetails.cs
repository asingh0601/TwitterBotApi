using System.ComponentModel.DataAnnotations;

namespace TwitterBotApi.Models
{
	public class BotDetails
	{
		[Key]
		public int Id { get; set; }
		public required string UserName { get; set; }
		public required string EmailId { get; set; }
		public required string Password { get; set; }
		public int IdDisabled { get; set; }
		public int LoginFailure { get; set; }
		public int IdLocked { get; set; }
		public int IdSuspended { get; set; }
	}
}
