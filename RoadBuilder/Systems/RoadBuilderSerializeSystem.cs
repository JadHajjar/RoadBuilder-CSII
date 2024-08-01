using Colossal.Serialization.Entities;

using Game;
using Game.Prefabs;
using Game.SceneFlow;

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
		public const ushort CURRENT_VERSION = 1;

		private static RoadBuilderSystem roadBuilderSystem;
		private static PrefabSystem prefabSystem;
		private static List<INetworkBuilderPrefab> prefabsToUpdate = new();

		protected override void OnCreate()
		{
			base.OnCreate();

			roadBuilderSystem = World.GetOrCreateSystemManaged<RoadBuilderSystem>();
			prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
		}

		protected override void OnUpdate()
		{
			var roadBuilderNetsQuery = SystemAPI.QueryBuilder().WithAll<RoadBuilderNetwork, PrefabRef>().Build();
			var prefabRefs = roadBuilderNetsQuery.ToComponentDataArray<PrefabRef>(Allocator.Temp);

			var placedNetworks = CreateNetworksList(in prefabRefs);

			foreach (var prefabName in placedNetworks)
			{
				var entity = EntityManager.CreateEntity();

				EntityManager.AddComponentData(entity, new NetworkConfigComponent { NetworkId = prefabName });
			}

			foreach (var config in roadBuilderSystem.Configurations.Values)
			{
				if (Mod.Settings.SaveUsedRoadsOnly && !placedNetworks.Contains(config.Config.ID))
				{
					continue;
				}

				LocalSaveUtil.Save(config.Config);
			}
		}

		protected override void OnGameLoadingComplete(Purpose purpose, GameMode mode)
		{
			base.OnGameLoadingComplete(purpose, mode);

			foreach (var item in prefabsToUpdate)
			{
				roadBuilderSystem.UpdatePrefab(item.Prefab);
			}

			prefabsToUpdate.Clear();
		}

		private List<string> CreateNetworksList(in NativeArray<PrefabRef> prefabRefs)
		{
			var list = new List<string>();

			for (var i = 0; i < prefabRefs.Length; i++)
			{
				if (prefabSystem.GetPrefab<PrefabBase>(prefabRefs[i]) is not INetworkBuilderPrefab prefab)
				{
					continue;
				}

				if (!list.Contains(prefab.Prefab.name))
				{
					list.Add(prefab.Prefab.name);
				}
			}

			return list;
		}

		public static void DeserializeNetwork<TReader>(TReader reader) where TReader : IReader
		{
			reader.Read(out ushort version);
			reader.Read(out string type);

			INetworkConfig config = type switch
			{
				nameof(RoadConfig) => new RoadConfig(),
				nameof(TrackConfig) => new TrackConfig(),
				nameof(FenceConfig) => new FenceConfig(),
				nameof(PathConfig) => new PathConfig(),
				_ => throw new Exception("Unknown Configuration Type: " + type),
			};

			config.Version = version;

			reader.Read(config);

			config.ApplyVersionChanges();

			Mod.Log.Debug($"DeserializeNetwork: {type} {config.ID}");

			var prefab = roadBuilderSystem.AddPrefab(config);

			if (prefab is not null)
			{
				prefabsToUpdate.Add(prefab);
			}
		}

		public static void SerializeNetwork<TWriter>(TWriter writer, string networkId) where TWriter : IWriter
		{
			if (!roadBuilderSystem.Configurations.TryGetValue(networkId, out var prefab))
			{
				Mod.Log.Error("Trying to save a prefab that has no configuration: " + networkId);

				return;
			}

			writer.Write(CURRENT_VERSION);
			writer.Write(prefab.Config.GetType().Name);
			writer.Write(prefab.Config);
		}
	}
}
