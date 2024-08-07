using Game.Prefabs;

using RoadBuilder.Domain.Components.Prefabs;
using RoadBuilder.Domain.Enums;

using System.Collections.Generic;

namespace RoadBuilder.LaneGroups
{
	public class BusGroupPrefab : BaseLaneGroupPrefab
	{
		private const string OptionName = "Lane Width";
		private const string OptionName2 = "Transport Option";

		public override void Initialize(Dictionary<string, NetSectionPrefab> sections)
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
				},
				new()
				{
					DefaultValue = "",
					Name = OptionName2,
					Options = new RoadBuilderLaneOptionValue[]
					{
						new()
						{
							Value = "",
							ThumbnailUrl = "coui://roadbuildericons/RB_BusWhite.svg"
						},
						new()
						{
							Value = "Tram",
							ThumbnailUrl = "coui://roadbuildericons/RB_TramWhite.svg"
						},
					}
				},
			};

			AddComponent<RoadBuilderLaneInfo>()
				.WithExcluded(RoadCategory.NonAsphalt)
				.WithColor(204, 83, 71, 200)
				.WithFrontThumbnail("coui://roadbuildericons/RB_BusFront.svg")
				.WithBackThumbnail("coui://roadbuildericons/RB_BusRear.svg")
				.AddLaneThumbnail("coui://roadbuildericons/Thumb_BusLane.svg");

			AddComponent<UIObject>().m_Icon = "coui://roadbuildericons/RB_Bus_Centered.svg";

			SetUp(sections["Public Transport Lane Section 3 - Tram Option"], "3m", "Tram").AddComponent<RoadBuilderLaneInfo>().WithFrontThumbnail("coui://roadbuildericons/RB_BusTramFront.svg").WithBackThumbnail("coui://roadbuildericons/RB_BusTramRear.svg");
			SetUp(sections["Public Transport Lane Section 4 - Tram Option"], "4m", "Tram").AddComponent<RoadBuilderLaneInfo>().WithFrontThumbnail("coui://roadbuildericons/RB_BusTramFront.svg").WithBackThumbnail("coui://roadbuildericons/RB_BusTramRear.svg");
			SetUp(sections["RB Public Transport Lane Section 3"], "3m", "");
			SetUp(sections["RB Public Transport Lane Section 4"], "4m", "");
		}

		private NetSectionPrefab SetUp(NetSectionPrefab prefab, string value, string value2)
		{
			var laneInfo = prefab.AddComponent<RoadBuilderLaneGroup>();
			laneInfo.GroupPrefab = this;
			laneInfo.Combination = new LaneOptionCombination[]
			{
				new()
				{
					OptionName = OptionName,
					Value = value
				},
				new()
				{
					OptionName = OptionName2,
					Value = value2
				}
			};

			LinkedSections.Add(prefab);

			return prefab;
		}
	}
}
