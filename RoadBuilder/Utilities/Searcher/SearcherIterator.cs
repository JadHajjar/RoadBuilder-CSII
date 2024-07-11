using Colossal.Collections;
using Colossal.Entities;
using Colossal.Mathematics;

using Game.Areas;

using RoadBuilder.Utilities.Searcher.QAccessor;

using System;

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace RoadBuilder.Utilities.Searcher
{
	internal struct SearcherIterator : IDisposable,
		INativeQuadTreeIterator<Entity, Game.Common.QuadTreeBoundsXZ>, IUnsafeQuadTreeIterator<Entity, Game.Common.QuadTreeBoundsXZ>,
		INativeQuadTreeIterator<AreaSearchItem, Game.Common.QuadTreeBoundsXZ>, IUnsafeQuadTreeIterator<AreaSearchItem, Game.Common.QuadTreeBoundsXZ>
	{
		public NativeList<Entity> m_EntityList;
		public QLookup m_Lookup;
		public EntityManager m_Manager;

		public SearchTypes m_Type;
		public Quad2 m_SearchRect;              // For SearchTypes.Marquee
		public Bounds2 m_SearchOuterBounds;     // For SearchTypes.Marquee, .Bounds
		public float2 m_SearchPoint;            // For SearchTypes.Point
		public Line3.Segment m_SearchRay;       // For SearchTypes.Ray

		/// <summary>
		/// Does the given <see cref="bounds">bounds</see> intersect the search area?
		/// </summary>
		/// <param name="bounds">The QuadTree bounds</param>
		/// <returns>Does bounds intersect search area?</returns>
		/// <exception cref="NotImplementedException"></exception>
		public readonly bool Intersect(Game.Common.QuadTreeBoundsXZ bounds)
		{
			var b = bounds.m_Bounds.xz;
			bool result = m_Type switch
			{
				SearchTypes.Marquee => MathUtils.Intersect(b, m_SearchRect),
				SearchTypes.Bounds => MathUtils.Intersect(b, m_SearchOuterBounds),
				SearchTypes.Point => MathUtils.Intersect(b, m_SearchPoint),
				_ => throw new NotImplementedException(),
			};

			//QIntersect.DoesLineIntersectBounds3(m_SearchRay, bounds.m_Bounds, out float2 xsect);
			//QLog.Debug($"Xsect {b.min.D(),18}::{b.max.D(),-18} {bounds.m_Bounds.y.min,8}::{bounds.m_Bounds.y.max,-8} = {result,-5}  {xsect.D()}");

			return result;
		}

		/// <summary>
		/// Check if the <see cref="e">entity's</see> <see cref="bounds">bounding box</see> intersects the search area
		/// </summary>
		/// <param name="bounds">The entity's bounds</param>
		/// <param name="e">The entity to check and, if valid, add to result</param>
		public void Iterate(Game.Common.QuadTreeBoundsXZ bounds, Entity e)
		{
			if (m_Type == SearchTypes.Marquee || m_Type == SearchTypes.Bounds)
			{
				if (!MathUtils.Intersect(m_SearchOuterBounds, bounds.m_Bounds.xz))
					return;
			}

			QObjectSimple obj = new(m_Manager, ref m_Lookup, e);

			var prefab = m_Manager.GetComponentData<Game.Prefabs.PrefabRef>(e).m_Prefab;

			// Building
			if (obj.m_Identity == Identity.Building || obj.m_Identity == Identity.Extension)
			{
				var objRect = Utils.CalculateBuildingCorners(m_Manager, ref obj, prefab);

				switch (m_Type)
				{
					case SearchTypes.Marquee:
						if (MathUtils.Intersect(objRect, m_SearchRect))
						{
							m_EntityList.Add(e);
							return;
						}
						break;

					case SearchTypes.Bounds:
						if (MathUtils.Intersect(m_SearchOuterBounds, objRect))
						{
							m_EntityList.Add(e);
							return;
						}
						break;

					case SearchTypes.Point:
						if (MathUtils.Intersect(objRect, m_SearchPoint))
						{
							m_EntityList.Add(e);
							return;
						}
						break;

					default:
						break;
				}

				return;
			}


			// Node
			if (obj.m_Identity == Identity.Node)
			{
				if (!m_Manager.TryGetComponent(e, out Game.Net.Node node))
					return;

				var circle = Utils.GetCircle(m_Manager, e, node);

				switch (m_Type)
				{
					case SearchTypes.Marquee:
						if (MathUtils.Intersect(m_SearchRect, circle))
						{
							m_EntityList.Add(e);
							return;
						}
						break;

					case SearchTypes.Bounds:
						if (MathUtils.Intersect(m_SearchOuterBounds, circle))
						{
							m_EntityList.Add(e);
							return;
						}
						break;

					case SearchTypes.Point:
						if (MathUtils.Intersect(circle, m_SearchPoint))
						{
							m_EntityList.Add(e);
							return;
						}
						break;

					default:
						break;
				}
				return;
			}


			var objBounds = bounds.m_Bounds.xz;

			// A circular object
			if (m_Manager.TryGetComponent<Game.Prefabs.ObjectGeometryData>(prefab, out var objGeoData))
			{
				if ((objGeoData.m_Flags & Game.Objects.GeometryFlags.Circular) > 0)
				{
					var pos = objBounds.Center();
					var radius = math.max(objGeoData.m_Size.x, objGeoData.m_Size.z) / 2;
					Circle2 circle = new(radius, pos);

					switch (m_Type)
					{
						case SearchTypes.Marquee:
							if (MathUtils.Intersect(m_SearchRect, circle))
							{
								m_EntityList.Add(e);
								return;
							}
							break;

						case SearchTypes.Bounds:
							if (MathUtils.Intersect(m_SearchOuterBounds, circle))
							{
								m_EntityList.Add(e);
								return;
							}
							break;

						case SearchTypes.Point:
							if (MathUtils.Intersect(circle, m_SearchPoint))
							{
								m_EntityList.Add(e);
								return;
							}
							break;

						default:
							break;
					}

					return;
				}
			}


			// Any other object
			switch (m_Type)
			{
				case SearchTypes.Marquee:
					if (MathUtils.Intersect(objBounds, m_SearchRect))
					{
						m_EntityList.Add(e);
						return;
					}
					break;

				case SearchTypes.Bounds:
					if (MathUtils.Intersect(m_SearchOuterBounds, objBounds))
					{
						m_EntityList.Add(e);
						return;
					}
					break;

				case SearchTypes.Point:
					if (MathUtils.Intersect(objBounds, m_SearchPoint))
					{
						m_EntityList.Add(e);
						return;
					}
					break;

				default:
					break;
			}
		}

		/// <summary>
		/// Check if the <see cref="areaSearchItem">area search item's</see> <see cref="bounds">bounding box</see> intersects the search area
		/// </summary>
		/// <param name="bounds">The entity's bounds</param>
		/// <param name="areaSearchItem">The entity to check and, if valid, add to result</param>
		public void Iterate(Game.Common.QuadTreeBoundsXZ bounds, AreaSearchItem areaSearchItem)
		{
			var e = areaSearchItem.m_Area;
			if (m_EntityList.Contains(e))
				return;
			if (m_Manager.HasComponent<Game.Common.Owner>(e))
				return;
			if (!m_Manager.HasComponent<Surface>(e))
				return;

			if (m_Manager.TryGetBuffer<Node>(e, true, out var nodes) && m_Manager.TryGetBuffer<Triangle>(e, true, out var triangles))
			{
				var tri = AreaUtils.GetTriangle3(nodes, triangles[areaSearchItem.m_Triangle]);
				m_EntityList.Add(e);
			}
		}

		public void Dispose()
		{
			m_EntityList.Dispose();
		}

		public static Identity GetEntityIdentity(EntityManager manager, Entity e)
		{
			if (e.Equals(Entity.Null) ||
				manager.HasComponent<Game.Common.Deleted>(e) ||
				manager.HasComponent<Game.Common.Terrain>(e))
			{
				return Identity.Invalid;
			}

			if (manager.HasComponent<Game.Objects.Plant>(e))
			{
				return Identity.Plant;
			}
			else if (manager.HasComponent<Game.Buildings.Extension>(e))
			{
				return Identity.Extension;
			}
			else if (manager.HasComponent<Game.Buildings.ServiceUpgrade>(e))
			{
				return Identity.ServiceUpgrade;
			}
			else if (manager.HasComponent<Game.Buildings.Building>(e))
			{
				return Identity.Building;
			}
			else if (manager.HasComponent<Game.Net.Edge>(e))
			{
				if (manager.HasComponent<Game.Net.EdgeGeometry>(e))
				{
					return Identity.Segment;
				}
				else
				{
					return Identity.NetLane;
				}
			}
			else if (manager.HasComponent<Game.Net.Node>(e))
			{
				return Identity.Node;
			}
			else if (manager.HasComponent<Game.Objects.ObjectGeometry>(e))
			{
				if (manager.HasComponent<Game.Objects.Surface>(e))
				{
					return Identity.Prop;
				}
				else
				{
					return Identity.Decal;
				}
			}
			else
			{
				return Identity.Other;
			}
		}
	}
}
