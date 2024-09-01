using Game.Prefabs;

using RoadBuilder.Domain.Prefabs;

using System;
using System.Collections.Generic;

using Unity.Entities;

namespace RoadBuilder.Domain.Components.Prefabs
{
	[ComponentMenu("RoadBuilder/", new Type[] { typeof(NetSectionPrefab), typeof(LaneGroupPrefab) })]
	public class RoadBuilderLaneAggregate : ComponentBase
	{
		public RoadBuilderAggregateSection[] LeftSections;
		public RoadBuilderAggregateSection[] RightSections;

		public override void GetArchetypeComponents(HashSet<ComponentType> components)
		{ }

		public override void GetPrefabComponents(HashSet<ComponentType> components)
		{ }
	}
}
