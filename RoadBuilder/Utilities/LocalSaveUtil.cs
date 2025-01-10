using Colossal.Json;

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

			config.Version = RoadBuilderSerializeSystem.CURRENT_VERSION;
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
					if (LoadFromJson(File.ReadAllText(item)) is INetworkConfig config)
					{
						list.Add(config);
					}
				}
				catch (Exception ex)
				{
					Mod.Log.Warn(ex, "Failed to load the configuration file: " + item);

					continue;
				}
			}

			return list;
		}

		public static INetworkConfig? LoadFromJson(string data)
		{
			var json = JSON.Load(data);

			switch (json["Type"]?.ToString())
			{
				case nameof(RoadConfig):
					return JSON.MakeInto<RoadConfig>(json);
				case nameof(TrackConfig):
					return JSON.MakeInto<TrackConfig>(json);
				case nameof(FenceConfig):
					return JSON.MakeInto<FenceConfig>(json);
				case nameof(PathConfig):
					return JSON.MakeInto<PathConfig>(json);
				default:
					Mod.Log.Warn("Unsupported configuration type: " + json["Type"]?.ToString());
					return null;
			}
		}
	}
}
