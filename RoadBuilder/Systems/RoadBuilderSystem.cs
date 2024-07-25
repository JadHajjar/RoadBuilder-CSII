using Colossal.Entities;
using Colossal.Serialization.Entities;

using Game;
using Game.City;
using Game.Common;
using Game.Net;
using Game.Prefabs;
using Game.SceneFlow;
using Game.Tools;
using Game.UI.InGame;

using RoadBuilder.Domain;
using RoadBuilder.Domain.Components;
using RoadBuilder.Domain.Configurations;
using RoadBuilder.Domain.Prefabs;
using RoadBuilder.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;

using Unity.Collections;
using Unity.Entities;

namespace RoadBuilder.Systems
{
	public partial class RoadBuilderSystem : GameSystemBase, ISerializable, IDefaultSerializable
	{
		public const ushort CURRENT_VERSION = 1;

		private readonly Queue<INetworkBuilderPrefab> _updatedRoadPrefabsQueue = new();

		private EntityQuery prefabRefQuery;
		private RoadNameUtil roadNameUtil;
		private PrefabSystem prefabSystem;
		private PrefabUISystem prefabUISystem;
		private NetSectionsSystem netSectionsSystem;
		private RoadBuilderSerializeSystem roadBuilderSerializeSystem;
		private CityConfigurationSystem cityConfigurationSystem;
		private ModificationBarrier1 modificationBarrier;
		private Dictionary<Entity, Entity> toolbarUISystemLastSelectedAssets;

