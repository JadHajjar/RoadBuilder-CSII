using Colossal.Entities;

using Game.Common;
using Game.Input;
using Game.Net;
using Game.Prefabs;
using Game.Tools;

using RoadBuilder.Domain.Components;
using RoadBuilder.Domain.Enums;
using RoadBuilder.Domain.Prefabs;
using RoadBuilder.Systems.UI;
using RoadBuilder.Utilities;

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
		private EntityQuery highlightedQuery;
		private EntityQuery roadBuilderNetworkQuery;
		private ProxyAction applyAction;
		private ProxyAction cancelAction;

		public override string toolID { get; } = "RoadBuilderTool";

		protected override void OnCreate()
		{
			base.OnCreate();

			prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
			roadBuilderUISystem = World.GetOrCreateSystemManaged<RoadBuilderUISystem>();

			highlightedQuery = GetEntityQuery(ComponentType.ReadOnly<Highlighted>());
			roadBuilderNetworkQuery = GetEntityQuery(ComponentType.ReadOnly<RoadBuilderNetwork>());

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

		public override void InitializeRaycast()
		{
			base.InitializeRaycast();

			m_ToolRaycastSystem.netLayerMask = Layer.Road | Layer.TrainTrack | Layer.TramTrack | Layer.SubwayTrack | Layer.Pathway | Layer.Fence | Layer.PublicTransportRoad;
			m_ToolRaycastSystem.typeMask = TypeMask.Net;
			m_ToolRaycastSystem.collisionMask = CollisionMask.OnGround;
		}

		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			cancelAction.shouldBeEnabled = true;

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

			applyAction.shouldBeEnabled = roadBuilderUISystem.Mode is RoadBuilderToolMode.Picker;

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
				{
					HandleHighlight(roadBuilderNetworkQuery, IsWorkingEntityPrefab);

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
			if (!EntityManager.HasComponent<Highlighted>(entity))
			{
				EntityManager.AddComponent<Highlighted>(entity);
				EntityManager.AddComponent<BatchesUpdated>(entity);
			}
		}

		private bool IsWorkingEntityPrefab(Entity entity)
		{
			if (!EntityManager.TryGetComponent<PrefabRef>(entity, out var prefabRef))
			{
				return false;
			}

			return EntityManager.GetComponentData<PrefabRef>(roadBuilderUISystem.WorkingEntity).m_Prefab == prefabRef;
		}

		private bool HandlePicker(out Entity entity)
		{
			if (!GetRaycastResult(out entity, out var hit))
			{
				return false;
			}

			if (!EntityManager.TryGetComponent<PrefabRef>(entity, out var prefabRef))
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

			if (applyAction.WasPerformedThisFrame())
			{
				if (prefab is INetworkBuilderPrefab)
				{
					roadBuilderUISystem.EditPrefab(entity);
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

			for (var i = 0; i < entities.Length; i++)
			{
				if (shouldBeHighlighted != null && shouldBeHighlighted(entities[i]))
				{
					EntityManager.AddComponent<Highlighted>(entities[i]);
				}
				else
				{
					EntityManager.RemoveComponent<Highlighted>(entities[i]);
				}

				EntityManager.AddComponent<BatchesUpdated>(entities[i]);
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
				EntityManager.RemoveComponent<Highlighted>(entities[i]);
				EntityManager.AddComponent<BatchesUpdated>(entities[i]);
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
