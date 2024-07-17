using System.Diagnostics;

namespace TwitterBotApi.Helpers
{
	public interface IBotHelper
	{
		void RunCommand(string commandUsername, long? chatId, string arguments);
	}
	public class BotHelper(ITelegramHelper telegramHelper, ILogger<BotHelper> logger) : IBotHelper
	{
		private readonly ITelegramHelper _telegramHelper = telegramHelper;
		private readonly ILogger<BotHelper> _logger = logger;
		public void RunCommand(string commandUsername, long? chatId, string arguments)
		{
			int successCount = 0;
			int failureCount = 0;
			ProcessStartInfo startInfo = new()
			{
				CreateNoWindow = false,
				UseShellExecute = false,
#if DEBUG
				FileName = @"C:\Users\singh\source\repos\TwitterBot\TwitterBot\bin\Debug\net8.0\TwitterBot.exe",
#endif
#if !DEBUG
				FileName = "TwitterBot\\TwitterBot.exe",
#endif
				WindowStyle = ProcessWindowStyle.Hidden,
				Verb = "runas",
				Arguments = $"{arguments} -commandBy#{commandUsername}",
				RedirectStandardOutput = true,
				RedirectStandardError = true
			};
			try
			{
				using Process? exeProcess = Process.Start(startInfo);
				exeProcess.BeginOutputReadLine();
				exeProcess.BeginErrorReadLine();
				exeProcess.WaitForExit();
				exeProcess.OutputDataReceived += async (s, e) => { await _telegramHelper.SendMessage(chatId, e.Data); };
				exeProcess.ErrorDataReceived += async (s, e) =>
				{
					_logger.LogError(e.Data);
					await _telegramHelper.SendMessage(chatId, e.Data);
				};
			}
			catch (Exception ex) { _logger.LogError(ex, ex.Message); }
		}
	}
}
