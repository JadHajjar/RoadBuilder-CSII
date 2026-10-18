using Colossal.PSI.Common;

using RoadBuilder.Domain.API;
using RoadBuilder.Domain.Configurations;

using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RoadBuilder.Utilities.Online
{
	public class ApiUtil
	{
		private readonly ApiUtilBase _apiUtil = new();

		public static ApiUtil Instance { get; } = new();

		public async Task<PagedContent<RoadBuilderEntry>?> GetEntries(string? query = null, int? category = null, int order = 0, int page = 1, CancellationToken cancellationToken = default)
		{
			return await Get<PagedContent<RoadBuilderEntry>>("/Roads", cancellationToken, (nameof(query), query), (nameof(category), category), (nameof(order), order), (nameof(page), page));
		}

		public async Task<ApiResponse?> UploadRoad(RoadBuilderEntryPost road, CancellationToken cancellationToken = default)
		{
			return await Post<RoadBuilderEntryPost, ApiResponse>("/SaveRoad", road, cancellationToken);
		}

		public async Task<INetworkConfig?> GetConfig(string id, CancellationToken cancellationToken = default)
		{
			var data = await Get<byte[]>($"/RoadConfig/{Uri.EscapeDataString(id)}.json", cancellationToken);

			if (data == null)
			{
				return null;
			}

			return LocalSaveUtil.LoadFromJson(Encoding.UTF8.GetString(data));
		}

		private async Task<T?> Get<T>(string url, CancellationToken cancellationToken, params (string, object?)[] queryParams)
		{
			return await _apiUtil.Get<T>(KEYS.API_URL + url, GetHeaders(), cancellationToken, queryParams);
		}

		private async Task<T?> Delete<T>(string url, CancellationToken cancellationToken, params (string, object?)[] queryParams)
		{
			return await _apiUtil.Delete<T>(KEYS.API_URL + url, GetHeaders(), cancellationToken, queryParams);
		}

		private async Task<T?> Post<TBody, T>(string url, TBody body, CancellationToken cancellationToken, params (string, object?)[] queryParams)
		{
			return await _apiUtil.Post<TBody, T>(KEYS.API_URL + url, body, GetHeaders(), cancellationToken, queryParams);
		}

		private static (string, string)[] GetHeaders()
		{
			return new (string, string)[] { ("API_KEY", KEYS.API_KEY), ("USER_ID", PdxModsUtil.UserId ?? string.Empty), ("IDENTIFIER", PlatformManager.instance.userSpecificPath) };
		}
	}
}
