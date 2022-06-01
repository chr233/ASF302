using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArchiSteamFarm.Steam;

namespace ASF302.Caddy
{
	internal sealed class Command
	{
		internal static async Task<string> ResponseTestCaddy()
		{
			CmdResponse result = await CaddyHelper.TestCaddy().ConfigureAwait(false);

			return result.ToString();
		}

		internal static async Task<string> ResponseStartCaddy()
		{
			CmdResponse result = await CaddyHelper.StartCaddy().ConfigureAwait(false);

			return result.ToString();
		}

		internal static async Task<string> ResponseStopCaddy()
		{
			CmdResponse result = await CaddyHelper.StopCaddy().ConfigureAwait(false);

			return result.ToString();
		}

		internal static async Task<string> ResponseReloadCaddy()
		{
			CmdResponse result = await CaddyHelper.ReloadCaddy().ConfigureAwait(false);

			return result.ToString();
		}

		internal static async Task<string> ResponseInstallCaddy()
		{
			string result = await CaddyHelper.InstallCaddy().ConfigureAwait(false);
			return result;
		}

		internal static string ResponseCaddyStatus()
		{
			bool isCaddyRunning = CaddyHelper.CheckCaddyProcess();
			return isCaddyRunning ? "Caddy 运行中" : "Caddy 没有运行";
		}

		internal static async Task<string> ResponseConfigCaddy()
		{
			string result = await CaddyHelper.GenerateCaddyFile().ConfigureAwait(false);
			return result;
		}

	}
}
