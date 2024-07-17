using Game.Prefabs;

using RoadBuilder.Domain.Components.Prefabs;
using RoadBuilder.Domain.Enums;

using System.Collections.Generic;

namespace RoadBuilder.LaneGroups
{
	public class TrainGroupPrefab : BaseLaneGroupPrefab
	{
		private const string OptionName = "Two-way Support";

		public TrainGroupPrefab(Dictionary<string, NetSectionPrefab> sections) : base(sections)
		{
			DisplayName = "Train Track";
			Options = new RoadBuilderLaneOption[]
			{
				new()
				{
					DefaultValue = "1",
					Name = OptionName,
					Options = new RoadBuilderLaneOptionValue[]
					{
						new() { Value = "1" },
						new() { Value = "2" },
					}
				}
			};

			AddComponent<RoadBuilderLaneInfo>()
				.WithExcluded(RoadCategory.Gravel | RoadCategory.Tiled | RoadCategory.Pathway | RoadCategory.Fence)
				.WithFrontThumbnail("coui://roadbuildericons/RB_TrainFront.svg")
				.WithBackThumbnail("coui://roadbuildericons/RB_TrainRear.svg");

			AddComponent<UIObject>().m_Icon = "coui://roadbuildericons/RB_TrainFront.svg";

			SetUp(sections["Train Track Section 4"], "1");
			SetUp(sections["Train Track Twoway Section 4"], "2");
		}

		private void SetUp(NetSectionPrefab prefab, string value)
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
		}
	}
}
