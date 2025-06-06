﻿using Colossal.Serialization.Entities;

using Game;
using Game.City;
using Game.Common;
using Game.Prefabs;

using RoadBuilder.Domain;
using RoadBuilder.Utilities;

using Unity.Collections;
using Unity.Entities;

using UnityEngine;

namespace RoadBuilder.Systems
{
	public partial class RoadBuilderGenerationDataSystem : RoadBuilderNetSectionsSystem
	{
#nullable disable
		private PrefabSystem prefabSystem;
		private CityConfigurationSystem cityConfigurationSystem;
		private bool firstTimeRun;
#nullable enable

		public RoadGenerationData? RoadGenerationData { get; private set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
			cityConfigurationSystem = World.GetOrCreateSystemManaged<CityConfigurationSystem>();
		}

		protected override void OnGamePreload(Purpose purpose, GameMode mode)
		{
			base.OnGamePreload(purpose, mode);

			OnUpdate();
		}

		protected override void OnUpdate()
		{
			base.OnUpdate();

			if (!InitialSetupFinished)
			{
				return;
			}

			var roadGenerationData = new RoadGenerationData();

			var zoneBlockDataQuery = SystemAPI.QueryBuilder().WithAll<ZoneBlockData>().Build();
			var zoneBlockDataEntities = zoneBlockDataQuery.ToEntityArray(Allocator.Temp);

			roadGenerationData.LeftHandTraffic = cityConfigurationSystem.leftHandTraffic;
			roadGenerationData.YellowDivider = !prefabSystem.TryGetSpecificPrefab<ThemePrefab>(cityConfigurationSystem.defaultTheme, out var theme) || theme.assetPrefix is not "EU";

			Mod.Log.Debug("RoadBuilderGenerationDataSystem " + roadGenerationData.LeftHandTraffic);

			for (var i = 0; i < zoneBlockDataEntities.Length; i++)
			{
				if (prefabSystem.TryGetSpecificPrefab<ZoneBlockPrefab>(zoneBlockDataEntities[i], out var prefab))
				{
					if (prefab.name == "Zone Block")
					{
						roadGenerationData.ZoneBlockPrefab = prefab;

						break;
					}
				}
			}

			var outsideConnectionDataQuery = SystemAPI.QueryBuilder().WithAll<OutsideConnectionData, TrafficSpawnerData>().Build();
			var outsideConnectionDataEntities = outsideConnectionDataQuery.ToEntityArray(Allocator.Temp);

			for (var i = 0; i < outsideConnectionDataEntities.Length; i++)
			{
				if (prefabSystem.TryGetSpecificPrefab<MarkerObjectPrefab>(outsideConnectionDataEntities[i], out var prefab))
				{
					if (prefab.name == "Road Outside Connection - Oneway")
					{
						roadGenerationData.RoadOutsideConnectionOneWay = prefab;
					}

					if (prefab.name == "Road Outside Connection - Twoway")
					{
						roadGenerationData.RoadOutsideConnectionTwoWay = prefab;
					}

					if (prefab.name == "Train Outside Connection - Oneway")
					{
						roadGenerationData.TrainOutsideConnectionOneWay = prefab;
					}

					if (prefab.name == "Train Outside Connection - Twoway")
					{
						roadGenerationData.TrainOutsideConnectionTwoWay = prefab;
					}
				}
			}

			var aggregateNetDataQuery = SystemAPI.QueryBuilder().WithAll<AggregateNetData>().Build();
			var aggregateNetDataEntities = aggregateNetDataQuery.ToEntityArray(Allocator.Temp);

			for (var i = 0; i < aggregateNetDataEntities.Length; i++)
			{
				if (prefabSystem.TryGetSpecificPrefab<AggregateNetPrefab>(aggregateNetDataEntities[i], out var prefab))
				{
					roadGenerationData.AggregateNetPrefabs[prefab.name] = prefab;
				}
			}

			var netSectionDataQuery = SystemAPI.QueryBuilder().WithAll<NetSectionData>().Build();
			var netSectionDataEntities = netSectionDataQuery.ToEntityArray(Allocator.Temp);

			for (var i = 0; i < netSectionDataEntities.Length; i++)
			{
				if (prefabSystem.TryGetSpecificPrefab<NetSectionPrefab>(netSectionDataEntities[i], out var prefab))
				{
					roadGenerationData.NetSectionPrefabs[prefab.name] = prefab;
				}
			}

			var serviceObjectDataQuery = SystemAPI.QueryBuilder().WithAll<ServiceData>().Build();
			var serviceObjectDataEntities = serviceObjectDataQuery.ToEntityArray(Allocator.Temp);

			for (var i = 0; i < serviceObjectDataEntities.Length; i++)
			{
				if (prefabSystem.TryGetSpecificPrefab<ServicePrefab>(serviceObjectDataEntities[i], out var prefab))
				{
					roadGenerationData.ServicePrefabs[prefab.name] = prefab;
				}
			}

			var pillarDataQuery = SystemAPI.QueryBuilder().WithAll<PillarData>().Build();
			var pillarDataEntities = pillarDataQuery.ToEntityArray(Allocator.Temp);

			for (var i = 0; i < pillarDataEntities.Length; i++)
			{
				if (prefabSystem.TryGetSpecificPrefab<StaticObjectPrefab>(pillarDataEntities[i], out var prefab))
				{
					roadGenerationData.PillarPrefabs[prefab.name] = prefab;
				}
			}

			if (prefabSystem.TryGetSpecificPrefab(new PrefabID(nameof(StaticObjectPrefab), "Train Pillar Placeholder"), out var trainPillar))
			{
				roadGenerationData.PillarPrefabs[trainPillar.name] = (trainPillar as StaticObjectPrefab)!;
			}

			if (prefabSystem.TryGetSpecificPrefab(new PrefabID(nameof(StaticObjectPrefab), "Subway Pillar Placeholder"), out var subwayPillar))
			{
				roadGenerationData.PillarPrefabs[subwayPillar.name] = (subwayPillar as StaticObjectPrefab)!;
			}

			if (prefabSystem.TryGetSpecificPrefab(new PrefabID(nameof(StaticObjectPrefab), "Pillar Pedestrian Placeholder"), out var pedestrianPillar))
			{
				roadGenerationData.PillarPrefabs[pedestrianPillar.name] = (pedestrianPillar as StaticObjectPrefab)!;
			}

			var uIGroupElementQuery = SystemAPI.QueryBuilder().WithAll<UIGroupElement>().Build();
			var uIGroupElementEntities = uIGroupElementQuery.ToEntityArray(Allocator.Temp);

			for (var i = 0; i < uIGroupElementEntities.Length; i++)
			{
				if (prefabSystem.TryGetSpecificPrefab<UIGroupPrefab>(uIGroupElementEntities[i], out var prefab))
				{
					roadGenerationData.UIGroupPrefabs[prefab.name] = prefab;
				}
			}

			var featureDataQuery = SystemAPI.QueryBuilder().WithAny<FeatureData, DevTreeNodeData, ObjectBuiltRequirementData, StrictObjectBuiltRequirementData, ZoneBuiltRequirementData>().Build();
			var featureDataEntities = featureDataQuery.ToEntityArray(Allocator.Temp);

			for (var i = 0; i < featureDataEntities.Length; i++)
			{
				if (prefabSystem.TryGetSpecificPrefab<PrefabBase>(featureDataEntities[i], out var prefab))
				{
					roadGenerationData.UnlocksPrefabs[prefab.name] = prefab;
				}
			}

			roadGenerationData.LaneGroupPrefabs = LaneGroups;

			RoadGenerationData = roadGenerationData;

			if (!firstTimeRun)
			{
				//AddStops();
				roadGenerationData.RoadBuilderPack = ScriptableObject.CreateInstance<AssetPackPrefab>();
				roadGenerationData.RoadBuilderPack.name = "RoadBuilderPack";
				roadGenerationData.RoadBuilderPack.AddComponent<UIObject>().m_Icon = "coui://roadbuildericons/Pack.svg";
				prefabSystem.AddPrefab(roadGenerationData.RoadBuilderPack);

				Mod.Log.Debug("RoadGenerationData First Created");

				firstTimeRun = true;

				RequireAnyForUpdate(
					SystemAPI.QueryBuilder().WithAll<ZoneBlockData>().WithAny<Updated, Created>().Build(),
					SystemAPI.QueryBuilder().WithAll<OutsideConnectionData, TrafficSpawnerData>().WithAny<Updated, Created>().Build(),
					SystemAPI.QueryBuilder().WithAll<AggregateNetData>().WithAny<Updated, Created>().Build(),
					SystemAPI.QueryBuilder().WithAll<NetSectionData>().WithAny<Updated, Created>().Build(),
					SystemAPI.QueryBuilder().WithAll<ServiceData>().WithAny<Updated, Created>().Build(),
					SystemAPI.QueryBuilder().WithAll<PillarData>().WithAny<Updated, Created>().Build(),
					SystemAPI.QueryBuilder().WithAll<UIGroupElement>().WithAny<Updated, Created>().Build());
			}

			Mod.Log.Info("Road Generation Data assembled");
		}

		private void AddStops()
		{
			var stopsQuery = SystemAPI.QueryBuilder().WithAll<PrefabData, TransportStopData>().Build();
			var stops = stopsQuery.ToEntityArray(Allocator.Temp);

			for (var i = 0; i < stops.Length; i++)
			{
				if (!prefabSystem.TryGetSpecificPrefab<MarkerObjectPrefab>(stops[i], out var stop) || stop?.name is not "Integrated Passenger Train Stop" and not "Integrated Cargo Train Stop" and not "Integrated Subway Stop - Side" and not "Integrated Subway Stop - Middle")
				{
					continue;
				}

				if (stop.Clone(stop.name.Substring(11)) is not MarkerObjectPrefab newStop)
				{
					continue;
				}

				newStop.AddComponent<ServiceObject>().m_Service = RoadGenerationData?.ServicePrefabs["Transportation"];

				newStop.AddOrGetComponent<UIObject>().m_Group = stop.name is "Integrated Passenger Train Stop" or "Integrated Cargo Train Stop"
					? RoadGenerationData?.UIGroupPrefabs["TransportationTrain"]
					: RoadGenerationData?.UIGroupPrefabs["TransportationSubway"];

				prefabSystem.AddPrefab(newStop);
			}
		}
	}
}
