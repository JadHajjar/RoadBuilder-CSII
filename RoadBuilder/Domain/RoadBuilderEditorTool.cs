using Game.UI.Editor;

using RoadBuilder.Systems;
using RoadBuilder.Systems.UI;

using Unity.Entities;

namespace RoadBuilder.Domain
{
	internal class RoadBuilderEditorTool : EditorTool
	{
		private readonly RoadBuilderUISystem _roadBuilderUISystem;

		public RoadBuilderEditorTool(World world, RoadBuilderToolSystem toolSystem, RoadBuilderUISystem roadBuilderUISystem) : base(world)
		{
			_roadBuilderUISystem = roadBuilderUISystem;

			id = toolSystem.toolID;
			icon = "coui://roadbuildericons/EditRoadBuilder.svg";
			tool = toolSystem;
		}

		protected override void OnEnable()
		{
			_roadBuilderUISystem.ToggleTool(true);
		}

		protected override void OnDisable()
		{
			_roadBuilderUISystem.ToggleTool(false);
		}
	}
}
