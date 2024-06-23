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
			File.WriteAllText(Path.Combine(FoldersUtil.ContentFolder, $"{roadConfig.ID}.json"), JSON.Dump(roadConfig));
		}

		public static void DeleteLocalConfig(string configID)
		{
			var fileName = Path.Combine(FoldersUtil.ContentFolder, $"{configID}.json");

			if (File.Exists(fileName))
			{
				File.Delete(fileName);
			}
		}
	}
}
