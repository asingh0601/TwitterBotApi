using System.Diagnostics;
using System.Management;
using System.Runtime.Versioning;

namespace TwitterBotApi.Helpers
{
	public interface IProcessHelper
	{
		bool KillProcessAndChildren(int pid);
		bool KillAllProcessesByName(string processName);
	}
	public class ProcessHelper : IProcessHelper
	{

		[SupportedOSPlatform("windows")]
		public bool KillProcessAndChildren(int pid)
		{
			ManagementObjectSearcher searcher = new($"Select * From Win32_Process Where ParentProcessID={pid}");
			ManagementObjectCollection moc = searcher.Get();
			foreach (ManagementObject mo in moc.Cast<ManagementObject>())
			{
				KillProcessAndChildren(Convert.ToInt32(mo["ProcessID"]));
			}
			try
			{
				Process proc = Process.GetProcessById(pid);
				proc.Kill();
				return true;
			}
			catch (ArgumentException) { return false; }
		}

		[SupportedOSPlatform("windows")]
		public bool KillAllProcessesByName(string processsName)
		{
			try
			{
				foreach (var process in Process.GetProcessesByName(processsName))
				{
					process.Kill();
				}
				return true;
			}
			catch (ArgumentException) { return false; }
		}
	}
}
