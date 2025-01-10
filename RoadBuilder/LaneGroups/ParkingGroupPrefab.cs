using Game.Prefabs;

using RoadBuilder.Domain.Components.Prefabs;
using RoadBuilder.Domain.Enums;

namespace RoadBuilder.LaneGroups
{
	public class ParkingGroupPrefab : BaseLaneGroupPrefab
	{
		private const string OptionName1 = "Parking Angle";
		private const string OptionName2 = "Markings";

		public override void Initialize()
		{
			Prefab!.Options = new RoadBuilderLaneOption[]
			{
				new()
				{
					DefaultValue = "Parallel",
					Type = LaneOptionType.SingleSelectionButtons,
					Name = OptionName1,
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
					DefaultValue = "",
					Name = OptionName2,
					Type = LaneOptionType.Toggle,
					Options = new RoadBuilderLaneOptionValue[]
					{
						new()
						{
							Value = "1",
							ThumbnailUrl = "coui://roadbuildericons/RB_MarkingsWhite.svg"
						},
						new()
						{
							Value = "",
							ThumbnailUrl = "coui://roadbuildericons/RB_NoMarkingsWhite.svg"
						}
					}
				}
			};

			Prefab.AddComponent<RoadBuilderLaneInfo>()
				.WithRequireNone(RoadCategory.NonAsphalt)
				.AddLaneThumbnail("coui://roadbuildericons/Thumb_CarLane.svg");

			Prefab.AddComponent<UIObject>().m_Icon = "coui://roadbuildericons/RB_ParkingFront0Center.svg";

			SetUp(Sections!["RB Parking Section Parallel"], "Parallel", "1").AddOrGetComponent<RoadBuilderLaneInfo>().AddLaneThumbnail("coui://roadbuildericons/Thumb_CarLaneSmall.svg").WithFrontThumbnail("coui://roadbuildericons/RB_ParkingFront0.svg").WithBackThumbnail("coui://roadbuildericons/RB_ParkingRear0.svg");
			SetUp(Sections!["RB Parking Section Parallel NoMarking"], "Parallel", "").AddOrGetComponent<RoadBuilderLaneInfo>().WithNoDirection().AddLaneThumbnail("coui://roadbuildericons/Thumb_CarLaneSmall.svg").WithFrontThumbnail("coui://roadbuildericons/RB_ParkingFront0.svg").WithBackThumbnail("coui://roadbuildericons/RB_ParkingRear0.svg");
			SetUp(Sections["RB Parking Section Angled"], "Angled", "1").AddOrGetComponent<RoadBuilderLaneInfo>().WithNoDirection().AddLaneThumbnail("coui://roadbuildericons/Thumb_AngledParkingLane.svg").WithFrontThumbnail("coui://roadbuildericons/RB_ParkingFront45-60.svg").WithBackThumbnail("coui://roadbuildericons/RB_ParkingRear45-60.svg");
			SetUp(Sections["RB Parking Section Angled NoMarking"], "Angled", "").AddOrGetComponent<RoadBuilderLaneInfo>().WithNoDirection().AddLaneThumbnail("coui://roadbuildericons/Thumb_AngledParkingLane.svg").WithFrontThumbnail("coui://roadbuildericons/RB_ParkingFront45-60.svg").WithBackThumbnail("coui://roadbuildericons/RB_ParkingRear45-60.svg");
			SetUp(Sections["RB Parking Section Perpendicular"], "Perpendicular", "1").AddOrGetComponent<RoadBuilderLaneInfo>().WithNoDirection().AddLaneThumbnail("coui://roadbuildericons/Thumb_90ParkingLane.svg").WithFrontThumbnail("coui://roadbuildericons/RB_ParkingFront90.svg").WithBackThumbnail("coui://roadbuildericons/RB_ParkingRear90.svg");
			SetUp(Sections["RB Parking Section Perpendicular NoMarking"], "Perpendicular", "").AddOrGetComponent<RoadBuilderLaneInfo>().WithNoDirection().AddLaneThumbnail("coui://roadbuildericons/Thumb_90ParkingLane.svg").WithFrontThumbnail("coui://roadbuildericons/RB_ParkingFront90.svg").WithBackThumbnail("coui://roadbuildericons/RB_ParkingRear90.svg");
		}

		private NetSectionPrefab SetUp(NetSectionPrefab prefab, string value1, string value2)
		{
			var laneInfo = prefab.AddComponent<RoadBuilderLaneGroup>();
			laneInfo.GroupPrefab = Prefab;
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

			return prefab;
		}
	}
}
