using Colossal.Serialization.Entities;

using Game;
using Game.Prefabs;
using Game.SceneFlow;
using Game.UI;
using Game.UI.InGame;

using RoadBuilder.Domain.Components;
using RoadBuilder.Domain.UI;
using RoadBuilder.Utilities;

using System.Collections.Generic;

using Unity.Collections;
using Unity.Entities;

namespace RoadBuilder.Systems.UI
{
	public partial class NetSectionsUISystem : ExtendedUISystemBase
	{
		private PrefabSystem prefabSystem;
		private PrefabUISystem prefabUISystem;
		private ValueBindingHelper<NetSectionItem[]> _NetSections;

		protected override void OnCreate()
		{
			base.OnCreate();

			prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
			prefabUISystem = World.GetOrCreateSystemManaged<PrefabUISystem>();

			_NetSections = CreateBinding("NetSections", new NetSectionItem[0]);
		}

		protected override void OnGameLoadingComplete(Purpose purpose, GameMode mode)
		{
			base.OnGameLoadingComplete(purpose, mode);

			if (mode != GameMode.Game)
			{
				//return;
			}

			var entityQuery = SystemAPI.QueryBuilder()
				.WithAll<NetSectionData>()
				.WithOptions(EntityQueryOptions.IncludePrefab)
				.Build();

			var entities = entityQuery.ToEntityArray(Allocator.Temp);
			var sections = new List<NetSectionItem>();

			var medianGroup = new Domain.Prefabs.LaneGroupPrefab
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
							new(){Value = "0"},
							new(){Value = "1"},
							new(){Value = "2"},
							new(){Value = "5"}
						}
					}
				}
			};

			prefabSystem.AddPrefab(medianGroup);

			for (var i = 0; i < entities.Length; i++)
			{
				if (!prefabSystem.TryGetPrefab<NetSectionPrefab>(entities[i], out var prefab))
				{
					continue;
				}

				if (prefab.name.StartsWith("Road Median "))
				{
					Mod.Log.Info(prefab.name);

					var laneInfo = prefab.AddComponent<RoadBuilderLaneGroupItem>();
					laneInfo.GroupPrefab = medianGroup;
					laneInfo.Combination = new LaneOptionCombination[]
					{
						new LaneOptionCombination
						{
							OptionName = "Median Width",
							Value = prefab.name.Remove(0, "Road Median ".Length)
						}
					};
				}

				sections.Add(new NetSectionItem
				{
					PrefabName = prefab.name,
					DisplayName = GetAssetName(prefab),
					Thumbnail = ImageSystem.GetThumbnail(prefab)
				});
			}

			_NetSections.Value = sections.ToArray();
		}

		private string GetAssetName(PrefabBase prefab)
		{
			prefabUISystem.GetTitleAndDescription(prefab, out var titleId, out var _);

			if (GameManager.instance.localizationManager.activeDictionary.TryGetValue(titleId, out var name))
			{
				return name;
			}

			return prefab.name.Replace('_', ' ').FormatWords();
		}
	}
}
