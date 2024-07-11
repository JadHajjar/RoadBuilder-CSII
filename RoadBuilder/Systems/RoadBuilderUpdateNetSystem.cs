using Colossal.Collections;
using Colossal.Mathematics;

using Game;
using Game.Areas;
using Game.Audio;
using Game.Buildings;
using Game.City;
using Game.Common;
using Game.Effects;
using Game.Net;
using Game.Objects;
using Game.Prefabs;
using Game.Simulation;
using Game.Tools;
using Game.Zones;

using RoadBuilder.Domain.Prefabs;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

using UnityEngine;

namespace RoadBuilder.Systems
{
	public partial class RoadBuilderToolSystem : ToolBaseSystem
	{
		public enum Mode
		{
			Straight,
			SimpleCurve,
			ComplexCurve,
			Continuous,
			Grid,
			Replace,
			Point
		}

		private struct PathEdge
		{
			public Entity m_Entity;

			public bool m_Invert;

			public bool m_Upgrade;
		}

		private struct UpgradeState
		{
			public bool m_IsUpgrading;

			public bool m_SkipFlags;

			public CompositionFlags m_OldFlags;

			public CompositionFlags m_AddFlags;

			public CompositionFlags m_RemoveFlags;
		}
		private struct AppliedUpgrade
		{
			public Entity m_Entity;

			public CompositionFlags m_Flags;
		}

		public struct PathItem : ILessThan<PathItem>
		{
			public Entity m_Node;

			public Entity m_Edge;

			public float m_Cost;

			public bool LessThan(PathItem other)
			{
				return m_Cost < other.m_Cost;
			}
		}

		private struct ControlPoint
		{
			internal Entity m_OriginalEntity;
			//internal float3 m_Position;
		}

		private ToolOutputBarrier m_ToolOutputBarrier;

		private TerrainSystem m_TerrainSystem;

		private WaterSystem m_WaterSystem;

		private Game.Net.SearchSystem m_NetSearchSystem;

		private Game.Objects.SearchSystem m_ObjectSearchSystem;

		private Game.Zones.SearchSystem m_ZoneSearchSystem;

		private CityConfigurationSystem m_CityConfigurationSystem;

		private PrefabSystem m_PrefabSystem;

		private AudioManager m_AudioManager;

		private NetInitializeSystem m_NetInitializeSystem;

		private PrefabBase m_Prefab;
		private EntityQuery m_DefinitionQuery;
		public readonly Queue<INetworkBuilderPrefab> _updatedRoadPrefabsQueue = new();

		private void OnCreate2()
		{
			m_ToolOutputBarrier = base.World.GetOrCreateSystemManaged<ToolOutputBarrier>();
			m_TerrainSystem = base.World.GetOrCreateSystemManaged<TerrainSystem>();
			m_WaterSystem = base.World.GetOrCreateSystemManaged<WaterSystem>();
			m_NetSearchSystem = base.World.GetOrCreateSystemManaged<Game.Net.SearchSystem>();
			m_ObjectSearchSystem = base.World.GetOrCreateSystemManaged<Game.Objects.SearchSystem>();
			m_ZoneSearchSystem = base.World.GetOrCreateSystemManaged<Game.Zones.SearchSystem>();
			m_CityConfigurationSystem = base.World.GetOrCreateSystemManaged<CityConfigurationSystem>();
			m_AudioManager = base.World.GetOrCreateSystemManaged<AudioManager>();
			m_NetInitializeSystem = base.World.GetOrCreateSystemManaged<NetInitializeSystem>();
			m_PrefabSystem = base.World.GetOrCreateSystemManaged<PrefabSystem>();

			m_DefinitionQuery = GetDefinitionQuery();

		}

		protected JobHandle UpdatePrefabs(JobHandle inputDeps)
		{
			//Dependency = SnapControlPoints(Dependency, removeUpgrade: false);
			//Dependency = FixControlPoints(Dependency);
			inputDeps = UpdateCourse(inputDeps, removeUpgrade: false);
			return inputDeps;
		}

		//private JobHandle SnapControlPoints(JobHandle inputDeps, bool removeUpgrade)
		//{
		//	SnapJob snapJob = default(SnapJob);
		//	snapJob.m_Mode = Mode.Replace;
		//	snapJob.m_Snap = Snap.None;
		//	snapJob.m_Elevation = 0f;
		//	snapJob.m_Prefab = m_PrefabSystem.GetEntity(m_Prefab);
		//	snapJob.m_LanePrefab = Entity.Null;
		//	snapJob.m_EditorMode = false;
		//	snapJob.m_RemoveUpgrade = removeUpgrade;
		//	snapJob.m_LeftHandTraffic = m_CityConfigurationSystem.leftHandTraffic;
		//	snapJob.m_TerrainHeightData = m_TerrainSystem.GetHeightData();
		//	snapJob.m_WaterSurfaceData = m_WaterSystem.GetSurfaceData(out var deps);
		//	snapJob.m_OwnerData = SystemAPI.GetComponentLookup<Owner>(true);
		//	snapJob.m_NodeData = SystemAPI.GetComponentLookup<Game.Net.Node>(true);
		//	snapJob.m_EdgeData = SystemAPI.GetComponentLookup<Edge>(true);
		//	snapJob.m_CurveData = SystemAPI.GetComponentLookup<Curve>(true);
		//	snapJob.m_RoadData = SystemAPI.GetComponentLookup<Road>(true);
		//	snapJob.m_UpgradedData = SystemAPI.GetComponentLookup<Upgraded>(true);
		//	snapJob.m_CompositionData = SystemAPI.GetComponentLookup<Composition>(true);
		//	snapJob.m_EdgeGeometryData = SystemAPI.GetComponentLookup<EdgeGeometry>(true);
		//	snapJob.m_TransformData = SystemAPI.GetComponentLookup<Game.Objects.Transform>(true);
		//	snapJob.m_ZoneBlockData = SystemAPI.GetComponentLookup<Block>(true);
		//	snapJob.m_PrefabRefData = SystemAPI.GetComponentLookup<PrefabRef>(true);
		//	snapJob.m_PrefabRoadData = SystemAPI.GetComponentLookup<RoadData>(true);
		//	snapJob.m_PrefabNetData = SystemAPI.GetComponentLookup<NetData>(true);
		//	snapJob.m_PrefabGeometryData = SystemAPI.GetComponentLookup<NetGeometryData>(true);
		//	snapJob.m_PrefabCompositionData = SystemAPI.GetComponentLookup<NetCompositionData>(true);
		//	snapJob.m_RoadCompositionData = SystemAPI.GetComponentLookup<RoadComposition>(true);
		//	snapJob.m_PlaceableData = SystemAPI.GetComponentLookup<PlaceableNetData>(true);
		//	snapJob.m_BuildingData = SystemAPI.GetComponentLookup<BuildingData>(true);
		//	snapJob.m_BuildingExtensionData = SystemAPI.GetComponentLookup<BuildingExtensionData>(true);
		//	snapJob.m_AssetStampData = SystemAPI.GetComponentLookup<AssetStampData>(true);
		//	snapJob.m_ObjectGeometryData = SystemAPI.GetComponentLookup<ObjectGeometryData>(true);
		//	snapJob.m_LocalConnectData = SystemAPI.GetComponentLookup<LocalConnectData>(true);
		//	snapJob.m_PrefabLaneData = SystemAPI.GetComponentLookup<NetLaneData>(true);
		//	snapJob.m_ConnectedEdges = SystemAPI.GetBufferLookup<ConnectedEdge>(true);
		//	snapJob.m_SubNets = SystemAPI.GetBufferLookup<Game.Net.SubNet>(true);
		//	snapJob.m_ZoneCells = SystemAPI.GetBufferLookup<Cell>(true);
		//	snapJob.m_PrefabCompositionAreas = SystemAPI.GetBufferLookup<NetCompositionArea>(true);
		//	snapJob.m_SubObjects = SystemAPI.GetBufferLookup<Game.Prefabs.SubObject>(true);
		//	snapJob.m_NetSearchTree = m_NetSearchSystem.GetNetSearchTree(readOnly: true, out var dependencies);
		//	snapJob.m_ObjectSearchTree = m_ObjectSearchSystem.GetStaticSearchTree(readOnly: true, out var dependencies2);
		//	snapJob.m_ZoneSearchTree = m_ZoneSearchSystem.GetSearchTree(readOnly: true, out var dependencies3);
		//	snapJob.m_ControlPoints = m_ControlPoints;
		//	snapJob.m_SnapLines = m_SnapLines;
		//	snapJob.m_UpgradeStates = m_UpgradeStates;
		//	snapJob.m_StartEntity = m_StartEntity;
		//	snapJob.m_AppliedUpgrade = m_AppliedUpgrade;
		//	snapJob.m_LastSnappedEntity = m_LastSnappedEntity;
		//	snapJob.m_SourceUpdateData = m_AudioManager.GetSourceUpdateData(out var deps2);
		//	SnapJob jobData = snapJob;
		//	inputDeps = JobHandle.CombineDependencies(inputDeps, dependencies, dependencies2);
		//	inputDeps = JobHandle.CombineDependencies(inputDeps, dependencies3, deps);
		//	inputDeps = JobHandle.CombineDependencies(inputDeps, deps2);
		//	JobHandle jobHandle = IJobExtensions.Schedule(jobData, inputDeps);
		//	m_TerrainSystem.AddCPUHeightReader(jobHandle);
		//	m_WaterSystem.AddSurfaceReader(jobHandle);
		//	m_NetSearchSystem.AddNetSearchTreeReader(jobHandle);
		//	m_ObjectSearchSystem.AddStaticSearchTreeReader(jobHandle);
		//	return jobHandle;
		//}

		//private JobHandle FixControlPoints(JobHandle inputDeps)
		//{
		//	JobHandle outJobHandle;
		//	NativeList<ArchetypeChunk> chunks = m_TempQuery.ToArchetypeChunkListAsync(Allocator.TempJob, out outJobHandle);
		//	FixControlPointsJob jobData = default(FixControlPointsJob);
		//	jobData.m_Chunks = chunks;
		//	jobData.m_Mode = mode;
		//	jobData.m_EntityType = SystemAPI.GetEntityTypeHandle();
		//	jobData.m_TempType = SystemAPI.GetComponentTypeHandle<Temp>();
		//	jobData.m_ControlPoints = m_ControlPoints;
		//	JobHandle jobHandle = IJobExtensions.Schedule(jobData, JobHandle.CombineDependencies(inputDeps, outJobHandle));
		//	chunks.Dispose(jobHandle);
		//	return jobHandle;
		//}

		private JobHandle UpdateCourse(JobHandle inputDeps, bool removeUpgrade)
		{
			JobHandle jobHandle = DestroyDefinitions(m_DefinitionQuery, m_ToolOutputBarrier, inputDeps);

			if (m_Prefab != null)
			{
				CreateDefinitionsJob createDefinitionsJob = default(CreateDefinitionsJob);
				createDefinitionsJob.m_EditorMode = m_ToolSystem.actionMode.IsEditor();
				createDefinitionsJob.m_RemoveUpgrade = removeUpgrade;
				createDefinitionsJob.m_LefthandTraffic = m_CityConfigurationSystem.leftHandTraffic;
				createDefinitionsJob.m_Mode = Mode.Replace;
				createDefinitionsJob.m_ParallelCount = default;
				createDefinitionsJob.m_ParallelOffset = 0;
				createDefinitionsJob.m_RandomSeed = RandomSeed.Next();
				createDefinitionsJob.m_ControlPoints = m_ControlPoints;
				createDefinitionsJob.m_UpgradeStates = m_UpgradeStates;
				createDefinitionsJob.m_EdgeData = SystemAPI.GetComponentLookup<Edge>(true);
				createDefinitionsJob.m_NodeData = SystemAPI.GetComponentLookup<Game.Net.Node>(true);
				createDefinitionsJob.m_CurveData = SystemAPI.GetComponentLookup<Curve>(true);
				createDefinitionsJob.m_EditorContainerData = SystemAPI.GetComponentLookup<Game.Tools.EditorContainer>(true);
				createDefinitionsJob.m_OwnerData = SystemAPI.GetComponentLookup<Owner>(true);
				createDefinitionsJob.m_TempData = SystemAPI.GetComponentLookup<Temp>(true);
				createDefinitionsJob.m_LocalTransformCacheData = SystemAPI.GetComponentLookup<LocalTransformCache>(true);
				createDefinitionsJob.m_TransformData = SystemAPI.GetComponentLookup<Game.Objects.Transform>(true);
				createDefinitionsJob.m_BuildingData = SystemAPI.GetComponentLookup<Game.Buildings.Building>(true);
				createDefinitionsJob.m_ExtensionData = SystemAPI.GetComponentLookup<Game.Buildings.Extension>(true);
				createDefinitionsJob.m_PrefabRefData = SystemAPI.GetComponentLookup<PrefabRef>(true);
				createDefinitionsJob.m_NetGeometryData = SystemAPI.GetComponentLookup<NetGeometryData>(true);
				createDefinitionsJob.m_PlaceableData = SystemAPI.GetComponentLookup<PlaceableNetData>(true);
				createDefinitionsJob.m_PrefabSpawnableObjectData = SystemAPI.GetComponentLookup<SpawnableObjectData>(true);
				createDefinitionsJob.m_PrefabAreaGeometryData = SystemAPI.GetComponentLookup<AreaGeometryData>(true);
				createDefinitionsJob.m_ConnectedEdges = SystemAPI.GetBufferLookup<ConnectedEdge>(true);
				createDefinitionsJob.m_SubNets = SystemAPI.GetBufferLookup<Game.Net.SubNet>(true);
				createDefinitionsJob.m_CachedNodes = SystemAPI.GetBufferLookup<LocalNodeCache>(true);
				createDefinitionsJob.m_SubAreas = SystemAPI.GetBufferLookup<Game.Areas.SubArea>(true);
				createDefinitionsJob.m_AreaNodes = SystemAPI.GetBufferLookup<Game.Areas.Node>(true);
				createDefinitionsJob.m_InstalledUpgrades = SystemAPI.GetBufferLookup<Game.Buildings.InstalledUpgrade>(true);
				createDefinitionsJob.m_PrefabSubObjects = SystemAPI.GetBufferLookup<Game.Prefabs.SubObject>(true);
				createDefinitionsJob.m_PrefabSubNets = SystemAPI.GetBufferLookup<Game.Prefabs.SubNet>(true);
				createDefinitionsJob.m_PrefabSubAreas = SystemAPI.GetBufferLookup<Game.Prefabs.SubArea>(true);
				createDefinitionsJob.m_PrefabSubAreaNodes = SystemAPI.GetBufferLookup<SubAreaNode>(true);
				createDefinitionsJob.m_PrefabPlaceholderElements = SystemAPI.GetBufferLookup<PlaceholderObjectElement>(true);
				createDefinitionsJob.m_NetPrefab = m_PrefabSystem.GetEntity(m_Prefab);
				createDefinitionsJob.m_WaterSurfaceData = m_WaterSystem.GetVelocitiesSurfaceData(out var deps);
				createDefinitionsJob.m_CommandBuffer = m_ToolOutputBarrier.CreateCommandBuffer();
				CreateDefinitionsJob jobData = createDefinitionsJob;
				JobHandle jobHandle2 = IJobExtensions.Schedule(jobData, JobHandle.CombineDependencies(inputDeps, deps));
				m_WaterSystem.AddVelocitySurfaceReader(jobHandle2);
				m_ToolOutputBarrier.AddJobHandleForProducer(jobHandle2);
				jobHandle = JobHandle.CombineDependencies(jobHandle, jobHandle2);
			}
			return jobHandle;
		}


		//[BurstCompile]
		//private struct SnapJob : IJob
		//{
		//	private struct ParentObjectIterator : INativeQuadTreeIterator<Entity, QuadTreeBoundsXZ>, IUnsafeQuadTreeIterator<Entity, QuadTreeBoundsXZ>
		//	{
		//		public ControlPoint m_BestSnapPosition;

		//		public Line3.Segment m_Line;

		//		public Bounds3 m_Bounds;

		//		public float m_Radius;

		//		public ComponentLookup<Owner> m_OwnerData;

		//		public ComponentLookup<Game.Objects.Transform> m_TransformData;

		//		public ComponentLookup<PrefabRef> m_PrefabRefData;

		//		public ComponentLookup<BuildingData> m_BuildingData;

		//		public ComponentLookup<BuildingExtensionData> m_BuildingExtensionData;

		//		public ComponentLookup<AssetStampData> m_AssetStampData;

		//		public ComponentLookup<ObjectGeometryData> m_PrefabObjectGeometryData;

		//		public bool Intersect(QuadTreeBoundsXZ bounds)
		//		{
		//			return MathUtils.Intersect(bounds.m_Bounds.xz, m_Bounds.xz);
		//		}

		//		public void Iterate(QuadTreeBoundsXZ bounds, Entity item)
		//		{
		//			if (!MathUtils.Intersect(bounds.m_Bounds.xz, m_Bounds.xz) || m_OwnerData.HasComponent(item))
		//			{
		//				return;
		//			}
		//			PrefabRef prefabRef = m_PrefabRefData[item];
		//			if (!m_BuildingData.HasComponent(prefabRef.m_Prefab) && !m_BuildingExtensionData.HasComponent(prefabRef.m_Prefab) && !m_AssetStampData.HasComponent(prefabRef.m_Prefab))
		//			{
		//				return;
		//			}
		//			Game.Objects.Transform transform = m_TransformData[item];
		//			ObjectGeometryData objectGeometryData = m_PrefabObjectGeometryData[prefabRef.m_Prefab];
		//			float3 @float = MathUtils.Center(bounds.m_Bounds);
		//			Line3.Segment segment = m_Line - @float;
		//			int2 @int = default(int2);
		//			@int.x = ZoneUtils.GetCellWidth(objectGeometryData.m_Size.x);
		//			@int.y = ZoneUtils.GetCellWidth(objectGeometryData.m_Size.z);
		//			float2 size = (float2)@int * 8f;
		//			if ((objectGeometryData.m_Flags & Game.Objects.GeometryFlags.Circular) != 0)
		//			{
		//				Circle2 circle = new Circle2(size.x * 0.5f, (transform.m_Position - @float).xz);
		//				if (MathUtils.Intersect(circle, new Circle2(m_Radius, segment.a.xz)))
		//				{
		//					m_BestSnapPosition.m_OriginalEntity = item;
		//					m_BestSnapPosition.m_ElementIndex = new int2(-1, -1);
		//					return;
		//				}
		//				if (MathUtils.Intersect(circle, new Circle2(m_Radius, segment.b.xz)))
		//				{
		//					m_BestSnapPosition.m_OriginalEntity = item;
		//					m_BestSnapPosition.m_ElementIndex = new int2(-1, -1);
		//					return;
		//				}
		//				float num = MathUtils.Length(segment.xz);
		//				if (num > m_Radius)
		//				{
		//					float2 float2 = MathUtils.Right((segment.b.xz - segment.a.xz) * (m_Radius / num));
		//					if (MathUtils.Intersect(new Quad2(segment.a.xz + float2, segment.b.xz + float2, segment.b.xz - float2, segment.a.xz - float2), circle))
		//					{
		//						m_BestSnapPosition.m_OriginalEntity = item;
		//						m_BestSnapPosition.m_ElementIndex = new int2(-1, -1);
		//					}
		//				}
		//				return;
		//			}
		//			Quad2 xz = ObjectUtils.CalculateBaseCorners(transform.m_Position - @float, transform.m_Rotation, size).xz;
		//			if (MathUtils.Intersect(xz, new Circle2(m_Radius, segment.a.xz)))
		//			{
		//				m_BestSnapPosition.m_OriginalEntity = item;
		//				m_BestSnapPosition.m_ElementIndex = new int2(-1, -1);
		//				return;
		//			}
		//			if (MathUtils.Intersect(xz, new Circle2(m_Radius, segment.b.xz)))
		//			{
		//				m_BestSnapPosition.m_OriginalEntity = item;
		//				m_BestSnapPosition.m_ElementIndex = new int2(-1, -1);
		//				return;
		//			}
		//			float num2 = MathUtils.Length(segment.xz);
		//			if (num2 > m_Radius)
		//			{
		//				float2 float3 = MathUtils.Right((segment.b.xz - segment.a.xz) * (m_Radius / num2));
		//				Quad2 quad = new Quad2(segment.a.xz + float3, segment.b.xz + float3, segment.b.xz - float3, segment.a.xz - float3);
		//				if (MathUtils.Intersect(xz, quad))
		//				{
		//					m_BestSnapPosition.m_OriginalEntity = item;
		//					m_BestSnapPosition.m_ElementIndex = new int2(-1, -1);
		//				}
		//			}
		//		}
		//	}

		//	private struct LotIterator : INativeQuadTreeIterator<Entity, QuadTreeBoundsXZ>, IUnsafeQuadTreeIterator<Entity, QuadTreeBoundsXZ>
		//	{
		//		public Bounds2 m_Bounds;

		//		public float m_Radius;

		//		public float m_EdgeOffset;

		//		public float m_MaxDistance;

		//		public int m_CellWidth;

		//		public ControlPoint m_ControlPoint;

		//		public ControlPoint m_BestSnapPosition;

		//		public NativeList<SnapLine> m_SnapLines;

		//		public ComponentLookup<Owner> m_OwnerData;

		//		public ComponentLookup<Edge> m_EdgeData;

		//		public ComponentLookup<Game.Net.Node> m_NodeData;

		//		public ComponentLookup<Game.Objects.Transform> m_TransformData;

		//		public ComponentLookup<PrefabRef> m_PrefabRefData;

		//		public ComponentLookup<BuildingData> m_BuildingData;

		//		public ComponentLookup<BuildingExtensionData> m_BuildingExtensionData;

		//		public ComponentLookup<AssetStampData> m_AssetStampData;

		//		public ComponentLookup<ObjectGeometryData> m_PrefabObjectGeometryData;

		//		public bool Intersect(QuadTreeBoundsXZ bounds)
		//		{
		//			return MathUtils.Intersect(bounds.m_Bounds.xz, m_Bounds);
		//		}

		//		public void Iterate(QuadTreeBoundsXZ bounds, Entity item)
		//		{
		//			if (!MathUtils.Intersect(bounds.m_Bounds.xz, m_Bounds) || m_OwnerData.HasComponent(item))
		//			{
		//				return;
		//			}
		//			PrefabRef prefabRef = m_PrefabRefData[item];
		//			if (!m_BuildingData.HasComponent(prefabRef.m_Prefab) && !m_BuildingExtensionData.HasComponent(prefabRef.m_Prefab) && !m_AssetStampData.HasComponent(prefabRef.m_Prefab))
		//			{
		//				return;
		//			}
		//			Game.Objects.Transform transform = m_TransformData[item];
		//			ObjectGeometryData objectGeometryData = m_PrefabObjectGeometryData[prefabRef.m_Prefab];
		//			float2 @float = math.normalizesafe(math.forward(transform.m_Rotation).xz, new float2(0f, 1f));
		//			float2 float2 = MathUtils.Right(@float);
		//			float2 x = m_ControlPoint.m_HitPosition.xz - transform.m_Position.xz;
		//			int2 @int = default(int2);
		//			@int.x = ZoneUtils.GetCellWidth(objectGeometryData.m_Size.x);
		//			@int.y = ZoneUtils.GetCellWidth(objectGeometryData.m_Size.z);
		//			float2 float3 = (float2)@int * 8f;
		//			float2 offset = math.select(0f, 4f, ((m_CellWidth + @int) & 1) != 0);
		//			float2 float4 = new float2(math.dot(x, float2), math.dot(x, @float));
		//			float2 float5 = MathUtils.Snap(float4, 8f, offset);
		//			if (m_EdgeOffset != 0f && (objectGeometryData.m_Flags & Game.Objects.GeometryFlags.Circular) == 0)
		//			{
		//				float5 = math.select(float5, float5 + math.select(m_EdgeOffset, 0f - m_EdgeOffset, float5 > 0f), math.abs(math.abs(float5) - float3 * 0.5f) < 4f);
		//			}
		//			bool2 @bool = math.abs(float4 - float5) < m_MaxDistance;
		//			if (!math.any(@bool))
		//			{
		//				return;
		//			}
		//			float5 = math.select(float4, float5, @bool);
		//			float2 float6 = transform.m_Position.xz + float2 * float5.x + @float * float5.y;
		//			if ((objectGeometryData.m_Flags & Game.Objects.GeometryFlags.Circular) != 0)
		//			{
		//				if (math.distance(float6, transform.m_Position.xz) > float3.x * 0.5f + m_Radius + 4f)
		//				{
		//					return;
		//				}
		//			}
		//			else if (math.any(math.abs(float5) > float3 * 0.5f + m_Radius + 4f))
		//			{
		//				return;
		//			}
		//			ControlPoint controlPoint = m_ControlPoint;
		//			if (!m_EdgeData.HasComponent(m_ControlPoint.m_OriginalEntity) && !m_NodeData.HasComponent(m_ControlPoint.m_OriginalEntity))
		//			{
		//				controlPoint.m_OriginalEntity = Entity.Null;
		//			}
		//			controlPoint.m_Direction = float2;
		//			controlPoint.m_Position.xz = float6;
		//			if (m_ControlPoint.m_OriginalEntity != item || m_ControlPoint.m_ElementIndex.x != -1)
		//			{
		//				controlPoint.m_Position.y = m_ControlPoint.m_HitPosition.y;
		//			}
		//			controlPoint.m_SnapPriority = ToolUtils.CalculateSnapPriority(0f, 1f, m_ControlPoint.m_HitPosition.xz, controlPoint.m_Position.xz, controlPoint.m_Direction);
		//			Line3 line = new Line3(controlPoint.m_Position, controlPoint.m_Position);
		//			Line3 line2 = new Line3(controlPoint.m_Position, controlPoint.m_Position);
		//			line.a.xz -= controlPoint.m_Direction * 8f;
		//			line.b.xz += controlPoint.m_Direction * 8f;
		//			line2.a.xz -= MathUtils.Right(controlPoint.m_Direction) * 8f;
		//			line2.b.xz += MathUtils.Right(controlPoint.m_Direction) * 8f;
		//			ToolUtils.AddSnapPosition(ref m_BestSnapPosition, controlPoint);
		//			if (@bool.y)
		//			{
		//				ToolUtils.AddSnapLine(ref m_BestSnapPosition, m_SnapLines, new SnapLine(controlPoint, NetUtils.StraightCurve(line.a, line.b), SnapLineFlags.Hidden));
		//			}
		//			controlPoint.m_Direction = MathUtils.Right(controlPoint.m_Direction);
		//			if (@bool.x)
		//			{
		//				ToolUtils.AddSnapLine(ref m_BestSnapPosition, m_SnapLines, new SnapLine(controlPoint, NetUtils.StraightCurve(line2.a, line2.b), SnapLineFlags.Hidden));
		//			}
		//		}
		//	}

