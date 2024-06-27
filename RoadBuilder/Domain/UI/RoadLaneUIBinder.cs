using RoadBuilder.Domain.Configuration;

using System.Linq;

namespace RoadBuilder.Domain.UI
{
	public class RoadLaneUIBinder
	{
		public string SectionPrefabName;
		public bool Invert;

		public static RoadLaneUIBinder[] From(RoadConfig config)
		{
			return config.Lanes.Select(x => new RoadLaneUIBinder
			{
				SectionPrefabName = x.SectionPrefabName,
				Invert = x.Invert
			}).ToArray();
		}

		public LaneConfig ToLaneConfig()
		{
			return new LaneConfig
			{
				SectionPrefabName = SectionPrefabName,
				Invert = Invert
			};
		}
	}
}
