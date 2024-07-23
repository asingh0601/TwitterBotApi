namespace TwitterBotApi.Helpers
{
	public interface ITelegramHelper
	{
		Task SendMessage(long? chatId, string message);
	}
	public class TelegramHelper(IConfiguration configuration, ILogger<TelegramHelper> logger) : ITelegramHelper
	{
		private readonly string _telegramBaseurl = $"https://api.telegram.org/bot{configuration.GetSection("TelegramToken").Value}";
		private readonly ILogger<TelegramHelper> _logger = logger;
		private static readonly HttpClient client = new();

		public async Task SendMessage(long? chatId, string message)
		{
			var url = $"{_telegramBaseurl}/sendMessage?chat_id={chatId ?? 0}&text={message}&parse_mode=html";
			try
			{
				var response = await client.GetAsync(url);
				var responseString = await response.Content.ReadAsStringAsync();
			}
			catch (Exception ex) { _logger.LogError(ex, ex.Message); }
		}
	}
}
