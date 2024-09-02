using Colossal.PSI.Common;
using Colossal.PSI.PdxSdk;

using PDX.SDK.Contracts;

using System.Threading.Tasks;

namespace RoadBuilder.Utilities
{
	public static class PdxModsUtil
	{
		private static readonly PdxSdkPlatform _pdxPlatform;
		private static readonly IContext _context;

		public static int CurrentPlayset { get; private set; }
		public static string UserId { get; private set; }

		static PdxModsUtil()
		{
			_pdxPlatform = PlatformManager.instance.GetPSI<PdxSdkPlatform>("PdxSdk");
			_context = typeof(PdxSdkPlatform).GetField("m_SDKContext", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(_pdxPlatform) as PDX.SDK.Contracts.IContext;
		}

		public static async Task Start()
		{
			var currentPlaysetResult = await _context.Mods.GetActivePlayset();
			var profileResult = await _context.Profile.Get();

			CurrentPlayset = currentPlaysetResult.PlaysetId;
			UserId = profileResult.Social?.DisplayName;

			_pdxPlatform.onActivePlaysetChanged += PdxPlatform_onActivePlaysetChanged;
			_pdxPlatform.onLoggedIn += PdxPlatform_onLoggedIn;
		}

		private static async void PdxPlatform_onLoggedIn(string firstName, string lastName, string email, AccountLinkState accountLinkState, bool firstTime)
		{
			var profileResult = await _context.Profile.Get();

			UserId = profileResult.Social?.DisplayName;
		}

		private static async void PdxPlatform_onActivePlaysetChanged()
		{
			var currentPlaysetResult = await _context.Mods.GetActivePlayset();

			CurrentPlayset = currentPlaysetResult.PlaysetId;
		}
	}
}
