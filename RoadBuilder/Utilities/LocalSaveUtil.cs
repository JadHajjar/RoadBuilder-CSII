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
			Mod.Log.Debug($"LocalSaveUtil.Save: {config.GetType().Name} id={config.ID} name={config.Name}");

			if (!IsSafeFileName(config.ID))
			{
				throw new InvalidOperationException("Invalid configuration ID: " + config.ID);
			}

			var previousId = config.OriginalID;
			var fileName = Path.Combine(FoldersUtil.ContentFolder, $"{config.ID}.json");
			var tempFileName = fileName + ".tmp";

			config.Version = RoadBuilderSerializeSystem.CURRENT_VERSION;
			config.OriginalID = config.ID;
			config.Type = config.GetType().Name;

			try
			{
				// Written to a temp file first so a failed write never loses the existing config
				File.WriteAllText(tempFileName, JSON.Dump(config));

				if (File.Exists(fileName))
				{
					File.Delete(fileName);
				}

				File.Move(tempFileName, fileName);
			}
			catch
			{
				config.OriginalID = previousId;

				throw;
			}

			if (previousId != config.ID)
			{
				DeleteLocalConfig(previousId);
			}
		}

		public static void DeletePreviousLocalConfig(INetworkConfig config)
		{
			DeleteLocalConfig(config.OriginalID);
		}

		private static void DeleteLocalConfig(string? id)
		{
			if (!IsSafeFileName(id))
			{
				return;
			}

			var fileName = Path.Combine(FoldersUtil.ContentFolder, $"{id}.json");

			if (File.Exists(fileName))
			{
				File.Delete(fileName);
			}
		}

		// IDs can come from online configs, they must never resolve outside of the target folder
		public static bool IsSafeFileName(string? id)
		{
			return !string.IsNullOrWhiteSpace(id)
				&& id!.IndexOfAny(Path.GetInvalidFileNameChars()) < 0
				&& !id.Contains("..");
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
