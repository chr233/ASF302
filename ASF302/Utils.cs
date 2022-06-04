using ArchiSteamFarm.Core;
using ArchiSteamFarm.NLog;
using ArchiSteamFarm.Steam;
using ArchiSteamFarm.Steam.Integration;
using ArchiSteamFarm.Steam.Interaction;

namespace ASF302
{
	internal static class Utils
	{
		/// <summary>
		/// 格式化返回文本
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		internal static string FormatStaticResponse(string message) => Commands.FormatStaticResponse(message);

		/// <summary>
		/// 格式化返回文本
		/// </summary>
		/// <param name="bot"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		internal static string FormatBotResponse(this Bot bot, string message) => bot.Commands.FormatBotResponse(message);

		/// <summary>
		/// 获取版本号
		/// </summary>
		internal static Version MyVersion => typeof(ASF302).Assembly.GetName().Version;

		/// <summary>
		/// 获取插件所在路径
		/// </summary>
		internal static string MyLocation => typeof(ASF302).Assembly.Location;

		internal static string PluginLocation => Path.GetFullPath("plugins");

		/// <summary>
		/// Steam商店链接
		/// </summary>
		internal static Uri SteamStoreURL => ArchiWebHandler.SteamStoreURL;

		/// <summary>
		/// Steam社区链接
		/// </summary>
		internal static Uri SteamCommunityURL => ArchiWebHandler.SteamCommunityURL;

		/// <summary>
		/// 日志
		/// </summary>
		internal static ArchiLogger ASFLogger => ASF.ArchiLogger;
	}
}
