using Game.Prefabs;

using RoadBuilder.Domain.Components.Prefabs;
using RoadBuilder.Domain.Enums;

using System.Collections.Generic;

namespace RoadBuilder.LaneGroups
{
	public class TrainGroupPrefab : BaseLaneGroupPrefab
	{
		private const string OptionName = "Two-way Support";

		public override void Initialize()
		{
			Prefab.Options = new RoadBuilderLaneOption[]
			{
				new()
				{
					Name = OptionName,
					Type = LaneOptionType.TwoWay,
				}
			};

			Prefab.AddComponent<RoadBuilderLaneInfo>()
				.WithRequireNone(RoadCategory.Gravel | RoadCategory.Tiled | RoadCategory.Fence | RoadCategory.Pathway | RoadCategory.Subway)
				.WithColor(82, 62, 51)
				.WithFrontThumbnail("coui://roadbuildericons/RB_TrainFront.svg")
				.WithBackThumbnail("coui://roadbuildericons/RB_TrainRear.svg")
				.AddLaneThumbnail("coui://roadbuildericons/Thumb_TrackLane.svg");

			Prefab.AddComponent<UIObject>().m_Icon = "coui://roadbuildericons/RB_TrainFront.svg";

			SetUp(Sections["Train Track Section 4"], false);
			SetUp(Sections["Train Track Twoway Section 4"], true);
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
