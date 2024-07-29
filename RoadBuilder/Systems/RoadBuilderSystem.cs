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

using RoadBuilder.Domain.Components;
using RoadBuilder.Domain.Configurations;
using RoadBuilder.Domain.Prefabs;
using RoadBuilder.Systems.UI;
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
		private RoadGenerationDataSystem roadGenerationDataSystem;
		private ModificationBarrier1 modificationBarrier;
		private Dictionary<Entity, Entity> toolbarUISystemLastSelectedAssets;

		public List<INetworkBuilderPrefab> Configurations { get; } = new();

		protected override void OnCreate()
		{
			base.OnCreate();

			prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
			prefabUISystem = World.GetOrCreateSystemManaged<PrefabUISystem>();
			netSectionsSystem = World.GetOrCreateSystemManaged<NetSectionsSystem>();
			roadBuilderSerializeSystem = World.GetOrCreateSystemManaged<RoadBuilderSerializeSystem>();
			cityConfigurationSystem = World.GetOrCreateSystemManaged<CityConfigurationSystem>();
			roadGenerationDataSystem = World.GetOrCreateSystemManaged<RoadGenerationDataSystem>();
			modificationBarrier = World.GetOrCreateSystemManaged<ModificationBarrier1>();
			roadNameUtil = new(this, World.GetOrCreateSystemManaged<RoadBuilderUISystem>(), prefabUISystem, netSectionsSystem);
			prefabRefQuery = SystemAPI.QueryBuilder()
				.WithAll<RoadBuilderNetwork, PrefabRef>()
				.WithNone<RoadBuilderUpdateFlagComponent, Temp>()
				.Build();
		}

		protected override void OnUpdate()
		{
			while (_updatedRoadPrefabsQueue.Count > 0)
			{
				if (roadGenerationDataSystem.RoadGenerationData is null)
				{
					Mod.Log.Warn("Generating roads before generation data was initialized");

					_updatedRoadPrefabsQueue.Clear();

					return;
				}

				var roadPrefab = _updatedRoadPrefabsQueue.Dequeue();
				var roadPrefabGeneration = new NetworkPrefabGenerationUtil(roadPrefab, roadGenerationDataSystem.RoadGenerationData);

				roadPrefabGeneration.GenerateRoad();

				roadPrefab.Prefab.name = roadPrefab.Config.ID;

				UpdatePrefab(roadPrefab.Prefab);

				GameManager.instance.localizationManager.ReloadActiveLocale();
			}
		}

		protected override void OnGamePreload(Purpose purpose, GameMode mode)
		{
			base.OnGamePreload(purpose, mode);

			toolbarUISystemLastSelectedAssets ??= typeof(ToolbarUISystem).GetField("m_LastSelectedAssets", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(World.GetOrCreateSystemManaged<ToolbarUISystem>()) as Dictionary<Entity, Entity>;
		}

		protected override void OnGameLoadingComplete(Purpose purpose, GameMode mode)
		{
			base.OnGameLoadingComplete(purpose, mode);

			if (mode == GameMode.Game)
			{
				GameManager.instance.localizationManager.ReloadActiveLocale();
			}
		}

		public void Deserialize<TReader>(TReader reader) where TReader : IReader
		{
			Mod.Log.Info(nameof(Deserialize));

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
					_ => throw new Exception("Unknown Configuration Type: " + type),
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
		}

		public void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
		{
			Mod.Log.Info(nameof(Serialize));

			var query = SystemAPI.QueryBuilder().WithAll<RoadBuilderPrefabData>().Build();
			var prefabEntities = query.ToEntityArray(Allocator.Temp);
			var configs = new List<INetworkBuilderPrefab>();

			for (var i = 0; i < prefabEntities.Length; i++)
			{
				if (prefabSystem.TryGetPrefab<PrefabBase>(prefabEntities[i], out var prefabBase) && prefabBase is INetworkBuilderPrefab prefab)
				{
					configs.Add(prefab);
				}
			}

			writer.Write(CURRENT_VERSION);

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
		{ }

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

			if (roadGenerationDataSystem.RoadGenerationData is null)
			{
				return null;
			}

			return new NetworkConfigGenerationUtil(roadPrefab, roadGenerationDataSystem.RoadGenerationData, prefabUISystem).GenerateConfiguration();
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

			if (roadGenerationDataSystem.RoadGenerationData is null)
			{
				return null;
			}

			return new NetworkConfigGenerationUtil(roadPrefab, roadGenerationDataSystem.RoadGenerationData, prefabUISystem).GenerateConfiguration();
		}

		public void InitializeExistingRoadPrefabs(List<INetworkConfig> configs)
		{
			Mod.Log.InfoFormat("{0} ({1})", nameof(InitializeExistingRoadPrefabs), configs.Count);

			if (roadGenerationDataSystem.RoadGenerationData is null)
			{
				Mod.Log.Warn("Generating roads before generation data was initialized");

				return;
			}

			foreach (var config in configs)
			{
				if (Configurations.Any(x => x.Config.ID == config.ID))
				{
					continue;
				}

				try
				{
					config.ApplyVersionChanges();

					var roadPrefab = NetworkPrefabGenerationUtil.CreatePrefab(config);
					var exists = false;

					if (exists = prefabSystem.TryGetPrefab(roadPrefab.Prefab.GetPrefabID(), out var prefabBase))
					{
						roadPrefab = prefabBase as INetworkBuilderPrefab;

						roadPrefab.Config = config;
					}

					var roadPrefabGeneration = new NetworkPrefabGenerationUtil(roadPrefab, roadGenerationDataSystem.RoadGenerationData);

					roadPrefabGeneration.GenerateRoad(false);

					roadPrefab.Prefab.name = roadPrefab.Config.ID;

					Configurations.Add(roadPrefab);

					if (exists)
					{
						UpdatePrefab(roadPrefab.Prefab);
					}
					else if (!prefabSystem.AddPrefab(roadPrefab.Prefab))
					{
						Mod.Log.Error($"Unable to add prefab {roadPrefab.Prefab.name} config name: {roadPrefab.Config.Name}!");
						continue;
					}
				}
				catch (Exception ex)
				{
					Mod.Log.Error(ex);
				}
			}
		}

		private void CreateNewRoadPrefab(INetworkConfig config, Entity entity)
		{
			if (roadGenerationDataSystem.RoadGenerationData is null)
			{
				Mod.Log.Warn("Generating roads before generation data was initialized");

				return;
			}

			try
			{
				var roadPrefab = NetworkPrefabGenerationUtil.CreatePrefab(config);
				var roadPrefabGeneration = new NetworkPrefabGenerationUtil(roadPrefab, roadGenerationDataSystem.RoadGenerationData);

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

		public void UpdatePrefab(NetGeometryPrefab prefab)
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