		//	private struct ZoneIterator : INativeQuadTreeIterator<Entity, Bounds2>, IUnsafeQuadTreeIterator<Entity, Bounds2>
		//	{
		//		public Bounds2 m_Bounds;

		//		public float2 m_HitPosition;

		//		public float3 m_BestPosition;

		//		public float2 m_BestDirection;

		//		public float m_BestDistance;

		//		public ComponentLookup<Block> m_ZoneBlockData;

		//		public BufferLookup<Cell> m_ZoneCells;

		//		public bool Intersect(Bounds2 bounds)
		//		{
		//			return MathUtils.Intersect(bounds, m_Bounds);
		//		}

		//		public void Iterate(Bounds2 bounds, Entity entity)
		//		{
		//			if (!MathUtils.Intersect(bounds, m_Bounds))
		//			{
		//				return;
		//			}
		//			Block block = m_ZoneBlockData[entity];
		//			DynamicBuffer<Cell> dynamicBuffer = m_ZoneCells[entity];
		//			int2 cellIndex = math.clamp(ZoneUtils.GetCellIndex(block, m_HitPosition), 0, block.m_Size - 1);
		//			float3 cellPosition = ZoneUtils.GetCellPosition(block, cellIndex);
		//			float num = math.distance(cellPosition.xz, m_HitPosition);
		//			if (num >= m_BestDistance)
		//			{
		//				return;
		//			}
		//			if ((dynamicBuffer[cellIndex.x + cellIndex.y * block.m_Size.x].m_State & CellFlags.Visible) != 0)
		//			{
		//				m_BestPosition = cellPosition;
		//				m_BestDirection = block.m_Direction;
		//				m_BestDistance = num;
		//				return;
		//			}
		//			cellIndex.y = 0;
		//			while (cellIndex.y < block.m_Size.y)
		//			{
		//				cellIndex.x = 0;
		//				while (cellIndex.x < block.m_Size.x)
		//				{
		//					if ((dynamicBuffer[cellIndex.x + cellIndex.y * block.m_Size.x].m_State & CellFlags.Visible) != 0)
		//					{
		//						cellPosition = ZoneUtils.GetCellPosition(block, cellIndex);
		//						num = math.distance(cellPosition.xz, m_HitPosition);
		//						if (num < m_BestDistance)
		//						{
		//							m_BestPosition = cellPosition;
		//							m_BestDirection = block.m_Direction;
		//							m_BestDistance = num;
		//						}
		//					}
		//					cellIndex.x++;
		//				}
		//				cellIndex.y++;
		//			}
		//		}
		//	}

		//	private struct ObjectIterator : INativeQuadTreeIterator<Entity, QuadTreeBoundsXZ>, IUnsafeQuadTreeIterator<Entity, QuadTreeBoundsXZ>
		//	{
		//		public Bounds3 m_Bounds;

		//		public Snap m_Snap;

		//		public float m_MaxDistance;

		//		public float m_NetSnapOffset;

		//		public float m_ObjectSnapOffset;

		//		public bool m_SnapCellLength;

		//		public NetData m_NetData;

		//		public NetGeometryData m_NetGeometryData;

		//		public ControlPoint m_ControlPoint;

		//		public ControlPoint m_BestSnapPosition;

		//		public NativeList<SnapLine> m_SnapLines;

		//		public ComponentLookup<Owner> m_OwnerData;

		//		public ComponentLookup<Curve> m_CurveData;

		//		public ComponentLookup<Game.Net.Node> m_NodeData;

		//		public ComponentLookup<Game.Objects.Transform> m_TransformData;

		//		public ComponentLookup<PrefabRef> m_PrefabRefData;

		//		public ComponentLookup<BuildingData> m_BuildingData;

		//		public ComponentLookup<ObjectGeometryData> m_ObjectGeometryData;

		//		public ComponentLookup<NetData> m_PrefabNetData;

		//		public ComponentLookup<NetGeometryData> m_PrefabGeometryData;

		//		public BufferLookup<ConnectedEdge> m_ConnectedEdges;

		//		public bool Intersect(QuadTreeBoundsXZ bounds)
		//		{
		//			return MathUtils.Intersect(bounds.m_Bounds, m_Bounds);
		//		}

		//		public void Iterate(QuadTreeBoundsXZ bounds, Entity entity)
		//		{
		//			if (!MathUtils.Intersect(bounds.m_Bounds, m_Bounds))
		//			{
		//				return;
		//			}
		//			if ((m_Snap & (Snap.ExistingGeometry | Snap.NearbyGeometry)) != 0 && m_OwnerData.HasComponent(entity))
		//			{
		//				Owner owner = m_OwnerData[entity];
		//				if (m_NodeData.HasComponent(owner.m_Owner))
		//				{
		//					SnapToNode(owner.m_Owner);
		//				}
		//			}
		//			if ((m_Snap & Snap.ObjectSide) != 0)
		//			{
		//				SnapObjectSide(entity);
		//			}
		//		}

		//		private void SnapToNode(Entity entity)
		//		{
		//			if ((entity == m_ControlPoint.m_OriginalEntity && (m_Snap & Snap.ExistingGeometry) != 0) || (m_ConnectedEdges.HasBuffer(entity) && m_ConnectedEdges[entity].Length > 0))
		//			{
		//				return;
		//			}
		//			Game.Net.Node node = m_NodeData[entity];
		//			PrefabRef prefabRef = m_PrefabRefData[entity];
		//			if (!m_PrefabNetData.HasComponent(prefabRef.m_Prefab) || (m_PrefabNetData[prefabRef.m_Prefab].m_ConnectLayers & m_NetData.m_ConnectLayers) == 0)
		//			{
		//				return;
		//			}
		//			ControlPoint controlPoint = m_ControlPoint;
		//			controlPoint.m_OriginalEntity = entity;
		//			controlPoint.m_Position = node.m_Position;
		//			controlPoint.m_Direction = math.mul(node.m_Rotation, new float3(0f, 0f, 1f)).xz;
		//			MathUtils.TryNormalize(ref controlPoint.m_Direction);
		//			float level = 1f;
		//			float num = math.distance(node.m_Position.xz, m_ControlPoint.m_HitPosition.xz);
		//			float num2 = m_NetGeometryData.m_DefaultWidth * 0.5f;
		//			if (m_PrefabGeometryData.TryGetComponent(prefabRef.m_Prefab, out var componentData))
		//			{
		//				num2 += componentData.m_DefaultWidth * 0.5f;
		//			}
		//			if (!(num >= num2 + m_NetSnapOffset))
		//			{
		//				if ((m_NetGeometryData.m_Flags & Game.Net.GeometryFlags.StrictNodes) != 0 && num <= num2 && num <= num2)
		//				{
		//					level = 2f;
		//				}
		//				controlPoint.m_SnapPriority = ToolUtils.CalculateSnapPriority(level, 1f, m_ControlPoint.m_HitPosition.xz, controlPoint.m_Position.xz, controlPoint.m_Direction);
		//				ToolUtils.AddSnapPosition(ref m_BestSnapPosition, controlPoint);
		//			}
		//		}

		//		private void SnapObjectSide(Entity entity)
		//		{
		//			if (!m_TransformData.HasComponent(entity))
		//			{
		//				return;
		//			}
		//			Game.Objects.Transform transform = m_TransformData[entity];
		//			PrefabRef prefabRef = m_PrefabRefData[entity];
		//			if (!m_ObjectGeometryData.HasComponent(prefabRef.m_Prefab))
		//			{
		//				return;
		//			}
		//			ObjectGeometryData objectGeometryData = m_ObjectGeometryData[prefabRef.m_Prefab];
		//			if ((objectGeometryData.m_Flags & Game.Objects.GeometryFlags.Circular) == 0)
		//			{
		//				bool flag = false;
		//				if (m_BuildingData.HasComponent(prefabRef.m_Prefab))
		//				{
		//					float2 @float = m_BuildingData[prefabRef.m_Prefab].m_LotSize;
		//					objectGeometryData.m_Bounds.min.xz = @float * -4f;
		//					objectGeometryData.m_Bounds.max.xz = @float * 4f;
		//					flag = m_SnapCellLength;
		//					objectGeometryData.m_Bounds.min.xz -= m_ObjectSnapOffset;
		//					objectGeometryData.m_Bounds.max.xz += m_ObjectSnapOffset;
		//					Quad3 quad = ObjectUtils.CalculateBaseCorners(transform.m_Position, transform.m_Rotation, objectGeometryData.m_Bounds);
		//					CheckLine(quad.ab, flag);
		//					CheckLine(quad.bc, flag);
		//					CheckLine(quad.cd, flag);
		//					CheckLine(quad.da, flag);
		//				}
		//			}
		//		}

		//		private void CheckLine(Line3.Segment line, bool snapCellLength)
		//		{
		//			if (MathUtils.Distance(line.xz, m_ControlPoint.m_HitPosition.xz, out var t) < m_MaxDistance)
		//			{
		//				if (snapCellLength)
		//				{
		//					t = MathUtils.Snap(t, 8f / MathUtils.Length(line.xz));
		//				}
		//				ControlPoint controlPoint = m_ControlPoint;
		//				controlPoint.m_Direction = math.normalizesafe(MathUtils.Tangent(line.xz));
		//				controlPoint.m_Position = MathUtils.Position(line, t);
		//				controlPoint.m_SnapPriority = ToolUtils.CalculateSnapPriority(1f, 1f, m_ControlPoint.m_HitPosition.xz, controlPoint.m_Position.xz, controlPoint.m_Direction);
		//				if (m_CurveData.HasComponent(m_ControlPoint.m_OriginalEntity))
		//				{
		//					MathUtils.Distance(m_CurveData[m_ControlPoint.m_OriginalEntity].m_Bezier.xz, controlPoint.m_Position.xz, out controlPoint.m_CurvePosition);
		//				}
		//				else if (!m_NodeData.HasComponent(m_ControlPoint.m_OriginalEntity))
		//				{
		//					controlPoint.m_OriginalEntity = Entity.Null;
		//					controlPoint.m_ElementIndex = -1;
		//				}
		//				ToolUtils.AddSnapPosition(ref m_BestSnapPosition, controlPoint);
		//				ToolUtils.AddSnapLine(ref m_BestSnapPosition, m_SnapLines, new SnapLine(controlPoint, NetUtils.StraightCurve(line.a, line.b), SnapLineFlags.Secondary));
		//			}
		//		}
		//	}

		//	private struct NetIterator : INativeQuadTreeIterator<Entity, QuadTreeBoundsXZ>, IUnsafeQuadTreeIterator<Entity, QuadTreeBoundsXZ>
		//	{
		//		public bool m_EditorMode;

		//		public Bounds3 m_TotalBounds;

		//		public Bounds3 m_Bounds;

		//		public Snap m_Snap;

		//		public float m_SnapOffset;

		//		public float m_SnapDistance;

		//		public float m_Elevation;

		//		public float m_GuideLength;

		//		public float m_LegSnapWidth;

		//		public Bounds1 m_HeightRange;

		//		public NetData m_NetData;

		//		public RoadData m_PrefabRoadData;

		//		public NetGeometryData m_NetGeometryData;

		//		public LocalConnectData m_LocalConnectData;

		//		public ControlPoint m_ControlPoint;

		//		public ControlPoint m_BestSnapPosition;

		//		public NativeList<SnapLine> m_SnapLines;

		//		public TerrainHeightData m_TerrainHeightData;

		//		public WaterSurfaceData m_WaterSurfaceData;

		//		public ComponentLookup<Owner> m_OwnerData;

		//		public ComponentLookup<Game.Net.Node> m_NodeData;

		//		public ComponentLookup<Edge> m_EdgeData;

		//		public ComponentLookup<Curve> m_CurveData;

		//		public ComponentLookup<Composition> m_CompositionData;

		//		public ComponentLookup<EdgeGeometry> m_EdgeGeometryData;

		//		public ComponentLookup<Road> m_RoadData;

		//		public ComponentLookup<PrefabRef> m_PrefabRefData;

		//		public ComponentLookup<NetData> m_PrefabNetData;

		//		public ComponentLookup<NetGeometryData> m_PrefabGeometryData;

		//		public ComponentLookup<NetCompositionData> m_PrefabCompositionData;

		//		public ComponentLookup<RoadComposition> m_RoadCompositionData;

		//		public BufferLookup<ConnectedEdge> m_ConnectedEdges;

		//		public BufferLookup<NetCompositionArea> m_PrefabCompositionAreas;

		//		public bool Intersect(QuadTreeBoundsXZ bounds)
		//		{
		//			return MathUtils.Intersect(bounds.m_Bounds, m_TotalBounds);
		//		}

		//		public void Iterate(QuadTreeBoundsXZ bounds, Entity entity)
		//		{
		//			if (MathUtils.Intersect(bounds.m_Bounds, m_TotalBounds) && (!(entity == m_ControlPoint.m_OriginalEntity) || (m_Snap & Snap.ExistingGeometry) == 0) && (!MathUtils.Intersect(bounds.m_Bounds, m_Bounds) || (m_Snap & (Snap.ExistingGeometry | Snap.NearbyGeometry)) == 0 || !HandleGeometry(entity)) && (m_Snap & Snap.GuideLines) != 0)
		//			{
		//				HandleGuideLines(entity);
		//			}
		//		}

		//		public void HandleGuideLines(Entity entity)
		//		{
		//			if (!m_CurveData.HasComponent(entity))
		//			{
		//				return;
		//			}
		//			bool flag = false;
		//			bool flag2 = (m_NetGeometryData.m_Flags & Game.Net.GeometryFlags.StrictNodes) == 0 && (m_PrefabRoadData.m_Flags & Game.Prefabs.RoadFlags.EnableZoning) != 0 && (m_Snap & Snap.CellLength) != 0;
		//			float defaultWidth = m_NetGeometryData.m_DefaultWidth;
		//			float num = defaultWidth;
		//			float num2 = m_NetGeometryData.m_DefaultWidth * 0.5f;
		//			bool flag3 = false;
		//			bool flag4 = false;
		//			PrefabRef prefabRef = m_PrefabRefData[entity];
		//			NetData netData = m_PrefabNetData[prefabRef.m_Prefab];
		//			NetGeometryData netGeometryData = default(NetGeometryData);
		//			if (m_PrefabGeometryData.HasComponent(prefabRef.m_Prefab))
		//			{
		//				netGeometryData = m_PrefabGeometryData[prefabRef.m_Prefab];
		//			}
		//			if (!NetUtils.CanConnect(m_NetData, netData) || (!m_EditorMode && (netGeometryData.m_Flags & Game.Net.GeometryFlags.Marker) != 0))
		//			{
		//				return;
		//			}
		//			if (m_CompositionData.HasComponent(entity))
		//			{
		//				Composition composition = m_CompositionData[entity];
		//				num2 += m_PrefabCompositionData[composition.m_Edge].m_Width * 0.5f;
		//				if ((m_NetGeometryData.m_Flags & Game.Net.GeometryFlags.StrictNodes) == 0)
		//				{
		//					num = netGeometryData.m_DefaultWidth;
		//					if (m_RoadCompositionData.HasComponent(composition.m_Edge))
		//					{
		//						flag = (m_RoadCompositionData[composition.m_Edge].m_Flags & Game.Prefabs.RoadFlags.EnableZoning) != 0 && (m_Snap & Snap.CellLength) != 0;
		//						if (flag && m_RoadData.HasComponent(entity))
		//						{
		//							Road road = m_RoadData[entity];
		//							flag3 = (road.m_Flags & Game.Net.RoadFlags.StartHalfAligned) != 0;
		//							flag4 = (road.m_Flags & Game.Net.RoadFlags.EndHalfAligned) != 0;
		//						}
		//					}
		//				}
		//			}
		//			int cellWidth = ZoneUtils.GetCellWidth(defaultWidth);
		//			int cellWidth2 = ZoneUtils.GetCellWidth(num);
		//			int num3;
		//			float num4;
		//			float num5;
		//			if (flag2)
		//			{
		//				num3 = 1 + math.abs(cellWidth2 - cellWidth);
		//				num4 = (float)(num3 - 1) * -4f;
		//				num5 = 8f;
		//			}
		//			else
		//			{
		//				float num6 = math.abs(num - defaultWidth);
		//				if (num6 > 1.6f)
		//				{
		//					num3 = 3;
		//					num4 = num6 * -0.5f;
		//					num5 = num6 * 0.5f;
		//				}
		//				else
		//				{
		//					num3 = 1;
		//					num4 = 0f;
		//					num5 = 0f;
		//				}
		//			}
		//			float num7;
		//			float num8;
		//			float num9;
		//			float num10;
		//			if (flag)
		//			{
		//				num7 = math.select(0f, 4f, (((cellWidth ^ cellWidth2) & 1) != 0) ^ flag3);
		//				num8 = math.select(0f, 4f, (((cellWidth ^ cellWidth2) & 1) != 0) ^ flag4);
		//				num9 = math.select(num7, 0f - num7, cellWidth > cellWidth2);
		//				num10 = math.select(num8, 0f - num8, cellWidth > cellWidth2);
		//				num9 += 8f * (float)((math.max(2, cellWidth2) - math.max(2, cellWidth)) / 2);
		//				num10 += 8f * (float)((math.max(2, cellWidth2) - math.max(2, cellWidth)) / 2);
		//			}
		//			else
		//			{
		//				num7 = 0f;
		//				num8 = 0f;
		//				num9 = 0f;
		//				num10 = 0f;
		//			}
		//			Curve curve = m_CurveData[entity];
		//			Edge edge = m_EdgeData[entity];
		//			float2 value = -MathUtils.StartTangent(curve.m_Bezier).xz;
		//			float2 value2 = MathUtils.EndTangent(curve.m_Bezier).xz;
		//			bool flag5 = MathUtils.TryNormalize(ref value);
		//			bool flag6 = MathUtils.TryNormalize(ref value2);
		//			bool flag7 = flag5;
		//			if (flag5)
		//			{
		//				DynamicBuffer<ConnectedEdge> dynamicBuffer = m_ConnectedEdges[edge.m_Start];
		//				for (int i = 0; i < dynamicBuffer.Length; i++)
		//				{
		//					Entity edge2 = dynamicBuffer[i].m_Edge;
		//					if (!(edge2 == entity))
		//					{
		//						Edge edge3 = m_EdgeData[edge2];
		//						if (edge3.m_Start == edge.m_Start || edge3.m_End == edge.m_Start)
		//						{
		//							flag7 = false;
		//							break;
		//						}
		//					}
		//				}
		//			}
		//			bool flag8 = flag6;
		//			if (flag6)
		//			{
		//				DynamicBuffer<ConnectedEdge> dynamicBuffer2 = m_ConnectedEdges[edge.m_End];
		//				for (int j = 0; j < dynamicBuffer2.Length; j++)
		//				{
		//					Entity edge4 = dynamicBuffer2[j].m_Edge;
		//					if (!(edge4 == entity))
		//					{
		//						Edge edge5 = m_EdgeData[edge4];
		//						if (edge5.m_Start == edge.m_End || edge5.m_End == edge.m_End)
		//						{
		//							flag8 = false;
		//							break;
		//						}
		//					}
		//				}
		//			}
		//			if (!(flag5 || flag6))
		//			{
		//				return;
		//			}
		//			for (int k = 0; k < num3; k++)
		//			{
		//				if (flag5)
		//				{
		//					float3 a = curve.m_Bezier.a;
		//					a.xz += MathUtils.Left(value) * num4;
		//					Line3.Segment line = new Line3.Segment(a, a);
		//					line.b.xz += value * m_GuideLength;
		//					if (MathUtils.Distance(line.xz, m_ControlPoint.m_HitPosition.xz, out var t) < m_SnapDistance)
		//					{
		//						ControlPoint controlPoint = m_ControlPoint;
		//						controlPoint.m_OriginalEntity = Entity.Null;
		//						if ((m_Snap & Snap.CellLength) != 0)
		//						{
		//							t = MathUtils.Snap(m_GuideLength * t, m_SnapDistance, num7) / m_GuideLength;
		//						}
		//						controlPoint.m_Position = MathUtils.Position(line, t);
		//						controlPoint.m_Direction = value;
		//						controlPoint.m_SnapPriority = ToolUtils.CalculateSnapPriority(0f, 1f, m_ControlPoint.m_HitPosition.xz, controlPoint.m_Position.xz, controlPoint.m_Direction);
		//						ToolUtils.AddSnapPosition(ref m_BestSnapPosition, controlPoint);
		//						ToolUtils.AddSnapLine(ref m_BestSnapPosition, m_SnapLines, new SnapLine(controlPoint, NetUtils.StraightCurve(line.a, line.b), SnapLineFlags.GuideLine));
		//					}
		//					if (k == 0 && flag7)
		//					{
		//						float3 @float = a;
		//						@float.xz += value * num9;
		//						Line3.Segment line2 = new Line3.Segment(@float, @float);
		//						line2.b.xz += MathUtils.Right(value) * m_GuideLength;
		//						if (MathUtils.Distance(line2.xz, m_ControlPoint.m_HitPosition.xz, out var t2) < m_SnapDistance)
		//						{
		//							ControlPoint controlPoint2 = m_ControlPoint;
		//							controlPoint2.m_OriginalEntity = Entity.Null;
		//							if ((m_Snap & Snap.CellLength) != 0)
		//							{
		//								t2 = MathUtils.Snap(m_GuideLength * t2, m_SnapDistance) / m_GuideLength;
		//							}
		//							controlPoint2.m_Position = MathUtils.Position(line2, t2);
		//							controlPoint2.m_Direction = MathUtils.Right(value);
		//							controlPoint2.m_SnapPriority = ToolUtils.CalculateSnapPriority(0f, 1f, m_ControlPoint.m_HitPosition.xz, controlPoint2.m_Position.xz, controlPoint2.m_Direction);
		//							ToolUtils.AddSnapPosition(ref m_BestSnapPosition, controlPoint2);
		//							ToolUtils.AddSnapLine(ref m_BestSnapPosition, m_SnapLines, new SnapLine(controlPoint2, NetUtils.StraightCurve(line2.a, line2.b), SnapLineFlags.GuideLine));
		//						}
		//					}
		//					if (k == num3 - 1 && flag7)
		//					{
		//						float3 float2 = a;
		//						float2.xz += value * num9;
		//						Line3.Segment line3 = new Line3.Segment(float2, float2);
		//						line3.b.xz += MathUtils.Left(value) * m_GuideLength;
		//						if (MathUtils.Distance(line3.xz, m_ControlPoint.m_HitPosition.xz, out var t3) < m_SnapDistance)
		//						{
		//							ControlPoint controlPoint3 = m_ControlPoint;
		//							controlPoint3.m_OriginalEntity = Entity.Null;
		//							if ((m_Snap & Snap.CellLength) != 0)
		//							{
		//								t3 = MathUtils.Snap(m_GuideLength * t3, m_SnapDistance) / m_GuideLength;
		//							}
		//							controlPoint3.m_Position = MathUtils.Position(line3, t3);
		//							controlPoint3.m_Direction = MathUtils.Left(value);
		//							controlPoint3.m_SnapPriority = ToolUtils.CalculateSnapPriority(0f, 1f, m_ControlPoint.m_HitPosition.xz, controlPoint3.m_Position.xz, controlPoint3.m_Direction);
		//							ToolUtils.AddSnapPosition(ref m_BestSnapPosition, controlPoint3);
		//							ToolUtils.AddSnapLine(ref m_BestSnapPosition, m_SnapLines, new SnapLine(controlPoint3, NetUtils.StraightCurve(line3.a, line3.b), SnapLineFlags.GuideLine));
		//						}
		//					}
		//				}
		//				if (flag6)
		//				{
		//					float3 d = curve.m_Bezier.d;
		//					d.xz += MathUtils.Left(value2) * num4;
		//					Line3.Segment line4 = new Line3.Segment(d, d);
		//					line4.b.xz += value2 * m_GuideLength;
		//					if (MathUtils.Distance(line4.xz, m_ControlPoint.m_HitPosition.xz, out var t4) < m_SnapDistance)
		//					{
		//						ControlPoint controlPoint4 = m_ControlPoint;
		//						controlPoint4.m_OriginalEntity = Entity.Null;
		//						if ((m_Snap & Snap.CellLength) != 0)
		//						{
		//							t4 = MathUtils.Snap(m_GuideLength * t4, m_SnapDistance, num8) / m_GuideLength;
		//						}
		//						controlPoint4.m_Position = MathUtils.Position(line4, t4);
		//						controlPoint4.m_Direction = value2;
		//						controlPoint4.m_SnapPriority = ToolUtils.CalculateSnapPriority(0f, 1f, m_ControlPoint.m_HitPosition.xz, controlPoint4.m_Position.xz, controlPoint4.m_Direction);
		//						ToolUtils.AddSnapPosition(ref m_BestSnapPosition, controlPoint4);
		//						ToolUtils.AddSnapLine(ref m_BestSnapPosition, m_SnapLines, new SnapLine(controlPoint4, NetUtils.StraightCurve(line4.a, line4.b), SnapLineFlags.GuideLine));
		//					}
		//					if (k == 0 && flag8)
		//					{
		//						float3 float3 = d;
		//						float3.xz += value2 * num10;
		//						Line3.Segment line5 = new Line3.Segment(float3, float3);
		//						line5.b.xz += MathUtils.Right(value2) * m_GuideLength;
		//						if (MathUtils.Distance(line5.xz, m_ControlPoint.m_HitPosition.xz, out var t5) < m_SnapDistance)
		//						{
		//							ControlPoint controlPoint5 = m_ControlPoint;
		//							controlPoint5.m_OriginalEntity = Entity.Null;
		//							if ((m_Snap & Snap.CellLength) != 0)
		//							{
		//								t5 = MathUtils.Snap(m_GuideLength * t5, m_SnapDistance) / m_GuideLength;
		//							}
		//							controlPoint5.m_Position = MathUtils.Position(line5, t5);
		//							controlPoint5.m_Direction = MathUtils.Right(value2);
		//							controlPoint5.m_SnapPriority = ToolUtils.CalculateSnapPriority(0f, 1f, m_ControlPoint.m_HitPosition.xz, controlPoint5.m_Position.xz, controlPoint5.m_Direction);
		//							ToolUtils.AddSnapPosition(ref m_BestSnapPosition, controlPoint5);
		//							ToolUtils.AddSnapLine(ref m_BestSnapPosition, m_SnapLines, new SnapLine(controlPoint5, NetUtils.StraightCurve(line5.a, line5.b), SnapLineFlags.GuideLine));
		//						}
		//					}
		//					if (k == num3 - 1 && flag8)
		//					{
		//						float3 float4 = d;
		//						float4.xz += value2 * num10;
		//						Line3.Segment line6 = new Line3.Segment(float4, float4);
		//						line6.b.xz += MathUtils.Left(value2) * m_GuideLength;
		//						if (MathUtils.Distance(line6.xz, m_ControlPoint.m_HitPosition.xz, out var t6) < m_SnapDistance)
		//						{
		//							ControlPoint controlPoint6 = m_ControlPoint;
		//							controlPoint6.m_OriginalEntity = Entity.Null;
		//							if ((m_Snap & Snap.CellLength) != 0)
		//							{
		//								t6 = MathUtils.Snap(m_GuideLength * t6, m_SnapDistance) / m_GuideLength;
		//							}
		//							controlPoint6.m_Position = MathUtils.Position(line6, t6);
		//							controlPoint6.m_Direction = MathUtils.Left(value2);
		//							controlPoint6.m_SnapPriority = ToolUtils.CalculateSnapPriority(0f, 1f, m_ControlPoint.m_HitPosition.xz, controlPoint6.m_Position.xz, controlPoint6.m_Direction);
		//							ToolUtils.AddSnapPosition(ref m_BestSnapPosition, controlPoint6);
		//							ToolUtils.AddSnapLine(ref m_BestSnapPosition, m_SnapLines, new SnapLine(controlPoint6, NetUtils.StraightCurve(line6.a, line6.b), SnapLineFlags.GuideLine));
		//						}
		//					}
		//				}
		//				num4 += num5;
		//			}
		//		}

