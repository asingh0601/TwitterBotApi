using System.Diagnostics;
using System.Management;
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
	public class BotService(IBotRepo botRepo, IBotHelper botHelper, IArgumentHelper argumentHelper, ITelegramHelper telegramHelper, IProcessHelper processHelper) : IBotService
	{
		private readonly IBotRepo _botRepo = botRepo;
		private readonly IBotHelper _botHelper = botHelper;
		private readonly IArgumentHelper _argumentHelper = argumentHelper;
		private readonly ITelegramHelper _telegramHelper = telegramHelper;
		private readonly IProcessHelper _processHelper = processHelper;

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
			var argument = _argumentHelper.GetArguments(webhookUpdate);

			if (_botRepo.IsUpdateProcessed(webhookUpdate?.UpdateId, webhookUpdate?.Message?.MessageId))
			{
				return;
			}

			if (argument.Contains(Commands.Start))
			{
				await _telegramHelper.SendMessage(webhookUpdate?.Message?.Chat?.Id, $"{WelcomeText.Message}{(_botRepo.IsAdmin(webhookUpdate?.Message?.From?.Username ?? string.Empty) ? WelcomeText.AdminCommands : string.Empty)}");
			}
			else if (argument.Contains(Commands.AddUser))
			{
				await SendResultResponse(webhookUpdate, await _botRepo.AddUser(webhookUpdate?.Message?.From?.Username ?? string.Empty, argument.Split(' ')[1]));
			}
			else if (argument.Contains(Commands.RemoveUser))
			{
				await SendResultResponse(webhookUpdate, await _botRepo.RemoveUser(webhookUpdate?.Message?.From?.Username ?? string.Empty, argument.Split(' ')[1]));
			}
			else if (argument.Contains(Commands.PromoteUser))
			{
				await SendResultResponse(webhookUpdate, await _botRepo.PromoteUser(webhookUpdate?.Message?.From?.Username ?? string.Empty, argument.Split(' ')[1]));
			}
			else if (argument.Contains(Commands.DemoteUser))
			{
				await SendResultResponse(webhookUpdate, await _botRepo.DemoteUser(webhookUpdate?.Message?.From?.Username ?? string.Empty, argument.Split(' ')[1]));
			}
			else if (argument.Contains(Commands.LikeRetweet))
			{
				_botHelper.RunCommand(webhookUpdate?.Message?.From?.Username ?? string.Empty, webhookUpdate?.Message?.Chat?.Id, argument);
			}
			else if (argument.Contains(Commands.JoinLaugh))
			{
				_botHelper.RunCommand(webhookUpdate?.Message?.From?.Username ?? string.Empty, webhookUpdate?.Message?.Chat?.Id, argument);
			}
			else if (argument.Contains(Commands.JoinSpace))
			{
				_botHelper.RunCommand(webhookUpdate?.Message?.From?.Username ?? string.Empty, webhookUpdate?.Message?.Chat?.Id, argument);
			}
			else if (argument.Contains(Commands.RJoinSpace))
			{
				_botHelper.RunCommand(webhookUpdate?.Message?.From?.Username ?? string.Empty, webhookUpdate?.Message?.Chat?.Id, argument);
			}
			else if (argument.Contains(Commands.DisabledIds))
			{
				await SendResultResponse(webhookUpdate, await _botRepo.GetDisabledIds(webhookUpdate?.Message?.From?.Username ?? string.Empty));
			}
			else if (argument.Contains(Commands.LockedIds))
			{
				await SendResultResponse(webhookUpdate, await _botRepo.GetLockedIds(webhookUpdate?.Message?.From?.Username ?? string.Empty));
			}
			else if (argument.Contains(Commands.SuspendedIds))
			{
				await SendResultResponse(webhookUpdate, await _botRepo.GetSuspendedIds(webhookUpdate?.Message?.From?.Username ?? string.Empty));
			}
			else if (argument.Contains(Commands.LeaveSpace))
			{
				if (OperatingSystem.IsWindows())
				{
					LeaveTwitterSpace(webhookUpdate?.Message?.From?.Username ?? string.Empty, argument.Split(' ')[1]);
				}
			}
			else if (argument.Contains(Commands.Follow))
			{
				_botHelper.RunCommand(webhookUpdate?.Message?.From?.Username ?? string.Empty, webhookUpdate?.Message?.Chat?.Id, argument);
			}
			else if (argument.Contains(Commands.AddBot))
			{
				await SendResultResponse(webhookUpdate, await _botRepo.AddBot(webhookUpdate?.Message?.From?.Username ?? string.Empty, argument.Split(' ')[1], argument.Split(' ')[2], argument.Split(' ')[3]));
			}
			else if (argument.Contains(Commands.RemoveBot))
			{
				await SendResultResponse(webhookUpdate, await _botRepo.RemoveBot(webhookUpdate?.Message?.From?.Username ?? string.Empty, argument.Split(' ')[1]));
			}
			else if (argument.Contains(Commands.FailedLogins))
			{
				await SendResultResponse(webhookUpdate, await _botRepo.GetFailedLogins(webhookUpdate?.Message?.From?.Username ?? string.Empty));
			}
			else if (argument.Contains(Commands.ReEnableId))
			{
				var userNames = argument.Split(" ").Skip(1).FirstOrDefault(u => !u.Contains('-'))?.Split(',').Select(u => u.ToLower()).ToList() ?? [];
				var switches = argument.Split(" ").Skip(1).Where(a => a.Contains('-')).Select(a => a.ToLower()).ToList();

				await SendResultResponse(webhookUpdate, _botRepo.ActivateIds(webhookUpdate?.Message?.From?.Username ?? string.Empty, userNames, switches));
			}
			else if (argument.Contains(Commands.BotStatistics))
			{
				await SendResultResponse(webhookUpdate, _botRepo.GetBotStatistics(webhookUpdate?.Message?.From?.Username ?? string.Empty));
			}
			else if (argument.Contains(Commands.KillSwitch))
			{
				await SendResultResponse(webhookUpdate, await KillAllProcesses(webhookUpdate));
			}
			else
			{
				await SendResultResponse(webhookUpdate, "Invalid Command");
			}

#if !DEBUG
			await _botRepo.AddUpdateProcessStatus(webhookUpdate?.UpdateId, webhookUpdate?.Message?.MessageId, webhookUpdate?.Message?.From?.Username ?? string.Empty, webhookUpdate?.Message?.Text ?? string.Empty);
#endif
		}
		[SupportedOSPlatform("windows")]
		private void LeaveTwitterSpace(string commandUserName, string spaceUrl)
		{
			var processIds = _botRepo.GetProcessIds(commandUserName, spaceUrl);
			List<int>? killedProcessIds = [];
			foreach (var pid in processIds ?? [])
			{
				if (_processHelper.KillProcessAndChildren(pid))
				{
					killedProcessIds.Add(pid);
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
	}
}
