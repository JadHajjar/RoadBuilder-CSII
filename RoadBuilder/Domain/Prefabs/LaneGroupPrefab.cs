using Game.Prefabs;
using RoadBuilder.Domain.Components.Prefabs;
using System.Collections.Generic;

namespace RoadBuilder.Domain.Prefabs
{
    public class LaneGroupPrefab : PrefabBase
	{
		public RoadBuilderLaneOption[] Options;

		public List<NetSectionPrefab> LinkedSections { get; set; } = new();
		public string DisplayName { get; set; }
	}
}