		//		public bool HandleGeometry(Entity entity)
		//		{
		//			PrefabRef prefabRef = m_PrefabRefData[entity];
		//			ControlPoint controlPoint = m_ControlPoint;
		//			controlPoint.m_OriginalEntity = entity;
		//			float num = m_NetGeometryData.m_DefaultWidth * 0.5f + m_SnapOffset;
		//			if (m_PrefabGeometryData.HasComponent(prefabRef.m_Prefab))
		//			{
		//				NetGeometryData netGeometryData = m_PrefabGeometryData[prefabRef.m_Prefab];
		//				if ((m_NetGeometryData.m_Flags & ~netGeometryData.m_Flags & Game.Net.GeometryFlags.StandingNodes) != 0)
		//				{
		//					num = m_LegSnapWidth * 0.5f + m_SnapOffset;
		//				}
		//			}
		//			if (m_ConnectedEdges.HasBuffer(entity))
		//			{
		//				Game.Net.Node node = m_NodeData[entity];
		//				DynamicBuffer<ConnectedEdge> dynamicBuffer = m_ConnectedEdges[entity];
		//				for (int i = 0; i < dynamicBuffer.Length; i++)
		//				{
		//					Edge edge = m_EdgeData[dynamicBuffer[i].m_Edge];
		//					if (edge.m_Start == entity || edge.m_End == entity)
		//					{
		//						return false;
		//					}
		//				}
		//				if (m_PrefabGeometryData.HasComponent(prefabRef.m_Prefab))
		//				{
		//					num += m_PrefabGeometryData[prefabRef.m_Prefab].m_DefaultWidth * 0.5f;
		//				}
		//				if (math.distance(node.m_Position.xz, m_ControlPoint.m_HitPosition.xz) >= num)
		//				{
		//					return false;
		//				}
		//				controlPoint.m_HitPosition.y = node.m_Position.y;
		//				return HandleGeometry(controlPoint, prefabRef);
		//			}
		//			if (m_CurveData.HasComponent(entity))
		//			{
		//				Curve curve = m_CurveData[entity];
		//				if (m_CompositionData.HasComponent(entity))
		//				{
		//					Composition composition = m_CompositionData[entity];
		//					num += m_PrefabCompositionData[composition.m_Edge].m_Width * 0.5f;
		//				}
		//				if (MathUtils.Distance(curve.m_Bezier.xz, m_ControlPoint.m_HitPosition.xz, out controlPoint.m_CurvePosition) >= num)
		//				{
		//					return false;
		//				}
		//				controlPoint.m_HitPosition.y = MathUtils.Position(curve.m_Bezier, controlPoint.m_CurvePosition).y;
		//				return HandleGeometry(controlPoint, prefabRef);
		//			}
		//			return false;
		//		}

		//		public bool HandleGeometry(ControlPoint controlPoint, PrefabRef prefabRef)
		//		{
		//			if (!m_PrefabNetData.HasComponent(prefabRef.m_Prefab))
		//			{
		//				return false;
		//			}
		//			NetData netData = m_PrefabNetData[prefabRef.m_Prefab];
		//			bool snapAdded = false;
		//			bool flag = true;
		//			bool allowEdgeSnap = true;
		//			float y = controlPoint.m_HitPosition.y;
		//			if (m_Elevation < 0f)
		//			{
		//				controlPoint.m_HitPosition.y = TerrainUtils.SampleHeight(ref m_TerrainHeightData, controlPoint.m_HitPosition) + m_Elevation;
		//			}
		//			else
		//			{
		//				controlPoint.m_HitPosition.y = WaterUtils.SampleHeight(ref m_WaterSurfaceData, ref m_TerrainHeightData, controlPoint.m_HitPosition) + m_Elevation;
		//			}
		//			if (m_PrefabGeometryData.HasComponent(prefabRef.m_Prefab))
		//			{
		//				NetGeometryData netGeometryData = m_PrefabGeometryData[prefabRef.m_Prefab];
		//				Bounds1 bounds = m_NetGeometryData.m_DefaultHeightRange + controlPoint.m_HitPosition.y;
		//				Bounds1 bounds2 = netGeometryData.m_DefaultHeightRange + y;
		//				if (!MathUtils.Intersect(bounds, bounds2))
		//				{
		//					flag = false;
		//					allowEdgeSnap = (netGeometryData.m_Flags & Game.Net.GeometryFlags.NoEdgeConnection) == 0;
		//				}
		//			}
		//			if (flag && (netData.m_ConnectLayers & m_NetData.m_ConnectLayers) == 0)
		//			{
		//				return snapAdded;
		//			}
		//			if ((m_NetData.m_ConnectLayers & ~netData.m_RequiredLayers & Layer.LaneEditor) != 0)
		//			{
		//				return snapAdded;
		//			}
		//			float position = y - controlPoint.m_HitPosition.y;
		//			if (!MathUtils.Intersect(m_HeightRange, position))
		//			{
		//				return snapAdded;
		//			}
		//			if (m_NodeData.HasComponent(controlPoint.m_OriginalEntity))
		//			{
		//				if (m_ConnectedEdges.HasBuffer(controlPoint.m_OriginalEntity))
		//				{
		//					DynamicBuffer<ConnectedEdge> dynamicBuffer = m_ConnectedEdges[controlPoint.m_OriginalEntity];
		//					if (dynamicBuffer.Length != 0)
		//					{
		//						for (int i = 0; i < dynamicBuffer.Length; i++)
		//						{
		//							Entity edge = dynamicBuffer[i].m_Edge;
		//							Edge edge2 = m_EdgeData[edge];
		//							if (!(edge2.m_Start != controlPoint.m_OriginalEntity) || !(edge2.m_End != controlPoint.m_OriginalEntity))
		//							{
		//								HandleCurve(controlPoint, edge, allowEdgeSnap, ref snapAdded);
		//							}
		//						}
		//						return snapAdded;
		//					}
		//				}
		//				ControlPoint snapPosition = controlPoint;
		//				Game.Net.Node node = m_NodeData[controlPoint.m_OriginalEntity];
		//				snapPosition.m_Position = node.m_Position;
		//				snapPosition.m_Direction = math.mul(node.m_Rotation, new float3(0f, 0f, 1f)).xz;
		//				MathUtils.TryNormalize(ref snapPosition.m_Direction);
		//				float level = 1f;
		//				if ((m_NetGeometryData.m_Flags & Game.Net.GeometryFlags.StrictNodes) != 0)
		//				{
		//					float num = m_NetGeometryData.m_DefaultWidth * 0.5f;
		//					if (m_PrefabGeometryData.HasComponent(prefabRef.m_Prefab))
		//					{
		//						num += m_PrefabGeometryData[prefabRef.m_Prefab].m_DefaultWidth * 0.5f;
		//					}
		//					if (math.distance(node.m_Position.xz, controlPoint.m_HitPosition.xz) <= num)
		//					{
		//						level = 2f;
		//					}
		//				}
		//				snapPosition.m_SnapPriority = ToolUtils.CalculateSnapPriority(level, 1f, controlPoint.m_HitPosition.xz, snapPosition.m_Position.xz, snapPosition.m_Direction);
		//				ToolUtils.AddSnapPosition(ref m_BestSnapPosition, snapPosition);
		//				snapAdded = true;
		//			}
		//			else if (m_CurveData.HasComponent(controlPoint.m_OriginalEntity))
		//			{
		//				HandleCurve(controlPoint, controlPoint.m_OriginalEntity, allowEdgeSnap, ref snapAdded);
		//			}
		//			return snapAdded;
		//		}

		//		private bool SnapSegmentAreas(ControlPoint controlPoint, NetCompositionData prefabCompositionData, DynamicBuffer<NetCompositionArea> areas, Segment segment, ref bool snapAdded)
		//		{
		//			bool result = false;
		//			for (int i = 0; i < areas.Length; i++)
		//			{
		//				NetCompositionArea netCompositionArea = areas[i];
		//				if ((netCompositionArea.m_Flags & NetAreaFlags.Buildable) == 0)
		//				{
		//					continue;
		//				}
		//				float num = netCompositionArea.m_Width * 0.51f;
		//				if (!(m_LegSnapWidth * 0.5f >= num))
		//				{
		//					result = true;
		//					Bezier4x3 curve = MathUtils.Lerp(segment.m_Left, segment.m_Right, netCompositionArea.m_Position.x / prefabCompositionData.m_Width + 0.5f);
		//					float t;
		//					float num2 = MathUtils.Distance(curve.xz, controlPoint.m_HitPosition.xz, out t);
		//					ControlPoint controlPoint2 = controlPoint;
		//					controlPoint2.m_Position = MathUtils.Position(curve, t);
		//					controlPoint2.m_Direction = math.normalizesafe(MathUtils.Tangent(curve, t).xz);
		//					if ((netCompositionArea.m_Flags & NetAreaFlags.Invert) != 0)
		//					{
		//						controlPoint2.m_Direction = -controlPoint2.m_Direction;
		//					}
		//					float3 @float = MathUtils.Position(MathUtils.Lerp(segment.m_Left, segment.m_Right, netCompositionArea.m_SnapPosition.x / prefabCompositionData.m_Width + 0.5f), t);
		//					float maxLength = math.max(0f, math.min(netCompositionArea.m_Width * 0.5f, math.abs(netCompositionArea.m_SnapPosition.x - netCompositionArea.m_Position.x) + netCompositionArea.m_SnapWidth * 0.5f) - m_LegSnapWidth * 0.5f);
		//					controlPoint2.m_Position.xz += MathUtils.ClampLength(@float.xz - controlPoint2.m_Position.xz, maxLength);
		//					controlPoint2.m_Position.y += netCompositionArea.m_Position.y;
		//					float level = 1f;
		//					if (num2 <= prefabCompositionData.m_Width * 0.5f - math.abs(netCompositionArea.m_Position.x) + m_LegSnapWidth * 0.5f)
		//					{
		//						level = 2f;
		//					}
		//					controlPoint2.m_Rotation = ToolUtils.CalculateRotation(controlPoint2.m_Direction);
		//					controlPoint2.m_SnapPriority = ToolUtils.CalculateSnapPriority(level, 1f, controlPoint.m_HitPosition.xz, controlPoint2.m_Position.xz, controlPoint2.m_Direction);
		//					ToolUtils.AddSnapPosition(ref m_BestSnapPosition, controlPoint2);
		//					ToolUtils.AddSnapLine(ref m_BestSnapPosition, m_SnapLines, new SnapLine(controlPoint2, curve, GetSnapLineFlags(m_NetGeometryData.m_Flags)));
		//					snapAdded = true;
		//				}
		//			}
		//			return result;
		//		}

		//		private void HandleCurve(ControlPoint controlPoint, Entity curveEntity, bool allowEdgeSnap, ref bool snapAdded)
		//		{
		//			bool flag = false;
		//			bool flag2 = (m_NetGeometryData.m_Flags & Game.Net.GeometryFlags.StrictNodes) == 0 && (m_PrefabRoadData.m_Flags & Game.Prefabs.RoadFlags.EnableZoning) != 0 && (m_Snap & Snap.CellLength) != 0;
		//			float defaultWidth = m_NetGeometryData.m_DefaultWidth;
		//			float num = defaultWidth;
		//			float num2 = m_NetGeometryData.m_DefaultWidth * 0.5f;
		//			bool2 @bool = false;
		//			PrefabRef prefabRef = m_PrefabRefData[curveEntity];
		//			NetGeometryData netGeometryData = default(NetGeometryData);
		//			if (m_PrefabGeometryData.HasComponent(prefabRef.m_Prefab))
		//			{
		//				netGeometryData = m_PrefabGeometryData[prefabRef.m_Prefab];
		//			}
		//			if (m_CompositionData.HasComponent(curveEntity))
		//			{
		//				Composition composition = m_CompositionData[curveEntity];
		//				NetCompositionData prefabCompositionData = m_PrefabCompositionData[composition.m_Edge];
		//				num2 += prefabCompositionData.m_Width * 0.5f;
		//				if ((m_NetGeometryData.m_Flags & Game.Net.GeometryFlags.StrictNodes) == 0)
		//				{
		//					num = netGeometryData.m_DefaultWidth;
		//					if (m_RoadCompositionData.HasComponent(composition.m_Edge))
		//					{
		//						flag = (m_RoadCompositionData[composition.m_Edge].m_Flags & Game.Prefabs.RoadFlags.EnableZoning) != 0 && (m_Snap & Snap.CellLength) != 0;
		//						if (flag && m_RoadData.HasComponent(curveEntity))
		//						{
		//							Road road = m_RoadData[curveEntity];
		//							@bool.x = (road.m_Flags & Game.Net.RoadFlags.StartHalfAligned) != 0;
		//							@bool.y = (road.m_Flags & Game.Net.RoadFlags.EndHalfAligned) != 0;
		//						}
		//					}
		//				}
		//				if ((m_NetGeometryData.m_Flags & Game.Net.GeometryFlags.SnapToNetAreas) != 0)
		//				{
		//					DynamicBuffer<NetCompositionArea> areas = m_PrefabCompositionAreas[composition.m_Edge];
		//					EdgeGeometry edgeGeometry = m_EdgeGeometryData[curveEntity];
		//					if (SnapSegmentAreas(controlPoint, prefabCompositionData, areas, edgeGeometry.m_Start, ref snapAdded) | SnapSegmentAreas(controlPoint, prefabCompositionData, areas, edgeGeometry.m_End, ref snapAdded))
		//					{
		//						return;
		//					}
		//				}
		//			}
		//			int num3;
		//			float num4;
		//			float num5;
		//			if (flag2)
		//			{
		//				int cellWidth = ZoneUtils.GetCellWidth(defaultWidth);
		//				int cellWidth2 = ZoneUtils.GetCellWidth(num);
		//				num3 = 1 + math.abs(cellWidth2 - cellWidth);
		//				num4 = (float)(num3 - 1) * -4f;
		//				num5 = 8f;
		//			}
		//			else
		//			{
		//				float num6 = math.abs(num - defaultWidth);
		//				if (num6 > 1.6f)
		//				{
		//					num3 = 3;
		//					num4 = num6 * -0.5f;
		//					num5 = num6 * 0.5f;
		//				}
		//				else
		//				{
		//					num3 = 1;
		//					num4 = 0f;
		//					num5 = 0f;
		//				}
		//			}
		//			float num7;
		//			if (flag)
		//			{
		//				int cellWidth3 = ZoneUtils.GetCellWidth(defaultWidth);
		//				int cellWidth4 = ZoneUtils.GetCellWidth(num);
		//				num7 = math.select(0f, 4f, (((cellWidth3 ^ cellWidth4) & 1) != 0) ^ @bool.x);
		//			}
		//			else
		//			{
		//				num7 = 0f;
		//			}
		//			Curve curve = m_CurveData[curveEntity];
		//			if (m_OwnerData.HasComponent(curveEntity) && !m_EditorMode)
		//			{
		//				allowEdgeSnap = false;
		//			}
		//			float2 @float = math.normalizesafe(MathUtils.Left(MathUtils.StartTangent(curve.m_Bezier).xz));
		//			float2 float2 = math.normalizesafe(MathUtils.Left(curve.m_Bezier.c.xz - curve.m_Bezier.b.xz));
		//			float2 float3 = math.normalizesafe(MathUtils.Left(MathUtils.EndTangent(curve.m_Bezier).xz));
		//			bool flag3 = math.dot(@float, float2) > 0.9998477f && math.dot(float2, float3) > 0.9998477f;
		//			for (int i = 0; i < num3; i++)
		//			{
		//				Bezier4x3 curve2;
		//				if (math.abs(num4) < 0.08f)
		//				{
		//					curve2 = curve.m_Bezier;
		//				}
		//				else if (flag3)
		//				{
		//					curve2 = curve.m_Bezier;
		//					curve2.a.xz += @float * num4;
		//					curve2.b.xz += math.lerp(@float, float3, 1f / 3f) * num4;
		//					curve2.c.xz += math.lerp(@float, float3, 2f / 3f) * num4;
		//					curve2.d.xz += float3 * num4;
		//				}
		//				else
		//				{
		//					curve2 = NetUtils.OffsetCurveLeftSmooth(curve.m_Bezier, num4);
		//				}
		//				float t;
		//				float num8 = (((m_NetGeometryData.m_Flags & Game.Net.GeometryFlags.StrictNodes) != 0) ? MathUtils.Distance(curve2.xz, controlPoint.m_HitPosition.xz, out t) : NetUtils.ExtendedDistance(curve2.xz, controlPoint.m_HitPosition.xz, out t));
		//				ControlPoint controlPoint2 = controlPoint;
		//				if ((m_Snap & Snap.CellLength) != 0)
		//				{
		//					float num9 = MathUtils.Length(curve2.xz);
		//					num9 += math.select(0f, 4f, @bool.x != @bool.y);
		//					num9 = math.fmod(num9 + 0.1f, 8f) * 0.5f;
		//					float value = NetUtils.ExtendedLength(curve2.xz, t);
		//					value = MathUtils.Snap(value, m_SnapDistance, num7 + num9);
		//					t = NetUtils.ExtendedClampLength(curve2.xz, value);
		//					if ((m_NetGeometryData.m_Flags & Game.Net.GeometryFlags.StrictNodes) != 0)
		//					{
		//						t = math.saturate(t);
		//					}
		//					controlPoint2.m_CurvePosition = t;
		//				}
		//				else
		//				{
		//					t = math.saturate(t);
		//					if ((netGeometryData.m_Flags & Game.Net.GeometryFlags.SnapCellSize) != 0)
		//					{
		//						float value2 = NetUtils.ExtendedLength(curve2.xz, t);
		//						value2 = MathUtils.Snap(value2, 4f);
		//						controlPoint2.m_CurvePosition = NetUtils.ExtendedClampLength(curve2.xz, value2);
		//					}
		//					else
		//					{
		//						if (t >= 0.5f)
		//						{
		//							if (math.distance(curve2.d.xz, controlPoint.m_HitPosition.xz) < m_SnapOffset)
		//							{
		//								t = 1f;
		//							}
		//						}
		//						else if (math.distance(curve2.a.xz, controlPoint.m_HitPosition.xz) < m_SnapOffset)
		//						{
		//							t = 0f;
		//						}
		//						controlPoint2.m_CurvePosition = t;
		//					}
		//				}
		//				if (!allowEdgeSnap && t > 0f && t < 1f)
		//				{
		//					if (t >= 0.5f)
		//					{
		//						if (math.distance(curve2.d.xz, controlPoint.m_HitPosition.xz) >= num2 + m_SnapOffset)
		//						{
		//							continue;
		//						}
		//						t = 1f;
		//						controlPoint2.m_CurvePosition = 1f;
		//					}
		//					else
		//					{
		//						if (math.distance(curve2.a.xz, controlPoint.m_HitPosition.xz) >= num2 + m_SnapOffset)
		//						{
		//							continue;
		//						}
		//						t = 0f;
		//						controlPoint2.m_CurvePosition = 0f;
		//					}
		//				}
		//				NetUtils.ExtendedPositionAndTangent(curve2, t, out controlPoint2.m_Position, out var tangent);
		//				controlPoint2.m_Direction = tangent.xz;
		//				MathUtils.TryNormalize(ref controlPoint2.m_Direction);
		//				float level = 1f;
		//				if ((m_NetGeometryData.m_Flags & Game.Net.GeometryFlags.StrictNodes) != 0 && num8 <= num2)
		//				{
		//					level = 2f;
		//				}
		//				controlPoint2.m_SnapPriority = ToolUtils.CalculateSnapPriority(level, 1f, controlPoint.m_HitPosition.xz, controlPoint2.m_Position.xz, controlPoint2.m_Direction);
		//				ToolUtils.AddSnapPosition(ref m_BestSnapPosition, controlPoint2);
		//				ToolUtils.AddSnapLine(ref m_BestSnapPosition, m_SnapLines, new SnapLine(controlPoint2, curve2, GetSnapLineFlags(m_NetGeometryData.m_Flags)));
		//				snapAdded = true;
		//				num4 += num5;
		//			}
		//		}
		//	}

		//	[ReadOnly]
		//	public Mode m_Mode;

