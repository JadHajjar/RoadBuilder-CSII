using Colossal.IO.AssetDatabase;

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
using System.Linq;

using Unity.Collections;
using Unity.Entities;

using UnityEngine;

using static Colossal.AssetPipeline.Diagnostic.Report;

namespace RoadBuilder.Systems
{
	public partial class RoadBuilderNetSectionsSystem : GameSystemBase
	{
		private PrefabSystem prefabSystem;
		private EntityQuery prefabQuery;
		private EntityQuery allPrefabQuery;
		private NetPrefabGenerationPhase prefabGenerationPhase;
		protected bool initialSetupFinished;

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

				if (prefabGenerationPhase != NetPrefabGenerationPhase.Complete && NetSections.Count > 0)
				{
					DoCustomSectionSetup();
				}
				else
				{
					SectionsAdded?.Invoke();

					if (!initialSetupFinished)
					{
						initialSetupFinished = true;

						RequireForUpdate(prefabQuery);
					}
				}
			}
			catch (Exception ex)
			{
				Mod.Log.Error(ex);
			}
		}

		private void DoCustomSectionSetup()
		{
			prefabGenerationPhase++;

			switch (prefabGenerationPhase)
			{
				case NetPrefabGenerationPhase.NetLaneCreation:
					DoNetLaneCreation();
					return;
				case NetPrefabGenerationPhase.NetPieceCreation:
					DoNetPieceCreation();
					return;
				case NetPrefabGenerationPhase.NetSectionCreation:
					DoNetSectionCreation();
					return;
				case NetPrefabGenerationPhase.NetGroupsCreation:
					DoNetGroupsCreation();
					return;
			}

			AddCustomPrefabComponents();

			GameManager.instance.localizationManager.ReloadActiveLocale();
		}

		private void DoNetLaneCreation()
		{

		}

		private void DoNetPieceCreation()
		{
			var medianTiledIntersectionPiece0 = NetPieces["Median Intersection Piece 0"].Clone("RB Tiled Median Intersection Piece 0") as NetPiecePrefab;
			var medianTiledIntersectionPiece0Flat = NetPieces["Median Intersection Piece 0 - Flat"].Clone("RB Tiled Median Intersection Piece 0 - Flat") as NetPiecePrefab;

			medianTiledIntersectionPiece0.geometryAsset = AssetDatabase.game.GetAsset<GeometryAsset>("6a5ebd30da0e34986c320b4cdc2014df"); // TiledMedianIntersection2
			medianTiledIntersectionPiece0.surfaceAssets = new[] { AssetDatabase.game.GetAsset<SurfaceAsset>("40dd255c63eb8f65f96c5333d7d28afb") }; // TiledRoad
			medianTiledIntersectionPiece0Flat.geometryAsset = AssetDatabase.game.GetAsset<GeometryAsset>("2549026d2a35f5fce401702b40c90258"); // TiledMedianIntersectionFlat2
			medianTiledIntersectionPiece0Flat.surfaceAssets = new[] { AssetDatabase.game.GetAsset<SurfaceAsset>("40dd255c63eb8f65f96c5333d7d28afb") }; // TiledRoad

			prefabSystem.AddPrefab(NetPieces[medianTiledIntersectionPiece0.name] = medianTiledIntersectionPiece0);
			prefabSystem.AddPrefab(NetPieces[medianTiledIntersectionPiece0Flat.name] = medianTiledIntersectionPiece0Flat);

			var median5Pieces = new[]
			{
				NetPieces["Median Piece 5"],
				NetPieces["Median Piece 5 - Grass"],
				NetPieces["Median Piece 5 - Platform"],
			};

			foreach (var oldPrefab in median5Pieces)
			{
				var prefab = oldPrefab.Clone("RB " + oldPrefab.name) as NetPiecePrefab;
				var objects = prefab.GetComponent<NetPieceObjects>();
				var tree = objects.m_PieceObjects.FirstOrDefault(x => x.m_Object.name == "Road Tree Placeholder");

				if (tree != null)
				{
					tree.m_RequireAll = tree.m_RequireAll.Where(x => x != NetPieceRequirements.Median).ToArray();
				}

				prefabSystem.AddPrefab(NetPieces[prefab.name] = prefab);
			}
		}

		private void DoNetSectionCreation()
		{
			var tiledMedian0 = NetSections["Road Median 0"].Clone("RB Tiled Median 0") as NetSectionPrefab;

			tiledMedian0.m_Pieces[0].m_RequireAll = Add(tiledMedian0.m_Pieces[0].m_RequireAll, NetPieceRequirements.Pavement);
			tiledMedian0.m_Pieces[1].m_RequireAll = Add(tiledMedian0.m_Pieces[1].m_RequireAll, NetPieceRequirements.Pavement);
			tiledMedian0.m_Pieces = Add(tiledMedian0.m_Pieces,
				new NetPieceInfo
				{
					m_Piece = NetPieces["RB Tiled Median Intersection Piece 0"],
					m_RequireAll = new[] { NetPieceRequirements.Median },
					m_RequireAny = new[] { NetPieceRequirements.Intersection, NetPieceRequirements.MedianBreak },
					m_RequireNone = new[] { NetPieceRequirements.LevelCrossing, NetPieceRequirements.Pavement },
				},
				new NetPieceInfo
				{
					m_Piece = NetPieces["RB Tiled Median Intersection Piece 0 - Flat"],
					m_RequireAll = new[] { NetPieceRequirements.Median, NetPieceRequirements.LevelCrossing },
					m_RequireAny = new[] { NetPieceRequirements.Intersection, NetPieceRequirements.MedianBreak },
					m_RequireNone = new[] { NetPieceRequirements.Pavement },
				});

			prefabSystem.AddPrefab(NetSections[tiledMedian0.name] = tiledMedian0);

			var newSection3 = NetSections["Public Transport Lane Section 3 - Tram Option"].Clone("RB Public Transport Lane Section 3") as NetSectionPrefab;
			var newSection4 = NetSections["Public Transport Lane Section 4 - Tram Option"].Clone("RB Public Transport Lane Section 4") as NetSectionPrefab;

			foreach (var item in new[] { newSection3, newSection4 })
			{
				var sections = new List<NetPieceInfo>();

				foreach (var piece in item.m_Pieces)
				{
					if (!piece.m_RequireAll.Contains(NetPieceRequirements.TramTrack) && !piece.m_RequireAny.Contains(NetPieceRequirements.TramTrack))
					{
						sections.Add(new NetPieceInfo
						{
							m_RequireAll = piece.m_RequireAll,
							m_RequireAny = piece.m_RequireAny,
							m_RequireNone = piece.m_RequireNone.Where(x => x != NetPieceRequirements.TramTrack).ToArray(),
							m_Offset = piece.m_Offset,
							m_Piece = piece.m_Piece,
						});
					}
				}

				item.m_Pieces = sections.ToArray();
			}

			prefabSystem.AddPrefab(NetSections[newSection3.name] = newSection3);
			prefabSystem.AddPrefab(NetSections[newSection4.name] = newSection4);

			var median1 = NetSections["Road Median 1"].Clone("RB Median 1") as NetSectionPrefab;
			var median2 = NetSections["Road Median 2"].Clone("RB Median 2") as NetSectionPrefab;
			var median5 = NetSections["Road Median 5"].Clone("RB Median 5") as NetSectionPrefab;
			var tiledMedian2 = NetSections["Tiled Median 2"].Clone("RB Tiled Median 2") as NetSectionPrefab;
			var median5Pieces = new[]
			{
				"Median Piece 5",
				"Median Piece 5 - Grass",
				"Median Piece 5 - Platform",
			};

			median5.m_Pieces = median5.m_Pieces.Concat(new[]
			{
				new NetPieceInfo
				{
					m_Piece = median5.m_Pieces[6].m_Piece,
					m_RequireAll = new[] { NetPieceRequirements.MiddleGrass, NetPieceRequirements.BusStop },
					m_RequireAny = median5.m_Pieces[6].m_RequireAny,
					m_RequireNone = median5.m_Pieces[6].m_RequireNone,
				}, new NetPieceInfo
				{
					m_Piece = median5.m_Pieces[7].m_Piece,
					m_RequireAll = new[] { NetPieceRequirements.MiddleGrass, NetPieceRequirements.OppositeBusStop },
					m_RequireAny = median5.m_Pieces[7].m_RequireAny,
					m_RequireNone = median5.m_Pieces[7].m_RequireNone,
				}
			}).ToArray();

			foreach (var prefab in new[] { median1, median2, median5, tiledMedian2 })
			{
				foreach (var item in prefab.m_Pieces)
				{
					item.m_RequireAll = replaceByNode(item.m_RequireAll.ToList(), false);
					item.m_RequireAny = replaceByNode(item.m_RequireAny.ToList(), true);
					item.m_RequireNone = replaceByNode(item.m_RequireNone.ToList(), true);

					if (median5Pieces.Contains(item.m_Piece.name))
					{
						item.m_Piece = NetPieces["RB " + item.m_Piece.name];
					}
				}

				prefabSystem.AddPrefab(NetSections[prefab.name] = prefab);
			}

			static NetPieceRequirements[] replaceByNode(List<NetPieceRequirements> array, bool addBus)
			{
				for (var i = 0; i < array.Count; i++)
				{
					if (array[i] == NetPieceRequirements.Intersection)
					{
						array[i] = NetPieceRequirements.Node;
					}
				}

				if (addBus)
				{
					if (array.Contains(NetPieceRequirements.TramStop))
					{
						array.Add(NetPieceRequirements.BusStop);
					}

					if (array.Contains(NetPieceRequirements.OppositeTramStop))
					{
						array.Add(NetPieceRequirements.OppositeBusStop);
					}
				}

				return array.ToArray();
			}

			var subwayPlatformSections = new[]
			{
				NetSections["Subway Median 8"],
				NetSections["Subway Median 8 - Plain"],
			};

			foreach (var oldPrefab in subwayPlatformSections)
			{
				var prefab = oldPrefab.Clone("RB " + oldPrefab.name) as NetSectionPrefab;

				foreach (var piece in prefab.m_Pieces)
				{
					piece.m_RequireAll = piece.m_RequireAll.Where(x => x != NetPieceRequirements.Median).ToArray();
				}

				prefabSystem.AddPrefab(NetSections[prefab.name] = prefab);
			}
		}

		private void AddCustomPrefabComponents()
		{
			SetUp("Pavement Path Section 3", "coui://roadbuildericons/RB_PedestrianLane.svg").WithRequired(RoadCategory.Pathway).AddLaneThumbnail("coui://roadbuildericons/Thumb_PedestrianLaneWide.svg");
			SetUp("Tiled Pedestrian Section 3", "coui://roadbuildericons/RB_PedestrianOnly.svg").WithRequired(RoadCategory.Tiled).AddLaneThumbnail("coui://roadbuildericons/Thumb_TiledSmall.svg");
			SetUp("RB Tiled Median 2", "coui://roadbuildericons/RB_TiledMedian_Centered.svg").WithRequired(RoadCategory.Tiled).WithThumbnail("coui://roadbuildericons/RB_TiledMedian.svg").AddLaneThumbnail("coui://roadbuildericons/Thumb_PedestrianLaneSmall.svg");
			SetUp("Sound Barrier 1", "coui://roadbuildericons/RB_SoundBarrier.svg").WithExcluded(RoadCategory.RaisedSidewalk).AddLaneThumbnail("coui://roadbuildericons/Thumb_SoundBarrier.svg");
		}

		private RoadBuilderLaneInfo SetUp(string prefabName, string thumbnail)
		{
			var prefab = NetSections[prefabName];

			prefab.AddOrGetComponent<UIObject>().m_Icon = thumbnail;

			return prefab.AddOrGetComponent<RoadBuilderLaneInfo>();
		}

		private void DoNetGroupsCreation()
		{
			foreach (var type in typeof(RoadBuilderNetSectionsSystem).Assembly.GetTypes())
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

		private T[] Add<T>(T[] array, params T[] values)
		{
			var newArray = new T[array.Length + values.Length];

			for (var i = 0; i < array.Length; i++)
			{
				newArray[i] = array[i];
			}

			for (var i = 0; i < values.Length; i++)
			{
				newArray[i + array.Length] = values[i];
			}

			return newArray;
		}

		private T[] Remove<T>(T[] array, params T[] values)
		{
			var newList = array.ToList();

			foreach (var item in values)
			{
				newList.Remove(item);
			}

			return newList.ToArray();
		}
	}
}
