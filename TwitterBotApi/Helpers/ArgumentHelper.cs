using TwitterBotApi.Models;

namespace TwitterBotApi.Helpers
{
    public interface IArgumentHelper
	{
		public string GetArguments(WebHookUpdate webhookUpdate);
	}
	public class ArgumentHelper : IArgumentHelper
	{
		public string GetArguments(WebHookUpdate webhookUpdate)
		{
			return webhookUpdate?.Message?.Text ?? string.Empty;
		}
	}
}
