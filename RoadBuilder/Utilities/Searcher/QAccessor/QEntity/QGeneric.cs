using RoadBuilder.Utilities.Searcher;

using Unity.Mathematics;

namespace MoveIt.QAccessor
{
	internal partial struct QEntity
	{
		private readonly float3 Generic_Position
		{
			get
			{
				//try
				{
					//StringBuilder sb = new($"Pos.GET " + m_Entity.D() + ": ");
					float3 result;

					if (m_Lookup.goTransform.HasComponent(m_Entity))
					{
						//sb.Append($"goTransform");
						result = m_Lookup.goTransform.GetRefRO(m_Entity).ValueRO.m_Position;
					}
					//else if (m_Lookup.gnNode.HasComponent(m_Entity))
					//{
					//    sb.Append($"gnNode");
					//    result = m_Lookup.gnNode.GetRefRO(m_Entity).ValueRO.m_Position;
					//}

					else
					{
						//sb.Append($"notFound");
						result = float3.zero;
					}

					//sb.AppendFormat(" ({0})", result.DX());

					//QLog.Bundle("GET", sb.ToString());

					return result;
				}
				//catch (Exception ex)
				//{
				//    bool exists = Manager.Exists(m_Entity);
				//    string has = " (detail failed)";
				//    try
				//    {
				//        if (exists)
				//        {
				//            has = " (";
				//            if (m_Lookup.goTransform.HasComponent(m_Entity)) has += "goTransform";
				//            else if (m_Lookup.gnNode.HasComponent(m_Entity)) has += "gnNode";
				//            else has += "notFound";
				//            has += ")";
				//        }
				//    }
				//    catch { }
				//    MIT.Log.Error($"Position.Get failed for {m_Entity.D()} (exists:{exists}){has}\n{ex}");

				//    return default;
				//}
			}
		}

		private readonly float Generic_Angle => Rotation.Y();

		private readonly quaternion Generic_Rotation => m_Lookup.goTransform.HasComponent(m_Entity) ? m_Lookup.goTransform.GetRefRO(m_Entity).ValueRO.m_Rotation : quaternion.identity;


		private readonly bool Generic_SetUpdated()
		{
			TryAddUpdate(m_Entity);
			return true;
		}
	}
}
