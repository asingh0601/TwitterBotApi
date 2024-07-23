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
				Thread.Sleep(3000);
				DeleteFilesRecursively(process.UserDataDirectory);
				if(!ReferenceEquals(dir.Parent, null))
				{
					if (!dir.Parent?.EnumerateFiles().Any() ?? false)
					{
						DeleteDirectory(dir.Parent.FullName);
					}
				}
			}
		}

		private void SetAttributesNormal(DirectoryInfo dir)
		{
			foreach (var subDir in dir.GetDirectories())
			{
				SetAttributesNormal(subDir);
			}
			foreach (var file in dir.GetFiles())
			{
				file.Attributes = FileAttributes.Normal;
			}
		}
		private void DeleteDirectory(string targetPath)
		{
			try
			{
				Directory.Delete(targetPath);
			}
			catch { }
		}


		private void DeleteFilesRecursively(string targetPath)
		{
			foreach (string filePath in Directory.GetFiles(targetPath, "*.*", SearchOption.AllDirectories))
			{
				try
				{
					File.Delete(filePath);
				}
				catch { }
			}

			foreach (string dirPath in Directory.GetDirectories(targetPath, "*", SearchOption.AllDirectories))
			{
				try
				{
					Directory.Delete(dirPath);
				}
				catch { }
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
