using Colossal.Entities;
using Colossal.Serialization.Entities;

using Game;
using Game.Common;
using Game.Prefabs;
using Game.Tools;

using RoadBuilder.Domain;
using RoadBuilder.Domain.Components;
using RoadBuilder.Domain.Configuration;
using RoadBuilder.Domain.Enums;
using RoadBuilder.Utilities;

using System.Collections.Generic;
using System.Linq;

using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;

namespace RoadBuilder.Systems
{
	public partial class RoadBuilderSystem : GameSystemBase, ISerializable, IDefaultSerializable
	{
		public const ushort CURRENT_VERSION = 1;

		private PrefabSystem prefabSystem;
		private ModificationBarrier1 modificationBarrier;
		private EntityQuery prefabRefQuery;
		private RoadGenerationData roadGenerationData;

		public List<RoadConfig> Configurations { get; } = new();
		public Queue<RoadBuilderPrefab> UpdatedRoadPrefabsQueue { get; } = new();

		protected override void OnCreate()
		{
			base.OnCreate();

			prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
			modificationBarrier = World.GetOrCreateSystemManaged<ModificationBarrier1>();
			prefabRefQuery = SystemAPI.QueryBuilder()
				.WithAll<RoadConfigReferenceComponent, PrefabRef>()
				.WithNone<Temp>()
				.Build();
		}

		protected override void OnUpdate()
		{
			while (UpdatedRoadPrefabsQueue.Count > 0)
			{
				if (roadGenerationData is null)
				{
					Mod.Log.Warn("Generating roads before generation data was initialized");
				}

				var roadPrefab = UpdatedRoadPrefabsQueue.Dequeue();
				var roadPrefabGeneration = new RoadPrefabGenerationUtil(roadPrefab, roadGenerationData ?? new());

				roadPrefabGeneration.GenerateRoad();

				if (!roadPrefab.WasGenerated)
				{
					roadPrefab.WasGenerated = true;

					prefabSystem.AddPrefab(roadPrefab);

					continue;
				}

				// Update all existing roads that use this road configuration
				var job = new ApplyUpdatedJob()
				{
					Prefab = prefabSystem.GetEntity(roadPrefab),
					EntityTypeHandle = SystemAPI.GetEntityTypeHandle(),
					PrefabRefTypeHandle = SystemAPI.GetComponentTypeHandle<PrefabRef>(true),
					CommandBuffer = modificationBarrier.CreateCommandBuffer().AsParallelWriter()
				};

				JobChunkExtensions.ScheduleParallel(job, prefabRefQuery, Dependency);
			}
		}

		private struct ApplyUpdatedJob : IJobChunk
		{
			internal Entity Prefab;
			internal EntityTypeHandle EntityTypeHandle;
			internal ComponentTypeHandle<PrefabRef> PrefabRefTypeHandle;
			internal EntityCommandBuffer.ParallelWriter CommandBuffer;

			public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
			{
				var entities = chunk.GetNativeArray(EntityTypeHandle);
				var prefabRefs = chunk.GetNativeArray(ref PrefabRefTypeHandle);

				for (var i = 0; i < prefabRefs.Length; i++)
				{
					if (prefabRefs[i].m_Prefab == Prefab)
					{
						CommandBuffer.AddComponent(unfilteredChunkIndex, entities[i], default(Updated));
					}
				}
			}
		}

		public void Deserialize<TReader>(TReader reader) where TReader : IReader
		{
			Mod.Log.Info(nameof(Deserialize));

			Configurations.Clear();

			reader.Read(out ushort version);
			reader.Read(out int length);

			Mod.Log.Info(length);

			for (var i = 0; i < length; i++)
			{
				var config = new RoadConfig();
				var prefab = new RoadBuilderPrefab(config);

				reader.Read(config);

				config.ApplyVersionChanges();

				Configurations.Add(config);
				UpdatedRoadPrefabsQueue.Enqueue(prefab);
			}

			foreach (var config in LocalSaveUtil.LoadConfigs())
			{
				if (Configurations.Any(x => x.ID == config.ID))
				{
					continue;
				}

				var prefab = new RoadBuilderPrefab(config);

				config.ApplyVersionChanges();

				Configurations.Add(config);
				UpdatedRoadPrefabsQueue.Enqueue(prefab);
			}
		}

		public void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
		{
			Mod.Log.Info(nameof(Serialize));

			writer.Write(CURRENT_VERSION);
			writer.Write(Configurations.Count);

			foreach (var roadConfig in Configurations)
			{
				writer.Write(roadConfig);

				if (roadConfig.OriginalID != roadConfig.ID)
				{
					LocalSaveUtil.Save(roadConfig);
				}
			}
		}

		public void SetDefaults(Context context)
		{
			Mod.Log.Info(nameof(SetDefaults));

			Configurations.Clear();
		}

		public RoadConfig GetOrGenerateConfiguration(Entity entity)
		{
			if (!EntityManager.TryGetComponent<PrefabRef>(entity, out var prefabRef))
			{
				return new();
			}

			if (!prefabSystem.TryGetPrefab<RoadPrefab>(prefabRef.m_Prefab, out var roadPrefab))
			{
				return new();
			}

			if (roadPrefab is RoadBuilderPrefab roadBuilderPrefab)
			{
				return roadBuilderPrefab.Config;
			}

			return GenerateConfiguration(roadPrefab);
		}

		private RoadConfig GenerateConfiguration(RoadPrefab roadPrefab)
		{
			var config = new RoadConfig
			{
				SpeedLimit = roadPrefab.m_SpeedLimit,
				GeneratesTrafficLights = roadPrefab.m_TrafficLights,
				GeneratesZoningBlocks = roadPrefab.m_ZoneBlock is not null,
				MaxSlopeSteepness = roadPrefab.m_MaxSlopeSteepness,
				AggregateType = roadPrefab.m_AggregateType?.name,
			};

			if (roadPrefab.m_HighwayRules)
			{
				config.Category |= RoadCategory.Highway;
			}

			switch (roadPrefab.m_RoadType)
			{
				case RoadType.PublicTransport:
					config.Category |= RoadCategory.PublicTransport;
					break;
			}

			return config;
		}

		protected override void OnGamePreload(Purpose purpose, GameMode mode)
		{
			base.OnGamePreload(purpose, mode);

			roadGenerationData = new();

			var zoneBlockDataQuery = SystemAPI.QueryBuilder().WithAll<ZoneBlockData>().Build();
			var zoneBlockDataEntities = zoneBlockDataQuery.ToEntityArray(Allocator.Temp);

			for (var i = 0; i < zoneBlockDataEntities.Length; i++)
			{
				if (prefabSystem.TryGetPrefab<ZoneBlockPrefab>(zoneBlockDataEntities[i], out var prefab))
				{
					if (prefab.name == "Zone Block")
					{
						roadGenerationData.ZoneBlockPrefab = prefab;
					}
				}
			}

			var aggregateNetDataQuery = SystemAPI.QueryBuilder().WithAll<AggregateNetData>().Build();
			var aggregateNetDataEntities = aggregateNetDataQuery.ToEntityArray(Allocator.Temp);

			for (var i = 0; i < aggregateNetDataEntities.Length; i++)
			{
				if (prefabSystem.TryGetPrefab<AggregateNetPrefab>(aggregateNetDataEntities[i], out var prefab))
				{
					roadGenerationData.AggregateNetPrefabs[prefab.name] = prefab;
				}
			}
		}
	}
}
