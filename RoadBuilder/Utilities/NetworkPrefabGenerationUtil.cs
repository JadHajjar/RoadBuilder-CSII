using Colossal.PSI.Common;

using Game.Prefabs;

using RoadBuilder.Domain;
using RoadBuilder.Domain.Configuration;
using RoadBuilder.Domain.Configurations;
using RoadBuilder.Domain.Enums;
using RoadBuilder.Domain.Prefabs;

using System;
using System.Collections.Generic;
using System.Linq;

using Unity.Mathematics;

using UnityEngine;

namespace RoadBuilder.Utilities
{
	public class NetworkPrefabGenerationUtil
	{
		private readonly RoadGenerationData _roadGenerationData;

		public INetworkBuilderPrefab NetworkPrefab { get; }

		public NetworkPrefabGenerationUtil(INetworkBuilderPrefab prefab, RoadGenerationData roadGenerationData)
		{
			_roadGenerationData = roadGenerationData;

			NetworkPrefab = prefab;
		}

		public static INetworkBuilderPrefab CreatePrefab(INetworkConfig config)
		{
			if (config is RoadConfig roadConfig)
			{
				return new RoadBuilderPrefab(roadConfig);
			}

			if (config is TrackConfig trackConfig)
			{
				return new TrackBuilderPrefab(trackConfig);
			}

			if (config is FenceConfig fenceConfig)
			{
				return new FenceBuilderPrefab(fenceConfig);
			}

			throw new Exception("Unknown config type " + (config?.GetType().Name ?? "NULL"));
		}

		public void GenerateRoad(bool generateId = true)
		{
			try
			{
				GenerateRoadInternal(generateId);
			}
			catch (Exception ex)
			{
				Mod.Log.Error(ex, "Unhandled Error During Road Generation");
			}
		}

		private void GenerateRoadInternal(bool generateId)
		{
			var cfg = NetworkPrefab.Config;
			var prefab = NetworkPrefab.Prefab;

			prefab.isDirty = true;
			prefab.name = cfg.ID;
			prefab.m_MaxSlopeSteepness = cfg.MaxSlopeSteepness;
			prefab.m_InvertMode = CompositionInvertMode.FlipLefthandTraffic;
			prefab.m_AggregateType = _roadGenerationData.AggregateNetPrefabs.TryGetValue(cfg.AggregateType, out var aggregate) ? aggregate : null;
			prefab.m_NodeStates = GenerateNodeStates().ToArray();
			prefab.m_EdgeStates = GenerateEdgeStates().ToArray();
			prefab.m_Sections = GenerateSections().ToArray();

			if (cfg is RoadConfig roadConfig)
			{
				var roadPrefab = NetworkPrefab as RoadPrefab;
				roadPrefab.m_SpeedLimit = roadConfig.SpeedLimit;
				roadPrefab.m_RoadType = roadConfig.Category.HasFlag(RoadCategory.PublicTransport) ? RoadType.PublicTransport : RoadType.Normal;
				roadPrefab.m_TrafficLights = roadConfig.GeneratesTrafficLights;
				roadPrefab.m_HighwayRules = roadConfig.Category.HasFlag(RoadCategory.Highway);
				roadPrefab.m_ZoneBlock = roadConfig.GeneratesZoningBlocks ? _roadGenerationData.ZoneBlockPrefab : null;
			}

			prefab.components.Clear();
			prefab.components.AddRange(GenerateComponents());

			foreach (var item in prefab.components)
			{
				item.prefab = prefab;
				item.name = item.GetType().Name;
			}

			if (generateId)
			{
				NetworkPrefab.Config.ID = $"{NetworkPrefab.GetType().Name.ToLower()[0]}{Guid.NewGuid()}-{PlatformManager.instance.userSpecificPath}";
			}
		}

