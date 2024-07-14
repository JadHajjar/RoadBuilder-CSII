using MoveIt.QAccessor;

using System;

using Unity.Entities;

namespace RoadBuilder.Utilities.Searcher.QAccessor
{
	/// <summary>
	/// Additional accessor for entities, does not include children
	/// </summary>
	public struct QObjectSimple : IDisposable
	{
		internal EntityManager m_Manager;
		public Entity m_Entity;
		internal QEntity m_Parent;
		internal Identity m_Identity;

		internal QObjectSimple(EntityManager manager, ref QLookup lookup, Entity e)
		{
			if (e == Entity.Null)
			{
				throw new ArgumentNullException("Creating QObject with null entity");
			}

			m_Manager = manager;
			m_Entity = e;
			m_Identity = SearcherIterator.GetEntityIdentity(manager, e);
			m_Parent = new(ref lookup, e, m_Identity);

			//DebugDumpFullObject();
		}

		public readonly bool TryGetComponent<T>(out T component) where T : unmanaged, IComponentData
		{
			return m_Parent.TryGetComponent(out component);
		}

		public readonly bool TryGetBuffer<T>(out DynamicBuffer<T> buffer, bool isReadOnly = false) where T : unmanaged, IBufferElementData
		{
			return m_Parent.TryGetBuffer(out buffer, isReadOnly);
		}

		public readonly void Dispose() { }


		public override readonly string ToString()
		{
			return $"{m_Identity}/{m_Entity.D()}";
		}
	}
}
