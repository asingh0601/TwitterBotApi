using System.ComponentModel.DataAnnotations;

namespace TwitterBotApi.Models
{
	public class AuthorisedUser
	{
		[Key]
		public int Id { get; set; }
		public string? UserName { get; set; }
		public int UserType { get; set; }
	}
}
