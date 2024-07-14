using Colossal.Mathematics;

using RoadBuilder.Systems;

using System;

using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace RoadBuilder.Utilities.Searcher
{
	internal class Searcher : IDisposable, INativeDisposable
	{
		protected QLookup _Lookup;

		internal Filters m_Filters;
		internal bool m_IsManipulating;
		internal NativeList<Entity> m_Results;
		internal float3 m_TerrainPosition;

		internal Searcher(Filters filters, bool isManipulating, float3 terrainPosition)
		{
			m_Filters = filters;
			m_IsManipulating = isManipulating;
			m_TerrainPosition = terrainPosition;
			QLookupFactory.Init(World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<RoadBuilderSystem>());
			_Lookup = QLookupFactory.Get();
		}

		internal static Game.Objects.SearchSystem ObjSearch
		{
			get
			{
				_ObjSearch ??= World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<Game.Objects.SearchSystem>();
				return _ObjSearch;
			}
		}
		private static Game.Objects.SearchSystem _ObjSearch;

		internal static Game.Net.SearchSystem NetSearch
		{
			get
			{
				_NetSearch ??= World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<Game.Net.SearchSystem>();
				return _NetSearch;
			}
		}
		private static Game.Net.SearchSystem _NetSearch;

		internal static Game.Areas.SearchSystem AreaSearch
		{
			get
			{
				_AreaSearch ??= World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<Game.Areas.SearchSystem>();
				return _AreaSearch;
			}
		}
		private static Game.Areas.SearchSystem _AreaSearch;


		/// <summary>
		/// Run the search for marquee
		/// </summary>
		/// <param name="outer">A map-align rectangle for quickly exluding irrelevant condidates</param>
		/// <param name="rect">The area to search within</param>
		internal void SearchMarquee(Bounds2 outer, Quad2 rect)
		{
			Execute(SearchTypes.Marquee, outer, rect, default, default);
		}

		/// <summary>
		/// Run the search for bounds rectangle
		/// </summary>
		/// <param name="outer">A map-align rectangle to search</param>
		internal void SearchBounds(Bounds2 outer)
		{
			Execute(SearchTypes.Bounds, outer, default, default, default);
		}

		/// <summary>
		/// Run the search for a single point
		/// </summary>
		/// <param name="point">A single point on the map</param>
		internal void SearchPoint(float3 point)
		{
			Execute(SearchTypes.Point, default, default, point, default);
		}

		private void Execute(SearchTypes type, Bounds2 outer, Quad2 rect, float3 point, Line3.Segment ray)
		{
			try
			{
				var staticTree = ObjSearch.GetStaticSearchTree(true, out var objSearchTreeHandle);
				var networkTree = NetSearch.GetNetSearchTree(true, out var netSearchTreeHandle);
				var surfaceTree = AreaSearch.GetSearchTree(true, out var surSearchTreeHandle);
				objSearchTreeHandle.Complete();
				netSearchTreeHandle.Complete();
				surSearchTreeHandle.Complete();

				m_Results = new NativeList<Entity>(0, Allocator.TempJob);

				var searchHandle = JobHandle.CombineDependencies(objSearchTreeHandle, netSearchTreeHandle);
				SearcherJob job = new()
				{
					m_StaticTree = staticTree,
					m_NetworkTree = networkTree,
					m_SurfaceTree = surfaceTree,
					m_Filters = m_Filters,
					m_IsManipulating = false,
					m_Lookup = _Lookup,
					m_Manager = World.DefaultGameObjectInjectionWorld.EntityManager,
					m_Results = m_Results,
					m_TerrainPosition = m_TerrainPosition,
					m_SearchType = type,
					m_SearchRect = rect,
					m_SearchOuterBounds = outer,
					m_SearchPoint = point,
					m_SearchRay = ray,
				};
				job.Run();
			}
			catch (Exception ex)
			{
			}
		}


		public virtual void Dispose()
		{
			m_Results.Dispose();
		}

		public virtual JobHandle Dispose(JobHandle handle)
		{
			return m_Results.Dispose(handle);
		}

		~Searcher()
		{
			Dispose();
		}
	}
}
