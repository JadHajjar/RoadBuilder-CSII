using Colossal.Entities;

using Game;
using Game.Common;
using Game.Net;
using Game.Prefabs;
using Game.Tools;

using RoadBuilder.Domain.Components;

using Unity.Collections;
using Unity.Entities;

namespace RoadBuilder.Systems
{
	public partial class RoadBuilderNodeCleanupSystem : GameSystemBase
	{
		private EntityQuery query;

		protected override void OnCreate()
		{
			base.OnCreate();

			query = SystemAPI.QueryBuilder().WithAll<RoadBuilderToBeDeletedComponent>().WithNone<Updated, Temp>().Build();

			RequireForUpdate(query);
		}

		protected override void OnUpdate()
		{
			foreach (var entity in query.ToEntityArray(Allocator.Temp))
			{
				if (EntityManager.TryGetBuffer<ConnectedEdge>(entity, true, out var connectedEdges))
				{
					if (connectedEdges.Length == 0)
					{
						EntityManager.AddComponent<Deleted>(entity);
					}
					else
					{
						EntityManager.RemoveComponent<RoadBuilderToBeDeletedComponent>(entity);

						foreach (var edge in connectedEdges)
						{
							EntityManager.AddComponent<Updated>(edge.m_Edge);

							if (EntityManager.TryGetComponent<Edge>(edge.m_Edge, out var edgeData))
							{
								EntityManager.AddComponent<Updated>(edgeData.m_Start);
								EntityManager.AddComponent<Updated>(edgeData.m_End);
							}

							EntityManager.SetComponentData(entity, new PrefabRef
							{
								m_Prefab = EntityManager.GetComponentData<PrefabRef>(connectedEdges[0].m_Edge).m_Prefab
							});
						}
					}
				}
				else
				{
					EntityManager.AddComponent<Deleted>(entity);
				}
			}
		}
	}
}
