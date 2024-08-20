using Game.Prefabs;

using RoadBuilder.Domain.Components.Prefabs;
using RoadBuilder.Domain.Enums;

using System.Collections.Generic;

namespace RoadBuilder.LaneGroups
{
	public class ParkingGroupPrefab : BaseLaneGroupPrefab
	{
		private const string OptionName = "Parking Angle";

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
			};

			AddComponent<RoadBuilderLaneInfo>()
				.WithRequireNone(RoadCategory.NonAsphalt)
				.WithNoDirection()
				.WithFrontThumbnail("coui://roadbuildericons/RB_CarFront.svg")
				.WithBackThumbnail("coui://roadbuildericons/RB_CarRear.svg")
				.AddLaneThumbnail("coui://roadbuildericons/Thumb_CarLane.svg");

			AddComponent<UIObject>().m_Icon = "coui://roadbuildericons/RB_Car_Centered.svg";

			SetUp(sections["RB Parking Section Parallel"], "Parallel");
			SetUp(sections["RB Parking Section Angled"], "Angled");
			SetUp(sections["RB Parking Section Perpendicular"], "Perpendicular");
		}

		private NetSectionPrefab SetUp(NetSectionPrefab prefab, string value)
		{
			var laneInfo = prefab.AddComponent<RoadBuilderLaneGroup>();
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

			return prefab;
		}
	}
}
