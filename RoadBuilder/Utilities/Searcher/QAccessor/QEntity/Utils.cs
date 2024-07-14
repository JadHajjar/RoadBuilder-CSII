using Colossal.Mathematics;

using Unity.Entities;
using Unity.Mathematics;

namespace MoveIt.QAccessor
{
	internal partial struct QEntity
	{
		internal enum ID
		{
			Generic,
			Seg,
			Lane,
			CP,
			Node,
		}


		private readonly Bezier4x3 Curve => m_Lookup.gnCurve.GetRefRO(m_Entity).ValueRO.m_Bezier;

		private readonly void TryAddUpdate(Entity e)
		{
			if (!Manager.HasComponent<Game.Common.Updated>(e))
			{
				Manager.AddComponent<Game.Common.Updated>(e);
			}

			if (!Manager.HasComponent<Game.Common.BatchesUpdated>(e))
			{
				Manager.AddComponent<Game.Common.BatchesUpdated>(e);
			}
		}


		private static Bounds3 MoveBounds3(Bounds3 input, float3 delta)
		{
			input.min += delta;
			input.max += delta;
			return input;
		}

		private static float3 BezierPosition(Bezier4x3 bezier)
		{
			var total = bezier.b + bezier.c;
			return total / 2;
		}


		#region Simple entity access

		public readonly bool TryGetComponent<T>(out T component) where T : unmanaged, IComponentData
		{
			if (!Manager.HasComponent<T>(m_Entity))
			{
				component = default;
				return false;
			}

			component = Manager.GetComponentData<T>(m_Entity);
			return true;
		}

		public readonly bool TryGetBuffer<T>(out DynamicBuffer<T> buffer, bool isReadOnly = false) where T : unmanaged, IBufferElementData
		{
			if (!Manager.HasBuffer<T>(m_Entity))
			{
				buffer = default;
				return false;
			}

			buffer = Manager.GetBuffer<T>(m_Entity, isReadOnly);
			return true;
		}

		#endregion
	}
}
