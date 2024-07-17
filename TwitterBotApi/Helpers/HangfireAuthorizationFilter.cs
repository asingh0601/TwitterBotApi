using Hangfire.Dashboard;

namespace TwitterBotApi.Helpers
{
	public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
	{
		public bool Authorize(DashboardContext context)
		{
			return true;
		}
	}
}
