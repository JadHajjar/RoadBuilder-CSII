﻿using Colossal.Json;

using Game.Prefabs;
using Game.SceneFlow;

using RoadBuilder.Domain;
using RoadBuilder.Domain.Components.Prefabs;
using RoadBuilder.Domain.Configurations;
using RoadBuilder.Domain.Enums;
using RoadBuilder.Domain.Prefabs;
using RoadBuilder.Systems;
using RoadBuilder.Systems.UI;

using System;
using System.Linq;

namespace RoadBuilder.Utilities
{
	public class NetworkConfigGenerationUtil
	{
		private readonly RoadGenerationData _roadGenerationData;
		private readonly RoadBuilderUISystem _roadBuilderUISystem;

		public NetGeometryPrefab NetworkPrefab { get; }

		public NetworkConfigGenerationUtil(NetGeometryPrefab prefab, RoadGenerationData roadGenerationData, RoadBuilderUISystem roadBuilderUISystem)
		{
			_roadGenerationData = roadGenerationData;
			_roadBuilderUISystem = roadBuilderUISystem;

			NetworkPrefab = prefab;
		}

		public INetworkConfig GenerateConfiguration()
		{
			if (NetworkPrefab is INetworkBuilderPrefab customPrefab && customPrefab.Config is not null)
			{
				return JsonClone(customPrefab.Config);
			}

			INetworkConfig config;

			if (NetworkPrefab is RoadPrefab roadPrefab)
			{
				config = GenerateRoadConfig(roadPrefab);
			}
			else if (NetworkPrefab is TrackPrefab trackPrefab)
			{
				config = GenerateTrackConfig(trackPrefab);
			}
			else if (NetworkPrefab is FencePrefab fencePrefab)
			{
				config = GenerateFenceConfig(fencePrefab);
			}
			else if (NetworkPrefab is PathwayPrefab pathPrefab)
			{
				config = GeneratePathConfig(pathPrefab);
			}
			else
			{
				throw new Exception("Invalid Prefab");
			}

			config.Playsets = new() { PdxModsUtil.CurrentPlayset };
			config.Name = $"Custom {GetAssetName(NetworkPrefab)}";
			config.MaxSlopeSteepness = NetworkPrefab.m_MaxSlopeSteepness;
			config.PillarPrefabName = FindPillarPrefab(NetworkPrefab);

			if (NetworkPrefab.Has<WaterPipeConnection>())
			{
				config.Addons |= RoadAddons.HasUndergroundWaterPipes;
			}

			if (NetworkPrefab.Has<ElectricityConnection>())
			{
				config.Addons |= RoadAddons.HasUndergroundElectricityCable;
			}

			if (NetworkPrefab.TryGet<ElectricityConnection>(out var electricityConnections) && electricityConnections.m_RequireAll.Contains(NetPieceRequirements.Lighting))
			{
				config.Addons |= RoadAddons.RequiresUpgradeForElectricity;
			}

			if (NetworkPrefab.m_EdgeStates.Any(x => x.m_SetState.Any(y => y == NetPieceRequirements.Gravel) && x.m_RequireAny.Length == 0 && x.m_RequireAll.Length == 0))
			{
				config.Category = RoadCategory.Gravel;
			}

			if (NetworkPrefab.m_EdgeStates.Any(x => x.m_SetState.Any(y => y == NetPieceRequirements.Tiles) && x.m_RequireAny.Length == 0 && x.m_RequireAll.Length == 0))
			{
				config.Category = RoadCategory.Tiled;
			}

			config.Lanes.AddRange(NetworkPrefab.m_Sections
				.Where(x => x.m_RequireAll.Length == 0 && x.m_RequireAny.Length == 0 && !x.m_Section.Has<RoadBuilderIgnoreSection>())
				.Select(x => GetLaneConfig(x)));

			// remove sides
			config.Lanes.RemoveAt(0);
			config.Lanes.RemoveAt(config.Lanes.Count - 1);

			if (NetworkPrefab.m_Sections[(NetworkPrefab.m_Sections.Length - 1) / 2].m_Section.CalculateWidth() == 0)
			{
				config.Lanes.RemoveAt((config.Lanes.Count - 1) / 2);
			}

			return config;
		}

		private INetworkConfig JsonClone(INetworkConfig config)
		{
			config.Type = config.GetType().Name;

			config = LocalSaveUtil.LoadFromJson(JSON.Dump(config))!;

			config.Version = RoadBuilderSerializeSystem.CURRENT_VERSION;
			config.ID = string.Empty;
			config.OriginalID = null;
			config.Name = $"Copy of {config.Name}";
			config.Playsets = new() { PdxModsUtil.CurrentPlayset };

			return config;
		}

