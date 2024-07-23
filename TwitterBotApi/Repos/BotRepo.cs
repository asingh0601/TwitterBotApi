using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using TwitterBotApi.Models;

namespace TwitterBotApi.Repos
{
	public interface IBotRepo
	{
		Task<bool> IsUserAuthorized(string commandUserName);
		bool IsAdmin(string commandUserName);
		Task<string> AddUser(string commandUserName, string username);
		Task<string> RemoveUser(string commandUserName, string username);
		Task<string> PromoteUser(string commandUserName, string username);
		Task<string> DemoteUser(string commandUserName, string username);
		Task AddUpdateProcessStatus(long? updateId, long? messageId, string userName, string message);
		bool IsUpdateProcessed(long? updateId, long? messageId);
		Task<string> AddBot(string commandUserName, string userName, string emailId, string password);
		Task<string> RemoveBot(string commandUserName, string username);
		Task<string> GetFailedLogins(string commandUserName);
		Task<string> GetDisabledIds(string commandUserName);
		Task<string> GetLockedIds(string commandUserName);
		Task<string> GetSuspendedIds(string commandUserName);
		string ActivateIds(string commandUserName, List<string> userNames, List<string>? switches);
		List<int>? GetProcessIds(string commandUserName, string spaceUrl);
		void UpdateKilledProcesses(List<int> processIds);
		void DeleteOldProcessIdEntries();
		void DeleteOldWebhookUpdateEntries();
		List<int> GetOrphanProcessesIds();
		List<int> GetOldProcessesIds();
		string GetBotStatistics(string commandUserName);
	}
	public class BotRepo(BotDbContext botDbContext, ILogger<BotRepo> logger) : IBotRepo
	{
		private readonly BotDbContext _botDbContext = botDbContext;
		private readonly ILogger<BotRepo> _logger = logger;

		public async Task<string> AddUser(string commandUserName, string username)
		{
			var commandUserType = _botDbContext.AuthorisedUsers.FirstOrDefault(u => u.UserName.ToLower() == commandUserName.ToLower())?.UserType ?? 0;
			if (commandUserType < 1)
			{
				_logger.LogInformation($"User [{commandUserName}] is not authorised to add user.");
				return "You are not authorised to add user.";
			}
			var existingUser = _botDbContext.AuthorisedUsers.FirstOrDefault(u => u.UserName.ToLower() == username.ToLower());
			if (existingUser is null)
			{
				await _botDbContext.AuthorisedUsers.AddAsync(new AuthorisedUser { UserName = username, UserType = 0 });
				await _botDbContext.SaveChangesAsync();
				_logger.LogInformation($"User [{username}] is added by [{commandUserName}].");
				return $"User [{username}] is added.";
			}
			else
			{
				_logger.LogInformation($"User [{username}] already exists. Command By: [{commandUserName}]");
				return $"User [{username}] already exists.";
			}
		}

		public async Task<string> RemoveUser(string commandUserName, string username)
		{
			if (string.Equals(commandUserName, username, StringComparison.InvariantCultureIgnoreCase))
			{
				_logger.LogInformation($"User [{commandUserName}] cannot remove self id.");
				return "You cannot remove yourself.";
			}
			var commandUserType = _botDbContext.AuthorisedUsers.FirstOrDefault(u => u.UserName.ToLower() == commandUserName.ToLower())?.UserType ?? 0;
			if (commandUserType < 1)
			{
				_logger.LogInformation($"User [{commandUserName}] is not authorised to remove user.");
				return "You are not authorised to remove user.";
			}
			var existingUser = _botDbContext.AuthorisedUsers.FirstOrDefault(u => u.UserName.ToLower() == username.ToLower());
			if (existingUser is not null)
			{
				_botDbContext.AuthorisedUsers.Remove(existingUser);
				await _botDbContext.SaveChangesAsync();
				_logger.LogInformation($"User [{username}] is removed by [{commandUserName}].");
				return $"User [{username}] is removed.";
			}
			else
			{
				_logger.LogInformation($"User [{username}] does not exist. Command By: [{commandUserName}]");
				return "User does not exist.";
			}
		}

