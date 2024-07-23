using TwitterBotApi.Models;

namespace TwitterBotApi.Helpers
{
	public interface IDirectoryHelper
	{
		void DeleteDirectoryAndEmptyParent(Process process);
	}
	public class DirectoryHelper: IDirectoryHelper
	{
		public void DeleteDirectoryAndEmptyParent(Process process)
		{
			if (Directory.Exists(process.UserDataDirectory))
			{
				var dir = new DirectoryInfo(process.UserDataDirectory);
				dir.Delete(true);
				if (!dir.Parent?.EnumerateFiles().Any() ?? false)
				{
					dir.Parent?.Delete(true);
				}
			}
		}
	}
}
