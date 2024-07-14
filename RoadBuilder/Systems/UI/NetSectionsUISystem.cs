using Game.Prefabs;
using Game.SceneFlow;
using Game.UI;
using Game.UI.InGame;

using RoadBuilder.Domain.Components;
using RoadBuilder.Domain.Configurations;
using RoadBuilder.Domain.UI;
using RoadBuilder.Utilities;

using System.Collections.Generic;

using Unity.Entities;

namespace RoadBuilder.Systems.UI
{
	public partial class NetSectionsUISystem : ExtendedUISystemBase
	{
		private PrefabSystem prefabSystem;
		private PrefabUISystem prefabUISystem;
		private NetSectionsSystem netSectionsSystem;
		private ValueBindingHelper<NetSectionItem[]> _NetSections;
		private INetworkConfig activeConfig;

		protected override void OnCreate()
		{
			base.OnCreate();

			prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
			prefabUISystem = World.GetOrCreateSystemManaged<PrefabUISystem>();
			netSectionsSystem = World.GetOrCreateSystemManaged<NetSectionsSystem>();

			_NetSections = CreateBinding("NetSections", GetSections());

			netSectionsSystem.SectionsAdded += () => _NetSections.Value = GetSections();
		}

		internal void RefreshEntries(INetworkConfig config)
		{
			activeConfig = config;

			_NetSections.Value = GetSections();
		}

		private NetSectionItem[] GetSections()
		{
			var sections = new List<NetSectionItem>();

			foreach (var prefab in netSectionsSystem.NetSections.Values)
			{
				if (prefab.Has<RoadBuilderLaneGroupItem>())
				{
					continue;
				}

				if (!MatchPrefab(prefab))
				{
					continue;
				}

				sections.Add(new NetSectionItem
				{
					PrefabName = prefab.name,
					DisplayName = prefab.name,//GetAssetName(prefab),
					Thumbnail = ImageSystem.GetIcon(prefab),
					Width = prefab.CalculateWidth()
				});
			}

			foreach (var prefab in netSectionsSystem.LaneGroups.Values)
			{
				if (!MatchPrefab(prefab))
				{
					continue;
				}

				sections.Add(new NetSectionItem
				{
					IsGroup = true,
					PrefabName = prefab.name,
					DisplayName = GetAssetName(prefab),
					Thumbnail = ImageSystem.GetIcon(prefab)
				});
			}

			return sections.ToArray();
		}

		private bool MatchPrefab(PrefabBase prefab)
		{
			if (activeConfig is null || !prefab.TryGet<RoadBuilderLaneInfoItem>(out var info))
			{
				return true;
			}

			var matchesRequired = (activeConfig.Category & info.RequiredCategories) == info.RequiredCategories;
			var matchesExcluded = (activeConfig.Category & info.ExcludedCategories) != 0;

			return matchesRequired && !matchesExcluded;
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
