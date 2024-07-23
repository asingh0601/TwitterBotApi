namespace TwitterBotApi
{
	public class WelcomeText
	{
		public const string Message = $"{WelcomeMessage}\n\n{UserCommands}";
		
		private const string WelcomeMessage = $"<b>Welcome to Nocturnal Twitter Bot!</b>";
		private const string UserCommands = $"<b>Available Commands:</b>\n{Commands.LikeRetweet} <i>TweetUrl</i>\n{Commands.JoinLaugh} <i>SpaceUrl</i>" +
			$"\n{Commands.LikeRetweet} <i>TweetUrl</i>\n{Commands.RJoinSpace} <i>SpaceUrl</i> -anonymous -timeoffset:20 <i>(switches are optional, offset is in seconds)</i>" +
			$"\n{Commands.LeaveSpace} <i>SpaceUrl</i>\n{Commands.Follow} <i>ProfileUrl</i>";
		public const string AdminCommands = $"\n\n<b>Admin Commands:</b>\n{Commands.AddUser} <i>TelegramUserName</i>\n{Commands.RemoveUser} <i>TelegramUserName</i>" +
			$"\n{Commands.PromoteUser} <i>TelegramUserName</i>\n{Commands.DemoteUser} <i>TelegramUserName</i>" +
			$"\n{Commands.AddBot} <i>UserName</i> <i>EmailId</i> <i>Password</i>\n{Commands.RemoveBot} <i>UserName</i>" +
			$"\n{Commands.ReEnableId} <i>BotUserName</i> (<i>switch</i>: [-all -> <i>activate all ids</i>] [[-disabled -> <i>enable a disabled id</i>]] [-suspended -> <i>enable a suspended id</i>])\n{Commands.FailedLogins}"+
			$"\n{Commands.LockedIds}\n{Commands.SuspendedIds}\n{Commands.BotStatistics}\n{Commands.KillSwitch}";
	}
}
