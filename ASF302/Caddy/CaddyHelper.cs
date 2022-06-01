using System.Diagnostics;
using System.Runtime.InteropServices;
using ArchiSteamFarm.Core;
using ArchiSteamFarm.Web.Responses;
using ASF302.Localization;
using static ASF302.Utils;

namespace ASF302.Caddy
{
	internal static class CaddyHelper
	{
		private static readonly Uri GitHubLink = new("https://raw.githubusercontent.com/chr233/");
		private static readonly Uri GitHubMirror = new("https://raw.chrxw.com/");

		private static readonly string CaddyFolder = Path.GetFullPath(Path.Join("plugins", "caddy"));

		private const string CaddyConfig = "CaddyFile";
		private const string SteamCert = "steamcommunity.crt";
		private const string SteamPKey = "steamcommunity.key";

		internal static async Task<string?> GetCaddyPath()
		{
			if (!Directory.Exists(CaddyFolder))
			{
				_ = Directory.CreateDirectory(CaddyFolder);
				return null;
			}

			foreach (string filePath in Directory.GetFiles(CaddyFolder, "*", SearchOption.TopDirectoryOnly))
			{
				if (Path.GetFileName(filePath) == CaddyConfig)
				{
					continue;
				}

				CmdResponse response = await TestCaddy(filePath).ConfigureAwait(false);

				if (response.Success)
				{
					return filePath;
				}
			}
			return null;
		}

		private static async Task<CmdResponse> ExecAndWaitResult(this Process process)
		{
			try
			{
				if (process.Start())
				{
					using CancellationTokenSource timeoutSignal = new(TimeSpan.FromSeconds(5));
					try
					{
						await process.WaitForExitAsync(timeoutSignal.Token).ConfigureAwait(false);
					}
					catch (OperationCanceledException)
					{
						process.Kill();
					}

					bool success = process.ExitCode == 0;

					return new(success, string.Empty);
				}
				else
				{
					return new(false, Langs.StartProcessFailed);
				}

			}
			catch (Exception ex)
			{
				ASFLogger.LogGenericError(ex.Message);
				return new(false, ex.Message);
			}

		}

		internal static bool CheckCaddyProcess()
		{
			foreach (Process process in Process.GetProcesses())
			{
				try
				{
					if (process.ProcessName.Contains("caddy", StringComparison.InvariantCultureIgnoreCase))
					{
						if (process.MainModule.FileName.StartsWith(CaddyFolder))
						{
							return true;
						}
					}
				}
				catch (Exception ex)
				{
					continue;
				}

			}
			return false;
		}


		internal static async Task<CmdResponse> TestCaddy()
		{
			string path = await GetCaddyPath().ConfigureAwait(false);
			return await TestCaddy(path).ConfigureAwait(false);
		}

		internal static async Task<CmdResponse> TestCaddy(string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				return new(false, Langs.CaddyNotExists);
			}

			using Process process = new()
			{
				StartInfo = new()
				{
					FileName = path,
					Arguments = "version",
					UseShellExecute = false,
					RedirectStandardError = true,
					RedirectStandardInput = true,
					RedirectStandardOutput = true,
					CreateNoWindow = true,
					WorkingDirectory = CaddyFolder
				}
			};

			CmdResponse result = await process.ExecAndWaitResult().ConfigureAwait(false);
			return result;
		}

		internal static async Task<CmdResponse> StartCaddy()
		{
			string path = await GetCaddyPath().ConfigureAwait(false);
			return await StartCaddy(path).ConfigureAwait(false);
		}

		internal static async Task<CmdResponse> StartCaddy(string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				return new(false, Langs.CaddyNotExists);
			}

			Process process = new()
			{
				StartInfo = new()
				{
					FileName = path,
					Arguments = $"start {CaddyConfig}",
					UseShellExecute = false,
					RedirectStandardError = true,
					RedirectStandardInput = true,
					RedirectStandardOutput = true,
					CreateNoWindow = true,
					WorkingDirectory = CaddyFolder
				}
			};

