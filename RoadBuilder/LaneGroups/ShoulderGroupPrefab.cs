using Game.Prefabs;

using RoadBuilder.Domain.Components.Prefabs;
using RoadBuilder.Domain.Enums;

namespace RoadBuilder.LaneGroups
{
	public class ShoulderGroupPrefab : BaseLaneGroupPrefab
	{
		private const string OptionName1 = "Lane Width";
		private const string OptionName2 = "Ground Type";

		public override void Initialize()
		{
			Prefab.Options = new RoadBuilderLaneOption[]
			{
				new()
				{
					DefaultValue = "Asphalt",
					Type = LaneOptionType.SingleSelectionButtons,
					Name = OptionName2,
					Options = new RoadBuilderLaneOptionValue[]
					{
						new() { Value = "Asphalt", ThumbnailUrl = "coui://roadbuildericons/RB_CarWhite.svg" },
						new() { Value = "Bus", ThumbnailUrl = "coui://roadbuildericons/RB_BusWhite.svg" },
						new() { Value = "Tram", ThumbnailUrl = "coui://roadbuildericons/RB_TramWhite.svg" },
						new() { Value = "Train", ThumbnailUrl = "coui://roadbuildericons/RB_TrainWhite.svg" },
						new() { Value = "Subway", ThumbnailUrl = "coui://roadbuildericons/RB_SubwayWhite.svg" },
						new() { Value = "Gravel", ThumbnailUrl = "coui://roadbuildericons/RB_GravelWhite.svg" },
						new() { Value = "Tiled", ThumbnailUrl = "coui://roadbuildericons/RB_TiledWhite.svg" },
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

			Prefab.AddComponent<RoadBuilderEdgeLaneInfo>();

			Prefab.AddComponent<RoadBuilderLaneInfo>()
				.WithGroundTexture(LaneGroundType.Asphalt)
				.AddLaneThumbnail("coui://roadbuildericons/Thumb_Shoulder.svg");

			Prefab.AddComponent<UIObject>().m_Icon = "coui://roadbuildericons/RB_Shoulder.svg";

			SetUp(Sections["Alley Shoulder 1"], Sections["Alley Side 0"], "1m", "Asphalt").WithThumbnail("coui://roadbuildericons/RB_ShoulderLight.svg");
			SetUp(Sections["Highway Shoulder 2"], Sections["Highway Side 0"], "2m", "Asphalt").WithThumbnail("coui://roadbuildericons/RB_ShoulderLight.svg");
			SetUp(Sections["Public Transport Shoulder 1"], Sections["Alley Side 0"], "1m", "Bus").WithThumbnail("coui://roadbuildericons/RB_ShoulderLight.svg");
			SetUp(Sections["Gravel Shoulder 1"], Sections["Gravel Side 0"], "1m", "Gravel").WithThumbnail("coui://roadbuildericons/RB_Empty.svg").WithGroundTexture(LaneGroundType.Gravel).WithColor(143, 131, 97).AddLaneThumbnail("coui://roadbuildericons/Thumb_ShoulderGravel.svg");
			SetUp(Sections["Tiled Shoulder 1"], Sections["Tiled Side 0"], "1m", "Tiled").WithThumbnail("coui://roadbuildericons/RB_Empty.svg").WithGroundTexture(LaneGroundType.Tiled).WithColor(76, 78, 83).AddLaneThumbnail("coui://roadbuildericons/Thumb_ShoulderPedestrian.svg");
			SetUp(Sections["Subway Shoulder 2"], Sections["Subway Side 0"], "2m", "Subway").WithThumbnail("coui://roadbuildericons/RB_Empty.svg").WithGroundTexture(LaneGroundType.Train).WithColor(82, 62, 51).AddLaneThumbnail("coui://roadbuildericons/Thumb_ShoulderTrack.svg");
			SetUp(Sections["Train Shoulder 2"], Sections["Train Side 0"], "2m", "Train").WithThumbnail("coui://roadbuildericons/RB_TrainShoulder.svg").WithGroundTexture(LaneGroundType.Train).WithColor(82, 62, 51).AddLaneThumbnail("coui://roadbuildericons/Thumb_ShoulderTrack.svg");
			SetUp(Sections["Tram Shoulder 1"], Sections["Alley Side 0"], "1m", "Tram").WithThumbnail("coui://roadbuildericons/RB_TramShoulder.svg").AddLaneThumbnail("coui://roadbuildericons/Thumb_ShoulderTrack.svg");
		}

		private RoadBuilderLaneInfo SetUp(NetSectionPrefab prefab, NetSectionPrefab side, string value1, string value2)
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

			prefab.AddComponent<RoadBuilderEdgeLaneInfo>().SidePrefab = side;

			return prefab.AddOrGetComponent<RoadBuilderLaneInfo>();
		}
	}
}
