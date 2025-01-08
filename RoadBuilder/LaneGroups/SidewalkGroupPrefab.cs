using Game.Prefabs;

using RoadBuilder.Domain.Components.Prefabs;
using RoadBuilder.Domain.Enums;

namespace RoadBuilder.LaneGroups
{
	public class SidewalkGroupPrefab : BaseLaneGroupPrefab
	{
		private const string OptionName1 = "Parking";
		private const string OptionName2 = "Width";
		private const string OptionName3 = "Parking Angle";

		public override void Initialize()
		{
			Prefab!.Options = new RoadBuilderLaneOption[]
			{
				new()
				{
					DefaultValue = "3.5m",
					Type = LaneOptionType.LaneWidth,
					Name = OptionName2,
					Options = new RoadBuilderLaneOptionValue[]
					{
						new() { Value = "1m" },
						new() { Value = "1.5m" },
						new() { Value = "2m" },
						new() { Value = "2.5m" },
						new() { Value = "3m" },
						new() { Value = "3.5m" },
						new() { Value = "4m" },
						new() { Value = "4.5m" },
						new() { Value = "5m" },
						new() { Value = "5.5m" },
						new() { Value = "6m" },
						new() { Value = "7m" },
						new() { Value = "9m" },
					}
				},
				new()
				{
					DefaultValue = "",
					Name = OptionName1,
					Type = LaneOptionType.Toggle,
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
					DefaultValue = "Parallel",
					Type = LaneOptionType.SingleSelectionButtons,
					Name = OptionName3,
					Options = new RoadBuilderLaneOptionValue[]
					{
						new()
						{
							Value = "Parallel",
							ThumbnailUrl = "coui://roadbuildericons/RB_WhiteParkingAngle0.svg"
						},
						new()
						{
							Value = "Angled",
							ThumbnailUrl = "coui://roadbuildericons/RB_WhiteParkingAngle60.svg"
						},
						new()
						{
							Value = "Perpendicular",
							ThumbnailUrl = "coui://roadbuildericons/RB_WhiteParkingAngle90.svg"
						},
					}
				},
				new()
				{
					Name = "Decoration",
					Type = LaneOptionType.Decoration,
				},
			};

			Prefab.AddComponent<RoadBuilderLaneInfo>()
				.AddLaneThumbnail("coui://roadbuildericons/Thumb_SidewalkWide.svg");

			Prefab.AddComponent<UIObject>().m_Icon = "coui://roadbuildericons/RB_WideSidewalkRight.svg";

			var edgeInfo = Prefab.AddComponent<RoadBuilderEdgeLaneInfo>();
			edgeInfo.AddSidewalkStateOnNode = true;
			edgeInfo.SidePrefab = Sections!["Road Side 0"];

			SetUp<RoadBuilderLaneGroup>(Sections["Sidewalk 1"], "", "1m", "", false);
			SetUp<RoadBuilderLaneGroup>(Sections["Sidewalk 1.5"], "", "1.5m", "", false);
			SetUp<RoadBuilderLaneGroup>(Sections["Sidewalk 2"], "", "2m", "", false);
			SetUp<RoadBuilderLaneGroup>(Sections["Sidewalk 2.5"], "", "2.5m", "", false);
			SetUp<RoadBuilderLaneGroup>(Sections["Sidewalk 3"], "", "3m", "", false);
			SetUp<RoadBuilderLaneGroup>(Sections["Sidewalk 4"], "", "4m", "", false);
			SetUp<RoadBuilderLaneGroup>(Sections["Sidewalk 3.5"], "", "3.5m", "", false);
			SetUp<RoadBuilderLaneGroup>(Sections["Sidewalk 4.5"], "", "4.5m", "", false);
			SetUp<RoadBuilderLaneGroup>(Sections["Sidewalk With Parking 5"], "P", "5m", "Parallel");
			SetUp<RoadBuilderLaneGroup>(Sections["Sidewalk With Parking 6"], "P", "6m", "Parallel");
			SetUp<RoadBuilderLaneGroup>(Sections["Sidewalk With Parking 7"], "P", "7m", "Parallel");
			SetUp<RoadBuilderLaneGroup>(Sections["Sidewalk With Parking 5.5"], "P", "5.5m", "Parallel");
			SetUp<RoadBuilderLaneGroup>(Sections["Sidewalk 5"], "", "5m", "");
			SetUp<RoadBuilderLaneGroup>(Sections["Sidewalk 7"], "", "7m", "");
			SetUp<RoadBuilderLaneGroup>(Sections["Sidewalk 6"], "", "6m", "");
			SetUp<RoadBuilderLaneGroup>(Sections["Sidewalk 5.5"], "", "5.5m", "");
			SetUp<RoadBuilderLaneGroup>(Sections["Sidewalk With Parking Lane 7"], "P", "7m", "Perpendicular");
			SetUp<RoadBuilderLaneGroup>(Sections["Sidewalk With Parking Lane Angled 7"], "P", "7m", "Angled");
			SetUp<RoadBuilderLaneGroup>(Sections["Sidewalk With Parking Lane 9"], "P", "9m", "Perpendicular");
			SetUp<RoadBuilderLaneGroup>(Sections["Sidewalk With Parking Lane Angled 9"], "P", "9m", "Angled");

			SetUp<RoadBuilderVanillaLaneGroup>(Sections["Sidewalk With Parking Lane Divided 9"], "P", "9m", "Perpendicular");
			SetUp<RoadBuilderVanillaLaneGroup>(Sections["Sidewalk With Parking Lane Divided Angled 9"], "P", "9m", "Angled");
		}

