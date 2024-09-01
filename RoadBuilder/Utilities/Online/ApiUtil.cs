using Colossal.PSI.Common;
using Colossal.PSI.PdxSdk;

using RoadBuilder.Domain.API;

using System.Threading.Tasks;

namespace RoadBuilder.Utilities.Online
{
	public class ApiUtil
	{
		private readonly ApiUtilBase _apiUtil = new();
		private string userId;

		public static ApiUtil Instance { get; } = new();

		public async Task<bool> Start()
		{
			if (userId is not null)
			{
				return true;
			}

			var pdxPlatform = PlatformManager.instance.GetPSI<PdxSdkPlatform>("PdxSdk");
			var context = typeof(PdxSdkPlatform).GetField("m_SDKContext", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(pdxPlatform) as PDX.SDK.Contracts.IContext;

			var result = await context.Profile.Get();

			userId = result.Social?.DisplayName;

			return userId is not null;
		}

		public async Task<PagedContent<RoadBuilderEntry>> GetEntries(string query = null, int? category = null, int order = 0, int page = 1)
		{
			return await Get<PagedContent<RoadBuilderEntry>>("/Roads", (nameof(query), query), (nameof(category), category), (nameof(order), order), (nameof(page), page));
		}

		public async Task<ApiResponse> GetEntries(RoadBuilderEntryPost road)
		{
			return await Post<RoadBuilderEntryPost, ApiResponse>("/SaveRoad", road);
		}

		private async Task<T> Get<T>(string url, params (string, object)[] queryParams)
		{
			return await _apiUtil.Get<T>(KEYS.API_URL + url
				, new (string, string)[] { ("USER_ID", userId), ("IDENTIFIER", PlatformManager.instance.userSpecificPath) }
				, queryParams);
		}

		private async Task<T> Delete<T>(string url, params (string, object)[] queryParams)
		{
			return await _apiUtil.Delete<T>(KEYS.API_URL + url
				, new (string, string)[] { ("USER_ID", userId), ("IDENTIFIER", PlatformManager.instance.userSpecificPath) }
				, queryParams);
		}

		private async Task<T> Post<TBody, T>(string url, TBody body, params (string, object)[] queryParams)
		{
			return await _apiUtil.Post<TBody, T>(KEYS.API_URL + url
				, body
				, new (string, string)[] { ("USER_ID", userId), ("IDENTIFIER", PlatformManager.instance.userSpecificPath) }
				, queryParams);
		}
	}
}
