using Colossal.Json;
using Colossal.PSI.Common;

using RoadBuilder.Domain.Configuration;

using System;
using System.IO;

namespace RoadBuilder.Utilities
{
	public static class LocalSaveUtil
	{
		public static void Save(RoadConfig roadConfig)
		{
			DeleteLocalConfig(roadConfig);

			roadConfig.ID = $"{PlatformManager.instance.userSpecificPath}-{Guid.NewGuid()}";

			File.WriteAllText(Path.Combine(FoldersUtil.ContentFolder, $"{roadConfig.ID}.json"), JSON.Dump(roadConfig));
		}

		private static void DeleteLocalConfig(RoadConfig roadConfig)
		{
			var fileName = Path.Combine(FoldersUtil.ContentFolder, $"{roadConfig.ID}.json");

			if (File.Exists(fileName))
			{
				File.Delete(fileName);
			}
		}
	}
}
