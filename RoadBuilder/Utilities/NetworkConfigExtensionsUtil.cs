using Game.Prefabs;

using RoadBuilder.Domain.Components.Prefabs;
using RoadBuilder.Domain.Configurations;

using System.Collections.Generic;
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
			var subSectionsWidth = netSection.m_SubSections.Sum(x => 
				x.m_RequireAll.Length == 0 &&
				x.m_RequireAny.Length == 0 ? x.m_Section.CalculateWidth() : 0f);

			if (netSection.m_Pieces.Length == 0)
			{
				return subSectionsWidth;
			}

			return subSectionsWidth + netSection.m_Pieces.Max(x => 
				x.m_RequireAll.Length == 0 && 
				x.m_RequireAny.Length == 0 ? x.m_Piece.m_Width : 0f);
		}

		public static bool IsMedian(this NetSectionPrefab netSection)
		{
			return netSection.m_Pieces.Any(x => 
				x.m_RequireAll.Length == 0 &&
				x.m_RequireAny.Length == 0 &&
				x.m_Piece.TryGet<NetDividerPiece>(out var divider) && divider.m_BlockTraffic);
		}

		public static bool SupportsTwoWay(this NetSectionPrefab netSection)
		{
			var carLanes = FindLanes<CarLane>(netSection);
			var trackLanes = FindLanes<TrackLane>(netSection);

			return carLanes.Any(x => x.m_Twoway) || trackLanes.Any(x => x.m_Twoway);
		}

		public static bool IsTrainOrSubway(this NetSectionPrefab netSection)
		{
			var trackLanes = FindLanes<TrackLane>(netSection);

			return trackLanes.Any(x => x.m_TrackType is Game.Net.TrackTypes.Train or Game.Net.TrackTypes.Subway);
		}

		public static bool IsBus(this NetSectionPrefab netSection)
		{
			var carLanes = FindLanes<CarLane>(netSection);

			return carLanes.Any(x => x.m_BusLane);
		}

		public static bool IsParking(this NetSectionPrefab netSection, out float angle)
		{
			var parkingLanes = FindLanes<ParkingLane>(netSection);

			if (parkingLanes.Count > 0)
			{
				angle = parkingLanes[0].m_SlotAngle;
				return true;
			}

			angle = default;
			return false;
		}

		public static List<TLane> FindLanes<TLane>(this NetSectionPrefab netSection) where TLane : ComponentBase
		{
			var list = new List<TLane>();

			foreach (var item in netSection.m_Pieces)
			{
				if (FindLane<TLane>(item) is TLane lane)
				{
					list.Add(lane);
				}
			}

			return list;
		}

		private static TLane FindLane<TLane>(NetPieceInfo piece) where TLane : ComponentBase
		{
			if (piece.m_RequireAll.Length > 0 || piece.m_RequireAny.Length > 0)
			{
				return null;
			}

			if (!piece.m_Piece.TryGet<NetPieceLanes>(out var lanes))
			{
				return null;
			}

			foreach (var item in lanes.m_Lanes)
			{
				if (item.m_Lane.TryGet<TLane>(out var lane))
				{
					return lane;
				}
			}

			return null;
		}

		public static bool MatchCategories(this PrefabBase prefab, INetworkConfig config)
		{
			if (Mod.Settings.AdvancedUserMode || config is null || !prefab.TryGet<RoadBuilderLaneInfo>(out var info))
			{
				return true;
			}

			var matchesRequired = (config.Category & info.RequiredCategories) == info.RequiredCategories;
			var matchesAny = (config.Category & info.AnyCategories) != 0 || info.AnyCategories == 0;
			var matchesExcluded = (config.Category & info.ExcludedCategories) != 0;

			return matchesRequired && matchesAny && !matchesExcluded;
		}

		public static bool MatchCategories(this RoadBuilderLaneInfo info, INetworkConfig config)
		{
			if (Mod.Settings.AdvancedUserMode || config is null)
			{
				return true;
			}

			var matchesRequired = (config.Category & info.RequiredCategories) == info.RequiredCategories;
			var matchesAny = (config.Category & info.AnyCategories) != 0 || info.AnyCategories == 0;
			var matchesExcluded = (config.Category & info.ExcludedCategories) != 0;

			return matchesRequired && matchesAny && !matchesExcluded;
		}
	}
}
