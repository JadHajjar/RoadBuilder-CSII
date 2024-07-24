using Game.Prefabs;

using RoadBuilder.Domain.Components;
using RoadBuilder.Domain.Components.Prefabs;
using System.Collections.Generic;

using Unity.Entities;

namespace RoadBuilder.Domain.Prefabs
{
    public class LaneGroupPrefab : PrefabBase
	{
		public RoadBuilderLaneOption[] Options;

		public List<NetSectionPrefab> LinkedSections { get; set; } = new();
		public string DisplayName { get; set; }

		public override void GetPrefabComponents(HashSet<ComponentType> components)
		{
			base.GetPrefabComponents(components);

			components.Add(ComponentType.ReadWrite<RoadBuilderPrefabData>());
		}
	}
}
