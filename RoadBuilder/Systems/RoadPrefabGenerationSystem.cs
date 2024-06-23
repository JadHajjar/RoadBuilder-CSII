using Colossal.Entities;

using Game;
using Game.Common;
using Game.Prefabs;
using Game.Tools;

using RoadBuilder.Domain.Components;
using RoadBuilder.Domain.Configuration;
using RoadBuilder.Utilities;

using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;

namespace RoadBuilder.Systems
{
	public partial class RoadPrefabGenerationSystem : GameSystemBase
	{
		private PrefabSystem prefabSystem;
		private ModificationBarrier1 modificationBarrier;
		private EntityQuery prefabRefQuery;
		private EntityQuery roadConfigurationQuery;

		protected override void OnCreate()
		{
			base.OnCreate();

			prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
			modificationBarrier = World.GetOrCreateSystemManaged<ModificationBarrier1>();
			prefabRefQuery = SystemAPI.QueryBuilder()
				.WithAll<RoadConfigReferenceComponent, PrefabRef>()
				.WithNone<Temp>()
				.Build();
			roadConfigurationQuery = SystemAPI.QueryBuilder()
				.WithAll<RoadConfigComponent>()
				.Build();
		}

		protected override void OnUpdate()
		{
			while (RoadConfigUtil.UpdatedRoadPrefabsQueue.Count > 0)
			{
				var roadPrefab = RoadConfigUtil.UpdatedRoadPrefabsQueue.Dequeue();
				var prefabEntity = prefabSystem.GetEntity(roadPrefab);
				var roadConfig = roadPrefab.Config;
				var oldID = roadConfig.ID;

				var roadPrefabGeneration = new RoadPrefabGenerationUtil(roadConfig, prefabSystem);
				
				roadPrefabGeneration.GenerateNewId();
				roadPrefabGeneration.GenerateRoad(roadPrefab);

				var roadConfigs = roadConfigurationQuery.ToComponentDataArray<RoadConfigComponent>(Allocator.Temp);
				var configurationEntities = roadConfigurationQuery.ToEntityArray(Allocator.Temp);
				var existingEntityFound = false;

				for (var i = 0; i < roadConfigs.Length; i++)
				{
					if (roadConfigs[i].ID == oldID)
					{
						existingEntityFound = true;

						SaveRoadConfig(configurationEntities[i], roadConfig);
					}
				}

				if (!existingEntityFound)
				{
					SaveRoadConfig(EntityManager.CreateEntity(), roadConfig);
				}

				// Move later to on-game-exit
				LocalSaveUtil.DeleteLocalConfig(oldID);
				LocalSaveUtil.Save(roadConfig);

				// Update all existing roads that use this road configuration
				var job = new ApplyUpdatedJob()
				{
					prefab = prefabEntity,
					entityTypeHandle = SystemAPI.GetEntityTypeHandle(),
					prefabRefTypeHandle = SystemAPI.GetComponentTypeHandle<PrefabRef>(true),
					commandBuffer = modificationBarrier.CreateCommandBuffer().AsParallelWriter()
				};

				JobChunkExtensions.ScheduleParallel(job, prefabRefQuery, Dependency);
			}
		}

		private void SaveRoadConfig(Entity entity, RoadConfig roadConfig)
		{
			EntityManager.RemoveComponent<RoadConfigComponent>(entity);
			EntityManager.AddComponentData(entity, new RoadConfigComponent
			{
				ID = roadConfig.ID,
				Name = roadConfig.Name
			});

			if (!EntityManager.TryGetBuffer<LaneConfigComponent>(entity, false, out var laneBuffer))
			{
				laneBuffer = EntityManager.AddBuffer<LaneConfigComponent>(entity);
			}
			else
			{
				laneBuffer.Clear();
			}

            foreach (var lane in roadConfig.Lanes)
            {
				laneBuffer.Add(new LaneConfigComponent
				{
					PrefabName = lane.PrefabName,
				});
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
	}
}
