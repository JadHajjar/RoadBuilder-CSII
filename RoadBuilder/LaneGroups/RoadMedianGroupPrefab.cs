using Game.Prefabs;

using RoadBuilder.Domain.Components;

using System.Collections.Generic;

namespace RoadBuilder.LaneGroups
{
	public class RoadMedianGroupPrefab : BaseLaneGroupPrefab
	{
		private const string OptionName = "Median Width";

		public RoadMedianGroupPrefab(Dictionary<string, NetSectionPrefab> sections) : base(sections)
		{
			DisplayName = "Median";
			Options = new RoadBuilderLaneOptionInfo[]
			{
				new()
				{
					DefaultValue = "2m",
					IsValue = true,
					Name = OptionName,
					Options = new RoadBuilderLaneOptionItemInfo[]
					{
						new() { Value = "0m" },
						new() { Value = "1m" },
						new() { Value = "2m" },
						new() { Value = "5m" }
					}
				}
			};

			var laneInfo = AddComponent<RoadBuilderLaneInfoItem>();
			laneInfo.ExcludedCategories = Domain.Enums.RoadCategory.NonRoad;

			var uiObj = AddComponent<UIObject>();
			uiObj.m_Icon = "coui://roadbuildericons/RB_Median.svg";

			SetUp(sections["Road Median 0"], "0m");
			SetUp(sections["Road Median 1"], "1m");
			SetUp(sections["Road Median 2"], "2m");
			SetUp(sections["Road Median 5"], "5m");
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
