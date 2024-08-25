using Game.Prefabs;

using RoadBuilder.Domain.Components.Prefabs;
using RoadBuilder.Domain.Enums;

using System;
using System.Collections.Generic;

namespace RoadBuilder.LaneGroups
{
	public class MedianGroupPrefab : BaseLaneGroupPrefab
	{
		private const string OptionName = "Median Width";

		public override void Initialize(Dictionary<string, NetSectionPrefab> sections)
		{
			Options = new RoadBuilderLaneOption[]
			{
				new()
				{
					DefaultValue = "2m",
					Type = LaneOptionType.ValueUpDown,
					IgnoreForSimilarDuplicate = true,
					Name = OptionName,
					Options = new RoadBuilderLaneOptionValue[]
					{
						new() { Value = "1m" },
						new() { Value = "2m" },
						new() { Value = "5m" }
					}
				},
				new()
				{
					Name = "Decoration",
					Type = LaneOptionType.Decoration,
				},
			};

			AddOrGetComponent<RoadBuilderLaneInfo>()
				.WithRequireNone(RoadCategory.Train | RoadCategory.Subway | RoadCategory.Gravel | RoadCategory.Fence | RoadCategory.Pathway)
				.WithThumbnail("coui://roadbuildericons/RB_Median.svg")
				.AddLaneThumbnail("coui://roadbuildericons/Thumb_MedianSmall.svg");

			AddOrGetComponent<UIObject>().m_Icon = "coui://roadbuildericons/RB_Median_Centered.svg";

			var decoInfo = sections["RB Median 5"].AddOrGetComponent<RoadBuilderLaneDecorationInfo>();
			decoInfo.GrassThumbnail = "coui://roadbuildericons/RB_GrassMedian.svg";
			decoInfo.TreeThumbnail = "coui://roadbuildericons/RB_TreeMedian.svg";
			decoInfo.GrassAndTreeThumbnail = "coui://roadbuildericons/RB_TreeGrassMedian.svg";
			decoInfo.LaneGrassThumbnail = new[] { "coui://roadbuildericons/Thumb_SidewalkGrass.svg" };
			decoInfo.LaneTreeThumbnail = new[] { "coui://roadbuildericons/Thumb_SidewalkTree.svg" };
			decoInfo.LaneGrassAndTreeThumbnail = new[] { "coui://roadbuildericons/Thumb_SidewalkGrassTree.svg" };

			SetUp(sections["RB Median 1"], true, "1m");
			SetUp(sections["RB Median 2"], true, "2m");
			SetUp(sections["RB Median 5"], true, "5m", true).AddOrGetComponent<RoadBuilderLaneInfo>().AddLaneThumbnail("coui://roadbuildericons/Thumb_MedianWide.svg");

			SetUp(sections["Road Median 1"], false, "1m");
			SetUp(sections["Road Median 2"], false, "2m");
			SetUp(sections["Road Median 5"], false, "5m", true);
		}

		private NetSectionPrefab SetUp(NetSectionPrefab prefab, bool link, string value, bool hasGrass = false)
		{
			var laneInfo = link ? prefab.AddComponent<RoadBuilderLaneGroup>() : prefab.AddComponent<RoadBuilderVanillaLaneGroup>();
			laneInfo.GroupPrefab = this;
			laneInfo.Combination = hasGrass
			? new LaneOptionCombination[]
			{
				new()
				{
					OptionName = "Decoration",
					Value = string.Empty
				},
				new()
				{
					OptionName = OptionName,
					Value = value
				}
			}
			: new LaneOptionCombination[]
			{
				new()
				{
					OptionName = OptionName,
					Value = value
				}
			};

			return prefab;
		}
	}
}