		//	[ReadOnly]
		//	public Snap m_Snap;

		//	[ReadOnly]
		//	public float m_Elevation;

		//	[ReadOnly]
		//	public Entity m_Prefab;

		//	[ReadOnly]
		//	public Entity m_LanePrefab;

		//	[ReadOnly]
		//	public bool m_EditorMode;

		//	[ReadOnly]
		//	public bool m_RemoveUpgrade;

		//	[ReadOnly]
		//	public bool m_LeftHandTraffic;

		//	[ReadOnly]
		//	public TerrainHeightData m_TerrainHeightData;

		//	[ReadOnly]
		//	public WaterSurfaceData m_WaterSurfaceData;

		//	[ReadOnly]
		//	public ComponentLookup<Owner> m_OwnerData;

		//	[ReadOnly]
		//	public ComponentLookup<Game.Net.Node> m_NodeData;

		//	[ReadOnly]
		//	public ComponentLookup<Edge> m_EdgeData;

		//	[ReadOnly]
		//	public ComponentLookup<Curve> m_CurveData;

		//	[ReadOnly]
		//	public ComponentLookup<Road> m_RoadData;

		//	[ReadOnly]
		//	public ComponentLookup<Upgraded> m_UpgradedData;

		//	[ReadOnly]
		//	public ComponentLookup<Composition> m_CompositionData;

		//	[ReadOnly]
		//	public ComponentLookup<EdgeGeometry> m_EdgeGeometryData;

		//	[ReadOnly]
		//	public ComponentLookup<Game.Objects.Transform> m_TransformData;

		//	[ReadOnly]
		//	public ComponentLookup<Block> m_ZoneBlockData;

		//	[ReadOnly]
		//	public ComponentLookup<PrefabRef> m_PrefabRefData;

		//	[ReadOnly]
		//	public ComponentLookup<RoadData> m_PrefabRoadData;

		//	[ReadOnly]
		//	public ComponentLookup<NetData> m_PrefabNetData;

		//	[ReadOnly]
		//	public ComponentLookup<NetGeometryData> m_PrefabGeometryData;

		//	[ReadOnly]
		//	public ComponentLookup<NetCompositionData> m_PrefabCompositionData;

		//	[ReadOnly]
		//	public ComponentLookup<RoadComposition> m_RoadCompositionData;

		//	[ReadOnly]
		//	public ComponentLookup<PlaceableNetData> m_PlaceableData;

		//	[ReadOnly]
		//	public ComponentLookup<BuildingData> m_BuildingData;

		//	[ReadOnly]
		//	public ComponentLookup<BuildingExtensionData> m_BuildingExtensionData;

		//	[ReadOnly]
		//	public ComponentLookup<AssetStampData> m_AssetStampData;

		//	[ReadOnly]
		//	public ComponentLookup<ObjectGeometryData> m_ObjectGeometryData;

		//	[ReadOnly]
		//	public ComponentLookup<LocalConnectData> m_LocalConnectData;

		//	[ReadOnly]
		//	public ComponentLookup<NetLaneData> m_PrefabLaneData;

		//	[ReadOnly]
		//	public BufferLookup<ConnectedEdge> m_ConnectedEdges;

		//	[ReadOnly]
		//	public BufferLookup<Game.Net.SubNet> m_SubNets;

		//	[ReadOnly]
		//	public BufferLookup<Cell> m_ZoneCells;

		//	[ReadOnly]
		//	public BufferLookup<NetCompositionArea> m_PrefabCompositionAreas;

		//	[ReadOnly]
		//	public BufferLookup<Game.Prefabs.SubObject> m_SubObjects;

		//	[ReadOnly]
		//	public NativeQuadTree<Entity, QuadTreeBoundsXZ> m_NetSearchTree;

		//	[ReadOnly]
		//	public NativeQuadTree<Entity, QuadTreeBoundsXZ> m_ObjectSearchTree;

		//	[ReadOnly]
		//	public NativeQuadTree<Entity, Bounds2> m_ZoneSearchTree;

		//	public NativeList<ControlPoint> m_ControlPoints;

		//	public NativeList<SnapLine> m_SnapLines;

		//	public NativeList<UpgradeState> m_UpgradeStates;

		//	public NativeValue<Entity> m_StartEntity;

		//	public NativeValue<Entity> m_LastSnappedEntity;

		//	public NativeValue<AppliedUpgrade> m_AppliedUpgrade;

		//	public SourceUpdateData m_SourceUpdateData;

		//	public void Execute()
		//	{
		//		RoadData prefabRoadData = default(RoadData);
		//		NetGeometryData netGeometryData = default(NetGeometryData);
		//		LocalConnectData localConnectData = default(LocalConnectData);
		//		PlaceableNetData placeableNetData = default(PlaceableNetData);
		//		NetData prefabNetData = m_PrefabNetData[m_Prefab];
		//		if (m_PrefabRoadData.HasComponent(m_Prefab))
		//		{
		//			prefabRoadData = m_PrefabRoadData[m_Prefab];
		//		}
		//		if (m_PrefabGeometryData.HasComponent(m_Prefab))
		//		{
		//			netGeometryData = m_PrefabGeometryData[m_Prefab];
		//		}
		//		if (m_LocalConnectData.HasComponent(m_Prefab))
		//		{
		//			localConnectData = m_LocalConnectData[m_Prefab];
		//		}
		//		if (m_PlaceableData.HasComponent(m_Prefab))
		//		{
		//			placeableNetData = m_PlaceableData[m_Prefab];
		//		}
		//		placeableNetData.m_SnapDistance = math.max(placeableNetData.m_SnapDistance, 1f);
		//		if (m_LanePrefab != Entity.Null)
		//		{
		//			netGeometryData.m_Flags |= Game.Net.GeometryFlags.StrictNodes;
		//		}
		//		m_SnapLines.Clear();
		//		m_UpgradeStates.Clear();
		//		if (m_Mode == Mode.Replace || m_ControlPoints.Length <= 1)
		//		{
		//			m_StartEntity.Clear();
		//		}
		//		if (m_Mode == Mode.Replace)
		//		{
		//			ControlPoint startPoint = m_ControlPoints[0];
		//			ControlPoint endPoint = m_ControlPoints[m_ControlPoints.Length - 1];
		//			m_ControlPoints.Clear();
		//			NativeList<PathEdge> path = new NativeList<PathEdge>(Allocator.Temp);
		//			CreatePath(startPoint, endPoint, path, prefabNetData, placeableNetData);
		//			AddControlPoints(startPoint, endPoint, path, netGeometryData, prefabRoadData, placeableNetData);
		//			return;
		//		}
		//		ControlPoint controlPoint = m_ControlPoints[m_ControlPoints.Length - 1];
		//		ControlPoint bestSnapPosition = controlPoint;
		//		bestSnapPosition.m_Position = bestSnapPosition.m_HitPosition;
		//		bestSnapPosition.m_OriginalEntity = Entity.Null;
		//		bestSnapPosition.m_ElementIndex = -1;
		//		HandleWorldSize(ref bestSnapPosition, controlPoint, netGeometryData);
		//		if ((m_Snap & Snap.ObjectSurface) != 0 && m_TransformData.HasComponent(controlPoint.m_OriginalEntity) && m_SubNets.HasBuffer(controlPoint.m_OriginalEntity))
		//		{
		//			bestSnapPosition.m_OriginalEntity = controlPoint.m_OriginalEntity;
		//			bestSnapPosition.m_ElementIndex = controlPoint.m_ElementIndex;
		//		}
		//		if ((m_Snap & (Snap.CellLength | Snap.StraightDirection)) != 0 && m_ControlPoints.Length >= 2)
		//		{
		//			HandleControlPoints(ref bestSnapPosition, controlPoint, netGeometryData, placeableNetData);
		//		}
		//		if ((m_Snap & (Snap.ExistingGeometry | Snap.NearbyGeometry | Snap.GuideLines)) != 0)
		//		{
		//			HandleExistingGeometry(ref bestSnapPosition, controlPoint, prefabRoadData, netGeometryData, prefabNetData, localConnectData, placeableNetData);
		//		}
		//		if ((m_Snap & (Snap.ExistingGeometry | Snap.ObjectSide | Snap.NearbyGeometry)) != 0)
		//		{
		//			HandleExistingObjects(ref bestSnapPosition, controlPoint, prefabRoadData, netGeometryData, prefabNetData, placeableNetData);
		//		}
		//		if ((m_Snap & Snap.LotGrid) != 0)
		//		{
		//			HandleLotGrid(ref bestSnapPosition, controlPoint, prefabRoadData, netGeometryData, prefabNetData, placeableNetData);
		//		}
		//		if ((m_Snap & Snap.ZoneGrid) != 0)
		//		{
		//			HandleZoneGrid(ref bestSnapPosition, controlPoint, prefabRoadData, netGeometryData, prefabNetData);
		//		}
		//		if (m_Mode == Mode.Grid)
		//		{
		//			AdjustMiddlePoint(ref bestSnapPosition, netGeometryData);
		//			AdjustControlPointHeight(ref bestSnapPosition, controlPoint, netGeometryData, placeableNetData);
		//		}
		//		else
		//		{
		//			AdjustControlPointHeight(ref bestSnapPosition, controlPoint, netGeometryData, placeableNetData);
		//			if (m_Mode == Mode.Continuous)
		//			{
		//				AdjustMiddlePoint(ref bestSnapPosition, netGeometryData);
		//			}
		//		}
		//		if (m_EditorMode)
		//		{
		//			if ((m_Snap & Snap.AutoParent) == 0)
		//			{
		//				bestSnapPosition.m_OriginalEntity = Entity.Null;
		//			}
		//			else if (bestSnapPosition.m_OriginalEntity == Entity.Null)
		//			{
		//				FindParent(ref bestSnapPosition, netGeometryData);
		//			}
		//		}
		//		if (m_LastSnappedEntity.value == Entity.Null && bestSnapPosition.m_OriginalEntity != Entity.Null && bestSnapPosition.m_OriginalEntity != controlPoint.m_OriginalEntity)
		//		{
		//			m_SourceUpdateData.AddSnap();
		//		}
		//		m_ControlPoints[m_ControlPoints.Length - 1] = bestSnapPosition;
		//		m_LastSnappedEntity.value = bestSnapPosition.m_OriginalEntity;
		//	}

		//	private void HandleWorldSize(ref ControlPoint bestSnapPosition, ControlPoint controlPoint, NetGeometryData prefabGeometryData)
		//	{
		//		Bounds3 bounds = TerrainUtils.GetBounds(ref m_TerrainHeightData);
		//		float num = prefabGeometryData.m_DefaultWidth * 0.5f;
		//		bool2 @bool = false;
		//		float2 b = 0f;
		//		if (controlPoint.m_HitPosition.x < bounds.min.x + num)
		//		{
		//			@bool.x = true;
		//			b.x = bounds.min.x - num;
		//		}
		//		else if (controlPoint.m_HitPosition.x > bounds.max.x - num)
		//		{
		//			@bool.x = true;
		//			b.x = bounds.max.x + num;
		//		}
		//		if (controlPoint.m_HitPosition.z < bounds.min.z + num)
		//		{
		//			@bool.y = true;
		//			b.y = bounds.min.z - num;
		//		}
		//		else if (controlPoint.m_HitPosition.z > bounds.max.z - num)
		//		{
		//			@bool.y = true;
		//			b.y = bounds.max.z + num;
		//		}
		//		if (math.any(@bool))
		//		{
		//			ControlPoint controlPoint2 = controlPoint;
		//			controlPoint2.m_OriginalEntity = Entity.Null;
		//			controlPoint2.m_Direction = new float2(0f, 1f);
		//			controlPoint2.m_Position.xz = math.select(controlPoint.m_HitPosition.xz, b, @bool);
		//			controlPoint2.m_Position.y = controlPoint.m_HitPosition.y;
		//			controlPoint2.m_SnapPriority = ToolUtils.CalculateSnapPriority(2f, 1f, controlPoint.m_HitPosition.xz, controlPoint2.m_Position.xz, controlPoint2.m_Direction);
		//			ToolUtils.AddSnapPosition(ref bestSnapPosition, controlPoint2);
		//			if (@bool.x)
		//			{
		//				Line3 line = new Line3(controlPoint2.m_Position, controlPoint2.m_Position)
		//				{
		//					a =
		//					{
		//						z = bounds.min.z
		//					},
		//					b =
		//					{
		//						z = bounds.max.z
		//					}
		//				};
		//				ToolUtils.AddSnapLine(ref bestSnapPosition, m_SnapLines, new SnapLine(controlPoint2, NetUtils.StraightCurve(line.a, line.b), SnapLineFlags.Hidden));
		//			}
		//			if (@bool.y)
		//			{
		//				controlPoint2.m_Direction = new float2(1f, 0f);
		//				Line3 line2 = new Line3(controlPoint2.m_Position, controlPoint2.m_Position)
		//				{
		//					a =
		//					{
		//						x = bounds.min.x
		//					},
		//					b =
		//					{
		//						x = bounds.max.x
		//					}
		//				};
		//				ToolUtils.AddSnapLine(ref bestSnapPosition, m_SnapLines, new SnapLine(controlPoint2, NetUtils.StraightCurve(line2.a, line2.b), SnapLineFlags.Hidden));
		//			}
		//		}
		//	}

		//	private void FindParent(ref ControlPoint bestSnapPosition, NetGeometryData prefabGeometryData)
		//	{
		//		Line3.Segment line = ((m_ControlPoints.Length < 2) ? new Line3.Segment(bestSnapPosition.m_Position, bestSnapPosition.m_Position) : new Line3.Segment(m_ControlPoints[m_ControlPoints.Length - 2].m_Position, bestSnapPosition.m_Position));
		//		float num = math.max(0.01f, prefabGeometryData.m_DefaultWidth * 0.5f - 0.5f);
		//		ParentObjectIterator parentObjectIterator = default(ParentObjectIterator);
		//		parentObjectIterator.m_BestSnapPosition = bestSnapPosition;
		//		parentObjectIterator.m_Line = line;
		//		parentObjectIterator.m_Bounds = MathUtils.Expand(MathUtils.Bounds(line), num + 0.4f);
		//		parentObjectIterator.m_Radius = num;
		//		parentObjectIterator.m_OwnerData = m_OwnerData;
		//		parentObjectIterator.m_TransformData = m_TransformData;
		//		parentObjectIterator.m_BuildingData = m_BuildingData;
		//		parentObjectIterator.m_BuildingExtensionData = m_BuildingExtensionData;
		//		parentObjectIterator.m_AssetStampData = m_AssetStampData;
		//		parentObjectIterator.m_PrefabRefData = m_PrefabRefData;
		//		parentObjectIterator.m_PrefabObjectGeometryData = m_ObjectGeometryData;
		//		ParentObjectIterator iterator = parentObjectIterator;
		//		m_ObjectSearchTree.Iterate(ref iterator);
		//		bestSnapPosition = iterator.m_BestSnapPosition;
		//	}

		//	private void HandleLotGrid(ref ControlPoint bestSnapPosition, ControlPoint controlPoint, RoadData prefabRoadData, NetGeometryData prefabGeometryData, NetData prefabNetData, PlaceableNetData placeableNetData)
		//	{
		//		int cellWidth = ZoneUtils.GetCellWidth(prefabGeometryData.m_DefaultWidth);
		//		float num = (float)(cellWidth + 1) * (placeableNetData.m_SnapDistance * 0.5f) * 1.4142135f;
		//		float edgeOffset = 0f;
		//		float maxDistance = placeableNetData.m_SnapDistance * 0.5f + 0.1f;
		//		if (m_PrefabLaneData.HasComponent(m_LanePrefab))
		//		{
		//			edgeOffset = m_PrefabLaneData[m_LanePrefab].m_Width * 0.5f;
		//		}
		//		LotIterator lotIterator = default(LotIterator);
		//		lotIterator.m_Bounds = new Bounds2(controlPoint.m_HitPosition.xz - num, controlPoint.m_HitPosition.xz + num);
		//		lotIterator.m_Radius = prefabGeometryData.m_DefaultWidth * 0.5f;
		//		lotIterator.m_EdgeOffset = edgeOffset;
		//		lotIterator.m_MaxDistance = maxDistance;
		//		lotIterator.m_CellWidth = cellWidth;
		//		lotIterator.m_ControlPoint = controlPoint;
		//		lotIterator.m_BestSnapPosition = bestSnapPosition;
		//		lotIterator.m_SnapLines = m_SnapLines;
		//		lotIterator.m_OwnerData = m_OwnerData;
		//		lotIterator.m_EdgeData = m_EdgeData;
		//		lotIterator.m_NodeData = m_NodeData;
		//		lotIterator.m_TransformData = m_TransformData;
		//		lotIterator.m_PrefabRefData = m_PrefabRefData;
		//		lotIterator.m_BuildingData = m_BuildingData;
		//		lotIterator.m_BuildingExtensionData = m_BuildingExtensionData;
		//		lotIterator.m_AssetStampData = m_AssetStampData;
		//		lotIterator.m_PrefabObjectGeometryData = m_ObjectGeometryData;
		//		LotIterator iterator = lotIterator;
		//		m_ObjectSearchTree.Iterate(ref iterator);
		//		bestSnapPosition = iterator.m_BestSnapPosition;
		//	}

		//	private void CreatePath(ControlPoint startPoint, ControlPoint endPoint, NativeList<PathEdge> path, NetData prefabNetData, PlaceableNetData placeableNetData)
		//	{
		//		if (math.distance(startPoint.m_Position, endPoint.m_Position) < placeableNetData.m_SnapDistance * 0.5f)
		//		{
		//			endPoint = startPoint;
		//		}
		//		CompositionFlags.General general = placeableNetData.m_SetUpgradeFlags.m_General | placeableNetData.m_UnsetUpgradeFlags.m_General;
		//		CompositionFlags.Side side = placeableNetData.m_SetUpgradeFlags.m_Left | placeableNetData.m_SetUpgradeFlags.m_Right | placeableNetData.m_UnsetUpgradeFlags.m_Left | placeableNetData.m_UnsetUpgradeFlags.m_Right;
		//		if (startPoint.m_OriginalEntity == endPoint.m_OriginalEntity)
		//		{
		//			if (m_EdgeData.HasComponent(endPoint.m_OriginalEntity))
		//			{
		//				PrefabRef prefabRef = m_PrefabRefData[endPoint.m_OriginalEntity];
		//				NetData netData = m_PrefabNetData[prefabRef.m_Prefab];
		//				bool num = (prefabNetData.m_RequiredLayers & netData.m_RequiredLayers) != 0;
		//				bool flag = !num && (placeableNetData.m_PlacementFlags & Game.Net.PlacementFlags.IsUpgrade) != 0 && (placeableNetData.m_PlacementFlags & Game.Net.PlacementFlags.NodeUpgrade) == 0 && ((netData.m_GeneralFlagMask & general) != 0 || (netData.m_SideFlagMask & side) != 0);
		//				if (num || flag)
		//				{
		//					PathEdge value = new PathEdge
		//					{
		//						m_Entity = endPoint.m_OriginalEntity,
		//						m_Invert = (endPoint.m_CurvePosition < startPoint.m_CurvePosition),
		//						m_Upgrade = flag
		//					};
		//					path.Add(in value);
		//				}
		//			}
		//			else
		//			{
		//				if (!m_NodeData.HasComponent(endPoint.m_OriginalEntity))
		//				{
		//					return;
		//				}
		//				PrefabRef prefabRef2 = m_PrefabRefData[endPoint.m_OriginalEntity];
		//				NetData netData2 = m_PrefabNetData[prefabRef2.m_Prefab];
		//				bool flag2 = (prefabNetData.m_RequiredLayers & netData2.m_RequiredLayers) != 0;
		//				if (flag2)
		//				{
		//					DynamicBuffer<ConnectedEdge> dynamicBuffer = m_ConnectedEdges[endPoint.m_OriginalEntity];
		//					for (int i = 0; i < dynamicBuffer.Length; i++)
		//					{
		//						Entity edge = dynamicBuffer[i].m_Edge;
		//						Edge edge2 = m_EdgeData[edge];
		//						if (edge2.m_Start == endPoint.m_OriginalEntity || edge2.m_End == endPoint.m_OriginalEntity)
		//						{
		//							flag2 = false;
		//							break;
		//						}
		//					}
		//				}
		//				bool flag3 = !flag2 && (placeableNetData.m_PlacementFlags & (Game.Net.PlacementFlags.IsUpgrade | Game.Net.PlacementFlags.NodeUpgrade)) == (Game.Net.PlacementFlags.IsUpgrade | Game.Net.PlacementFlags.NodeUpgrade) && ((netData2.m_GeneralFlagMask & general) != 0 || (netData2.m_SideFlagMask & side) != 0);
		//				if (flag2 || flag3)
		//				{
		//					PathEdge value = new PathEdge
		//					{
		//						m_Entity = endPoint.m_OriginalEntity,
		//						m_Upgrade = flag3
		//					};
		//					path.Add(in value);
		//				}
		//			}
		//			return;
		//		}
		//		NativeMinHeap<PathItem> nativeMinHeap = new NativeMinHeap<PathItem>(100, Allocator.Temp);
		//		NativeParallelHashMap<Entity, Entity> nativeParallelHashMap = new NativeParallelHashMap<Entity, Entity>(100, Allocator.Temp);
		//		if (m_EdgeData.HasComponent(endPoint.m_OriginalEntity))
		//		{
		//			Edge edge3 = m_EdgeData[endPoint.m_OriginalEntity];
		//			PrefabRef prefabRef3 = m_PrefabRefData[endPoint.m_OriginalEntity];
		//			NetData netData3 = m_PrefabNetData[prefabRef3.m_Prefab];
		//			bool num2 = (prefabNetData.m_RequiredLayers & netData3.m_RequiredLayers) != 0;
		//			bool flag4 = !num2 && (placeableNetData.m_PlacementFlags & Game.Net.PlacementFlags.IsUpgrade) != 0 && ((netData3.m_GeneralFlagMask & general) != 0 || (netData3.m_SideFlagMask & side) != 0);
		//			if (num2 || flag4)
		//			{
		//				nativeMinHeap.Insert(new PathItem
		//				{
		//					m_Node = edge3.m_Start,
		//					m_Edge = endPoint.m_OriginalEntity,
		//					m_Cost = 0f
		//				});
		//				nativeMinHeap.Insert(new PathItem
		//				{
		//					m_Node = edge3.m_End,
		//					m_Edge = endPoint.m_OriginalEntity,
		//					m_Cost = 0f
		//				});
		//			}
		//		}
		//		else if (m_NodeData.HasComponent(endPoint.m_OriginalEntity))
		//		{
		//			nativeMinHeap.Insert(new PathItem
		//			{
		//				m_Node = endPoint.m_OriginalEntity,
		//				m_Edge = Entity.Null,
		//				m_Cost = 0f
		//			});
		//		}
		//		Entity entity = Entity.Null;
		//		while (nativeMinHeap.Length != 0)
		//		{
		//			PathItem pathItem = nativeMinHeap.Extract();
		//			if (pathItem.m_Edge == startPoint.m_OriginalEntity)
		//			{
		//				nativeParallelHashMap[pathItem.m_Node] = pathItem.m_Edge;
		//				entity = pathItem.m_Node;
		//				break;
		//			}
		//			if (!nativeParallelHashMap.TryAdd(pathItem.m_Node, pathItem.m_Edge))
		//			{
		//				continue;
		//			}
		//			if (pathItem.m_Node == startPoint.m_OriginalEntity)
		//			{
		//				entity = pathItem.m_Node;
		//				break;
		//			}
		//			DynamicBuffer<ConnectedEdge> dynamicBuffer2 = m_ConnectedEdges[pathItem.m_Node];
		//			PrefabRef prefabRef4 = default(PrefabRef);
		//			if (pathItem.m_Edge != Entity.Null)
		//			{
		//				prefabRef4 = m_PrefabRefData[pathItem.m_Edge];
		//			}
		//			for (int j = 0; j < dynamicBuffer2.Length; j++)
		//			{
		//				Entity edge4 = dynamicBuffer2[j].m_Edge;
		//				if (edge4 == pathItem.m_Edge)
		//				{
		//					continue;
		//				}
		//				Edge edge5 = m_EdgeData[edge4];
		//				Entity entity2;
		//				if (edge5.m_Start == pathItem.m_Node)
		//				{
		//					entity2 = edge5.m_End;
		//				}
		//				else
		//				{
		//					if (!(edge5.m_End == pathItem.m_Node))
		//					{
		//						continue;
		//					}
		//					entity2 = edge5.m_Start;
		//				}
		//				if (!nativeParallelHashMap.ContainsKey(entity2) || !(edge4 != startPoint.m_OriginalEntity))
		//				{
		//					PrefabRef prefabRef5 = m_PrefabRefData[edge4];
		//					NetData netData4 = m_PrefabNetData[prefabRef5.m_Prefab];
		//					bool num3 = (prefabNetData.m_RequiredLayers & netData4.m_RequiredLayers) != 0;
		//					bool flag5 = !num3 && (placeableNetData.m_PlacementFlags & Game.Net.PlacementFlags.IsUpgrade) != 0 && ((netData4.m_GeneralFlagMask & general) != 0 || (netData4.m_SideFlagMask & side) != 0);
		//					if (num3 || flag5)
		//					{
		//						Curve curve = m_CurveData[edge4];
		//						float num4 = pathItem.m_Cost + curve.m_Length;
		//						num4 += math.select(0f, 9.9f, prefabRef5.m_Prefab != prefabRef4.m_Prefab);
		//						num4 += math.select(0f, 10f, dynamicBuffer2.Length > 2);
		//						nativeMinHeap.Insert(new PathItem
		//						{
		//							m_Node = entity2,
		//							m_Edge = edge4,
		//							m_Cost = num4
		//						});
		//					}
		//				}
		//			}
		//		}
		//		Entity item;
		//		while (nativeParallelHashMap.TryGetValue(entity, out item) && !(item == Entity.Null))
		//		{
		//			Edge edge6 = m_EdgeData[item];
		//			PrefabRef prefabRef6 = m_PrefabRefData[item];
		//			NetData netData5 = m_PrefabNetData[prefabRef6.m_Prefab];
		//			bool flag6 = edge6.m_End == entity;
		//			bool flag7 = (prefabNetData.m_RequiredLayers & netData5.m_RequiredLayers) != 0;
		//			Entity entity3 = (flag6 ? edge6.m_Start : edge6.m_End);
		//			if (flag7 || (placeableNetData.m_PlacementFlags & Game.Net.PlacementFlags.NodeUpgrade) == 0)
		//			{
		//				PathEdge value = new PathEdge
		//				{
		//					m_Entity = item,
		//					m_Invert = flag6,
		//					m_Upgrade = !flag7
		//				};
		//				path.Add(in value);
		//			}
		//			else
		//			{
		//				if (entity == startPoint.m_OriginalEntity)
		//				{
		//					PathEdge value = new PathEdge
		//					{
		//						m_Entity = entity,
		//						m_Upgrade = true
		//					};
		//					path.Add(in value);
		//				}
		//				if (item != endPoint.m_OriginalEntity)
		//				{
		//					PathEdge value = new PathEdge
		//					{
		//						m_Entity = entity3,
		//						m_Upgrade = true
		//					};
		//					path.Add(in value);
		//				}
		//			}
		//			if (!(item == endPoint.m_OriginalEntity))
		//			{
		//				entity = entity3;
		//				continue;
		//			}
		//			break;
		//		}
		//	}

