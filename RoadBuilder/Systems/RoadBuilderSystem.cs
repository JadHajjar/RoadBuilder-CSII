using Colossal.Json;
using Colossal.Serialization.Entities;
using Colossal.UI.Binding;

using Game;
using Game.Common;
using Game.Prefabs;
using Game.Tools;

using RoadBuilder.Domain;
using RoadBuilder.Domain.Components;
using RoadBuilder.Domain.Configuration;
using RoadBuilder.Utilities;

using System.Collections.Generic;
using System.Linq;

using Unity.Burst.Intrinsics;
using Unity.Entities;

namespace RoadBuilder.Systems
{
	public partial class RoadBuilderSystem : GameSystemBase, ISerializable, IDefaultSerializable
	{
		public const ushort CURRENT_VERSION = 1;

		private PrefabSystem prefabSystem;
		private ModificationBarrier1 modificationBarrier;
		private EntityQuery prefabRefQuery;

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
				var roadPrefab = UpdatedRoadPrefabsQueue.Dequeue();
				var roadPrefabGeneration = new RoadPrefabGenerationUtil(roadPrefab, prefabSystem);

				roadPrefabGeneration.GenerateRoad();

				// Update all existing roads that use this road configuration
				var job = new ApplyUpdatedJob()
				{
					prefab = prefabSystem.GetEntity(roadPrefab),
					entityTypeHandle = SystemAPI.GetEntityTypeHandle(),
					prefabRefTypeHandle = SystemAPI.GetComponentTypeHandle<PrefabRef>(true),
					commandBuffer = modificationBarrier.CreateCommandBuffer().AsParallelWriter()
				};

				JobChunkExtensions.ScheduleParallel(job, prefabRefQuery, Dependency);
			}
		}

		private struct ApplyUpdatedJob : IJobChunk
		{
			internal Entity prefab;
			internal EntityTypeHandle entityTypeHandle;
			internal ComponentTypeHandle<PrefabRef> prefabRefTypeHandle;
			internal EntityCommandBuffer.ParallelWriter commandBuffer;

			public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
			{
				var entities = chunk.GetNativeArray(entityTypeHandle);
				var prefabRefs = chunk.GetNativeArray(ref prefabRefTypeHandle);

				for (var i = 0; i < prefabRefs.Length; i++)
				{
					if (prefabRefs[i].m_Prefab == prefab)
					{
						commandBuffer.AddComponent(unfilteredChunkIndex, entities[i], default(Updated));
					}
				}
			}
		}

		public void Deserialize<TReader>(TReader reader) where TReader : IReader
		{
			Configurations.Clear();

			reader.Read(out int version);
			reader.Read(out int length);

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
			Configurations.Clear();
		}
	}
}
