using Colossal.Entities;
using Colossal.Win32;

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
		private RoadBuilderUpdateSystem roadBuilderUpdateSystem;
		private PrefabSystem prefabSystem;
		private CameraUpdateSystem cameraUpdateSystem;
		private ToolSystem toolSystem;
		private EntityQuery prefabRefQuery;

		private ValueBindingHelper<RoadConfigurationUIBinder[]> RoadConfigurations;

		private string lastFindId;
		private int lastFindIndex;

		protected override void OnCreate()
		{
			base.OnCreate();

			roadBuilderSystem = World.GetOrCreateSystemManaged<RoadBuilderSystem>();
			roadBuilderUISystem = World.GetOrCreateSystemManaged<RoadBuilderUISystem>();
			roadBuilderUpdateSystem = World.GetOrCreateSystemManaged<RoadBuilderUpdateSystem>();
			prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
			cameraUpdateSystem = World.GetOrCreateSystemManaged<CameraUpdateSystem>();
			toolSystem = World.GetOrCreateSystemManaged<ToolSystem>();

			prefabRefQuery = SystemAPI.QueryBuilder()
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
				ID = x.Config.ID,
				Name = x.Config.Name,
				Thumbnail = ImageSystem.GetIcon(x.Prefab)
			}).ToArray();
		}

		private void ActivateRoad(string id)
		{
			var prefab = roadBuilderSystem.Configurations.FirstOrDefault(x => x.Config.ID == id);

			if (prefab is null)
			{
				return;
			}

			toolSystem.ActivatePrefabTool(prefab.Prefab);
		}

		private void EditRoad(string id)
		{
			var prefab = roadBuilderSystem.Configurations.FirstOrDefault(x => x.Config.ID == id);

			if (prefab is null)
			{
				return;
			}

			roadBuilderUISystem.SetWorkingPrefab(prefab.Config, Domain.Enums.RoadBuilderToolMode.EditingNonExistent);
		}

		private void FindRoad(string id)
		{
			var prefab = roadBuilderSystem.Configurations.FirstOrDefault(x => x.Config.ID == id);

			if (prefab is null)
			{
				return;
			}

			if (lastFindId != id)
			{
				lastFindIndex = 0;
			}

			lastFindId = id;

			var prefabEntity = prefabSystem.GetEntity(prefab.Prefab);
			var edgeEntities = prefabRefQuery.ToEntityArray(Allocator.Temp);
			var index = 0;
			var first = Entity.Null;

			for (var i = 0; i < edgeEntities.Length; i++)
			{
				if (EntityManager.TryGetComponent<PrefabRef>(edgeEntities[i], out var prefabRef) && prefabRef.m_Prefab == prefabEntity)
				{
					if (index == lastFindIndex)
					{
						JumpTo(edgeEntities[i]);

						return;
					}

					if (index == 0)
					{
						first = edgeEntities[i];
					}

					index++;
				}
			}

			JumpTo(first);
		}

		private void JumpTo(Entity entity)
		{
			if (cameraUpdateSystem.orbitCameraController != null && entity != Entity.Null)
			{
				lastFindIndex++;
				cameraUpdateSystem.orbitCameraController.followedEntity = entity;
				cameraUpdateSystem.orbitCameraController.TryMatchPosition(cameraUpdateSystem.activeCameraController);
				cameraUpdateSystem.activeCameraController = cameraUpdateSystem.orbitCameraController;
			}
		}

		private void DeleteRoad(string id)
		{
			GameManager.instance.userInterface.appBindings.ShowConfirmationDialog(new ConfirmationDialog(null, "RoadBuilder.DIALOG_MESSAGE[DELETE]", "Common.DIALOG_ACTION[Yes]", "Common.DIALOG_ACTION[No]"), msg => { if (msg == 0) ApplyDeleteRoad(id); });
		}

		private void ApplyDeleteRoad(string id)
		{
			var prefab = roadBuilderSystem.Configurations.FirstOrDefault(x => x.Config.ID == id);

			if (prefab is null)
			{
				return;
			}

			if (roadBuilderUISystem.WorkingId == prefab.Config.ID)
			{
				roadBuilderUISystem.WorkingEntity = Entity.Null;
				roadBuilderUISystem.Mode = RoadBuilderToolMode.Picker;
			}

			LocalSaveUtil.DeletePreviousLocalConfig(prefab.Config);

			var prefabEntity = prefabSystem.GetEntity(prefab.Prefab);
			var edgeEntities = prefabRefQuery.ToEntityArray(Allocator.Temp);
			var edgeList = new HashSet<Entity>(edgeEntities);

			for (var i = 0; i < edgeEntities.Length; i++)
			{
				if (EntityManager.TryGetComponent<PrefabRef>(edgeEntities[i], out var prefabRef) && prefabRef.m_Prefab == prefabEntity)
				{
					EntityManager.AddComponent<Deleted>(edgeEntities[i]);

					foreach (var edge in roadBuilderUpdateSystem.GetEdges(edgeEntities[i]))
					{
						edgeList.Add(edge);
					}
				}
			}

			foreach (var entity in edgeList)
			{
				roadBuilderUpdateSystem.UpdateEdge(entity);
			}

			roadBuilderSystem.Configurations.Remove(prefab);

			prefab.Prefab.Remove<UIObject>();
			prefab.Prefab.Remove<ServiceObject>();

			roadBuilderSystem.UpdatePrefab(prefab.Prefab);

			UpdateConfigurationList();
		}
	}
}
