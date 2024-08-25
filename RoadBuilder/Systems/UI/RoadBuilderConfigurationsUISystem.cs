using Colossal.Entities;

using Game.Common;
using Game.Net;
using Game.Prefabs;
using Game.Rendering;
using Game.SceneFlow;
using Game.Tools;
using Game.UI;

using RoadBuilder.Domain.Components;
using RoadBuilder.Domain.Enums;
using RoadBuilder.Domain.UI;
using RoadBuilder.Utilities;

using System.Collections.Generic;
using System.Linq;

using Unity.Collections;
using Unity.Entities;

namespace RoadBuilder.Systems.UI
{
	public partial class RoadBuilderConfigurationsUISystem : ExtendedUISystemBase
	{
		private RoadBuilderSystem roadBuilderSystem;
		private RoadBuilderUISystem roadBuilderUISystem;
		private RoadBuilderPrefabUpdateSystem roadBuilderUpdateSystem;
		private PrefabSystem prefabSystem;
		private CameraUpdateSystem cameraUpdateSystem;
		private ToolSystem toolSystem;
		private EntityQuery prefabRefQuery;
		private EntityQuery edgeRefQuery;
		private ValueBindingHelper<RoadConfigurationUIBinder[]> RoadConfigurations;

		private string lastFindId;
		private int lastFindIndex;

		protected override void OnCreate()
		{
			base.OnCreate();

			roadBuilderSystem = World.GetOrCreateSystemManaged<RoadBuilderSystem>();
			roadBuilderUISystem = World.GetOrCreateSystemManaged<RoadBuilderUISystem>();
			roadBuilderUpdateSystem = World.GetOrCreateSystemManaged<RoadBuilderPrefabUpdateSystem>();
			prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
			cameraUpdateSystem = World.GetOrCreateSystemManaged<CameraUpdateSystem>();
			toolSystem = World.GetOrCreateSystemManaged<ToolSystem>();

			roadBuilderSystem.ConfigurationsUpdated += UpdateConfigurationList;

			prefabRefQuery = SystemAPI.QueryBuilder()
				.WithAll<RoadBuilderNetwork, PrefabRef>()
				.Build();

			edgeRefQuery = SystemAPI.QueryBuilder()
				.WithAll<RoadBuilderNetwork, PrefabRef, Edge>()
				.Build();

			RoadConfigurations = CreateBinding("GetRoadConfigurations", new RoadConfigurationUIBinder[0]);
			CreateTrigger<string>("ActivateRoad", ActivateRoad);
			CreateTrigger<string>("EditRoad", EditRoad);
			CreateTrigger<string>("FindRoad", FindRoad);
			CreateTrigger<string>("DeleteRoad", DeleteRoad);
		}

		public void UpdateConfigurationList()
		{
			RoadConfigurations.Value = roadBuilderSystem.Configurations.Select(x => new RoadConfigurationUIBinder
			{
				ID = x.Key,
#if DEBUG
				Name = x.Value.Config.Name + " - " + x.Value.Config.ID,
#else
				Name = x.Value.Config.Name,
#endif
				Locked = !Mod.Settings.RemoveLockRequirements && EntityManager.HasEnabledComponent<Locked>(prefabSystem.GetEntity(x.Value.Prefab)),
				Thumbnail = ImageSystem.GetIcon(x.Value.Prefab)
			}).ToArray();
		}

		private void ActivateRoad(string id)
		{
			if (!roadBuilderSystem.Configurations.TryGetValue(id, out var prefab))
			{
				return;
			}

			toolSystem.ActivatePrefabTool(prefab.Prefab);
		}

		private void EditRoad(string id)
		{
			if (!roadBuilderSystem.Configurations.TryGetValue(id, out var prefab))
			{
				return;
			}

			roadBuilderUISystem.SetWorkingPrefab(prefab.Config, RoadBuilderToolMode.EditingNonExistent);
		}