		public async Task<string> PromoteUser(string commandUserName, string username)
		{
			if (string.Equals(commandUserName, username, StringComparison.InvariantCultureIgnoreCase))
			{
				_logger.LogInformation($"User [{commandUserName}] cannot promote self id.");
				return "You cannot promote yourself.";
			}
			var commandUserType = _botDbContext.AuthorisedUsers.FirstOrDefault(u => u.UserName.ToLower() == commandUserName.ToLower())?.UserType ?? 0;
			var existingUser = _botDbContext.AuthorisedUsers.FirstOrDefault(u => u.UserName.ToLower() == username.ToLower());
			if (existingUser is not null)
			{
				if (commandUserType > existingUser.UserType && existingUser.UserType < 2)
				{
					existingUser.UserType += 1;
					await _botDbContext.SaveChangesAsync();
					_logger.LogInformation($"User [{username}] is promoted to {GetUserTypeName(existingUser.UserType)}.");
					return $"User [{username}] is promoted to {GetUserTypeName(existingUser.UserType)}.";
				}
				else if (commandUserType <= existingUser.UserType)
				{
					_logger.LogInformation($"User [{commandUserType}] is not authorised to promote [{username}]");
					return "You are not authorised to promote this user.";
				}
				else
				{
					_logger.LogInformation($"User [{username}] already has the maximum privileges. Command By: [{commandUserName}]");
					return "This user already has the maximum privileges.";
				}
			}
			else
			{
				_logger.LogInformation($"User [{username}] does not exist. Command By: [{commandUserName}]");
				return "User does not exist.";
			}
		}

		public async Task<string> DemoteUser(string commandUserName, string username)
		{
			if (string.Equals(commandUserName, username, StringComparison.InvariantCultureIgnoreCase))
			{
				_logger.LogInformation($"User [{commandUserName}] cannot demote self id.");
				return "You cannot demote yourself.";
			}
			var commandUserType = _botDbContext.AuthorisedUsers.FirstOrDefault(u => u.UserName.ToLower() == commandUserName.ToLower())?.UserType ?? 0;
			var existingUser = _botDbContext.AuthorisedUsers.FirstOrDefault(u => u.UserName.ToLower() == username.ToLower());
			if (existingUser is not null)
			{
				if (commandUserType > existingUser.UserType && existingUser.UserType > 0)
				{
					existingUser.UserType -= 1;
					await _botDbContext.SaveChangesAsync();
					_logger.LogInformation($"User [{username}] is demoted to {GetUserTypeName(existingUser.UserType)}.");
					return $"User [{username}] is demoted to {GetUserTypeName(existingUser.UserType)}.";
				}
				else if (commandUserType <= existingUser.UserType)
				{
					_logger.LogInformation($"User [{commandUserType}] is not authorised to demote [{username}]");
					return "You are not authorised to demote this user.";
				}
				else
				{
					_logger.LogInformation($"User [{username}] already has the least privileges. Command By: [{commandUserName}]");
					return "This user already has the least privileges.";
				}
			}
			else
			{
				_logger.LogInformation($"User [{username}] does not exist. Command By: [{commandUserName}]");
				return "User does not exist.";
			}
		}

		public async Task<bool> IsUserAuthorized(string commandUserName)
		{
			if (string.IsNullOrWhiteSpace(commandUserName)) return false;
			return await _botDbContext.AuthorisedUsers.AnyAsync(x => x.UserName == commandUserName.ToLower());
		}

		public async Task AddUpdateProcessStatus(long? updateId, long? messageId, string userName, string message)
		{
			await _botDbContext.ProcessedUpdates.AddAsync(new ProcessedUpdate { UpdateId = updateId ?? 0, ProcessingDate = DateTime.Now, MessageId = messageId ?? 0, UserName = userName, Message = message });
			await _botDbContext.SaveChangesAsync();
		}

		public bool IsUpdateProcessed(long? updateId, long? messageId)
		{
			if (_botDbContext.ProcessedUpdates.Any(p => p.UpdateId == updateId || p.MessageId == messageId))
			{
				return true;
			}
			return false;
		}

		public async Task<string> AddBot(string commandUserName, string userName, string emailId, string password)
		{
			var commandUserType = _botDbContext.AuthorisedUsers.FirstOrDefault(u => u.UserName.ToLower() == commandUserName.ToLower())?.UserType ?? 0;
			if (commandUserType < 1)
			{
				_logger.LogInformation($"[{commandUserName}] is not authorised to add bots.");
				return "You are not authorised to add bots.";
			}
			var existingBot = _botDbContext.BotDetails.FirstOrDefault(u => u.UserName.ToLower() == userName.ToLower());
			if (existingBot is null)
			{
				await _botDbContext.BotDetails.AddAsync(new BotDetails
				{
					UserName = userName,
					EmailId = emailId,
					Password = password,
					LoginFailure = 0,
					IdDisabled = 0,
					IdLocked = 0,
					IdSuspended = 0,
				});
				await _botDbContext.SaveChangesAsync();
				_logger.LogInformation($"Bot [{userName}] is added. Command By: [{commandUserName}]");
				return $"Bot [{userName}] is added.";
			}
			else
			{
				_logger.LogInformation($"Bot [{userName}] already exists. Command By: [{commandUserName}]");
				return "Bot already exists.";
			}
		}

