using System.Runtime.Versioning;
using TwitterBotApi.Helpers;
using TwitterBotApi.Models;
using TwitterBotApi.Repos;

namespace TwitterBotApi.Services
{
	public interface IBotService
	{
		Task<bool> IsAuthorizedUser(WebHookUpdate webhookUpdate);
		Task ProcessUpdate(WebHookUpdate webhookUpdate);
	}
	public class BotService(IBotRepo botRepo, IBotHelper botHelper, IArgumentHelper argumentHelper, ITelegramHelper telegramHelper, IProcessHelper processHelper, IDirectoryHelper directoryHelper) : IBotService
	{
		private readonly IBotRepo _botRepo = botRepo;
		private readonly IBotHelper _botHelper = botHelper;
		private readonly IArgumentHelper _argumentHelper = argumentHelper;
		private readonly ITelegramHelper _telegramHelper = telegramHelper;
		private readonly IProcessHelper _processHelper = processHelper;
		private readonly IDirectoryHelper _directoryHelper = directoryHelper;

		public async Task<bool> IsAuthorizedUser(WebHookUpdate webhookUpdate)
		{
			var isAuthorized = await _botRepo.IsUserAuthorized(webhookUpdate?.Message?.From?.Username ?? string.Empty);
			if (!isAuthorized)
			{
				await _telegramHelper.SendMessage(webhookUpdate?.Message?.Chat?.Id, "You are not authorized to use this bot.");
			}
			return isAuthorized;
		}

