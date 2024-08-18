﻿using Colossal.IO.AssetDatabase.Internal;

using Game.Prefabs;

using RoadBuilder.Domain.Components.Prefabs;

using System.Collections.Generic;

namespace RoadBuilder.LaneGroups
{
	public class EmptyGroupPrefab : BaseLaneGroupPrefab
	{
		private const string OptionName1 = "Lane Width";
		private const string OptionName2 = "Ground Type";

		public override void Initialize(Dictionary<string, NetSectionPrefab> sections)
		{
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
						new() { Value = "Train", ThumbnailUrl = "coui://roadbuildericons/RB_TrainWhite.svg" },
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

			AddComponent<UIObject>().m_Icon = "coui://roadbuildericons/RB_Empty.svg";

			SetUp(sections, "RB Empty Section {0}", 4, "Asphalt", "Thumb_CarLane", "Thumb_CarLaneSmall").ForEach(x => x.WithExcluded(Domain.Enums.RoadCategory.NonAsphalt));
			SetUp(sections, "RB Train Empty Section {0}", 4, "Train", "Thumb_TracklessLane", "Thumb_TracklessLaneSmall").ForEach(x => x.WithAny(Domain.Enums.RoadCategory.Train | Domain.Enums.RoadCategory.Subway));
			SetUp(sections, "RB Gravel Empty Section {0}", 3, "Gravel", "Thumb_GravelLane", "Thumb_GravelLaneSmall").ForEach(x => x.WithAny(Domain.Enums.RoadCategory.Gravel));
			SetUp(sections, "RB Tiled Empty Section {0}", 3, "Tiled", "Thumb_TiledWide", "Thumb_TiledSmall").ForEach(x => x.WithAny(Domain.Enums.RoadCategory.Tiled));
		}

		private IEnumerable<RoadBuilderLaneInfo> SetUp(Dictionary<string, NetSectionPrefab> sections, string name, float maxWidth, string value2, string largeThumb, string smallThumb)
		{
			for (var width = 0.5f; width <= maxWidth; width += 0.5f)
			{
				var prefab = sections[string.Format(name, width)];
				var laneGroup = prefab.AddComponent<RoadBuilderLaneGroup>();
				laneGroup.GroupPrefab = this;
				laneGroup.Combination = new LaneOptionCombination[]
				{
					new()
					{
						OptionName = OptionName1,
						Value = $"{width}m"
					},
					new()
					{
						OptionName = OptionName2,
						Value = value2
					},
				};

				LinkedSections.Add(prefab);

				var laneInfo = prefab.AddOrGetComponent<RoadBuilderLaneInfo>();

				if (width <= 2)
				{
					laneInfo.AddLaneThumbnail($"coui://roadbuildericons/{smallThumb}.svg");
				}
				else
				{
					laneInfo.AddLaneThumbnail($"coui://roadbuildericons/{largeThumb}.svg");
				}

				if (width != maxWidth)
				{
					yield return laneInfo;
				}
			}
		}
	}
}