		//	private bool IsNearEnd(Entity edge, Curve curve, float3 position, bool invert)
		//	{
		//		if (m_EdgeGeometryData.TryGetComponent(edge, out var componentData))
		//		{
		//			Bezier4x3 bezier4x = MathUtils.Lerp(componentData.m_Start.m_Left, componentData.m_Start.m_Right, 0.5f);
		//			Bezier4x3 bezier4x2 = MathUtils.Lerp(componentData.m_End.m_Left, componentData.m_End.m_Right, 0.5f);
		//			float t;
		//			float num = MathUtils.Distance(bezier4x.xz, position.xz, out t);
		//			float t2;
		//			float num2 = MathUtils.Distance(bezier4x2.xz, position.xz, out t2);
		//			float middleLength = componentData.m_Start.middleLength;
		//			float middleLength2 = componentData.m_End.middleLength;
		//			return math.select(t * middleLength, middleLength + t2 * middleLength2, num2 < num) > (middleLength + middleLength2) * 0.5f != invert;
		//		}
		//		MathUtils.Distance(curve.m_Bezier.xz, position.xz, out var t3);
		//		return t3 > 0.5f;
		//	}

		//	private void AddControlPoints(ControlPoint startPoint, ControlPoint endPoint, NativeList<PathEdge> path, NetGeometryData prefabGeometryData, RoadData prefabRoadData, PlaceableNetData placeableNetData)
		//	{
		//		m_ControlPoints.Add(in startPoint);
		//		float num = 0f;
		//		float num2 = 0f;
		//		bool flag = false;
		//		CompositionFlags.General general = placeableNetData.m_SetUpgradeFlags.m_General | placeableNetData.m_UnsetUpgradeFlags.m_General;
		//		CompositionFlags.Side side = placeableNetData.m_SetUpgradeFlags.m_Left | placeableNetData.m_SetUpgradeFlags.m_Right | placeableNetData.m_UnsetUpgradeFlags.m_Left | placeableNetData.m_UnsetUpgradeFlags.m_Right;
		//		if (path.Length != 0)
		//		{
		//			PathEdge pathEdge = path[path.Length - 1];
		//			if (m_EdgeData.HasComponent(pathEdge.m_Entity))
		//			{
		//				PrefabRef prefabRef = m_PrefabRefData[pathEdge.m_Entity];
		//				NetData netData = m_PrefabNetData[prefabRef.m_Prefab];
		//				bool flag2 = (netData.m_GeneralFlagMask & general) != 0;
		//				bool flag3 = (netData.m_SideFlagMask & side) != 0;
		//				if (pathEdge.m_Upgrade && !flag3)
		//				{
		//					flag = true;
		//				}
		//				else
		//				{
		//					Composition composition = m_CompositionData[pathEdge.m_Entity];
		//					Curve curve = m_CurveData[pathEdge.m_Entity];
		//					NetCompositionData netCompositionData = m_PrefabCompositionData[composition.m_Edge];
		//					num2 = netCompositionData.m_Width * 0.5f;
		//					MathUtils.Distance(curve.m_Bezier.xz, endPoint.m_HitPosition.xz, out var t);
		//					float3 @float = MathUtils.Position(curve.m_Bezier, t);
		//					float3 value = MathUtils.Tangent(curve.m_Bezier, t);
		//					value = MathUtils.Normalize(value, value.xz);
		//					num = math.dot(endPoint.m_HitPosition.xz - @float.xz, MathUtils.Right(value.xz));
		//					num = math.select(num, 0f - num, pathEdge.m_Invert);
		//					flag = flag2 && math.abs(num) <= netCompositionData.m_Width * (1f / 6f);
		//				}
		//			}
		//		}
		//		for (int i = 0; i < path.Length; i++)
		//		{
		//			PathEdge pathEdge2 = path[i];
		//			if (m_EdgeData.HasComponent(pathEdge2.m_Entity))
		//			{
		//				Edge edge = m_EdgeData[pathEdge2.m_Entity];
		//				Curve curve2 = m_CurveData[pathEdge2.m_Entity];
		//				if (pathEdge2.m_Invert)
		//				{
		//					CommonUtils.Swap(ref edge.m_Start, ref edge.m_End);
		//					curve2.m_Bezier = MathUtils.Invert(curve2.m_Bezier);
		//				}
		//				float num3 = 0f;
		//				if (pathEdge2.m_Upgrade)
		//				{
		//					UpgradeState upgradeState = default(UpgradeState);
		//					upgradeState.m_IsUpgrading = true;
		//					UpgradeState value2 = upgradeState;
		//					if (m_UpgradedData.HasComponent(pathEdge2.m_Entity))
		//					{
		//						value2.m_OldFlags = m_UpgradedData[pathEdge2.m_Entity].m_Flags;
		//					}
		//					if (m_CompositionData.TryGetComponent(pathEdge2.m_Entity, out var componentData))
		//					{
		//						if (m_PrefabCompositionData.TryGetComponent(componentData.m_StartNode, out var componentData2))
		//						{
		//							if ((componentData2.m_Flags.m_General & CompositionFlags.General.Crosswalk) != 0)
		//							{
		//								if ((componentData2.m_Flags.m_General & CompositionFlags.General.Invert) != 0)
		//								{
		//									value2.m_OldFlags.m_Left |= CompositionFlags.Side.AddCrosswalk;
		//								}
		//								else
		//								{
		//									value2.m_OldFlags.m_Right |= CompositionFlags.Side.AddCrosswalk;
		//								}
		//							}
		//							else if ((componentData2.m_Flags.m_General & CompositionFlags.General.Invert) != 0)
		//							{
		//								value2.m_OldFlags.m_Left |= CompositionFlags.Side.RemoveCrosswalk;
		//							}
		//							else
		//							{
		//								value2.m_OldFlags.m_Right |= CompositionFlags.Side.RemoveCrosswalk;
		//							}
		//						}
		//						if (m_PrefabCompositionData.TryGetComponent(componentData.m_EndNode, out var componentData3))
		//						{
		//							if ((componentData3.m_Flags.m_General & CompositionFlags.General.Crosswalk) != 0)
		//							{
		//								if ((componentData3.m_Flags.m_General & CompositionFlags.General.Invert) != 0)
		//								{
		//									value2.m_OldFlags.m_Left |= CompositionFlags.Side.AddCrosswalk;
		//								}
		//								else
		//								{
		//									value2.m_OldFlags.m_Right |= CompositionFlags.Side.AddCrosswalk;
		//								}
		//							}
		//							else if ((componentData3.m_Flags.m_General & CompositionFlags.General.Invert) != 0)
		//							{
		//								value2.m_OldFlags.m_Left |= CompositionFlags.Side.RemoveCrosswalk;
		//							}
		//							else
		//							{
		//								value2.m_OldFlags.m_Right |= CompositionFlags.Side.RemoveCrosswalk;
		//							}
		//						}
		//					}
		//					CompositionFlags compositionFlags;
		//					CompositionFlags compositionFlags2;
		//					if (num < 0f != pathEdge2.m_Invert)
		//					{
		//						compositionFlags = NetCompositionHelpers.InvertCompositionFlags(placeableNetData.m_SetUpgradeFlags);
		//						compositionFlags2 = NetCompositionHelpers.InvertCompositionFlags(placeableNetData.m_UnsetUpgradeFlags);
		//					}
		//					else
		//					{
		//						compositionFlags = placeableNetData.m_SetUpgradeFlags;
		//						compositionFlags2 = placeableNetData.m_UnsetUpgradeFlags;
		//					}
		//					CompositionFlags.Side side2 = CompositionFlags.Side.ForbidLeftTurn | CompositionFlags.Side.ForbidRightTurn | CompositionFlags.Side.AddCrosswalk | CompositionFlags.Side.RemoveCrosswalk | CompositionFlags.Side.ForbidStraight;
		//					CompositionFlags.Side side3 = (compositionFlags.m_Left | compositionFlags.m_Right) & side2;
		//					CompositionFlags.Side side4 = (compositionFlags2.m_Left | compositionFlags2.m_Right) & side2;
		//					if ((side3 | side4) != 0)
		//					{
		//						bool2 @bool = false;
		//						if ((i > 0) & (i < path.Length - 1))
		//						{
		//							@bool = true;
		//						}
		//						else
		//						{
		//							if (i == 0)
		//							{
		//								bool flag4 = IsNearEnd(pathEdge2.m_Entity, curve2, startPoint.m_HitPosition, pathEdge2.m_Invert);
		//								@bool |= new bool2(!flag4, flag4);
		//								if (i + 1 < path.Length)
		//								{
		//									PathEdge pathEdge3 = path[i + 1];
		//									if (m_EdgeData.TryGetComponent(pathEdge3.m_Entity, out var componentData4))
		//									{
		//										@bool |= new bool2((edge.m_Start == componentData4.m_Start) | (edge.m_Start == componentData4.m_End), (edge.m_End == componentData4.m_Start) | (edge.m_End == componentData4.m_End));
		//									}
		//								}
		//							}
		//							if (i == path.Length - 1)
		//							{
		//								bool flag5 = IsNearEnd(pathEdge2.m_Entity, curve2, endPoint.m_HitPosition, pathEdge2.m_Invert);
		//								@bool |= new bool2(!flag5, flag5);
		//								if (i - 1 >= 0)
		//								{
		//									PathEdge pathEdge4 = path[i - 1];
		//									if (m_EdgeData.TryGetComponent(pathEdge4.m_Entity, out var componentData5))
		//									{
		//										@bool |= new bool2((edge.m_Start == componentData5.m_Start) | (edge.m_Start == componentData5.m_End), (edge.m_End == componentData5.m_Start) | (edge.m_End == componentData5.m_End));
		//									}
		//								}
		//							}
		//						}
		//						if (pathEdge2.m_Invert != m_LeftHandTraffic)
		//						{
		//							@bool = @bool.yx;
		//						}
		//						if (@bool.x)
		//						{
		//							compositionFlags.m_Left |= side3;
		//							compositionFlags2.m_Left |= side4;
		//						}
		//						else
		//						{
		//							compositionFlags.m_Left &= ~side3;
		//							compositionFlags2.m_Left &= ~side4;
		//						}
		//						if (@bool.y)
		//						{
		//							compositionFlags.m_Right |= side3;
		//							compositionFlags2.m_Right |= side4;
		//						}
		//						else
		//						{
		//							compositionFlags.m_Right &= ~side3;
		//							compositionFlags2.m_Right &= ~side4;
		//						}
		//					}
		//					PrefabRef prefabRef2 = m_PrefabRefData[pathEdge2.m_Entity];
		//					NetData netData2 = m_PrefabNetData[prefabRef2.m_Prefab];
		//					bool flag6 = (netData2.m_GeneralFlagMask & general) != 0;
		//					bool flag7 = (netData2.m_SideFlagMask & side) != 0;
		//					if (flag || !flag7)
		//					{
		//						CompositionFlags.Side side5 = ~(CompositionFlags.Side.PrimaryBeautification | CompositionFlags.Side.SecondaryBeautification | CompositionFlags.Side.WideSidewalk);
		//						compositionFlags.m_Left &= side5;
		//						compositionFlags.m_Right &= side5;
		//						compositionFlags2.m_Left &= side5;
		//						compositionFlags2.m_Right &= side5;
		//					}
		//					if (!flag || !flag6)
		//					{
		//						CompositionFlags.General general2 = ~(CompositionFlags.General.WideMedian | CompositionFlags.General.PrimaryMiddleBeautification | CompositionFlags.General.SecondaryMiddleBeautification);
		//						compositionFlags.m_General &= general2;
		//						compositionFlags2.m_General &= general2;
		//					}
		//					if (m_RemoveUpgrade)
		//					{
		//						compositionFlags2.m_General = (CompositionFlags.General)0u;
		//						compositionFlags2.m_Left &= CompositionFlags.Side.RemoveCrosswalk;
		//						compositionFlags2.m_Right &= CompositionFlags.Side.RemoveCrosswalk;
		//						value2.m_AddFlags = compositionFlags2;
		//						value2.m_RemoveFlags = compositionFlags;
		//					}
		//					else
		//					{
		//						value2.m_AddFlags = compositionFlags;
		//						value2.m_RemoveFlags = compositionFlags2;
		//					}
		//					m_UpgradeStates.Add(in value2);
		//				}
		//				else
		//				{
		//					ref NativeList<UpgradeState> upgradeStates = ref m_UpgradeStates;
		//					UpgradeState upgradeState = default(UpgradeState);
		//					upgradeStates.Add(in upgradeState);
		//					if ((prefabGeometryData.m_Flags & Game.Net.GeometryFlags.StrictNodes) == 0)
		//					{
		//						num3 = num;
		//						if ((m_Snap & Snap.ExistingGeometry) != 0)
		//						{
		//							Composition composition2 = m_CompositionData[pathEdge2.m_Entity];
		//							NetCompositionData netCompositionData2 = m_PrefabCompositionData[composition2.m_Edge];
		//							RoadComposition roadComposition = default(RoadComposition);
		//							if (m_RoadCompositionData.HasComponent(composition2.m_Edge))
		//							{
		//								roadComposition = m_RoadCompositionData[composition2.m_Edge];
		//							}
		//							float num4 = math.abs(netCompositionData2.m_Width - prefabGeometryData.m_DefaultWidth);
		//							if ((m_Snap & Snap.CellLength) != 0 && (roadComposition.m_Flags & prefabRoadData.m_Flags & Game.Prefabs.RoadFlags.EnableZoning) != 0)
		//							{
		//								int cellWidth = ZoneUtils.GetCellWidth(netCompositionData2.m_Width);
		//								int cellWidth2 = ZoneUtils.GetCellWidth(prefabGeometryData.m_DefaultWidth);
		//								float offset = math.select(0f, 4f, ((cellWidth ^ cellWidth2) & 1) != 0);
		//								num4 = (float)math.abs(cellWidth - cellWidth2) * 8f;
		//								num3 *= (num4 * 0.5f + 3.92f) / num2;
		//								num3 = MathUtils.Snap(num3, 8f, offset);
		//								num3 = math.clamp(num3, num4 * -0.5f, num4 * 0.5f);
		//							}
		//							else if (num4 > 1.6f)
		//							{
		//								num3 *= num4 * 0.74f / num2;
		//								num3 = MathUtils.Snap(num3, num4 * 0.5f);
		//								num3 = math.clamp(num3, num4 * -0.5f, num4 * 0.5f);
		//							}
		//							else
		//							{
		//								num3 = 0f;
		//							}
		//						}
		//					}
		//				}
		//				ControlPoint value3 = endPoint;
		//				value3.m_OriginalEntity = edge.m_Start;
		//				value3.m_Position = curve2.m_Bezier.a;
		//				ControlPoint value4 = endPoint;
		//				value4.m_OriginalEntity = edge.m_End;
		//				value4.m_Position = curve2.m_Bezier.d;
		//				if (math.abs(num3) >= 0.01f)
		//				{
		//					float3 value5 = MathUtils.StartTangent(curve2.m_Bezier);
		//					float3 value6 = MathUtils.EndTangent(curve2.m_Bezier);
		//					value5 = MathUtils.Normalize(value5, value5.xz);
		//					value6 = MathUtils.Normalize(value6, value6.xz);
		//					value3.m_Position.xz += MathUtils.Right(value5.xz) * num3;
		//					value4.m_Position.xz += MathUtils.Right(value6.xz) * num3;
		//				}
		//				m_ControlPoints.Add(in value3);
		//				m_ControlPoints.Add(in value4);
		//			}
		//			else
		//			{
		//				if (!m_NodeData.HasComponent(pathEdge2.m_Entity))
		//				{
		//					continue;
		//				}
		//				Game.Net.Node node = m_NodeData[pathEdge2.m_Entity];
		//				if (pathEdge2.m_Upgrade)
		//				{
		//					UpgradeState upgradeState = default(UpgradeState);
		//					upgradeState.m_IsUpgrading = true;
		//					UpgradeState value7 = upgradeState;
		//					if (m_UpgradedData.HasComponent(pathEdge2.m_Entity))
		//					{
		//						value7.m_OldFlags = m_UpgradedData[pathEdge2.m_Entity].m_Flags;
		//					}
		//					if (m_ConnectedEdges.TryGetBuffer(pathEdge2.m_Entity, out var bufferData))
		//					{
		//						CompositionFlags compositionFlags3 = default(CompositionFlags);
		//						for (int j = 0; j < bufferData.Length; j++)
		//						{
		//							Entity edge2 = bufferData[j].m_Edge;
		//							Edge edge3 = m_EdgeData[edge2];
		//							Composition componentData8;
		//							NetCompositionData componentData9;
		//							if (edge3.m_Start == pathEdge2.m_Entity)
		//							{
		//								if (m_CompositionData.TryGetComponent(edge2, out var componentData6) && m_PrefabCompositionData.TryGetComponent(componentData6.m_StartNode, out var componentData7))
		//								{
		//									compositionFlags3 |= componentData7.m_Flags;
		//								}
		//							}
		//							else if (edge3.m_End == pathEdge2.m_Entity && m_CompositionData.TryGetComponent(edge2, out componentData8) && m_PrefabCompositionData.TryGetComponent(componentData8.m_EndNode, out componentData9))
		//							{
		//								compositionFlags3 |= componentData9.m_Flags;
		//							}
		//						}
		//						if ((compositionFlags3.m_General & CompositionFlags.General.TrafficLights) != 0)
		//						{
		//							value7.m_OldFlags.m_General |= CompositionFlags.General.TrafficLights;
		//						}
		//						else
		//						{
		//							value7.m_OldFlags.m_General |= CompositionFlags.General.RemoveTrafficLights;
		//						}
		//					}
		//					CompositionFlags setUpgradeFlags = placeableNetData.m_SetUpgradeFlags;
		//					CompositionFlags unsetUpgradeFlags = placeableNetData.m_UnsetUpgradeFlags;
		//					if (m_RemoveUpgrade)
		//					{
		//						unsetUpgradeFlags.m_General &= CompositionFlags.General.RemoveTrafficLights;
		//						unsetUpgradeFlags.m_Left = (CompositionFlags.Side)0u;
		//						unsetUpgradeFlags.m_Right = (CompositionFlags.Side)0u;
		//						value7.m_AddFlags = unsetUpgradeFlags;
		//						value7.m_RemoveFlags = setUpgradeFlags;
		//					}
		//					else
		//					{
		//						value7.m_AddFlags = setUpgradeFlags;
		//						value7.m_RemoveFlags = unsetUpgradeFlags;
		//					}
		//					m_UpgradeStates.Add(in value7);
		//				}
		//				else
		//				{
		//					ref NativeList<UpgradeState> upgradeStates2 = ref m_UpgradeStates;
		//					UpgradeState upgradeState = default(UpgradeState);
		//					upgradeStates2.Add(in upgradeState);
		//				}
		//				ControlPoint value8 = endPoint;
		//				value8.m_OriginalEntity = pathEdge2.m_Entity;
		//				value8.m_Position = node.m_Position;
		//				m_ControlPoints.Add(in value8);
		//				m_ControlPoints.Add(in value8);
		//			}
		//		}
		//		m_ControlPoints.Add(in endPoint);
		//		AppliedUpgrade value9 = m_AppliedUpgrade.value;
		//		if (value9.m_Entity != Entity.Null)
		//		{
		//			if (m_UpgradeStates.Length != 1 || m_UpgradeStates[0].m_AddFlags != value9.m_Flags)
		//			{
		//				m_AppliedUpgrade.Clear();
		//				return;
		//			}
		//			UpgradeState value10 = m_UpgradeStates[0];
		//			value10.m_SkipFlags = true;
		//			m_UpgradeStates[0] = value10;
		//		}
		//	}

		//	private void AdjustControlPointHeight(ref ControlPoint bestSnapPosition, ControlPoint controlPoint, NetGeometryData prefabGeometryData, PlaceableNetData placeableNetData)
		//	{
		//		float y = bestSnapPosition.m_Position.y;
		//		if ((m_Snap & Snap.ObjectSurface) == 0 || !m_TransformData.HasComponent(controlPoint.m_OriginalEntity))
		//		{
		//			if (m_Elevation < 0f)
		//			{
		//				bestSnapPosition.m_Position.y = TerrainUtils.SampleHeight(ref m_TerrainHeightData, bestSnapPosition.m_Position) + m_Elevation;
		//			}
		//			else
		//			{
		//				bestSnapPosition.m_Position.y = WaterUtils.SampleHeight(ref m_WaterSurfaceData, ref m_TerrainHeightData, bestSnapPosition.m_Position) + m_Elevation;
		//			}
		//		}
		//		else
		//		{
		//			bestSnapPosition.m_Position.y = controlPoint.m_HitPosition.y;
		//		}
		//		Bounds1 bounds = prefabGeometryData.m_DefaultHeightRange + bestSnapPosition.m_Position.y;
		//		if (m_PrefabRefData.HasComponent(controlPoint.m_OriginalEntity))
		//		{
		//			PrefabRef prefabRef = m_PrefabRefData[controlPoint.m_OriginalEntity];
		//			if (m_PrefabGeometryData.HasComponent(prefabRef.m_Prefab))
		//			{
		//				Bounds1 bounds2 = m_PrefabGeometryData[prefabRef.m_Prefab].m_DefaultHeightRange + controlPoint.m_Position.y;
		//				if (bounds2.max > bounds.min)
		//				{
		//					bounds.max = math.max(bounds.max, bounds2.max);
		//					if (bestSnapPosition.m_OriginalEntity == Entity.Null)
		//					{
		//						bestSnapPosition.m_OriginalEntity = controlPoint.m_OriginalEntity;
		//					}
		//				}
		//			}
		//		}
		//		if (!m_PrefabRefData.HasComponent(bestSnapPosition.m_OriginalEntity))
		//		{
		//			return;
		//		}
		//		PrefabRef prefabRef2 = m_PrefabRefData[bestSnapPosition.m_OriginalEntity];
		//		if (m_PrefabGeometryData.HasComponent(prefabRef2.m_Prefab))
		//		{
		//			Bounds1 bounds3 = m_PrefabGeometryData[prefabRef2.m_Prefab].m_DefaultHeightRange + y;
		//			if (MathUtils.Intersect(bounds, bounds3))
		//			{
		//				bestSnapPosition.m_Elevation += y - bestSnapPosition.m_Position.y;
		//				bestSnapPosition.m_Position.y = y;
		//				bestSnapPosition.m_Elevation = MathUtils.Clamp(bestSnapPosition.m_Elevation, placeableNetData.m_ElevationRange);
		//			}
		//			else
		//			{
		//				bestSnapPosition.m_OriginalEntity = Entity.Null;
		//			}
		//		}
		//	}

