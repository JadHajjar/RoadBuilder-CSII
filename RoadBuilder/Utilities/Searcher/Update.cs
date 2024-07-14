using Colossal.Entities;
using Colossal.Mathematics;

using System;
using System.Linq;

using Unity.Collections;
using Unity.Entities;

namespace RoadBuilder.Utilities.Searcher
{
	public class Update2
	{
		private EntityManager EntityManager;

		public Update2(EntityManager entityManager)
		{
			EntityManager = entityManager;
		}

		private const float TERRAIN_UPDATE_MARGIN = 16f;

		public void Update(ref NativeArray<Entity> entities)
		{
			Bounds3 bounds = default;

			for (var i = 0; i < entities.Length; i++)
			{
				if (EntityManager.TryGetComponent<Game.Rendering.CullingInfo>(entities[i], out var cullingInfo))
				{
					bounds = i == 0 ? cullingInfo.m_Bounds : bounds.Encapsulate(cullingInfo.m_Bounds);
				}
			}

			Mod.Log.Info($"{bounds.x} {bounds.y} {bounds.z} {bounds.xy} {bounds.xz} {bounds.yx} {bounds.yz} {bounds.zx} {bounds.zy}");

			//UpdateTerrain(bounds);

			UpdateNearbyBuildingConnections(EntityManager, bounds);
		}

		internal void UpdateNearbyBuildingConnections(EntityManager manager, Bounds3 bounds)
		{
			Bounds2 outerBounds = new(bounds.min.XZ(), bounds.max.XZ());
			using Searcher searcher = new(Filters.AllNets | Filters.Buildings, false, default);
			searcher.SearchBounds(outerBounds);

			Mod.Log.InfoFormat("Results: {0}", searcher.m_Results.Length);
			foreach (var e in searcher.m_Results)
			{
				var entities = GetSubEntities(e);
				Mod.Log.InfoFormat("sub-entities: {0}", entities.Length);

				for (var i = 0; i < entities.Length; i++)
				{
					EntityManager.AddComponent<Game.Common.Updated>(entities[i]);
					EntityManager.AddComponent<Game.Common.BatchesUpdated>(entities[i]);
				}
			}
		}

		//private void UpdateTerrain(Bounds3 area = default)
		//{
		//	area.min = math.min(area.min, bounds.min - TERRAIN_UPDATE_MARGIN);
		//	area.max = math.max(area.max, bounds.max + TERRAIN_UPDATE_MARGIN);

		//	//Overlays.DebugBounds.Factory(area, Overlays.Overlay.DEBUG_TTL, new(0.1f, 0.1f, 0.8f, 0.6f));

		//	SetUpdateAreaField(area);
		//}

		//private void SetUpdateAreaField(Bounds3 bounds)
		//{
		//	if (bounds.min.x == bounds.max.x || bounds.min.z == bounds.max.z)
		//		return;
		//	float4 area = new(bounds.min.x, bounds.min.z, bounds.max.x, bounds.max.z);
		//	FieldInfo field = m_TerrainSystem.GetType().GetField("m_UpdateArea", BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new Exception("Failed to find TerrainSystem.m_UpdateArea");
		//	field.SetValue(m_TerrainSystem, area);
		//}

		private NativeArray<Entity> GetSubEntities(Entity e)
		{
			var entities = IterateSubEntities(e, e, 0, Identity.None, Identity.None);

			return entities;
		}

		private NativeArray<Entity> IterateSubEntities(Entity top, Entity e, int depth, Identity identity, Identity parentIdentity)
		{
			if (depth > 3)
			{
				throw new Exception($"Moveable.IterateSubEntities depth ({depth}) too deep for {top.D()}/{e.D()}");
			}

			depth++;

			NativeList<Entity> entities = new(Allocator.Temp);

			// Handle Control Points, Segments, and Netlanes
			if (identity is Identity.ControlPoint or Identity.Segment or Identity.NetLane)
			{
				// Do nothing
			}

			// Handle Nodes
			else if (identity == Identity.Node)
			{
				if (EntityManager.TryGetBuffer(e, true, out DynamicBuffer<Game.Objects.SubObject> buffer))
				{
					for (var i = 0; i < buffer.Length; i++)
					{
						var sub = buffer[i].m_SubObject;
						if (!entities.Contains(sub) && IsValidChild(parentIdentity, sub))
						{
							entities.Add(sub);
							entities.AddRange(IterateSubEntities(top, sub, depth, Identity.None, parentIdentity));
						}
					}
				}
			}

			// Handle everything else
			else
			{
				if (EntityManager.TryGetBuffer(e, true, out DynamicBuffer<Game.Areas.SubArea> buffer1))
				{
					for (var i = 0; i < buffer1.Length; i++)
					{
						var sub = buffer1[i].m_Area;
						if (!entities.Contains(sub) && IsValidChild(parentIdentity, sub))
						{
							entities.Add(sub);
							entities.AddRange(IterateSubEntities(top, sub, depth, Identity.None, parentIdentity));
						}
					}
				}

				if (EntityManager.TryGetBuffer(e, true, out DynamicBuffer<Game.Net.SubNet> buffer2))
				{
					for (var i = 0; i < buffer2.Length; i++)
					{
						var sub = buffer2[i].m_SubNet;
						if (!entities.Contains(sub) && IsValidChild(parentIdentity, sub))
						{
							entities.Add(sub);
							entities.AddRange(IterateSubEntities(top, sub, depth, Identity.None, parentIdentity));
						}
					}
				}

				if (EntityManager.TryGetBuffer(e, true, out DynamicBuffer<Game.Net.SubLane> buffer3))
				{
					for (var i = 0; i < buffer3.Length; i++)
					{
						var sub = buffer3[i].m_SubLane;
						if (!entities.Contains(sub) && IsValidChild(parentIdentity, sub))
						{
							entities.Add(sub);
							entities.AddRange(IterateSubEntities(top, sub, depth, Identity.None, parentIdentity));
						}
					}
				}

				//        //new() { tParentComponent = typeof(Game.Buildings.InstalledUpgrade), m_FieldInfo = GetEntityReferenceField(typeof(Game.Buildings.InstalledUpgrade)) },
				//        //new() { tParentComponent = typeof(Game.Objects.SubObject),  m_FieldInfo = GetEntityReferenceField(typeof(Game.Objects.SubObject)) },
			}

			return entities.ToArray(Allocator.Temp);
		}

		private bool IsValidChild(Identity parentIdentity, Entity e)
		{
			switch (parentIdentity)
			{
				case Identity.Node:
					var EM = World.DefaultGameObjectInjectionWorld.EntityManager;
					if (EM.HasComponent<Game.Objects.Attached>(e))
					{
						return true;
					}

					return false;

				default:
					return true;
			}
		}
	}
	public enum Identity
	{
		None,
		Building,
		ControlPoint,
		Node,
		Other,
		Plant,
		Segment,
		NetLane,
		Extension,
		ServiceUpgrade,
		Prop,
		Decal,
		Invalid
	}
}
