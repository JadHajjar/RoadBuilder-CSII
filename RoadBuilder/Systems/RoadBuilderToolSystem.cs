using Colossal.Entities;
using Colossal.Serialization.Entities;

using Game;
using Game.Common;
using Game.Input;
using Game.Net;
using Game.Prefabs;
using Game.SceneFlow;
using Game.Tools;
using Game.UI.Editor;

using RoadBuilder.Domain;
using RoadBuilder.Domain.Components;
using RoadBuilder.Domain.Enums;
using RoadBuilder.Domain.Prefabs;
using RoadBuilder.Systems.UI;

using System;
using System.Linq;

using Unity.Collections;

using Unity.Entities;
using Unity.Jobs;

using UnityEngine.InputSystem;

namespace RoadBuilder.Systems
{
	public partial class RoadBuilderToolSystem : ToolBaseSystem
	{
		private PrefabSystem prefabSystem;
		private RoadBuilderUISystem roadBuilderUISystem;
		private RoadBuilderSystem roadBuilderSystem;
		private ToolSystem toolSystem;
		private EditorToolUISystem editorToolUISystem;
		private EntityQuery highlightedQuery;
		private EntityQuery roadBuilderNetworkQuery;
		private ProxyAction placeAction;
		private ProxyAction applyAction;
		private ProxyAction cancelAction;

		public override string toolID { get; } = "RoadBuilderTool";

		protected override void OnCreate()
		{
			base.OnCreate();

			prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
			roadBuilderUISystem = World.GetOrCreateSystemManaged<RoadBuilderUISystem>();
			roadBuilderSystem = World.GetOrCreateSystemManaged<RoadBuilderSystem>();
			toolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
			editorToolUISystem = World.GetExistingSystemManaged<EditorToolUISystem>();

			highlightedQuery = SystemAPI.QueryBuilder().WithAny<Highlighted, RoadBuilderUpdateFlagComponent>().Build();
			roadBuilderNetworkQuery = GetEntityQuery(ComponentType.ReadOnly<RoadBuilderNetwork>());

			placeAction = Mod.Settings.GetAction(nameof(Setting.PlaceToggle));
			applyAction = Mod.Settings.GetAction(nameof(RoadBuilder) + "Apply");
			cancelAction = Mod.Settings.GetAction(nameof(RoadBuilder) + "Cancel");

			var builtInApplyAction = InputManager.instance.FindAction(InputManager.kToolMap, "Apply");
			var mimicApplyBinding = applyAction.bindings.FirstOrDefault(b => b.group == nameof(Mouse));
			var builtInApplyBinding = builtInApplyAction.bindings.FirstOrDefault(b => b.group == nameof(Mouse));

			mimicApplyBinding.path = builtInApplyBinding.path;
			mimicApplyBinding.modifiers = builtInApplyBinding.modifiers;

			var builtInCancelAction = InputManager.instance.FindAction(InputManager.kToolMap, "Mouse Cancel");
			var mimicCancelBinding = cancelAction.bindings.FirstOrDefault(b => b.group == nameof(Mouse));
			var builtInCancelBinding = builtInCancelAction.bindings.FirstOrDefault(b => b.group == nameof(Mouse));

			mimicCancelBinding.path = builtInCancelBinding.path;
			mimicCancelBinding.modifiers = builtInCancelBinding.modifiers;

			InputManager.instance.SetBinding(mimicApplyBinding, out _);
			InputManager.instance.SetBinding(mimicCancelBinding, out _);
		}

		protected override void OnGamePreload(Purpose purpose, GameMode mode)
		{
			base.OnGamePreload(purpose, mode);

			if (mode == GameMode.Editor)
			{
				if (editorToolUISystem.tools.Any(t => t.id == toolID))
				{
					return;
				}

				var tools = editorToolUISystem.tools;
				Array.Resize(ref tools, tools.Length + 1);
				tools[tools.Length - 1] = new RoadBuilderEditorTool(World, this, roadBuilderUISystem);
				editorToolUISystem.tools = tools;
			}
		}

		public override void InitializeRaycast()
		{
			base.InitializeRaycast();

			m_ToolRaycastSystem.netLayerMask = Layer.All;// | Layer.Road | Layer.TrainTrack | Layer.TramTrack | Layer.SubwayTrack | Layer.Pathway | Layer.PublicTransportRoad /*| Layer.Fence*/ ;
			m_ToolRaycastSystem.typeMask = TypeMask.Net;
			m_ToolRaycastSystem.collisionMask = CollisionMask.OnGround | CollisionMask.Overground;
		}

		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			placeAction.shouldBeEnabled = roadBuilderUISystem.Mode is RoadBuilderToolMode.Editing or RoadBuilderToolMode.EditingNonExistent;
			applyAction.shouldBeEnabled = roadBuilderUISystem.Mode is RoadBuilderToolMode.Picker;
			cancelAction.shouldBeEnabled = true;

			if (placeAction.WasPerformedThisFrame())
			{
				if (roadBuilderSystem.Configurations.TryGetValue(roadBuilderUISystem.GetWorkingId(), out var prefab))
				{
					roadBuilderUISystem.ActivateRoad(prefab.Prefab);
				}

				return base.OnUpdate(inputDeps);
			}

