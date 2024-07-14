using Game.Prefabs;

using RoadBuilder.Domain.Components;

using System.Collections.Generic;

namespace RoadBuilder.LaneGroups
{
	public class RoadSidewalkGroupPrefab : BaseLaneGroupPrefab
	{
		private const string OptionName1 = "Parking";
		private const string OptionName2 = "Width";

		public RoadSidewalkGroupPrefab(Dictionary<string, NetSectionPrefab> sections) : base(sections)
		{
			DisplayName = "Sidewalk";
			Options = new RoadBuilderLaneOptionInfo[]
			{
				new()
				{
					DefaultValue = "",
					Name = OptionName1,
					Options = new RoadBuilderLaneOptionItemInfo[]
					{
						new()
						{
							Value = "",
							ThumbnailUrl = "coui://roadbuildericons/RB_WideSidewalkRight.svg"
						},
						new()
						{
							Value = "P" ,
							ThumbnailUrl = "coui://roadbuildericons/RB_SidewalkRightwParking.svg"
						},
					}
				},
				new()
				{
					DefaultValue = "3.5",
					IsValue = true,
					Name = OptionName2,
					Options = new RoadBuilderLaneOptionItemInfo[]
					{
						new() { Value = "3.5m" },
						new() { Value = "4.5m" },
						new() { Value = "5m" },
						new() { Value = "5.5m" },
						new() { Value = "6m" },
						new() { Value = "7m" },
					}
				}
			};

			var laneInfo = AddComponent<RoadBuilderLaneInfoItem>();
			laneInfo.ExcludedCategories = Domain.Enums.RoadCategory.NonRoad;

			var uiObj = AddComponent<UIObject>();
			uiObj.m_Icon = "coui://roadbuildericons/RB_WideSidewalkRight.svg";

			SetUp(sections["Sidewalk With Parking 5"], "P", "5m");
			SetUp(sections["Sidewalk 3.5"], "", "3.5m");
			SetUp(sections["Sidewalk With Parking 6"], "P", "6m");
			SetUp(sections["Sidewalk 4.5"], "", "4.5m");
			SetUp(sections["Sidewalk With Parking 7"], "P", "7m");
			SetUp(sections["Sidewalk With Parking 5.5"], "P", "5.5m");
			SetUp(sections["Sidewalk 5"], "", "5m");
			SetUp(sections["Sidewalk 7"], "", "7m");
			SetUp(sections["Sidewalk 6"], "", "6m");
			SetUp(sections["Sidewalk 5.5"], "", "5.5m");
		}

		private void SetUp(NetSectionPrefab prefab, string value1, string value2)
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
				new()
				{
					OptionName = OptionName2,
					Value = value2
				}
			};

			var uiObj = prefab.AddOrGetComponent<UIObject>();
			uiObj.m_Icon = value1 == "" ? "coui://roadbuildericons/RB_WideSidewalkRight.svg" : "coui://roadbuildericons/RB_SidewalkRightwParking.svg";

			LinkedSections.Add(prefab);
		}
	}
}
