using TwitterBotApi.Repos;

namespace TwitterBotApi.Helpers
{
	public interface IRecurringTasks
	{
		void CleanupDB();
		void KillOldProcesses();
		void KillOrphanProcesses();
	}
	public class RecurringTasks(IBotRepo botRepo, IProcessHelper processHelper) : IRecurringTasks
	{
		private readonly IBotRepo _botRepo = botRepo;
		private readonly IProcessHelper _processHelper = processHelper;
		public void CleanupDB()
		{
			_botRepo.DeleteOldProcessIdEntries();
			_botRepo.DeleteOldWebhookUpdateEntries();
		}

		public void KillOldProcesses()
		{
			foreach(var pid in _botRepo.GetOldProcessesIds())
			{
				_processHelper.KillProcessAndChildren(pid);
			}
		}

		public void KillOrphanProcesses()
		{
			foreach (var pid in _botRepo.GetOrphanProcessesIds())
			{
				_processHelper.KillProcessAndChildren(pid);
			}
		}
	}
}
