using Game.Prefabs;

using RoadBuilder.Domain.Components.Prefabs;
using RoadBuilder.Domain.Enums;

namespace RoadBuilder.LaneGroups
{
	public class TramGroupPrefab : BaseLaneGroupPrefab
	{
		public override bool Initialize()
		{
			Prefab!.Options = new RoadBuilderLaneOption[0];

			Prefab.AddOrGetComponent<RoadBuilderLaneInfo>()
				.WithRequireNone(RoadCategory.Gravel | RoadCategory.Pathway | RoadCategory.Fence | RoadCategory.Subway)
				.WithFrontThumbnail("coui://roadbuildericons/RB_TramFront.svg")
				.WithBackThumbnail("coui://roadbuildericons/RB_TramRear.svg")
				.AddLaneThumbnail("coui://roadbuildericons/Thumb_TramLane.svg");

			Prefab.AddOrGetComponent<UIObject>().m_Icon = "coui://roadbuildericons/RB_TramFront.svg";

			var laneInfo1 = Sections!["Tram Track Section 3"].AddOrGetComponent<RoadBuilderLaneGroup>();
			laneInfo1.GroupPrefab = Prefab;
			laneInfo1.Combination = new LaneOptionCombination[0];

			var laneInfo2 = Sections["RB Tiled Tram Section 3"].AddOrGetComponent<RoadBuilderLaneGroup>();
			laneInfo2.GroupPrefab = Prefab;
			laneInfo2.Combination = new LaneOptionCombination[0];

			Sections["Tram Track Section 3"].AddOrGetComponent<RoadBuilderLaneInfo>().WithRequireNone(RoadCategory.Tiled).WithGroundTexture(LaneGroundType.Asphalt);
			Sections["RB Tiled Tram Section 3"].AddOrGetComponent<RoadBuilderLaneInfo>().WithRequireAll(RoadCategory.Tiled).AddLaneThumbnail("coui://roadbuildericons/Thumb_TiledTramLane.svg").WithGroundTexture(LaneGroundType.Tiled);

			return true;
		}
	}
}