		//	private void AdjustMiddlePoint(ref ControlPoint bestSnapPosition, NetGeometryData netGeometryData)
		//	{
		//		float2 @float = (((netGeometryData.m_Flags & Game.Net.GeometryFlags.SnapCellSize) == 0) ? (netGeometryData.m_DefaultWidth * new float2(16f, 8f)) : ((float)ZoneUtils.GetCellWidth(netGeometryData.m_DefaultWidth) * 8f + new float2(192f, 96f)));
		//		float2 float2_ = @float * 11f;
		//		if (m_ControlPoints.Length == 2)
		//		{
		//			ControlPoint controlPoint = m_ControlPoints[m_ControlPoints.Length - 2];
		//			float2 value = bestSnapPosition.m_Position.xz - controlPoint.m_Position.xz;
		//			if (MathUtils.TryNormalize(ref value))
		//			{
		//				bestSnapPosition.m_Direction = value;
		//			}
		//			if (m_Mode == Mode.Grid && math.distance(controlPoint.m_Position.xz, bestSnapPosition.m_Position.xz) > float2_.x)
		//			{
		//				bestSnapPosition.m_Position.xz = controlPoint.m_Position.xz + value * float2_.x;
		//				bestSnapPosition.m_OriginalEntity = Entity.Null;
		//			}
		//		}
		//		else
		//		{
		//			if (m_ControlPoints.Length != 3)
		//			{
		//				return;
		//			}
		//			ControlPoint controlPoint2 = m_ControlPoints[m_ControlPoints.Length - 3];
		//			ControlPoint value2 = m_ControlPoints[m_ControlPoints.Length - 2];
		//			if (m_Mode == Mode.Grid)
		//			{
		//				float2 x = bestSnapPosition.m_Position.xz - controlPoint2.m_Position.xz;
		//				float2 float3 = new float2(math.dot(x, value2.m_Direction), math.dot(x, MathUtils.Right(value2.m_Direction)));
		//				bool2 @bool = math.abs(float3) > float2_;
		//				float3 = math.select(float3, math.select(float2_, -float2_, float3 < 0f), @bool);
		//				value2.m_Position = controlPoint2.m_Position;
		//				value2.m_Position.xz += value2.m_Direction * float3.x;
		//				if (math.any(@bool))
		//				{
		//					bestSnapPosition.m_Position.xz = value2.m_Position.xz + MathUtils.Right(value2.m_Direction) * float3.y;
		//					bestSnapPosition.m_OriginalEntity = Entity.Null;
		//				}
		//			}
		//			else
		//			{
		//				value2.m_Elevation = (controlPoint2.m_Elevation + bestSnapPosition.m_Elevation) * 0.5f;
		//				float2 float4 = bestSnapPosition.m_Position.xz - controlPoint2.m_Position.xz;
		//				float2 direction = value2.m_Direction;
		//				float2 value3 = float4;
		//				if (MathUtils.TryNormalize(ref value3))
		//				{
		//					float num = math.dot(direction, value3);
		//					if (num >= 0.70710677f)
		//					{
		//						float2 float5 = math.lerp(controlPoint2.m_Position.xz, bestSnapPosition.m_Position.xz, 0.5f);
		//						Line2 line = new Line2(controlPoint2.m_Position.xz, controlPoint2.m_Position.xz + direction);
		//						Line2 line2 = new Line2(float5, float5 + MathUtils.Right(value3));
		//						if (MathUtils.Intersect(line, line2, out var t))
		//						{
		//							value2.m_Position = controlPoint2.m_Position;
		//							value2.m_Position.xz += direction * t.x;
		//							float2 value4 = bestSnapPosition.m_Position.xz - value2.m_Position.xz;
		//							if (MathUtils.TryNormalize(ref value4))
		//							{
		//								bestSnapPosition.m_Direction = value4;
		//							}
		//						}
		//					}
		//					else if (num >= 0f)
		//					{
		//						float2 float6 = math.lerp(controlPoint2.m_Position.xz, bestSnapPosition.m_Position.xz, 0.5f);
		//						Line2 line3 = new Line2(controlPoint2.m_Position.xz, controlPoint2.m_Position.xz + MathUtils.Right(direction));
		//						Line2 line4 = new Line2(float6, float6 + MathUtils.Right(value3));
		//						if (MathUtils.Intersect(line3, line4, out var t2))
		//						{
		//							value2.m_Position = controlPoint2.m_Position;
		//							value2.m_Position.xz += direction * math.abs(t2.x);
		//							float2 value5 = bestSnapPosition.m_Position.xz - MathUtils.Position(line3, t2.x);
		//							if (MathUtils.TryNormalize(ref value5))
		//							{
		//								bestSnapPosition.m_Direction = math.select(MathUtils.Right(value5), MathUtils.Left(value5), math.dot(MathUtils.Right(direction), value3) < 0f);
		//							}
		//						}
		//					}
		//					else
		//					{
		//						value2.m_Position = controlPoint2.m_Position;
		//						value2.m_Position.xz += direction * math.abs(math.dot(float4, MathUtils.Right(direction)) * 0.5f);
		//						bestSnapPosition.m_Direction = -value2.m_Direction;
		//					}
		//				}
		//				else
		//				{
		//					value2.m_Position = controlPoint2.m_Position;
		//				}
		//			}
		//			if (value2.m_Elevation < 0f)
		//			{
		//				value2.m_Position.y = TerrainUtils.SampleHeight(ref m_TerrainHeightData, value2.m_Position) + value2.m_Elevation;
		//			}
		//			else
		//			{
		//				value2.m_Position.y = WaterUtils.SampleHeight(ref m_WaterSurfaceData, ref m_TerrainHeightData, value2.m_Position) + value2.m_Elevation;
		//			}
		//			m_ControlPoints[m_ControlPoints.Length - 2] = value2;
		//		}
		//	}

		//	private void HandleControlPoints(ref ControlPoint bestSnapPosition, ControlPoint controlPoint, NetGeometryData prefabGeometryData, PlaceableNetData placeableNetData)
		//	{
		//		ControlPoint controlPoint2 = controlPoint;
		//		controlPoint2.m_OriginalEntity = Entity.Null;
		//		controlPoint2.m_Position = controlPoint.m_HitPosition;
		//		float num = placeableNetData.m_SnapDistance;
		//		if (m_Mode == Mode.Grid && m_ControlPoints.Length == 3)
		//		{
		//			if ((m_Snap & Snap.CellLength) != 0)
		//			{
		//				float2 xz = m_ControlPoints[0].m_Position.xz;
		//				float2 direction = m_ControlPoints[1].m_Direction;
		//				float2 @float = MathUtils.Right(direction);
		//				float2 x = controlPoint.m_HitPosition.xz - xz;
		//				x = new float2(math.dot(x, direction), math.dot(x, @float));
		//				x = MathUtils.Snap(x, num);
		//				xz += x.x * direction + x.y * @float;
		//				controlPoint2.m_Direction = direction;
		//				controlPoint2.m_Position.xz = xz;
		//				controlPoint2.m_Position.y = controlPoint.m_HitPosition.y;
		//				controlPoint2.m_SnapPriority = ToolUtils.CalculateSnapPriority(0f, 1f, controlPoint.m_HitPosition.xz, controlPoint2.m_Position.xz, controlPoint2.m_Direction);
		//				Line3 line = new Line3(controlPoint2.m_Position, controlPoint2.m_Position);
		//				Line3 line2 = new Line3(controlPoint2.m_Position, controlPoint2.m_Position);
		//				line.a.xz -= controlPoint2.m_Direction * 8f;
		//				line.b.xz += controlPoint2.m_Direction * 8f;
		//				line2.a.xz -= MathUtils.Right(controlPoint2.m_Direction) * 8f;
		//				line2.b.xz += MathUtils.Right(controlPoint2.m_Direction) * 8f;
		//				ToolUtils.AddSnapPosition(ref bestSnapPosition, controlPoint2);
		//				ToolUtils.AddSnapLine(ref bestSnapPosition, m_SnapLines, new SnapLine(controlPoint2, NetUtils.StraightCurve(line.a, line.b), SnapLineFlags.Hidden));
		//				controlPoint2.m_Direction = MathUtils.Right(controlPoint2.m_Direction);
		//				ToolUtils.AddSnapLine(ref bestSnapPosition, m_SnapLines, new SnapLine(controlPoint2, NetUtils.StraightCurve(line2.a, line2.b), SnapLineFlags.Hidden));
		//			}
		//			return;
		//		}
		//		ControlPoint prev;
		//		if (m_Mode == Mode.Continuous && m_ControlPoints.Length == 3)
		//		{
		//			prev = m_ControlPoints[0];
		//			prev.m_OriginalEntity = Entity.Null;
		//			prev.m_Direction = m_ControlPoints[1].m_Direction;
		//		}
		//		else
		//		{
		//			prev = m_ControlPoints[m_ControlPoints.Length - 2];
		//		}
		//		float3 value = controlPoint.m_HitPosition - prev.m_Position;
		//		value = MathUtils.Normalize(value, value.xz);
		//		value.y = math.clamp(value.y, -1f, 1f);
		//		bool flag = false;
		//		bool flag2 = false;
		//		if ((m_Snap & Snap.StraightDirection) != 0)
		//		{
		//			float bestDirectionDistance = float.MaxValue;
		//			if (prev.m_OriginalEntity != Entity.Null)
		//			{
		//				HandleStartDirection(prev.m_OriginalEntity, prev, controlPoint, placeableNetData, ref bestDirectionDistance, ref controlPoint2.m_Position, ref value);
		//			}
		//			if (m_StartEntity.value != Entity.Null && m_StartEntity.value != prev.m_OriginalEntity && m_ControlPoints.Length == 2)
		//			{
		//				HandleStartDirection(m_StartEntity.value, prev, controlPoint, placeableNetData, ref bestDirectionDistance, ref controlPoint2.m_Position, ref value);
		//			}
		//			if (!prev.m_Direction.Equals(default(float2)) && bestDirectionDistance == float.MaxValue)
		//			{
		//				ToolUtils.DirectionSnap(ref bestDirectionDistance, ref controlPoint2.m_Position, ref value, controlPoint.m_HitPosition, prev.m_Position, new float3(prev.m_Direction.x, 0f, prev.m_Direction.y), placeableNetData.m_SnapDistance);
		//				if (bestDirectionDistance >= placeableNetData.m_SnapDistance && m_Mode == Mode.Continuous && m_ControlPoints.Length == 3)
		//				{
		//					float2 float2 = MathUtils.RotateLeft(prev.m_Direction, Mathf.PI / 4f);
		//					ToolUtils.DirectionSnap(ref bestDirectionDistance, ref controlPoint2.m_Position, ref value, controlPoint.m_HitPosition, prev.m_Position, new float3(float2.x, 0f, float2.y), placeableNetData.m_SnapDistance);
		//					float2 = MathUtils.RotateRight(prev.m_Direction, Mathf.PI / 4f);
		//					ToolUtils.DirectionSnap(ref bestDirectionDistance, ref controlPoint2.m_Position, ref value, controlPoint.m_HitPosition, prev.m_Position, new float3(float2.x, 0f, float2.y), placeableNetData.m_SnapDistance);
		//					num *= 1.4142135f;
		//				}
		//			}
		//			flag = bestDirectionDistance < placeableNetData.m_SnapDistance;
		//			flag2 = bestDirectionDistance < placeableNetData.m_SnapDistance;
		//		}
		//		if ((m_Snap & Snap.CellLength) != 0 && (m_Mode != Mode.Continuous || (m_ControlPoints.Length == 3 && flag2)))
		//		{
		//			float value2 = math.distance(prev.m_Position, controlPoint2.m_Position);
		//			controlPoint2.m_Position = prev.m_Position + value * MathUtils.Snap(value2, num);
		//			flag = true;
		//		}
		//		controlPoint2.m_Direction = value.xz;
		//		controlPoint2.m_SnapPriority = ToolUtils.CalculateSnapPriority(0f, 1f, controlPoint.m_HitPosition.xz, controlPoint2.m_Position.xz, controlPoint2.m_Direction);
		//		if (flag)
		//		{
		//			ToolUtils.AddSnapPosition(ref bestSnapPosition, controlPoint2);
		//		}
		//		if (flag2)
		//		{
		//			float3 position = controlPoint2.m_Position;
		//			float3 endPos = position;
		//			endPos.xz += controlPoint2.m_Direction;
		//			ToolUtils.AddSnapLine(ref bestSnapPosition, m_SnapLines, new SnapLine(controlPoint2, NetUtils.StraightCurve(position, endPos), GetSnapLineFlags(prefabGeometryData.m_Flags) | SnapLineFlags.Hidden));
		//		}
		//	}

		//	private void HandleStartDirection(Entity startEntity, ControlPoint prev, ControlPoint controlPoint, PlaceableNetData placeableNetData, ref float bestDirectionDistance, ref float3 snapPosition, ref float3 snapDirection)
		//	{
		//		if (m_ConnectedEdges.HasBuffer(startEntity))
		//		{
		//			DynamicBuffer<ConnectedEdge> dynamicBuffer = m_ConnectedEdges[startEntity];
		//			for (int i = 0; i < dynamicBuffer.Length; i++)
		//			{
		//				Entity edge = dynamicBuffer[i].m_Edge;
		//				Edge edge2 = m_EdgeData[edge];
		//				if (!(edge2.m_Start != startEntity) || !(edge2.m_End != startEntity))
		//				{
		//					Curve curve = m_CurveData[edge];
		//					float3 value = ((edge2.m_Start == startEntity) ? MathUtils.StartTangent(curve.m_Bezier) : MathUtils.EndTangent(curve.m_Bezier));
		//					value = MathUtils.Normalize(value, value.xz);
		//					value.y = math.clamp(value.y, -1f, 1f);
		//					ToolUtils.DirectionSnap(ref bestDirectionDistance, ref snapPosition, ref snapDirection, controlPoint.m_HitPosition, prev.m_Position, value, placeableNetData.m_SnapDistance);
		//				}
		//			}
		//		}
		//		else if (m_CurveData.HasComponent(startEntity))
		//		{
		//			float3 value2 = MathUtils.Tangent(m_CurveData[startEntity].m_Bezier, prev.m_CurvePosition);
		//			value2 = MathUtils.Normalize(value2, value2.xz);
		//			value2.y = math.clamp(value2.y, -1f, 1f);
		//			ToolUtils.DirectionSnap(ref bestDirectionDistance, ref snapPosition, ref snapDirection, controlPoint.m_HitPosition, prev.m_Position, value2, placeableNetData.m_SnapDistance);
		//		}
		//		else if (m_TransformData.HasComponent(startEntity))
		//		{
		//			float3 value3 = math.forward(m_TransformData[startEntity].m_Rotation);
		//			value3 = MathUtils.Normalize(value3, value3.xz);
		//			value3.y = math.clamp(value3.y, -1f, 1f);
		//			ToolUtils.DirectionSnap(ref bestDirectionDistance, ref snapPosition, ref snapDirection, controlPoint.m_HitPosition, prev.m_Position, value3, placeableNetData.m_SnapDistance);
		//		}
		//	}

		//	private void HandleZoneGrid(ref ControlPoint bestSnapPosition, ControlPoint controlPoint, RoadData prefabRoadData, NetGeometryData prefabGeometryData, NetData prefabNetData)
		//	{
		//		int cellWidth = ZoneUtils.GetCellWidth(prefabGeometryData.m_DefaultWidth);
		//		float num = (float)(cellWidth + 1) * 4f * 1.4142135f;
		//		float offset = math.select(0f, 4f, (cellWidth & 1) == 0);
		//		ZoneIterator zoneIterator = default(ZoneIterator);
		//		zoneIterator.m_Bounds = new Bounds2(controlPoint.m_HitPosition.xz - num, controlPoint.m_HitPosition.xz + num);
		//		zoneIterator.m_HitPosition = controlPoint.m_HitPosition.xz;
		//		zoneIterator.m_BestDistance = num;
		//		zoneIterator.m_ZoneBlockData = m_ZoneBlockData;
		//		zoneIterator.m_ZoneCells = m_ZoneCells;
		//		ZoneIterator iterator = zoneIterator;
		//		m_ZoneSearchTree.Iterate(ref iterator);
		//		if (iterator.m_BestDistance < num)
		//		{
		//			float2 x = controlPoint.m_HitPosition.xz - iterator.m_BestPosition.xz;
		//			float2 @float = MathUtils.Right(iterator.m_BestDirection);
		//			float num2 = MathUtils.Snap(math.dot(x, iterator.m_BestDirection), 8f, offset);
		//			float num3 = MathUtils.Snap(math.dot(x, @float), 8f, offset);
		//			ControlPoint controlPoint2 = controlPoint;
		//			if (!m_EdgeData.HasComponent(controlPoint.m_OriginalEntity) && !m_NodeData.HasComponent(controlPoint.m_OriginalEntity))
		//			{
		//				controlPoint2.m_OriginalEntity = Entity.Null;
		//			}
		//			controlPoint2.m_Direction = iterator.m_BestDirection;
		//			controlPoint2.m_Position.xz = iterator.m_BestPosition.xz + iterator.m_BestDirection * num2 + @float * num3;
		//			controlPoint2.m_SnapPriority = ToolUtils.CalculateSnapPriority(0f, 1f, controlPoint.m_HitPosition.xz, controlPoint2.m_Position.xz, controlPoint2.m_Direction);
		//			Line3 line = new Line3(controlPoint2.m_Position, controlPoint2.m_Position);
		//			Line3 line2 = new Line3(controlPoint2.m_Position, controlPoint2.m_Position);
		//			line.a.xz -= controlPoint2.m_Direction * 8f;
		//			line.b.xz += controlPoint2.m_Direction * 8f;
		//			line2.a.xz -= MathUtils.Right(controlPoint2.m_Direction) * 8f;
		//			line2.b.xz += MathUtils.Right(controlPoint2.m_Direction) * 8f;
		//			ToolUtils.AddSnapPosition(ref bestSnapPosition, controlPoint2);
		//			ToolUtils.AddSnapLine(ref bestSnapPosition, m_SnapLines, new SnapLine(controlPoint2, NetUtils.StraightCurve(line.a, line.b), SnapLineFlags.Hidden));
		//			controlPoint2.m_Direction = MathUtils.Right(controlPoint2.m_Direction);
		//			ToolUtils.AddSnapLine(ref bestSnapPosition, m_SnapLines, new SnapLine(controlPoint2, NetUtils.StraightCurve(line2.a, line2.b), SnapLineFlags.Hidden));
		//		}
		//	}

		//	private void HandleExistingObjects(ref ControlPoint bestSnapPosition, ControlPoint controlPoint, RoadData prefabRoadData, NetGeometryData prefabGeometryData, NetData prefabNetData, PlaceableNetData placeableNetData)
		//	{
		//		float num = (((m_Snap & Snap.NearbyGeometry) != 0) ? placeableNetData.m_SnapDistance : 0f);
		//		float num2 = (((prefabRoadData.m_Flags & Game.Prefabs.RoadFlags.EnableZoning) == 0 || (m_Snap & Snap.CellLength) == 0) ? (prefabGeometryData.m_DefaultWidth * 0.5f) : ((float)ZoneUtils.GetCellWidth(prefabGeometryData.m_DefaultWidth) * 4f));
		//		float num3 = 0f;
		//		if ((m_Snap & (Snap.ExistingGeometry | Snap.NearbyGeometry)) != 0)
		//		{
		//			num3 = math.max(num3, prefabGeometryData.m_DefaultWidth + num);
		//		}
		//		if ((m_Snap & Snap.ObjectSide) != 0)
		//		{
		//			num3 = math.max(num3, num2 + placeableNetData.m_SnapDistance);
		//		}
		//		ObjectIterator objectIterator = default(ObjectIterator);
		//		objectIterator.m_Bounds = new Bounds3(controlPoint.m_HitPosition - num3, controlPoint.m_HitPosition + num3);
		//		objectIterator.m_Snap = m_Snap;
		//		objectIterator.m_MaxDistance = placeableNetData.m_SnapDistance;
		//		objectIterator.m_NetSnapOffset = num;
		//		objectIterator.m_ObjectSnapOffset = num2;
		//		objectIterator.m_SnapCellLength = (prefabRoadData.m_Flags & Game.Prefabs.RoadFlags.EnableZoning) != 0 && (m_Snap & Snap.CellLength) != 0;
		//		objectIterator.m_NetData = prefabNetData;
		//		objectIterator.m_NetGeometryData = prefabGeometryData;
		//		objectIterator.m_ControlPoint = controlPoint;
		//		objectIterator.m_BestSnapPosition = bestSnapPosition;
		//		objectIterator.m_SnapLines = m_SnapLines;
		//		objectIterator.m_OwnerData = m_OwnerData;
		//		objectIterator.m_CurveData = m_CurveData;
		//		objectIterator.m_NodeData = m_NodeData;
		//		objectIterator.m_TransformData = m_TransformData;
		//		objectIterator.m_PrefabRefData = m_PrefabRefData;
		//		objectIterator.m_BuildingData = m_BuildingData;
		//		objectIterator.m_ObjectGeometryData = m_ObjectGeometryData;
		//		objectIterator.m_PrefabNetData = m_PrefabNetData;
		//		objectIterator.m_PrefabGeometryData = m_PrefabGeometryData;
		//		objectIterator.m_ConnectedEdges = m_ConnectedEdges;
		//		ObjectIterator iterator = objectIterator;
		//		m_ObjectSearchTree.Iterate(ref iterator);
		//		bestSnapPosition = iterator.m_BestSnapPosition;
		//	}

		//	private static SnapLineFlags GetSnapLineFlags(Game.Net.GeometryFlags geometryFlags)
		//	{
		//		SnapLineFlags snapLineFlags = (SnapLineFlags)0;
		//		if ((geometryFlags & Game.Net.GeometryFlags.StrictNodes) == 0)
		//		{
		//			snapLineFlags |= SnapLineFlags.ExtendedCurve;
		//		}
		//		return snapLineFlags;
		//	}

		//	private void HandleExistingGeometry(ref ControlPoint bestSnapPosition, ControlPoint controlPoint, RoadData prefabRoadData, NetGeometryData prefabGeometryData, NetData prefabNetData, LocalConnectData localConnectData, PlaceableNetData placeableNetData)
		//	{
		//		float num = (((m_Snap & Snap.NearbyGeometry) != 0) ? placeableNetData.m_SnapDistance : 0f);
		//		float num2 = prefabGeometryData.m_DefaultWidth + num;
		//		float num3 = placeableNetData.m_SnapDistance * 64f;
		//		Bounds1 bounds = new Bounds1(-50f, 50f) | localConnectData.m_HeightRange;
		//		Bounds3 bounds2 = default(Bounds3);
		//		bounds2.xz = new Bounds2(controlPoint.m_HitPosition.xz - num2, controlPoint.m_HitPosition.xz + num2);
		//		bounds2.y = controlPoint.m_HitPosition.y + bounds;
		//		Bounds3 totalBounds = bounds2;
		//		if ((m_Snap & Snap.GuideLines) != 0)
		//		{
		//			totalBounds.min -= num3;
		//			totalBounds.max += num3;
		//		}
		//		float num4 = -1f;
		//		if ((prefabGeometryData.m_Flags & (Game.Net.GeometryFlags.SnapToNetAreas | Game.Net.GeometryFlags.StandingNodes)) != 0 && m_SubObjects.HasBuffer(m_Prefab))
		//		{
		//			DynamicBuffer<Game.Prefabs.SubObject> dynamicBuffer = m_SubObjects[m_Prefab];
		//			for (int i = 0; i < dynamicBuffer.Length; i++)
		//			{
		//				Game.Prefabs.SubObject subObject = dynamicBuffer[i];
		//				if (m_ObjectGeometryData.HasComponent(subObject.m_Prefab))
		//				{
		//					ObjectGeometryData objectGeometryData = m_ObjectGeometryData[subObject.m_Prefab];
		//					if ((objectGeometryData.m_Flags & Game.Objects.GeometryFlags.Standing) != 0)
		//					{
		//						num4 = math.max(num4, objectGeometryData.m_LegSize.x);
		//					}
		//				}
		//			}
		//		}
		//		num4 = math.select(num4, prefabGeometryData.m_DefaultWidth, num4 <= 0f);
		//		NetIterator netIterator = default(NetIterator);
		//		netIterator.m_TotalBounds = totalBounds;
		//		netIterator.m_Bounds = bounds2;
		//		netIterator.m_Snap = m_Snap;
		//		netIterator.m_SnapOffset = num;
		//		netIterator.m_SnapDistance = placeableNetData.m_SnapDistance;
		//		netIterator.m_Elevation = m_Elevation;
		//		netIterator.m_GuideLength = num3;
		//		netIterator.m_LegSnapWidth = num4;
		//		netIterator.m_HeightRange = bounds;
		//		netIterator.m_NetData = prefabNetData;
		//		netIterator.m_PrefabRoadData = prefabRoadData;
		//		netIterator.m_NetGeometryData = prefabGeometryData;
		//		netIterator.m_LocalConnectData = localConnectData;
		//		netIterator.m_ControlPoint = controlPoint;
		//		netIterator.m_BestSnapPosition = bestSnapPosition;
		//		netIterator.m_SnapLines = m_SnapLines;
		//		netIterator.m_TerrainHeightData = m_TerrainHeightData;
		//		netIterator.m_WaterSurfaceData = m_WaterSurfaceData;
		//		netIterator.m_OwnerData = m_OwnerData;
		//		netIterator.m_EditorMode = m_EditorMode;
		//		netIterator.m_NodeData = m_NodeData;
		//		netIterator.m_EdgeData = m_EdgeData;
		//		netIterator.m_CurveData = m_CurveData;
		//		netIterator.m_CompositionData = m_CompositionData;
		//		netIterator.m_EdgeGeometryData = m_EdgeGeometryData;
		//		netIterator.m_RoadData = m_RoadData;
		//		netIterator.m_PrefabRefData = m_PrefabRefData;
		//		netIterator.m_PrefabNetData = m_PrefabNetData;
		//		netIterator.m_PrefabGeometryData = m_PrefabGeometryData;
		//		netIterator.m_PrefabCompositionData = m_PrefabCompositionData;
		//		netIterator.m_RoadCompositionData = m_RoadCompositionData;
		//		netIterator.m_ConnectedEdges = m_ConnectedEdges;
		//		netIterator.m_PrefabCompositionAreas = m_PrefabCompositionAreas;
		//		NetIterator iterator = netIterator;
		//		if ((m_Snap & Snap.ExistingGeometry) != 0 && m_PrefabRefData.HasComponent(controlPoint.m_OriginalEntity))
		//		{
		//			PrefabRef prefabRef = m_PrefabRefData[controlPoint.m_OriginalEntity];
		//			if (!iterator.HandleGeometry(controlPoint, prefabRef) && (m_Snap & Snap.GuideLines) != 0)
		//			{
		//				iterator.HandleGuideLines(controlPoint.m_OriginalEntity);
		//			}
		//		}
		//		m_NetSearchTree.Iterate(ref iterator);
		//		bestSnapPosition = iterator.m_BestSnapPosition;
		//	}
		//}

