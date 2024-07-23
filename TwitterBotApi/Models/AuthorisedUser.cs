using System.ComponentModel.DataAnnotations;

namespace TwitterBotApi.Models
{
	public class AuthorisedUser
	{
		[Key]
		public int Id { get; set; }
		public required string UserName { get; set; }
		public int UserType { get; set; }
	}
}
