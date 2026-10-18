using Game.Prefabs;
using Game.SceneFlow;
using Game.UI;

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
#nullable disable
		private RoadBuilderUISystem roadBuilderUISystem;
		private RoadBuilderSystem roadBuilderSystem;
		private RoadBuilderNetSectionsSystem netSectionsSystem;
		private ValueBindingHelper<NetSectionGroup[]> _NetSections;
#nullable enable

		private List<NetSectionItem>? availableSections;
		private INetworkConfig? activeConfig;
		private string query = string.Empty;

		protected override void OnCreate()
		{
			base.OnCreate();

			roadBuilderUISystem = World.GetOrCreateSystemManaged<RoadBuilderUISystem>();
			roadBuilderSystem = World.GetOrCreateSystemManaged<RoadBuilderSystem>();
			netSectionsSystem = World.GetOrCreateSystemManaged<RoadBuilderNetSectionsSystem>();

			_NetSections = CreateBinding("NetSections", new NetSectionGroup[0]);

			CreateTrigger<string>("Lanes.SetSearchQuery", SetSearchQuery);

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

		public void RefreshEntries(INetworkConfig? config)
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
				// Variants are listed through their group, a variant whose group isn't loaded is listed on its own instead of vanishing
				if (IsLinkedVariant(prefab) || prefab.Has<RoadBuilderVanillaLaneGroup>() || !prefab.Has<RoadBuilderLaneInfo>())
				{
					continue;
				}

				bool restricted;

				if (restricted = activeConfig != null && !prefab.MatchCategories(activeConfig))
				{
					if (!Mod.Settings!.UnrestrictedLanes)
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
					IsCustom = !netSectionsSystem.IsBuiltIn(prefab),
					SectionType = GetSectionType(prefab),
				});
			}

			foreach (var prefab in netSectionsSystem.LaneGroups.Values)
			{
				// A group without variants can't produce a lane
				if (prefab.LinkedSections.Count == 0)
				{
					continue;
				}

				bool restricted;

				// The group's own road types and at least one variant's road types both have to allow the road
				if (restricted = activeConfig != null && (!prefab.MatchCategories(activeConfig) || !prefab.LinkedSections.Any(x => x.MatchCategories(activeConfig))))
				{
					if (!Mod.Settings!.UnrestrictedLanes)
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
					IsEdge = prefab.Has<RoadBuilderEdgeLaneInfo>() || prefab.LinkedSections.All(x => x.Has<RoadBuilderEdgeLaneInfo>()),
					IsRestricted = restricted,
					IsCustom = !netSectionsSystem.IsBuiltIn(prefab),
					SectionType = GetSectionType(prefab),
				});
			}

			sections.Sort((x, y) => string.Compare(x.DisplayName, y.DisplayName));

			return sections;
		}

		//private int GetUseCount(NetSectionItem section)
		//{
		//	var name = section.PrefabName;
		//	var count = 0;

		//	foreach (var item in roadBuilderSystem.Configurations.Values)
		//	{
		//		foreach (var lane in item.Config.Lanes)
		//		{
		//			if (lane.SectionPrefabName == name || lane.GroupPrefabName == name)
		//			{
		//				count++;
		//			}
		//		}
		//	}

		//	return count;
		//}

		private bool IsLinkedVariant(NetSectionPrefab prefab)
		{
			return prefab.TryGetExactly<RoadBuilderLaneGroup>(out var laneGroup)
				&& laneGroup.GroupPrefab is not null
				&& netSectionsSystem.LaneGroups.ContainsKey(laneGroup.GroupPrefab.name);
		}

		private bool Filter(string? name)
		{
			return string.IsNullOrWhiteSpace(query) || query.SearchCheck(name);
		}

		private bool IsInvalidLane(NetSectionPrefab prefab)
		{
			var carLanes = prefab.FindLanes<CarLane>();

			if (carLanes.Any(x => (x.m_RoadType & (Game.Net.RoadTypes.Watercraft | Game.Net.RoadTypes.Helicopter | Game.Net.RoadTypes.Airplane)) != 0))
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

			if (prefab.LinkedSections.Count == 0)
			{
				return LaneSectionType.Misc;
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

			if (prefab.IsMedian() || (prefab.TryGet<RoadBuilderLaneInfo>(out var laneInfo) && laneInfo.Median) || prefab.name.IndexOf("Median", StringComparison.InvariantCultureIgnoreCase) >= 0)
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
			roadBuilderUISystem.GetTitleAndDescription(prefab, out var titleId, out var _);

			if (GameManager.instance.localizationManager.activeDictionary.TryGetValue(titleId, out var name))
			{
				return name;
			}

			return prefab.name.Replace('_', ' ').FormatWords();
		}
	}
}
