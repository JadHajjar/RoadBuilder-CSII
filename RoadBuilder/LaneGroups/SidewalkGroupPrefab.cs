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

		public override bool Initialize()
		{
			Prefab!.Options = new RoadBuilderLaneOption[]
			{
				new()
				{
					DefaultValue = "",
					Name = OptionName1,
					Type = LaneOptionType.SingleSelectionButtons,
					Options = new RoadBuilderLaneOptionValue[]
					{
						new()
						{
							Value = "H" ,
							ThumbnailUrl = "coui://roadbuildericons/RB_Harbor.svg"
						},
						new()
						{
							Value = "B" ,
							ThumbnailUrl = "coui://roadbuildericons/RB_YesBikes.svg"
						},
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
					Type = LaneOptionType.SingleSelectionButtons,
					Name = OptionName3,
					Options = new RoadBuilderLaneOptionValue[]
					{
						new()
						{
							Value = "",
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

			SetUp<RoadBuilderLaneGroup>(Sections["Sidewalk 1"], "", "1m", "", null);
			SetUp<RoadBuilderLaneGroup>(Sections["Sidewalk 1.5"], "", "1.5m", "", null);
			SetUp<RoadBuilderLaneGroup>(Sections["Sidewalk 2"], "", "2m", "", null);
			SetUp<RoadBuilderLaneGroup>(Sections["Sidewalk 2.5"], "", "2.5m", "", null);
			SetUp<RoadBuilderLaneGroup>(Sections["Sidewalk 3"], "", "3m", "", null);
			SetUp<RoadBuilderLaneGroup>(Sections["Sidewalk 4"], "", "4m", "", null);
			SetUp<RoadBuilderLaneGroup>(Sections["Sidewalk 3.5"], "", "3.5m", "", null);
			SetUp<RoadBuilderLaneGroup>(Sections["Sidewalk 4.5"], "", "4.5m", "", null);

			SetUp<RoadBuilderLaneGroup>(Sections["Sidewalk 5"], "", "5m", "");
			SetUp<RoadBuilderLaneGroup>(Sections["Sidewalk 7"], "", "7m", "");
			SetUp<RoadBuilderLaneGroup>(Sections["Sidewalk 6"], "", "6m", "");
			SetUp<RoadBuilderLaneGroup>(Sections["Sidewalk 5.5"], "", "5.5m", "");

			SetUp<RoadBuilderLaneGroup>(Sections["Sidewalk With Parking 5"], "P", "5m", "");
			SetUp<RoadBuilderLaneGroup>(Sections["Sidewalk With Parking 6"], "P", "6m", "");
			SetUp<RoadBuilderLaneGroup>(Sections["Sidewalk With Parking 7"], "P", "7m", "");
			SetUp<RoadBuilderLaneGroup>(Sections["Sidewalk With Parking 5.5"], "P", "5.5m", "");
			SetUp<RoadBuilderLaneGroup>(Sections["Sidewalk With Parking Lane 7"], "P", "7m", "Perpendicular");
			SetUp<RoadBuilderLaneGroup>(Sections["Sidewalk With Parking Lane Angled 7"], "P", "7m", "Angled");
			SetUp<RoadBuilderLaneGroup>(Sections["Sidewalk With Parking Lane 9"], "P", "9m", "Perpendicular");
			SetUp<RoadBuilderLaneGroup>(Sections["Sidewalk With Parking Lane Angled 9"], "P", "9m", "Angled");

			SetUp<RoadBuilderLaneGroup>(Sections["Sidewalk With Bikelane 3.5"], "B", "3.5m", "", null);
			SetUp<RoadBuilderLaneGroup>(Sections["Sidewalk With Bikelane 4.5"], "B", "4.5m", "", "T");
			SetUp<RoadBuilderLaneGroup>(Sections["Sidewalk With Bikelane 5"], "B", "5m", "", "T");
			SetUp<RoadBuilderLaneGroup>(Sections["Sidewalk With Bikelane 5.5"], "B", "5.5m", "", "T");
			SetUp<RoadBuilderLaneGroup>(Sections["Sidewalk With Bikelane 6"], "B", "6m", "", "T");
			SetUp<RoadBuilderLaneGroup>(Sections["Sidewalk With Bikelane 7"], "B", "7m", "", "T");

			if (Sections!.ContainsKey("Harbor Sidewalk Section 6"))
			{
				SetUp<RoadBuilderLaneGroup>(Sections["Harbor Sidewalk Section 6"], "H", "6m", "", null).WithColor(145, 155, 163, 150);
				var harborEdgeInfo = Sections["Harbor Sidewalk Section 6"].AddComponent<RoadBuilderEdgeLaneInfo>();
				harborEdgeInfo.AddSidewalkStateOnNode = true;
				harborEdgeInfo.SidePrefab = Sections!["Harbor Road Side Section 0"];
			}

			SetUp<RoadBuilderVanillaLaneGroup>(Sections["Sidewalk With Parking Lane Divided 9"], "P", "9m", "Perpendicular");
			SetUp<RoadBuilderVanillaLaneGroup>(Sections["Sidewalk With Parking Lane Divided Angled 9"], "P", "9m", "Angled");

			return true;
		}

		private RoadBuilderLaneInfo SetUp<T>(NetSectionPrefab prefab, string value1, string value2, string value3, string? deco = "") where T : RoadBuilderLaneGroup
		{
			var laneInfo = prefab.AddComponent<T>();
			laneInfo.GroupPrefab = Prefab;
			laneInfo.Combination = new LaneOptionCombination[]
			{
				new()
				{
					OptionName = "Decoration",
					Value = deco
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
			};

			var uiObj = prefab.AddOrGetComponent<UIObject>();
			uiObj.m_Icon = value1 == "" ? "coui://roadbuildericons/RB_WideSidewalkRight.svg" : "coui://roadbuildericons/RB_SidewalkRightwParking.svg";

			switch (value1 + value3)
			{
				case "PAngled":
					prefab.AddComponent<RoadBuilderLaneInfo>()
						.WithFrontThumbnail("coui://roadbuildericons/RB_ParkingFront45-60withSidewalk.svg")
						.WithBackThumbnail("coui://roadbuildericons/RB_ParkingRear45-60withSidewalk.svg")
						.AddLaneThumbnail("coui://roadbuildericons/Thumb_AngledParkingLane.svg")
						.AddLaneThumbnail("coui://roadbuildericons/Thumb_ParkingSidewalk.svg");
					break;
				case "PPerpendicular":
					prefab.AddComponent<RoadBuilderLaneInfo>()
						.WithFrontThumbnail("coui://roadbuildericons/RB_ParkingFront90withSidewalk.svg")
						.WithBackThumbnail("coui://roadbuildericons/RB_ParkingRear90withSidewalk.svg")
						.AddLaneThumbnail("coui://roadbuildericons/Thumb_90ParkingLane.svg")
						.AddLaneThumbnail("coui://roadbuildericons/Thumb_ParkingSidewalk.svg");
					break;
				case "B":
					prefab.AddComponent<RoadBuilderLaneInfo>()
						.WithColor(43, 161, 82, 200)
						.WithThumbnail("coui://roadbuildericons/RB_BikewSidewalkRear.svg")
						.AddLaneThumbnail("coui://roadbuildericons/Thumb_BikeLane.svg")
						.AddLaneThumbnail("coui://roadbuildericons/Thumb_Sidewalk.svg");
					break;
				default:
					prefab.AddComponent<RoadBuilderLaneInfo>()
						.WithFrontThumbnail("coui://roadbuildericons/RB_SidewalkwParkingFront.svg")
						.WithBackThumbnail("coui://roadbuildericons/RB_SidewalkwParkingRear.svg")
						.AddLaneThumbnail("coui://roadbuildericons/Thumb_ParkingLane.svg")
						.AddLaneThumbnail("coui://roadbuildericons/Thumb_ParkingSidewalk.svg");
					break;
			}

			if (deco is not null)
			{
				var decoInfo = prefab.AddOrGetComponent<RoadBuilderLaneDecorationInfo>();
				decoInfo.GrassThumbnail = "coui://roadbuildericons/RB_SidewalkRightwGrass.svg";
				decoInfo.TreeThumbnail = "coui://roadbuildericons/RB_SidewalkRightwTree.svg";
				decoInfo.GrassAndTreeThumbnail = "coui://roadbuildericons/RB_SidewalkRightwTreeAndGrass.svg";
				decoInfo.LaneGrassThumbnail = new[] { "coui://roadbuildericons/Thumb_SidewalkGrass.svg" };
				decoInfo.LaneTreeThumbnail = new[] { "coui://roadbuildericons/Thumb_SidewalkTree.svg" };
				decoInfo.LaneGrassAndTreeThumbnail = new[] { "coui://roadbuildericons/Thumb_SidewalkGrassTree.svg" };

				if (value1 == "B")
				{
					decoInfo.TreeThumbnail = "coui://roadbuildericons/RB_BikewSidewalkwTree.svg";
					decoInfo.LaneTreeThumbnail = new[]
					{
						"coui://roadbuildericons/Thumb_BikeLane.svg",
						"coui://roadbuildericons/Thumb_SidewalkTree.svg"
					};
				}
			}

			return prefab.GetComponent<RoadBuilderLaneInfo>();
		}
	}
}
