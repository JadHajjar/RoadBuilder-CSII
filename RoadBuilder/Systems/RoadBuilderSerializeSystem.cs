using Game;
using Game.Prefabs;
using Game.Tools;

using RoadBuilder.Domain.Components;
using RoadBuilder.Domain.Configurations;
using RoadBuilder.Domain.Prefabs;
using RoadBuilder.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;

using Unity.Collections;
using Unity.Entities;

namespace RoadBuilder.Systems
{
	public partial class RoadBuilderSerializeSystem : GameSystemBase
	{
		private static RoadBuilderSystem roadBuilderSystem;
		private static PrefabSystem prefabSystem;
		private EntityQuery roadBuilderNetsQuery;

		public IEnumerable<string> UsedConfigurations { get; private set; } = Enumerable.Empty<string>();

		protected override void OnCreate()
		{
			base.OnCreate();

			roadBuilderSystem = World.GetOrCreateSystemManaged<RoadBuilderSystem>();
			prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
			roadBuilderNetsQuery = SystemAPI.QueryBuilder()
				.WithAll<RoadBuilderNetwork, PrefabRef>()
				.WithNone<Temp>()
				.Build();
		}

		public static void RegisterRoadID(string type, string id)
		{
			Mod.Log.Debug($"Try Register: {type} - {id}");

			INetworkConfig config = type switch
			{
				nameof(RoadConfig) => new RoadConfig { ID = id },
				nameof(TrackConfig) => new TrackConfig { ID = id },
				nameof(FenceConfig) => new FenceConfig { ID = id },
				nameof(PathConfig) => new PathConfig { ID = id },
				_ => null
			};

			try
			{
				var prefab = NetworkPrefabGenerationUtil.CreatePrefab(config);

				if (!prefabSystem.TryGetPrefab(prefab.Prefab.GetPrefabID(), out _) && !roadBuilderSystem.Configurations.Any(x => x.Config.ID == prefab.Config.ID))
				{
					Mod.Log.Debug($"Added: {type} - {id}");

					prefabSystem.AddPrefab(prefab.Prefab);
				}
			}
			catch (Exception ex)
			{
				Mod.Log.Error(ex);
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
