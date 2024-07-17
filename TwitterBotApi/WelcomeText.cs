namespace TwitterBotApi
{
	public class WelcomeText
	{
		public const string Message = $"{WelcomeMessage}\n\n{UserCommands}";
		
		private const string WelcomeMessage = $"Welcome to Nocturnal Twitter Bot!";
		private const string UserCommands = $"Available Commands:\n{Commands.LikeTweet} <TweetUrl>\n{Commands.Retweet} <TweetUrl>" +
			$"\n{Commands.JoinSpace} <SpaceUrl>\n{Commands.RJoinSpace} <SpaceUrl> -anonymous -timeoffset:20 (switches are optional, offset is in seconds)"+
			$"\n{Commands.LeaveSpace} <SpaceUrl>\n{Commands.Follow} <ProfileUrl>";
		public const string AdminCommands = $"\n\nAdmin Commands:\n{Commands.AddUser} <TelegramUserName>\n{Commands.RemoveUser} <TelegramUserName>" +
			$"\n{Commands.PromoteUser} <TelegramUserName>\n{Commands.DemoteUser} <TelegramUserName>" +
			$"\n{Commands.AddBot} <UserName> <EmailId> <Password>\n{Commands.RemoveBot} <UserName>" +
			$"\n{Commands.ReEnableId} <BotUserName>(use activate -all to activate all ids)\n{Commands.FailedLogins}"+
			$"\n{Commands.BotStatistics}";
	}
}
