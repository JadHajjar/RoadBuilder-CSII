using Colossal.Collections;
using Colossal.Mathematics;

using System.Linq;

using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace RoadBuilder.Utilities.Searcher
{
	//#if USE_BURST
	//    [BurstCompile]
	//#endif
	internal struct SearcherJob : IJob
	{
		internal NativeQuadTree<Entity, Game.Common.QuadTreeBoundsXZ> m_StaticTree;
		internal NativeQuadTree<Entity, Game.Common.QuadTreeBoundsXZ> m_NetworkTree;
		internal NativeQuadTree<Game.Areas.AreaSearchItem, Game.Common.QuadTreeBoundsXZ> m_SurfaceTree;
		internal Filters m_Filters;
		internal bool m_IsManipulating;
		internal QLookup m_Lookup;
		internal EntityManager m_Manager;
		internal NativeList<Entity> m_Results;
		internal float3 m_TerrainPosition;

		internal SearchTypes m_SearchType;
		internal Quad2 m_SearchRect;            // For SearchTypes.Marquee
		internal Bounds2 m_SearchOuterBounds;   // For SearchTypes.Marquee, .Bounds
		internal float3 m_SearchPoint;          // For SearchTypes.Point
		internal Line3.Segment m_SearchRay;     // For SearchTypes.Ray

		public void Execute()
		{
			// Static objects
			if ((m_Filters & Filters.AllStatics) != Filters.None)
			{
				SearcherIterator iterator = default;
				iterator.m_EntityList = new(Allocator.Temp);
				iterator.m_Lookup = m_Lookup;
				iterator.m_Manager = m_Manager;

				iterator.m_Type = m_SearchType;
				iterator.m_SearchRect = m_SearchRect;
				iterator.m_SearchOuterBounds = m_SearchOuterBounds;
				iterator.m_SearchPoint = default;
				iterator.m_SearchRay = m_SearchRay;

				m_StaticTree.Iterate(ref iterator);

				for (var i = 0; i < iterator.m_EntityList.Length; i++)
				{
					var e = iterator.m_EntityList[i];
					if (!m_Results.Contains(e) && FilterStatic(e))
					{
						m_Results.Add(e);
					}
				}
				iterator.Dispose();
			}

			// Networks
			if ((m_Filters & Filters.AllNets) != Filters.None)
			{
				SearcherIterator iterator = default;
				iterator.m_EntityList = new(Allocator.Temp);
				iterator.m_Lookup = m_Lookup;
				iterator.m_Manager = m_Manager;

				iterator.m_Type = m_SearchType;
				iterator.m_SearchRect = m_SearchRect;
				iterator.m_SearchOuterBounds = m_SearchOuterBounds;
				iterator.m_SearchPoint = default;
				iterator.m_SearchRay = m_SearchRay;

				m_NetworkTree.Iterate(ref iterator);

				for (var i = 0; i < iterator.m_EntityList.Length; i++)
				{
					if ((m_Filters & Filters.Segments) == 0 && m_Manager.HasComponent<Game.Net.Edge>(iterator.m_EntityList[i]))
						continue;
					if ((m_Filters & Filters.Nodes) == 0 && m_Manager.HasComponent<Game.Net.Node>(iterator.m_EntityList[i]))
						continue;

					if (!m_Results.Contains(iterator.m_EntityList[i]))
					{
						m_Results.Add(iterator.m_EntityList[i]);
					}
				}
				iterator.Dispose();
			}

			// Surfaces
			if ((m_Filters & Filters.Surfaces) != Filters.None)
			{
				SearcherIterator iterator = default;
				iterator.m_EntityList = new(Allocator.Temp);
				iterator.m_Lookup = m_Lookup;
				iterator.m_Manager = m_Manager;

				iterator.m_Type = m_SearchType;
				iterator.m_SearchRect = m_SearchRect;
				iterator.m_SearchOuterBounds = m_SearchOuterBounds;
				iterator.m_SearchPoint = default;
				iterator.m_SearchRay = m_SearchRay;

				m_SurfaceTree.Iterate(ref iterator);

				for (var i = 0; i < iterator.m_EntityList.Length; i++)
				{
					var e = iterator.m_EntityList[i];
					if (!m_Results.Contains(e) && FilterStatic(e))
					{
						m_Results.Add(e);
					}
				}
				iterator.Dispose();
			}
		}


		private readonly bool FilterStatic(Entity e)
		{
			if ((m_Filters & Filters.AllStatics) == 0)
				return false; // Not looking for a static
			if ((m_Filters & Filters.AllStatics) == Filters.AllStatics)
				return true; // Looking for any static

			if ((m_Filters & Filters.Buildings) != 0)
			{
				if (HasOr<Game.Buildings.Building, Game.Buildings.Extension>(e))
				{
					return true;
				}
			}

			if ((m_Filters & Filters.Plants) != 0)
			{
				if (Has<Game.Objects.Plant>(e))
				{
					return true;
				}
			}

			if ((m_Filters & Filters.Props) != 0)
			{
				if (HasOr<Game.Objects.ObjectGeometry, Game.Objects.Surface>(e))
				{
					return true;
				}
			}

			if ((m_Filters & Filters.Decals) != 0)
			{
				if (Has<Game.Objects.ObjectGeometry>(e) && !Has<Game.Objects.Surface>(e))
				{
					return true;
				}
			}

			if ((m_Filters & Filters.Surfaces) != 0)
			{
				if (Has<Game.Areas.Area>(e) && Has<Game.Areas.Surface>(e))
				{
					return true;
				}
			}

			return false;
		}

		private readonly bool Has<T>(Entity e) where T : IComponentData
			=> m_Manager.HasComponent<T>(e);
		private readonly bool HasOr<T1, T2>(Entity e) where T1 : IComponentData where T2 : IComponentData
			=> m_Manager.HasComponent<T1>(e) || m_Manager.HasComponent<T2>(e);
	}
}
