using System.Management;
using System.Runtime.Versioning;
using Process = System.Diagnostics.Process;

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
				proc.WaitForExit();
				proc.Dispose();
				return true;
			}
			catch (ArgumentException) { return false; }
		}

		[SupportedOSPlatform("windows")]
		public bool KillAllProcessesByName(string processName)
		{
			try
			{
				Process[] workers = Process.GetProcessesByName(processName);
				foreach (Process worker in workers)
				{
					worker.Kill();
					worker.WaitForExit();
					worker.Dispose();
				}
				return true;
			}
			catch (Exception) { return false; }
		}
	}
}
