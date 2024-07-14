﻿using RoadBuilder.Utilities.Searcher;

using Unity.Mathematics;

using UnityEngine;

namespace MoveIt.QAccessor
{
	internal partial struct QEntity
	{
		private readonly float3 Lane_Position => BezierPosition(Curve);

		private readonly float Lane_Angle
		{
			get
			{
				var mag = Curve.d - Curve.a;
				return math.atan2(mag.z, mag.x) * Mathf.Rad2Deg;
			}
		}

		private readonly quaternion Lane_Rotation => quaternion.EulerXYZ(0f, Angle, 0f);


		private bool Lane_SetUpdated()
		{
			TryAddUpdate(m_Entity);

			var edge = m_Lookup.gnEdge.GetRefRO(m_Entity).ValueRO;
			QEntity node = new(ref m_Lookup, edge.m_Start, Identity.Node);
			node.Node_SetUpdated();
			node = new(ref m_Lookup, edge.m_End, Identity.Node);
			node.Node_SetUpdated();

			if (TryGetComponent<Game.Net.Aggregated>(out var component))
			{
				var aggregate = component.m_Aggregate;
				TryAddUpdate(aggregate);
			}

			return true;
		}
	}
}
