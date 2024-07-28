using Game.Prefabs;

using RoadBuilder.Domain.Components.Prefabs;

using System.Collections.Generic;

namespace RoadBuilder.LaneGroups
{
	public class ShoulderGroupPrefab : BaseLaneGroupPrefab
	{
		private const string OptionName1 = "Lane Width";
		private const string OptionName2 = "Ground Type";

		public override void Initialize(Dictionary<string, NetSectionPrefab> sections)
		{
			DisplayName = "Shoulder";
			Options = new RoadBuilderLaneOption[]
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
					Type = LaneOptionType.ValueUpDown,
					Name = OptionName1,
					Options = new RoadBuilderLaneOptionValue[]
					{
						new() { Value = "1m" },
						new() { Value = "2m" },
					}
				},
			};

			AddComponent<RoadBuilderLaneInfo>()
				.AddLaneThumbnail("coui://roadbuildericons/Thumb_Shoulder.svg");

			AddComponent<UIObject>().m_Icon = "coui://roadbuildericons/RB_Empty.svg";

			SetUp(sections["Alley Shoulder 1"], "1m", "Asphalt");
			SetUp(sections["Highway Shoulder 2"], "2m", "Asphalt");
			SetUp(sections["Public Transport Shoulder 1"], "1m", "Bus");
			SetUp(sections["Gravel Shoulder 1"], "1m", "Gravel").AddLaneThumbnail("coui://roadbuildericons/Thumb_ShoulderGravel.svg");
			SetUp(sections["Tiled Shoulder 1"], "1m", "Tiled").AddLaneThumbnail("coui://roadbuildericons/Thumb_ShoulderPedestrian.svg");
			SetUp(sections["Subway Shoulder 2"], "2m", "Subway").AddLaneThumbnail("coui://roadbuildericons/Thumb_ShoulderTrack.svg");
			SetUp(sections["Train Shoulder 2"], "2m", "Train").AddLaneThumbnail("coui://roadbuildericons/Thumb_ShoulderTrack.svg");
			SetUp(sections["Tram Shoulder 1"], "1m", "Tram").AddLaneThumbnail("coui://roadbuildericons/Thumb_ShoulderTrack.svg");
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
