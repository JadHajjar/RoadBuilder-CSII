using Game;
using Game.Prefabs;
using Game.Tools;

using RoadBuilder.Domain.Components;
using RoadBuilder.Domain.Configurations;
using RoadBuilder.Domain.Prefabs;

using System.Collections.Generic;
using System.Linq;

using Unity.Collections;
using Unity.Entities;

namespace RoadBuilder.Systems
{
	public partial class RoadBuilderSerializeSystem : GameSystemBase
	{
		private static PrefabSystem prefabSystem;
		private EntityQuery roadBuilderNetsQuery;

		public IEnumerable<string> UsedConfigurations { get; private set; } = Enumerable.Empty<string>();

		protected override void OnCreate()
		{
			base.OnCreate();

			prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
			roadBuilderNetsQuery = SystemAPI.QueryBuilder()
				.WithAll<RoadBuilderNetwork, PrefabRef>()
				.WithNone<Temp>()
				.Build();
		}

		public static void RegisterRoadID(string type, string id)
		{
			PrefabBase prefab;

			Mod.Log.Debug($"Try Register: {type} - {id}");

			switch (type)
			{
				case nameof(RoadConfig):
					prefab = new RoadBuilderPrefab(new RoadConfig { ID = id });
					break;
				case nameof(TrackConfig):
					prefab = new TrackBuilderPrefab(new TrackConfig { ID = id });
					break;
				case nameof(FenceConfig):
					prefab = new FenceBuilderPrefab(new FenceConfig { ID = id });
					break;
				default:
					return;
			}

			if (!prefabSystem.TryGetPrefab(prefab.GetPrefabID(), out _))
			{
				Mod.Log.Debug($"Added: {type} - {id}");

				prefabSystem.AddPrefab(prefab);
			}
		}

		protected override void OnUpdate()
		{
			if (!SystemAPI.TryGetSingletonBuffer<NetworkConfigBuffer>(out var networkConfigBuffer))
			{
				var singleton = EntityManager.CreateSingletonBuffer<NetworkConfigBuffer>();

				networkConfigBuffer = EntityManager.GetBuffer<NetworkConfigBuffer>(singleton);
			}

			var prefabRefs = roadBuilderNetsQuery.ToComponentDataArray<PrefabRef>(Allocator.Temp);

			networkConfigBuffer.Clear();

			var addedIds = new List<string>();

			for (var i = 0; i < prefabRefs.Length; i++)
			{
				if (!prefabSystem.TryGetPrefab<NetGeometryPrefab>(prefabRefs[i], out var prefabBase)
					|| prefabBase is not INetworkBuilderPrefab prefab
					|| addedIds.Contains(prefab.Config.ID))
				{
					continue;
				}

				addedIds.Add(prefab.Config.ID);

				Mod.Log.Debug($"Serializing: {prefab.Config.GetType().Name} - {prefab.Config.ID}");

				networkConfigBuffer.Add(new NetworkConfigBuffer
				{
					ConfigurationType = prefab.Config.GetType().Name,
					ConfigurationID = prefab.Config.ID
				});
			}

			Mod.Log.Debug($"Serialized {addedIds.Count}");

			UsedConfigurations = addedIds;
		}
	}
}
