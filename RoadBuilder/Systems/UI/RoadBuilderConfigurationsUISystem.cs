using Colossal.Entities;

using Game;
using Game.Common;
using Game.Net;
using Game.Prefabs;
using Game.Rendering;
using Game.SceneFlow;
using Game.UI;

using RoadBuilder.Domain.Components;
using RoadBuilder.Domain.Enums;
using RoadBuilder.Domain.UI;
using RoadBuilder.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;

using Unity.Collections;
using Unity.Entities;

namespace RoadBuilder.Systems.UI
{
	public partial class RoadBuilderConfigurationsUISystem : ExtendedUISystemBase
	{
		private RoadBuilderSystem roadBuilderSystem;
		private RoadBuilderRoadTrackerSystem roadBuilderRoadTrackerSystem;
		private PrefabSystem prefabSystem;
		private RoadBuilderGenericFunctionsSystem genericFunctionsSystem;
		private ValueBindingHelper<RoadConfigurationUIBinder[]> RoadConfigurations;
		private string query = string.Empty;

		public event Action ConfigurationsUpdated;

		public List<RoadConfigurationUIBinder> AvailableConfigurations { get; private set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			roadBuilderSystem = World.GetOrCreateSystemManaged<RoadBuilderSystem>();
			roadBuilderRoadTrackerSystem = World.GetOrCreateSystemManaged<RoadBuilderRoadTrackerSystem>();
			prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
			genericFunctionsSystem = World.GetOrCreateSystemManaged<RoadBuilderGenericFunctionsSystem>();

			roadBuilderSystem.ConfigurationsUpdated += UpdateConfigurationList;

			RoadConfigurations = CreateBinding("GetRoadConfigurations", new RoadConfigurationUIBinder[0]);

			CreateTrigger<string>("ActivateRoad", genericFunctionsSystem.ActivateRoad);
			CreateTrigger<string>("EditRoad", genericFunctionsSystem.EditRoad);
			CreateTrigger<string>("FindRoad", genericFunctionsSystem.FindRoad);
			CreateTrigger<string>("DeleteRoad", genericFunctionsSystem.DeleteRoad);
			CreateTrigger<string>("Roads.SetSearchQuery", SetSearchQuery);
		}

		private void SetSearchQuery(string obj)
		{
			query = obj;

			RoadConfigurations.Value = AvailableConfigurations
				.Where(x => Filter(x.Name))
				.ToArray();
		}

		private bool Filter(string name)
		{
			return string.IsNullOrWhiteSpace(query) || query.SearchCheck(name);
		}

		public void UpdateConfigurationList()
		{
			AvailableConfigurations = roadBuilderSystem.Configurations.Select(x => new RoadConfigurationUIBinder
			{
				ID = x.Key,
#if DEBUG&&false
				Name = x.Value.Config.Name + " - " + x.Value.Config.ID,
#else
				Name = x.Value.Config.Name,
#endif
				IsNotInPlayset = !x.Value.Config.IsInPlayset(),
				Locked = !Mod.Settings.RemoveLockRequirements && GameManager.instance.gameMode == GameMode.Game && EntityManager.HasEnabledComponent<Locked>(prefabSystem.GetEntity(x.Value.Prefab)),
				Used = roadBuilderRoadTrackerSystem.UsedNetworkPrefabs.Contains(x.Value),
				Category = x.Value.Config.Category,
				Thumbnail = ImageSystem.GetIcon(x.Value.Prefab)
			})
				.OrderBy(x => x.Locked)
				.ThenBy(x => x.Name)
				.ToList();

			SetSearchQuery(query);

			ConfigurationsUpdated?.Invoke();
		}
	}
}
