using Game.Prefabs;

using RoadBuilder.Domain.Components;
using RoadBuilder.Domain.Enums;

using System.Collections.Generic;

namespace RoadBuilder.LaneGroups
{
	public class CarGroupPrefab : BaseLaneGroupPrefab
	{
		private const string OptionName1 = "Lane Width";
		private const string OptionName2 = "Transport Option";

		public CarGroupPrefab(Dictionary<string, NetSectionPrefab> sections) : base(sections)
		{
			DisplayName = "Car";
			Options = new RoadBuilderLaneOptionInfo[]
			{
				new()
				{
					DefaultValue = "3m",
					IsValue = true,
					Name = OptionName1,
					Options = new RoadBuilderLaneOptionItemInfo[]
					{
						new() { Value = "3m" },
						new() { Value = "4m" },
					}
				},
				new()
				{
					DefaultValue = "",
					Name = OptionName2,
					Options = new RoadBuilderLaneOptionItemInfo[]
					{
						new()
						{
							Value = "",
							ThumbnailUrl = "coui://roadbuildericons/RB_CarWhite.svg"
						},
						new()
						{
							Value = "Transport" ,
							ThumbnailUrl = "coui://roadbuildericons/RB_BusWhite.svg"
						},
						new()
						{
							Value = "Tram",
							ThumbnailUrl = "coui://roadbuildericons/RB_TramWhite.svg"
						},
					}
				}
			};

			AddComponent<RoadBuilderLaneInfoItem>()
				.WithExcluded(RoadCategory.NonAsphalt)
				.WithFrontThumbnail("coui://roadbuildericons/RB_CarFront.svg")
				.WithBackThumbnail("coui://roadbuildericons/RB_CarRear.svg");

			var uiObj = AddComponent<UIObject>();
			uiObj.m_Icon = "coui://roadbuildericons/RB_CarFront.svg";

			SetUp(sections["Car Drive Section 3"], "3m", "").AddOrGetComponent<RoadBuilderLaneInfoItem>().WithRequired(RoadCategory.RaisedSidewalk);
			SetUp(sections["Alley Drive Section 3"], "3m", "").AddOrGetComponent<RoadBuilderLaneInfoItem>().WithExcluded(RoadCategory.RaisedSidewalk);
			SetUp(sections["Car Drive Section 3 - Transport Option"], "3m", "Transport");
			SetUp(sections["Car Drive Section 3 - Transport Tram Option"], "3m", "Tram");
			SetUp(sections["Highway Drive Section 4"], "4m", "");
			SetUp(sections["Highway Drive Section 4 - Transport Option"], "4m", "Transport");
			SetUp(sections["Car Drive Section 4 - Transport Tram Option"], "4m", "Tram");
		}

		private NetSectionPrefab SetUp(NetSectionPrefab prefab, string value1, string value2)
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

			LinkedSections.Add(prefab);

			return prefab;
		}
	}
}
