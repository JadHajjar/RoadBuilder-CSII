using Colossal.IO.AssetDatabase.Internal;

using Game;
using Game.Common;
using Game.Prefabs;
using Game.SceneFlow;

using RoadBuilder.Domain.Components.Prefabs;
using RoadBuilder.Domain.Enums;
using RoadBuilder.Domain.Prefabs;
using RoadBuilder.LaneGroups;

using System;
using System.Collections.Generic;

using Unity.Collections;
using Unity.Entities;

using UnityEngine;

namespace RoadBuilder.Systems
{
	public partial class NetSectionsSystem : GameSystemBase
	{
		private PrefabSystem prefabSystem;
		private EntityQuery prefabQuery;
		private EntityQuery allPrefabQuery;
		private bool customSectionsSetUp;
		private bool initialSetupFinished;

		public event Action SectionsAdded;

		public Dictionary<string, NetSectionPrefab> NetSections { get; } = new();
		public Dictionary<string, NetPiecePrefab> NetPieces { get; } = new();
		public Dictionary<string, NetLanePrefab> NetLanes { get; } = new();
		public Dictionary<string, LaneGroupPrefab> LaneGroups { get; } = new();

		protected override void OnCreate()
		{
			base.OnCreate();

			prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
			prefabQuery = SystemAPI.QueryBuilder().WithAll<Created, PrefabData>().WithAny<NetSectionData, NetPieceData, NetLaneData>().Build();
			allPrefabQuery = SystemAPI.QueryBuilder().WithAll<PrefabData>().WithAny<NetSectionData, NetPieceData, NetLaneData>().Build();
		}

		protected override void OnUpdate()
		{
			if (!initialSetupFinished)
			{
				RequireForUpdate(prefabQuery);
			}

			try
			{
				var entities = (initialSetupFinished ? prefabQuery : allPrefabQuery).ToEntityArray(Allocator.Temp);

				for (var i = 0; i < entities.Length; i++)
				{
					if (!prefabSystem.TryGetPrefab<PrefabBase>(entities[i], out var prefab))
					{
						continue;
					}

					if (prefab is NetSectionPrefab netSectionPrefab)
					{
						NetSections[netSectionPrefab.name] = netSectionPrefab;
					}
					else if (prefab is NetPiecePrefab netPiecePrefab)
					{
						NetPieces[netPiecePrefab.name] = netPiecePrefab;
					}
					else if (prefab is NetLanePrefab netLanePrefab)
					{
						NetLanes[netLanePrefab.name] = netLanePrefab;
					}
				}

				if (!customSectionsSetUp && NetSections.Count > 0)
				{
					//AddCustomSections();

					AddCustomGroups();

					AddCustomPrefabComponents();

					GameManager.instance.localizationManager.ReloadActiveLocale();

					customSectionsSetUp = true;
				}

				SectionsAdded?.Invoke();

				initialSetupFinished = true;
			}
			catch (Exception ex)
			{
				Mod.Log.Error(ex);
			}
		}

		private void AddCustomSections()
		{
			var twoWaySection = NetSections["Car Drive Section 3"].Clone("Car Drive Twoway Section 3") as NetSectionPrefab;
			var twoWayPiece = NetPieces["Car Drive Piece 3"].Clone("Car Drive Twoway Piece 3") as NetPiecePrefab;
			var twoWayPieceFlat = NetPieces["Car Drive Piece 3 - Flat"].Clone("Car Drive Twoway Piece 3 - Flat") as NetPiecePrefab;
			var twoWayLane = NetLanes["Car Drive Lane 3"].Clone("Car Drive Twoway Lane 3") as NetLanePrefab;

			var lanes = twoWayPiece.AddOrGetComponent<NetPieceLanes>();
			var lanesFlat = twoWayPieceFlat.AddOrGetComponent<NetPieceLanes>();

			lanes.m_Lanes[0].m_Lane = twoWayLane;
			lanesFlat.m_Lanes[0].m_Lane = twoWayLane;

			foreach (var item in twoWaySection.m_Pieces)
			{
				if (item.m_Piece.name is "Car Drive Piece 3")
				{
					item.m_Piece = twoWayPiece;
				}

				if (item.m_Piece.name is "Car Drive Piece 3 - Flat")
				{
					item.m_Piece = twoWayPieceFlat;
				}
			}

			twoWayLane.Remove<ObsoleteIdentifiers>();
			twoWayPieceFlat.Remove<ObsoleteIdentifiers>();
			twoWayPiece.Remove<ObsoleteIdentifiers>();
			twoWaySection.Remove<ObsoleteIdentifiers>();

			prefabSystem.AddPrefab(twoWayLane);
			prefabSystem.AddPrefab(twoWayPieceFlat);
			prefabSystem.AddPrefab(twoWayPiece);
			prefabSystem.AddPrefab(twoWaySection);

			NetSections[twoWaySection.name] = twoWaySection;
		}

