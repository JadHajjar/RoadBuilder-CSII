using Game.Prefabs;

using RoadBuilder.Domain.Enums;

using System;
using System.Collections.Generic;

using Unity.Entities;

namespace RoadBuilder.Domain.Components.Prefabs
{
	[ComponentMenu("RoadBuilder/", new Type[] { typeof(NetSectionPrefab) })]
	public class RoadBuilderEdgeLaneInfo : ComponentBase
	{
		public NetSectionPrefab? LeftSidePrefab;
		public NetSectionPrefab? RightSidePrefab;
		public bool AddSidewalkStateOnNode;
		public bool DoNotRequireBeingOnEdge;

		internal NetSectionPrefab? SidePrefab { set => LeftSidePrefab = RightSidePrefab = value; }

		public override void GetArchetypeComponents(HashSet<ComponentType> components)
		{ }

		public override void GetPrefabComponents(HashSet<ComponentType> components)
		{ }

		public RoadBuilderEdgeLaneInfo WithLeftSide(NetSectionPrefab prefab)
		{
			LeftSidePrefab = prefab;
			return this;
		}

		public RoadBuilderEdgeLaneInfo WithRightSide(NetSectionPrefab prefab)
		{
			RightSidePrefab = prefab;
			return this;
		}

		public RoadBuilderEdgeLaneInfo WithSide(NetSectionPrefab prefab)
		{
			LeftSidePrefab = RightSidePrefab = prefab;
			return this;
		}

		public RoadBuilderEdgeLaneInfo WithAddSidewalkStateOnNode()
		{
			AddSidewalkStateOnNode = true;
			return this;
		}

		public RoadBuilderEdgeLaneInfo WithDoNotRequireBeingOnEdge()
		{
			DoNotRequireBeingOnEdge = true;
			return this;
		}
	}
}
