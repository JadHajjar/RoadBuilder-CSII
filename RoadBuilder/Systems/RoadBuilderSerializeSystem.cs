﻿using Colossal.Serialization.Entities;

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

#nullable disable
		private static RoadBuilderSystem roadBuilderSystem;
		private static PrefabSystem prefabSystem;
		private static readonly List<INetworkBuilderPrefab> _prefabsToUpdate = new();
#nullable enable

		protected override void OnCreate()
		{
			base.OnCreate();

			roadBuilderSystem = World.GetOrCreateSystemManaged<RoadBuilderSystem>();
			prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
		}

		protected override void OnUpdate()
		{
			roadBuilderSystem.UpdateConfigurationList();

			var placedNetworks = CreateNetworksList();

			foreach (var prefabName in placedNetworks)
			{
				var entity = EntityManager.CreateEntity();

				EntityManager.AddComponentData(entity, new NetworkConfigComponent { NetworkId = prefabName });
			}

			foreach (var config in roadBuilderSystem.Configurations.Values)
			{
				if (Mod.Settings!.SaveUsedRoadsOnly && !placedNetworks.Contains(config.Config?.ID ?? string.Empty))
				{
					continue;
				}

				if (config.Config is not null)
				{
					LocalSaveUtil.Save(config.Config);
				}
			}
		}

		protected override void OnGameLoadingComplete(Purpose purpose, GameMode mode)
		{
			base.OnGameLoadingComplete(purpose, mode);

			foreach (var item in _prefabsToUpdate)
			{
				roadBuilderSystem.UpdatePrefab(item.Prefab);
			}

			if (_prefabsToUpdate.Count > 0)
			{
				Mod.ReloadActiveLocale();

				_prefabsToUpdate.Clear();

				GameManager.instance.userInterface.appBindings.ShowMessageDialog(new MessageDialog("Options.SECTION[RoadBuilder.RoadBuilder.Mod]", "RoadBuilder.DIALOG_MESSAGE[ReloadSave]", "RoadBuilder.DIALOG_MESSAGE[Ok]"), null);
			}

			var sections = SystemAPI.QueryBuilder().WithAll<Edge, RoadBuilderNetwork, Game.Objects.SubObject>().Build().ToEntityArray(Allocator.Temp);

			for (var i = 0; i < sections.Length; i++)
			{
				var subObjects = EntityManager.GetBuffer<Game.Objects.SubObject>(sections[i]);

				for (var j = 0; j < subObjects.Length;)
				{
					var subobject = subObjects[j];

					if (prefabSystem.TryGetSpecificPrefab<PrefabBase>(EntityManager.GetComponentData<PrefabRef>(subobject.m_SubObject), out var prefab) && prefab?.name is "ParkingLotSidewaysDecal01" or "ParkingLotDiagonalDecal01")
					{
						EntityManager.AddComponent<Deleted>(subobject.m_SubObject);
						subObjects.RemoveAt(j);
					}
					else
					{
						j++;
					}
				}
			}

			roadBuilderSystem.UpdateConfigurationList(true);

			try
			{
				FixInvalidEdges();
			}
			catch (Exception ex)
			{
				Mod.Log.Error(ex);
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

			if (Mod.Settings?.AskToResetRoads == true)
			{
				FixEdges(invalidEntities);
			}
			else
			{
				GameManager.instance.RegisterUpdater(() => GameManager.instance.userInterface.appBindings.ShowConfirmationDialog(new ConfirmationDialog("Options.SECTION[RoadBuilder.RoadBuilder.Mod]", "RoadBuilder.DIALOG_MESSAGE[FixInvalidEdges]", "Common.DIALOG_ACTION[Yes]", "Common.DIALOG_ACTION[No]"), msg =>
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
			var updateSystem = World.GetOrCreateSystemManaged<RoadBuilderPrefabUpdateSystem>();
			var smallRoadId = prefabSystem.TryGetSpecificPrefab(new PrefabID(nameof(RoadPrefab), "Small Road"), out var smallRoad) ? prefabSystem.GetEntity(smallRoad) : Entity.Null;
			var trainTrackId = prefabSystem.TryGetSpecificPrefab(new PrefabID(nameof(TrackPrefab), "Double Train Track"), out var trainTrack) ? prefabSystem.GetEntity(trainTrack) : Entity.Null;
			var tramTrackId = prefabSystem.TryGetSpecificPrefab(new PrefabID(nameof(TrackPrefab), "Double Tram Track"), out var tramTrack) ? prefabSystem.GetEntity(tramTrack) : Entity.Null;
			var subwayTrackId = prefabSystem.TryGetSpecificPrefab(new PrefabID(nameof(TrackPrefab), "Double Subway Track"), out var subwayTrack) ? prefabSystem.GetEntity(subwayTrack) : Entity.Null;

			foreach (var entity in invalidEntities)
			{
				updateSystem.UpdateEdge(entity);

				if (EntityManager.HasComponent<Road>(entity))
				{
					EntityManager.SetComponentData(entity, new PrefabRef
					{
						m_Prefab = smallRoadId
					});
				}
				else if (EntityManager.HasComponent<TrainTrack>(entity))
				{
					EntityManager.SetComponentData(entity, new PrefabRef
					{
						m_Prefab = trainTrackId
					});
				}
				else if (EntityManager.HasComponent<TramTrack>(entity))
				{
					EntityManager.SetComponentData(entity, new PrefabRef
					{
						m_Prefab = tramTrackId
					});
				}
				else if (EntityManager.HasComponent<SubwayTrack>(entity))
				{
					EntityManager.SetComponentData(entity, new PrefabRef
					{
						m_Prefab = subwayTrackId
					});
				}
				else
				{
					EntityManager.SetComponentData(entity, new PrefabRef
					{
						m_Prefab = smallRoadId
					});
				}
			}
		}

		private HashSet<string> CreateNetworksList()
		{
			var roadBuilderNetsQuery = SystemAPI.QueryBuilder().WithAll<Edge, PrefabRef>().Build();
			var prefabRefs = roadBuilderNetsQuery.ToComponentDataArray<PrefabRef>(Allocator.Temp);

			var list = new HashSet<string>();

			for (var i = 0; i < prefabRefs.Length; i++)
			{
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

			Mod.Log.Debug($"DeserializeNetwork: {type} {config.ID}");

			var prefab = roadBuilderSystem.AddPrefab(config);

			if (prefab is not null)
			{
				_prefabsToUpdate.Add(prefab);
			}
		}

		public static void SerializeNetwork<TWriter>(TWriter writer, string networkId) where TWriter : IWriter
		{
			if (!roadBuilderSystem.Configurations.TryGetValue(networkId, out var prefab) || prefab.Config is null)
			{
				Mod.Log.Error("Trying to save a prefab that has no configuration: " + networkId);

				return;
			}

			writer.Write(CURRENT_VERSION);
			writer.Write(prefab.Config.GetType().Name);
			writer.Write(prefab.Config);
		}
	}
}
