using Colossal.Json;

using RoadBuilder.Domain.Configuration;
using RoadBuilder.Domain.Configurations;
using RoadBuilder.Systems;

using System;
using System.Collections.Generic;
using System.IO;

namespace RoadBuilder.Utilities
{
	public static class LocalSaveUtil
	{
		public static void Save(INetworkConfig config)
		{
			DeletePreviousLocalConfig(config);

			config.Version = RoadBuilderSystem.CURRENT_VERSION;
			config.OriginalID = config.ID;
			config.Type = config.GetType().Name;

			File.WriteAllText(Path.Combine(FoldersUtil.ContentFolder, $"{config.ID}.json"), JSON.Dump(config));
		}

		public static void DeletePreviousLocalConfig(INetworkConfig config)
		{
			var fileName = Path.Combine(FoldersUtil.ContentFolder, $"{config.OriginalID}.json");

			if (File.Exists(fileName))
			{
				File.Delete(fileName);
			}
		}

		internal static IEnumerable<INetworkConfig> LoadConfigs()
		{
			var list = new List<INetworkConfig>();

			if (!Directory.Exists(FoldersUtil.ContentFolder))
			{
				return list;
			}

			foreach (var item in Directory.EnumerateFiles(FoldersUtil.ContentFolder, "*.json"))
			{
				try
				{
					var json = JSON.Load(File.ReadAllText(item));

					switch (json["Type"].ToString())
					{
						case nameof(RoadConfig):
							list.Add(JSON.MakeInto<RoadConfig>(json));
							break;
						case nameof(TrackConfig):
							list.Add(JSON.MakeInto<TrackConfig>(json));
							break;
						case nameof(FenceConfig):
							list.Add(JSON.MakeInto<FenceConfig>(json));
							break;
						default:
							break;
					}
				}
				catch (Exception ex)
				{
					Mod.Log.Error(ex, "Failed to load the configuration file: " + item);

					continue;
				}
			}

			return list;
		}
	}
}
