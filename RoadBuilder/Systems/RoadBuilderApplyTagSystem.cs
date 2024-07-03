using Game;
using Game.Common;
using Game.Net;
using Game.Prefabs;
using Game.Tools;

using RoadBuilder.Domain.Components;
using RoadBuilder.Domain.Prefabs;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Unity.Collections;
using Unity.Entities;

namespace RoadBuilder.Systems
{
	public partial class RoadBuilderApplyTagSystem : GameSystemBase
	{
		private PrefabSystem prefabSystem;
		private EntityQuery segmentQuery;

		protected override void OnCreate()
		{
			base.OnCreate();

			prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
			segmentQuery = SystemAPI.QueryBuilder()
				.WithAll<Edge, PrefabRef, Created>()
				.WithNone<RoadBuilderNetwork, Temp>()
				.Build();

			RequireForUpdate(segmentQuery);
		}

		protected override void OnUpdate()
		{
			var entities = segmentQuery.ToEntityArray(Allocator.Temp);

			Mod.Log.Info(World.GetOrCreateSystemManaged<UpdateSystem>().currentPhase);
			Mod.Log.Info(entities.Length);

			for (var i = 0; i < entities.Length; i++)
			{
				var entity = entities[i];

				if (prefabSystem.GetPrefab<PrefabBase>(EntityManager.GetComponentData<PrefabRef>(entity)) is INetworkBuilderPrefab)
				{
					EntityManager.AddComponent<RoadBuilderNetwork>(entity);
				}
			}
		}
	}
}
