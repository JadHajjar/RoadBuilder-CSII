using Game.Prefabs;
using Game.SceneFlow;
using Game.UI;
using Game.UI.InGame;

using RoadBuilder.Domain.Components.Prefabs;
using RoadBuilder.Domain.Configurations;
using RoadBuilder.Domain.Enums;
using RoadBuilder.Domain.Prefabs;
using RoadBuilder.Domain.UI;
using RoadBuilder.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;

using Unity.Entities;

namespace RoadBuilder.Systems.UI
{
	public partial class RoadBuilderNetSectionsUISystem : ExtendedUISystemBase
	{
		private PrefabSystem prefabSystem;
		private PrefabUISystem prefabUISystem;
		private RoadBuilderSystem roadBuilderSystem;
		private RoadBuilderNetSectionsSystem netSectionsSystem;
		private ValueBindingHelper<NetSectionGroup[]> _NetSections;
		private List<NetSectionItem> availableSections;
		private INetworkConfig activeConfig;
		private string query = string.Empty;

		protected override void OnCreate()
		{
			base.OnCreate();

			prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
			prefabUISystem = World.GetOrCreateSystemManaged<PrefabUISystem>();
			roadBuilderSystem = World.GetOrCreateSystemManaged<RoadBuilderSystem>();
			netSectionsSystem = World.GetOrCreateSystemManaged<RoadBuilderNetSectionsSystem>();

			_NetSections = CreateBinding("NetSections", new NetSectionGroup[0]);

			CreateTrigger<string>("SetSearchQuery", SetSearchQuery);

			netSectionsSystem.SectionsAdded += () => RefreshEntries(activeConfig);
		}

		private void SetSearchQuery(string obj)
		{
			query = obj;

			_NetSections.Value = availableSections
				.Where(x => Filter(x.DisplayName))
				.GroupBy(x => x.SectionType)
				.Select(x => new NetSectionGroup
				{
					Type = x.Key,
					Sections = x.ToArray()
				})
				.ToArray();
		}

		public void RefreshEntries(INetworkConfig config)
		{
			activeConfig = config;

			availableSections = GetSections();

			SetSearchQuery(query);
		}

		private List<NetSectionItem> GetSections()
		{
			var sections = new List<NetSectionItem>();

			foreach (var prefab in netSectionsSystem.NetSections.Values)
			{
				if (prefab.Has<RoadBuilderLaneGroup>() || !prefab.Has<RoadBuilderLaneInfo>())
				{
					continue;
				}

				bool restricted;

				if (restricted = !prefab.MatchCategories(activeConfig))
				{
					if (!Mod.Settings.UnrestrictedLanes)
					{
						continue;
					}
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
					IsEdge = prefab.Has<RoadBuilderEdgeLaneInfo>(),
					IsRestricted = restricted,
					IsCustom = !prefab.GetComponent<RoadBuilderLaneInfo>().RoadBuilder,
					SectionType = GetSectionType(prefab),
				});
			}

			foreach (var prefab in netSectionsSystem.LaneGroups.Values)
			{
				bool restricted;

				if (restricted = !prefab.MatchCategories(activeConfig))
				{
					if (!Mod.Settings.UnrestrictedLanes)
					{
						continue;
					}
				}

				sections.Add(new NetSectionItem
				{
					IsGroup = true,
					PrefabName = prefab.name,
					DisplayName = GetAssetName(prefab),
					Thumbnail = ImageSystem.GetIcon(prefab),
					IsEdge = prefab.Has<RoadBuilderEdgeLaneInfo>(),
					IsRestricted = false,
					IsCustom = !prefab.RoadBuilder,
					SectionType = GetSectionType(prefab),
				});
			}

			sections.Sort((x, y) => GetUseCount(y) - GetUseCount(x));

			return sections;
		}

		private int GetUseCount(NetSectionItem section)
		{
			var name = section.PrefabName;
			var count = 0;

			foreach (var item in roadBuilderSystem.Configurations.Values)
			{
				foreach (var lane in item.Config.Lanes)
				{
					if (lane.SectionPrefabName == name || lane.GroupPrefabName == name)
					{
						count++;
					}
				}
			}

			return count;
		}

		private bool Filter(string name)
		{
			return string.IsNullOrWhiteSpace(query) || query.SearchCheck(name);
		}

		private bool IsInvalidLane(NetSectionPrefab prefab)
		{
			var carLanes = prefab.FindLanes<CarLane>();

			if (carLanes.Any(x => x.m_RoadType > Game.Net.RoadTypes.Car))
			{
				return true;
			}

			return false;
		}

		private LaneSectionType GetSectionType(LaneGroupPrefab prefab)
		{
			if (prefab.Has<RoadBuilderEdgeLaneInfo>())
			{
				return LaneSectionType.Edges;
			}

			return prefab.LinkedSections.Min(GetSectionType);
		}

		private LaneSectionType GetSectionType(NetSectionPrefab prefab)
		{
			if (prefab.Has<RoadBuilderEdgeLaneInfo>())
			{
				return LaneSectionType.Edges;
			}

			if (prefab.FindLanes<CarLane>().Any())
			{
				return LaneSectionType.Vehicles;
			}

			if (prefab.FindLanes<TrackLane>().Any())
			{
				return LaneSectionType.Tracks;
			}

			if (prefab.IsMedian() || prefab.name.IndexOf("Median", StringComparison.InvariantCultureIgnoreCase) >= 0)
			{
				return LaneSectionType.Medians;
			}

			if (prefab.FindLanes<ParkingLane>().Any())
			{
				return LaneSectionType.Vehicles;
			}

			return LaneSectionType.Misc;
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
