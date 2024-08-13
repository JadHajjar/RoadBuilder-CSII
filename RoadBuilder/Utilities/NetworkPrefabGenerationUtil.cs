using Colossal.PSI.Common;

using Game.Prefabs;
using Game.SceneFlow;

using RoadBuilder.Domain;
using RoadBuilder.Domain.Components.Prefabs;
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
			var prefab = (config is null ? null : ScriptableObject.CreateInstance(config.GetPrefabType()) as INetworkBuilderPrefab)
				?? throw new Exception("Unknown config type " + (config?.GetType().Name ?? "NULL"));

			prefab.Config = config;
			prefab.Prefab.name = config.ID;
			prefab.Prefab.m_Sections = new NetSectionInfo[0];
			prefab.Prefab.m_EdgeStates = new NetEdgeStateInfo[0];
			prefab.Prefab.m_NodeStates = new NetNodeStateInfo[0];

			Mod.Log.DebugFormat("CreatePrefab: {0}", config.ID);

			return prefab;
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
			prefab.m_InvertMode = CompositionInvertMode.InvertLefthandTraffic;
			prefab.m_AggregateType = _roadGenerationData.AggregateNetPrefabs.TryGetValue(GetAggregateName(), out var aggregate) ? aggregate : null;
			prefab.m_Sections = Fix(GenerateSections()).ToArray();
			prefab.m_NodeStates = GenerateNodeStates().ToArray();
			prefab.m_EdgeStates = GenerateEdgeStates().ToArray();

			if (cfg is RoadConfig roadConfig)
			{
				var roadPrefab = NetworkPrefab as RoadPrefab;
				roadPrefab.m_SpeedLimit = roadConfig.SpeedLimit;
				roadPrefab.m_RoadType = roadConfig.Category.HasFlag(RoadCategory.PublicTransport) ? RoadType.PublicTransport : RoadType.Normal;
				roadPrefab.m_TrafficLights = roadConfig.Addons.HasFlag(RoadAddons.GeneratesTrafficLights);
				roadPrefab.m_HighwayRules = roadConfig.Category.HasFlag(RoadCategory.Highway);
				roadPrefab.m_ZoneBlock = roadConfig.Addons.HasFlag(RoadAddons.GeneratesZoningBlocks) ? _roadGenerationData.ZoneBlockPrefab : null;
			}

			if (cfg is TrackConfig trackConfig)
			{
				var trackPrefab = NetworkPrefab as TrackBuilderPrefab;
				trackPrefab.m_SpeedLimit = trackConfig.SpeedLimit;
				trackPrefab.m_TrackType =
					cfg.Category.HasFlag(RoadCategory.Train) ? Game.Net.TrackTypes.Train :
					cfg.Category.HasFlag(RoadCategory.Subway) ? Game.Net.TrackTypes.Subway :
					cfg.Category.HasFlag(RoadCategory.Tram) ? Game.Net.TrackTypes.Tram :
					Game.Net.TrackTypes.None;
			}

			prefab.components.ForEach(UnityEngine.Object.Destroy);
			prefab.components.Clear();
			prefab.components.AddRange(GenerateComponents());

			foreach (var item in prefab.components)
			{
				item.prefab = prefab;
				item.name = item.GetType().Name;
			}

			if (generateId)
			{
				//ThumbnailGenerationUtil.DeleteThumbnail(NetworkPrefab.Config.ID);

				NetworkPrefab.Prefab.name = NetworkPrefab.Config.ID = $"{NetworkPrefab.GetType().Name.ToLower()[0]}{Guid.NewGuid()}-{PlatformManager.instance.userSpecificPath}";
			}

			if (GameManager.instance.configuration.uiDeveloperMode)
			{
				return;
			}

			var thumbnail = new ThumbnailGenerationUtil(NetworkPrefab, _roadGenerationData).GenerateThumbnail();

			if (thumbnail is not null and not "")
			{
				prefab.GetComponent<UIObject>().m_Icon = thumbnail;
			}
		}

		private string GetAggregateName()
		{
			var category = NetworkPrefab.Config.Category;

			if (category.HasFlag(RoadCategory.Subway))
			{
				return "Subway Track";
			}

			if (category.HasFlag(RoadCategory.Train))
			{
				return "Train Track";
			}

			if (category.HasFlag(RoadCategory.Tram))
			{
				return "Tram Track";
			}

			if (category.HasFlag(RoadCategory.Pathway))
			{
				return "Pathway";
			}

			if (category.HasFlag(RoadCategory.Highway))
			{
				return "Highway";
			}

			if (category.HasFlag(RoadCategory.PublicTransport))
			{
				return "Public Transport Lane";
			}

			if (category.HasFlag(RoadCategory.RaisedSidewalk))
			{
				return "Street";
			}

			return "Alley";
		}

		private IEnumerable<NetNodeStateInfo> GenerateNodeStates()
		{
			if ((NetworkPrefab.Config.Category & RoadCategory.Pathway) != 0)
			{
				yield return new NetNodeStateInfo
				{
					m_RequireAll = new NetPieceRequirements[0],
					m_RequireAny = new NetPieceRequirements[0],
					m_RequireNone = new NetPieceRequirements[0],
					m_SetState = new[] { NetPieceRequirements.Sidewalk, NetPieceRequirements.OppositeSidewalk }
				};
			}
			else if ((NetworkPrefab.Config.Category & RoadCategory.Gravel) != 0)
			{
				yield return new NetNodeStateInfo
				{
					m_RequireAll = new NetPieceRequirements[0],
					m_RequireAny = new NetPieceRequirements[0],
					m_RequireNone = new[] { NetPieceRequirements.Elevated, NetPieceRequirements.Tunnel },
					m_SetState = new[] { NetPieceRequirements.Gravel }
				};
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
			}
			else if ((NetworkPrefab.Config.Category & (RoadCategory.Train | RoadCategory.Subway | RoadCategory.Gravel)) == 0)
			{
				yield return new NetNodeStateInfo
				{
					m_RequireAll = new NetPieceRequirements[0],
					m_RequireAny = new NetPieceRequirements[0],
					m_RequireNone = new NetPieceRequirements[0],
					m_SetState = NetworkPrefab.Config.Category.HasFlag(RoadCategory.RaisedSidewalk)
					? new[]
					{
						NetPieceRequirements.Sidewalk,
						NetPieceRequirements.OppositeSidewalk,
						NetPieceRequirements.Pavement
					}
					: new[]
					{
						NetPieceRequirements.Pavement
					}
				};
			}
		}

		private IEnumerable<NetEdgeStateInfo> GenerateEdgeStates()
		{
			var states = new List<NetPieceRequirements>();

			if ((NetworkPrefab.Config.Category & (RoadCategory.Train | RoadCategory.Subway | RoadCategory.Gravel | RoadCategory.Tiled)) == 0)
			{
				states.Add(NetPieceRequirements.Pavement);
			}

            var trackLanes = NetworkPrefab.Prefab.m_Sections.SelectMany(x => x.m_Section.FindLanes<TrackLane>()).ToList();

            if (trackLanes.Any(x => x.m_TrackType is Game.Net.TrackTypes.Tram))
			{
				states.Add(NetPieceRequirements.TramTrack);
				states.Add(NetPieceRequirements.OppositeTramTrack);
			}

			if (NetworkPrefab.Config.Addons.HasFlag(RoadAddons.GrassLeft))
			{
				states.Add(NetPieceRequirements.OppositeGrass);
			}

			if (NetworkPrefab.Config.Addons.HasFlag(RoadAddons.GrassRight))
			{
				states.Add(NetPieceRequirements.SideGrass);
			}

			if (NetworkPrefab.Config.Addons.HasFlag(RoadAddons.GrassCenter))
			{
				states.Add(NetPieceRequirements.MiddleGrass);
			}

			if (NetworkPrefab.Config.Addons.HasFlag(RoadAddons.TreesLeft))
			{
				states.Add(NetPieceRequirements.OppositeTrees);
			}

			if (NetworkPrefab.Config.Addons.HasFlag(RoadAddons.TreesRight))
			{
				states.Add(NetPieceRequirements.SideTrees);
			}

			if (NetworkPrefab.Config.Addons.HasFlag(RoadAddons.TreesCenter))
			{
				states.Add(NetPieceRequirements.MiddleTrees);
			}

			if ((NetworkPrefab.Config.Addons & (RoadAddons.HasUndergroundElectricityCable | RoadAddons.RequiresUpgradeForElectricity)) == RoadAddons.HasUndergroundElectricityCable)
			{
				states.Add(NetPieceRequirements.Lighting);
			}

			if (states.Count != 0)
			{
				yield return new NetEdgeStateInfo
				{
					m_RequireAll = new NetPieceRequirements[0],
					m_RequireAny = new NetPieceRequirements[0],
					m_RequireNone = new NetPieceRequirements[0],
					m_SetState = states.ToArray()
				};
			}

			if (NetworkPrefab.Config.Category.HasFlag(RoadCategory.Gravel))
			{
				yield return new NetEdgeStateInfo
				{
					m_RequireAll = new NetPieceRequirements[0],
					m_RequireAny = new NetPieceRequirements[0],
					m_RequireNone = new[] { NetPieceRequirements.Pavement | NetPieceRequirements.Elevated | NetPieceRequirements.Tunnel },
					m_SetState = new[] { NetPieceRequirements.Gravel }
				};

				yield return new NetEdgeStateInfo
				{
					m_RequireAll = new NetPieceRequirements[0],
					m_RequireAny = new[] { NetPieceRequirements.Pavement | NetPieceRequirements.Elevated | NetPieceRequirements.Tunnel },
					m_RequireNone = new NetPieceRequirements[0],
					m_SetState = new[] { NetPieceRequirements.Pavement }
				};
			}

			if (NetworkPrefab.Config.Category.HasFlag(RoadCategory.Tiled))
			{
				yield return new NetEdgeStateInfo
				{
					m_RequireAll = new NetPieceRequirements[0],
					m_RequireAny = new NetPieceRequirements[0],
					m_RequireNone = new[] { NetPieceRequirements.Pavement | NetPieceRequirements.Gravel },
					m_SetState = new[] { NetPieceRequirements.Tiles }
				};

				yield return new NetEdgeStateInfo
				{
					m_RequireAll = new NetPieceRequirements[0],
					m_RequireAny = new[] { NetPieceRequirements.Pavement | NetPieceRequirements.Gravel },
					m_RequireNone = new NetPieceRequirements[0],
					m_SetState = new[] { NetPieceRequirements.Pavement }
				};
			}
		}

		private IEnumerable<NetSectionInfo> GenerateSections()
		{
			var index = 0;
			var side = _roadGenerationData.NetSectionPrefabs[GetSideName()];

			yield return new NetSectionInfo
			{
				m_Section = side,
				m_Invert = true
			};

			foreach (var item in NetworkPrefab.Config.Lanes)
			{
				if (!GetNetSection(_roadGenerationData, NetworkPrefab.Config, item, out var section, out _))
				{
					Mod.Log.Warn($"NET SECTION '{item.SectionPrefabName ?? item.GroupPrefabName ?? "NULL"}' could not be found");
					continue;
				}

				if (index++ == NetworkPrefab.Config.Lanes.Count / 2 && NetworkPrefab.Config.Lanes.Count % 2 == 0)
				{
					yield return new NetSectionInfo
					{
						m_Section = _roadGenerationData.NetSectionPrefabs[GetEmptyMedianName()],
					};
				}

				yield return new NetSectionInfo
				{
					m_Section = section,
					m_Invert = item.Invert,
				};
			}

			yield return new NetSectionInfo
			{
				m_Section = side
			};
		}

		public static bool GetNetSection(RoadGenerationData roadGenerationData, INetworkConfig config, LaneConfig item, out NetSectionPrefab section, out LaneGroupPrefab groupPrefab)
		{
			if (!string.IsNullOrEmpty(item.SectionPrefabName)
				&& roadGenerationData.NetSectionPrefabs.TryGetValue(item.SectionPrefabName ?? string.Empty, out section))
			{
				groupPrefab = default;
				return true;
			}
			else if (!string.IsNullOrEmpty(item.GroupPrefabName)
				&& roadGenerationData.LaneGroupPrefabs.TryGetValue(item.GroupPrefabName ?? string.Empty, out groupPrefab)
				&& GetNetSection(config, item, groupPrefab, out section))
			{
				return true;
			}

			groupPrefab = default;
			section = default;
			return false;
		}

		private static bool GetNetSection(INetworkConfig config, LaneConfig lane, LaneGroupPrefab group, out NetSectionPrefab section)
		{
			var defaults = group.Options.ToDictionary(x => x.Name, x => x.DefaultValue);

			foreach (var item in group.LinkedSections)
			{
				if (!item.TryGet<RoadBuilderLaneGroup>(out var groupItem) || !item.MatchCategories(config))
				{
					continue;
				}

				var matched = true;

				foreach (var option in group.Options)
				{
					if (option.Type is LaneOptionType.Decoration)
					{
						continue;
					}

					var selectedValue = LaneOptionsUtil.GetSelectedOptionValue(config, lane, option);
					var combination = groupItem.Combination.FirstOrDefault(x => x.OptionName.Equals(option.Name, StringComparison.InvariantCultureIgnoreCase))?.Value;

					if (option.Type is LaneOptionType.TwoWay)
					{
						if (combination != null != (selectedValue == "1"))
						{
							matched = false;
						}

						continue;
					}

					if ((combination is not null || selectedValue is not null) && !(combination?.Equals(selectedValue, StringComparison.InvariantCultureIgnoreCase) ?? false))
					{
						matched = false;
					}
				}

				if (matched)
				{
					section = item;
					return true;
				}
			}

			section = default;
			return false;
		}

		private IEnumerable<ComponentBase> GenerateComponents()
		{
			var netPollution = ScriptableObject.CreateInstance<NetPollution>();
			var undergroundNetSections = ScriptableObject.CreateInstance<UndergroundNetSections>();
			var netSubObjects = ScriptableObject.CreateInstance<NetSubObjects>();
			var placeableNet = ScriptableObject.CreateInstance<PlaceableNet>();

			GetUIGroup(out var service, out var group);

			if (service != null)
			{
				var serviceObject = ScriptableObject.CreateInstance<ServiceObject>();
				serviceObject.m_Service = _roadGenerationData.ServicePrefabs[service];
				yield return serviceObject;
			}

			if (group != null)
			{
				var uIObject = ScriptableObject.CreateInstance<UIObject>();
				uIObject.m_Group = _roadGenerationData.UIGroupPrefabs[group];
				uIObject.m_Icon = _roadGenerationData.UIGroupPrefabs[group].GetComponent<UIObject>().m_Icon;
				uIObject.m_Priority = 999999;
				yield return uIObject;
			}

			if (NetworkPrefab.Config.Addons.HasFlag(RoadAddons.HasUndergroundWaterPipes))
			{
				yield return ScriptableObject.CreateInstance<WaterPipeConnection>();
			}

			if (NetworkPrefab.Config.Addons.HasFlag(RoadAddons.HasUndergroundElectricityCable))
			{
				var electricityConnection = ScriptableObject.CreateInstance<ElectricityConnection>();

				electricityConnection.m_Voltage = ElectricityConnection.Voltage.Low;
				electricityConnection.m_Direction = Game.Net.FlowDirection.Both;
				electricityConnection.m_Capacity = 400000;
				electricityConnection.m_RequireAll = NetworkPrefab.Config.Addons.HasFlag(RoadAddons.RequiresUpgradeForElectricity) ? new NetPieceRequirements[] { NetPieceRequirements.Lighting } : new NetPieceRequirements[0];
				electricityConnection.m_RequireAny = new NetPieceRequirements[0];
				electricityConnection.m_RequireNone = new NetPieceRequirements[0];

				yield return electricityConnection;
			}

			placeableNet.m_AllowParallelMode = true;
			placeableNet.m_XPReward = 3;
			placeableNet.m_ElevationRange = new Colossal.Mathematics.Bounds1
			{
				min = -100,
				max = 100
			};

			netSubObjects.m_SubObjects = GenerateSubObjects().ToArray();

			undergroundNetSections.m_Sections = Fix(GenerateUndergroundNetSections()).ToArray();

			yield return netPollution;
			yield return undergroundNetSections;
			yield return netSubObjects;
			yield return placeableNet;
		}

		private IEnumerable<NetSectionInfo> GenerateUndergroundNetSections()
		{
			if (NetworkPrefab.Config.Addons.HasFlag(RoadAddons.HasUndergroundWaterPipes))
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

			if (NetworkPrefab.Config.Addons.HasFlag(RoadAddons.HasUndergroundElectricityCable))
			{
				yield return new NetSectionInfo
				{
					m_Section = _roadGenerationData.NetSectionPrefabs["Ground Cable Section 1"],
					m_RequireAll = NetworkPrefab.Config.Addons.HasFlag(RoadAddons.RequiresUpgradeForElectricity) ? new[] { NetPieceRequirements.Lighting } : null,
					m_Offset = new float3(0, -0.65f, 0)
				};
			}

			if (NetworkPrefab.Config.Addons.HasFlag(RoadAddons.HasUndergroundWaterPipes))
			{
				if (NetworkPrefab.Config.Addons.HasFlag(RoadAddons.HasUndergroundElectricityCable))
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
					m_Offset = new float3(0, -1.45f, 0),
					m_RequireNone = new[] { NetPieceRequirements.Node, NetPieceRequirements.Edge }
				};

				yield return new NetSectionInfo
				{
					m_Section = _roadGenerationData.NetSectionPrefabs["Stormwater Pipe Section 1.5"],
					m_Offset = new float3(0, -1.45f, 0),
					m_RequireNone = new[] { NetPieceRequirements.Node, NetPieceRequirements.Edge }
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
			if (_roadGenerationData.PillarPrefabs.TryGetValue(NetworkPrefab.Config.PillarPrefabName ?? string.Empty, out var pillar))
			{
				yield return new NetSubObjectInfo
				{
					m_Object = pillar,
					m_Position = new float3(0, -3, 0),
					m_Placement = NetObjectPlacement.Node,
					m_RequireElevated = true,
				};
			}

			var isOneWay = NetworkPrefab.Config.IsOneWay();
			yield return new NetSubObjectInfo
			{
				m_Object = isOneWay ? _roadGenerationData.OutsideConnectionOneWay : _roadGenerationData.OutsideConnectionTwoWay,
				m_Position = new float3(0, 5, 0),
				m_Rotation = new quaternion(0, 0, 0, 0),
				m_Placement = NetObjectPlacement.Node,
				m_RequireOutsideConnection = true,
			};
		}

		private void GetUIGroup(out string service, out string group)
		{
			if (NetworkPrefab.Config.Category.HasFlag(RoadCategory.Train))
			{
				service = "Transportation";
				group = "TransportationTrain";
			}
			else if (NetworkPrefab.Config.Category.HasFlag(RoadCategory.Tram))
			{
				service = "Transportation";
				group = "TransportationTram";
			}
			else if (NetworkPrefab.Config.Category.HasFlag(RoadCategory.Subway))
			{
				service = "Transportation";
				group = "TransportationSubway";
			}
			else if (NetworkPrefab.Config.Category.HasFlag(RoadCategory.PublicTransport))
			{
				service = "Transportation";
				group = "TransportationRoad";
			}
			else if (NetworkPrefab.Config.Category.HasFlag(RoadCategory.Highway))
			{
				service = "Roads";
				group = "RoadsHighways";
			}
			else if (NetworkPrefab.Config.Category.HasFlag(RoadCategory.Pathway))
			{
				service = "Landscaping";
				group = "Pathways";
			}
			else if (NetworkPrefab is RoadPrefab)
			{
				service = "Roads";
				var width = NetworkPrefab.Prefab.m_Sections.Sum(x => x.m_Section.CalculateWidth());

				if (width >= 32)
				{
					group = "RoadsLargeRoads";
				}
				else if (width >= 18)
				{
					group = "RoadsMediumRoads";
				}
				else
				{
					group = "RoadsSmallRoads";
				}
			}
			else
			{
				service = null;
				group = null;
			}
		}

		private string GetSideName()
		{
			if (NetworkPrefab.Config.Category.HasFlag(RoadCategory.RaisedSidewalk))
			{
				return "Road Side 0";
			}

			if (NetworkPrefab.Config.Category.HasFlag(RoadCategory.Train))
			{
				return "Train Side 0";
			}

			if (NetworkPrefab.Config.Category.HasFlag(RoadCategory.Tram))
			{
				return "Alley Side 0";
			}

			if (NetworkPrefab.Config.Category.HasFlag(RoadCategory.Subway))
			{
				return "Subway Side 0";
			}

			if (NetworkPrefab.Config.Category.HasFlag(RoadCategory.Highway))
			{
				return "Highway Side 0";
			}

			if (NetworkPrefab.Config.Category.HasFlag(RoadCategory.Gravel))
			{
				return "Gravel Side 0";
			}

			if (NetworkPrefab.Config.Category.HasFlag(RoadCategory.Tiled))
			{
				return "Tiled Side 0";
			}

			if (NetworkPrefab.Config.Category.HasFlag(RoadCategory.Pathway))
			{
				return "Pavement Path Side Section 0";
			}

			return "Alley Side 0";
		}

		private string GetEmptyMedianName()
		{
			if (NetworkPrefab.Config.Category.HasFlag(RoadCategory.Train))
			{
				return "Train Median 0";
			}

			if (NetworkPrefab.Config.Category.HasFlag(RoadCategory.Tram))
			{
				return "Tram Median 0";
			}

			if (NetworkPrefab.Config.Category.HasFlag(RoadCategory.Subway))
			{
				return "Subway Median 0";
			}

			if (NetworkPrefab.Config.Category.HasFlag(RoadCategory.Highway))
			{
				return "Highway Median 0";
			}

			if (NetworkPrefab.Config.Category.HasFlag(RoadCategory.Gravel))
			{
				return "Gravel Median 0";
			}

			if (NetworkPrefab.Config.Category.HasFlag(RoadCategory.Tiled))
			{
				return "Tiled Median 2";
			}

			if (NetworkPrefab.Config.Category.HasFlag(RoadCategory.PublicTransport))
			{
				return "Public Transport Median 0";
			}

			if (NetworkPrefab.Config.Category.HasFlag(RoadCategory.RaisedSidewalk))
			{
				return "Road Median 0";
			}

			return "Alley Median 0";
		}
	}
}
