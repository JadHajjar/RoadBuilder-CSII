using Colossal.Serialization.Entities;

using Game;
using Game.Common;
using Game.Net;
using Game.Prefabs;
using Game.Tools;

using RoadBuilder.Domain.Components;
using RoadBuilder.Domain.Prefabs;
using RoadBuilder.Utilities;

using System.Collections.Generic;

using Unity.Collections;
using Unity.Entities;

namespace RoadBuilder.Systems
{
	public partial class RoadBuilderRoadTrackerSystem : GameSystemBase
	{
#nullable disable
		private PrefabSystem prefabSystem;
		private EntityQuery segmentQuery;
#nullable enable

		public IEnumerable<INetworkBuilderPrefab> UsedNetworkPrefabs { get; private set; } = new INetworkBuilderPrefab[0];

		protected override void OnCreate()
		{
			base.OnCreate();

			prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
			segmentQuery = SystemAPI.QueryBuilder()
				.WithAll<Edge, RoadBuilderNetwork, PrefabRef>()
				.WithAny<Created, Updated, Deleted>()
				.WithNone<Temp>()
				.Build();

			RequireForUpdate(segmentQuery);
		}

		protected override void OnGameLoadingComplete(Purpose purpose, GameMode mode)
		{
			base.OnGameLoadingComplete(purpose, mode);

			OnUpdate();
		}

		protected override void OnUpdate()
		{
			var allSegmentQuery = SystemAPI.QueryBuilder()
				.WithAll<Edge, RoadBuilderNetwork, PrefabRef>()
				.WithNone<Temp>()
				.Build();

			var prefabRefs = allSegmentQuery.ToComponentDataArray<PrefabRef>(Allocator.Temp);
			var prefabIds = new HashSet<Entity>();

			for (var i = 0; i < prefabRefs.Length; i++)
			{
				prefabIds.Add(prefabRefs[i].m_Prefab);
			}

			var list = new List<INetworkBuilderPrefab>();

			foreach (var prefabRef in prefabIds)
			{
				if (prefabSystem.TryGetSpecificPrefab<PrefabBase>(prefabRef, out var prefab) && prefab is INetworkBuilderPrefab builderPrefab)
				{
					list.Add(builderPrefab);
				}
			}

			UsedNetworkPrefabs = list;
		}
	}
}
