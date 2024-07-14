using Game.Prefabs;

using RoadBuilder.Domain.Prefabs;

using System;
using System.Collections.Generic;

using Unity.Entities;

namespace RoadBuilder.Domain.Components
{
	[ComponentMenu("RoadBuilder/", new Type[] { typeof(NetSectionPrefab) })]
	public class RoadBuilderLaneGroupItem : ComponentBase
	{
		public LaneGroupPrefab GroupPrefab;
		public LaneOptionCombination[] Combination;

		public override void GetArchetypeComponents(HashSet<ComponentType> components)
		{ }

		public override void GetPrefabComponents(HashSet<ComponentType> components)
		{ }
	}
}
