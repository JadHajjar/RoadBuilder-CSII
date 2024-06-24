using Colossal.Json;

using RoadBuilder.Domain.Configuration;
using RoadBuilder.Systems;

using System;
using System.Collections.Generic;
using System.IO;

namespace RoadBuilder.Utilities
{
	public static class LocalSaveUtil
	{
		public static void Save(RoadConfig roadConfig)
		{
			DeletePreviousLocalConfig(roadConfig);

			roadConfig.Version = RoadBuilderSystem.CURRENT_VERSION;
			roadConfig.OriginalID = roadConfig.ID;

			File.WriteAllText(Path.Combine(FoldersUtil.ContentFolder, $"{roadConfig.ID}.json"), JSON.Dump(roadConfig));
		}

		public static void DeletePreviousLocalConfig(RoadConfig roadConfig)
		{
			var fileName = Path.Combine(FoldersUtil.ContentFolder, $"{roadConfig.OriginalID}.json");

			if (File.Exists(fileName))
			{
				File.Delete(fileName);
			}
		}

		internal static IEnumerable<RoadConfig> LoadConfigs()
		{
			if (!Directory.Exists(FoldersUtil.ContentFolder))
			{
				yield break;
			}

			foreach (var item in Directory.EnumerateFiles(FoldersUtil.ContentFolder, "*.json"))
			{
				RoadConfig roadConfig;

				try
				{
					roadConfig = JSON.MakeInto<RoadConfig>(JSON.Load(File.ReadAllText(item)));
				}
				catch (Exception ex)
				{
					Mod.Log.Error(ex, "Failed to load the configuration file: " + item);

					continue;
				}

				yield return roadConfig;
			}
		}
	}
}
