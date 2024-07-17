using System.ComponentModel.DataAnnotations;

namespace TwitterBotApi.Models
{
	public class ProcessedUpdate
	{
		[Key]
		public int Id { get; set; }
		public DateTime ProcessingDate { get; set; }
		public long UpdateId { get; set; }
		public long MessageId { get; set; }
		public string? UserName { get; set; }
		public string? Message { get; set; }
	}
}