		private IEnumerable<NetNodeStateInfo> GenerateNodeStates()
		{
			if ((NetworkPrefab.Config.Category & RoadCategory.Gravel) != 0)
			{
				yield return new NetNodeStateInfo
				{
					m_RequireAll = new NetPieceRequirements[0],
					m_RequireAny = new NetPieceRequirements[0],
					m_RequireNone = new NetPieceRequirements[0],
					m_SetState = new[]
					{
						NetPieceRequirements.Gravel
					}
				};

				yield break;
			}
			else if ((NetworkPrefab.Config.Category & RoadCategory.Tiled) != 0)
			{
				yield return new NetNodeStateInfo
				{
					m_RequireAll = new NetPieceRequirements[0],
					m_RequireAny = new NetPieceRequirements[0],
					m_RequireNone = new NetPieceRequirements[0],
					m_SetState = new[]
					{
						NetPieceRequirements.Tiles
					}
				};

				yield break;
			}
			else if ((NetworkPrefab.Config.Category & (RoadCategory.Train | RoadCategory.Subway | RoadCategory.Gravel)) == 0)
			{
				yield return new NetNodeStateInfo
				{
					m_RequireAll = new NetPieceRequirements[0],
					m_RequireAny = new NetPieceRequirements[0],
					m_RequireNone = new NetPieceRequirements[0],
					m_SetState = new[]
					{
						NetPieceRequirements.Sidewalk,
						NetPieceRequirements.OppositeSidewalk,
						NetPieceRequirements.Pavement
					}
				};

				yield break;
			}
		}

		private IEnumerable<NetEdgeStateInfo> GenerateEdgeStates()
		{
			if ((NetworkPrefab.Config.Category & (RoadCategory.Train | RoadCategory.Subway)) == 0)
			{
				yield return new NetEdgeStateInfo
				{
					m_RequireAll = new NetPieceRequirements[0],
					m_RequireAny = new NetPieceRequirements[0],
					m_RequireNone = new NetPieceRequirements[0],
					m_SetState = new[]
					{
						((NetworkPrefab.Config.Category & RoadCategory.Gravel) != 0)
							? NetPieceRequirements.Gravel :
							((NetworkPrefab.Config.Category & RoadCategory.Tiled) != 0)
							? NetPieceRequirements.Tiles 
							: NetPieceRequirements.Pavement
					}
				};

				yield break;
			}
		}

		private IEnumerable<NetSectionInfo> GenerateSections()
		{
			foreach (var item in NetworkPrefab.Config.Lanes)
			{
				if (!_roadGenerationData.NetSectionPrefabs.TryGetValue(item.SectionPrefabName, out var section))
				{
					Mod.Log.Warn($"NET SECTION '{item.SectionPrefabName}' could not be found");
					continue;
				}

				yield return new NetSectionInfo
				{
					m_Section = section,
					m_Invert = item.Invert,
				};
			}
		}

		private IEnumerable<ComponentBase> GenerateComponents()
		{
			var uIObject = ScriptableObject.CreateInstance<UIObject>();
			var serviceObject = ScriptableObject.CreateInstance<ServiceObject>();
			var netPollution = ScriptableObject.CreateInstance<NetPollution>();
			var undergroundNetSections = ScriptableObject.CreateInstance<UndergroundNetSections>();
			var netSubObjects = ScriptableObject.CreateInstance<NetSubObjects>();
			var placeableNet = ScriptableObject.CreateInstance<PlaceableNet>();

			uIObject.m_Group = _roadGenerationData.UIGroupPrefabs["Roads"];
			uIObject.m_Priority = 999999;

			serviceObject.m_Service = _roadGenerationData.ServicePrefabs["Roads"];

			if (NetworkPrefab.Config.HasUndergroundWaterPipes)
			{
				yield return ScriptableObject.CreateInstance<WaterPipeConnection>();
			}

			if (NetworkPrefab.Config.HasUndergroundElectricityCable)
			{
				var electricityConnection = ScriptableObject.CreateInstance<ElectricityConnection>();

				electricityConnection.m_Voltage = ElectricityConnection.Voltage.Low;
				electricityConnection.m_Direction = Game.Net.FlowDirection.Both;
				electricityConnection.m_Capacity = 400000;
				electricityConnection.m_RequireAll = NetworkPrefab.Config.RequiresUpgradeForElectricity ? new NetPieceRequirements[] { NetPieceRequirements.Lighting } : new NetPieceRequirements[0];
				electricityConnection.m_RequireAny = new NetPieceRequirements[0];
				electricityConnection.m_RequireNone = new NetPieceRequirements[0];

				yield return electricityConnection;
			}

			placeableNet.m_AllowParallelMode = true;
			placeableNet.m_XPReward = 3;
			placeableNet.m_ElevationRange = new Colossal.Mathematics.Bounds1
			{
				min = -50,
				max = 50
			};

			netSubObjects.m_SubObjects = GenerateSubObjects().ToArray();

			undergroundNetSections.m_Sections = Fix(GenerateUndergroundNetSections()).ToArray();

			//yield return uIObject;
			//yield return serviceObject;
			yield return netPollution;
			yield return undergroundNetSections;
			yield return netSubObjects;
			yield return placeableNet;
		}

