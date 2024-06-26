using Game.Prefabs;
using Game.Tools;
using Game.UI.InGame;
using RoadBuilder.Domain.Enums;
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

        private ValueBindingHelper<RoadBuilderToolMode> RoadBuilderMode;

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

            toolSystem.EventToolChanged += OnToolChanged;

            RoadBuilderMode = CreateBinding("RoadBuilderToolMode", RoadBuilderToolMode.None);

            CreateTrigger("ToggleTool", ToggleTool);
            CreateTrigger("ActionPopup.New", () => CreateNewPrefab(workingEntity));
            CreateTrigger("ActionPopup.Cancel", ClearTool);
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
            workingEntity = entity;
            RoadBuilderMode.Value = RoadBuilderToolMode.Editing;

            var config = roadBuilderSystem.GetOrGenerateConfiguration(workingEntity);


		}

		public void CreateNewPrefab(Entity entity)
        {
            workingEntity = entity;
			RoadBuilderMode.Value = RoadBuilderToolMode.EditingSingleMode;

			var config = roadBuilderSystem.GetOrGenerateConfiguration(workingEntity);


		}
	}
}
