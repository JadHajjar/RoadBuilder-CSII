using Game;

using RoadBuilder.Utilities;

using Unity.Entities;

namespace RoadBuilder.Systems
{
	public partial class RoadBuilderInitializerSystem : GameSystemBase
	{
		private RoadBuilderSystem roadBuilderSystem;
		private RoadBuilderGenerationDataSystem roadGenerationDataSystem;

		protected override void OnCreate()
		{
			base.OnCreate();

			roadBuilderSystem = World.GetOrCreateSystemManaged<RoadBuilderSystem>();
			roadGenerationDataSystem = World.GetOrCreateSystemManaged<RoadBuilderGenerationDataSystem>();
		}

		protected override void OnUpdate()
		{
			if (roadGenerationDataSystem.RoadGenerationData is null)
			{
				return;
			}

			Mod.Log.Info("RoadInitializerSystem.OnUpdate");

			Enabled = false;

			foreach (var config in LocalSaveUtil.LoadConfigs())
			{
				Mod.Log.Debug($"LoadLocalNetwork: {config.GetType().Name} {config.ID}");

				config.ApplyVersionChanges();

				roadBuilderSystem.AddPrefab(config);
			}

			roadBuilderSystem.UpdateConfigurationList();
		}
	}
}
