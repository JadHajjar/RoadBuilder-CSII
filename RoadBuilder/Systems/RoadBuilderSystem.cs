using Colossal.Core;
using Colossal.Entities;

using Game;
using Game.City;
using Game.Common;
using Game.Net;
using Game.Prefabs;
using Game.SceneFlow;
using Game.UI.InGame;

using RoadBuilder.Domain.Components;
using RoadBuilder.Domain.Configurations;
using RoadBuilder.Domain.Prefabs;
using RoadBuilder.Systems.UI;
using RoadBuilder.Utilities;

using System;
using System.Collections.Generic;

using Unity.Collections;
using Unity.Entities;

namespace RoadBuilder.Systems
{
	public partial class RoadBuilderSystem : GameSystemBase
	{
		private readonly Queue<(INetworkBuilderPrefab prefab, bool generateId)> _updatedRoadPrefabsQueue = new();
		private readonly Dictionary<string, INetworkBuilderPrefab> deletedPrefabs = new();

#nullable disable
		private PrefabSystem prefabSystem;
		private RoadBuilderUISystem roadBuilderUISystem;
		private RoadBuilderNetSectionsSystem netSectionsSystem;
		private RoadBuilderSerializeSystem roadBuilderSerializeSystem;
		private CityConfigurationSystem cityConfigurationSystem;
		private RoadBuilderGenerationDataSystem roadGenerationDataSystem;
		private ModificationBarrier1 modificationBarrier;
#nullable enable

		private Dictionary<Entity, Entity>? toolbarUISystemLastSelectedAssets;
		private DateTime lastUpdateRequest;

		public event Action? ConfigurationsUpdated;

