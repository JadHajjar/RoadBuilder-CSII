using Game.Prefabs;

using RoadBuilder.Domain.Components.Prefabs;
using RoadBuilder.Domain.Configurations;
using RoadBuilder.Domain.Prefabs;

using System.Collections.Generic;
using System.Linq;

namespace RoadBuilder.Utilities
{
	public static class NetworkConfigExtensionsUtil
	{
		public static bool IsInPlayset(this INetworkConfig config)
		{
			return Mod.Settings!.NoPlaysetIsolation
				|| config.Playsets is null
				|| ((!config.Playsets.Any(x => x > 0)
				|| config.Playsets.Contains(PdxModsUtil.CurrentPlayset))
				&& !config.Playsets.Contains(-PdxModsUtil.CurrentPlayset));
		}

		public static bool GetEdgeLaneInfo(NetSectionPrefab? section, LaneGroupPrefab? groupPrefab, out RoadBuilderEdgeLaneInfo? sectionEdgeInfo)
		{
			if (section != null && section.TryGet(out sectionEdgeInfo))
			{
				return true;
			}

			if (groupPrefab != null && groupPrefab.TryGet(out sectionEdgeInfo))
			{
				return true;
			}

			sectionEdgeInfo = null;
			return false;
		}

		public static bool IsOneWay(this INetworkConfig config)
		{
			if (config.Lanes.Count < 2)
			{
				return true;
			}

			var first = config.Lanes[1].Invert;

			for (var i = 2; i < config.Lanes.Count - 1; i++)
			{
				if (config.Lanes[i].Invert != first)
				{
					return false;
				}
			}

			return true;
		}

		public static float CalculateWidth(this NetSectionPrefab netSection, LaneGroupPrefab? groupPrefab = null)
		{
			var subSectionsWidth = netSection.m_SubSections.Sum(x =>
				x.m_RequireAll.Length == 0 &&
				x.m_RequireAny.Length == 0 ? x.m_Section.CalculateWidth() : 0f);

			if (groupPrefab is not null && (netSection.TryGet<RoadBuilderLaneAggregate>(out var aggregate) || groupPrefab.TryGet(out aggregate)))
			{
				subSectionsWidth += (aggregate?.LeftSections?.Sum(x => x.Section?.CalculateWidth() ?? 0) ?? 0) + (aggregate?.RightSections?.Sum(x => x.Section?.CalculateWidth() ?? 0) ?? 0);
			}

			if (netSection.m_Pieces.Length == 0)
			{
				return subSectionsWidth;
			}

			var pieceWidths = netSection.m_Pieces.Max(x =>
				x.m_RequireAll.Length == 0 &&
				x.m_RequireAny.Length == 0 ? x.m_Piece.m_Width : 0f);

			if (pieceWidths == 0)
			{
				pieceWidths = netSection.m_Pieces[0].m_Piece.m_Width;
			}

			return subSectionsWidth + pieceWidths;
		}

		public static bool IsMedian(this NetSectionPrefab netSection)
		{
			return netSection.m_Pieces.Any(x =>
				x.m_RequireAll.Length == 0 &&
				x.m_RequireAny.Length == 0 &&
				x.m_Piece.TryGet<NetDividerPiece>(out var divider) && divider.m_BlockTraffic);
		}

		public static bool SupportsTwoWay(this NetSectionPrefab netSection, LaneConfig? lane = null, LaneGroupPrefab? groupPrefab = null)
		{
			var carLanes = FindLanes<CarLane>(netSection);
			var trackLanes = FindLanes<TrackLane>(netSection);

			if (carLanes.Any(x => x.m_Twoway) || trackLanes.Any(x => x.m_Twoway))
			{
				return true;
			}

			if (netSection.TryGet<RoadBuilderLaneInfo>(out var laneInfo) && laneInfo.NoDirection)
			{
				return true;
			}

			if (groupPrefab is null || lane is null)
			{
				return false;
			}

			var twoWayOption = groupPrefab.Options.FirstOrDefault(x => x.Type == Domain.Enums.LaneOptionType.TwoWay);

			if (twoWayOption is null)
			{
				return false;
			}

			return LaneOptionsUtil.GetSelectedOptionValue(null, lane, twoWayOption) is not null;
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

		private static TLane? FindLane<TLane>(NetPieceInfo piece) where TLane : ComponentBase
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
			if (config is null || !prefab.TryGet<RoadBuilderLaneInfo>(out var info))
			{
				return true;
			}

			var matchesRequired = (config.Category & info.RequireAll) == info.RequireAll;
			var matchesAny = (config.Category & info.RequireAny) != 0 || info.RequireAny == 0;
			var matchesExcluded = (config.Category & info.RequireNone) != 0;

			return matchesRequired && matchesAny && !matchesExcluded;
		}

		public static bool MatchCategories(this RoadBuilderLaneInfo info, INetworkConfig config)
		{
			if (config is null)
			{
				return true;
			}

			var matchesRequired = (config.Category & info.RequireAll) == info.RequireAll;
			var matchesAny = (config.Category & info.RequireAny) != 0 || info.RequireAny == 0;
			var matchesExcluded = (config.Category & info.RequireNone) != 0;

			return matchesRequired && matchesAny && !matchesExcluded;
		}
	}
}
