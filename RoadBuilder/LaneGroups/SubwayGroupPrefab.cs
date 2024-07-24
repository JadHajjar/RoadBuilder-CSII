using Game.Prefabs;

using RoadBuilder.Domain.Components.Prefabs;
using RoadBuilder.Domain.Enums;

using System.Collections.Generic;

namespace RoadBuilder.LaneGroups
{
	public class SubwayGroupPrefab : BaseLaneGroupPrefab
	{
		private const string OptionName = "Two-way Support";

		public override void Initialize(Dictionary<string, NetSectionPrefab> sections)
		{
			DisplayName = "Subway Track";
			Options = new RoadBuilderLaneOption[]
			{
				new()
				{
					Name = OptionName,
					Type = LaneOptionType.TwoWay,
				}
			};

			AddComponent<RoadBuilderLaneInfo>()
				.WithExcluded(RoadCategory.Gravel | RoadCategory.Tiled | RoadCategory.Fence | RoadCategory.Pathway)
				.WithColor(66, 60, 51)
				.WithFrontThumbnail("coui://roadbuildericons/RB_SubwayFront.svg")
				.WithBackThumbnail("coui://roadbuildericons/RB_SubwayRear.svg");

			AddComponent<UIObject>().m_Icon = "coui://roadbuildericons/RB_SubwayFront.svg";

			SetUp(sections["Subway Track Section 4"], false);
			SetUp(sections["Subway Track Twoway Section 4"], true);
		}

		private void SetUp(NetSectionPrefab prefab, bool value)
		{
			var laneInfo = prefab.AddComponent<RoadBuilderLaneGroup>();
			laneInfo.GroupPrefab = this;
			laneInfo.Combination = value ? new LaneOptionCombination[]
			{
				new()
				{
					OptionName = OptionName,
					Value = string.Empty
				}
			} : new LaneOptionCombination[0];

			LinkedSections.Add(prefab);
		}
	}
}
