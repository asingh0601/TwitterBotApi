using TwitterBotApi.Models;

namespace TwitterBotApi.Helpers
{
	public interface IDirectoryHelper
	{
		void DeleteDirectoryAndEmptyParent(Process process);
	}
	public class DirectoryHelper : IDirectoryHelper
	{
		public void DeleteDirectoryAndEmptyParent(Process process)
		{
			if (Directory.Exists(process.UserDataDirectory))
			{
				var dir = new DirectoryInfo(process.UserDataDirectory);
				
				if (process.LoginSuccessful == 1)
				{
					var parentDirectoryPath = dir.Parent?.FullName;
                    if (!Directory.Exists(@$"{parentDirectoryPath}\master"))
                    {
                       Directory.CreateDirectory(@$"{parentDirectoryPath}\master"); 
                    }

					CopyFilesRecursively(dir.FullName, @$"{parentDirectoryPath}\master\{dir.Name}");
				}
				dir.Delete(true);
				if (!dir.Parent?.EnumerateFiles().Any() ?? false)
				{
					dir.Parent?.Delete(true);
				}
			}
		}
		private void CopyFilesRecursively(string sourcePath, string targetPath)
		{
			foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
			{
				Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
			}

			foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
			{
				File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
			}
		}

	}
}
