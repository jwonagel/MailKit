#if NET6_0_OR_GREATER
#nullable enable

using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace MailKit.Net.Proxy;

internal class SystemProxy : ProxyClient
{
	internal SystemProxy() : base(string.Empty, 0)
	{
	}

	public override Stream Connect (string host, int port, CancellationToken cancellationToken = default)
	{
		var systemProxy = HttpClient.DefaultProxy;
		var proxyUri = systemProxy.GetProxy (new Uri ($"{host}:{port}"));
		if (proxyUri is null) {
			// proxy is on bypass list, connect without
			var socket = SocketUtils.Connect (host, port, LocalEndPoint, int.MaxValue, cancellationToken); // Timeout can be ignored because it is already validated in base class
			return new NetworkStream (socket, true);
		}

		var proxyClient = GetProxyClient (proxyUri);
		systemProxy.Credentials = systemProxy.Credentials;
		return proxyClient.Connect (host, port, cancellationToken);
	}

	public override async Task<Stream> ConnectAsync (string host, int port, CancellationToken cancellationToken = default)
	{
		var systemProxy = HttpClient.DefaultProxy;
		var proxyUri = systemProxy.GetProxy (new Uri ($"{host}:{port}"));
		if (proxyUri is null) {
			// proxy is on bypass list, connect without
			var socket = await SocketUtils.ConnectAsync (host, port, LocalEndPoint, int.MaxValue, cancellationToken)  // Timeout can be ignored because it is already validated in base class
				.ConfigureAwait (false);
			return new NetworkStream (socket, true);
		}
		
		ProxyClient proxyClient = GetProxyClient (proxyUri);
		
		systemProxy.Credentials = systemProxy.Credentials;
		return await proxyClient.ConnectAsync (host, port, cancellationToken);
	}

	private ProxyClient GetProxyClient (Uri proxyUri)
	{
		return proxyUri.Scheme switch {
			"https" => new HttpsProxyClient (proxyUri.Host, proxyUri.Port),
			"http" => new HttpProxyClient (proxyUri.Host, proxyUri.Port),
			_ => throw new NotImplementedException()
		};
		// todo throw useful exception and check maybe for other schemes..
	}
}
#endif
