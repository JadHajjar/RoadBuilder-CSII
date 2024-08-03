using Game;

using RoadBuilder.Utilities;

using Unity.Entities;

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

			foreach (var item in LocalSaveUtil.LoadConfigs())
			{
				Mod.Log.Debug($"LoadLocalNetwork: {item.GetType().Name} {item.ID}");

				roadBuilderSystem.AddPrefab(item, queueForUpdate: false);
			}

			roadBuilderSystem.UpdateConfigurationList();
		}
	}
}
