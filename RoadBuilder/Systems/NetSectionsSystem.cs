using Game;
using Game.Common;
using Game.Prefabs;

using RoadBuilder.Domain.Components;
using RoadBuilder.Domain.Prefabs;

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

			if (!customGroupsAdded)
			{
				AddCustomGroups();

				customGroupsAdded = true;
			}

			SectionsAdded?.Invoke();
		}

		private void AddCustomGroups()
		{
			var medianGroup = new LaneGroupPrefab
			{
				name = "RoadBuilder.MedianGroup",
				Options = new RoadBuilderLaneOptionInfo[]
				{
					new()
					{
						DefaultValue = "2",
						IsValue = true,
						Name = "Median Width",
						Options = new RoadBuilderLaneOptionItemInfo[]
						{
							new() { Value = "0" },
							new() { Value = "1" },
							new() { Value = "2" },
							new() { Value = "5" }
						}
					}
				}
			};

			var obj = medianGroup.AddComponent<UIObject>();
			obj.m_Icon = "coui://uil/Colored/Intersection.svg";

			prefabSystem.AddPrefab(medianGroup);

			LaneGroups[medianGroup.name] = medianGroup;

			foreach (var prefab in NetSections.Values)
			{
				if (prefab.name.StartsWith("Road Median "))
				{
					var laneInfo = prefab.AddComponent<RoadBuilderLaneGroupItem>();
					laneInfo.GroupPrefab = medianGroup;
					laneInfo.Combination = new LaneOptionCombination[]
					{
						new()
						{
							OptionName = "Median Width",
							Value = prefab.name.Remove(0, "Road Median ".Length)
						}
					};

					medianGroup.LinkedSections.Add(prefab);
				}
			}
		}
	}
}
