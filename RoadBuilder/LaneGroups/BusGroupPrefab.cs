using Game.Prefabs;

using RoadBuilder.Domain.Components.Prefabs;
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
			Options = new RoadBuilderLaneOption[]
			{
				new()
				{
					DefaultValue = "3m",
					Type = LaneOptionType.ValueUpDown,
					Name = OptionName,
					Options = new RoadBuilderLaneOptionValue[]
					{
						new() { Value = "3m" },
						new() { Value = "4m" },
					}
				}
			};

			AddComponent<RoadBuilderLaneInfo>()
				.WithExcluded(RoadCategory.NonAsphalt)
				.WithColor(204, 83, 71, 200)
				.WithFrontThumbnail("coui://roadbuildericons/RB_BusFront.svg")
				.WithBackThumbnail("coui://roadbuildericons/RB_BusRear.svg");

			AddComponent<UIObject>().m_Icon = "coui://roadbuildericons/RB_Bus_Centered.svg";

			SetUp(sections["Public Transport Lane Section 3 - Tram Option"], "3m");
			SetUp(sections["Public Transport Lane Section 4 - Tram Option"], "4m");
		}

		private void SetUp(NetSectionPrefab prefab, string value)
		{
			var laneInfo = prefab.AddComponent<RoadBuilderLaneGroup>();
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