		private void FindRoad(string id)
		{
			if (!roadBuilderSystem.Configurations.TryGetValue(id, out var prefab))
			{
				return;
			}

			if (lastFindId != id)
			{
				lastFindIndex = 0;
			}

			lastFindId = id;

			var prefabEntity = prefabSystem.GetEntity(prefab.Prefab);
			var edgeEntities = edgeRefQuery.ToEntityArray(Allocator.Temp);
			var index = 0;
			var first = Entity.Null;

			for (var i = 0; i < edgeEntities.Length; i++)
			{
				var entity = edgeEntities[i];

				if (EntityManager.TryGetComponent<PrefabRef>(entity, out var prefabRef) && prefabRef.m_Prefab == prefabEntity)
				{
					if (index == lastFindIndex)
					{
						JumpTo(entity);

						return;
					}

					if (index == 0)
					{
						first = entity;
					}

					index++;
				}
			}

			lastFindIndex = 0;
			JumpTo(first);
		}

		private void JumpTo(Entity entity)
		{
			lastFindIndex++;

			if (cameraUpdateSystem.orbitCameraController != null && entity != Entity.Null)
			{
				cameraUpdateSystem.orbitCameraController.followedEntity = entity;
				cameraUpdateSystem.orbitCameraController.TryMatchPosition(cameraUpdateSystem.activeCameraController);
				cameraUpdateSystem.activeCameraController = cameraUpdateSystem.orbitCameraController;
			}
		}

		private void DeleteRoad(string id)
		{
			GameManager.instance.userInterface.appBindings.ShowConfirmationDialog(new ConfirmationDialog("Options.SECTION[RoadBuilder.RoadBuilder.Mod]", "RoadBuilder.DIALOG_MESSAGE[DELETE]", "Common.DIALOG_ACTION[Yes]", "Common.DIALOG_ACTION[No]"), msg =>
			{
				if (msg == 0)
				{
					ApplyDeleteRoad(id);
				}
			});
		}

		private void ApplyDeleteRoad(string id)
		{
			if (!roadBuilderSystem.Configurations.TryGetValue(id, out var prefab))
			{
				return;
			}

			prefab.Deleted = true;

			if (roadBuilderUISystem.GetWorkingId() == prefab.Config.ID)
			{
				roadBuilderUISystem.SetWorkingPrefab(null, RoadBuilderToolMode.Picker);
			}

			LocalSaveUtil.DeletePreviousLocalConfig(prefab.Config);

			var prefabEntity = prefabSystem.GetEntity(prefab.Prefab);
			var edgeEntities = prefabRefQuery.ToEntityArray(Allocator.Temp);
			var edgeList = new HashSet<Entity>();

			for (var i = 0; i < edgeEntities.Length; i++)
			{
				var entity = edgeEntities[i];

				if (EntityManager.TryGetComponent<PrefabRef>(entity, out var prefabRef) && prefabRef.m_Prefab == prefabEntity)
				{
					if (EntityManager.HasComponent<Edge>(entity))
					{
						EntityManager.AddComponent<Deleted>(entity);

						foreach (var item in EntityManager.GetBuffer<Game.Objects.SubObject>(entity))
						{
							EntityManager.AddComponent<Deleted>(item.m_SubObject);
						}

						foreach (var edge in roadBuilderUpdateSystem.GetEdges(entity))
						{
							edgeList.Add(edge);
						}
					}
					else
					{
						EntityManager.AddComponent<RoadBuilderToBeDeletedComponent>(entity);
						EntityManager.AddComponent<Updated>(entity);
					}

					EntityManager.RemoveComponent<RoadBuilderNetwork>(entity);
				}
			}

			foreach (var entity in edgeList)
			{
				roadBuilderUpdateSystem.UpdateEdge(entity);
			}

			prefab.Prefab.components.Clear();

			roadBuilderSystem.UpdatePrefab(prefab.Prefab);

			roadBuilderSystem.UpdateConfigurationList();
		}
	}
}