		private LaneConfig GetLaneConfig(NetSectionInfo section)
		{
			if (section.m_Section.TryGet<RoadBuilderLaneGroup>(out var groupItem))
			{
				return new LaneConfig
				{
					GroupPrefabName = groupItem.GroupPrefab?.name,
					GroupOptions = groupItem.Combination?.ToDictionary(x => x.OptionName ?? string.Empty, x => x.Value) ?? new(),
					Invert = section.m_Invert
				};
			}

			var config = new LaneConfig
			{
				SectionPrefabName = section.m_Section.name,
				Invert = section.m_Invert
			};

			if (NetworkPrefab.name is "Pedestrian Street" && config.SectionPrefabName == "Tiled Section 3")
			{
				config.SectionPrefabName = "Tiled Pedestrian Section 3";
			}

			if (NetworkPrefab.name is "Pedestrian Street" && config.SectionPrefabName == "Tiled Median Pedestrian 2")
			{
				config.SectionPrefabName = "RB Tiled Median 2";
			}

			return config;
		}

		private INetworkConfig GenerateRoadConfig(RoadPrefab roadPrefab)
		{
			var config = new RoadConfig
			{
				SpeedLimit = roadPrefab.m_SpeedLimit
			};

			if (roadPrefab.m_HighwayRules)
			{
				config.Category = RoadCategory.Highway;
			}

			if (roadPrefab.m_TrafficLights)
			{
				config.Addons |= RoadAddons.GeneratesTrafficLights;
			}

			if (roadPrefab.m_ZoneBlock)
			{
				config.Addons |= RoadAddons.GeneratesZoningBlocks;
			}

			switch (roadPrefab.m_RoadType)
			{
				case RoadType.PublicTransport:
					config.Category = RoadCategory.PublicTransport;
					break;
			}

			FixTrafficDirection(config);

			return config;
		}

		private void FixTrafficDirection(INetworkConfig config)
		{
			if (_roadGenerationData.LeftHandTraffic)
			{
				switch (NetworkPrefab.m_InvertMode)
				{
					case CompositionInvertMode.InvertLefthandTraffic:
						config.Lanes.Reverse();
						break;
					case CompositionInvertMode.FlipLefthandTraffic:
						config.Lanes.ForEach(x => x.Invert = !x.Invert);
						break;
				}
			}
			else
			{
				switch (NetworkPrefab.m_InvertMode)
				{
					case CompositionInvertMode.InvertRighthandTraffic:
						config.Lanes.Reverse();
						break;
					case CompositionInvertMode.FlipRighthandTraffic:
						config.Lanes.ForEach(x => x.Invert = !x.Invert);
						break;
				}
			}
		}

		private INetworkConfig GenerateTrackConfig(TrackPrefab trackPrefab)
		{
			var config = new TrackConfig
			{
				SpeedLimit = trackPrefab.m_SpeedLimit
			};

			if (trackPrefab.m_TrackType.HasFlag(Game.Net.TrackTypes.Train))
			{
				config.Category = RoadCategory.Train;
			}

			if (trackPrefab.m_TrackType.HasFlag(Game.Net.TrackTypes.Tram))
			{
				config.Category = RoadCategory.Tram;
			}

			if (trackPrefab.m_TrackType.HasFlag(Game.Net.TrackTypes.Subway))
			{
				config.Category = RoadCategory.Subway;
			}

			return config;
		}

		private INetworkConfig GenerateFenceConfig(FencePrefab _)
		{
			var config = new FenceConfig
			{
				Category = RoadCategory.Fence
			};

			return config;
		}

		private INetworkConfig GeneratePathConfig(PathwayPrefab _)
		{
			var config = new PathConfig
			{
				Category = RoadCategory.Pathway
			};

			return config;
		}

		private string? FindPillarPrefab(NetGeometryPrefab RoadPrefab)
		{
			if (!RoadPrefab.TryGet<NetSubObjects>(out var netSubObjects))
			{
				return null;
			}

			foreach (var item in netSubObjects.m_SubObjects)
			{
				if (item.m_RequireElevated && item.m_Placement == NetObjectPlacement.Node && item.m_Object.Has<PillarObject>())
				{
					return item.m_Object.name;
				}
			}

			foreach (var item in netSubObjects.m_SubObjects)
			{
				if (item.m_RequireElevated && item.m_Placement == NetObjectPlacement.Node)
				{
					return item.m_Object.name;
				}
			}

			return null;
		}

		private string GetAssetName(PrefabBase prefab)
		{
			_roadBuilderUISystem.GetTitleAndDescription(prefab, out var titleId, out var _);

			if (GameManager.instance.localizationManager.activeDictionary.TryGetValue(titleId, out var name))
			{
				return name;
			}

			return prefab.name.Replace('_', ' ').FormatWords();
		}
	}
}
