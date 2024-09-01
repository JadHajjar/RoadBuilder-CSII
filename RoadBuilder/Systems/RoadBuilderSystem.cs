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

		private PrefabSystem prefabSystem;
		private PrefabUISystem prefabUISystem;
		private RoadBuilderNetSectionsSystem netSectionsSystem;
		private RoadBuilderSerializeSystem roadBuilderSerializeSystem;
		private CityConfigurationSystem cityConfigurationSystem;
		private RoadBuilderGenerationDataSystem roadGenerationDataSystem;
		private ModificationBarrier1 modificationBarrier;
		private Dictionary<Entity, Entity> toolbarUISystemLastSelectedAssets;
		private DateTime lastUpdateRequest;

		public event Action ConfigurationsUpdated;

		public Dictionary<string, INetworkBuilderPrefab> Configurations { get; } = new();
		public bool IsDragging { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
			prefabUISystem = World.GetOrCreateSystemManaged<PrefabUISystem>();
			netSectionsSystem = World.GetOrCreateSystemManaged<RoadBuilderNetSectionsSystem>();
			roadBuilderSerializeSystem = World.GetOrCreateSystemManaged<RoadBuilderSerializeSystem>();
			cityConfigurationSystem = World.GetOrCreateSystemManaged<CityConfigurationSystem>();
			roadGenerationDataSystem = World.GetOrCreateSystemManaged<RoadBuilderGenerationDataSystem>();
			modificationBarrier = World.GetOrCreateSystemManaged<ModificationBarrier1>();

			new RoadNameUtil(this, World.GetOrCreateSystemManaged<RoadBuilderUISystem>(), prefabUISystem, netSectionsSystem);

			// Delay getting the toolbar ui system assets for the next frame
			GameManager.instance.RegisterUpdater(() => toolbarUISystemLastSelectedAssets ??= typeof(ToolbarUISystem).GetField("m_LastSelectedAssets", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(World.GetOrCreateSystemManaged<ToolbarUISystem>()) as Dictionary<Entity, Entity>);
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
				var roadPrefabGeneration = new NetworkPrefabGenerationUtil(item.prefab, roadGenerationDataSystem.RoadGenerationData);

				roadPrefabGeneration.GenerateRoad(item.generateId);

				item.prefab.Prefab.name = item.prefab.Config.ID;

				UpdatePrefab(item.prefab.Prefab);
			}
			while (_updatedRoadPrefabsQueue.Count > 0);
		}

		public void UpdateRoad(INetworkConfig config, Entity entity, bool createNewPrefab)
		{
			INetworkBuilderPrefab networkBuilderPrefab;

			if (entity == Entity.Null)
			{
				if (!Configurations.TryGetValue(config.ID, out networkBuilderPrefab))
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

				if (createNewPrefab || !(prefabSystem.TryGetPrefab<NetGeometryPrefab>(prefabRef, out var netPrefab) && netPrefab is INetworkBuilderPrefab _networkBuilderPrefab))
				{
					CreateNewRoadPrefab(config, entity);

					return;
				}

				networkBuilderPrefab = _networkBuilderPrefab;
			}

			lastUpdateRequest = DateTime.Now;

			_updatedRoadPrefabsQueue.Enqueue((networkBuilderPrefab, true));
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
			var entity = prefabSystem.GetEntity(prefab);

			EntityManager.AddComponent<DiscardedRoadBuilderPrefab>(entity);

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

		public INetworkBuilderPrefab AddPrefab(INetworkConfig config, bool generateId = false)
		{
			try
			{
				if (config.ID is not null and not "" && Configurations.ContainsKey(config.ID))
				{
					Mod.Log.Debug("Trying to add a road that already exists: " + config.ID);

					return null;
				}

				var roadPrefab = NetworkPrefabGenerationUtil.CreatePrefab(config);

				var roadPrefabGeneration = new NetworkPrefabGenerationUtil(roadPrefab, roadGenerationDataSystem.RoadGenerationData);

				roadPrefabGeneration.GenerateRoad(generateId);

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

		public void UpdateConfigurationList(bool generateNewThumbnails = false)
		{
			var roadBuilderConfigsQuery = SystemAPI.QueryBuilder().WithAll<RoadBuilderPrefabData>().WithNone<DiscardedRoadBuilderPrefab>().Build();
			var prefabs = roadBuilderConfigsQuery.ToEntityArray(Allocator.Temp);

			Configurations.Clear();

			for (var i = 0; i < prefabs.Length; i++)
			{
				if (prefabSystem.GetPrefab<PrefabBase>(prefabs[i]) is INetworkBuilderPrefab prefab && !prefab.Deleted)
				{
					Configurations[prefab.Prefab.name] = prefab;

					Mod.Log.Debug($"Configuration Found: {prefab.Prefab.name} - {prefab.Config.ID}");

					if (generateNewThumbnails)
					{
						var thumbnail = new ThumbnailGenerationUtil(prefab, roadGenerationDataSystem.RoadGenerationData).GenerateThumbnail();

						if (thumbnail is not null and not "")
						{
							prefab.Prefab.AddOrGetComponent<UIObject>().m_Icon = thumbnail;
						}
					}

					if (prefab.Prefab.name != prefab.Config.ID)
					{
						Mod.Log.Warn($"Configuration Mismatch: {prefab.Prefab.name} - {prefab.Config.ID}");

						prefab.Prefab.name = prefab.Config.ID;

						UpdatePrefab(prefab.Prefab);
					}
				}
			}

			Mod.Log.Debug("Configuration Count: " + Configurations.Count);

			ConfigurationsUpdated?.Invoke();
		}
	}
}
