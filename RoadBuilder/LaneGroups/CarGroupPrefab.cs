using Game.Prefabs;

using RoadBuilder.Domain.Components.Prefabs;
using RoadBuilder.Domain.Enums;

namespace RoadBuilder.LaneGroups
{
	public class CarGroupPrefab : BaseLaneGroupPrefab
	{
		private const string OptionName1 = "Lane Width";
		private const string OptionName2 = "Transport Option";
		private const string OptionName3 = "Markings";

		public override bool Initialize()
		{
			Prefab!.Options = new RoadBuilderLaneOption[]
			{
				new()
				{
					DefaultValue = "3m",
					Type = LaneOptionType.LaneWidth,
					Name = OptionName1,
					Options = new RoadBuilderLaneOptionValue[]
					{
						new() { Value = "3m" },
						new() { Value = "4m" },
					}
				},
				new()
				{
					DefaultValue = "",
					Name = OptionName2,
					Options = new RoadBuilderLaneOptionValue[]
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
				},
				new()
				{
					DefaultValue = "",
					Name = OptionName3,
					Type = LaneOptionType.Toggle,
					Options = new RoadBuilderLaneOptionValue[]
					{
						new()
						{
							Value = "",
							ThumbnailUrl = "coui://roadbuildericons/RB_MarkingsWhite.svg"
						},
						new()
						{
							Value = "1",
							ThumbnailUrl = "coui://roadbuildericons/RB_NoMarkingsWhite.svg"
						}
					}
				}
			};

			Prefab.AddComponent<RoadBuilderLaneInfo>()
				.WithRequireNone(RoadCategory.NonAsphalt & ~RoadCategory.Gravel & ~RoadCategory.Tiled)
				.WithFrontThumbnail("coui://roadbuildericons/RB_CarFront.svg")
				.WithBackThumbnail("coui://roadbuildericons/RB_CarRear.svg");

			Prefab.AddComponent<UIObject>().m_Icon = "coui://roadbuildericons/RB_Car_Centered.svg";

			SetUp(Sections!["Gravel Drive Section 3"], "3m", "").AddOrGetComponent<RoadBuilderLaneInfo>().WithRequireAll(RoadCategory.Gravel).AddLaneThumbnail("coui://roadbuildericons/Thumb_GravelLane.svg");
			SetUp(Sections["RB Tiled Drive Section 3 - Car"], "3m", "").AddOrGetComponent<RoadBuilderLaneInfo>().WithRequireAll(RoadCategory.Tiled).AddLaneThumbnail("coui://roadbuildericons/Thumb_TiledWide.svg");
			SetUp(Sections["Tiled Drive Section 3"], "3m", "Tram").AddOrGetComponent<RoadBuilderLaneInfo>().WithRequireAll(RoadCategory.Tiled).AddLaneThumbnail("coui://roadbuildericons/Thumb_TiledWide.svg").WithFrontThumbnail("coui://roadbuildericons/RB_CarTramFront.svg").WithBackThumbnail("coui://roadbuildericons/RB_CarTramRear.svg");
			SetUp(Sections["Alley Drive Section 3"], "3m", "", true).AddOrGetComponent<RoadBuilderLaneInfo>().WithRequireNone(RoadCategory.Gravel | RoadCategory.Tiled).WithFrontThumbnail("coui://roadbuildericons/RB_AlleyCarFront.svg").WithBackThumbnail("coui://roadbuildericons/RB_AlleyCarRear.svg");
			SetUp(Sections["Car Drive Section 3"], "3m", "").AddOrGetComponent<RoadBuilderLaneInfo>().WithRequireNone(RoadCategory.Gravel | RoadCategory.Tiled);
			SetUp(Sections["Car Drive Section 3 - Transport Option"], "3m", "Transport").AddOrGetComponent<RoadBuilderLaneInfo>().WithRequireNone(RoadCategory.Gravel | RoadCategory.Tiled).WithFrontThumbnail("coui://roadbuildericons/RB_CarBusFront.svg").WithBackThumbnail("coui://roadbuildericons/RB_CarBusRear.svg");
			SetUp(Sections["Car Drive Section 3 - Transport Tram Option"], "3m", "Tram").AddOrGetComponent<RoadBuilderLaneInfo>().WithRequireNone(RoadCategory.Gravel | RoadCategory.Tiled).WithFrontThumbnail("coui://roadbuildericons/RB_CarTramFront.svg").WithBackThumbnail("coui://roadbuildericons/RB_CarTramRear.svg");
			SetUp(Sections["Highway Drive Section 4"], "4m", "").AddOrGetComponent<RoadBuilderLaneInfo>().WithRequireNone(RoadCategory.Gravel | RoadCategory.Tiled);
			SetUp(Sections["Highway Drive Section 4 - Transport Option"], "4m", "Transport").AddOrGetComponent<RoadBuilderLaneInfo>().WithRequireNone(RoadCategory.Gravel | RoadCategory.Tiled).WithFrontThumbnail("coui://roadbuildericons/RB_CarBusFront.svg").WithBackThumbnail("coui://roadbuildericons/RB_CarBusRear.svg");
			SetUp(Sections["Car Drive Section 4 - Transport Tram Option"], "4m", "Tram").AddOrGetComponent<RoadBuilderLaneInfo>().WithRequireNone(RoadCategory.Gravel | RoadCategory.Tiled).WithFrontThumbnail("coui://roadbuildericons/RB_CarTramFront.svg").WithBackThumbnail("coui://roadbuildericons/RB_CarTramRear.svg");

			return true;
		}

		private NetSectionPrefab SetUp(NetSectionPrefab prefab, string value1, string value2, bool noMarkings = false)
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
				},
				new()
				{
					OptionName = OptionName3,
					Value = noMarkings ? "1" : ""
				}
			};

			return prefab;
		}
	}
}
