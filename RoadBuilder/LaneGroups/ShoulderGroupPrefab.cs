using Game.Prefabs;

using RoadBuilder.Domain.Components;
using RoadBuilder.Domain.Enums;

using System.Collections.Generic;

namespace RoadBuilder.LaneGroups
{
	public class ShoulderGroupPrefab : BaseLaneGroupPrefab
	{
		private const string OptionName1 = "Lane Width";

		public ShoulderGroupPrefab(Dictionary<string, NetSectionPrefab> sections) : base(sections)
		{
			DisplayName = "Shoulder";
			Options = new RoadBuilderLaneOptionInfo[]
			{
				new()
				{
					DefaultValue = "1m",
					IsValue = true,
					Name = OptionName1,
					Options = new RoadBuilderLaneOptionItemInfo[]
					{
						new() { Value = "1m" },
						new() { Value = "2m" },
					}
				},
			};

			AddComponent<RoadBuilderLaneInfoItem>().WithExcluded(RoadCategory.RaisedSidewalk);

			var uiObj = AddComponent<UIObject>();
			uiObj.m_Icon = "coui://roadbuildericons/RB_Empty.svg";

			SetUp(sections["Alley Shoulder 1"], "1m").WithExcluded(RoadCategory.Train | RoadCategory.Tram | RoadCategory.Subway | RoadCategory.Tiled | RoadCategory.Gravel);
			SetUp(sections["Highway Shoulder 2"], "2m").WithExcluded(RoadCategory.Train | RoadCategory.Tram | RoadCategory.Subway | RoadCategory.PublicTransport | RoadCategory.Tiled | RoadCategory.Gravel);
			SetUp(sections["Public Transport Shoulder 1"], "1m").WithRequired(RoadCategory.PublicTransport);
			SetUp(sections["Gravel Shoulder 1"], "1m").WithRequired(RoadCategory.Gravel);
			SetUp(sections["Tiled Shoulder 1"], "1m").WithRequired(RoadCategory.Tiled);
			SetUp(sections["Subway Shoulder 2"], "2m").WithRequired(RoadCategory.Subway);
			SetUp(sections["Train Shoulder 2"], "2m").WithRequired(RoadCategory.Train);
			SetUp(sections["Tram Shoulder 1"], "1m").WithRequired(RoadCategory.Tram);
		}

		private RoadBuilderLaneInfoItem SetUp(NetSectionPrefab prefab, string value1)
		{
			var laneInfo = prefab.AddComponent<RoadBuilderLaneGroupItem>();
			laneInfo.GroupPrefab = this;
			laneInfo.Combination = new LaneOptionCombination[]
			{
				new()
				{
					OptionName = OptionName1,
					Value = value1
				},
			};

			LinkedSections.Add(prefab);

			return prefab.AddOrGetComponent<RoadBuilderLaneInfoItem>();
		}
	}
}
