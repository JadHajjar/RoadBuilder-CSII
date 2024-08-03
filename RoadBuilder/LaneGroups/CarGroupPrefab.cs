using Game.Prefabs;

using RoadBuilder.Domain.Components.Prefabs;
using RoadBuilder.Domain.Enums;

using System.Collections.Generic;

namespace RoadBuilder.LaneGroups
{
	public class CarGroupPrefab : BaseLaneGroupPrefab
	{
		private const string OptionName1 = "Lane Width";
		private const string OptionName2 = "Transport Option";
		private const string OptionName3 = "Markings";

		public override void Initialize(Dictionary<string, NetSectionPrefab> sections)
		{
			DisplayName = "Car";
			Options = new RoadBuilderLaneOption[]
			{
				new()
				{
					DefaultValue = "3m",
					Type = LaneOptionType.ValueUpDown,
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
				}
			};

			AddComponent<RoadBuilderLaneInfo>()
				.WithExcluded(RoadCategory.NonAsphalt & ~RoadCategory.Gravel & ~RoadCategory.Tiled)
				.WithFrontThumbnail("coui://roadbuildericons/RB_CarFront.svg")
				.WithBackThumbnail("coui://roadbuildericons/RB_CarRear.svg");

			AddComponent<UIObject>().m_Icon = "coui://roadbuildericons/RB_Car_Centered.svg";

			SetUp(sections["Gravel Drive Section 3"], "3m", "").AddOrGetComponent<RoadBuilderLaneInfo>().WithRequired(RoadCategory.Gravel).AddLaneThumbnail("coui://roadbuildericons/Thumb_GravelLane.svg");
			SetUp(sections["Tiled Drive Section 3"], "3m", "").AddOrGetComponent<RoadBuilderLaneInfo>().WithRequired(RoadCategory.Tiled).AddLaneThumbnail("coui://roadbuildericons/Thumb_TiledWide.svg");
			SetUp(sections["Car Drive Section 3"], "3m", "").AddOrGetComponent<RoadBuilderLaneInfo>().WithExcluded(RoadCategory.Gravel | RoadCategory.Tiled);
			SetUp(sections["Car Drive Section 3 - Transport Option"], "3m", "Transport").AddOrGetComponent<RoadBuilderLaneInfo>().WithExcluded(RoadCategory.Gravel | RoadCategory.Tiled).WithFrontThumbnail("coui://roadbuildericons/RB_CarBusFront.svg").WithBackThumbnail("coui://roadbuildericons/RB_CarBusRear.svg");
			SetUp(sections["Car Drive Section 3 - Transport Tram Option"], "3m", "Tram").AddOrGetComponent<RoadBuilderLaneInfo>().WithExcluded(RoadCategory.Gravel | RoadCategory.Tiled).WithFrontThumbnail("coui://roadbuildericons/RB_CarTramFront.svg").WithBackThumbnail("coui://roadbuildericons/RB_CarTramRear.svg");
			SetUp(sections["Highway Drive Section 4"], "4m", "").AddOrGetComponent<RoadBuilderLaneInfo>().WithExcluded(RoadCategory.Gravel | RoadCategory.Tiled);
			SetUp(sections["Highway Drive Section 4 - Transport Option"], "4m", "Transport").AddOrGetComponent<RoadBuilderLaneInfo>().WithExcluded(RoadCategory.Gravel | RoadCategory.Tiled).WithFrontThumbnail("coui://roadbuildericons/RB_CarBusFront.svg").WithBackThumbnail("coui://roadbuildericons/RB_CarBusRear.svg");
			SetUp(sections["Car Drive Section 4 - Transport Tram Option"], "4m", "Tram").AddOrGetComponent<RoadBuilderLaneInfo>().WithExcluded(RoadCategory.Gravel | RoadCategory.Tiled).WithFrontThumbnail("coui://roadbuildericons/RB_CarTramFront.svg").WithBackThumbnail("coui://roadbuildericons/RB_CarTramRear.svg");
		}

		private NetSectionPrefab SetUp(NetSectionPrefab prefab, string value1, string value2)
		{
			var laneInfo = prefab.AddComponent<RoadBuilderLaneGroup>();
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
