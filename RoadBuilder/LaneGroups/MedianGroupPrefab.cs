using Game.Prefabs;

using RoadBuilder.Domain.Components;
using RoadBuilder.Domain.Enums;

using System.Collections.Generic;

namespace RoadBuilder.LaneGroups
{
	public class MedianGroupPrefab : BaseLaneGroupPrefab
	{
		private const string OptionName = "Median Width";

		public MedianGroupPrefab(Dictionary<string, NetSectionPrefab> sections) : base(sections)
		{
			DisplayName = "Median";
			Options = new RoadBuilderLaneOptionInfo[]
			{
				new()
				{
					Name = "Decoration",
					IsDecoration = true,
				},
				new()
				{
					DefaultValue = "2m",
					IsValue = true,
					Name = OptionName,
					Options = new RoadBuilderLaneOptionItemInfo[]
					{
						new() { Value = "1m" },
						new() { Value = "2m" },
						new() { Value = "5m" }
					}
				}
			};

			AddComponent<RoadBuilderLaneInfoItem>().WithExcluded(RoadCategory.NoRaisedSidewalkSupport);

			var uiObj = AddComponent<UIObject>();
			uiObj.m_Icon = "coui://roadbuildericons/RB_Median.svg";

			SetUp(sections["Road Median 1"], "1m");
			SetUp(sections["Road Median 2"], "2m");
			SetUp(sections["Road Median 5"], "5m", true);			
		}

		private void SetUp(NetSectionPrefab prefab, string value, bool hasGrass = false)
		{
			var laneInfo = prefab.AddComponent<RoadBuilderLaneGroupItem>();
			laneInfo.GroupPrefab = this;
			laneInfo.Combination = hasGrass
			? new LaneOptionCombination[]
			{
				new()
				{
					OptionName = "Decoration",
					Value = string.Empty
				},
				new()
				{
					OptionName = OptionName,
					Value = value
				}
			}
			: new LaneOptionCombination[]
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
