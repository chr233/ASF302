using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using Titanium.Web.Proxy;
using Titanium.Web.Proxy.EventArguments;
using Titanium.Web.Proxy.Models;
using Titanium.Web.Proxy.Network;
using static ASF302.Utils;

namespace ASF302.Proxy
{
	internal class HttpProxy : IDisposable
	{
		private readonly ProxyServer ProxyServer = new();

		public CertificateEngine CertificateEngine { get; set; } = CertificateEngine.BouncyCastle;

		private static HashSet<string> ProxyDomains { get; } = new()
		{
			"steamcommunity.com"
		};

		private static List<IPAddress> AkamaiIPs { get; } = new()
		{
			IPAddress.Parse("184.28.218.19"),
			IPAddress.Parse("23.45.51.123"),
			IPAddress.Parse("23.45.51.170"),
			IPAddress.Parse("2.23.167.185"),
			IPAddress.Parse("23.55.47.172"),
			IPAddress.Parse("23.199.54.58"),
			IPAddress.Parse("23.2.16.33"),
		};

		public int ProxyPort { get; set; } = 8765;

		public IPAddress ProxyIp { get; set; } = IPAddress.Any;

		public IPAddress? ProxyDNS { get; set; }

		public bool ProxyRunning => ProxyServer.ProxyRunning;

		private const bool IsIpv6Support = false;

		private bool DisposedValue;

		public HttpProxy()
		{
			ProxyServer.ExceptionFunc = (exception) => ASFLogger.LogGenericError(exception.Message);

			ProxyServer.EnableHttp2 = true;
			ProxyServer.EnableConnectionPool = true;
			ProxyServer.CheckCertificateRevocation = X509RevocationMode.NoCheck;
			// 可选地设置证书引擎
			ProxyServer.CertificateManager.CertificateEngine = CertificateEngine;
			//proxyServer.CertificateManager.PfxPassword = $"{CertificateName}";
			//proxyServer.ThreadPoolWorkerThread = Environment.ProcessorCount * 8;

			string path =

			ProxyServer.CertificateManager.PfxFilePath = PluginLocation;
			ProxyServer.CertificateManager.RootCertificateIssuerName = "ASF302";
			ProxyServer.CertificateManager.RootCertificateName = "ASF302";
			//mac和ios的证书信任时间不能超过300天
			ProxyServer.CertificateManager.CertificateValidDays = 300;
			//proxyServer.CertificateManager.SaveFakeCertificates = true;

			ProxyServer.CertificateManager.RootCertificate = ProxyServer.CertificateManager.LoadRootCertificate();
		}

		private async Task OnRequest(object sender, SessionEventArgs e)
		{
#if DEBUG
			Debug.WriteLine("OnRequest " + e.HttpClient.Request.RequestUri.AbsoluteUri);
			Debug.WriteLine("OnRequest HTTP " + e.HttpClient.Request.HttpVersion);
			Debug.WriteLine("ClientRemoteEndPoint " + e.ClientRemoteEndPoint.ToString());
#endif
			if (e.HttpClient.Request.Host == null)
			{
				return;
			}

			//host模式下不启用加速会出现无限循环问题

			foreach (string domain in ProxyDomains)
			{

				if (e.HttpClient.Request.RequestUri.AbsoluteUri.Contains("steamcommunity.com", StringComparison.InvariantCultureIgnoreCase))
				{

					if (!e.HttpClient.IsHttps)
					{
						e.HttpClient.Request.RequestUri = new Uri(e.HttpClient.Request.RequestUri.AbsoluteUri.Remove(0, 4).Insert(0, "https"));
					}

					if (e.HttpClient.UpStreamEndPoint == null)
					{
						Random r = new();
						IPAddress ip = AkamaiIPs[r.Next(0, AkamaiIPs.Count)];
						e.HttpClient.UpStreamEndPoint = new IPEndPoint(ip, 443);
						e.HttpClient.Request.HttpVersion = HttpVersion.Version20;
					}

					if (e.HttpClient.ConnectRequest?.ClientHelloInfo?.Extensions != null)
					{
#if DEBUG
						//Logger.Info("ClientHelloInfo Info: " + e.HttpClient.ConnectRequest.ClientHelloInfo);
						Debug.WriteLine("ClientHelloInfo Info: " + e.HttpClient.ConnectRequest.ClientHelloInfo);
#endif

						e.HttpClient.ConnectRequest.ClientHelloInfo.Extensions.Remove("server_name");
						//e.HttpClient.ConnectRequest.ClientHelloInfo.Extensions.Add("server_name","steamcommunity.rmbgame.net");

					}
					return;
				}
			}
		}

		private async Task OnResponse(object sender, SessionEventArgs e)
		{
#if DEBUG
			Debug.WriteLine("OnResponse" + e.HttpClient.Request.RequestUri.AbsoluteUri);
#endif
		}

