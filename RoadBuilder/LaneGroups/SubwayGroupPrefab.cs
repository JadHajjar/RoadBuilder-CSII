using Game.Prefabs;

using RoadBuilder.Domain.Components;
using RoadBuilder.Domain.Enums;

using System.Collections.Generic;

namespace RoadBuilder.LaneGroups
{
	public class SubwayGroupPrefab : BaseLaneGroupPrefab
	{
		private const string OptionName = "Two-way Support";

		public SubwayGroupPrefab(Dictionary<string, NetSectionPrefab> sections) : base(sections)
		{
			DisplayName = "Subway Track";
			Options = new RoadBuilderLaneOptionInfo[]
			{
				new()
				{
					DefaultValue = "1",
					Name = OptionName,
					Options = new RoadBuilderLaneOptionItemInfo[]
					{
						new() { Value = "1" },
						new() { Value = "2" },
					}
				}
			};

			AddComponent<RoadBuilderLaneInfoItem>()
				.WithExcluded(RoadCategory.Gravel | RoadCategory.Tiled | RoadCategory.Pathway | RoadCategory.Fence)
				.WithFrontThumbnail("coui://roadbuildericons/RB_SubwayFront.svg")
				.WithBackThumbnail("coui://roadbuildericons/RB_SubwayRear.svg");

			var uiObj = AddComponent<UIObject>();
			uiObj.m_Icon = "coui://roadbuildericons/RB_SubwayFront.svg";

			SetUp(sections["Subway Track Section 4"], "1");
			SetUp(sections["Subway Track Twoway Section 4"], "2");
		}

		private void SetUp(NetSectionPrefab prefab, string value)
		{
			var laneInfo = prefab.AddComponent<RoadBuilderLaneGroupItem>();
			laneInfo.GroupPrefab = this;
			laneInfo.Combination = new LaneOptionCombination[]
			{
				new()
				{
					OptionName = OptionName,
					Value = value
				}
			};

			LinkedSections.Add(prefab);
		}
	}
}