		private void SetUp<T>(NetSectionPrefab prefab, string value1, string value2, string value3, bool deco = true) where T : RoadBuilderLaneGroup
		{
			var laneInfo = prefab.AddComponent<T>();
			laneInfo.GroupPrefab = Prefab;
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
					},
					new()
					{
						OptionName = OptionName3,
						Value = value3
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
					},
					new()
					{
						OptionName = OptionName3,
						Value = value3
					}
				};

			var uiObj = prefab.AddOrGetComponent<UIObject>();
			uiObj.m_Icon = value1 == "" ? "coui://roadbuildericons/RB_WideSidewalkRight.svg" : "coui://roadbuildericons/RB_SidewalkRightwParking.svg";

			if (value1 != "")
			{
				switch (value3)
				{
					case "Angled":
						prefab.AddComponent<RoadBuilderLaneInfo>()
							.WithFrontThumbnail("coui://roadbuildericons/RB_ParkingFront45-60withSidewalk.svg")
							.WithBackThumbnail("coui://roadbuildericons/RB_ParkingRear45-60withSidewalk.svg")
							.AddLaneThumbnail("coui://roadbuildericons/Thumb_AngledParkingLane.svg")
							.AddLaneThumbnail("coui://roadbuildericons/Thumb_ParkingSidewalk.svg");
						break;
					case "Perpendicular":
						prefab.AddComponent<RoadBuilderLaneInfo>()
							.WithFrontThumbnail("coui://roadbuildericons/RB_ParkingFront90withSidewalk.svg")
							.WithBackThumbnail("coui://roadbuildericons/RB_ParkingRear90withSidewalk.svg")
							.AddLaneThumbnail("coui://roadbuildericons/Thumb_90ParkingLane.svg")
							.AddLaneThumbnail("coui://roadbuildericons/Thumb_ParkingSidewalk.svg");
						break;
					default:
						prefab.AddComponent<RoadBuilderLaneInfo>()
							.WithFrontThumbnail("coui://roadbuildericons/RB_SidewalkwParkingFront.svg")
							.WithBackThumbnail("coui://roadbuildericons/RB_SidewalkwParkingRear.svg")
							.AddLaneThumbnail("coui://roadbuildericons/Thumb_ParkingLane.svg")
							.AddLaneThumbnail("coui://roadbuildericons/Thumb_ParkingSidewalk.svg");
						break;
				}
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
		}
	}
}
