using Game.Prefabs;

using RoadBuilder.Domain.Components.Prefabs;
using RoadBuilder.Domain.Enums;

using System.Collections.Generic;

namespace RoadBuilder.LaneGroups
{
	public class PoleGroupPrefab : BaseLaneGroupPrefab
	{
		private const string OptionName1 = "Lane Width";
		private const string OptionName2 = "Ground Type";

		public override void Initialize(Dictionary<string, NetSectionPrefab> sections)
		{
			Options = new RoadBuilderLaneOption[]
			{
				new()
				{
					DefaultValue = "Tram",
					Type = LaneOptionType.SingleSelectionButtons,
					Name = OptionName2,
					Options = new RoadBuilderLaneOptionValue[]
					{
						new() { Value = "Tram", ThumbnailUrl = "coui://roadbuildericons/RB_TramWhite.svg" },
						new() { Value = "Train", ThumbnailUrl = "coui://roadbuildericons/RB_TrainWhite.svg" },
					}
				},
				new()
				{
					DefaultValue = "1m",
					Type = LaneOptionType.ValueUpDown,
					Name = OptionName1,
					Options = new RoadBuilderLaneOptionValue[]
					{
						new() { Value = "1m" },
						new() { Value = "2m" },
					}
				},
			};

			AddComponent<UIObject>().m_Icon = "coui://roadbuildericons/RB_TramShoulder.svg";

			SetUp(sections["RB Train Pole Section 2"], "2m", "Train").WithThumbnail("coui://roadbuildericons/RB_TrainShoulder.svg").WithGroundTexture(LaneGroundType.Train).WithColor(82, 62, 51).AddLaneThumbnail("coui://roadbuildericons/Thumb_ShoulderTrack.svg");
			SetUp(sections["RB Train Pole Section 1"], "1m", "Train").WithThumbnail("coui://roadbuildericons/RB_TrainShoulder.svg").WithGroundTexture(LaneGroundType.Train).WithColor(82, 62, 51).AddLaneThumbnail("coui://roadbuildericons/Thumb_ShoulderTrack.svg");
			SetUp(sections["RB Tram Pole Section 2"], "2m", "Tram").WithThumbnail("coui://roadbuildericons/RB_TramShoulder.svg").WithGroundTexture(LaneGroundType.Asphalt).AddLaneThumbnail("coui://roadbuildericons/Thumb_ShoulderTrack.svg");
			SetUp(sections["RB Tram Pole Section 1"], "1m", "Tram").WithThumbnail("coui://roadbuildericons/RB_TramShoulder.svg").WithGroundTexture(LaneGroundType.Asphalt).AddLaneThumbnail("coui://roadbuildericons/Thumb_ShoulderTrack.svg");
		}

		private RoadBuilderLaneInfo SetUp(NetSectionPrefab prefab, string value1, string value2)
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
				},
			};

			LinkedSections.Add(prefab);

			return prefab.AddOrGetComponent<RoadBuilderLaneInfo>();
		}
	}
}
