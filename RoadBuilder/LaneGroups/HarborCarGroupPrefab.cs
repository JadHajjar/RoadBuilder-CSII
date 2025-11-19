using Game.Prefabs;

using RoadBuilder.Domain.Components.Prefabs;
using RoadBuilder.Domain.Enums;

namespace RoadBuilder.LaneGroups
{
	public class HarborCarGroupPrefab : BaseLaneGroupPrefab
	{
		private const string OptionName2 = "Transport Option";

		public override bool Initialize()
		{
			if (!Sections!.ContainsKey("Harbor Drive Section 4"))
				return false;

			Prefab!.Options = new RoadBuilderLaneOption[]
			{
				new()
				{
					DefaultValue = "",
					Name = OptionName2,
					Options = new RoadBuilderLaneOptionValue[]
					{
						new()
						{
							Value = "",
							ThumbnailUrl = "coui://roadbuildericons/RB_CarWhite.svg"
						},
						new()
						{
							Value = "Transport" ,
							ThumbnailUrl = "coui://roadbuildericons/RB_BusWhite.svg"
						},
					}
				}
			};

			Prefab.AddComponent<RoadBuilderLaneInfo>()
				.WithColor(145, 155, 163, 150)
				.WithRequireNone(RoadCategory.NonAsphalt & ~RoadCategory.Gravel & ~RoadCategory.Tiled)
				.WithFrontThumbnail("coui://roadbuildericons/RB_CarFront.svg")
				.WithBackThumbnail("coui://roadbuildericons/RB_CarRear.svg");

			Prefab.AddComponent<UIObject>().m_Icon = "coui://roadbuildericons/RB_Cargo_Centered.svg";

			SetUp(Sections!["Harbor Drive Section 4"], "").AddOrGetComponent<RoadBuilderLaneInfo>().WithRequireNone(RoadCategory.Gravel | RoadCategory.Tiled);
			SetUp(Sections["Harbor Drive Section 4 - Transport Option"], "Transport").AddOrGetComponent<RoadBuilderLaneInfo>().WithRequireNone(RoadCategory.Gravel | RoadCategory.Tiled)
				.WithFrontThumbnail("coui://roadbuildericons/RB_CarBusFront.svg").WithBackThumbnail("coui://roadbuildericons/RB_CarBusRear.svg");

			return true;
		}

		private NetSectionPrefab SetUp(NetSectionPrefab prefab, string value2)
		{
			var laneInfo = prefab.AddComponent<RoadBuilderLaneGroup>();
			laneInfo.GroupPrefab = Prefab;
			laneInfo.Combination = new LaneOptionCombination[]
			{
				new()
				{
					OptionName = OptionName2,
					Value = value2
				},
			};

			return prefab;
		}
	}
}
