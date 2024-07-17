using Game.Prefabs;
using RoadBuilder.Domain.Components.Prefabs;
using RoadBuilder.Domain.Configurations;

using System.Linq;

namespace RoadBuilder.Utilities
{
    public static class NetworkConfigExtensionsUtil
	{
		public static bool IsOneWay(this INetworkConfig config)
		{
			return config.Lanes.All(x => !x.Invert);
		}

		public static float CalculateWidth(this NetSectionPrefab netSection)
		{
			var subSectionsWidth = netSection.m_SubSections.Sum(x => x.m_RequireAll.Length == 0 && x.m_RequireAny.Length == 0 ? x.m_Section.CalculateWidth() : 0f);

			if (netSection.m_Pieces.Length == 0)
			{
				return subSectionsWidth;
			}

			return subSectionsWidth + netSection.m_Pieces.Max(x => x.m_RequireAll.Length == 0 && x.m_RequireAny.Length == 0 ? x.m_Piece.m_Width : 0f);
		}

		public static bool MatchCategories(this PrefabBase prefab, INetworkConfig config)
		{
			if (config is null || !prefab.TryGet<RoadBuilderLaneInfo>(out var info))
			{
				return true;
			}

			var matchesRequired = (config.Category & info.RequiredCategories) == info.RequiredCategories;
			var matchesExcluded = (config.Category & info.ExcludedCategories) != 0;

			return matchesRequired && !matchesExcluded;
		}

		public static bool MatchCategories(this RoadBuilderLaneInfo info, INetworkConfig config)
		{
			if (config is null)
			{
				return true;
			}

			var matchesRequired = (config.Category & info.RequiredCategories) == info.RequiredCategories;
			var matchesExcluded = (config.Category & info.ExcludedCategories) != 0;

			return matchesRequired && !matchesExcluded;
		}
	}
}
