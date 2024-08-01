using Colossal.Serialization.Entities;

using Game;
using Game.Common;
using Game.Prefabs;

using RoadBuilder.Utilities;

using System.Linq;

using Unity.Collections;
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
				roadBuilderSystem.AddPrefab(item);
			}

			roadBuilderSystem.UpdateConfigurationList();
		}
	}
}
