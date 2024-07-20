using Game.Prefabs;
using Game.SceneFlow;
using Game.Simulation;
using Game.Tools;
using Game.UI;
using Game.UI.InGame;

using RoadBuilder.Domain;
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
	public partial class RoadBuilderUISystem : ExtendedUISystemBase
	{
		private Entity workingEntity;

		private PrefabSystem prefabSystem;
		private PrefabUISystem prefabUISystem;
		private ToolSystem toolSystem;
		private RoadBuilderSystem roadBuilderSystem;
		private RoadBuilderToolSystem roadBuilderToolSystem;
		private DefaultToolSystem defaultToolSystem;
		private SimulationSystem simulationSystem;
		private NetSectionsUISystem netSectionsUISystem;
		private NetSectionsSystem netSectionsSystem;
		private ValueBindingHelper<RoadBuilderToolMode> RoadBuilderMode;
		private ValueBindingHelper<string> RoadName;
		private ValueBindingHelper<RoadLaneUIBinder[]> RoadLanes;
		private ValueBindingHelper<OptionSectionUIEntry[]> RoadOptions;
		private ValueBindingHelper<bool> IsPaused;

		public RoadBuilderToolMode Mode { get => RoadBuilderMode; set => RoadBuilderMode.Value = value; }
		public Entity WorkingEntity => workingEntity;

		protected override void OnCreate()
		{
			base.OnCreate();

			prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
			prefabUISystem = World.GetOrCreateSystemManaged<PrefabUISystem>();
			toolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
			roadBuilderSystem = World.GetOrCreateSystemManaged<RoadBuilderSystem>();
			roadBuilderToolSystem = World.GetOrCreateSystemManaged<RoadBuilderToolSystem>();
			defaultToolSystem = World.GetOrCreateSystemManaged<DefaultToolSystem>();
			simulationSystem = World.GetOrCreateSystemManaged<SimulationSystem>();
			netSectionsUISystem = World.GetOrCreateSystemManaged<NetSectionsUISystem>();
			netSectionsSystem = World.GetOrCreateSystemManaged<NetSectionsSystem>();

			toolSystem.EventToolChanged += OnToolChanged;

			RoadBuilderMode = CreateBinding("RoadBuilderToolMode", RoadBuilderToolMode.None);
			RoadName = CreateBinding("GetRoadName", "SetRoadName", string.Empty, name =>
			{
				UpdateRoad(x => x.Name = name);
				GameManager.instance.localizationManager.ReloadActiveLocale();
			});
			RoadLanes = CreateBinding("GetRoadLanes", new RoadLaneUIBinder[0]);
			RoadOptions = CreateBinding("GetRoadOptions", new OptionSectionUIEntry[0]);
			IsPaused = CreateBinding("IsPaused", simulationSystem.selectedSpeed == 0f);

			CreateTrigger<RoadLaneUIBinder[]>("SetRoadLanes", x => UpdateRoad(c => UpdateLaneOrder(c, x)));
			CreateTrigger<int, int, int>("RoadOptionClicked", (x, y, z) => UpdateRoad(c => RoadOptionClicked(c, x, y, z)));
			CreateTrigger<int, int, int, int>("OptionClicked", (i, x, y, z) => UpdateRoad(c => LaneOptionClicked(c, i, x, y, z)));
			CreateTrigger("ToggleTool", ToggleTool);
			CreateTrigger("CreateNewPrefab", () => CreateNewPrefab(workingEntity));
			CreateTrigger("ClearTool", ClearTool);
		}

		protected override void OnUpdate()
		{
			IsPaused.Value = simulationSystem.selectedSpeed == 0f;

			base.OnUpdate();
		}

		private void OnToolChanged(ToolBaseSystem system)
		{
			if (system is not RoadBuilderToolSystem)
			{
				RoadBuilderMode.Value = RoadBuilderToolMode.None;
			}
		}

		private void ToggleTool()
		{
			if (toolSystem.activeTool is RoadBuilderToolSystem)
			{
				ClearTool();
			}
			else
			{
				RoadBuilderMode.Value = RoadBuilderToolMode.Picker;

				toolSystem.selected = Entity.Null;
				toolSystem.activeTool = roadBuilderToolSystem;
			}
		}

		public void ClearTool()
		{
			RoadBuilderMode.Value = RoadBuilderToolMode.None;

			toolSystem.selected = Entity.Null;
			toolSystem.activeTool = defaultToolSystem;
		}

		public void EditPrefab(Entity entity)
		{
			RoadBuilderMode.Value = RoadBuilderToolMode.Editing;

			SetWorkingEntity(entity);
		}

		public void CreateNewPrefab(Entity entity)
		{
			RoadBuilderMode.Value = RoadBuilderToolMode.EditingSingle;

			SetWorkingEntity(entity);
		}

		private void SetWorkingEntity(Entity entity)
		{
			workingEntity = entity;

			var config = roadBuilderSystem.GetOrGenerateConfiguration(workingEntity);

			if (config == null)
			{
				return;
			}

			netSectionsUISystem.RefreshEntries(config);
			RoadName.Value = config.Name;
			RoadOptions.Value = RoadOptionsUtil.GetRoadOptions(config);
			RoadLanes.Value = From(config, roadBuilderSystem.RoadGenerationData);
		}

		private void UpdateRoad(Action<INetworkConfig> action)
		{
			var createNew = RoadBuilderMode.Value is RoadBuilderToolMode.EditingSingle;
			var config = createNew
				? roadBuilderSystem.GenerateConfiguration(workingEntity)
				: roadBuilderSystem.GetOrGenerateConfiguration(workingEntity);

			if (config == null)
			{
				return;
			}

			action(config);

			roadBuilderSystem.UpdateRoad(config, workingEntity, createNew);

			RoadBuilderMode.Value = RoadBuilderToolMode.Editing;

			netSectionsUISystem.RefreshEntries(config);
			RoadName.Value = config.Name;
			RoadOptions.Value = RoadOptionsUtil.GetRoadOptions(config);
			RoadLanes.Value = From(config, roadBuilderSystem.RoadGenerationData);
		}

		private void UpdateLaneOrder(INetworkConfig config, RoadLaneUIBinder[] roadLanes)
		{
			var newLanes = new List<LaneConfig>();

			foreach (var item in roadLanes)
			{
				var existingLane = config.Lanes.ElementAtOrDefault(item.Index);

				if (existingLane != null)
				{
					newLanes.Add(existingLane);
				}
				else
				{
					var lane = item.ToLaneConfig();

					if (lane.GroupPrefabName is not null && netSectionsSystem.LaneGroups.TryGetValue(lane.GroupPrefabName, out var group))
					{
						LaneOptionsUtil.FixGroupOptions(config, lane, group);
					}

					newLanes.Add(lane);
				}
			}

			config.Lanes = newLanes;
		}

		private void RoadOptionClicked(INetworkConfig config, int option, int id, int value)
		{
			RoadOptionsUtil.OptionClicked(config, option, id, value);

			config.Lanes.RemoveAll(x =>
			{
				NetworkPrefabGenerationUtil.GetNetSection(roadBuilderSystem.RoadGenerationData, config, x, out var section, out var group);

				return !(section?.MatchCategories(config) ?? true) || !(group?.MatchCategories(config) ?? true);
			});
		}

		private void LaneOptionClicked(INetworkConfig config, int index, int option, int id, int value)
		{
			var existingLane = config.Lanes.ElementAtOrDefault(index);

			if (existingLane != null)
			{
				LaneOptionsUtil.OptionClicked(roadBuilderSystem.RoadGenerationData, config, existingLane, option, id, value);
			}
		}

		private RoadLaneUIBinder[] From(INetworkConfig config, RoadGenerationData roadGenerationData)
		{
			var binders = new RoadLaneUIBinder[config.Lanes.Count];

			for (var i = 0; i < binders.Length; i++)
			{
				var lane = config.Lanes[i];
				var validSection = NetworkPrefabGenerationUtil.GetNetSection(roadGenerationData, config, lane, out var section, out var groupPrefab);

				binders[i] = new RoadLaneUIBinder
				{
					Index = i,
					Invert = lane.Invert,
					TwoWay = validSection && section.SupportsTwoWay(),
					SectionPrefabName = string.IsNullOrEmpty(lane.GroupPrefabName) ? lane.SectionPrefabName : lane.GroupPrefabName,
					IsGroup = !string.IsNullOrEmpty(lane.GroupPrefabName),
					Options = LaneOptionsUtil.GenerateOptions(roadGenerationData, config, lane),
					NetSection = !validSection ? new() : new()
					{
						PrefabName = section.name,
						IsGroup = !string.IsNullOrEmpty(lane.GroupPrefabName),
						DisplayName = GetAssetName((PrefabBase)groupPrefab ?? section),
						Thumbnail = GetThumbnail(config, lane, section, groupPrefab, lane.Invert),
						Width = validSection ? section.CalculateWidth() : 1F,
					}
				};
			}

			return binders;
		}

		private static string GetThumbnail(INetworkConfig config, LaneConfig lane, NetSectionPrefab section, LaneGroupPrefab groupPrefab, bool invert)
		{
			if (section.TryGet<RoadBuilderLaneDecorationInfo>(out var decorationInfo) && groupPrefab?.Options.FirstOrDefault(x => x.Type is LaneOptionType.Decoration) is RoadBuilderLaneOption decorationOption)
			{
				switch (LaneOptionsUtil.GetSelectedOptionValue(config, lane, decorationOption))
				{
					case "G":
						if (decorationInfo.GrassThumbnail is not null or "")
						{
							return decorationInfo.GrassThumbnail;
						}

						break;
					case "T":
						if (decorationInfo.TreeThumbnail is not null or "")
						{
							return decorationInfo.TreeThumbnail;
						}

						break;
					case "GT":
						if (decorationInfo.GrassAndTreeThumbnail is not null or "")
						{
							return decorationInfo.GrassAndTreeThumbnail;
						}

						break;
				}
			}

			if (section.TryGet<RoadBuilderLaneInfo>(out var sectionInfo))
			{
				if (!string.IsNullOrEmpty(invert ? sectionInfo.FrontThumbnail : sectionInfo.BackThumbnail))
				{
					return invert ? sectionInfo.FrontThumbnail : sectionInfo.BackThumbnail;
				}
			}

			if (ImageSystem.GetIcon(section) is string sectionIcon)
			{
				return sectionIcon;
			}

			if (groupPrefab?.TryGet<RoadBuilderLaneInfo>(out var groupInfo) ?? false)
			{
				if (!string.IsNullOrEmpty(invert ? groupInfo.FrontThumbnail : groupInfo.BackThumbnail))
				{
					return invert ? groupInfo.FrontThumbnail : groupInfo.BackThumbnail;
				}
			}

			if (groupPrefab is null)
			{
				return null;
			}

			return ImageSystem.GetIcon(groupPrefab);
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