		//[BurstCompile]
		//private struct FixControlPointsJob : IJob
		//{
		//	[ReadOnly]
		//	public NativeList<ArchetypeChunk> m_Chunks;

		//	[ReadOnly]
		//	public Mode m_Mode;

		//	[ReadOnly]
		//	public EntityTypeHandle m_EntityType;

		//	[ReadOnly]
		//	public ComponentTypeHandle<Temp> m_TempType;

		//	public NativeList<ControlPoint> m_ControlPoints;

		//	public void Execute()
		//	{
		//		Entity entity = Entity.Null;
		//		for (int i = 0; i < m_Chunks.Length; i++)
		//		{
		//			ArchetypeChunk archetypeChunk = m_Chunks[i];
		//			NativeArray<Entity> nativeArray = archetypeChunk.GetNativeArray(m_EntityType);
		//			NativeArray<Temp> nativeArray2 = archetypeChunk.GetNativeArray(ref m_TempType);
		//			for (int j = 0; j < nativeArray2.Length; j++)
		//			{
		//				Temp temp = nativeArray2[j];
		//				Entity entity2 = nativeArray[j];
		//				if ((temp.m_Flags & TempFlags.Delete) != 0)
		//				{
		//					if (temp.m_Original != Entity.Null)
		//					{
		//						FixControlPoints(temp.m_Original, Entity.Null);
		//					}
		//				}
		//				else if ((temp.m_Flags & (TempFlags.Replace | TempFlags.Combine)) != 0 && temp.m_Original != Entity.Null)
		//				{
		//					FixControlPoints(temp.m_Original, entity2);
		//				}
		//				if ((temp.m_Flags & TempFlags.IsLast) != 0)
		//				{
		//					entity = (((temp.m_Flags & (TempFlags.Create | TempFlags.Replace)) == 0) ? temp.m_Original : entity2);
		//				}
		//			}
		//		}
		//		if (entity != Entity.Null && m_Mode != Mode.Replace)
		//		{
		//			for (int k = 0; k < m_ControlPoints.Length; k++)
		//			{
		//				ControlPoint value = m_ControlPoints[k];
		//				value.m_OriginalEntity = entity;
		//				m_ControlPoints[k] = value;
		//			}
		//		}
		//	}

		//	private void FixControlPoints(Entity entity, Entity replace)
		//	{
		//		if (!(entity != Entity.Null))
		//		{
		//			return;
		//		}
		//		for (int i = 0; i < m_ControlPoints.Length; i++)
		//		{
		//			ControlPoint value = m_ControlPoints[i];
		//			if (value.m_OriginalEntity == entity)
		//			{
		//				value.m_OriginalEntity = replace;
		//				m_ControlPoints[i] = value;
		//			}
		//		}
		//	}
		//}

		[BurstCompile]
		private struct CreateDefinitionsJob : IJob
		{
			[ReadOnly]
			public bool m_EditorMode;

			[ReadOnly]
			public bool m_RemoveUpgrade;

			[ReadOnly]
			public bool m_LefthandTraffic;

			[ReadOnly]
			public Mode m_Mode;

			[ReadOnly]
			public int2 m_ParallelCount;

			[ReadOnly]
			public float m_ParallelOffset;

			[ReadOnly]
			public RandomSeed m_RandomSeed;

			[ReadOnly]
			public NativeList<ControlPoint> m_ControlPoints;

			[ReadOnly]
			public NativeList<UpgradeState> m_UpgradeStates;

			[ReadOnly]
			public ComponentLookup<Edge> m_EdgeData;

			[ReadOnly]
			public ComponentLookup<Game.Net.Node> m_NodeData;

			[ReadOnly]
			public ComponentLookup<Curve> m_CurveData;

			[ReadOnly]
			public ComponentLookup<Game.Tools.EditorContainer> m_EditorContainerData;

			[ReadOnly]
			public ComponentLookup<Owner> m_OwnerData;

			[ReadOnly]
			public ComponentLookup<Temp> m_TempData;

			[ReadOnly]
			public ComponentLookup<LocalTransformCache> m_LocalTransformCacheData;

			[ReadOnly]
			public ComponentLookup<Game.Objects.Transform> m_TransformData;

			[ReadOnly]
			public ComponentLookup<Building> m_BuildingData;

			[ReadOnly]
			public ComponentLookup<Extension> m_ExtensionData;

			[ReadOnly]
			public ComponentLookup<PrefabRef> m_PrefabRefData;

			[ReadOnly]
			public ComponentLookup<NetGeometryData> m_NetGeometryData;

			[ReadOnly]
			public ComponentLookup<PlaceableNetData> m_PlaceableData;

			[ReadOnly]
			public ComponentLookup<SpawnableObjectData> m_PrefabSpawnableObjectData;

			[ReadOnly]
			public ComponentLookup<AreaGeometryData> m_PrefabAreaGeometryData;

			[ReadOnly]
			public BufferLookup<ConnectedEdge> m_ConnectedEdges;

			[ReadOnly]
			public BufferLookup<Game.Net.SubNet> m_SubNets;

			[ReadOnly]
			public BufferLookup<LocalNodeCache> m_CachedNodes;

			[ReadOnly]
			public BufferLookup<Game.Areas.SubArea> m_SubAreas;

			[ReadOnly]
			public BufferLookup<Game.Areas.Node> m_AreaNodes;

			[ReadOnly]
			public BufferLookup<InstalledUpgrade> m_InstalledUpgrades;

			[ReadOnly]
			public BufferLookup<Game.Prefabs.SubObject> m_PrefabSubObjects;

			[ReadOnly]
			public BufferLookup<Game.Prefabs.SubNet> m_PrefabSubNets;

			[ReadOnly]
			public BufferLookup<Game.Prefabs.SubArea> m_PrefabSubAreas;

			[ReadOnly]
			public BufferLookup<SubAreaNode> m_PrefabSubAreaNodes;

			[ReadOnly]
			public BufferLookup<PlaceholderObjectElement> m_PrefabPlaceholderElements;

			[ReadOnly]
			public Entity m_NetPrefab;

			[ReadOnly]
			public Entity m_LanePrefab;

			[ReadOnly]
			public WaterSurfaceData m_WaterSurfaceData;

			public EntityCommandBuffer m_CommandBuffer;

			public void Execute()
			{
				NativeParallelHashMap<Entity, OwnerDefinition> ownerDefinitions = default(NativeParallelHashMap<Entity, OwnerDefinition>);
				if (m_Mode == Mode.Replace)
				{
					CreateReplacement(ref ownerDefinitions);
				}
				if (ownerDefinitions.IsCreated)
				{
					ownerDefinitions.Dispose();
				}
			}

			private bool GetLocalCurve(NetCourse course, OwnerDefinition ownerDefinition, out LocalCurveCache localCurveCache)
			{
				Game.Objects.Transform inverseParentTransform = ObjectUtils.InverseTransform(new Game.Objects.Transform(ownerDefinition.m_Position, ownerDefinition.m_Rotation));
				localCurveCache = default(LocalCurveCache);
				localCurveCache.m_Curve.a = ObjectUtils.WorldToLocal(inverseParentTransform, course.m_Curve.a);
				localCurveCache.m_Curve.b = ObjectUtils.WorldToLocal(inverseParentTransform, course.m_Curve.b);
				localCurveCache.m_Curve.c = ObjectUtils.WorldToLocal(inverseParentTransform, course.m_Curve.c);
				localCurveCache.m_Curve.d = ObjectUtils.WorldToLocal(inverseParentTransform, course.m_Curve.d);
				return true;
			}

			private bool GetOwnerDefinition(ref NativeParallelHashMap<Entity, OwnerDefinition> ownerDefinitions, Entity original, bool checkControlPoints, CoursePos startPos, CoursePos endPos, out OwnerDefinition ownerDefinition)
			{
				Entity entity = Entity.Null;
				ownerDefinition = default(OwnerDefinition);
				if (m_EditorMode)
				{
					if (m_OwnerData.HasComponent(original))
					{
						entity = m_OwnerData[original].m_Owner;
					}
					else if (checkControlPoints)
					{
						for (int i = 0; i < m_ControlPoints.Length; i++)
						{
							Entity entity2 = m_ControlPoints[i].m_OriginalEntity;
							if (m_NodeData.HasComponent(entity2))
							{
								entity2 = Entity.Null;
							}
							while (m_OwnerData.HasComponent(entity2) && !m_BuildingData.HasComponent(entity2))
							{
								entity2 = m_OwnerData[entity2].m_Owner;
								if (m_TempData.HasComponent(entity2))
								{
									Temp temp = m_TempData[entity2];
									if (temp.m_Original != Entity.Null)
									{
										entity2 = temp.m_Original;
									}
								}
							}
							if (m_InstalledUpgrades.TryGetBuffer(entity2, out var bufferData) && bufferData.Length != 0)
							{
								entity2 = bufferData[0].m_Upgrade;
							}
							if (m_TransformData.HasComponent(entity2) && m_SubNets.HasBuffer(entity2))
							{
								entity = entity2;
								break;
							}
						}
					}
				}
				if (ownerDefinitions.IsCreated && ownerDefinitions.TryGetValue(entity, out var item))
				{
					ownerDefinition = item;
				}
				else if (m_TransformData.HasComponent(entity))
				{
					Game.Objects.Transform transform = m_TransformData[entity];
					Entity owner = Entity.Null;
					if (m_OwnerData.HasComponent(entity))
					{
						owner = m_OwnerData[entity].m_Owner;
					}
					UpdateOwnerObject(owner, entity, transform);
					ownerDefinition.m_Prefab = m_PrefabRefData[entity].m_Prefab;
					ownerDefinition.m_Position = transform.m_Position;
					ownerDefinition.m_Rotation = transform.m_Rotation;
					if (!ownerDefinitions.IsCreated)
					{
						ownerDefinitions = new NativeParallelHashMap<Entity, OwnerDefinition>(8, Allocator.Temp);
					}
					ownerDefinitions.Add(entity, ownerDefinition);
				}
				if ((startPos.m_Flags & endPos.m_Flags & (CoursePosFlags.IsFirst | CoursePosFlags.IsLast)) != (CoursePosFlags.IsFirst | CoursePosFlags.IsLast) && m_PrefabSubObjects.HasBuffer(m_NetPrefab))
				{
					DynamicBuffer<Game.Prefabs.SubObject> dynamicBuffer = m_PrefabSubObjects[m_NetPrefab];
					NativeParallelHashMap<Entity, int> selectedSpawnables = default(NativeParallelHashMap<Entity, int>);
					for (int j = 0; j < dynamicBuffer.Length; j++)
					{
						Game.Prefabs.SubObject subObject = dynamicBuffer[j];
						if ((subObject.m_Flags & SubObjectFlags.MakeOwner) != 0)
						{
							Game.Objects.Transform courseObjectTransform = GetCourseObjectTransform(subObject, startPos, endPos);
							CreateCourseObject(subObject.m_Prefab, courseObjectTransform, ownerDefinition, ref selectedSpawnables);
							ownerDefinition.m_Prefab = subObject.m_Prefab;
							ownerDefinition.m_Position = courseObjectTransform.m_Position;
							ownerDefinition.m_Rotation = courseObjectTransform.m_Rotation;
							break;
						}
					}
					for (int k = 0; k < dynamicBuffer.Length; k++)
					{
						Game.Prefabs.SubObject subObject2 = dynamicBuffer[k];
						if ((subObject2.m_Flags & (SubObjectFlags.CoursePlacement | SubObjectFlags.MakeOwner)) == SubObjectFlags.CoursePlacement)
						{
							Game.Objects.Transform courseObjectTransform2 = GetCourseObjectTransform(subObject2, startPos, endPos);
							CreateCourseObject(subObject2.m_Prefab, courseObjectTransform2, ownerDefinition, ref selectedSpawnables);
						}
					}
					if (selectedSpawnables.IsCreated)
					{
						selectedSpawnables.Dispose();
					}
				}
				return ownerDefinition.m_Prefab != Entity.Null;
			}

			private void UpdateOwnerObject(Entity owner, Entity original, Game.Objects.Transform transform)
			{
				Entity e = m_CommandBuffer.CreateEntity();
				Entity prefab = m_PrefabRefData[original].m_Prefab;
				CreationDefinition component = default(CreationDefinition);
				component.m_Owner = owner;
				component.m_Original = original;
				component.m_Flags |= CreationFlags.Upgrade | CreationFlags.Parent;
				ObjectDefinition component2 = default(ObjectDefinition);
				component2.m_ParentMesh = -1;
				component2.m_Position = transform.m_Position;
				component2.m_Rotation = transform.m_Rotation;
				if (m_TransformData.HasComponent(owner))
				{
					Game.Objects.Transform transform2 = ObjectUtils.WorldToLocal(ObjectUtils.InverseTransform(m_TransformData[owner]), transform);
					component2.m_LocalPosition = transform2.m_Position;
					component2.m_LocalRotation = transform2.m_Rotation;
				}
				else
				{
					component2.m_LocalPosition = transform.m_Position;
					component2.m_LocalRotation = transform.m_Rotation;
				}
				m_CommandBuffer.AddComponent(e, component);
				m_CommandBuffer.AddComponent(e, component2);
				m_CommandBuffer.AddComponent(e, default(Updated));
				UpdateSubNets(transform, prefab, original);
				UpdateSubAreas(transform, prefab, original);
			}

			private Game.Objects.Transform GetCourseObjectTransform(Game.Prefabs.SubObject subObject, CoursePos startPos, CoursePos endPos)
			{
				CoursePos coursePos = (((subObject.m_Flags & SubObjectFlags.StartPlacement) != 0) ? startPos : endPos);
				Game.Objects.Transform result = default(Game.Objects.Transform);
				result.m_Position = ObjectUtils.LocalToWorld(coursePos.m_Position, coursePos.m_Rotation, subObject.m_Position);
				result.m_Rotation = math.mul(coursePos.m_Rotation, subObject.m_Rotation);
				return result;
			}

			private void CreateCourseObject(Entity prefab, Game.Objects.Transform transform, OwnerDefinition ownerDefinition, ref NativeParallelHashMap<Entity, int> selectedSpawnables)
			{
				Entity e = m_CommandBuffer.CreateEntity();
				CreationDefinition component = default(CreationDefinition);
				component.m_Prefab = prefab;
				ObjectDefinition component2 = default(ObjectDefinition);
				component2.m_ParentMesh = -1;
				component2.m_Position = transform.m_Position;
				component2.m_Rotation = transform.m_Rotation;
				if (ownerDefinition.m_Prefab != Entity.Null)
				{
					Game.Objects.Transform transform2 = ObjectUtils.WorldToLocal(ObjectUtils.InverseTransform(new Game.Objects.Transform(ownerDefinition.m_Position, ownerDefinition.m_Rotation)), transform);
					component2.m_LocalPosition = transform2.m_Position;
					component2.m_LocalRotation = transform2.m_Rotation;
					m_CommandBuffer.AddComponent(e, ownerDefinition);
				}
				else
				{
					component2.m_LocalPosition = transform.m_Position;
					component2.m_LocalRotation = transform.m_Rotation;
				}
				m_CommandBuffer.AddComponent(e, component);
				m_CommandBuffer.AddComponent(e, component2);
				m_CommandBuffer.AddComponent(e, default(Updated));
				CreateSubNets(transform, prefab);
				CreateSubAreas(transform, prefab, ref selectedSpawnables);
			}

			private void CreateSubAreas(Game.Objects.Transform transform, Entity prefab, ref NativeParallelHashMap<Entity, int> selectedSpawnables)
			{
				if (!m_PrefabSubAreas.HasBuffer(prefab))
				{
					return;
				}
				DynamicBuffer<Game.Prefabs.SubArea> dynamicBuffer = m_PrefabSubAreas[prefab];
				DynamicBuffer<SubAreaNode> nodes = m_PrefabSubAreaNodes[prefab];
				Unity.Mathematics.Random random = m_RandomSeed.GetRandom(10000);
				for (int i = 0; i < dynamicBuffer.Length; i++)
				{
					Game.Prefabs.SubArea subArea = dynamicBuffer[i];
					int seed;
					if (!m_EditorMode && m_PrefabPlaceholderElements.HasBuffer(subArea.m_Prefab))
					{
						DynamicBuffer<PlaceholderObjectElement> placeholderElements = m_PrefabPlaceholderElements[subArea.m_Prefab];
						if (!selectedSpawnables.IsCreated)
						{
							selectedSpawnables = new NativeParallelHashMap<Entity, int>(10, Allocator.Temp);
						}
						if (!AreaUtils.SelectAreaPrefab(placeholderElements, m_PrefabSpawnableObjectData, selectedSpawnables, ref random, out subArea.m_Prefab, out seed))
						{
							continue;
						}
					}
					else
					{
						seed = random.NextInt();
					}
					AreaGeometryData areaGeometryData = m_PrefabAreaGeometryData[subArea.m_Prefab];
					Entity e = m_CommandBuffer.CreateEntity();
					CreationDefinition component = new CreationDefinition
					{
						m_Prefab = subArea.m_Prefab,
						m_RandomSeed = seed
					};
					if (areaGeometryData.m_Type != 0)
					{
						component.m_Flags |= CreationFlags.Hidden;
					}
					m_CommandBuffer.AddComponent(e, component);
					m_CommandBuffer.AddComponent(e, default(Updated));
					OwnerDefinition component2 = default(OwnerDefinition);
					component2.m_Prefab = prefab;
					component2.m_Position = transform.m_Position;
					component2.m_Rotation = transform.m_Rotation;
					m_CommandBuffer.AddComponent(e, component2);
					DynamicBuffer<Game.Areas.Node> dynamicBuffer2 = m_CommandBuffer.AddBuffer<Game.Areas.Node>(e);
					dynamicBuffer2.ResizeUninitialized(subArea.m_NodeRange.y - subArea.m_NodeRange.x + 1);
					DynamicBuffer<LocalNodeCache> dynamicBuffer3 = default(DynamicBuffer<LocalNodeCache>);
					if (m_EditorMode)
					{
						dynamicBuffer3 = m_CommandBuffer.AddBuffer<LocalNodeCache>(e);
						dynamicBuffer3.ResizeUninitialized(dynamicBuffer2.Length);
					}
					int num = ObjectToolBaseSystem.GetFirstNodeIndex(nodes, subArea.m_NodeRange);
					int num2 = 0;
					for (int j = subArea.m_NodeRange.x; j <= subArea.m_NodeRange.y; j++)
					{
						float3 position = nodes[num].m_Position;
						float3 position2 = ObjectUtils.LocalToWorld(transform, position);
						int parentMesh = nodes[num].m_ParentMesh;
						float elevation = math.select(float.MinValue, position.y, parentMesh >= 0);
						dynamicBuffer2[num2] = new Game.Areas.Node(position2, elevation);
						if (m_EditorMode)
						{
							dynamicBuffer3[num2] = new LocalNodeCache
							{
								m_Position = position,
								m_ParentMesh = parentMesh
							};
						}
						num2++;
						if (++num == subArea.m_NodeRange.y)
						{
							num = subArea.m_NodeRange.x;
						}
					}
				}
			}

			private void CreateSubNets(Game.Objects.Transform transform, Entity prefab)
			{
				if (!m_PrefabSubNets.HasBuffer(prefab))
				{
					return;
				}
				DynamicBuffer<Game.Prefabs.SubNet> subNets = m_PrefabSubNets[prefab];
				NativeList<float4> nativeList = new NativeList<float4>(subNets.Length * 2, Allocator.Temp);
				for (int i = 0; i < subNets.Length; i++)
				{
					Game.Prefabs.SubNet subNet = subNets[i];
					if (subNet.m_NodeIndex.x >= 0)
					{
						while (nativeList.Length <= subNet.m_NodeIndex.x)
						{
							float4 value = default(float4);
							nativeList.Add(in value);
						}
						nativeList[subNet.m_NodeIndex.x] += new float4(subNet.m_Curve.a, 1f);
					}
					if (subNet.m_NodeIndex.y >= 0)
					{
						while (nativeList.Length <= subNet.m_NodeIndex.y)
						{
							float4 value = default(float4);
							nativeList.Add(in value);
						}
						nativeList[subNet.m_NodeIndex.y] += new float4(subNet.m_Curve.d, 1f);
					}
				}
				for (int j = 0; j < nativeList.Length; j++)
				{
					nativeList[j] /= math.max(1f, nativeList[j].w);
				}
				for (int k = 0; k < subNets.Length; k++)
				{
					Game.Prefabs.SubNet subNet2 = NetUtils.GetSubNet(subNets, k, m_LefthandTraffic, ref m_NetGeometryData);
					Entity e = m_CommandBuffer.CreateEntity();
					CreationDefinition component = default(CreationDefinition);
					component.m_Prefab = subNet2.m_Prefab;
					m_CommandBuffer.AddComponent(e, component);
					m_CommandBuffer.AddComponent(e, default(Updated));
					OwnerDefinition component2 = default(OwnerDefinition);
					component2.m_Prefab = prefab;
					component2.m_Position = transform.m_Position;
					component2.m_Rotation = transform.m_Rotation;
					m_CommandBuffer.AddComponent(e, component2);
					NetCourse component3 = default(NetCourse);
					component3.m_Curve = TransformCurve(subNet2.m_Curve, transform.m_Position, transform.m_Rotation);
					component3.m_StartPosition.m_Position = component3.m_Curve.a;
					component3.m_StartPosition.m_Rotation = NetUtils.GetNodeRotation(MathUtils.StartTangent(component3.m_Curve), transform.m_Rotation);
					component3.m_StartPosition.m_CourseDelta = 0f;
					component3.m_StartPosition.m_Elevation = subNet2.m_Curve.a.y;
					component3.m_StartPosition.m_ParentMesh = subNet2.m_ParentMesh.x;
					if (subNet2.m_NodeIndex.x >= 0)
					{
						component3.m_StartPosition.m_Position = ObjectUtils.LocalToWorld(transform, nativeList[subNet2.m_NodeIndex.x].xyz);
					}
					component3.m_EndPosition.m_Position = component3.m_Curve.d;
					component3.m_EndPosition.m_Rotation = NetUtils.GetNodeRotation(MathUtils.EndTangent(component3.m_Curve), transform.m_Rotation);
					component3.m_EndPosition.m_CourseDelta = 1f;
					component3.m_EndPosition.m_Elevation = subNet2.m_Curve.d.y;
					component3.m_EndPosition.m_ParentMesh = subNet2.m_ParentMesh.y;
					if (subNet2.m_NodeIndex.y >= 0)
					{
						component3.m_EndPosition.m_Position = ObjectUtils.LocalToWorld(transform, nativeList[subNet2.m_NodeIndex.y].xyz);
					}
					component3.m_Length = MathUtils.Length(component3.m_Curve);
					component3.m_FixedIndex = -1;
					component3.m_StartPosition.m_Flags |= CoursePosFlags.IsFirst;
					component3.m_EndPosition.m_Flags |= CoursePosFlags.IsLast;
					if (component3.m_StartPosition.m_Position.Equals(component3.m_EndPosition.m_Position))
					{
						component3.m_StartPosition.m_Flags |= CoursePosFlags.IsLast;
						component3.m_EndPosition.m_Flags |= CoursePosFlags.IsFirst;
					}
					m_CommandBuffer.AddComponent(e, component3);
					if (subNet2.m_Upgrades != default(CompositionFlags))
					{
						Upgraded upgraded = default(Upgraded);
						upgraded.m_Flags = subNet2.m_Upgrades;
						Upgraded component4 = upgraded;
						m_CommandBuffer.AddComponent(e, component4);
					}
					if (m_EditorMode)
					{
						LocalCurveCache component5 = default(LocalCurveCache);
						component5.m_Curve = subNet2.m_Curve;
						m_CommandBuffer.AddComponent(e, component5);
					}
				}
				nativeList.Dispose();
			}

