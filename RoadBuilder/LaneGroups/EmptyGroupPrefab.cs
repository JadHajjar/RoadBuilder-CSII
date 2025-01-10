using Colossal.IO.AssetDatabase.Internal;

using Game.Prefabs;

using RoadBuilder.Domain.Components.Prefabs;
using RoadBuilder.Domain.Enums;

using System.Collections.Generic;
using System.Globalization;

namespace RoadBuilder.LaneGroups
{
	public class EmptyGroupPrefab : BaseLaneGroupPrefab
	{
		private const string OptionName1 = "Lane Width";
		private const string OptionName2 = "Ground Type";

		public override void Initialize()
		{
			Prefab!.Options = new RoadBuilderLaneOption[]
			{
				new()
				{
					DefaultValue = "Asphalt",
					Type = LaneOptionType.SingleSelectionButtons,
					Name = OptionName2,
					Options = new RoadBuilderLaneOptionValue[]
					{
						new() { Value = "Asphalt", ThumbnailUrl = "coui://roadbuildericons/RB_CarWhite.svg" },
						new() { Value = "Train", ThumbnailUrl = "coui://roadbuildericons/RB_TrainWhite.svg" },
						new() { Value = "Gravel", ThumbnailUrl = "coui://roadbuildericons/RB_GravelWhite.svg" },
						new() { Value = "Tiled", ThumbnailUrl = "coui://roadbuildericons/RB_TiledWhite.svg" },
					}
				},
				new()
				{
					DefaultValue = "1m",
					Type = LaneOptionType.LaneWidth,
					IgnoreForSimilarDuplicate = true,
					Name = OptionName1,
					Options = new RoadBuilderLaneOptionValue[]
					{
						new() { Value = "0.5m" },
						new() { Value = "1m" },
						new() { Value = "1.5m" },
						new() { Value = "2m" },
						new() { Value = "2.5m" },
						new() { Value = "3m" },
						new() { Value = "3.5m" },
						new() { Value = "4m" },
					}
				},
			};

			Prefab.AddComponent<UIObject>().m_Icon = "coui://roadbuildericons/RB_Empty.svg";

			SetUp(Sections!, "RB Empty Section {0}", 4, "Asphalt", "Thumb_CarLane", "Thumb_CarLaneSmall").ForEach(x => x.Item2.WithRequireNone(x.Item1 is 3 or 4 ? default : RoadCategory.NonAsphalt).WithGroundTexture(LaneGroundType.Asphalt));
			SetUp(Sections!, "RB Train Empty Section {0}", 4, "Train", "Thumb_TracklessLane", "Thumb_TracklessLaneSmall").ForEach(x => x.Item2.WithRequireAny(x.Item1 is 4 ? default : RoadCategory.Train | RoadCategory.Subway).WithGroundTexture(LaneGroundType.Train).WithColor(82, 62, 51));
			SetUp(Sections!, "RB Gravel Empty Section {0}", 3, "Gravel", "Thumb_GravelLane", "Thumb_GravelLaneSmall").ForEach(x => x.Item2.WithRequireAny(x.Item1 is 3 ? default : RoadCategory.Gravel).WithGroundTexture(LaneGroundType.Gravel).WithColor(143, 131, 97));
			SetUp(Sections!, "RB Tiled Empty Section {0}", 3, "Tiled", "Thumb_TiledWide", "Thumb_TiledSmall").ForEach(x => x.Item2.WithRequireAny(x.Item1 is 3 ? default : RoadCategory.Tiled).WithGroundTexture(LaneGroundType.Tiled).WithColor(76, 78, 83));
		}

		private IEnumerable<(float, RoadBuilderLaneInfo)> SetUp(Dictionary<string, NetSectionPrefab> sections, string name, float maxWidth, string value2, string largeThumb, string smallThumb)
		{
			for (var width = 0.5f; width <= maxWidth; width += 0.5f)
			{
				var prefab = sections[string.Format(name, width.ToString(CultureInfo.InvariantCulture))];
				var laneGroup = prefab.AddComponent<RoadBuilderLaneGroup>();
				laneGroup.GroupPrefab = Prefab;
				laneGroup.Combination = new LaneOptionCombination[]
				{
					new()
					{
						OptionName = OptionName1,
						Value = $"{width.ToString(CultureInfo.InvariantCulture)}m"
					},
					new()
					{
						OptionName = OptionName2,
						Value = value2
					},
				};

				var laneInfo = prefab.AddOrGetComponent<RoadBuilderLaneInfo>();

				if (width <= 2)
				{
					laneInfo.AddLaneThumbnail($"coui://roadbuildericons/{smallThumb}.svg");
				}
				else
				{
					laneInfo.AddLaneThumbnail($"coui://roadbuildericons/{largeThumb}.svg");
				}

				yield return (width, laneInfo);
			}
		}
	}
}
