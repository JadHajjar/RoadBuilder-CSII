using Game.Prefabs;

using RoadBuilder.Domain;
using RoadBuilder.Domain.Configuration;
using RoadBuilder.Domain.Enums;

using System.Linq;

namespace RoadBuilder.Utilities
{
	public class RoadConfigGenerationUtil
	{
		private readonly RoadGenerationData _roadGenerationData;

		public RoadPrefab RoadPrefab { get; }

		public RoadConfigGenerationUtil(RoadPrefab prefab, RoadGenerationData roadGenerationData)
		{
			_roadGenerationData = roadGenerationData;

			RoadPrefab = prefab;
		}

		public RoadConfig GenerateConfiguration()
		{
			var config = new RoadConfig
			{
				SpeedLimit = RoadPrefab.m_SpeedLimit,
				GeneratesTrafficLights = RoadPrefab.m_TrafficLights,
				GeneratesZoningBlocks = RoadPrefab.m_ZoneBlock is not null,
				MaxSlopeSteepness = RoadPrefab.m_MaxSlopeSteepness,
				AggregateType = RoadPrefab.m_AggregateType?.name,
				EdgeStates = RoadPrefab.m_EdgeStates.ToList(),
				NodeStates = RoadPrefab.m_NodeStates.ToList(),
				PillarPrefabName = FindPillarPrefab(RoadPrefab),
				HasUndergroundWaterPipes = RoadPrefab.Has<WaterPipeConnection>(),
				HasUndergroundElectricityCable = RoadPrefab.Has<ElectricityConnection>(),
				RequiresUpgradeForElectricity = RoadPrefab.TryGet<ElectricityConnection>(out var electricityConnections) && electricityConnections.m_RequireAll.Contains(NetPieceRequirements.Lighting),
			};

			if (RoadPrefab.m_HighwayRules)
			{
				config.Category |= RoadCategory.Highway;
			}

			switch (RoadPrefab.m_RoadType)
			{
				case RoadType.PublicTransport:
					config.Category |= RoadCategory.PublicTransport;
					break;
			}

			config.Lanes.AddRange(RoadPrefab.m_Sections.Select(x => new LaneConfig
			{
				SectionPrefabName = x.m_Section.name,
				Invert = x.m_Invert
			}));

			return config;
		}

		private string FindPillarPrefab(RoadPrefab RoadPrefab)
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
	}
}
