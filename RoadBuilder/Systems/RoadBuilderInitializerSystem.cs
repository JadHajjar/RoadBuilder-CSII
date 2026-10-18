using Game;

using RoadBuilder.Utilities;

using System;

using Unity.Entities;

namespace RoadBuilder.Systems
{
	public partial class RoadBuilderInitializerSystem : GameSystemBase
	{
#nullable disable
		private RoadBuilderSystem roadBuilderSystem;
		private RoadBuilderGenerationDataSystem roadGenerationDataSystem;
#nullable enable

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

			var configs = LocalSaveUtil.LoadConfigs();
			var count = 0;

			foreach (var config in configs)
			{
				Mod.Log.Debug($"LoadLocalNetwork: {config.GetType().Name} id={config.ID} name={config.Name}");

				try
				{
					config.ApplyVersionChanges();

					roadBuilderSystem.AddPrefab(config);

					count++;
				}
				catch (Exception ex)
				{
					Mod.Log.Error(ex, $"Failed to load local network config: {config.GetType().Name} id={config.ID}");
				}
			}

			Mod.Log.Info($"RoadInitializerSystem: loaded {count} network configs");

			roadBuilderSystem.UpdateConfigurationList();
		}
	}
}
