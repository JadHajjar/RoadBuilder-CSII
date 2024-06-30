using Game;
using Game.Prefabs;
using Game.Tools;

using RoadBuilder.Domain;
using RoadBuilder.Domain.Components;
using RoadBuilder.Domain.Configuration;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Unity.Collections;
using Unity.Entities;

namespace RoadBuilder.Systems
{
	public partial class RoadBuilderSerializeSystem : GameSystemBase
	{
		private static PrefabSystem prefabSystem;
		private EntityQuery roadBuilderNetsQuery;

		protected override void OnCreate()
		{
			base.OnCreate();

			prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
			roadBuilderNetsQuery = SystemAPI.QueryBuilder()
				.WithAll<RoadBuilderNetwork, PrefabRef>()
				.WithNone<Temp>()
				.Build();
		}

		public static void RegisterRoadID(string id)
		{
			if (!prefabSystem.TryGetPrefab(new PrefabID(typeof(RoadBuilderPrefab).Name, id), out _))
			{
				prefabSystem.AddPrefab(new RoadBuilderPrefab(new RoadConfig { ID = id }));
			}
		}

		protected override void OnUpdate()
		{
			if (!SystemAPI.TryGetSingletonBuffer<RoadConfigBuffer>(out var roadConfigBuffer))
			{
				var singleton = EntityManager.CreateSingletonBuffer<RoadConfigBuffer>();

				roadConfigBuffer = EntityManager.GetBuffer<RoadConfigBuffer>(singleton);
			}

			var prefabRefs = roadBuilderNetsQuery.ToComponentDataArray<PrefabRef>(Allocator.Temp);

			roadConfigBuffer.Clear();

			var addedIds = new List<string>();

			for (var i = 0; i < prefabRefs.Length; i++)
			{
				if (prefabSystem.TryGetPrefab<RoadBuilderPrefab>(prefabRefs[i], out var prefab))
				{
					if (!addedIds.Contains(prefab.Config.ID))
					{
						addedIds.Add(prefab.Config.ID);

						roadConfigBuffer.Add(new RoadConfigBuffer { ConfigurationID = prefab.Config.ID });
					}
				}
			}
		}
	}
}
