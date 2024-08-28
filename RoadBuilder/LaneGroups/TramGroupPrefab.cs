using Game.Prefabs;

using RoadBuilder.Domain.Components.Prefabs;
using RoadBuilder.Domain.Enums;

using System.Collections.Generic;

namespace RoadBuilder.LaneGroups
{
	public class TramGroupPrefab : BaseLaneGroupPrefab
	{
		public override void Initialize(Dictionary<string, NetSectionPrefab> sections)
		{
			Options = new RoadBuilderLaneOption[0];

			AddComponent<RoadBuilderLaneInfo>()
				.WithRequireNone(RoadCategory.Gravel | RoadCategory.Pathway | RoadCategory.Fence | RoadCategory.Subway)
				.WithFrontThumbnail("coui://roadbuildericons/RB_TramFront.svg")
				.WithBackThumbnail("coui://roadbuildericons/RB_TramRear.svg")
				.AddLaneThumbnail("coui://roadbuildericons/Thumb_TramLane.svg");

			AddComponent<UIObject>().m_Icon = "coui://roadbuildericons/RB_TramFront.svg";

			var laneInfo1 = sections["Tram Track Section 3"].AddComponent<RoadBuilderLaneGroup>();
			laneInfo1.GroupPrefab = this;
			laneInfo1.Combination = new LaneOptionCombination[0];

			var laneInfo2 = sections["RB Tiled Tram Section 3"].AddComponent<RoadBuilderLaneGroup>();
			laneInfo2.GroupPrefab = this;
			laneInfo2.Combination = new LaneOptionCombination[0];

			sections["Tram Track Section 3"].AddComponent<RoadBuilderLaneInfo>().WithRequireNone(RoadCategory.Tiled).WithGroundTexture(LaneGroundType.Asphalt);
			sections["RB Tiled Tram Section 3"].AddComponent<RoadBuilderLaneInfo>().WithRequireAll(RoadCategory.Tiled).AddLaneThumbnail("coui://roadbuildericons/Thumb_TiledTramLane.svg").WithGroundTexture(LaneGroundType.Tiled);
		}
	}
}