		public List<INetworkBuilderPrefab> Configurations { get; } = new();
		public RoadGenerationData RoadGenerationData { get; private set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
			prefabUISystem = World.GetOrCreateSystemManaged<PrefabUISystem>();
			netSectionsSystem = World.GetOrCreateSystemManaged<NetSectionsSystem>();
			roadBuilderSerializeSystem = World.GetOrCreateSystemManaged<RoadBuilderSerializeSystem>();
			cityConfigurationSystem = World.GetOrCreateSystemManaged<CityConfigurationSystem>();
			modificationBarrier = World.GetOrCreateSystemManaged<ModificationBarrier1>();
			roadNameUtil = new(this, prefabUISystem, netSectionsSystem);
			prefabRefQuery = SystemAPI.QueryBuilder()
				.WithAll<RoadBuilderNetwork, PrefabRef>()
				.WithNone<RoadBuilderUpdateFlagComponent, Temp>()
				.Build();

			toolbarUISystemLastSelectedAssets = typeof(ToolbarUISystem).GetField("m_LastSelectedAssets", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(World.GetOrCreateSystemManaged<ToolbarUISystem>()) as Dictionary<Entity, Entity>;
		}

		protected override void OnUpdate()
		{
			while (_updatedRoadPrefabsQueue.Count > 0)
			{
				if (RoadGenerationData is null)
				{
					Mod.Log.Warn("Generating roads before generation data was initialized");
				}

				var roadPrefab = _updatedRoadPrefabsQueue.Dequeue();
				var roadPrefabGeneration = new NetworkPrefabGenerationUtil(roadPrefab, RoadGenerationData ?? new());

				roadPrefabGeneration.GenerateRoad();

				roadPrefab.Prefab.name = roadPrefab.Config.ID;

				UpdatePrefab(roadPrefab.Prefab);
			}
		}

		protected override void OnGamePreload(Purpose purpose, GameMode mode)
		{
			base.OnGamePreload(purpose, mode);

			FillRoadGenerationData();
		}

		public void Deserialize<TReader>(TReader reader) where TReader : IReader
		{
			Mod.Log.Info(nameof(Deserialize));

			Configurations.Clear();

			reader.Read(out ushort version);
			reader.Read(out int length);

			var configs = new List<INetworkConfig>();

			for (var i = 0; i < length; i++)
			{
				reader.Read(out string type);

				INetworkConfig config = type switch
				{
					nameof(RoadConfig) => new RoadConfig(),
					nameof(TrackConfig) => new TrackConfig(),
					nameof(FenceConfig) => new FenceConfig(),
					nameof(PathConfig) => new PathConfig(),
					_ => throw new System.Exception("Unknown Configuration Type: " + type),
				};

				config.Version = version;

				reader.Read(config);

				configs.Add(config);
			}

			foreach (var config in LocalSaveUtil.LoadConfigs())
			{
				if (!configs.Any(x => x.ID == config.ID))
				{
					configs.Add(config);
				}
			}

			Mod.Log.Info($"{configs.Count} configurations loaded");

			InitializeExistingRoadPrefabs(configs);

			GameManager.instance.localizationManager.ReloadActiveLocale();
		}

		public void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
		{
			Mod.Log.Info(nameof(Serialize));

			writer.Write(CURRENT_VERSION);

			var configs = Configurations.FindAll(c => roadBuilderSerializeSystem.UsedConfigurations.Contains(c.Config.ID));

			writer.Write(configs.Count);

			foreach (var config in configs)
			{
				writer.Write(config.Config.GetType().Name);
				writer.Write(config.Config);
			}

			foreach (var config in Mod.Settings.SaveUsedRoadsOnly ? configs : Configurations)
			{
				if (config.Config.OriginalID != config.Config.ID)
				{
					LocalSaveUtil.Save(config.Config);
				}
			}

			Mod.Log.Info(nameof(Serialize) + " End");
		}

		public void SetDefaults(Context context)
		{
			Mod.Log.Info(nameof(SetDefaults));

			Configurations.Clear();

			InitializeExistingRoadPrefabs(LocalSaveUtil.LoadConfigs().ToList());
		}

		public void UpdateRoad(INetworkConfig config, Entity entity, bool createNewPrefab)
		{
			if (!EntityManager.TryGetComponent<PrefabRef>(entity, out var prefabRef))
			{
				return;
			}

			if (createNewPrefab || !(prefabSystem.TryGetPrefab<NetGeometryPrefab>(prefabRef, out var netPrefab) && netPrefab is INetworkBuilderPrefab networkBuilderPrefab))
			{
				CreateNewRoadPrefab(config, entity);

				return;
			}

			Mod.Log.Debug(string.Join("\r\n", config.Lanes.Select(x => x.GroupPrefabName ?? x.SectionPrefabName)));

			_updatedRoadPrefabsQueue.Enqueue(networkBuilderPrefab);
		}

		public INetworkConfig GetOrGenerateConfiguration(Entity entity)
		{
			if (!EntityManager.TryGetComponent<PrefabRef>(entity, out var prefabRef))
			{
				return null;
			}

			if (!prefabSystem.TryGetPrefab<NetGeometryPrefab>(prefabRef, out var roadPrefab))
			{
				return null;
			}

			if (roadPrefab is INetworkBuilderPrefab networkBuilderPrefab)
			{
				return networkBuilderPrefab.Config;
			}

			return new NetworkConfigGenerationUtil(roadPrefab, RoadGenerationData, prefabUISystem).GenerateConfiguration();
		}

		public INetworkConfig GenerateConfiguration(Entity entity)
		{
			if (!EntityManager.TryGetComponent<PrefabRef>(entity, out var prefabRef))
			{
				return null;
			}

			if (!prefabSystem.TryGetPrefab<NetGeometryPrefab>(prefabRef, out var roadPrefab))
			{
				return null;
			}

			return new NetworkConfigGenerationUtil(roadPrefab, RoadGenerationData, prefabUISystem).GenerateConfiguration();
		}

		private void InitializeExistingRoadPrefabs(List<INetworkConfig> configs)
		{
			Mod.Log.InfoFormat("{0} ({1})", nameof(InitializeExistingRoadPrefabs), configs.Count);

			foreach (var config in configs)
			{
				try
				{
					config.ApplyVersionChanges();

					var roadPrefab = NetworkPrefabGenerationUtil.CreatePrefab(config);

					if (prefabSystem.TryGetPrefab(roadPrefab.Prefab.GetPrefabID(), out var prefabBase))
					{
						roadPrefab = prefabBase as INetworkBuilderPrefab;

						roadPrefab.Config = config;
					}
					else
					{
						prefabSystem.AddPrefab(roadPrefab.Prefab);
					}

					var roadPrefabGeneration = new NetworkPrefabGenerationUtil(roadPrefab, RoadGenerationData ?? new());

					roadPrefabGeneration.GenerateRoad(false);

					roadPrefab.Prefab.name = roadPrefab.Config.ID;

					Configurations.Add(roadPrefab);

					UpdatePrefab(roadPrefab.Prefab);
				}
				catch (Exception ex)
				{
					Mod.Log.Error(ex);
				}
			}
		}

		private void CreateNewRoadPrefab(INetworkConfig config, Entity entity)
		{
			try
			{
				var roadPrefab = NetworkPrefabGenerationUtil.CreatePrefab(config);
				var roadPrefabGeneration = new NetworkPrefabGenerationUtil(roadPrefab, RoadGenerationData ?? new());

				roadPrefabGeneration.GenerateRoad();

				roadPrefab.Prefab.name = roadPrefab.Config.ID;

				prefabSystem.AddPrefab(roadPrefab.Prefab);

				Configurations.Add(roadPrefab);

				var prefabEntity = prefabSystem.GetEntity(roadPrefab.Prefab);
				var prefabRef = new PrefabRef { m_Prefab = prefabEntity };

				EntityManager.SetComponentData(entity, prefabRef);
				EntityManager.TryAddComponent<RoadBuilderNetwork>(entity);

				if (EntityManager.TryGetComponent<Edge>(entity, out var edge))
				{
					EntityManager.SetComponentData(edge.m_Start, prefabRef);
					EntityManager.SetComponentData(edge.m_End, prefabRef);
					EntityManager.TryAddComponent<RoadBuilderNetwork>(edge.m_Start);
					EntityManager.TryAddComponent<RoadBuilderNetwork>(edge.m_End);
				}

				_updatedRoadPrefabsQueue.Enqueue(roadPrefab);
			}
			catch (Exception ex)
			{
				Mod.Log.Error(ex);
			}
		}

		private void FillRoadGenerationData()
		{
			RoadGenerationData = new();

			var zoneBlockDataQuery = SystemAPI.QueryBuilder().WithAll<ZoneBlockData>().Build();
			var zoneBlockDataEntities = zoneBlockDataQuery.ToEntityArray(Allocator.Temp);

			RoadGenerationData.LeftHandTraffic = cityConfigurationSystem.leftHandTraffic;

			for (var i = 0; i < zoneBlockDataEntities.Length; i++)
			{
				if (prefabSystem.TryGetPrefab<ZoneBlockPrefab>(zoneBlockDataEntities[i], out var prefab))
				{
					if (prefab.name == "Zone Block")
					{
						RoadGenerationData.ZoneBlockPrefab = prefab;

						break;
					}
				}
			}

			var outsideConnectionDataQuery = SystemAPI.QueryBuilder().WithAll<OutsideConnectionData, TrafficSpawnerData>().Build();
			var outsideConnectionDataEntities = outsideConnectionDataQuery.ToEntityArray(Allocator.Temp);

			for (var i = 0; i < outsideConnectionDataEntities.Length; i++)
			{
				if (prefabSystem.TryGetPrefab<MarkerObjectPrefab>(outsideConnectionDataEntities[i], out var prefab))
				{
					if (prefab.name == "Road Outside Connection - Oneway")
					{
						RoadGenerationData.OutsideConnectionOneWay = prefab;
					}

					if (prefab.name == "Road Outside Connection - Twoway")
					{
						RoadGenerationData.OutsideConnectionTwoWay = prefab;
					}
				}
			}

			var aggregateNetDataQuery = SystemAPI.QueryBuilder().WithAll<AggregateNetData>().Build();
			var aggregateNetDataEntities = aggregateNetDataQuery.ToEntityArray(Allocator.Temp);

			for (var i = 0; i < aggregateNetDataEntities.Length; i++)
			{
				if (prefabSystem.TryGetPrefab<AggregateNetPrefab>(aggregateNetDataEntities[i], out var prefab))
				{
					RoadGenerationData.AggregateNetPrefabs[prefab.name] = prefab;
				}
			}

			var netSectionDataQuery = SystemAPI.QueryBuilder().WithAll<NetSectionData>().Build();
			var netSectionDataEntities = netSectionDataQuery.ToEntityArray(Allocator.Temp);

			for (var i = 0; i < netSectionDataEntities.Length; i++)
			{
				if (prefabSystem.TryGetPrefab<NetSectionPrefab>(netSectionDataEntities[i], out var prefab))
				{
					RoadGenerationData.NetSectionPrefabs[prefab.name] = prefab;
				}
			}

			var serviceObjectDataQuery = SystemAPI.QueryBuilder().WithAll<ServiceData>().Build();
			var serviceObjectDataEntities = serviceObjectDataQuery.ToEntityArray(Allocator.Temp);

			for (var i = 0; i < serviceObjectDataEntities.Length; i++)
			{
				if (prefabSystem.TryGetPrefab<ServicePrefab>(serviceObjectDataEntities[i], out var prefab))
				{
					RoadGenerationData.ServicePrefabs[prefab.name] = prefab;
				}
			}

			var pillarDataQuery = SystemAPI.QueryBuilder().WithAll<PillarData>().Build();
			var pillarDataEntities = pillarDataQuery.ToEntityArray(Allocator.Temp);

			for (var i = 0; i < pillarDataEntities.Length; i++)
			{
				if (prefabSystem.TryGetPrefab<StaticObjectPrefab>(pillarDataEntities[i], out var prefab))
				{
					RoadGenerationData.PillarPrefabs[prefab.name] = prefab;
				}
			}

			var uIGroupElementQuery = SystemAPI.QueryBuilder().WithAll<UIGroupElement>().Build();
			var uIGroupElementEntities = uIGroupElementQuery.ToEntityArray(Allocator.Temp);

			for (var i = 0; i < uIGroupElementEntities.Length; i++)
			{
				if (prefabSystem.TryGetPrefab<UIGroupPrefab>(uIGroupElementEntities[i], out var prefab))
				{
					RoadGenerationData.UIGroupPrefabs[prefab.name] = prefab;
				}
			}

			RoadGenerationData.LaneGroupPrefabs = netSectionsSystem.LaneGroups;
		}

		private void UpdatePrefab(NetGeometryPrefab prefab)
		{
			var entity = prefabSystem.GetEntity(prefab);

			prefabSystem.UpdatePrefab(prefab, entity);

			foreach (var kvp in toolbarUISystemLastSelectedAssets)
			{
				if (kvp.Value == entity)
				{
					toolbarUISystemLastSelectedAssets.Remove(kvp.Key);
					break;
				}
			}

			var uIGroupElements = SystemAPI.QueryBuilder().WithAll<UIGroupElement>().Build().ToEntityArray(Allocator.Temp);

			for (var i = 0; i < uIGroupElements.Length; i++)
			{
				var buffer = EntityManager.GetBuffer<UIGroupElement>(uIGroupElements[i]);

				for (var j = 0; j < buffer.Length; j++)
				{
					if (buffer[j].m_Prefab == entity)
					{
						buffer.RemoveAt(j);
						return;
					}
				}
			}
		}
	}
}