			if (cancelAction.WasPerformedThisFrame())
			{
				if (roadBuilderUISystem.Mode is RoadBuilderToolMode.Picker)
				{
					applyAction.shouldBeEnabled = false;

					roadBuilderUISystem.ClearTool();
				}
				else
				{
					roadBuilderUISystem.Mode = RoadBuilderToolMode.Picker;
				}

				return base.OnUpdate(inputDeps);
			}

			switch (roadBuilderUISystem.Mode)
			{
				case RoadBuilderToolMode.Picker:
				{
					var raycastHit = HandlePicker(out var entity);

					HandleHighlight(highlightedQuery, raycastHit ? x => x == entity : null);

					if (raycastHit)
					{
						TryHighlightEntity(entity);
					}

					break;
				}

				case RoadBuilderToolMode.Editing:
				case RoadBuilderToolMode.EditingNonExistent:
				{
					var workingId = roadBuilderUISystem.GetWorkingId();

					HandleHighlight(roadBuilderNetworkQuery, x => IsWorkingPrefab(x, workingId));

					break;
				}

				case RoadBuilderToolMode.EditingSingle:
				{
					HandleHighlight(highlightedQuery, x => x == roadBuilderUISystem.WorkingEntity);

					TryHighlightEntity(roadBuilderUISystem.WorkingEntity);

					break;
				}
			}

			return base.OnUpdate(inputDeps);
		}

		private void TryHighlightEntity(Entity entity)
		{
			if (entity != Entity.Null && !EntityManager.HasComponent<Highlighted>(entity))
			{
				EntityManager.AddComponent<Highlighted>(entity);
				EntityManager.AddComponent<BatchesUpdated>(entity);
			}
		}

		private bool IsWorkingPrefab(Entity entity, string workingId)
		{
			if (!EntityManager.TryGetComponent<PrefabRef>(entity, out var prefabRef))
			{
				return false;
			}

			return workingId == prefabSystem.GetPrefabName(prefabRef);
		}

		private bool HandlePicker(out Entity entity)
		{
			if (!GetRaycastResult(out entity, out var hit))
			{
				return false;
			}

			if (!EntityManager.TryGetComponent<PrefabRef>(entity, out var prefabRef) || EntityManager.HasComponent<Owner>(entity) || !EntityManager.HasComponent<Edge>(entity))
			{
				return false;
			}

			if (!Mod.Settings.RemoveLockRequirements && GameManager.instance.gameMode == GameMode.Game && EntityManager.HasEnabledComponent<Locked>(prefabRef.m_Prefab))
			{
				return false;
			}

			if (!prefabSystem.TryGetPrefab<NetGeometryPrefab>(prefabRef, out var prefab))
			{
				return false;
			}

			if (prefab is not (RoadPrefab or TrackPrefab or FencePrefab or PathwayPrefab))
			{
				return false;
			}

			if (prefab is RoadPrefab && !EntityManager.HasComponent<Road>(entity))
			{
				return false;
			}

			if (prefab.Has<Bridge>())
			{
				return false;
			}

			if (applyAction.WasPerformedThisFrame())
			{
				if (prefab is INetworkBuilderPrefab)
				{
					roadBuilderUISystem.ShowActionPopup(entity, prefab);
				}
				else
				{
					roadBuilderUISystem.CreateNewPrefab(entity);
				}
			}

			return true;
		}

		private void HandleHighlight(EntityQuery query, Func<Entity, bool> shouldBeHighlighted)
		{
			var entities = query.ToEntityArray(Allocator.Temp);
			var editing = roadBuilderUISystem.Mode >= RoadBuilderToolMode.Editing;

			for (var i = 0; i < entities.Length; i++)
			{
				var entity = entities[i];

				if (shouldBeHighlighted != null && shouldBeHighlighted(entity))
				{
					EntityManager.AddComponent<Highlighted>(entity);

					if (editing)
					{
						EntityManager.AddComponent<RoadBuilderUpdateFlagComponent>(entity);
					}
				}
				else
				{
					EntityManager.RemoveComponent<Highlighted>(entity);
					EntityManager.RemoveComponent<RoadBuilderUpdateFlagComponent>(entity);
				}

				EntityManager.AddComponent<BatchesUpdated>(entity);
			}
		}

		protected override void OnStopRunning()
		{
			base.OnStopRunning();

			applyAction.shouldBeEnabled = false;
			cancelAction.shouldBeEnabled = false;

			var entities = highlightedQuery.ToEntityArray(Allocator.Temp);

			for (var i = 0; i < entities.Length; i++)
			{
				var entity = entities[i];

				EntityManager.RemoveComponent<RoadBuilderUpdateFlagComponent>(entity);
				EntityManager.RemoveComponent<Highlighted>(entity);
				EntityManager.AddComponent<BatchesUpdated>(entity);
			}
		}

		public override bool TrySetPrefab(PrefabBase prefab)
		{
			return false;
		}

		public override PrefabBase GetPrefab()
		{
			return default;
		}
	}
}
