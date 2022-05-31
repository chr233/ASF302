using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ASF302
{
	internal static class CaddyHelper
	{

		internal static async Task<string?> InstallCaddy()
		{
			string sysName;
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				sysName = "windows";
			} else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			{
				sysName = "linux";
			} else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			{
				sysName = "macos";
			} else
			{
				return "未知的系统类型, 请手动下载caddy文件放进 plugins/caddy 目录下";
			}

			string arch;
			switch (RuntimeInformation.OSArchitecture)
			{
				case Architecture.X64:
					arch = "amd64";
					break;
				case Architecture.Arm64:
					arch = "arm64";
					break;
				case Architecture.Arm:
					arch = "arm6";
					break;
				default:
					return "未知的系统类型, 请手动下载caddy文件放进 plugins/caddy 目录下";
			}


			//if


			return null;
		}
	}
}
