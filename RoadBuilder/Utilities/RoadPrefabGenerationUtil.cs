using Colossal.PSI.Common;

using Game.Prefabs;

using RoadBuilder.Domain;
using RoadBuilder.Domain.Enums;

using System;
using System.Collections.Generic;
using System.Linq;

using Unity.Mathematics;

using UnityEngine;

namespace RoadBuilder.Utilities
{
	public class RoadPrefabGenerationUtil
	{
		private readonly RoadGenerationData _roadGenerationData;

		public RoadBuilderPrefab RoadPrefab { get; }

		public RoadPrefabGenerationUtil(RoadBuilderPrefab prefab, RoadGenerationData roadGenerationData)
		{
			_roadGenerationData = roadGenerationData;

			RoadPrefab = prefab;
		}

		public void GenerateRoad()
		{
			var cfg = RoadPrefab.Config;

			RoadPrefab.isDirty = true;
			RoadPrefab.name = cfg.ID;
			RoadPrefab.m_SpeedLimit = cfg.SpeedLimit;
			RoadPrefab.m_RoadType = cfg.Category.HasFlag(RoadCategory.PublicTransport) ? RoadType.PublicTransport : RoadType.Normal;
			RoadPrefab.m_TrafficLights = cfg.GeneratesTrafficLights;
			RoadPrefab.m_HighwayRules = cfg.Category.HasFlag(RoadCategory.Highway);
			RoadPrefab.m_MaxSlopeSteepness = cfg.MaxSlopeSteepness;
			RoadPrefab.m_InvertMode = CompositionInvertMode.FlipLefthandTraffic;
			RoadPrefab.m_ZoneBlock = cfg.GeneratesZoningBlocks ? _roadGenerationData.ZoneBlockPrefab : null;
			RoadPrefab.m_AggregateType = _roadGenerationData.AggregateNetPrefabs.TryGetValue(cfg.AggregateType, out var aggregate) ? aggregate : null;
			RoadPrefab.m_NodeStates = cfg.NodeStates.ToArray();
			RoadPrefab.m_EdgeStates = cfg.EdgeStates.ToArray();
			RoadPrefab.m_Sections = GenerateSections().ToArray();

			RoadPrefab.components.Clear();
			RoadPrefab.components.AddRange(GenerateComponents());

			foreach (var item in RoadPrefab.components)
			{
				item.prefab = RoadPrefab;
				item.name = item.GetType().Name;
			}

			if (RoadPrefab.WasGenerated)
			{
				RoadPrefab.Config.ID = $"{Guid.NewGuid()}-{PlatformManager.instance.userSpecificPath}";
			}
		}