		public Dictionary<string, INetworkBuilderPrefab> Configurations { get; } = new();
		public bool IsDragging { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
			roadBuilderUISystem = World.GetOrCreateSystemManaged<RoadBuilderUISystem>();
			netSectionsSystem = World.GetOrCreateSystemManaged<RoadBuilderNetSectionsSystem>();
			roadBuilderSerializeSystem = World.GetOrCreateSystemManaged<RoadBuilderSerializeSystem>();
			cityConfigurationSystem = World.GetOrCreateSystemManaged<CityConfigurationSystem>();
			roadGenerationDataSystem = World.GetOrCreateSystemManaged<RoadBuilderGenerationDataSystem>();
			modificationBarrier = World.GetOrCreateSystemManaged<ModificationBarrier1>();

			new RoadNameUtil(this, roadBuilderUISystem, netSectionsSystem);

			// Delay getting the toolbar ui system assets for the next frame
			MainThreadDispatcher.RegisterUpdater(() =>
			{
				try
				{
					toolbarUISystemLastSelectedAssets ??= typeof(ToolbarUISystem).GetField("m_LastSelectedAssets", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(World.GetOrCreateSystemManaged<ToolbarUISystem>()) as Dictionary<Entity, Entity>;
				}
				catch (Exception ex)
				{
					Mod.Log.Warn(ex, "Failed to retrieve ToolbarUISystem.m_LastSelectedAssets via reflection");
				}
			});
		}

		protected override void OnUpdate()
		{
			if (IsDragging || _updatedRoadPrefabsQueue.Count == 0 || lastUpdateRequest > DateTime.Now.AddSeconds(-0.66))
			{
				return;
			}

			Mod.Log.Debug("RoadBuilderSystem.OnUpdate: " + _updatedRoadPrefabsQueue.Count);

			do
			{
				if (roadGenerationDataSystem.RoadGenerationData is null)
				{
					Mod.Log.Warn("Generating roads before generation data was initialized");

					_updatedRoadPrefabsQueue.Clear();

					return;
				}

				var item = _updatedRoadPrefabsQueue.Dequeue();

				DoUpdatePrefab(item.prefab, item.generateId);
			}
			while (_updatedRoadPrefabsQueue.Count > 0);
		}

		private void DoUpdatePrefab(INetworkBuilderPrefab prefab, bool generateId)
		{
			if (roadGenerationDataSystem.RoadGenerationData is null)
			{
				Mod.Log.Warn("Updating roads before generation data was initialized");

				return;
			}

			if (prefab.Config?.ID is null)
			{
				Mod.Log.Error($"NULL Config for prefab '{prefab.Prefab.name}'");

				return;
			}

			var roadPrefabGeneration = new NetworkPrefabGenerationUtil(prefab, roadGenerationDataSystem.RoadGenerationData);

			if (!roadPrefabGeneration.GenerateRoad(generateId))
			{
				Mod.Log.Error($"Skipping prefab update after failed generation: '{prefab.Prefab.name}'");

				return;
			}

			prefab.Prefab.name = prefab.Config.ID;

			UpdatePrefab(prefab.Prefab);
		}

		public void UpdateRoad(INetworkConfig config, Entity entity, bool createNewPrefab, bool now = false)
		{
			INetworkBuilderPrefab networkBuilderPrefab;

			if (entity == Entity.Null)
			{
				if (!Configurations.TryGetValue(config.ID ?? string.Empty, out networkBuilderPrefab))
				{
					return;
				}
			}
			else
			{
				if (!EntityManager.TryGetComponent<PrefabRef>(entity, out var prefabRef))
				{
					return;
				}

				if (createNewPrefab || !(prefabSystem.TryGetSpecificPrefab<NetGeometryPrefab>(prefabRef, out var netPrefab) && netPrefab is INetworkBuilderPrefab _networkBuilderPrefab))
				{
					CreateNewRoadPrefab(config, entity);

					return;
				}

				networkBuilderPrefab = _networkBuilderPrefab;
			}

			if (now)
			{
				DoUpdatePrefab(networkBuilderPrefab, true);
			}
			else
			{
				lastUpdateRequest = DateTime.Now;

				_updatedRoadPrefabsQueue.Enqueue((networkBuilderPrefab, true));
			}
		}

		public INetworkConfig? GetOrGenerateConfiguration(Entity entity)
		{
			if (!EntityManager.TryGetComponent<PrefabRef>(entity, out var prefabRef))
			{
				return null;
			}

			if (!prefabSystem.TryGetSpecificPrefab<NetGeometryPrefab>(prefabRef, out var roadPrefab))
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

			try
			{
				return new NetworkConfigGenerationUtil(roadPrefab, roadGenerationDataSystem.RoadGenerationData, roadBuilderUISystem).GenerateConfiguration();
			}
			catch (Exception ex)
			{
				Mod.Log.Error(ex, $"Failed to generate configuration for prefab '{roadPrefab?.name}'");
				return null;
			}
		}

		public INetworkConfig? GenerateConfiguration(Entity entity)
		{
			if (!EntityManager.TryGetComponent<PrefabRef>(entity, out var prefabRef))
			{
				return null;
			}

			if (!prefabSystem.TryGetSpecificPrefab<NetGeometryPrefab>(prefabRef, out var roadPrefab))
			{
				return null;
			}

			if (roadGenerationDataSystem.RoadGenerationData is null)
			{
				return null;
			}

			try
			{
				return new NetworkConfigGenerationUtil(roadPrefab, roadGenerationDataSystem.RoadGenerationData, roadBuilderUISystem).GenerateConfiguration();
			}
			catch (Exception ex)
			{
				Mod.Log.Error(ex, $"Failed to generate configuration for prefab '{roadPrefab?.name}'");
				return null;
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
				var roadPrefab = AddPrefab(config, generateId: true);

				if (roadPrefab is null)
				{
					return;
				}

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
			}
			catch (Exception ex)
			{
				Mod.Log.Error(ex);
			}
		}

		public void UpdatePrefab(NetGeometryPrefab prefab)
		{
			Mod.Log.Debug($"UpdatePrefab: {prefab?.name}");

			var entity = prefabSystem.GetEntity(prefab);

			prefabSystem.UpdatePrefab(prefab, entity);

			if (toolbarUISystemLastSelectedAssets is not null)
			{
				foreach (var kvp in toolbarUISystemLastSelectedAssets)
				{
					if (kvp.Value == entity)
					{
						toolbarUISystemLastSelectedAssets.Remove(kvp.Key);
						break;
					}
				}
			}

			var uIGroupElements = SystemAPI.QueryBuilder().WithAll<UIGroupElement>().Build().ToEntityArray(Allocator.Temp);

			for (var i = 0; i < uIGroupElements.Length; i++)
			{
				var buffer = EntityManager.GetBuffer<UIGroupElement>(uIGroupElements[i]);

				for (var j = buffer.Length - 1; j >= 0; j--)
				{
					if (buffer[j].m_Prefab == entity)
					{
						buffer.RemoveAt(j);
					}
				}
			}
		}

		public INetworkBuilderPrefab? AddPrefab(INetworkConfig config, bool generateId = false)
		{
			try
			{
				if (config.ID is not null and not "" && Configurations.ContainsKey(config.ID))
				{
					Mod.Log.Debug("Trying to add a road that already exists: " + config.ID);

					return null;
				}

				if (roadGenerationDataSystem.RoadGenerationData is null)
				{
					Mod.Log.Warn("Generating roads before generation data was initialized");

					return null;
				}

				// A road deleted this session is still registered in the prefab system under its ID,
				// so it has to be revived instead of added again
				if (!generateId && config.ID is not null and not "" && deletedPrefabs.TryGetValue(config.ID, out var deletedPrefab) && deletedPrefab.Prefab.GetType() == config.GetPrefabType())
				{
					return RestoreDeletedPrefab(deletedPrefab, config);
				}

				var roadPrefab = NetworkPrefabGenerationUtil.CreatePrefab(config);

				var roadPrefabGeneration = new NetworkPrefabGenerationUtil(roadPrefab, roadGenerationDataSystem.RoadGenerationData);

				if (!roadPrefabGeneration.GenerateRoad(generateId))
				{
					UnityEngine.Object.Destroy(roadPrefab.Prefab);

					return null;
				}

				if (roadPrefab.Config?.ID is null)
				{
					Mod.Log.Error($"NULL Config for prefab '{roadPrefab.Prefab.name}'");

					return null;
				}

				roadPrefab.Prefab.name = roadPrefab.Config.ID;

				if (!prefabSystem.AddPrefab(roadPrefab.Prefab))
				{
					Mod.Log.Error($"Unable to add prefab {roadPrefab.Prefab.name} config name: {roadPrefab.Config.Name}");

					return null;
				}

				_updatedRoadPrefabsQueue.Enqueue((roadPrefab, false));

				Mod.Log.Debug("Added Prefab: " + roadPrefab.Prefab.name);

				return roadPrefab;
			}
			catch (Exception ex)
			{
				Mod.Log.Error(ex);

				return null;
			}
		}

		private INetworkBuilderPrefab? RestoreDeletedPrefab(INetworkBuilderPrefab prefab, INetworkConfig config)
		{
			prefab.Config = config;

			if (!new NetworkPrefabGenerationUtil(prefab, roadGenerationDataSystem.RoadGenerationData!).GenerateRoad(false))
			{
				prefab.Config = null;

				return null;
			}

			prefab.Deleted = false;
			prefab.Prefab.name = config.ID;

			deletedPrefabs.Remove(config.ID!);

			UpdatePrefab(prefab.Prefab);

			Mod.Log.Debug("Restored Deleted Prefab: " + prefab.Prefab.name);

			return prefab;
		}

		public void UpdateConfigurationList(bool generateNewThumbnails = false)
		{
			// Discarded prefabs are included so deleted roads can be tracked and revived
			var roadBuilderConfigsQuery = SystemAPI.QueryBuilder().WithAny<RoadBuilderPrefabData, DiscardedRoadBuilderPrefab>().Build();
			var prefabs = roadBuilderConfigsQuery.ToEntityArray(Allocator.Temp);

			Configurations.Clear();
			deletedPrefabs.Clear();

			for (var i = 0; i < prefabs.Length; i++)
			{
				if (prefabSystem.GetPrefab<PrefabBase>(prefabs[i]) is not INetworkBuilderPrefab prefab)
				{
					continue;
				}

				if (prefab.Deleted)
				{
					deletedPrefabs[prefab.Prefab.name] = prefab;

					continue;
				}

				Mod.Log.Debug($"Configuration Found: {prefab.Prefab.name} - {prefab.Config?.ID}");

				try
				{
					if (generateNewThumbnails && roadGenerationDataSystem.RoadGenerationData is not null)
					{
						var thumbnail = new ThumbnailGenerationUtil(prefab, roadGenerationDataSystem.RoadGenerationData).GenerateThumbnail();

						if (thumbnail is not null and not "")
						{
							prefab.Prefab.AddOrGetComponent<UIObject>().m_Icon = thumbnail;
						}
					}

					if (prefab.Prefab.name != prefab.Config?.ID && prefab.Config is not null)
					{
						Mod.Log.Warn($"Configuration Mismatch: {prefab.Prefab.name} - {prefab.Config?.ID}");

						prefab.Prefab.name = prefab.Config?.ID;

						UpdatePrefab(prefab.Prefab);
					}
				}
				catch (Exception ex)
				{
					Mod.Log.Error(ex, $"Error while updating configuration: {prefab.Prefab.name} - {prefab.Config?.ID}");
				}

				// Keyed after the mismatch repair above so lookups by config ID always match
				Configurations[prefab.Prefab.name ?? string.Empty] = prefab;
			}

			Mod.Log.Debug("Configuration Count: " + Configurations.Count);

			ConfigurationsUpdated?.Invoke();
		}
	}
}
