using Game.Prefabs;

using RoadBuilder.Domain.Components;

using System.Collections.Generic;

namespace RoadBuilder.LaneGroups
{
	public class HighwayCarGroupPrefab : BaseLaneGroupPrefab
	{
		private const string OptionName = "Transport Option";

		public HighwayCarGroupPrefab(Dictionary<string, NetSectionPrefab> sections) : base(sections)
		{
			DisplayName = "Car";
			Options = new RoadBuilderLaneOptionInfo[]
			{
				new()
				{
					DefaultValue = "",
					Name = OptionName,
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
					}
				}
			};

			var laneInfo = AddComponent<RoadBuilderLaneInfoItem>();
			laneInfo.RequiredCategories = Domain.Enums.RoadCategory.Highway;
			laneInfo.FrontThumbnail = "coui://roadbuildericons/RB_CarFront.svg";
			laneInfo.BackThumbnail = "coui://roadbuildericons/RB_CarRear.svg";

			var uiObj = AddComponent<UIObject>();
			uiObj.m_Icon = "coui://roadbuildericons/RB_CarFront.svg";

			SetUp(sections["Highway Drive Section 4"], "");
			SetUp(sections["Highway Drive Section 4 - Transport Option"], "Transport");
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
