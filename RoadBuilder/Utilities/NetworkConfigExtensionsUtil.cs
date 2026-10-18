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
		// Playsets holds the playsets a network is restricted to, and "-{id}" entries for playsets it was removed from.
		// With no positive entries, the network is available in every playset that didn't remove it.
		public static bool IsInPlayset(this INetworkConfig config)
		{
			if (Mod.Settings!.NoPlaysetIsolation || config.Playsets is null || PdxModsUtil.CurrentPlayset is null)
			{
				return true;
			}

			if (config.Playsets.Contains($"-{PdxModsUtil.CurrentPlayset}"))
			{
				return false;
			}

			var positivePlaysets = config.GetPositivePlaysets();

			return positivePlaysets.Count == 0 || positivePlaysets.Contains(PdxModsUtil.CurrentPlayset);
		}

		public static List<string> GetPositivePlaysets(this INetworkConfig config)
		{
			return config.Playsets?.Where(x => !string.IsNullOrEmpty(x) && !x.StartsWith("-")).ToList() ?? new();
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

		// Only sections with car or track lanes have a direction, medians and sidewalks are ignored
		public static bool IsOneWay(this NetGeometryPrefab prefab)
		{
			bool? direction = null;

			foreach (var section in prefab.m_Sections)
			{
				if (section.m_RequireAll.Length != 0 || section.m_RequireAny.Length != 0 || section.m_Section is null)
				{
					continue;
				}

				var carLanes = section.m_Section.FindLanes<CarLane>();
				var trackLanes = section.m_Section.FindLanes<TrackLane>();

				if (carLanes.Count == 0 && trackLanes.Count == 0)
				{
					continue;
				}

				if (carLanes.Any(x => x.m_Twoway) || trackLanes.Any(x => x.m_Twoway))
				{
					return false;
				}

				if (direction is null)
				{
					direction = section.m_Invert;
				}
				else if (direction != section.m_Invert)
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
				// Conditional sections are alternatives to the unconditional ones, only the plain edge layout is counted
				subSectionsWidth += (aggregate?.LeftSections?.Sum(GetAggregateSectionWidth) ?? 0) + (aggregate?.RightSections?.Sum(GetAggregateSectionWidth) ?? 0);
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

		private static float GetAggregateSectionWidth(RoadBuilderAggregateSection section)
		{
			return section.PieceRequireAll.Length == 0 && section.PieceRequireAny.Length == 0
				? section.Section?.CalculateWidth() ?? 0f
				: 0f;
		}

		public static float CalculateWidth(this NetGeometryPrefab prefab)
		{
			return prefab.m_Sections.Sum(x =>
				x.m_RequireAll.Length == 0 &&
				x.m_RequireAny.Length == 0 ? x.m_Section.CalculateWidth() : 0f);
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