		public async Task<string> RemoveBot(string commandUserName, string username)
		{
			var commandUserType = _botDbContext.AuthorisedUsers.FirstOrDefault(u => u.UserName.ToLower() == commandUserName.ToLower())?.UserType ?? 0;
			if (commandUserType < 1)
			{
				_logger.LogInformation($"[{commandUserName}] is not authorised to remove bots.");
				return "You are not authorised to remove bots.";
			}
			var existingBot = _botDbContext.BotDetails.FirstOrDefault(u => u.UserName.ToLower() == username.ToLower());
			if (existingBot is not null)
			{
				_botDbContext.BotDetails.Remove(existingBot);
				await _botDbContext.SaveChangesAsync();
				_logger.LogInformation($"Bot [{username}] is removed. Command By: [{commandUserName}]");
				return $"Bot [{username}] is removed.";
			}
			else
			{
				_logger.LogInformation($"Bot [{username}] does not exist. Command By: [{commandUserName}]");
				return "Bot does not exist.";
			}
		}
		private static string GetUserTypeName(int userType)
		{
			return userType switch
			{
				0 => "User",
				1 => "Admin",
				2 => "Super Admin",
				_ => string.Empty,
			};
		}
		public async Task<string> GetFailedLogins(string commandUserName)
		{
			var commandUserType = _botDbContext.AuthorisedUsers.FirstOrDefault(u => u.UserName.ToLower() == commandUserName.ToLower())?.UserType ?? 0;
			if (commandUserType < 1)
			{
				_logger.LogInformation($"[{commandUserName}] is not authorised to view failed logins.");
				return "You are not authorised to view failed logins.";
			}
			var botNames = await _botDbContext.BotDetails.Where(b => b.LoginFailure >= 3).Select(b => b.UserName).ToListAsync();
			var result = string.Join("\n", botNames);
			if (string.IsNullOrWhiteSpace(result))
			{
				_logger.LogInformation($"There are no failed Logins. Command By: [{commandUserName}]");
				return "There are no failed Logins.";
			}
			_logger.LogInformation($"Logins are failing for: {result}. Command By: [{commandUserName}]");
			return $"<b>Logins are failing for {botNames.Count} bots:</b>\n{result}";
		}

		public async Task<string> GetDisabledIds(string commandUserName)
		{
			var commandUserType = _botDbContext.AuthorisedUsers.FirstOrDefault(u => u.UserName.ToLower() == commandUserName.ToLower())?.UserType ?? 0;
			if (commandUserType < 1)
			{
				_logger.LogInformation($"[{commandUserName}] is not authorised to view ids that require email verification.");
				return "You are not authorised to view ids that require email verification.";
			}
			var botNames = await _botDbContext.BotDetails.Where(b => b.IdDisabled == 1).Select(b => b.UserName).ToListAsync();
			var result = string.Join("\n", botNames);
			if (string.IsNullOrWhiteSpace(result))
			{
				_logger.LogInformation($"There are no ids that require email verification. Command By: [{commandUserName}]");
				return "There are no ids that require email verification.";
			}
			_logger.LogInformation($"Email verification is required for: {result}. Command By: [{commandUserName}]");
			return $"<b>Email verification is required for {botNames.Count} bots:</b>\n{result}";
		}
		public async Task<string> GetSuspendedIds(string commandUserName)
		{
			var commandUserType = _botDbContext.AuthorisedUsers.FirstOrDefault(u => u.UserName.ToLower() == commandUserName.ToLower())?.UserType ?? 0;
			if (commandUserType < 1)
			{
				_logger.LogInformation($"[{commandUserName}] is not authorised to view suspended ids.");
				return "You are not authorised to view suspended ids.";
			}
			var botNames = await _botDbContext.BotDetails.Where(b => b.IdSuspended == 1).Select(b => b.UserName).ToListAsync();
			var result = string.Join("\n", botNames);
			if (string.IsNullOrWhiteSpace(result))
			{
				_logger.LogInformation($"There are no suspended ids. Command By: [{commandUserName}]");
				return "There are no suspended ids.";
			}
			_logger.LogInformation($"Following Ids are suspended: {result}. Command By: [{commandUserName}]");
			return $"<b>Total {botNames.Count} ids are suspended:</b>\n{result}";
		}

