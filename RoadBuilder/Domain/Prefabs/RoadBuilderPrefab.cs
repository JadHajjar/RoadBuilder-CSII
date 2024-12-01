using Game.Prefabs;

using RoadBuilder.Domain.Components;
using RoadBuilder.Domain.Configurations;

using System.Collections.Generic;

using Unity.Entities;

namespace RoadBuilder.Domain.Prefabs
{
	public class RoadBuilderPrefab : RoadPrefab, INetworkBuilderPrefab
	{
		public RoadConfig? Config { get; set; }
		public bool Deleted { get; set; }
		NetGeometryPrefab INetworkBuilderPrefab.Prefab => this;
		INetworkConfig? INetworkBuilderPrefab.Config { get => Config; set => Config = value as RoadConfig; }

		public override void GetPrefabComponents(HashSet<ComponentType> components)
		{
			base.GetPrefabComponents(components);

			if (!Deleted)
			{
				components.Add(ComponentType.ReadWrite<RoadBuilderPrefabData>());
			}
			else
			{
				components.Add(ComponentType.ReadOnly<DiscardedRoadBuilderPrefab>());
			}
		}
	}
}
