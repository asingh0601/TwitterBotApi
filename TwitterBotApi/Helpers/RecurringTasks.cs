using TwitterBotApi.Repos;

namespace TwitterBotApi.Helpers
{
	public interface IRecurringTasks
	{
		void CleanupDB();
		void KillOldProcesses();
		void KillOrphanProcesses();
	}
	public class RecurringTasks(IBotRepo botRepo, IProcessHelper processHelper, IDirectoryHelper directoryHelper) : IRecurringTasks
	{
		private readonly IBotRepo _botRepo = botRepo;
		private readonly IProcessHelper _processHelper = processHelper;
		private readonly IDirectoryHelper _directoryHelper = directoryHelper;
		public void CleanupDB()
		{
			_botRepo.DeleteOldProcessIdEntries();
			_botRepo.DeleteOldWebhookUpdateEntries();
		}

		public void KillOldProcesses()
		{
			var processes = _botRepo.GetOldProcesses();

			if(processes == null) { return; }

			foreach (var process in processes)
			{
				_processHelper.KillProcessAndChildren(process.ProcessId);
				_directoryHelper.DeleteDirectoryAndEmptyParent(process);
			}
		}

		public void KillOrphanProcesses()
		{
			var processes = _botRepo.GetOrphanProcesses();

			if (processes == null) { return; }

			foreach (var process in processes)
			{
				_processHelper.KillProcessAndChildren(process.ProcessId);
				_directoryHelper.DeleteDirectoryAndEmptyParent(process);
			}
		}
	}
}
