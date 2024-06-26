using RoadBuilder.Domain.Configuration;

using System.Linq;

namespace RoadBuilder.Domain.UI
{
	public class RoadLaneUIBinder
	{
		public string SectionPrefabName;

		public static RoadLaneUIBinder[] From(RoadConfig config)
		{
			return config.Lanes.Select(x => new RoadLaneUIBinder
			{
				SectionPrefabName = x.SectionPrefabName,
			}).ToArray();
		}

		public LaneConfig ToLaneConfig()
		{
			return new LaneConfig
			{
				SectionPrefabName = SectionPrefabName,
			};
		}
	}
}