		public async Task ProcessUpdate(WebHookUpdate webhookUpdate)
		{
			var commandUserName = webhookUpdate?.Message?.From?.Username ?? string.Empty;
			var userMessageString = _argumentHelper.GetArguments(webhookUpdate ?? new());
			var command = userMessageString.Split(" ").FirstOrDefault() ?? string.Empty;
			var arguments = userMessageString.Split(" ").Skip(1).FirstOrDefault(u => !u.Contains('-'))?.Split(',').Select(u => u.ToLower()).ToList() ?? [];
			if (!arguments.Any()) { arguments.Add(string.Empty); }

			var switches = userMessageString.Split(" ").Skip(1).Where(a => a.Contains('-')).Select(a => a.ToLower()).ToList();
			var verbose = switches.Any(a => a.Equals("-verbose", StringComparison.InvariantCultureIgnoreCase));

			if (_botRepo.IsUpdateProcessed(webhookUpdate?.UpdateId, webhookUpdate?.Message?.MessageId))
			{
				return;
			}

			if (command.Equals(Commands.Start))
			{
				await _telegramHelper.SendMessage(webhookUpdate?.Message?.Chat?.Id, $"{WelcomeText.Message}{(_botRepo.IsAdmin(webhookUpdate?.Message?.From?.Username ?? string.Empty) ? WelcomeText.AdminCommands : string.Empty)}");
			}

			#region usermanagement
			else if (command.Equals(Commands.AddUser, StringComparison.InvariantCultureIgnoreCase))
			{
				await SendResultResponse(webhookUpdate, await _botRepo.AddUser(commandUserName, arguments[0]));
			}
			else if (command.Equals(Commands.RemoveUser, StringComparison.InvariantCultureIgnoreCase))
			{
				await SendResultResponse(webhookUpdate, await _botRepo.RemoveUser(commandUserName, arguments[0]));
			}
			else if (command.Equals(Commands.PromoteUser, StringComparison.InvariantCultureIgnoreCase))
			{
				await SendResultResponse(webhookUpdate, await _botRepo.PromoteUser(commandUserName, arguments[0]));
			}
			else if (command.Equals(Commands.DemoteUser, StringComparison.InvariantCultureIgnoreCase))
			{
				await SendResultResponse(webhookUpdate, await _botRepo.DemoteUser(commandUserName, arguments[0]));
			}
			#endregion

			#region botactions
			else if (command.Equals(Commands.LikeRetweet, StringComparison.InvariantCultureIgnoreCase))
			{
				var commandDisabled = true;
				if(switches.Any(a => a.Equals("-bypass", StringComparison.InvariantCultureIgnoreCase)))
				{
					commandDisabled = false;
				}
				if (commandDisabled)
				{
					await SendResultResponse(webhookUpdate, "Like and retweet bot is disabled for now.");
				}
				else
				{
					_botHelper.RunCommand(commandUserName, webhookUpdate?.Message?.Chat?.Id, $"{Commands.LikeRetweet} {arguments[0]}");
				}
			}
			else if (command.Equals(Commands.JoinLaugh, StringComparison.InvariantCultureIgnoreCase))
			{
				_botHelper.RunCommand(commandUserName, webhookUpdate?.Message?.Chat?.Id, $"{Commands.JoinLaugh} {arguments[0]}");
			}
			else if (command.Equals(Commands.JoinSpace, StringComparison.InvariantCultureIgnoreCase))
			{
				_botHelper.RunCommand(commandUserName, webhookUpdate?.Message?.Chat?.Id, $"{Commands.JoinSpace} {arguments[0]}");
			}
			else if (command.Equals(Commands.RJoinSpace, StringComparison.InvariantCultureIgnoreCase))
			{
				_botHelper.RunCommand(commandUserName, webhookUpdate?.Message?.Chat?.Id, $"{Commands.RJoinSpace} {arguments[0]}");
			}
			else if (command.Equals(Commands.LeaveSpace, StringComparison.InvariantCultureIgnoreCase))
			{
				if (OperatingSystem.IsWindows())
				{
					LeaveTwitterSpace(commandUserName, arguments[0]);
				}
			}
			else if (command.Equals(Commands.Follow, StringComparison.InvariantCultureIgnoreCase))
			{
				var commandDisabled = true;
				if (switches.Any(a => a == "-bypass"))
				{
					commandDisabled = false;
				}
				if (commandDisabled)
				{
					await SendResultResponse(webhookUpdate, "Follow bot is disabled for now.");
				}
				else
				{
					_botHelper.RunCommand(commandUserName, webhookUpdate?.Message?.Chat?.Id, $"{Commands.Follow} {arguments[0]}");
				}
			}
			else if (command.Equals(Commands.ReportSpace, StringComparison.InvariantCultureIgnoreCase))
			{
				_botHelper.RunCommand(commandUserName, webhookUpdate?.Message?.Chat?.Id, $"{Commands.ReportSpace} {arguments[0]}");
			}
			#endregion

			#region botmanagement
			else if (command.Equals(Commands.AddBot, StringComparison.InvariantCultureIgnoreCase))
			{
				var userName = arguments.FirstOrDefault() ?? string.Empty;
				var emailId = arguments.Skip(1).FirstOrDefault() ?? string.Empty;
				var password = arguments.Skip(2).FirstOrDefault() ?? string.Empty;	
				await SendResultResponse(webhookUpdate, await _botRepo.AddBot(commandUserName, userName, emailId, password));
			}
			else if (command.Equals(Commands.RemoveBot, StringComparison.InvariantCultureIgnoreCase))
			{
				await SendResultResponse(webhookUpdate, await _botRepo.RemoveBot(commandUserName, arguments[0]));
			}
			else if (command.Equals(Commands.DisableBot, StringComparison.InvariantCultureIgnoreCase))
			{
				await SendResultResponse(webhookUpdate, await _botRepo.DisableBot(commandUserName, arguments[0]));
			}
			else if (command.Equals(Commands.FailedLogins, StringComparison.InvariantCultureIgnoreCase))
			{
				await SendResultResponse(webhookUpdate, await _botRepo.GetFailedLogins(commandUserName));
			}
			else if (command.Equals(Commands.DisabledIds, StringComparison.InvariantCultureIgnoreCase))
			{
				await SendResultResponse(webhookUpdate, await _botRepo.GetDisabledIds(commandUserName));
			}
			else if (command.Equals(Commands.LockedIds, StringComparison.InvariantCultureIgnoreCase))
			{
				await SendResultResponse(webhookUpdate, await _botRepo.GetLockedIds(commandUserName));
			}
			else if (command.Equals(Commands.SuspendedIds, StringComparison.InvariantCultureIgnoreCase))
			{
				await SendResultResponse(webhookUpdate, await _botRepo.GetSuspendedIds(commandUserName));
			}
			else if (command.Equals(Commands.ReEnableId, StringComparison.InvariantCultureIgnoreCase))
			{
				

				await SendResultResponse(webhookUpdate, _botRepo.ActivateIds(commandUserName, arguments, switches));
			}
			else if (command.Equals(Commands.BotStatistics, StringComparison.InvariantCultureIgnoreCase))
			{
				await SendResultResponse(webhookUpdate, _botRepo.GetBotStatistics(commandUserName));
			}
			#endregion

			else if (command.Equals(Commands.KillSwitch, StringComparison.InvariantCultureIgnoreCase))
			{
				await SendResultResponse(webhookUpdate, await KillAllProcesses(webhookUpdate));
			}
			else
			{
				await SendResultResponse(webhookUpdate, "Invalid Command");
			}

#if !DEBUG
			await _botRepo.AddUpdateProcessStatus(webhookUpdate?.UpdateId, webhookUpdate?.Message?.MessageId, commandUserName, webhookUpdate?.Message?.Text ?? string.Empty);
#endif
		}
		#region privatemethods
		[SupportedOSPlatform("windows")]
		private void LeaveTwitterSpace(string commandUserName, string spaceUrl)
		{
			var processes = _botRepo.GetProcesses(commandUserName, spaceUrl);
			List<int>? killedProcessIds = [];
			foreach (var process in processes ?? [])
			{
				if (_processHelper.KillProcessAndChildren(process.ProcessId))
				{
					killedProcessIds.Add(process.ProcessId);
					_directoryHelper.DeleteDirectoryAndEmptyParent(process);
				}
			}
			_botRepo.UpdateKilledProcesses(killedProcessIds);
		}

		private async Task<string> KillAllProcesses(WebHookUpdate? webhookUpdate)
		{
			var isAuthorized = await _botRepo.IsUserAuthorized(webhookUpdate?.Message?.From?.Username ?? string.Empty);
			if (!isAuthorized)
			{
				await _telegramHelper.SendMessage(webhookUpdate?.Message?.Chat?.Id, "You are not authorized to use this kill switch.");
			}
			try
			{
				_processHelper.KillAllProcessesByName("chromedriver");
				_processHelper.KillAllProcessesByName("chrome");
				_processHelper.KillAllProcessesByName("TwitterBot");
				return "All chrome drivers and chrome instances have been killed.";
			}
			catch (Exception)
			{
				return "Failed to kill chrome drivers and chrome instances.";
			}
		}

		private async Task SendResultResponse(WebHookUpdate? webhookUpdate, string? message)
		{
			if (string.IsNullOrWhiteSpace(message)) { return; }
			await _telegramHelper.SendMessage(webhookUpdate?.Message?.Chat?.Id, message);
		}
		#endregion
	}
}
