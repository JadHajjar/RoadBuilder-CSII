using Colossal;
using Colossal.Json;

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace RoadBuilder.Utilities.Online
{
	public class ApiUtilBase
	{
		public async Task<T> Get<T>(string url, params (string, object)[] queryParams)
		{
			return await Get<T>(url, new (string, string)[0], queryParams);
		}

		public async Task<T> Get<T>(string url, (string, string)[] headers, params (string, object)[] queryParams)
		{
			return await Send<T>("GET", url, headers, queryParams);
		}

		public async Task<T> Delete<T>(string url, params (string, object)[] queryParams)
		{
			return await Delete<T>(url, new (string, string)[0], queryParams);
		}

		public async Task<T> Delete<T>(string url, (string, string)[] headers, params (string, object)[] queryParams)
		{
			return await Send<T>("DELETE", url, headers, queryParams);
		}

		private async Task<T> Send<T>(string method, string baseUrl, (string, string)[] headers, params (string, object)[] queryParams)
		{
			var url = baseUrl;

			if (queryParams.Length > 0)
			{
				var query = queryParams.Where(x => x.Item2 is not null).Select(x => $"{Uri.EscapeDataString(x.Item1)}={Uri.EscapeDataString(x.Item2.ToString())}");

				url += "?" + string.Join("&", query);
			}

			using var httpClient = new HttpClient();

			foreach (var item in headers)
			{
				httpClient.DefaultRequestHeaders.Add(item.Item1, item.Item2);
			}

			Mod.Log.Debug($"[API] [{method}] {baseUrl}");

			var httpResponse = await httpClient.SendAsync(new HttpRequestMessage(new HttpMethod(method), new Uri(url)));

			if (httpResponse.IsSuccessStatusCode)
			{
				if (typeof(T) == typeof(byte[]))
				{
					return (T)(object)await httpResponse.Content.ReadAsByteArrayAsync();
				}

				var response = await httpResponse.Content.ReadAsStringAsync();

				return JSON.MakeInto<T>(JSON.Load(response));
			}

			Mod.Log.Error($"[API] ({baseUrl}) failed: {httpResponse.ReasonPhrase}");

			return typeof(T) == typeof(ApiResponse)
				? (T)(object)new ApiResponse
				{
					message = httpResponse.ReasonPhrase
				}
				: default;
		}

		public async Task<T> Post<TBody, T>(string url, TBody body, params (string, object)[] queryParams)
		{
			return await Post<TBody, T>(url, body, new (string, string)[0], queryParams);
		}

		public async Task<T> Post<TBody, T>(string baseUrl, TBody body, (string, string)[] headers, params (string, object)[] queryParams)
		{
			var url = baseUrl;
			var json = JSON.Dump(body, EncodeOptions.CompactPrint);

			if (queryParams.Length > 0)
			{
				var query = queryParams.Where(x => x.Item2 is not null).Select(x => $"{Uri.EscapeDataString(x.Item1)}={Uri.EscapeDataString(x.Item2.ToString())}");

				url += "?" + string.Join("&", query);
			}

			using var httpClient = new HttpClient();

			foreach (var item in headers)
			{
				httpClient.DefaultRequestHeaders.Add(item.Item1, item.Item2);
			}

			Mod.Log.Debug($"[API] [POST] {baseUrl}");

			var content = new StringContent(json, Encoding.UTF8, "application/json");
			var httpResponse = await httpClient.PostAsync(url, content);

			if (httpResponse.IsSuccessStatusCode)
			{
				if (typeof(T) == typeof(byte[]))
				{
					return (T)(object)await httpResponse.Content.ReadAsByteArrayAsync();
				}

				var response = await httpResponse.Content.ReadAsStringAsync();

				return JSON.MakeInto<T>(JSON.Load(response));
			}

			Mod.Log.Error($"[API] ({baseUrl}) failed: {httpResponse.ReasonPhrase}");

			return typeof(T) == typeof(ApiResponse)
				? (T)(object)new ApiResponse { message = httpResponse.ReasonPhrase }
				: default;
		}
	}
}
