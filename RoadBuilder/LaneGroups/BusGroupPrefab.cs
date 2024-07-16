using Game.Prefabs;

using RoadBuilder.Domain.Components;
using RoadBuilder.Domain.Enums;

using System.Collections.Generic;

namespace RoadBuilder.LaneGroups
{
	public class BusGroupPrefab : BaseLaneGroupPrefab
	{
		private const string OptionName = "Lane Width";

		public BusGroupPrefab(Dictionary<string, NetSectionPrefab> sections) : base(sections)
		{
			DisplayName = "Bus";
			Options = new RoadBuilderLaneOptionInfo[]
			{
				new()
				{
					DefaultValue = "3m",
					IsValue = true,
					Name = OptionName,
					Options = new RoadBuilderLaneOptionItemInfo[]
					{
						new() { Value = "3m" },
						new() { Value = "4m" },
					}
				}
			};

			AddComponent<RoadBuilderLaneInfoItem>()
				.WithExcluded(RoadCategory.NonAsphalt)
				.WithFrontThumbnail("coui://roadbuildericons/RB_BusFront.svg")
				.WithBackThumbnail("coui://roadbuildericons/RB_BusRear.svg");

			var uiObj = AddComponent<UIObject>();
			uiObj.m_Icon = "coui://roadbuildericons/RB_BusFront.svg";

			SetUp(sections["Public Transport Lane Section 3 - Tram Option"], "3m");
			SetUp(sections["Public Transport Lane Section 4 - Tram Option"], "4m");
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
