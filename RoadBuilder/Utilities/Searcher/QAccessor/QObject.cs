using Colossal.Entities;

using MoveIt.QAccessor;

using System;
using System.Text;

using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

using UnityEngine;

namespace RoadBuilder.Utilities.Searcher.QAccessor
{
	/// <summary>
	/// Primary accessor for entities, including children
	/// </summary>
	public struct QObject : IDisposable, INativeDisposable
	{
		internal static EntityManager m_Manager;
		internal Entity m_Entity;
		internal QEntity m_Parent;
		internal NativeList<QEntity> m_Children;
		internal Identity m_Identity;

		internal QObject(EntityManager manager, ref QLookup lookup, Entity e, Identity identity = Identity.None)
		{
			if (e == Entity.Null)
				throw new ArgumentNullException("Creating QObject with null entity");

			m_Manager = manager;
			m_Entity = e;
			m_Identity = identity == Identity.None ? SearcherIterator.GetEntityIdentity(manager, e) : identity;
			m_Parent = new(ref lookup, e, m_Identity);
			m_Children = new(0, Allocator.Persistent);

			var subEntities = GetSubEntities(e, m_Identity);

			if (subEntities.Length > 0)
			{
				for (var i = 0; i < subEntities.Length; i++)
				{
					if (subEntities[i] == Entity.Null)
						throw new NullReferenceException($"Creating child for {e.D()} with null entity");

					if (m_Manager.HasComponent<Game.Net.ConnectionLane>(subEntities[i]))
						continue;

					Identity subType = SearcherIterator.GetEntityIdentity(manager, subEntities[i]);
					m_Children.Add(new(ref lookup, subEntities[i], subType, m_Entity));
				}
			}

			//DebugDumpFullObject(new(Allocator.Temp) { (int)Identity.Node, (int)Identity.Segment, (int)Identity.NetLane }, true, $"QObject.Ctor {m_Entity.D()}: ");
		}

		public void Dispose()
		{
			m_Children.Dispose();
		}

		public JobHandle Dispose(JobHandle handle)
		{
			handle = m_Children.Dispose(handle);
			return handle;
		}

		#region Transforming

		public readonly int UpdateAll()
		{
			//m_Manager.AddComponent<Game.Common.Updated>(m_Entity);
			//m_Manager.AddComponent<Game.Common.BatchesUpdated>(m_Entity);

			foreach (var child in m_Children)
			{
				m_Manager.AddComponent<Game.Common.Updated>(child.m_Entity);
				m_Manager.AddComponent<Game.Common.BatchesUpdated>(child.m_Entity);
			}

			m_Parent.SetUpdated();

			return m_Children.Length + 1;
		}

		private readonly void GetMatrix(float delta, float3 origin, out Matrix4x4 matrix)
		{
			matrix = default;
			matrix.SetTRS(origin, Quaternion.Euler(0f, delta, 0f), Vector3.one);
		}
		#endregion

		#region Load children
		private static NativeArray<Entity> GetSubEntities(Entity e, Identity identity)
		{
			var entities = IterateSubEntities(e, e, 0, identity, identity);

			return entities;
		}

		private static NativeArray<Entity> IterateSubEntities(Entity top, Entity e, int depth, Identity identity, Identity parentIdentity)
		{
			if (depth > 3)
				throw new Exception($"Moveable.IterateSubEntities depth ({depth}) too deep for {top.D()}/{e.D()}");
			depth++;

			NativeList<Entity> entities = new(Allocator.Temp);

			// Handle Control Points, Segments, and Netlanes
			if (identity == Identity.ControlPoint || identity == Identity.Segment || identity == Identity.NetLane)
			{
				// Do nothing
			}

			// Handle Nodes
			else if (identity == Identity.Node)
			{
				if (m_Manager.TryGetBuffer(e, true, out DynamicBuffer<Game.Objects.SubObject> buffer))
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
				if (m_Manager.TryGetBuffer(e, true, out DynamicBuffer<Game.Areas.SubArea> buffer1))
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

				if (m_Manager.TryGetBuffer(e, true, out DynamicBuffer<Game.Net.SubNet> buffer2))
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

				if (m_Manager.TryGetBuffer(e, true, out DynamicBuffer<Game.Net.SubLane> buffer3))
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

		private static bool IsValidChild(Identity parentIdentity, Entity e)
		{
			switch (parentIdentity)
			{
				case Identity.Node:
					var EM = World.DefaultGameObjectInjectionWorld.EntityManager;
					if (EM.HasComponent<Game.Objects.Attached>(e))
						return true;
					return false;

				default:
					return true;
			}
		}
		#endregion

		public readonly override string ToString()
		{
			return $"{m_Identity}/{m_Entity.D()}  Children: {(m_Children.IsCreated ? m_Children.Length : "Not Created!")}";
		}
	}
}