		public async Task<string> GetLockedIds(string commandUserName)
		{
			var commandUserType = _botDbContext.AuthorisedUsers.FirstOrDefault(u => u.UserName.ToLower() == commandUserName.ToLower())?.UserType ?? 0;
			if (commandUserType < 1)
			{
				_logger.LogInformation($"[{commandUserName}] is not authorised to view locked ids.");
				return "You are not authorised to view locked ids.";
			}
			var botNames = await _botDbContext.BotDetails.Where(b => b.IdLocked == 1).Select(b => b.UserName).ToListAsync();
			var result = string.Join("\n", botNames);
			if (string.IsNullOrWhiteSpace(result))
			{
				_logger.LogInformation($"There are no locked ids. Command By: [{commandUserName}]");
				return "There are no locked ids.";
			}
			_logger.LogInformation($"Following Ids are locked: {result}. Command By: [{commandUserName}]");
			return $"<b>Total {botNames.Count} ids are locked:</b>\n{result}";
		}

		public string ActivateIds(string commandUserName, List<string> userNames, List<string>? switches)
		{
			var allSwitch = switches?.Contains("-all") ?? false;
			var unSuspend = switches?.Contains("-suspended") ?? false;
			var disabled = switches?.Contains("-disabled") ?? false;

			var commandUserType = _botDbContext.AuthorisedUsers.FirstOrDefault(u => u.UserName.ToLower() == commandUserName.ToLower())?.UserType ?? 0;
			if (commandUserType < 1)
			{
				_logger.LogInformation($"[{commandUserName}] is not authorised to activate ids.");
				return "You are not authorised activate ids.";
			}
			var botsQueryable = _botDbContext.BotDetails.AsQueryable();

			if (!allSwitch)
			{
				botsQueryable = botsQueryable.Where(u => userNames.Contains(u.UserName.ToLower()));
			}

			var bots = botsQueryable.ToList();
			var result = string.Join("\n", bots.Select(b => b.UserName));
			foreach (var bot in bots)
			{
				if (bot is not null)
				{
					bot.LoginFailure = 0;
					bot.IdLocked = 0;

					if (unSuspend)
					{
						bot.IdSuspended = 0;
					}
					if (disabled)
					{
						bot.IdDisabled = 0;
					}
					if (userNames.Count > 1)
					{
						_logger.LogInformation($"Following bots are re-activated:\n{result}\nCommand By: [{commandUserName}]");
						return $"Following bots are re-activated:\n{result}";
					}
					else
					{
						_logger.LogInformation($"Bot [{userNames[0]}] is re-activated. Command By: [{commandUserName}]");
						return $"Bot [{userNames[0]}] is re-activated.";
					}
				}
			}
			_botDbContext.SaveChanges();

			if(userNames.Count == 1)
			{
				_logger.LogInformation($"Bot [{userNames[0]}] does not exist. Command By: [{commandUserName}]");
				return $"Bot [{userNames[0]}] does not exist.";
			}
			else
			{
				_logger.LogInformation($"None of the bots {result} could be found. Command By: [{commandUserName}]");
				return $"None of the bots {result} could be found";
			}
		}

		public List<int>? GetProcessIds(string commandUserName, string spaceUrl)
		{
			var isAdminCommand = false;
			var commandUserType = _botDbContext.AuthorisedUsers.FirstOrDefault(u => u.UserName.ToLower() == commandUserName.ToLower())?.UserType ?? 0;
			if (commandUserType > 0) isAdminCommand = true;
			List<int>? processIds = [];
			try
			{
				SqlConnection conn = new(_botDbContext.ConnectionString);
				conn.Open();
				var sqlQuery = $"SELECT ProcessId FROM SpaceProcessIds WHERE Url = '{spaceUrl}'{(isAdminCommand ? string.Empty : $" AND CommandUserName  = {commandUserName}")}";

				using SqlCommand command = new(sqlQuery, conn);
				var result = command.ExecuteReader();

				while (result.Read())
				{
					processIds.Add((int)result["ProcessId"]);
				}
				conn.Close();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, ex.Message);
			}
			return processIds;
		}

		public void DeleteOldProcessIdEntries()
		{
			try
			{
				SqlConnection conn = new(_botDbContext.ConnectionString);
				conn.Open();
				var sqlQuery = $"DELETE FROM SpaceProcessIds WHERE ProcessDate < '{DateTime.Now.AddHours(-2):yyyyMMdd HH:mm:ss}' AND ProcessKilled = 1";

				using SqlCommand command = new(sqlQuery, conn);
				var result = command.ExecuteNonQuery();
				conn.Close();
			}
			catch (Exception ex) { _logger.LogError(ex, ex.Message); }
		}

