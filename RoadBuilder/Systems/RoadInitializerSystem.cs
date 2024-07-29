using Game;

using RoadBuilder.Utilities;

using System.Linq;

namespace RoadBuilder.Systems
{
	public partial class RoadInitializerSystem : GameSystemBase
	{
		private RoadBuilderSystem roadBuilderSystem;
		private RoadGenerationDataSystem roadGenerationDataSystem;

		protected override void OnCreate()
		{
			base.OnCreate();

			roadBuilderSystem = World.GetOrCreateSystemManaged<RoadBuilderSystem>();
			roadGenerationDataSystem = World.GetOrCreateSystemManaged<RoadGenerationDataSystem>();
		}

		protected override void OnUpdate()
		{
			if (roadGenerationDataSystem.RoadGenerationData is null)
			{
				return;
			}

			Mod.Log.Info("RoadInitializerSystem.OnUpdate");

			Enabled = false;

			roadBuilderSystem.InitializeExistingRoadPrefabs(LocalSaveUtil.LoadConfigs().ToList());
		}
	}
}
