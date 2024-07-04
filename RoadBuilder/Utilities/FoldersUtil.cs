using Colossal.PSI.Environment;

using System.IO;

namespace RoadBuilder.Utilities
{
	public static class FoldersUtil
	{
		public static string ContentFolder { get; }
		public static string SettingsFolder { get; }
		public static string TempFolder { get; }

		static FoldersUtil()
		{
			ContentFolder = Path.Combine(EnvPath.kUserDataPath, "ModsData", nameof(RoadBuilder));

			SettingsFolder = Path.Combine(EnvPath.kUserDataPath, "ModsSettings", nameof(RoadBuilder));

			TempFolder = Path.Combine(EnvPath.kTempDataPath, nameof(RoadBuilder));

			Directory.CreateDirectory(ContentFolder);   // Only create those
			Directory.CreateDirectory(SettingsFolder);  // folders if you
			Directory.CreateDirectory(TempFolder);      // need them
		}
	}
}
