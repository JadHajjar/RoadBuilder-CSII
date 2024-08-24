using Game.Prefabs;

using RoadBuilder.Domain.Components.Prefabs;
using RoadBuilder.Domain.Enums;

using System.Collections.Generic;

namespace RoadBuilder.LaneGroups
{
	public class ParkingGroupPrefab : BaseLaneGroupPrefab
	{
		private const string OptionName = "Parking Angle";
		private const string OptionName2 = "Markings";

		public override void Initialize(Dictionary<string, NetSectionPrefab> sections)
		{
			Options = new RoadBuilderLaneOption[]
			{
				new()
				{
					DefaultValue = "Parallel",
					Type = LaneOptionType.ValueUpDown,
					Name = OptionName,
					Options = new RoadBuilderLaneOptionValue[]
					{
						new() { Value = "Parallel" },
						new() { Value = "Angled" },
						new() { Value = "Perpendicular" },
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

			AddComponent<RoadBuilderLaneInfo>()
				.WithRequireNone(RoadCategory.NonAsphalt)
				.WithFrontThumbnail("coui://roadbuildericons/RB_CarFront.svg")
				.WithBackThumbnail("coui://roadbuildericons/RB_CarRear.svg")
				.AddLaneThumbnail("coui://roadbuildericons/Thumb_CarLane.svg");

			AddComponent<UIObject>().m_Icon = "coui://roadbuildericons/RB_Car_Centered.svg";

			SetUp(sections["RB Parking Section Parallel"], "Parallel", "1");
			SetUp(sections["RB Parking Section Parallel NoMarking"], "Parallel", "").AddOrGetComponent<RoadBuilderLaneInfo>().WithNoDirection();
			SetUp(sections["RB Parking Section Angled"], "Angled", "1");
			SetUp(sections["RB Parking Section Angled NoMarking"], "Angled", "").AddOrGetComponent<RoadBuilderLaneInfo>().WithNoDirection();
			SetUp(sections["RB Parking Section Perpendicular"], "Perpendicular", "1").AddOrGetComponent<RoadBuilderLaneInfo>().WithNoDirection();
			SetUp(sections["RB Parking Section Perpendicular NoMarking"], "Perpendicular", "").AddOrGetComponent<RoadBuilderLaneInfo>().WithNoDirection();
		}

		private NetSectionPrefab SetUp(NetSectionPrefab prefab, string value, string value2)
		{
			var laneInfo = prefab.AddComponent<RoadBuilderLaneGroup>();
			laneInfo.GroupPrefab = this;
			laneInfo.Combination = new LaneOptionCombination[]
			{
				new()
				{
					OptionName = OptionName,
					Value = value
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
