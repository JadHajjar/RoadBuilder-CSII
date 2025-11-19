using Game.Prefabs;

using RoadBuilder.Domain.Components.Prefabs;
using RoadBuilder.Domain.Enums;

namespace RoadBuilder.LaneGroups
{
	public class BikeGroupPrefab : BaseLaneGroupPrefab
	{
		private const string OptionName = "Two-way Support";

		public override bool Initialize()
		{
			Prefab!.Options = new RoadBuilderLaneOption[]
			{
				new()
				{
					Name = OptionName,
					Type = LaneOptionType.TwoWay,
				}
			};

			Prefab.AddComponent<RoadBuilderLaneInfo>()
				.WithRequireAll(RoadCategory.Pathway)
				.WithColor(43, 161, 82, 200)
				.WithFrontThumbnail("coui://roadbuildericons/RB_BikeFront.svg")
				.WithBackThumbnail("coui://roadbuildericons/RB_BikeRear.svg")
				.AddLaneThumbnail("coui://roadbuildericons/Thumb_BikeLane.svg");

			Prefab.AddComponent<UIObject>().m_Icon = "coui://roadbuildericons/RB_BikeCentered.svg";

			SetUp(Sections!["OneWayBikeSection 3"], false).WithRequireAll(RoadCategory.Pathway).WithMedian();
			SetUp(Sections!["OneWayBikeSection 3"], "BikeSideSection 0", "BikeSideSection 0");

			SetUp(Sections!["TwoWayBikeSection 3"], true).WithRequireAll(RoadCategory.Pathway).WithMedian();
			SetUp(Sections!["TwoWayBikeSection 3"], "BikeSideSection 0", "BikeSideSection 0");
			Sections!["TwoWayBikeSection 3"].AddComponent<RoadBuilderLaneAggregate>().RightSections = new[]
			{
				new RoadBuilderAggregateSection
				{
					Section = Sections["TwoWayBikeSection 0"],
					Invert = true
				} 
			};

			Sections["TwoWayBikeSection 0"].AddComponent<RoadBuilderIgnoreSection>();

			return true;
		}

		private RoadBuilderLaneInfo SetUp(NetSectionPrefab prefab, bool value)
		{
			var laneInfo = prefab.AddComponent<RoadBuilderLaneGroup>();
			laneInfo.GroupPrefab = Prefab;
			laneInfo.Combination = value ? new LaneOptionCombination[]
			{
				new()
				{
					OptionName = OptionName,
					Value = string.Empty
				}
			} : new LaneOptionCombination[0];

			return prefab.AddComponent<RoadBuilderLaneInfo>();
		}

		private void SetUp(NetSectionPrefab prefab, string leftSide, string rightSide)
		{
			var laneInfo = prefab.AddComponent<RoadBuilderEdgeLaneInfo>();

			laneInfo.LeftSidePrefab = Sections![leftSide];
			laneInfo.RightSidePrefab = Sections![rightSide];
			laneInfo.DoNotRequireBeingOnEdge = true;
		}
	}
}
