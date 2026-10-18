using Colossal.Entities;

using Game;
using Game.Common;
using Game.Net;
using Game.Prefabs;
using Game.Tools;

using RoadBuilder.Domain.Components;
using RoadBuilder.Domain.Prefabs;
using RoadBuilder.Utilities;

using Unity.Collections;
using Unity.Entities;

namespace RoadBuilder.Systems
{
	public partial class RoadBuilderClearTagSystem : GameSystemBase
	{
#nullable disable
		private PrefabSystem prefabSystem;
		private EntityQuery segmentQuery;
#nullable enable

		protected override void OnCreate()
		{
			base.OnCreate();

			prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
			segmentQuery = SystemAPI.QueryBuilder()
				.WithAll<Edge, PrefabRef, RoadBuilderNetwork>()
				.WithAny<Created, Updated>()
				.WithNone<Temp>()
				.Build();

			RequireForUpdate(segmentQuery);
		}

		protected override void OnUpdate()
		{
			var entities = segmentQuery.ToEntityArray(Allocator.Temp);

			for (var i = 0; i < entities.Length; i++)
			{
				var entity = entities[i];

				if (!prefabSystem.TryGetSpecificPrefab<PrefabBase>(EntityManager.GetComponentData<PrefabRef>(entity), out var prefab) || prefab is not INetworkBuilderPrefab)
				{
					EntityManager.RemoveComponent<RoadBuilderNetwork>(entity);

					if (EntityManager.TryGetComponent<Edge>(entity, out var edge))
					{
						if (!HasOtherRoadBuilderEdge(edge.m_Start, entity))
						{
							EntityManager.RemoveComponent<RoadBuilderNetwork>(edge.m_Start);
						}

						if (!HasOtherRoadBuilderEdge(edge.m_End, entity))
						{
							EntityManager.RemoveComponent<RoadBuilderNetwork>(edge.m_End);
						}
					}
				}
			}
		}

		// Nodes shared with a road builder edge keep their tag
		private bool HasOtherRoadBuilderEdge(Entity node, Entity edge)
		{
			if (!EntityManager.TryGetBuffer<ConnectedEdge>(node, true, out var connectedEdges))
			{
				return false;
			}

			for (var i = 0; i < connectedEdges.Length; i++)
			{
				var connectedEdge = connectedEdges[i].m_Edge;

				if (connectedEdge != edge && EntityManager.HasComponent<RoadBuilderNetwork>(connectedEdge))
				{
					return true;
				}
			}

			return false;
		}
	}
}
