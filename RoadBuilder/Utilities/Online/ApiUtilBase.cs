using Colossal.Json;

using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RoadBuilder.Utilities.Online
{
	public class ApiUtilBase
	{
		// A single client is reused for every request, creating one per request exhausts sockets
		private static readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromSeconds(30) };

		public async Task<T?> Get<T>(string url, (string, string)[] headers, CancellationToken cancellationToken, params (string, object?)[] queryParams)
		{
			return await Send<T>(HttpMethod.Get, url, headers, null, cancellationToken, queryParams);
		}

		public async Task<T?> Delete<T>(string url, (string, string)[] headers, CancellationToken cancellationToken, params (string, object?)[] queryParams)
		{
			return await Send<T>(HttpMethod.Delete, url, headers, null, cancellationToken, queryParams);
		}

		public async Task<T?> Post<TBody, T>(string url, TBody body, (string, string)[] headers, CancellationToken cancellationToken, params (string, object?)[] queryParams)
		{
			var content = new StringContent(JSON.Dump(body, EncodeOptions.CompactPrint), Encoding.UTF8, "application/json");

			return await Send<T>(HttpMethod.Post, url, headers, content, cancellationToken, queryParams);
		}

		private async Task<T?> Send<T>(HttpMethod method, string baseUrl, (string, string)[] headers, HttpContent? content, CancellationToken cancellationToken, params (string, object?)[] queryParams)
		{
			var url = baseUrl;

			if (queryParams.Length > 0)
			{
				var query = queryParams.Where(x => x.Item2 is not null).Select(x => $"{Uri.EscapeDataString(x.Item1)}={Uri.EscapeDataString(x.Item2!.ToString())}");

				url += "?" + string.Join("&", query);
			}

			using var request = new HttpRequestMessage(method, new Uri(url)) { Content = content };

			foreach (var item in headers)
			{
				request.Headers.Add(item.Item1, item.Item2);
			}

			Mod.Log.Debug($"[API] [{method}] {baseUrl}");

			using var httpResponse = await _httpClient.SendAsync(request, cancellationToken);

			if (httpResponse.IsSuccessStatusCode)
			{
				if (typeof(T) == typeof(byte[]))
				{
					return (T)(object)await httpResponse.Content.ReadAsByteArrayAsync();
				}

				var response = await httpResponse.Content.ReadAsStringAsync();

				return JSON.MakeInto<T>(JSON.Load(response));
			}

			Mod.Log.Warn($"[API] ({baseUrl}) failed: {httpResponse.ReasonPhrase}");

			return typeof(T) == typeof(ApiResponse)
				? (T)(object)new ApiResponse { message = httpResponse.ReasonPhrase }
				: default;
		}
	}
}
