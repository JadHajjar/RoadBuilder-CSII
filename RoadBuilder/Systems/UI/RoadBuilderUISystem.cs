using Game.Prefabs;
using Game.Tools;
using Game.UI.InGame;
using RoadBuilder.Domain;
using Unity.Entities;

namespace RoadBuilder.Systems.UI
{
    public partial class RoadBuilderUISystem : ExtendedUISystemBase
    {
        private Entity workingEntity;

        private PrefabSystem prefabSystem;
        private PrefabUISystem prefabUISystem;
        private ToolSystem toolSystem;
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
            roadBuilderToolSystem = World.GetOrCreateSystemManaged<RoadBuilderToolSystem>();
            defaultToolSystem = World.GetOrCreateSystemManaged<DefaultToolSystem>();

            toolSystem.EventToolChanged += OnToolChanged;

            RoadBuilderMode = CreateBinding("RoadBuilderToolMode", RoadBuilderToolMode.None);

            CreateTrigger("ToggleTool", ToggleTool);
            CreateTrigger("ActionPopup.New", CreateNewPrefab);
            CreateTrigger("ActionPopup.Edit", EditPrefab);
            CreateTrigger("ActionPopup.Cancel", ClearTool);
        }

        private void OnToolChanged(ToolBaseSystem system)
        {
            if (system is not RoadBuilderToolSystem)
            {
                RoadBuilderMode.Value = RoadBuilderToolMode.None;
            }
        }

        internal void ShowActionPopup(Entity entity)
        {
            workingEntity = entity;
            RoadBuilderMode.Value = RoadBuilderToolMode.ActionSelection;
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

        private void EditPrefab()
        {
            RoadBuilderMode.Value = RoadBuilderToolMode.Editing;

        }

        private void CreateNewPrefab()
        {
            RoadBuilderMode.Value = RoadBuilderToolMode.Editing;

        }
    }
}
