using Game.Prefabs;

using RoadBuilder.Domain.Components.Prefabs;
using RoadBuilder.Domain.Enums;

namespace RoadBuilder.LaneGroups
{
	public class MedianGroupPrefab : BaseLaneGroupPrefab
	{
		private const string OptionName = "Median Width";

		public override bool Initialize()
		{
			Prefab!.Options = new RoadBuilderLaneOption[]
			{
				new()
				{
					DefaultValue = "2m",
					Type = LaneOptionType.LaneWidth,
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

			Prefab.AddOrGetComponent<RoadBuilderLaneInfo>()
				.WithRequireNone(RoadCategory.Train | RoadCategory.Subway | RoadCategory.Gravel | RoadCategory.Fence | RoadCategory.Pathway)
				.WithThumbnail("coui://roadbuildericons/RB_Median.svg")
				.AddLaneThumbnail("coui://roadbuildericons/Thumb_MedianSmall.svg");

			Prefab.AddOrGetComponent<UIObject>().m_Icon = "coui://roadbuildericons/RB_Median_Centered.svg";

			var decoInfo = Sections!["RB Median 5"].AddOrGetComponent<RoadBuilderLaneDecorationInfo>();
			decoInfo.GrassThumbnail = "coui://roadbuildericons/RB_GrassMedian.svg";
			decoInfo.TreeThumbnail = "coui://roadbuildericons/RB_TreeMedian.svg";
			decoInfo.GrassAndTreeThumbnail = "coui://roadbuildericons/RB_TreeGrassMedian.svg";
			decoInfo.LaneGrassThumbnail = new[] { "coui://roadbuildericons/Thumb_SidewalkGrass.svg" };
			decoInfo.LaneTreeThumbnail = new[] { "coui://roadbuildericons/Thumb_SidewalkTree.svg" };
			decoInfo.LaneGrassAndTreeThumbnail = new[] { "coui://roadbuildericons/Thumb_SidewalkGrassTree.svg" };

			SetUp<RoadBuilderLaneGroup>(Sections["RB Median 1"], "1m");
			SetUp<RoadBuilderLaneGroup>(Sections["RB Median 2"], "2m");
			SetUp<RoadBuilderLaneGroup>(Sections["RB Median 5"], "5m", true).AddOrGetComponent<RoadBuilderLaneInfo>().AddLaneThumbnail("coui://roadbuildericons/Thumb_MedianWide.svg");

			SetUp<RoadBuilderVanillaLaneGroup>(Sections["Road Median 1"], "1m");
			SetUp<RoadBuilderVanillaLaneGroup>(Sections["Road Median 2"], "2m");
			SetUp<RoadBuilderVanillaLaneGroup>(Sections["Road Median 5"], "5m", true);
			SetUp<RoadBuilderVanillaLaneGroup>(Sections["Parking Road Median 5"], "5m", true);

			return true;
		}

		private NetSectionPrefab SetUp<T>(NetSectionPrefab prefab, string value, bool hasGrass = false) where T : RoadBuilderLaneGroup
		{
			var laneInfo = prefab.AddComponent<T>();
			laneInfo.GroupPrefab = Prefab;
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