		// 允许重写默认的证书验证逻辑
		private static Task OnCertificateValidation(object sender, CertificateValidationEventArgs e)
		{
			// 根据证书错误，设置IsValid为真/假
			//if (e.SslPolicyErrors == System.Net.Security.SslPolicyErrors.None)
			e.IsValid = true;
			return Task.CompletedTask;
		}

		// 允许在相互身份验证期间重写默认客户端证书选择逻辑
		private static Task OnCertificateSelection(object sender, CertificateSelectionEventArgs e)
		{
			// set e.clientCertificate to override
			return Task.CompletedTask;
		}

		public async Task<bool> StartProxy()
		{
			#region 启动代理
			ProxyServer.BeforeRequest += OnRequest;
			ProxyServer.BeforeResponse += OnResponse;
			ProxyServer.ServerCertificateValidationCallback += OnCertificateValidation;
			//ProxyServer.ClientCertificateSelectionCallback += OnCertificateSelection;

			try
			{
				ExplicitProxyEndPoint explicitProxyEndPoint = new(ProxyIp, ProxyPort, true)
				{
					// 通过不启用为每个http的域创建证书来优化性能
					//GenericCertificate = proxyServer.CertificateManager.RootCertificate
				};
				explicitProxyEndPoint.BeforeTunnelConnectRequest += ExplicitProxyEndPoint_BeforeTunnelConnectRequest;


				//explicit endpoint 是客户端知道代理存在的地方
				ProxyServer.AddEndPoint(explicitProxyEndPoint);

				//if (PortInUse(443))
				//{
				//    return false;
				//}

				TransparentProxyEndPoint transparentProxyEndPoint;

				transparentProxyEndPoint = new TransparentProxyEndPoint(ProxyIp, 443, true)
				{
					// 通过不启用为每个http的域创建证书来优化性能
					//GenericCertificate = proxyServer.CertificateManager.RootCertificate
				};
				transparentProxyEndPoint.BeforeSslAuthenticate += TransparentProxyEndPoint_BeforeSslAuthenticate;
				ProxyServer.AddEndPoint(transparentProxyEndPoint);


				ProxyServer.Start();

			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.ToString());
				return false;
			}

			#endregion
#if DEBUG
			foreach (var endPoint in ProxyServer.ProxyEndPoints)
				Debug.WriteLine("Listening on '{0}' endpoint at Ip {1} and port: {2} ",
					endPoint.GetType().Name, endPoint.IpAddress, endPoint.Port);
#endif
			return true;
		}

		private Task TransparentProxyEndPoint_BeforeSslAuthenticate(object sender, BeforeSslAuthenticateEventArgs e)
		{
			e.DecryptSsl = false;

			if (e.SniHostName.Contains("localhost", StringComparison.OrdinalIgnoreCase))
			{
				e.DecryptSsl = true;
				return Task.CompletedTask;
			}
			foreach (string domain in ProxyDomains)
			{
				if (e.SniHostName.Contains(domain, StringComparison.InvariantCultureIgnoreCase))
				{
					e.DecryptSsl = true;
					return Task.CompletedTask;
				}
			}

			return Task.CompletedTask;
		}

		private async Task ExplicitProxyEndPoint_BeforeTunnelConnectRequest(object sender, TunnelConnectSessionEventArgs e)
		{
			e.DecryptSsl = false;
			if (ProxyDomains is null || e.HttpClient?.Request?.Host == null)
			{
				return;
			}
			if (e.HttpClient.Request.Host.Contains("localhost", StringComparison.OrdinalIgnoreCase))
			{
				e.DecryptSsl = true;
				return;
			}
			foreach (string domain in ProxyDomains)
			{
				if (e.HttpClient.Request.Url.Contains(domain, StringComparison.InvariantCultureIgnoreCase))
				{
					e.DecryptSsl = true;

					if (e.HttpClient.UpStreamEndPoint == null)
					{
						Random r = new();
						IPAddress ip = AkamaiIPs[r.Next(0, AkamaiIPs.Count)];
						e.HttpClient.UpStreamEndPoint = new IPEndPoint(ip, 443);
					}

					return;
				}

			}
			return;
		}

		public void StopProxy()
		{
			try
			{
				if (ProxyServer.ProxyRunning)
				{
					ProxyServer.BeforeRequest -= OnRequest;
					ProxyServer.BeforeResponse -= OnResponse;
					ProxyServer.ServerCertificateValidationCallback -= OnCertificateValidation;
					ProxyServer.ClientCertificateSelectionCallback -= OnCertificateSelection;
					ProxyServer.Stop();
				}
			}
			catch (Exception ex)
			{
				ASFLogger.LogGenericException(ex);
			}
		}

		private void Dispose(bool disposing)
		{
			if (!DisposedValue)
			{
				if (disposing)
				{
					// TODO: 释放托管状态(托管对象)
					if (ProxyServer.ProxyRunning)
					{
						StopProxy();
					}
					ProxyServer.Dispose();
				}

				// TODO: 释放未托管的资源(未托管的对象)并重写终结器
				// TODO: 将大型字段设置为 null
				DisposedValue = true;
			}
		}

		public void Dispose()
		{
			// 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}