			private Bezier4x3 TransformCurve(Bezier4x3 curve, float3 position, quaternion rotation)
			{
				curve.a = ObjectUtils.LocalToWorld(position, rotation, curve.a);
				curve.b = ObjectUtils.LocalToWorld(position, rotation, curve.b);
				curve.c = ObjectUtils.LocalToWorld(position, rotation, curve.c);
				curve.d = ObjectUtils.LocalToWorld(position, rotation, curve.d);
				return curve;
			}

			private void UpdateSubNets(Game.Objects.Transform transform, Entity prefab, Entity original)
			{
				NativeParallelHashSet<Entity> nativeParallelHashSet = default(NativeParallelHashSet<Entity>);
				if (m_Mode == Mode.Replace && m_UpgradeStates.Length != 0)
				{
					nativeParallelHashSet = new NativeParallelHashSet<Entity>(m_UpgradeStates.Length, Allocator.Temp);
					for (int i = 0; i < m_UpgradeStates.Length; i++)
					{
						ControlPoint controlPoint = m_ControlPoints[i * 2 + 1];
						ControlPoint controlPoint2 = m_ControlPoints[i * 2 + 2];
						DynamicBuffer<ConnectedEdge> dynamicBuffer = m_ConnectedEdges[controlPoint.m_OriginalEntity];
						for (int j = 0; j < dynamicBuffer.Length; j++)
						{
							Entity edge = dynamicBuffer[j].m_Edge;
							Edge edge2 = m_EdgeData[edge];
							if (edge2.m_Start == controlPoint.m_OriginalEntity && edge2.m_End == controlPoint2.m_OriginalEntity)
							{
								nativeParallelHashSet.Add(edge);
							}
							else if (edge2.m_End == controlPoint.m_OriginalEntity && edge2.m_Start == controlPoint2.m_OriginalEntity)
							{
								nativeParallelHashSet.Add(edge);
							}
						}
					}
				}
				if (m_SubNets.HasBuffer(original))
				{
					DynamicBuffer<Game.Net.SubNet> dynamicBuffer2 = m_SubNets[original];
					for (int k = 0; k < dynamicBuffer2.Length; k++)
					{
						Entity subNet = dynamicBuffer2[k].m_SubNet;
						if (m_NodeData.HasComponent(subNet))
						{
							if (!HasEdgeStartOrEnd(subNet, original))
							{
								Game.Net.Node node = m_NodeData[subNet];
								Entity e = m_CommandBuffer.CreateEntity();
								CreationDefinition component = default(CreationDefinition);
								component.m_Original = subNet;
								if (m_EditorContainerData.HasComponent(subNet))
								{
									component.m_SubPrefab = m_EditorContainerData[subNet].m_Prefab;
								}
								OwnerDefinition component2 = default(OwnerDefinition);
								component2.m_Prefab = prefab;
								component2.m_Position = transform.m_Position;
								component2.m_Rotation = transform.m_Rotation;
								m_CommandBuffer.AddComponent(e, component2);
								m_CommandBuffer.AddComponent(e, component);
								m_CommandBuffer.AddComponent(e, default(Updated));
								NetCourse component3 = default(NetCourse);
								component3.m_Curve = new Bezier4x3(node.m_Position, node.m_Position, node.m_Position, node.m_Position);
								component3.m_Length = 0f;
								component3.m_FixedIndex = -1;
								component3.m_StartPosition.m_Entity = subNet;
								component3.m_StartPosition.m_Position = node.m_Position;
								component3.m_StartPosition.m_Rotation = node.m_Rotation;
								component3.m_StartPosition.m_CourseDelta = 0f;
								component3.m_EndPosition.m_Entity = subNet;
								component3.m_EndPosition.m_Position = node.m_Position;
								component3.m_EndPosition.m_Rotation = node.m_Rotation;
								component3.m_EndPosition.m_CourseDelta = 1f;
								m_CommandBuffer.AddComponent(e, component3);
							}
						}
						else if (m_EdgeData.HasComponent(subNet) && (!nativeParallelHashSet.IsCreated || !nativeParallelHashSet.Contains(subNet)))
						{
							Edge edge3 = m_EdgeData[subNet];
							Entity e2 = m_CommandBuffer.CreateEntity();
							CreationDefinition component4 = default(CreationDefinition);
							component4.m_Original = subNet;
							if (m_EditorContainerData.HasComponent(subNet))
							{
								component4.m_SubPrefab = m_EditorContainerData[subNet].m_Prefab;
							}
							OwnerDefinition component5 = default(OwnerDefinition);
							component5.m_Prefab = prefab;
							component5.m_Position = transform.m_Position;
							component5.m_Rotation = transform.m_Rotation;
							m_CommandBuffer.AddComponent(e2, component5);
							m_CommandBuffer.AddComponent(e2, component4);
							m_CommandBuffer.AddComponent(e2, default(Updated));
							NetCourse component6 = default(NetCourse);
							component6.m_Curve = m_CurveData[subNet].m_Bezier;
							component6.m_Length = MathUtils.Length(component6.m_Curve);
							component6.m_FixedIndex = -1;
							component6.m_StartPosition.m_Entity = edge3.m_Start;
							component6.m_StartPosition.m_Position = component6.m_Curve.a;
							component6.m_StartPosition.m_Rotation = NetUtils.GetNodeRotation(MathUtils.StartTangent(component6.m_Curve));
							component6.m_StartPosition.m_CourseDelta = 0f;
							component6.m_EndPosition.m_Entity = edge3.m_End;
							component6.m_EndPosition.m_Position = component6.m_Curve.d;
							component6.m_EndPosition.m_Rotation = NetUtils.GetNodeRotation(MathUtils.EndTangent(component6.m_Curve));
							component6.m_EndPosition.m_CourseDelta = 1f;
							m_CommandBuffer.AddComponent(e2, component6);
						}
					}
				}
				if (nativeParallelHashSet.IsCreated)
				{
					nativeParallelHashSet.Dispose();
				}
			}

			private bool HasEdgeStartOrEnd(Entity node, Entity owner)
			{
				DynamicBuffer<ConnectedEdge> dynamicBuffer = m_ConnectedEdges[node];
				for (int i = 0; i < dynamicBuffer.Length; i++)
				{
					Entity edge = dynamicBuffer[i].m_Edge;
					Edge edge2 = m_EdgeData[edge];
					if ((edge2.m_Start == node || edge2.m_End == node) && m_OwnerData.HasComponent(edge) && m_OwnerData[edge].m_Owner == owner)
					{
						return true;
					}
				}
				return false;
			}

			private void UpdateSubAreas(Game.Objects.Transform transform, Entity prefab, Entity original)
			{
				if (!m_SubAreas.HasBuffer(original))
				{
					return;
				}
				DynamicBuffer<Game.Areas.SubArea> dynamicBuffer = m_SubAreas[original];
				for (int i = 0; i < dynamicBuffer.Length; i++)
				{
					Entity area = dynamicBuffer[i].m_Area;
					Entity e = m_CommandBuffer.CreateEntity();
					CreationDefinition component = default(CreationDefinition);
					component.m_Original = area;
					OwnerDefinition component2 = default(OwnerDefinition);
					component2.m_Prefab = prefab;
					component2.m_Position = transform.m_Position;
					component2.m_Rotation = transform.m_Rotation;
					m_CommandBuffer.AddComponent(e, component2);
					m_CommandBuffer.AddComponent(e, component);
					m_CommandBuffer.AddComponent(e, default(Updated));
					DynamicBuffer<Game.Areas.Node> dynamicBuffer2 = m_AreaNodes[area];
					m_CommandBuffer.AddBuffer<Game.Areas.Node>(e).CopyFrom(dynamicBuffer2.AsNativeArray());
					if (m_CachedNodes.HasBuffer(area))
					{
						DynamicBuffer<LocalNodeCache> dynamicBuffer3 = m_CachedNodes[area];
						m_CommandBuffer.AddBuffer<LocalNodeCache>(e).CopyFrom(dynamicBuffer3.AsNativeArray());
					}
				}
			}

			private void CreateReplacement(ref NativeParallelHashMap<Entity, OwnerDefinition> ownerDefinitions)
			{
				for (int i = 0; i < m_UpgradeStates.Length; i++)
				{
					ControlPoint controlPoint = m_ControlPoints[i * 2 + 1];
					ControlPoint endPoint = m_ControlPoints[i * 2 + 2];
					UpgradeState upgradeState = m_UpgradeStates[i];
					if (controlPoint.m_OriginalEntity == Entity.Null || endPoint.m_OriginalEntity == Entity.Null)
					{
						continue;
					}
					if (controlPoint.m_OriginalEntity == endPoint.m_OriginalEntity)
					{
						if (upgradeState.m_IsUpgrading || m_RemoveUpgrade)
						{
							CreateUpgrade(ref ownerDefinitions, upgradeState, controlPoint, i == 0, i == m_UpgradeStates.Length - 1);
						}
						else
						{
							CreateReplacement(ref ownerDefinitions, controlPoint, i == 0, i == m_UpgradeStates.Length - 1);
						}
						continue;
					}
					DynamicBuffer<ConnectedEdge> dynamicBuffer = m_ConnectedEdges[controlPoint.m_OriginalEntity];
					for (int j = 0; j < dynamicBuffer.Length; j++)
					{
						Entity edge = dynamicBuffer[j].m_Edge;
						Edge edge2 = m_EdgeData[edge];
						if (edge2.m_Start == controlPoint.m_OriginalEntity && edge2.m_End == endPoint.m_OriginalEntity)
						{
							if (upgradeState.m_IsUpgrading || m_RemoveUpgrade)
							{
								CreateUpgrade(ref ownerDefinitions, edge, upgradeState, invert: false, i == 0, i == m_UpgradeStates.Length - 1);
							}
							else
							{
								CreateReplacement(ref ownerDefinitions, controlPoint, endPoint, edge, invert: false, i == 0, i == m_UpgradeStates.Length - 1);
							}
						}
						else if (edge2.m_End == controlPoint.m_OriginalEntity && edge2.m_Start == endPoint.m_OriginalEntity)
						{
							if (upgradeState.m_IsUpgrading || m_RemoveUpgrade)
							{
								CreateUpgrade(ref ownerDefinitions, edge, upgradeState, invert: true, i == 0, i == m_UpgradeStates.Length - 1);
							}
							else
							{
								CreateReplacement(ref ownerDefinitions, controlPoint, endPoint, edge, invert: true, i == 0, i == m_UpgradeStates.Length - 1);
							}
						}
					}
				}
			}

			private void CreateReplacement(ref NativeParallelHashMap<Entity, OwnerDefinition> ownerDefinitions, ControlPoint point, bool isStart, bool isEnd)
			{
				Entity e = m_CommandBuffer.CreateEntity();
				CreationDefinition component = default(CreationDefinition);
				component.m_Original = point.m_OriginalEntity;
				component.m_Prefab = m_NetPrefab;
				component.m_SubPrefab = m_LanePrefab;
				m_CommandBuffer.AddComponent(e, component);
				m_CommandBuffer.AddComponent(e, default(Updated));
				NetCourse netCourse = default(NetCourse);
				netCourse.m_Curve = new Bezier4x3(point.m_Position, point.m_Position, point.m_Position, point.m_Position);
				netCourse.m_StartPosition = GetCoursePos(netCourse.m_Curve, point, 0f);
				netCourse.m_EndPosition = GetCoursePos(netCourse.m_Curve, point, 1f);
				netCourse.m_Length = MathUtils.Length(netCourse.m_Curve);
				netCourse.m_FixedIndex = -1;
				if (isStart)
				{
					netCourse.m_StartPosition.m_Flags |= CoursePosFlags.IsFirst;
				}
				if (isEnd)
				{
					netCourse.m_EndPosition.m_Flags |= CoursePosFlags.IsLast;
				}
				if (GetOwnerDefinition(ref ownerDefinitions, point.m_OriginalEntity, checkControlPoints: false, netCourse.m_StartPosition, netCourse.m_EndPosition, out var ownerDefinition))
				{
					m_CommandBuffer.AddComponent(e, ownerDefinition);
					if (m_EditorMode && GetLocalCurve(netCourse, ownerDefinition, out var localCurveCache))
					{
						m_CommandBuffer.AddComponent(e, localCurveCache);
					}
				}
				else
				{
					netCourse.m_StartPosition.m_ParentMesh = -1;
					netCourse.m_EndPosition.m_ParentMesh = -1;
				}
				m_CommandBuffer.AddComponent(e, netCourse);
			}

			private void CreateReplacement(ref NativeParallelHashMap<Entity, OwnerDefinition> ownerDefinitions, ControlPoint startPoint, ControlPoint endPoint, Entity edge, bool invert, bool isStart, bool isEnd)
			{
				Entity e = m_CommandBuffer.CreateEntity();
				CreationDefinition component = default(CreationDefinition);
				component.m_Original = edge;
				component.m_Prefab = m_NetPrefab;
				component.m_SubPrefab = m_LanePrefab;
				component.m_Flags |= CreationFlags.Align;
				Curve curve = m_CurveData[edge];
				if (invert)
				{
					curve.m_Bezier = MathUtils.Invert(curve.m_Bezier);
					component.m_Flags |= CreationFlags.Invert;
				}
				m_CommandBuffer.AddComponent(e, component);
				m_CommandBuffer.AddComponent(e, default(Updated));
				NetCourse netCourse = default(NetCourse);
				if (startPoint.m_Position.Equals(curve.m_Bezier.a) && endPoint.m_Position.Equals(curve.m_Bezier.d))
				{
					netCourse.m_Curve = curve.m_Bezier;
					netCourse.m_Length = curve.m_Length;
					netCourse.m_FixedIndex = -1;
				}
				else
				{
					float3 value = MathUtils.StartTangent(curve.m_Bezier);
					float3 value2 = MathUtils.EndTangent(curve.m_Bezier);
					value = MathUtils.Normalize(value, value.xz);
					value2 = MathUtils.Normalize(value2, value2.xz);
					netCourse.m_Curve = NetUtils.FitCurve(startPoint.m_Position, value, value2, endPoint.m_Position);
					netCourse.m_Length = MathUtils.Length(netCourse.m_Curve);
					netCourse.m_FixedIndex = -1;
				}
				netCourse.m_StartPosition.m_Entity = startPoint.m_OriginalEntity;
				netCourse.m_StartPosition.m_Position = startPoint.m_Position;
				netCourse.m_StartPosition.m_Rotation = NetUtils.GetNodeRotation(MathUtils.StartTangent(netCourse.m_Curve));
				netCourse.m_StartPosition.m_CourseDelta = 0f;
				netCourse.m_EndPosition.m_Entity = endPoint.m_OriginalEntity;
				netCourse.m_EndPosition.m_Position = endPoint.m_Position;
				netCourse.m_EndPosition.m_Rotation = NetUtils.GetNodeRotation(MathUtils.EndTangent(netCourse.m_Curve));
				netCourse.m_EndPosition.m_CourseDelta = 1f;
				if (isStart)
				{
					netCourse.m_StartPosition.m_Flags |= CoursePosFlags.IsFirst;
				}
				if (isEnd)
				{
					netCourse.m_EndPosition.m_Flags |= CoursePosFlags.IsLast;
				}
				if (GetOwnerDefinition(ref ownerDefinitions, edge, checkControlPoints: false, netCourse.m_StartPosition, netCourse.m_EndPosition, out var ownerDefinition))
				{
					m_CommandBuffer.AddComponent(e, ownerDefinition);
					if (m_EditorMode && GetLocalCurve(netCourse, ownerDefinition, out var localCurveCache))
					{
						m_CommandBuffer.AddComponent(e, localCurveCache);
					}
				}
				m_CommandBuffer.AddComponent(e, netCourse);
			}

			private void CreateUpgrade(ref NativeParallelHashMap<Entity, OwnerDefinition> ownerDefinitions, UpgradeState upgradeState, ControlPoint point, bool isStart, bool isEnd)
			{
				Entity e = m_CommandBuffer.CreateEntity();
				CreationDefinition component = default(CreationDefinition);
				component.m_Original = point.m_OriginalEntity;
				component.m_Prefab = m_PrefabRefData[point.m_OriginalEntity].m_Prefab;
				component.m_Flags |= CreationFlags.Align;
				if (!upgradeState.m_SkipFlags)
				{
					Upgraded component2 = default(Upgraded);
					component2.m_Flags = (upgradeState.m_OldFlags & ~upgradeState.m_RemoveFlags) | upgradeState.m_AddFlags;
					if (component2.m_Flags != upgradeState.m_OldFlags)
					{
						component.m_Flags |= CreationFlags.Upgrade | CreationFlags.Parent;
					}
					m_CommandBuffer.AddComponent(e, component2);
				}
				m_CommandBuffer.AddComponent(e, component);
				m_CommandBuffer.AddComponent(e, default(Updated));
				NetCourse netCourse = default(NetCourse);
				netCourse.m_Curve = new Bezier4x3(point.m_Position, point.m_Position, point.m_Position, point.m_Position);
				netCourse.m_StartPosition = GetCoursePos(netCourse.m_Curve, point, 0f);
				netCourse.m_EndPosition = GetCoursePos(netCourse.m_Curve, point, 1f);
				netCourse.m_Length = MathUtils.Length(netCourse.m_Curve);
				netCourse.m_FixedIndex = -1;
				if (isStart)
				{
					netCourse.m_StartPosition.m_Flags |= CoursePosFlags.IsFirst;
				}
				if (isEnd)
				{
					netCourse.m_EndPosition.m_Flags |= CoursePosFlags.IsLast;
				}
				if (GetOwnerDefinition(ref ownerDefinitions, point.m_OriginalEntity, checkControlPoints: false, netCourse.m_StartPosition, netCourse.m_EndPosition, out var ownerDefinition))
				{
					m_CommandBuffer.AddComponent(e, ownerDefinition);
					if (m_EditorMode && GetLocalCurve(netCourse, ownerDefinition, out var localCurveCache))
					{
						m_CommandBuffer.AddComponent(e, localCurveCache);
					}
				}
				else
				{
					netCourse.m_StartPosition.m_ParentMesh = -1;
					netCourse.m_EndPosition.m_ParentMesh = -1;
				}
				m_CommandBuffer.AddComponent(e, netCourse);
			}

			private void CreateUpgrade(ref NativeParallelHashMap<Entity, OwnerDefinition> ownerDefinitions, Entity edge, UpgradeState upgradeState, bool invert, bool isStart, bool isEnd)
			{
				Entity e = m_CommandBuffer.CreateEntity();
				CreationDefinition component = default(CreationDefinition);
				component.m_Original = edge;
				component.m_Prefab = m_PrefabRefData[edge].m_Prefab;
				component.m_Flags |= CreationFlags.Align;
				if (!upgradeState.m_SkipFlags)
				{
					Upgraded component2 = default(Upgraded);
					component2.m_Flags = (upgradeState.m_OldFlags & ~upgradeState.m_RemoveFlags) | upgradeState.m_AddFlags;
					if (component2.m_Flags != upgradeState.m_OldFlags)
					{
						component.m_Flags |= CreationFlags.Upgrade | CreationFlags.Parent;
					}
					m_CommandBuffer.AddComponent(e, component2);
				}
				m_CommandBuffer.AddComponent(e, component);
				m_CommandBuffer.AddComponent(e, default(Updated));
				Edge edge2 = m_EdgeData[edge];
				NetCourse netCourse = default(NetCourse);
				netCourse.m_Curve = m_CurveData[edge].m_Bezier;
				netCourse.m_Length = MathUtils.Length(netCourse.m_Curve);
				netCourse.m_FixedIndex = -1;
				netCourse.m_StartPosition.m_Entity = edge2.m_Start;
				netCourse.m_StartPosition.m_Position = netCourse.m_Curve.a;
				netCourse.m_StartPosition.m_Rotation = NetUtils.GetNodeRotation(MathUtils.StartTangent(netCourse.m_Curve));
				netCourse.m_StartPosition.m_CourseDelta = 0f;
				netCourse.m_EndPosition.m_Entity = edge2.m_End;
				netCourse.m_EndPosition.m_Position = netCourse.m_Curve.d;
				netCourse.m_EndPosition.m_Rotation = NetUtils.GetNodeRotation(MathUtils.EndTangent(netCourse.m_Curve));
				netCourse.m_EndPosition.m_CourseDelta = 1f;
				if (invert)
				{
					if (isStart)
					{
						netCourse.m_EndPosition.m_Flags |= CoursePosFlags.IsFirst;
					}
					if (isEnd)
					{
						netCourse.m_StartPosition.m_Flags |= CoursePosFlags.IsLast;
					}
				}
				else
				{
					if (isStart)
					{
						netCourse.m_StartPosition.m_Flags |= CoursePosFlags.IsFirst;
					}
					if (isEnd)
					{
						netCourse.m_EndPosition.m_Flags |= CoursePosFlags.IsLast;
					}
				}
				if (GetOwnerDefinition(ref ownerDefinitions, edge, checkControlPoints: false, netCourse.m_StartPosition, netCourse.m_EndPosition, out var ownerDefinition))
				{
					m_CommandBuffer.AddComponent(e, ownerDefinition);
					if (m_EditorMode && GetLocalCurve(netCourse, ownerDefinition, out var localCurveCache))
					{
						m_CommandBuffer.AddComponent(e, localCurveCache);
					}
				}
				m_CommandBuffer.AddComponent(e, netCourse);
			}

			private void LinearizeElevation(ref Bezier4x3 curve)
			{
				float2 @float = math.lerp(curve.a.y, curve.d.y, new float2(1f / 3f, 2f / 3f));
				curve.b.y = @float.x;
				curve.c.y = @float.y;
			}

			private CoursePos GetCoursePos(Bezier4x3 curve, ControlPoint controlPoint, float courseDelta)
			{
				CoursePos result = default(CoursePos);
				if (controlPoint.m_OriginalEntity != Entity.Null)
				{
					if (m_EdgeData.HasComponent(controlPoint.m_OriginalEntity))
					{
						if (controlPoint.m_CurvePosition <= 0f)
						{
							result.m_Entity = m_EdgeData[controlPoint.m_OriginalEntity].m_Start;
							result.m_SplitPosition = 0f;
						}
						else if (controlPoint.m_CurvePosition >= 1f)
						{
							result.m_Entity = m_EdgeData[controlPoint.m_OriginalEntity].m_End;
							result.m_SplitPosition = 1f;
						}
						else
						{
							result.m_Entity = controlPoint.m_OriginalEntity;
							result.m_SplitPosition = controlPoint.m_CurvePosition;
						}
					}
					else if (m_NodeData.HasComponent(controlPoint.m_OriginalEntity))
					{
						result.m_Entity = controlPoint.m_OriginalEntity;
						result.m_SplitPosition = controlPoint.m_CurvePosition;
					}
				}
				result.m_Position = controlPoint.m_Position;
				result.m_Elevation = controlPoint.m_Elevation;
				result.m_Rotation = NetUtils.GetNodeRotation(MathUtils.Tangent(curve, courseDelta));
				result.m_CourseDelta = courseDelta;
				result.m_ParentMesh = controlPoint.m_ElementIndex.x;
				Entity entity = controlPoint.m_OriginalEntity;
				while (m_OwnerData.HasComponent(entity) && !m_BuildingData.HasComponent(entity) && !m_ExtensionData.HasComponent(entity))
				{
					Edge componentData;
					LocalTransformCache componentData2;
					LocalTransformCache componentData3;
					if (m_LocalTransformCacheData.HasComponent(entity))
					{
						result.m_ParentMesh = m_LocalTransformCacheData[entity].m_ParentMesh;
					}
					else if (m_EdgeData.TryGetComponent(entity, out componentData) && m_LocalTransformCacheData.TryGetComponent(componentData.m_Start, out componentData2) && m_LocalTransformCacheData.TryGetComponent(componentData.m_End, out componentData3))
					{
						result.m_ParentMesh = math.select(componentData2.m_ParentMesh, -1, componentData2.m_ParentMesh != componentData3.m_ParentMesh);
					}
					entity = m_OwnerData[entity].m_Owner;
				}
				return result;
			}
		}
	}
}