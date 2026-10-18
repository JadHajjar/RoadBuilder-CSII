using Colossal.PSI.Common;
using Colossal.PSI.PdxSdk;

using PDX.SDK.Contracts;

using System;
using System.Threading.Tasks;

namespace RoadBuilder.Utilities
{
	public static class PdxModsUtil
	{
		private static readonly PdxSdkPlatform? _pdxPlatform;
		private static readonly IContext? _context;

		public static string? CurrentPlayset { get; private set; }
		public static string? UserId { get; private set; }

		static PdxModsUtil()
		{
			// A throwing static constructor would poison the type and break every CurrentPlayset access
			try
			{
				_pdxPlatform = PlatformManager.instance.GetPSI<PdxSdkPlatform>("PdxSdk");
				_context = _pdxPlatform is null ? null : typeof(PdxSdkPlatform).GetField("m_SDKContext", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(_pdxPlatform) as IContext;
			}
			catch (Exception ex)
			{
				Mod.Log.Warn(ex, "Failed to retrieve the PDX SDK context");
			}
		}

		public static async Task Start()
		{
			if (_context is null || _pdxPlatform is null)
			{
				return;
			}

			try
			{
				var currentPlaysetResult = await _context.Mods.GetActivePlayset();
				var profileResult = await _context.Profile.Get();

				CurrentPlayset = _context.Mods.GetActivePlaysetId();
				UserId = profileResult.Social?.DisplayName;

				_pdxPlatform.onActivePlaysetChanged += PdxPlatform_onActivePlaysetChanged;
				_pdxPlatform.onLoggedIn += PdxPlatform_onLoggedIn;
			}
			catch (Exception ex)
			{
				Mod.Log.Warn(ex, "Failed to retrieve the active playset");
			}
		}

		private static async void PdxPlatform_onLoggedIn(string firstName, string lastName, string email, AccountLinkState accountLinkState, bool firstTime)
		{
			if (_context is null)
			{
				return;
			}

			try
			{
				var profileResult = await _context.Profile.Get();

				UserId = profileResult.Social?.DisplayName;
			}
			catch (Exception ex)
			{
				Mod.Log.Warn(ex, "Failed to retrieve the user profile");
			}
		}

		private static void PdxPlatform_onActivePlaysetChanged()
		{
			if (_context is null)
			{
				return;
			}

			try
			{
				CurrentPlayset = _context.Mods.GetActivePlaysetId();
			}
			catch (Exception ex)
			{
				Mod.Log.Warn(ex, "Failed to retrieve the active playset");
			}
		}
	}
}
