using Game.Prefabs;

using RoadBuilder.Domain.Components.Prefabs;
using RoadBuilder.Domain.Enums;

using System.Collections.Generic;

namespace RoadBuilder.LaneGroups
{
	public class SidewalkGroupPrefab : BaseLaneGroupPrefab
	{
		private const string OptionName1 = "Parking";
		private const string OptionName2 = "Width";

		public override void Initialize(Dictionary<string, NetSectionPrefab> sections)
		{
			DisplayName = "Sidewalk";
			Options = new RoadBuilderLaneOption[]
			{
				new()
				{
					DefaultValue = "3.5m",
					Type = LaneOptionType.ValueUpDown,
					Name = OptionName2,
					Options = new RoadBuilderLaneOptionValue[]
					{
						new() { Value = "3.5m" },
						new() { Value = "4.5m" },
						new() { Value = "5m" },
						new() { Value = "5.5m" },
						new() { Value = "6m" },
						new() { Value = "7m" },
					}
				},
				new()
				{
					DefaultValue = "",
					Name = OptionName1,
					Options = new RoadBuilderLaneOptionValue[]
					{
						new()
						{
							Value = "",
							ThumbnailUrl = "coui://roadbuildericons/RB_NoParking.svg"
						},
						new()
						{
							Value = "P" ,
							ThumbnailUrl = "coui://roadbuildericons/RB_YesParking.svg"
						},
					}
				},
				new()
				{
					Name = "Decoration",
					Type = LaneOptionType.Decoration,
				},
			};

			AddComponent<RoadBuilderLaneInfo>()
				.WithRequired(RoadCategory.RaisedSidewalk)
				.AddLaneThumbnail("coui://roadbuildericons/Thumb_SidewalkWide.svg");

			AddComponent<UIObject>().m_Icon = "coui://roadbuildericons/RB_WideSidewalkRight.svg";

			SetUp(sections["Sidewalk With Parking 5"], "P", "5m");
			SetUp(sections["Sidewalk 3.5"], "", "3.5m", false);
			SetUp(sections["Sidewalk With Parking 6"], "P", "6m");
			SetUp(sections["Sidewalk 4.5"], "", "4.5m", false);
			SetUp(sections["Sidewalk With Parking 7"], "P", "7m");
			SetUp(sections["Sidewalk With Parking 5.5"], "P", "5.5m");
			SetUp(sections["Sidewalk 5"], "", "5m");
			SetUp(sections["Sidewalk 7"], "", "7m");
			SetUp(sections["Sidewalk 6"], "", "6m");
			SetUp(sections["Sidewalk 5.5"], "", "5.5m");
		}

		private void SetUp(NetSectionPrefab prefab, string value1, string value2, bool deco = true)
		{
			var laneInfo = prefab.AddComponent<RoadBuilderLaneGroup>();
			laneInfo.GroupPrefab = this;
			laneInfo.Combination = deco
				? new LaneOptionCombination[]
				{
					new()
					{
						OptionName = "Decoration",
						Value = string.Empty
					},
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
				}
				: new LaneOptionCombination[]
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

			if (value1 != "")
			{
				prefab.AddComponent<RoadBuilderLaneInfo>()
					.WithFrontThumbnail("coui://roadbuildericons/RB_SidewalkwParkingFront.svg")
					.WithBackThumbnail("coui://roadbuildericons/RB_SidewalkwParkingRear.svg")
					.AddLaneThumbnail("coui://roadbuildericons/Thumb_ParkingLane.svg")
					.AddLaneThumbnail("coui://roadbuildericons/Thumb_ParkingSidewalk.svg");
			}

			if (deco)
			{
				var decoInfo = prefab.AddOrGetComponent<RoadBuilderLaneDecorationInfo>();
				decoInfo.GrassThumbnail = "coui://roadbuildericons/RB_SidewalkRightwGrass.svg";
				decoInfo.TreeThumbnail = "coui://roadbuildericons/RB_SidewalkRightwTree.svg";
				decoInfo.GrassAndTreeThumbnail = "coui://roadbuildericons/RB_SidewalkRightwTreeAndGrass.svg";
				decoInfo.LaneGrassThumbnail = new[] { "coui://roadbuildericons/Thumb_SidewalkGrass.svg" };
				decoInfo.LaneTreeThumbnail = new[] { "coui://roadbuildericons/Thumb_SidewalkTree.svg" };
				decoInfo.LaneGrassAndTreeThumbnail = new[] { "coui://roadbuildericons/Thumb_SidewalkGrassTree.svg" };
			}

			LinkedSections.Add(prefab);
		}
	}
}
