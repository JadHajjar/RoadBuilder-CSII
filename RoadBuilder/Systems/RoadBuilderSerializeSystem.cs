using Colossal.Core;
using Colossal.Entities;
using Colossal.Serialization.Entities;

using Game;
using Game.Common;
using Game.Net;
using Game.Prefabs;
using Game.SceneFlow;
using Game.UI;

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
	public partial class RoadBuilderSerializeSystem : GameSystemBase
	{
		public const ushort CURRENT_VERSION = 5;

		// Version History
		public const ushort VER_REMOVE_AGGREGATE_TYPE = 2;
		public const ushort VER_FIX_PEDESTRIAN_ROADS = 3;
		public const ushort VER_MANAGEMENT_REWORK = 4;
		public const ushort VER_CHANGE_SOUND_BARRIER = 5;

		private const string EMPTY_CONFIG_MARKER = "EMPTY_CONFIG";

#nullable disable
		private static RoadBuilderSystem roadBuilderSystem;
		private static PrefabSystem prefabSystem;
		// Filled off the main thread, where Unity can't create prefabs.
		private static readonly List<INetworkConfig> _configsToAdd = new();
#nullable enable

		protected override void OnCreate()
		{
			base.OnCreate();

			roadBuilderSystem = World.GetOrCreateSystemManaged<RoadBuilderSystem>();
			prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
		}

		protected override void OnUpdate()
		{
			Mod.Log.Debug("RoadBuilderSerializeSystem.OnUpdate");

			roadBuilderSystem.UpdateConfigurationList();

			var placedNetworks = CreateNetworksList();

			foreach (var prefabName in placedNetworks)
			{
				var entity = EntityManager.CreateEntity();

				EntityManager.AddComponentData(entity, new NetworkConfigComponent { NetworkId = prefabName });
			}

			var savedCount = 0;

			foreach (var config in roadBuilderSystem.Configurations.Values)
			{
				if (Mod.Settings!.SaveUsedRoadsOnly && !placedNetworks.Contains(config.Config?.ID ?? string.Empty))
				{
					continue;
				}

				if (config.Config is not null)
				{
					try
					{
						LocalSaveUtil.Save(config.Config);
						savedCount++;
					}
					catch (Exception ex)
					{
						Mod.Log.Error(ex, $"Failed to save network config: {config.Config.GetType().Name} id={config.Config.ID}");
					}
				}
			}

			Mod.Log.Debug($"RoadBuilderSerializeSystem.OnUpdate: saved {savedCount} configs, {placedNetworks.Count} placed networks");
		}

		protected override void OnGamePreload(Purpose purpose, GameMode mode)
		{
			base.OnGamePreload(purpose, mode);

			_configsToAdd.Clear();
		}

		protected override void OnGameLoadingComplete(Purpose purpose, GameMode mode)
		{
			base.OnGameLoadingComplete(purpose, mode);

			var reloadNeeded = false;

			foreach (var config in _configsToAdd)
			{
				var prefab = roadBuilderSystem.AddPrefab(config);

				if (prefab is not null)
				{
					roadBuilderSystem.UpdatePrefab(prefab.Prefab);
					reloadNeeded = true;
				}
			}

			if (reloadNeeded)
			{
				Mod.ReloadActiveLocale();

				GameManager.instance.userInterface.appBindings.ShowMessageDialog(new MessageDialog("Options.SECTION[RoadBuilder.RoadBuilder.Mod]", "RoadBuilder.DIALOG_MESSAGE[ReloadSave]", "RoadBuilder.DIALOG_MESSAGE[Ok]"), null);
			}

			var sections = SystemAPI.QueryBuilder().WithAll<Edge, RoadBuilderNetwork, Game.Objects.SubObject>().Build().ToEntityArray(Allocator.Temp);

			for (var i = 0; i < sections.Length; i++)
			{
				var subObjects = EntityManager.GetBuffer<Game.Objects.SubObject>(sections[i]);
				var toDelete = new List<Entity>();

				for (var j = 0; j < subObjects.Length;)
				{
					var subobject = subObjects[j];

					if (prefabSystem.TryGetSpecificPrefab<PrefabBase>(EntityManager.GetComponentData<PrefabRef>(subobject.m_SubObject), out var prefab) && prefab?.name is "ParkingLotSidewaysDecal01" or "ParkingLotDiagonalDecal01")
					{
						toDelete.Add(subobject.m_SubObject);
						subObjects.RemoveAt(j);
					}
					else
					{
						j++;
					}
				}

				// Structural changes are deferred until the buffer is no longer used
				foreach (var subObject in toDelete)
				{
					EntityManager.AddComponent<Deleted>(subObject);
				}
			}

			roadBuilderSystem.UpdateConfigurationList(true);

			// Resetting the edges now would lose the roads the reload brings back.
			if (!reloadNeeded)
			{
				try
				{
					FixInvalidEdges();
				}
				catch (Exception ex)
				{
					Mod.Log.Error(ex);
				}
			}
		}

		private void FixInvalidEdges()
		{
			var edgeQuery = SystemAPI.QueryBuilder().WithAll<Edge, PrefabRef>().WithAny<Road, TrainTrack, TramTrack, SubwayTrack>().Build();
			var edges = edgeQuery.ToEntityArray(Allocator.Temp);
			var refs = edgeQuery.ToComponentDataArray<PrefabRef>(Allocator.Temp);
			var invalidEntities = new List<Entity>();

			for (var i = 0; i < edges.Length; i++)
			{
				if (refs[i].m_Prefab.Index < 0 || !prefabSystem.TryGetSpecificPrefab<NetGeometryPrefab>(refs[i], out var prefab))
				{
					invalidEntities.Add(edges[i]);
				}
			}

			if (invalidEntities.Count == 0)
			{
				return;
			}

			Mod.Log.WarnFormat("{0} invalid edges found", invalidEntities.Count);

			if (Mod.Settings?.AskToResetRoads != true)
			{
				FixEdges(invalidEntities);
			}
			else
			{
				MainThreadDispatcher.RegisterUpdater(() => GameManager.instance.userInterface.appBindings.ShowConfirmationDialog(new ConfirmationDialog("Options.SECTION[RoadBuilder.RoadBuilder.Mod]", "RoadBuilder.DIALOG_MESSAGE[FixInvalidEdges]", "Common.DIALOG_ACTION[Yes]", "Common.DIALOG_ACTION[No]"), msg =>
				{
					if (msg == 0)
					{
						FixEdges(invalidEntities);
					}
				}));
			}
		}

		private void FixEdges(List<Entity> invalidEntities)
		{
			// The confirmation dialog can be answered later, after some of these entities were removed
			invalidEntities = invalidEntities.Where(x => EntityManager.Exists(x) && !EntityManager.HasComponent<Deleted>(x)).ToList();

			var updateSystem = World.GetOrCreateSystemManaged<RoadBuilderPrefabUpdateSystem>();
			var smallRoadId = prefabSystem.TryGetSpecificPrefab(new PrefabID(nameof(RoadPrefab), "Small Road"), out var smallRoad) ? prefabSystem.GetEntity(smallRoad) : Entity.Null;
			var trainTrackId = prefabSystem.TryGetSpecificPrefab(new PrefabID(nameof(TrackPrefab), "Double Train Track"), out var trainTrack) ? prefabSystem.GetEntity(trainTrack) : Entity.Null;
			var tramTrackId = prefabSystem.TryGetSpecificPrefab(new PrefabID(nameof(TrackPrefab), "Double Tram Track"), out var tramTrack) ? prefabSystem.GetEntity(tramTrack) : Entity.Null;
			var subwayTrackId = prefabSystem.TryGetSpecificPrefab(new PrefabID(nameof(TrackPrefab), "Double Subway Track"), out var subwayTrack) ? prefabSystem.GetEntity(subwayTrack) : Entity.Null;

			foreach (var entity in invalidEntities)
			{
				updateSystem.UpdateEdge(entity);

				var prefabRef = new PrefabRef
				{
					m_Prefab =
						EntityManager.HasComponent<TrainTrack>(entity) ? trainTrackId :
						EntityManager.HasComponent<TramTrack>(entity) ? tramTrackId :
						EntityManager.HasComponent<SubwayTrack>(entity) ? subwayTrackId :
						smallRoadId
				};

				EntityManager.SetComponentData(entity, prefabRef);

				// Nodes were assigned the road builder prefab too, reset them if their prefab is also invalid
				if (EntityManager.TryGetComponent<Edge>(entity, out var edge))
				{
					FixNode(edge.m_Start, prefabRef);
					FixNode(edge.m_End, prefabRef);
				}
			}
		}

		private void FixNode(Entity node, PrefabRef prefabRef)
		{
			if (EntityManager.TryGetComponent<PrefabRef>(node, out var nodePrefabRef)
				&& (nodePrefabRef.m_Prefab.Index < 0 || !prefabSystem.TryGetSpecificPrefab<NetGeometryPrefab>(nodePrefabRef, out _)))
			{
				EntityManager.SetComponentData(node, prefabRef);
			}
		}

		private HashSet<string> CreateNetworksList()
		{
			var roadBuilderNetsQuery = SystemAPI.QueryBuilder().WithAll<Edge, PrefabRef>().Build();
			var prefabRefs = roadBuilderNetsQuery.ToComponentDataArray<PrefabRef>(Allocator.Temp);

			var list = new HashSet<string>();

			for (var i = 0; i < prefabRefs.Length; i++)
			{
				// Roads added on load point at a placeholder until the save is reloaded.
				if (EntityManager.TryGetComponent<PrefabData>(prefabRefs[i].m_Prefab, out var prefabData) && prefabData.m_Index < 0)
				{
					var obsoleteName = prefabSystem.GetObsoleteID(prefabData).GetName();

					if (roadBuilderSystem.Configurations.ContainsKey(obsoleteName))
					{
						list.Add(obsoleteName);
					}

					continue;
				}

				if (!prefabSystem.TryGetSpecificPrefab<PrefabBase>(prefabRefs[i], out var prefab) || prefab is not INetworkBuilderPrefab builderPrefab)
				{
					continue;
				}

				if (builderPrefab.Prefab.name != builderPrefab.Config?.ID)
				{
					Mod.Log.Error($"ANOMALY - NAME <> ID: {builderPrefab.Prefab.name} - {builderPrefab.Config?.ID}");
				}

				if (!list.Contains(builderPrefab.Prefab.name))
				{
					Mod.Log.Debug("Adding for save: " + builderPrefab.Config?.Name + " - " + builderPrefab.Config?.ID);

					list.Add(builderPrefab.Prefab.name);
				}
			}

			return list;
		}

		public static void DeserializeNetwork<TReader>(TReader reader) where TReader : IReader
		{
			reader.Read(out ushort version);
			reader.Read(out string type);

			Mod.Log.Debug($"DeserializeNetwork: type={type} version={version}");

			if (type == EMPTY_CONFIG_MARKER)
			{
				Mod.Log.Warn("Skipping a saved network that had no configuration at save time");

				return;
			}

			try
			{
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

				config.ApplyVersionChanges();

				Mod.Log.Debug($"DeserializeNetwork: {type} id={config.ID} name={config.Name}");

				_configsToAdd.Add(config);
			}
			catch (Exception ex)
			{
				Mod.Log.Error(ex, $"Failed to deserialize network config: type={type} version={version}");
			}
		}

		public static void SerializeNetwork<TWriter>(TWriter writer, string networkId) where TWriter : IWriter
		{
			writer.Write(CURRENT_VERSION);

			if (!roadBuilderSystem.Configurations.TryGetValue(networkId, out var prefab) || prefab.Config is null)
			{
				Mod.Log.Error("Trying to save a prefab that has no configuration: " + networkId);

				writer.Write(EMPTY_CONFIG_MARKER);

				return;
			}

			writer.Write(prefab.Config.GetType().Name);
			writer.Write(prefab.Config);
		}
	}
}
