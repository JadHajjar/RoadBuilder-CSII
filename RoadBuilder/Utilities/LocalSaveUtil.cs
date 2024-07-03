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
		public static void Save(INetworkConfig roadConfig)
		{
			DeletePreviousLocalConfig(roadConfig);

			roadConfig.Version = RoadBuilderSystem.CURRENT_VERSION;
			roadConfig.OriginalID = roadConfig.ID;

			File.WriteAllText(Path.Combine(FoldersUtil.ContentFolder, $"{roadConfig.ID}.json"), JSON.Dump(roadConfig));
		}

		public static void DeletePreviousLocalConfig(INetworkConfig roadConfig)
		{
			var fileName = Path.Combine(FoldersUtil.ContentFolder, $"{roadConfig.OriginalID}.json");

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
					switch (Path.GetFileName(item)[0])
					{
						case 'r':
							list.Add(JSON.MakeInto<RoadConfig>(JSON.Load(File.ReadAllText(item))));
							break;
						case 't':
							list.Add(JSON.MakeInto<TrackConfig>(JSON.Load(File.ReadAllText(item))));
							break;
						case 'f':
							list.Add(JSON.MakeInto<FenceConfig>(JSON.Load(File.ReadAllText(item))));
							break;
						default:
							throw new Exception("file name is invalid");
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
