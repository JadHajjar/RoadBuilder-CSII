using Game.City;
using Game.Prefabs;
using Game.SceneFlow;
using Game.Simulation;
using Game.Tools;
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

using UnityEngine;

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
		private CityConfigurationSystem cityConfigurationSystem;
		private RoadBuilderConfigurationsUISystem roadBuilderConfigurationsUISystem;

		private ValueBindingHelper<RoadBuilderToolMode> RoadBuilderMode;
		private ValueBindingHelper<string> RoadId;
		private ValueBindingHelper<string> RoadName;
		private ValueBindingHelper<RoadLaneUIBinder[]> RoadLanes;
		private ValueBindingHelper<OptionSectionUIEntry[]> RoadOptions;
		private ValueBindingHelper<bool> RoadListView;
		private ValueBindingHelper<bool> IsPaused;
		private ValueBindingHelper<bool> IsCustomRoadSelected;

		public RoadBuilderToolMode Mode { get => RoadBuilderMode; set => RoadBuilderMode.Value = value; }
		public string WorkingId => RoadId;
		public Entity WorkingEntity { get => workingEntity; set => workingEntity = value; }

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
			cityConfigurationSystem = World.GetOrCreateSystemManaged<CityConfigurationSystem>();
			roadBuilderConfigurationsUISystem = World.GetOrCreateSystemManaged<RoadBuilderConfigurationsUISystem>();

			toolSystem.EventToolChanged += OnToolChanged;

			RoadBuilderMode = CreateBinding("RoadBuilderToolMode", RoadBuilderToolMode.None);
			RoadId = CreateBinding("GetRoadId", string.Empty);
			RoadName = CreateBinding("GetRoadName", "SetRoadName", string.Empty, name => UpdateRoad(x => x.Name = name));
			RoadLanes = CreateBinding("GetRoadLanes", new RoadLaneUIBinder[0]);
			RoadOptions = CreateBinding("GetRoadOptions", new OptionSectionUIEntry[0]);
			IsPaused = CreateBinding("IsPaused", simulationSystem.selectedSpeed == 0f);
			RoadListView = CreateBinding("RoadListView", "SetRoadListView", true);
			IsCustomRoadSelected = CreateBinding("IsCustomRoadSelected", false);

            CreateTrigger<RoadLaneUIBinder[]>("SetRoadLanes", x => UpdateRoad(c => UpdateLaneOrder(c, x)));
			CreateTrigger<int, int, int>("RoadOptionClicked", (x, y, z) => UpdateRoad(c => RoadOptionClicked(c, x, y, z)));
			CreateTrigger<int, int, int, int>("OptionClicked", (i, x, y, z) => UpdateRoad(c => LaneOptionClicked(c, i, x, y, z)));
			CreateTrigger("ToggleTool", ToggleTool);
			CreateTrigger("CreateNewPrefab", () => CreateNewPrefab(workingEntity));
			CreateTrigger("ClearTool", ClearTool);
			CreateTrigger("EditPrefab", () => EditPrefab(workingEntity));
			CreateTrigger("CancelActionPopup", CancelActionPopup);
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

		internal void ShowActionPopup(Entity entity, PrefabBase prefab)
		{
			IsCustomRoadSelected.Value = prefab is INetworkBuilderPrefab;
            SetWorkingEntity(entity, RoadBuilderToolMode.ActionSelection);
		}

		public void CancelActionPopup()
		{
			SetWorkingEntity(Entity.Null, RoadBuilderToolMode.Picker);
		}

		public void EditPrefab(Entity entity)
		{
			SetWorkingEntity(entity, RoadBuilderToolMode.Editing);
		}

		public void CreateNewPrefab(Entity entity)
		{
			SetWorkingEntity(entity, RoadBuilderToolMode.EditingSingle);
		}

		private void SetWorkingEntity(Entity entity, RoadBuilderToolMode mode)
		{
			workingEntity = entity;

			bool isConfigEditorOpened = mode >= RoadBuilderToolMode.Editing;
            RoadBuilderMode.Value = mode;
			RoadListView.Value = !isConfigEditorOpened;
			if (!isConfigEditorOpened && entity != Entity.Null)
			{
                var config = roadBuilderSystem.GetOrGenerateConfiguration(workingEntity);

                if (config == null)
                {
                    return;
                }

                SetWorkingPrefab(config, mode);
            }			
		}

		public void SetWorkingPrefab(INetworkConfig config, RoadBuilderToolMode mode)
		{
			RoadBuilderMode.Value = mode;
			netSectionsUISystem.RefreshEntries(config);
			RoadId.Value = config.ID;
			RoadName.Value = config.Name;
			RoadOptions.Value = RoadOptionsUtil.GetRoadOptions(config);
			RoadLanes.Value = From(config);

			roadBuilderConfigurationsUISystem.UpdateConfigurationList();
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
			RoadId.Value = config.ID;
			RoadName.Value = config.Name;
			RoadOptions.Value = RoadOptionsUtil.GetRoadOptions(config);
			RoadLanes.Value = From(config);

			roadBuilderConfigurationsUISystem.UpdateConfigurationList();
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

					lane.Invert = newLanes.Count < roadLanes.Length / 2;

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

		private RoadLaneUIBinder[] From(INetworkConfig config)
		{
			var binders = new RoadLaneUIBinder[config.Lanes.Count];

			for (var i = 0; i < binders.Length; i++)
			{
				var lane = config.Lanes[i];
				var validSection = NetworkPrefabGenerationUtil.GetNetSection(roadBuilderSystem.RoadGenerationData, config, lane, out var section, out var groupPrefab);
				var isBackward = cityConfigurationSystem.leftHandTraffic ? !lane.Invert : lane.Invert;

				GetThumbnailAndColor(config, lane, section, groupPrefab, isBackward, out var thumbnail, out var color, out var texture);

				binders[i] = new RoadLaneUIBinder
				{
					Index = i,
					Invert = isBackward,
					InvertImage = lane.Invert,
					TwoWay = validSection && section.SupportsTwoWay(),
					SectionPrefabName = string.IsNullOrEmpty(lane.GroupPrefabName) ? lane.SectionPrefabName : lane.GroupPrefabName,
					IsGroup = !string.IsNullOrEmpty(lane.GroupPrefabName),
					Options = LaneOptionsUtil.GenerateOptions(roadBuilderSystem.RoadGenerationData, config, lane),
					Texture = texture ?? "asphalt",
					Color = color is null ? null : $"rgba({color?.r * 255}, {color?.g * 255}, {color?.b * 255}, {color?.a})",
					NetSection = !validSection ? new() : new()
					{
						PrefabName = section.name,
						IsGroup = !string.IsNullOrEmpty(lane.GroupPrefabName),
						DisplayName = GetAssetName((PrefabBase)groupPrefab ?? section),
						Width = validSection ? section.CalculateWidth() : 1F,
						Thumbnail = thumbnail,
					}
				};
			}

			return binders;
		}

		private static void GetThumbnailAndColor(INetworkConfig config, LaneConfig lane, NetSectionPrefab section, LaneGroupPrefab groupPrefab, bool invert, out string thumbnail, out Color? color, out string texture)
		{
			thumbnail = null;
			color = null;
			texture = null;

			if (section.TryGet<RoadBuilderLaneDecorationInfo>(out var decorationInfo) && groupPrefab?.Options.FirstOrDefault(x => x.Type is LaneOptionType.Decoration) is RoadBuilderLaneOption decorationOption)
			{
				switch (LaneOptionsUtil.GetSelectedOptionValue(config, lane, decorationOption))
				{
					case "G":
						if (decorationInfo.GrassThumbnail is not null or "")
						{
							thumbnail = decorationInfo.GrassThumbnail;
						}

						break;
					case "T":
						if (decorationInfo.TreeThumbnail is not null or "")
						{
							thumbnail = decorationInfo.TreeThumbnail;
						}

						break;
					case "GT":
						if (decorationInfo.GrassAndTreeThumbnail is not null or "")
						{
							thumbnail = decorationInfo.GrassAndTreeThumbnail;
						}

						break;
				}
			}

			if (section.TryGet<RoadBuilderLaneInfo>(out var sectionInfo))
			{
				if (thumbnail is null && !string.IsNullOrEmpty(invert ? sectionInfo.FrontThumbnail : sectionInfo.BackThumbnail))
				{
					thumbnail = invert ? sectionInfo.FrontThumbnail : sectionInfo.BackThumbnail;
				}

				if (sectionInfo.LaneColor != default)
				{
					color = sectionInfo.LaneColor;
				}
			}

			if (thumbnail is null && ImageSystem.GetIcon(section) is string sectionIcon)
			{
				thumbnail = sectionIcon;
			}

			if (groupPrefab?.TryGet<RoadBuilderLaneInfo>(out var groupInfo) ?? false)
			{
				if (thumbnail is null && !string.IsNullOrEmpty(invert ? groupInfo.FrontThumbnail : groupInfo.BackThumbnail))
				{
					thumbnail = invert ? groupInfo.FrontThumbnail : groupInfo.BackThumbnail;
				}

				if (groupInfo.LaneColor != default && color is null)
				{
					color = groupInfo.LaneColor;
				}
			}

			if (thumbnail is null && groupPrefab is not null)
			{
				thumbnail = ImageSystem.GetIcon(groupPrefab);
			}

			if (color is null)
			{
				if (config.Category.HasFlag(RoadCategory.Gravel))
				{
					texture = "gravel";
					color = new(143 / 255f, 131 / 255f, 97 / 255f);
				}
				else if (config.Category.HasFlag(RoadCategory.Tiled))
				{
					texture = "tiled";
					color = new(76 / 255f, 78 / 255f, 83 / 255f);
				}
			}

			if (section.IsTrainOrSubway())
			{
				texture = "train";
			}
			else if (section.IsBus())
			{
				texture = "bus";
			}
		}

		public string GetLaneName(INetworkConfig config, LaneConfig lane)
		{
			var validSection = NetworkPrefabGenerationUtil.GetNetSection(roadBuilderSystem.RoadGenerationData, config, lane, out var section, out var groupPrefab);

			if (validSection)
			{
				return GetAssetName((PrefabBase)groupPrefab ?? section);
			}

			return string.Empty;
		}

		private string GetAssetName(PrefabBase prefab)
		{
			prefabUISystem.GetTitleAndDescription(prefab, out var titleId, out var _);

			if (GameManager.instance.localizationManager.activeDictionary.TryGetValue(titleId, out var name))
			{
				return name;
			}

			if (prefab is LaneGroupPrefab laneGroup && !string.IsNullOrEmpty(laneGroup.DisplayName))
			{
				return laneGroup.DisplayName;
			}

			return prefab.name.Replace('_', ' ').FormatWords();
		}
	}
}
