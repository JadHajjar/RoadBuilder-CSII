using Game.Prefabs;

using RoadBuilder.Domain.Components.Prefabs;
using RoadBuilder.Domain.Enums;

namespace RoadBuilder.LaneGroups
{
	public class SubwayGroupPrefab : BaseLaneGroupPrefab
	{
		private const string OptionName = "Two-way Support";

		public override void Initialize()
		{
			Prefab!.Options = new RoadBuilderLaneOption[]
			{
				new()
				{
					Name = OptionName,
					Type = LaneOptionType.TwoWay,
				}
			};

			Prefab.AddComponent<RoadBuilderLaneInfo>()
				.WithRequireAll(RoadCategory.Subway)
				.WithColor(66, 60, 51)
				.WithFrontThumbnail("coui://roadbuildericons/RB_SubwayFront.svg")
				.WithBackThumbnail("coui://roadbuildericons/RB_SubwayRear.svg")
				.AddLaneThumbnail("coui://roadbuildericons/Thumb_SubwayRail.svg")
				.AddLaneThumbnail("coui://roadbuildericons/Thumb_TrackLane.svg");

			Prefab.AddComponent<UIObject>().m_Icon = "coui://roadbuildericons/RB_SubwayFront.svg";

			SetUp(Sections!["Subway Track Section 4"], false);
			SetUp(Sections["Subway Track Twoway Section 4"], true);
		}

		private void SetUp(NetSectionPrefab prefab, bool value)
		{
			var laneInfo = prefab.AddComponent<RoadBuilderLaneGroup>();
			laneInfo.GroupPrefab = Prefab;
			laneInfo.Combination = value ? new LaneOptionCombination[]
			{
				new()
				{
					OptionName = OptionName,
					Value = string.Empty
				}
			} : new LaneOptionCombination[0];
		}
	}
}