		private void AddCustomPrefabComponents()
		{
			_blacklist.ForEach(x => NetSections[x].AddOrGetComponent<RoadBuilderHide>());

			SetUp("Pavement Path Section 3", "coui://roadbuildericons/RB_PedestrianLane.svg").WithRequired(RoadCategory.Pathway).AddLaneThumbnail("coui://roadbuildericons/Thumb_PedestrianLaneWide.svg");
			SetUp("Tiled Section 3", "coui://roadbuildericons/RB_PedestrianOnly.svg").WithRequired(RoadCategory.Tiled).AddLaneThumbnail("coui://roadbuildericons/Thumb_TiledSmall.svg");
			SetUp("Tiled Median Pedestrian 2", "coui://roadbuildericons/RB_TiledMedian_Centered.svg").WithRequired(RoadCategory.Tiled).WithThumbnail("coui://roadbuildericons/RB_TiledMedian.svg").AddLaneThumbnail("coui://roadbuildericons/Thumb_PedestrianLaneSmall.svg");
			SetUp("Sound Barrier 1", "coui://roadbuildericons/RB_SoundBarrier.svg").WithExcluded(RoadCategory.RaisedSidewalk).AddLaneThumbnail("coui://roadbuildericons/Thumb_SoundBarrier.svg");
		}

		private RoadBuilderLaneInfo SetUp(string prefabName, string thumbnail)
		{
			var prefab = NetSections[prefabName];

			prefab.AddOrGetComponent<UIObject>().m_Icon = thumbnail;

			return prefab.AddOrGetComponent<RoadBuilderLaneInfo>();
		}

		private void AddCustomGroups()
		{
			foreach (var type in typeof(NetSectionsSystem).Assembly.GetTypes())
			{
				if (typeof(BaseLaneGroupPrefab).IsAssignableFrom(type) && !type.IsAbstract)
				{
					var prefab = (BaseLaneGroupPrefab)ScriptableObject.CreateInstance(type);

					prefab.Initialize(NetSections);
					prefab.name = type.FullName;

					prefabSystem.AddPrefab(prefab);

					LaneGroups[prefab.name] = prefab;
				}
			}
		}

		#region Blacklist
		private readonly HashSet<string> _blacklist = new()
		{
			"Missing Net Section",
			"Tiled Pedestrian Section 3",
			"Tiled Median 2",
			"Road Median 0",
			"Highway Median 0",
			"Alley Median 0",
			"Tram Median 0",
			"Train Median 0",
			"Subway Median 0",
			"Public Transport Median 0",
			"Gravel Median 0",
			"Alley Side 0",
			"Road Side 0",
			"Highway Side 0",
			"Gravel Side 0",
			"Tiled Side 0",
			"Subway Side 0",
			"Train Side 0",
			"Pavement Path Side Section 0",
			"Grass",
			"Trees",
			"Golden Gate Sidewalk",
			"Golden Gate Drive",
			"Golden Gate Bridge",
			"Ground Cable Section 1",
			"Invisible Edge Section 0",
			"Invisible Pedestrian Section 2",
			"Invisible Pedestrian Section 0.5",
			"Invisible Boarding Section 0",
			"Invisible Car Oneway Section 3",
			"Invisible Median Section 0",
			"Invisible Parking Section 5.5",
			"Invisible Car Twoway Section 3",
			"Small Sewage Marker Section",
			"Low-voltage Marker Section - Small",
			"Small Water Marker Section",
			"Invisible Car Bay Section 3",
			"Invisible Airplane Airspace Section 75",
			"Invisible Helicopter Twoway Section 12",
			"Invisible Helicopter Edge Section 0",
			"Invisible Airplane Twoway Section 60",
			"Invisible Airplane Oneway Section 60",
			"Invisible Airplane Runway Section 75",
			"Invisible Airplane Edge Section 0",
			"Waterway Median Section 0",
			"Ship Drive Section 50",
			"Ship Drive Section 50 - Outermost",
			"Seaway Edge Section 5",
			"Water Pipe Section 3",
			"Water Pipe Section 1.5",
			"Water Pipe Section 1",
			"Stormwater Pipe Section 1.5",
			"Pipeline Spacing Section 1",
			"Pipeline Spacing Section 2",
			"Sewage Pipe Section 4",
			"Sewage Pipe Section 2",
			"Sewage Pipe Section 1.5",
			"Low-voltage Marker Section",
			"High-voltage Marker Section",
			"Hydroelectric_Power_Plant_01 Dam Section",
			"Ground Cable Section 8",
			"Ground Cable Section 1.5",
			"Low-voltage Cables",
			"High-voltage Cables",
			"All-way Stop",
			"Traffic Lights",
			"Wide Sidewalk",
			"Wooden Covered Bridge Shoulder",
			"2-Lane Wooden Covered  Bridge",
			"2-Lane Truss Arch Bridge",
			"Highway Shoulder 2",
			"2-Lane Suspension Bridge",
			"3-Lane Suspension Bridge",
			"4-Lane Suspension Bridge",
			"5-Lane Suspension Bridge",
			"4-Lane Tied Arch Bridge 00",
			"8-Lane Cable Stayed Bridge 00",
			"6-Lane Extradosed Bridge",
			"Grand Bridge",
			"Cable Stayed Pedestrian Bridge",
			"Covered Pedestrian Bridge",
			"Arc Pedestrian Bridge",
		};
		#endregion
	}
}
