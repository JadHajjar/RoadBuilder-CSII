using Game.Prefabs;
using Game.SceneFlow;
using Game.UI.InGame;

using RoadBuilder.Domain;
using RoadBuilder.Domain.Configurations;
using RoadBuilder.Domain.Enums;

using System;
using System.Linq;

namespace RoadBuilder.Utilities
{
	public class NetworkConfigGenerationUtil
	{
		private readonly RoadGenerationData _roadGenerationData;
		private readonly PrefabUISystem _prefabUISystem;

		public NetGeometryPrefab NetworkPrefab { get; }

		public NetworkConfigGenerationUtil(NetGeometryPrefab prefab, RoadGenerationData roadGenerationData, PrefabUISystem prefabUISystem)
		{
			_roadGenerationData = roadGenerationData;
			_prefabUISystem = prefabUISystem;

			NetworkPrefab = prefab;
		}

		public INetworkConfig GenerateConfiguration()
		{
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
			else
			{
				throw new Exception("Invalid Prefab");
			}

			config.Name = $"Custom {GetAssetName(NetworkPrefab)}";
			config.MaxSlopeSteepness = NetworkPrefab.m_MaxSlopeSteepness;
			config.AggregateType = NetworkPrefab.m_AggregateType?.name;
			config.PillarPrefabName = FindPillarPrefab(NetworkPrefab);
			config.HasUndergroundWaterPipes = NetworkPrefab.Has<WaterPipeConnection>();
			config.HasUndergroundElectricityCable = NetworkPrefab.Has<ElectricityConnection>();
			config.RequiresUpgradeForElectricity = NetworkPrefab.TryGet<ElectricityConnection>(out var electricityConnections) && electricityConnections.m_RequireAll.Contains(NetPieceRequirements.Lighting);

			if (NetworkPrefab.m_EdgeStates.Any(x => x.m_SetState.Any(y => y == NetPieceRequirements.Gravel) && x.m_RequireAny.Length == 0 && x.m_RequireAll.Length == 0))
			{
				config.Category |= RoadCategory.Gravel;
			}

			if (NetworkPrefab.m_EdgeStates.Any(x => x.m_SetState.Any(y => y == NetPieceRequirements.Tiles) && x.m_RequireAny.Length == 0 && x.m_RequireAll.Length == 0))
			{
				config.Category |= RoadCategory.Tiled;
			}

			config.Lanes.AddRange(NetworkPrefab.m_Sections
				.Where(x => x.m_RequireAll.Length == 0 && x.m_RequireAny.Length == 0)
				.Select(x => new LaneConfig
				{
					SectionPrefabName = x.m_Section.name,
					Invert = x.m_Invert
				}));

			return config;
		}

		private INetworkConfig GenerateRoadConfig(RoadPrefab roadPrefab)
		{
			var config = new RoadConfig
			{
				SpeedLimit = roadPrefab.m_SpeedLimit,
				GeneratesTrafficLights = roadPrefab.m_TrafficLights,
				GeneratesZoningBlocks = roadPrefab.m_ZoneBlock is not null
			};

			if (roadPrefab.m_HighwayRules)
			{
				config.Category |= RoadCategory.Highway;
			}

			switch (roadPrefab.m_RoadType)
			{
				case RoadType.PublicTransport:
					config.Category |= RoadCategory.PublicTransport;
					break;
			}

			return config;
		}

		private INetworkConfig GenerateTrackConfig(TrackPrefab trackPrefab)
		{
			var config = new TrackConfig
			{
				SpeedLimit = trackPrefab.m_SpeedLimit
			};

			if (trackPrefab.m_TrackType.HasFlag(Game.Net.TrackTypes.Train))
			{
				config.Category |= RoadCategory.Train;
			}

			if (trackPrefab.m_TrackType.HasFlag(Game.Net.TrackTypes.Tram))
			{
				config.Category |= RoadCategory.Tram;
			}

			if (trackPrefab.m_TrackType.HasFlag(Game.Net.TrackTypes.Subway))
			{
				config.Category |= RoadCategory.Subway;
			}

			return config;
		}

		private INetworkConfig GenerateFenceConfig(FencePrefab fencePrefab)
		{
			var config = new FenceConfig();

			return config;
		}

		private string FindPillarPrefab(NetGeometryPrefab RoadPrefab)
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

			return null;
		}

		private string GetAssetName(PrefabBase prefab)
		{
			_prefabUISystem.GetTitleAndDescription(prefab, out var titleId, out var _);

			if (GameManager.instance.localizationManager.activeDictionary.TryGetValue(titleId, out var name))
			{
				return name;
			}

			return prefab.name.Replace('_', ' ').FormatWords();
		}
	}
}