		private IEnumerable<NetSectionInfo> GenerateSections()
		{
			foreach (var item in RoadPrefab.Config.Lanes)
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

			if (RoadPrefab.Config.HasUndergroundWaterPipes)
			{
				yield return ScriptableObject.CreateInstance<WaterPipeConnection>();
			}

			if (RoadPrefab.Config.HasUndergroundElectricityCable)
			{
				var electricityConnection = ScriptableObject.CreateInstance<ElectricityConnection>();

				electricityConnection.m_Voltage = ElectricityConnection.Voltage.Low;
				electricityConnection.m_Direction = Game.Net.FlowDirection.Both;
				electricityConnection.m_Capacity = 400000;
				electricityConnection.m_RequireAll = RoadPrefab.Config.RequiresUpgradeForElectricity ? new NetPieceRequirements[] { NetPieceRequirements.Lighting } : new NetPieceRequirements[0];
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
			if (RoadPrefab.Config.HasUndergroundWaterPipes)
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

			if (RoadPrefab.Config.HasUndergroundElectricityCable)
			{
				yield return new NetSectionInfo
				{
					m_Section = _roadGenerationData.NetSectionPrefabs["Ground Cable Section 1"],
					m_RequireAll = RoadPrefab.Config.RequiresUpgradeForElectricity ? new[] { NetPieceRequirements.Lighting } : null,
					m_Offset = new float3(0, -0.65f, 0)
				};
			}

			if (RoadPrefab.Config.HasUndergroundWaterPipes)
			{
				if (RoadPrefab.Config.HasUndergroundElectricityCable)
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
			if (_roadGenerationData.PillarPrefabs.TryGetValue(RoadPrefab.Config.PillarPrefabName, out var pillar))
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
				m_Object = RoadPrefab.Config.IsOneWay() ? _roadGenerationData.OutsideConnectionOneWay : _roadGenerationData.OutsideConnectionTwoWay,
				m_Position = new float3(0, 5, 0),
				m_Placement = NetObjectPlacement.Node,
				m_RequireOutsideConnection = true,
			};
		}

		public void DummyGenerateRoad(RoadBuilderPrefab prefab)
		{
			var prefabs = new List<PrefabBase>();//Traverse.Create(_prefabSystem).Field<List<PrefabBase>>("m_Prefabs").Value;
			var basePrefab = (RoadPrefab)prefabs.FirstOrDefault(p => p.name == "Small Road");
			var roadLanePrefab = (NetSectionPrefab)prefabs.FirstOrDefault(p => p.name == "Car Drive Section 3");
			var sidewalk5Prefab = (NetSectionPrefab)prefabs.FirstOrDefault(p => p.name == "Sidewalk 5");

			var baseComponents = new ComponentBase[basePrefab.components.Count];
			basePrefab.components.CopyTo(baseComponents);

			prefab.m_SpeedLimit = 80;
			prefab.m_RoadType = RoadType.Normal;
			prefab.m_TrafficLights = false;
			prefab.m_HighwayRules = false;
			prefab.m_MaxSlopeSteepness = 0.2f;
			prefab.m_InvertMode = CompositionInvertMode.FlipLefthandTraffic;
			prefab.isDirty = true;
			prefab.active = true;
			prefab.m_Sections = new NetSectionInfo[basePrefab.m_Sections.Length];
			prefab.m_ZoneBlock = basePrefab.m_ZoneBlock;
			prefab.m_AggregateType = basePrefab.m_AggregateType;
			prefab.m_NodeStates = new NetNodeStateInfo[basePrefab.m_NodeStates.Length];
			prefab.m_EdgeStates = new NetEdgeStateInfo[basePrefab.m_EdgeStates.Length];

			prefab.components.AddRange(baseComponents);
			basePrefab.m_Sections.CopyTo(prefab.m_Sections, 0);
			basePrefab.m_NodeStates.CopyTo(prefab.m_NodeStates, 0);
			basePrefab.m_EdgeStates.CopyTo(prefab.m_EdgeStates, 0);
			prefab.name = "Test Road Prefab";
			prefab.m_Sections[1].m_Section = sidewalk5Prefab;
			prefab.m_Sections[5].m_Section = sidewalk5Prefab;
			prefab.m_Sections = new NetSectionInfo[]
			{
				prefab.m_Sections[0],
				prefab.m_Sections[1],
				new()
				{
					m_Section = roadLanePrefab,
					m_Flip = false,
					m_Invert = true,
					m_RequireAll = new NetPieceRequirements[0],
					m_RequireAny = new NetPieceRequirements[0],
					m_RequireNone = new NetPieceRequirements[0],
					m_Median = false,
					m_Offset = float3.zero
				},
				prefab.m_Sections[2],
				prefab.m_Sections[3],
				prefab.m_Sections[4],
				new()
				{
					m_Section = roadLanePrefab,
					m_Flip = false,
					m_Invert = false,
					m_RequireAll = new NetPieceRequirements[0],
					m_RequireAny = new NetPieceRequirements[0],
					m_RequireNone = new NetPieceRequirements[0],
					m_Median = false,
					m_Offset = float3.zero
				},
				prefab.m_Sections[5],
				prefab.m_Sections[6]
			};
		}
	}
}