			CmdResponse result = await process.ExecAndWaitResult().ConfigureAwait(false);
			return result;
		}
		internal static async Task<CmdResponse> StopCaddy()
		{
			string path = await GetCaddyPath().ConfigureAwait(false);
			return await StopCaddy(path).ConfigureAwait(false);
		}


		internal static async Task<CmdResponse> StopCaddy(string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				return new(false, Langs.CaddyNotExists);
			}

			Process process = new()
			{
				StartInfo = new()
				{
					FileName = path,
					Arguments = $"stop {CaddyConfig}",
					UseShellExecute = false,
					RedirectStandardError = true,
					RedirectStandardInput = true,
					RedirectStandardOutput = true,
					CreateNoWindow = true,
					WorkingDirectory = CaddyFolder
				}
			};

			CmdResponse result = await process.ExecAndWaitResult().ConfigureAwait(false);
			return result;
		}

		internal static async Task<CmdResponse> ReloadCaddy()
		{
			string path = await GetCaddyPath().ConfigureAwait(false);
			return await ReloadCaddy(path).ConfigureAwait(false);
		}

		internal static async Task<CmdResponse> ReloadCaddy(string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				return new(false, Langs.CaddyNotExists);
			}

			Process process = new()
			{
				StartInfo = new()
				{
					FileName = path,
					Arguments = $"reload {CaddyConfig}",
					UseShellExecute = false,
					RedirectStandardError = true,
					RedirectStandardInput = true,
					RedirectStandardOutput = true,
					CreateNoWindow = true,
					WorkingDirectory = CaddyFolder
				}
			};

			CmdResponse result = await process.ExecAndWaitResult().ConfigureAwait(false);
			return result;
		}

		internal static async Task<string> GenerateCaddyFile()
		{
			string filePath = Path.Join(CaddyFolder, CaddyConfig);

			try
			{
				using StreamWriter sw = new(filePath);

				string config = Static.Config.Replace("$$PORT$$", "443").Replace("$$CERT$$", SteamCert).Replace("$$PKEY$$", SteamPKey);

				await sw.WriteAsync(config).ConfigureAwait(false);

				await sw.FlushAsync().ConfigureAwait(false);

				sw.Close();

				return "Caddy 配置写入成功";
			}
			catch (Exception ex)
			{
				return $"Caddy 配置写入失败 {ex.Message}";
			}
		}

		internal static async Task<string> GenerateCaddyCert()
		{
			string filePath = Path.Join(CaddyFolder, SteamCert);
			try
			{
				using StreamWriter sw = new(filePath);

				string config = Static.Config;

				await sw.WriteAsync(config).ConfigureAwait(false);

				await sw.FlushAsync().ConfigureAwait(false);

				sw.Close();

				return "Caddy 配置写入成功";
			}
			catch (Exception ex)
			{
				return $"Caddy 配置写入失败 {ex.Message}";
			}
		}

		internal static async Task<string> InstallCaddy()
		{
			bool isWin = false;
			string sysName;
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				sysName = "windows";
				isWin = true;
			}
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			{
				sysName = "linux";
			}
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			{
				sysName = "macos";
			}
			else
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
				case Architecture.X86:
				case Architecture.Wasm:
				case Architecture.S390x:
				default:
					return "未知的系统类型, 请手动下载caddy文件放进 plugins/caddy 目录下";
			}

			Uri request = new(GitHubMirror, $"/ASF302/master/Caddy/caddy_{sysName}_{arch}");

			BinaryResponse binResponse = await ASF.WebBrowser.UrlGetToBinary(request).ConfigureAwait(false);

			if (binResponse == null)
			{
				return "网络错误";
			}

			string path = Path.Join(CaddyFolder, isWin ? "caddy.exe" : "caddy");

			using (FileStream fs = File.Create(path))
			{
				await fs.WriteAsync(binResponse.Content.ToArray()).ConfigureAwait(false);
				await fs.FlushAsync().ConfigureAwait(false);
				fs.Close();
			}

			return "下载caddy完成";
		}
	}
}
