using Colossal.Entities;

using Game.Common;
using Game.Input;
using Game.Net;
using Game.Prefabs;
using Game.Tools;

using RoadBuilder.Domain;
using RoadBuilder.Domain.Enums;
using RoadBuilder.Systems.UI;
using Unity.Collections;

using Unity.Entities;

using Unity.Jobs;

namespace RoadBuilder.Systems
{
    public partial class RoadBuilderToolSystem : ToolBaseSystem
	{
		private PrefabSystem prefabSystem;
		private RoadBuilderUISystem roadBuilderUISystem;
		private EntityQuery highlightedQuery;
		private ProxyAction applyAction;

		public override string toolID { get; } = "RoadBuilderTool";

		protected override void OnCreate()
		{
			base.OnCreate();

			prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
			roadBuilderUISystem = World.GetOrCreateSystemManaged<RoadBuilderUISystem>();

			highlightedQuery = GetEntityQuery(ComponentType.ReadOnly<Highlighted>());

			applyAction = InputManager.instance.FindAction("Tool", "Apply");
		}

		public override void InitializeRaycast()
		{
			base.InitializeRaycast();

			m_ToolRaycastSystem.netLayerMask = Layer.Road | Layer.TrainTrack | Layer.TramTrack | Layer.SubwayTrack;
			m_ToolRaycastSystem.typeMask = TypeMask.Net;
			m_ToolRaycastSystem.collisionMask = CollisionMask.Overground | CollisionMask.OnGround | CollisionMask.Underground;
		}

		protected override void OnStartRunning()
		{
			base.OnStartRunning();

			applyAction.shouldBeEnabled = roadBuilderUISystem.Mode is RoadBuilderToolMode.Picker;
		}

		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			if (roadBuilderUISystem.Mode is RoadBuilderToolMode.Picker)
			{
				HandlePicker();
			}

			return base.OnUpdate(inputDeps);
		}

		private void HandlePicker()
		{
			var raycastHit = GetRaycastResult(out var entity, out RaycastHit hit);
			var entities = highlightedQuery.ToEntityArray(Allocator.Temp);

			if (raycastHit)
			{
				if (applyAction.WasPressedThisFrame()
					&& EntityManager.TryGetComponent<PrefabRef>(entity, out var prefabRef)
					&& prefabSystem.TryGetPrefab<RoadPrefab>(prefabRef, out var prefab))
				{
					if (prefab is RoadBuilderPrefab)
					{
						roadBuilderUISystem.EditPrefab(entity);
					}
					else
					{
						roadBuilderUISystem.CreateNewPrefab(entity);
					}

					return;
				}
				else if (!EntityManager.HasComponent<Highlighted>(entity))
				{
					EntityManager.AddComponent<Highlighted>(entity);
					EntityManager.AddComponent<BatchesUpdated>(entity);
				}
			}

			for (var i = 0; i < entities.Length; i++)
			{
				if (raycastHit && entity == entities[i])
				{
					continue;
				}

				EntityManager.RemoveComponent<Highlighted>(entities[i]);
				EntityManager.AddComponent<BatchesUpdated>(entities[i]);
			}
		}

		protected override void OnStopRunning()
		{
			base.OnStopRunning();

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
