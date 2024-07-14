using Game.Prefabs;

using RoadBuilder.Domain.Components;

using System.Collections.Generic;

namespace RoadBuilder.Domain.Prefabs
{
	public class LaneGroupPrefab : PrefabBase
	{
		public RoadBuilderLaneOptionInfo[] Options;

		public List<NetSectionPrefab> LinkedSections { get; set; } = new();
		public string DisplayName { get; set; }
	}
}
