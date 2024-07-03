using Game.Prefabs;

using RoadBuilder.Domain.Configuration;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoadBuilder.Utilities
{
	public static class RoadConfigExtensionsUtil
	{
		public static bool IsOneWay(this INetworkConfig config)
		{
			return config.Lanes.All(x => !x.Invert);
		}

		public static float CalculateWidth(this NetSectionPrefab netSection)
		{
			return netSection.m_SubSections.Sum(x => x.m_RequireAll.Length == 0 && x.m_RequireAny.Length == 0 ? x.m_Section.CalculateWidth() : 0f)
				+ netSection.m_Pieces.Max(x => x.m_RequireAll.Length == 0 && x.m_RequireAny.Length == 0 ? x.m_Piece.m_Width : 0f);
		}
	}
}
