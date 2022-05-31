using System.ComponentModel;
using System.Composition;
using System.Text;
using ArchiSteamFarm.Plugins.Interfaces;
using ArchiSteamFarm.Steam;
using ASF302.Localization;
using Newtonsoft.Json.Linq;

using static ASF302.Utils;

namespace ASF302
{

	[Export(typeof(IPlugin))]
	internal sealed class ASF302 : IPlugin, IASF, IBotCommand2
	{
		public string Name => nameof(ASF302);
		public Version Version => typeof(ASF302).Assembly.GetName().Version ?? throw new InvalidOperationException(nameof(Version));

		public Task OnLoaded()
		{
			StringBuilder message = new("\n");
			message.AppendLine(Static.Line);
			message.AppendLine(Static.Logo);
			message.AppendLine(Static.Line);

			ASFLogger.LogGenericInfo(message.ToString());

			return Task.CompletedTask;
		}

		public Task OnASFInit(IReadOnlyDictionary<string, JToken>? additionalConfigProperties = null)
		{
			if (additionalConfigProperties == null)
			{
				return Task.CompletedTask;
			}

			foreach ((string configProperty, JToken configValue) in additionalConfigProperties)
			{
				if (configProperty == "ASFEnhanceDevFuture" && configValue.Type == JTokenType.Boolean)
				{
					//DeveloperFeature = configValue.Value<bool>();
					break;
				}
			}

			return Task.CompletedTask;
		}
		public async Task<string?> OnBotCommand(Bot bot, EAccess access, string message, string[] args, ulong steamID = 0)
		{
			if (!Enum.IsDefined(access))
			{
				throw new InvalidEnumArgumentException(nameof(access), (int) access, typeof(EAccess));
			}

			switch (args.Length)
			{
				case 0:
					throw new InvalidOperationException(nameof(args.Length));
				case 1: //不带参数
					switch (args[0].ToUpperInvariant())
					{
						case "302INSTALL":
						case "3I":
							return await CaddyHelper.InstallCaddy().ConfigureAwait(false);


						default:
							return null;
					}
				default: //带参数
					switch (args[0].ToUpperInvariant())
					{

						default:
							return null;
					}
			}

		}

	}
}
