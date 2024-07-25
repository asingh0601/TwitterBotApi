using System.Diagnostics;

namespace TwitterBotApi.Helpers
{
	public interface IBotHelper
	{
		void RunCommand(string commandUsername, long? chatId, string arguments, bool verbose = false);
	}
	public class BotHelper(ITelegramHelper telegramHelper, ILogger<BotHelper> logger) : IBotHelper
	{
		private readonly ITelegramHelper _telegramHelper = telegramHelper;
		private readonly ILogger<BotHelper> _logger = logger;
		private readonly string MSG_IDENTIFIER = "BOT_RESPONSE: ";
		public void RunCommand(string commandUsername, long? chatId, string arguments, bool verbose = false)
		{
			ProcessStartInfo startInfo = new()
			{
				CreateNoWindow = false,
				UseShellExecute = false,
#if DEBUG
				FileName = @"C:\Users\singh\source\repos\TwitterBot\TwitterBot\bin\Debug\net8.0\TwitterBot.exe",
				WindowStyle = ProcessWindowStyle.Normal,
#endif
#if !DEBUG
				FileName = "TwitterBot\\TwitterBot.exe",
				WindowStyle = ProcessWindowStyle.Hidden,
#endif
				Verb = "runas",
				Arguments = $"{arguments} -commandBy#{commandUsername}",
				RedirectStandardOutput = verbose,
				RedirectStandardError = verbose
			};
			try
			{
				using Process? exeProcess = Process.Start(startInfo);
				if (verbose)
				{
					exeProcess.BeginOutputReadLine();
					exeProcess.BeginErrorReadLine();
					exeProcess.OutputDataReceived += async (s, e) =>
					{
						_logger.LogInformation(e.Data);
						await SendMessageToTelegramIfSentByBot(chatId, e.Data);

					};
					exeProcess.ErrorDataReceived += async (s, e) =>
					{
						_logger.LogError(e.Data);
						await SendMessageToTelegramIfSentByBot(chatId, e.Data);
					};
				}
				exeProcess.WaitForExit();
			}
			catch (Exception ex) { _logger.LogError(ex, ex.Message); }
		}

		private async Task SendMessageToTelegramIfSentByBot(long? chatId, string? rawResponse)
		{
			var message = string.Empty;
			if (string.IsNullOrEmpty(rawResponse)) { return; };
			if (rawResponse.Contains(MSG_IDENTIFIER))
			{
				message = rawResponse.Replace(MSG_IDENTIFIER, string.Empty);
			}

			if (!string.IsNullOrEmpty(message))
			{
				await _telegramHelper.SendMessage(chatId, message);
			}
		}
	}
}
