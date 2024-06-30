using Game.Prefabs;
using Game.Simulation;
using Game.Tools;
using Game.UI.InGame;

using RoadBuilder.Domain.Configuration;
using RoadBuilder.Domain.Enums;
using RoadBuilder.Domain.UI;

using System;
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
			RoadProperties = CreateBinding("GetRoadProperties", "SetRoadProperties", new RoadPropertiesUIBinder(), _ => UpdateRoad());
            RoadLanes = CreateBinding("GetRoadLanes", "SetRoadLanes", new RoadLaneUIBinder[0], _ => UpdateRoad());
			IsPaused = CreateBinding("IsPaused", simulationSystem.selectedSpeed == 0f);

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

			RoadProperties.Value = RoadPropertiesUIBinder.From(config);
			RoadLanes.Value = RoadLaneUIBinder.From(config);
		}

		private void UpdateRoad()
		{
            var createNew = RoadBuilderMode.Value is RoadBuilderToolMode.EditingSingle;
			var config = createNew
                ? roadBuilderSystem.GenerateConfiguration(workingEntity)
                : roadBuilderSystem.GetOrGenerateConfiguration(workingEntity);

            RoadProperties.Value.Fill(config);

			config.Lanes = RoadLanes.Value.Select(x => x.ToLaneConfig()).ToList();

            roadBuilderSystem.UpdateRoad(config, workingEntity, createNew);

            RoadBuilderMode.Value = RoadBuilderToolMode.Editing;
		}
	}
}
