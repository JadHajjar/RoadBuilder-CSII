using Colossal.PSI.Environment;

using System.IO;

namespace RoadBuilder.Utilities
{
	public static class FoldersUtil
	{
		public static string ContentFolder { get; }
		public static string SettingsFolder { get; }
		public static string TempFolder { get; }
		public static string GameUIPath { get; }

		static FoldersUtil()
		{
			ContentFolder = Path.Combine(EnvPath.kUserDataPath, "ModsData", nameof(RoadBuilder));

			SettingsFolder = Path.Combine(EnvPath.kUserDataPath, "ModsSettings", nameof(RoadBuilder));

			TempFolder = Path.Combine(EnvPath.kTempDataPath, nameof(RoadBuilder));

			GameUIPath = Path.Combine(EnvPath.kStreamingDataPath, "~UI~", "GameUI");

			if (Directory.Exists(TempFolder))
			{
				new DirectoryInfo(TempFolder).Delete(true);
			}

			Directory.CreateDirectory(ContentFolder);
			Directory.CreateDirectory(TempFolder);
		}
	}
}
