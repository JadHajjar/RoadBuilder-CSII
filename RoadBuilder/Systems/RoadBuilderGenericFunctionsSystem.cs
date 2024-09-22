using Colossal.Entities;

using Game.Common;
using Game.Prefabs;
using Game.Rendering;
using Game.SceneFlow;

using RoadBuilder.Domain.Components;
using RoadBuilder.Domain.Enums;
using RoadBuilder.Domain.UI;
using RoadBuilder.Systems.UI;
using RoadBuilder.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Unity.Collections;
using Unity.Entities;
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

using System.Collections.Generic;
using System.Linq;

using Unity.Collections;
using Unity.Entities;
using Game.Tools;
using Game.UI.Editor;
using Game.UI.InGame;

namespace RoadBuilder.Systems
{
	public partial class RoadBuilderGenericFunctionsSystem : SystemBase
	{
		private RoadBuilderSystem roadBuilderSystem;
		private RoadBuilderUISystem roadBuilderUISystem;
		private RoadBuilderPrefabUpdateSystem roadBuilderUpdateSystem;
		private PrefabSystem prefabSystem;
		private PrefabUISystem prefabUISystem;
		private ToolSystem toolSystem;
		private EditorToolUISystem editorToolUISystem;
		private CameraUpdateSystem cameraUpdateSystem;

		private EntityQuery prefabRefQuery;
		private EntityQuery edgeRefQuery;

		private string lastFindId;
		private int lastFindIndex;

		protected override void OnCreate()
		{
			base.OnCreate();

			roadBuilderSystem = World.GetOrCreateSystemManaged<RoadBuilderSystem>();
			roadBuilderUISystem = World.GetOrCreateSystemManaged<RoadBuilderUISystem>();
			roadBuilderUpdateSystem = World.GetOrCreateSystemManaged<RoadBuilderPrefabUpdateSystem>();
			prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
			prefabUISystem = World.GetOrCreateSystemManaged<PrefabUISystem>();
			toolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
			editorToolUISystem = World.GetOrCreateSystemManaged<EditorToolUISystem>();
			cameraUpdateSystem = World.GetOrCreateSystemManaged<CameraUpdateSystem>();

			prefabRefQuery = SystemAPI.QueryBuilder()
				.WithAll<RoadBuilderNetwork, PrefabRef>()
				.Build();

			edgeRefQuery = SystemAPI.QueryBuilder()
				.WithAll<RoadBuilderNetwork, PrefabRef, Edge>()
				.Build();
		}

		protected override void OnUpdate()
		{ }

		public void ActivateRoad(string id)
		{
			if (!roadBuilderSystem.Configurations.TryGetValue(id, out var prefab))
			{
				return;
			}

			if (GameManager.instance.gameMode == GameMode.Editor)
			{
				editorToolUISystem.activeTool = editorToolUISystem.tools.First(x => x is EditorPrefabTool);

				GameManager.instance.RegisterUpdater(() => toolSystem.ActivatePrefabTool(prefab.Prefab));
			}
			else
			{
				toolSystem.ActivatePrefabTool(prefab.Prefab);
			}
		}

		public void EditRoad(string id)
		{
			if (!roadBuilderSystem.Configurations.TryGetValue(id, out var prefab))
			{
				return;
			}

			roadBuilderUISystem.SetWorkingPrefab(prefab.Config, RoadBuilderToolMode.EditingNonExistent);
		}

		public void FindRoad(string id)
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

		public void DeleteRoad(string id)
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
