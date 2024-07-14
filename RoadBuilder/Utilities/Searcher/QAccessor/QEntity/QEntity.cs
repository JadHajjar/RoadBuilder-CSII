using RoadBuilder.Utilities.Searcher;

using Unity.Entities;
using Unity.Mathematics;

namespace MoveIt.QAccessor
{
	internal partial struct QEntity
	{
		private readonly EntityManager Manager => World.DefaultGameObjectInjectionWorld.EntityManager;

		internal Entity m_Entity;
		internal Entity m_Parent;
		internal Identity m_Identity;
		internal ID m_ID;
		internal QLookup m_Lookup;

		internal QEntity(ref QLookup lookup, Entity e, Identity identity, Entity parent = default)
		{
			m_Lookup = lookup;
			m_Entity = e;
			m_Identity = identity;
			m_Parent = parent;

			m_ID = identity switch
			{
				Identity.Segment => parent == Entity.Null ? ID.Seg : ID.Lane,
				Identity.NetLane => ID.Lane,
				Identity.Node => ID.Node,
				Identity.ControlPoint => ID.CP,
				_ => ID.Generic,
			};
		}

		internal readonly float3 Position => m_ID switch
		{
			ID.Seg => Segment_Position,
			ID.Lane => Lane_Position,
			ID.Node => Node_Position,
			_ => Generic_Position,
		};

		internal readonly float Angle => m_ID switch
		{
			ID.Seg => Segment_Angle,
			ID.Lane => Lane_Angle,
			ID.Node => Node_Angle,
			_ => Generic_Angle,
		};

		internal readonly quaternion Rotation => m_ID switch
		{
			ID.Seg => Segment_Rotation,
			ID.Lane => Lane_Rotation,
			ID.Node => Node_Rotation,
			_ => Generic_Rotation,
		};


		internal bool SetUpdated()
		{
			return m_ID switch
			{
				ID.Seg => Segment_SetUpdated(),
				ID.Lane => Lane_SetUpdated(),
				ID.Node => Node_SetUpdated(),
				_ => Generic_SetUpdated(),
			};
		}
	}
}