		private IEnumerable<NetSectionInfo> GenerateUndergroundNetSections()
		{
			if (NetworkPrefab.Config.HasUndergroundWaterPipes)
			{
				yield return new NetSectionInfo
				{
					m_Section = _roadGenerationData.NetSectionPrefabs["Sewage Pipe Section 1.5"],
					m_Offset = new float3(0, -1.25f, 0)
				};

				yield return new NetSectionInfo
				{
					m_Section = _roadGenerationData.NetSectionPrefabs["Pipeline Spacing Section 1"],
					m_Offset = new float3(0, -1.25f, 0)
				};
			}

			if (NetworkPrefab.Config.HasUndergroundElectricityCable)
			{
				yield return new NetSectionInfo
				{
					m_Section = _roadGenerationData.NetSectionPrefabs["Ground Cable Section 1"],
					m_RequireAll = NetworkPrefab.Config.RequiresUpgradeForElectricity ? new[] { NetPieceRequirements.Lighting } : null,
					m_Offset = new float3(0, -0.65f, 0)
				};
			}

			if (NetworkPrefab.Config.HasUndergroundWaterPipes)
			{
				if (NetworkPrefab.Config.HasUndergroundElectricityCable)
				{
					yield return new NetSectionInfo
					{
						m_Section = _roadGenerationData.NetSectionPrefabs["Pipeline Spacing Section 1"],
						m_Offset = new float3(0, -0.85f, 0)
					};
				}

				yield return new NetSectionInfo
				{
					m_Section = _roadGenerationData.NetSectionPrefabs["Water Pipe Section 1"],
					m_Offset = new float3(0, -0.85f, 0)
				};
				yield return new NetSectionInfo
				{
					m_Section = _roadGenerationData.NetSectionPrefabs["Pipeline Spacing Section 1"],
					m_Offset = new float3(0, -1.45f, 0)
				};
				yield return new NetSectionInfo
				{
					m_Section = _roadGenerationData.NetSectionPrefabs["Stormwater Pipe Section 1.5"],
					m_Offset = new float3(0, -1.45f, 0)
				};
			}
		}

		private IEnumerable<NetSectionInfo> Fix(IEnumerable<NetSectionInfo> sections)
		{
			foreach (var item in sections)
			{
				item.m_RequireAll ??= new NetPieceRequirements[0];
				item.m_RequireAny ??= new NetPieceRequirements[0];
				item.m_RequireNone ??= new NetPieceRequirements[0];

				yield return item;
			}
		}

		private IEnumerable<NetSubObjectInfo> GenerateSubObjects()
		{
			if (_roadGenerationData.PillarPrefabs.TryGetValue(NetworkPrefab.Config.PillarPrefabName, out var pillar))
			{
				yield return new NetSubObjectInfo
				{
					m_Object = pillar,
					m_Position = new float3(0, -3, 0),
					m_Placement = NetObjectPlacement.Node,
					m_RequireElevated = true,
				};
			}

			yield return new NetSubObjectInfo
			{
				m_Object = NetworkPrefab.Config.IsOneWay() ? _roadGenerationData.OutsideConnectionOneWay : _roadGenerationData.OutsideConnectionTwoWay,
				m_Position = new float3(0, 5, 0),
				m_Placement = NetObjectPlacement.Node,
				m_RequireOutsideConnection = true,
			};
		}
	}
}
