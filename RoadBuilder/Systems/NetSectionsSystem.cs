using Game;
using Game.Common;
using Game.Prefabs;
using Game.SceneFlow;

using RoadBuilder.Domain.Prefabs;
using RoadBuilder.LaneGroups;

using System;
using System.Collections.Generic;

using Unity.Collections;
using Unity.Entities;

namespace RoadBuilder.Systems
{
	public partial class NetSectionsSystem : GameSystemBase
	{
		private PrefabSystem prefabSystem;
		private EntityQuery prefabQuery;
		private bool customGroupsAdded;

		public event Action SectionsAdded;

		public Dictionary<string, NetSectionPrefab> NetSections { get; } = new();
		public Dictionary<string, LaneGroupPrefab> LaneGroups { get; } = new();

		protected override void OnCreate()
		{
			base.OnCreate();

			prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
			prefabQuery = SystemAPI.QueryBuilder().WithAll<Created, PrefabData, NetSectionData>().Build();

			RequireForUpdate(prefabQuery);
		}

		protected override void OnUpdate()
		{
			var entities = prefabQuery.ToEntityArray(Allocator.Temp);

			for (var i = 0; i < entities.Length; i++)
			{
				if (prefabSystem.TryGetPrefab<NetSectionPrefab>(entities[i], out var prefab))
				{
					NetSections[prefab.name] = prefab;
				}
			}

			if (!customGroupsAdded && NetSections.Count > 0)
			{
				AddCustomGroups();

				GameManager.instance.localizationManager.ReloadActiveLocale();

				customGroupsAdded = true;
			}

			SectionsAdded?.Invoke();
		}

		private void AddCustomGroups()
		{
			foreach (var type in typeof(NetSectionsSystem).Assembly.GetTypes())
			{
				if (typeof(BaseLaneGroupPrefab).IsAssignableFrom(type) && !type.IsAbstract)
				{
					var prefab = (BaseLaneGroupPrefab)Activator.CreateInstance(type, NetSections);

					prefab.name = type.FullName;

					prefabSystem.AddPrefab(prefab);

					LaneGroups[prefab.name] = prefab;
				}
			}
		}
	}
}
