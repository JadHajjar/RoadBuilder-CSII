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
	public partial class RoadBuilderApplyTagSystem : GameSystemBase
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
				.WithAll<Edge, PrefabRef>()
				.WithAny<Created, Updated>()
				.WithNone<RoadBuilderNetwork, Temp>()
				.Build();

			RequireForUpdate(segmentQuery);
		}

		protected override void OnUpdate()
		{
			var entities = segmentQuery.ToEntityArray(Allocator.Temp);

			for (var i = 0; i < entities.Length; i++)
			{
				var entity = entities[i];

				if (prefabSystem.TryGetSpecificPrefab<PrefabBase>(EntityManager.GetComponentData<PrefabRef>(entity), out var prefab) && prefab is INetworkBuilderPrefab builderPrefab && !builderPrefab.Deleted)
				{
					EntityManager.AddComponent<RoadBuilderNetwork>(entity);

					if (EntityManager.TryGetComponent<Edge>(entity, out var edge))
					{
						EntityManager.AddComponent<RoadBuilderNetwork>(edge.m_Start);
						EntityManager.AddComponent<RoadBuilderNetwork>(edge.m_End);
					}
				}
			}
		}
	}
}