		public void DeleteOldWebhookUpdateEntries()
		{
			try
			{
				SqlConnection conn = new(_botDbContext.ConnectionString);
				conn.Open();
				var sqlQuery = $"DELETE FROM ProcessedUpdates WHERE ProcessingDate < '{DateTime.Now.AddHours(-2):yyyyMMdd HH:mm:ss}'";

				using SqlCommand command = new(sqlQuery, conn);
				var result = command.ExecuteNonQuery();
				conn.Close();
			}
			catch (Exception ex) { _logger.LogError(ex, ex.Message); }
		}

		public List<int> GetOrphanProcessesIds()
		{
			List<int>? processIds = [];
			try
			{
				SqlConnection conn = new(_botDbContext.ConnectionString);
				conn.Open();
				var sqlQuery = $"SELECT ProcessId FROM SpaceProcessIds WHERE ProcessDate < '{DateTime.Now.AddMinutes(-10):yyyyMMdd HH:mm:ss}') AND (Url is null OR Url = '') And ProcessKilled = 0";

				using SqlCommand command = new(sqlQuery, conn);
				var result = command.ExecuteReader();

				while (result.Read())
				{
					processIds.Add((int)result["ProcessId"]);
				}
				conn.Close();
			}
			catch (Exception ex) { _logger.LogError(ex, ex.Message); }
			return processIds;
		}

		public List<int> GetOldProcessesIds()
		{
			List<int>? processIds = [];
			try
			{
				SqlConnection conn = new(_botDbContext.ConnectionString);
				conn.Open();
				var sqlQuery = $"SELECT ProcessId FROM SpaceProcessIds WHERE ProcessDate < '{DateTime.Now.AddHours(-1):yyyyMMdd HH:mm:ss}') And ProcessKilled = 0";

				using SqlCommand command = new(sqlQuery, conn);
				var result = command.ExecuteReader();

				while (result.Read())
				{
					processIds.Add((int)result["ProcessId"]);
				}
				conn.Close();
			}
			catch (Exception ex) { _logger.LogError(ex, ex.Message); }
			return processIds;
		}

		public void UpdateKilledProcesses(List<int> processIds)
		{
			try
			{
				SqlConnection conn = new(_botDbContext.ConnectionString);
				conn.Open();
				var sqlQuery = $"Update SpaceProcessIds Set ProcessKilled = 1 WHERE ProcessId in ({string.Join(',', processIds.ToArray())})";

				using SqlCommand command = new(sqlQuery, conn);
				var result = command.ExecuteNonQuery();
				conn.Close();
			}
			catch (Exception ex) { _logger.LogError(ex, ex.Message); }
		}

		public bool IsAdmin(string commandUserName)
		{
			var commandUserType = _botDbContext.AuthorisedUsers.FirstOrDefault(u => u.UserName.ToLower() == commandUserName.ToLower())?.UserType ?? 0;
			if (commandUserType < 1) return false;
			return true;
		}

		public string GetBotStatistics(string commandUserName)
		{
			var commandUserType = _botDbContext.AuthorisedUsers.FirstOrDefault(u => u.UserName.ToLower() == commandUserName.ToLower())?.UserType ?? 0;
			if (commandUserType < 1)
			{
				_logger.LogInformation($"User [{commandUserName}] is not authorised to view statistics.");
				return "You are not authorised to view statistics.";
			}
			var botsQueryable = _botDbContext.BotDetails.AsQueryable();
			var totalCount = botsQueryable.Count();
			var activeBotsCount = botsQueryable.Count(b => b.LoginFailure < 3 && b.IdLocked == 0 && b.IdSuspended == 0 && b.IdDisabled == 0);
			var disabledCount = botsQueryable.Count(b => b.IdDisabled == 1);
			var inactiveBotsCount = botsQueryable.Count(b => b.LoginFailure >= 3);
			var lockedBotsCount = botsQueryable.Count(b => b.IdLocked == 1);
			var suspendedBotsCount = botsQueryable.Count(b => b.IdSuspended == 1);
			return $"<b>Total Bots: {totalCount}</b>\nActive Bots: {activeBotsCount}\nDisabled Bots: {disabledCount}\nLogin Failures: {inactiveBotsCount}\nLocked Bots: {lockedBotsCount}\nSuspended Bots: {suspendedBotsCount}";
		}
	}
}
