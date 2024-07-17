using Hangfire;
using Microsoft.AspNetCore.Mvc;
using TwitterBotApi.Models;
using TwitterBotApi.Services;

namespace TwitterBotApi.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class BotController(IBotService botService, IBackgroundJobClient backgroundJobClient) : ControllerBase
	{
		private readonly IBotService _botService = botService;
		private readonly IBackgroundJobClient _backgroundJobClient = backgroundJobClient;

		[HttpPost]
		public IActionResult Get([FromBody] WebHookUpdate webhookUpdate)
		{
			if (!_botService.IsAuthorizedUser(webhookUpdate).Result)
			{
				return Ok("You are not authorized to use this API");
			}
#if DEBUG
			_botService.ProcessUpdate(webhookUpdate).Wait();
#endif

#if !DEBUG
			_backgroundJobClient.Enqueue(() => _botService.ProcessUpdate(webhookUpdate));
#endif
			return Ok();
		}
	}
}
