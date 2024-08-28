using Colossal.Entities;
using Colossal.Serialization.Entities;

using Game;
using Game.Common;
using Game.Net;
using Game.Prefabs;
using Game.Tools;

using RoadBuilder.Domain.Components;

using System.Collections.Generic;
using System.Reflection;

using Unity.Collections;
using Unity.Entities;

namespace RoadBuilder.Systems
{
	public partial class RoadBuilderPrefabUpdateSystem : GameSystemBase
	{
		private RoadBuilderSystem roadBuilderSystem;
		private EntityQuery queryUpdated;
		private EntityQuery queryAll;
		private EntityQuery prefabRefQuery;
		private PrefabSystem prefabSystem;
		private Dictionary<PrefabID, int> prefabSystem_PrefabIndices;
		private List<PrefabBase> prefabSystem_Prefabs;

		protected override void OnCreate()
		{
			base.OnCreate();

			roadBuilderSystem = World.GetOrCreateSystemManaged<RoadBuilderSystem>();
			prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
			prefabSystem_PrefabIndices = typeof(PrefabSystem).GetField("m_PrefabIndices", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(prefabSystem) as Dictionary<PrefabID, int>;
			prefabSystem_Prefabs = typeof(PrefabSystem).GetField("m_Prefabs", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(prefabSystem) as List<PrefabBase>;
			queryUpdated = SystemAPI.QueryBuilder().WithAll<RoadBuilderPrefabData, Updated>().Build();
			queryAll = SystemAPI.QueryBuilder().WithAll<RoadBuilderPrefabData>().WithAny<Created, Updated>().Build();
			prefabRefQuery = SystemAPI.QueryBuilder()
				.WithAll<RoadBuilderNetwork, PrefabRef, Edge>()
				.WithNone<Temp>()
				.Build();

			RequireForUpdate(queryAll);
		}

		protected override void OnGamePreload(Purpose purpose, GameMode mode)
		{
			base.OnGamePreload(purpose, mode);

			Enabled = false;
		}

		protected override void OnGameLoadingComplete(Purpose purpose, GameMode mode)
		{
			base.OnGameLoadingComplete(purpose, mode);

			Enabled = true;
		}

		protected override void OnUpdate()
		{
			Mod.ReloadActiveLocale();

			roadBuilderSystem.UpdateConfigurationList();

			var prefabs = queryUpdated.ToEntityArray(Allocator.Temp);
			var edgeEntities = prefabRefQuery.ToEntityArray(Allocator.Temp);

			var edgeList = new HashSet<Entity>(edgeEntities);

			for (var j = 0; j < prefabs.Length; j++)
			{
				for (var i = 0; i < edgeEntities.Length; i++)
				{
					if (EntityManager.TryGetComponent<PrefabRef>(edgeEntities[i], out var prefabRef) && prefabRef.m_Prefab == prefabs[j])
					{
						foreach (var edge in GetEdges(edgeEntities[i]))
						{
							edgeList.Add(edge);
						}
					}
				}

				if (prefabSystem.TryGetPrefab<PrefabBase>(prefabs[j], out var prefab))
				{
					prefabSystem_PrefabIndices[prefab.GetPrefabID()] = prefabSystem_Prefabs.IndexOf(prefab);
				}
			}

			foreach (var entity in edgeList)
			{
				UpdateEdge(entity);
			}
		}

		public IEnumerable<Entity> GetEdges(Entity entity)
		{
			if (!EntityManager.TryGetComponent<Edge>(entity, out var edge))
			{
				yield break;
			}

			if (EntityManager.TryGetBuffer<ConnectedEdge>(edge.m_Start, true, out var connectedEdges1))
			{
				for (var i = 0; i < connectedEdges1.Length; i++)
				{
					yield return connectedEdges1[i].m_Edge;
				}
			}

			if (EntityManager.TryGetBuffer<ConnectedEdge>(edge.m_End, true, out var connectedEdges2))
			{
				for (var i = 0; i < connectedEdges2.Length; i++)
				{
					yield return connectedEdges2[i].m_Edge;
				}
			}
		}

		public void UpdateEdge(Entity entity)
		{
			UpdateEntity(entity);

			if (EntityManager.TryGetComponent<Composition>(entity, out var comp))
			{
				UpdateEntity(comp.m_StartNode);
				UpdateEntity(comp.m_EndNode);
			}

			if (EntityManager.TryGetComponent<Edge>(entity, out var edge))
			{
				UpdateEntity(edge.m_Start);
				UpdateEntity(edge.m_End);
			}

			if (EntityManager.TryGetBuffer<Game.Net.SubLane>(entity, true, out var subLanes))
			{
				for (var j = 0; j < subLanes.Length; j++)
				{
					UpdateEntity(subLanes[j].m_SubLane);
				}
			}

			if (EntityManager.TryGetBuffer<Game.Objects.SubObject>(entity, true, out var subObjects))
			{
				for (var j = 0; j < subObjects.Length; j++)
				{
					UpdateEntity(subObjects[j].m_SubObject);
				}
			}
		}

		private void UpdateEntity(Entity entity)
		{
			if (entity != Entity.Null && !EntityManager.HasComponent<Deleted>(entity))
			{
				EntityManager.AddComponent<Updated>(entity);
			}
		}
	}
}
