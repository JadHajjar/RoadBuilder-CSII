using Game.Prefabs;

using RoadBuilder.Domain.Components.Prefabs;
using RoadBuilder.Domain.Enums;

using System.Collections.Generic;

namespace RoadBuilder.LaneGroups
{
	public class SubwayMedianGroupPrefab : BaseLaneGroupPrefab
	{
		private const string OptionName = "Style";

		public override void Initialize(Dictionary<string, NetSectionPrefab> sections)
		{
			Options = new RoadBuilderLaneOption[]
			{
				new()
				{
					Name = OptionName,
					Type = LaneOptionType.SingleSelectionButtons,
					DefaultValue = "Raised",
					Options = new RoadBuilderLaneOptionValue[]
					{
						new()
						{
							Value = "Raised",
							ThumbnailUrl = "coui://roadbuildericons/RB_RaisedCenterPlatformWhite.svg"
						},
						new()
						{
							Value = "Flat",
							ThumbnailUrl = "coui://roadbuildericons/RB_CenterPlatformWhite.svg"
						}
					}
				}
			};

			AddComponent<RoadBuilderLaneInfo>()
				.WithRequireNone(RoadCategory.Tiled | RoadCategory.Fence | RoadCategory.Pathway)
				.WithGroundTexture(LaneGroundType.Train)
				.WithColor(82, 62, 51);

			AddComponent<UIObject>().m_Icon = "coui://roadbuildericons/RB_CenterPlatform.svg";

			SetUp(sections["RB Subway Median 8"], "Raised", true).AddComponent<RoadBuilderLaneInfo>().WithThumbnail("coui://roadbuildericons/RB_CenterPlatform.svg").AddLaneThumbnail("coui://roadbuildericons/Thumb_Platform.svg");
			SetUp(sections["RB Subway Median 8 - Plain"], "Flat", true).AddComponent<RoadBuilderLaneInfo>().WithThumbnail("coui://roadbuildericons/RB_Empty.svg").AddLaneThumbnail("coui://roadbuildericons/Thumb_PlatformEmpty.svg").WithRequireAny(RoadCategory.Train | RoadCategory.Subway);

			SetUp(sections["Subway Median 8"], "Raised", false);
			SetUp(sections["Subway Median 8 - Plain"], "Flat", false);
		}

		private NetSectionPrefab SetUp(NetSectionPrefab prefab, string value, bool link)
		{
			var laneInfo = link ? prefab.AddComponent<RoadBuilderLaneGroup>() : prefab.AddComponent<RoadBuilderVanillaLaneGroup>();
			laneInfo.GroupPrefab = this;
			laneInfo.Combination = new LaneOptionCombination[]
			{
				new()
				{
					OptionName = OptionName,
					Value = value
				}
			};

			return prefab;
		}
	}
}
