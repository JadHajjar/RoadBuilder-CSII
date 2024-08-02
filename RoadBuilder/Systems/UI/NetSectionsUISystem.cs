using Game.Prefabs;
using Game.SceneFlow;
using Game.UI;
using Game.UI.InGame;

using RoadBuilder.Domain.Components.Prefabs;
using RoadBuilder.Domain.Configurations;
using RoadBuilder.Domain.UI;
using RoadBuilder.Utilities;

using System.Collections.Generic;
using System.Linq;

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

		public void RefreshEntries(INetworkConfig config)
		{
			activeConfig = config;

			_NetSections.Value = GetSections();
		}

		private NetSectionItem[] GetSections()
		{
			var sections = new List<NetSectionItem>();

			foreach (var prefab in netSectionsSystem.NetSections.Values)
			{
				if (prefab.Has<RoadBuilderLaneGroup>() || !prefab.Has<RoadBuilderLaneInfo>())
				{
					continue;
				}

				if (!Mod.Settings.AdvancedUserMode && (!prefab.Has<RoadBuilderLaneInfo>() || !prefab.MatchCategories(activeConfig)))
				{
					continue;
				}

				if (IsInvalidLane(prefab))
				{
					continue;
				}

				sections.Add(new NetSectionItem
				{
					PrefabName = prefab.name,
					DisplayName = GetAssetName(prefab),
					Thumbnail = ImageSystem.GetIcon(prefab),
					Width = prefab.CalculateWidth()
				});
			}

			foreach (var prefab in netSectionsSystem.LaneGroups.Values)
			{
				if (!prefab.MatchCategories(activeConfig))
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

			return sections.OrderBy(x => x.DisplayName).ToArray();
		}

		private bool IsInvalidLane(NetSectionPrefab prefab)
		{
			var carLanes = prefab.FindLanes<CarLane>();

			if (carLanes.Any(x => x.m_RoadType > Game.Net.RoadTypes.Car))
			{
				return true;
			}

			if (prefab.FindLanes<UtilityLane>().Any(x => x.m_UtilityType is not Game.Net.UtilityTypes.Fence))
			{
				return true;
			}

			return false;
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
