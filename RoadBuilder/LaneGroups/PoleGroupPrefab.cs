using Game.Prefabs;

using RoadBuilder.Domain.Components.Prefabs;
using RoadBuilder.Domain.Enums;

namespace RoadBuilder.LaneGroups
{
	public class PoleGroupPrefab : BaseLaneGroupPrefab
	{
		private const string OptionName1 = "Lane Width";
		private const string OptionName2 = "Ground Type";

		public override bool Initialize()
		{
			Prefab!.Options = new RoadBuilderLaneOption[]
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
					Type = LaneOptionType.LaneWidth,
					Name = OptionName1,
					Options = new RoadBuilderLaneOptionValue[]
					{
						new() { Value = "1m" },
						new() { Value = "2m" },
					}
				},
			};

			Prefab.AddComponent<UIObject>().m_Icon = "coui://roadbuildericons/RB_TramShoulder.svg";

			SetUp(Sections!["RB Train Pole Section 2"], "2m", "Train").WithThumbnail("coui://roadbuildericons/RB_TrainShoulder.svg").WithGroundTexture(LaneGroundType.Train).WithColor(82, 62, 51).AddLaneThumbnail("coui://roadbuildericons/Thumb_ShoulderTrack.svg");
			SetUp(Sections["RB Train Pole Section 1"], "1m", "Train").WithThumbnail("coui://roadbuildericons/RB_TrainShoulder.svg").WithGroundTexture(LaneGroundType.Train).WithColor(82, 62, 51).AddLaneThumbnail("coui://roadbuildericons/Thumb_ShoulderTrack.svg");
			SetUp(Sections["RB Tram Pole Section 2"], "2m", "Tram").WithThumbnail("coui://roadbuildericons/RB_TramShoulder.svg").WithGroundTexture(LaneGroundType.Asphalt).AddLaneThumbnail("coui://roadbuildericons/Thumb_ShoulderTrack.svg");
			SetUp(Sections["RB Tram Pole Section 1"], "1m", "Tram").WithThumbnail("coui://roadbuildericons/RB_TramShoulder.svg").WithGroundTexture(LaneGroundType.Asphalt).AddLaneThumbnail("coui://roadbuildericons/Thumb_ShoulderTrack.svg");

			return true;
		}

		private RoadBuilderLaneInfo SetUp(NetSectionPrefab prefab, string value1, string value2)
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
			};

			return prefab.AddOrGetComponent<RoadBuilderLaneInfo>();
		}
	}
}
