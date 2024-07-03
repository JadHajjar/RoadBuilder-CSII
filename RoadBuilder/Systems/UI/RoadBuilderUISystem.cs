using Game.Prefabs;
using Game.SceneFlow;
using Game.Simulation;
using Game.Tools;
using Game.UI;
using Game.UI.InGame;

using RoadBuilder.Domain;
using RoadBuilder.Domain.Configuration;
using RoadBuilder.Domain.Enums;
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

        private ValueBindingHelper<RoadBuilderToolMode> RoadBuilderMode;
		private ValueBindingHelper<RoadPropertiesUIBinder> RoadProperties;
		private ValueBindingHelper<RoadLaneUIBinder[]> RoadLanes;
		private ValueBindingHelper<bool> IsPaused;

		public RoadBuilderToolMode Mode => RoadBuilderMode;
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

			toolSystem.EventToolChanged += OnToolChanged;

			RoadBuilderMode = CreateBinding("RoadBuilderToolMode", RoadBuilderToolMode.None);
			RoadProperties = CreateBinding("GetRoadProperties", new RoadPropertiesUIBinder());
			RoadLanes = CreateBinding("GetRoadLanes", new RoadLaneUIBinder[0]);
			IsPaused = CreateBinding("IsPaused", simulationSystem.selectedSpeed == 0f);

			CreateTrigger<RoadPropertiesUIBinder> ("SetRoadProperties", x => UpdateRoad(c => UpdateProperties(c, x)));
			CreateTrigger<RoadLaneUIBinder[]> ("SetRoadLanes", x => UpdateRoad(c => UpdateLaneOrder(c, x)));
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

		private void ClearTool()
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
				return;

			RoadProperties.Value = RoadPropertiesUIBinder.From(config);
			RoadLanes.Value = From(config, roadBuilderSystem.RoadGenerationData);
		}

		private void UpdateRoad(Action<INetworkConfig> action)
		{
			var createNew = RoadBuilderMode.Value is RoadBuilderToolMode.EditingSingle;
			var config = createNew
				? roadBuilderSystem.GenerateConfiguration(workingEntity)
				: roadBuilderSystem.GetOrGenerateConfiguration(workingEntity);

			if (config == null)
				return;

			action(config);

			roadBuilderSystem.UpdateRoad(config, workingEntity, createNew);

			RoadBuilderMode.Value = RoadBuilderToolMode.Editing;
			RoadProperties.Value = RoadPropertiesUIBinder.From(config);
			RoadLanes.Value = From(config, roadBuilderSystem.RoadGenerationData);
		}

		private void UpdateProperties(INetworkConfig config, RoadPropertiesUIBinder roadProperties)
		{
			roadProperties.Fill(config);
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
					newLanes.Add(item.ToLaneConfig());
				}
			}

			config.Lanes = newLanes;
		}

		private void LaneOptionClicked(INetworkConfig config, int index, int option, int id, int value)
		{
			var existingLane = config.Lanes.ElementAtOrDefault(index);

			if (existingLane != null)
			{
				LaneOptionsUtil.OptionClicked(config, existingLane, option, id, value);
			}
		}

		private RoadLaneUIBinder[] From(INetworkConfig config, RoadGenerationData roadGenerationData)
		{
			var binders = new RoadLaneUIBinder[config.Lanes.Count];

			for (var i = 0; i < binders.Length; i++)
			{
				var lane = config.Lanes[i];
				var validSection = roadGenerationData.NetSectionPrefabs.TryGetValue(lane.SectionPrefabName, out var section);

				binders[i] = new RoadLaneUIBinder
				{
					SectionPrefabName = lane.SectionPrefabName,
					Index = i,
					Invert = lane.Invert,
					Width = validSection ? section.CalculateWidth() : 1F,
					Options = LaneOptionsUtil.GenerateOptions(lane),
					NetSection = !validSection ? new() : new()
					{
						PrefabName = section.name,
						DisplayName = GetAssetName(section),
						Thumbnail = ImageSystem.GetThumbnail(section)
					}
				};
			}

			return binders;
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
