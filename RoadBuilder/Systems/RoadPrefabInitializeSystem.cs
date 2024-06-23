using Colossal.Entities;

using Game;
using Game.Common;
using Game.Prefabs;
using Game.Tools;

using RoadBuilder.Domain.Components;
using RoadBuilder.Domain.Configuration;
using RoadBuilder.Utilities;

using System;

using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;

namespace RoadBuilder.Systems
{
	public partial class RoadPrefabInitializeSystem : GameSystemBase
	{
		private PrefabSystem prefabSystem;
		private EntityQuery roadConfigurationQuery;

		protected override void OnCreate()
		{
			base.OnCreate();

			prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
			roadConfigurationQuery = SystemAPI.QueryBuilder()
				.WithAll<RoadConfigComponent>()
				.WithNone<RoadConfigComponent.Generated>()
				.Build();

			RequireForUpdate(roadConfigurationQuery);
		}

		protected override void OnUpdate()
		{
			var roadConfigEntities = roadConfigurationQuery.ToEntityArray(Allocator.Temp);

            for (var i = 0; i < roadConfigEntities.Length; i++)
            {
				var roadConfig = GenerateRoadConfig(roadConfigEntities[i]);
				var roadPrefabGeneration = new RoadPrefabGenerationUtil(roadConfig, prefabSystem);
				var prefab = roadPrefabGeneration.GenerateRoad();

				prefabSystem.AddPrefab(prefab);
			}
		}

		private RoadConfig GenerateRoadConfig(Entity entity)
		{
			throw new NotImplementedException();
		}
	}
}
