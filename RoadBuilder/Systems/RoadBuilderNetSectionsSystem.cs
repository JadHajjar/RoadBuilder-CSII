using Colossal.IO.AssetDatabase;
using Colossal.IO.AssetDatabase.Internal;

using Game;
using Game.Common;
using Game.Prefabs;

using RoadBuilder.Domain.Components;
using RoadBuilder.Domain.Components.Prefabs;
using RoadBuilder.Domain.Enums;
using RoadBuilder.Domain.Prefabs;
using RoadBuilder.LaneGroups;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

using UnityEngine;

namespace RoadBuilder.Systems
{
	public partial class RoadBuilderNetSectionsSystem : GameSystemBase
	{
		private PrefabSystem prefabSystem;
		private EntityQuery prefabQuery;
		private EntityQuery allPrefabQuery;
		private NetPrefabGenerationPhase prefabGenerationPhase;
		protected bool InitialSetupFinished;

		public event Action SectionsAdded;

		public Dictionary<string, NetSectionPrefab> NetSections { get; } = new();
		public Dictionary<string, NetPiecePrefab> NetPieces { get; } = new();
		public Dictionary<string, NetLanePrefab> NetLanes { get; } = new();
		public Dictionary<string, LaneGroupPrefab> LaneGroups { get; } = new();

		protected override void OnCreate()
		{
			base.OnCreate();

			prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
			prefabQuery = SystemAPI.QueryBuilder().WithAll<Created, PrefabData>().WithAny<NetSectionData, NetPieceData, NetLaneData, RoadBuilderLaneGroupPrefabData>().Build();
			allPrefabQuery = SystemAPI.QueryBuilder().WithAll<PrefabData>().WithAny<NetSectionData, NetPieceData, NetLaneData, RoadBuilderLaneGroupPrefabData>().Build();
		}

		protected override void OnUpdate()
		{
			try
			{
				var entities = (InitialSetupFinished ? prefabQuery : allPrefabQuery).ToEntityArray(Allocator.Temp);

				for (var i = 0; i < entities.Length; i++)
				{
					if (!prefabSystem.TryGetPrefab<PrefabBase>(entities[i], out var prefab))
					{
						continue;
					}

					if (prefab is NetSectionPrefab netSectionPrefab)
					{
						NetSections[netSectionPrefab.name] = netSectionPrefab;

						//if (InitialSetupFinished && GameManager.instance.gameMode == GameMode.Editor && !netSectionPrefab.builtin && !netSectionPrefab.Has<RoadBuilderLaneGroup>() && !netSectionPrefab.Has<RoadBuilderLaneInfo>())
						//{
						//	netSectionPrefab.AddOrGetComponent<RoadBuilderLaneGroup>();
						//	netSectionPrefab.AddOrGetComponent<RoadBuilderLaneInfo>();
						//	netSectionPrefab.AddOrGetComponent<RoadBuilderLaneDecorationInfo>();
						//	netSectionPrefab.AddOrGetComponent<RoadBuilderEdgeLaneInfo>();
						//}
					}
					else if (prefab is NetPiecePrefab netPiecePrefab)
					{
						NetPieces[netPiecePrefab.name] = netPiecePrefab;
					}
					else if (prefab is NetLanePrefab netLanePrefab)
					{
						NetLanes[netLanePrefab.name] = netLanePrefab;
					}
					else if (prefab is LaneGroupPrefab laneGroupPrefab)
					{
						LaneGroups[laneGroupPrefab.name] = laneGroupPrefab;
					}
				}

				if (NetSections.Count == 0)
				{
					return;
				}

				if (prefabGenerationPhase != NetPrefabGenerationPhase.Complete)
				{
					DoCustomSectionSetup();

					return;
				}

				foreach (var item in LaneGroups.Values)
				{
					item.LinkedSections = new();

					foreach (var section in NetSections.Values)
					{
						if (section.TryGetExactly<RoadBuilderLaneGroup>(out var laneGroup) && laneGroup.GroupPrefab == item)
						{
							item.LinkedSections.Add(section);
						}
					}
				}

				SectionsAdded?.Invoke();

				if (!InitialSetupFinished)
				{
					InitialSetupFinished = true;

					RequireForUpdate(prefabQuery);
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

			Mod.ReloadActiveLocale();
		}

		private void DoNetLaneCreation()
		{
		}

		private void DoNetPieceCreation()
		{
			CreateTiledMedian0Piece();

			CreateCustomMedian5Pieces();

			CreateAllEmptyPieces();

			CreateTiledTramPiece();

			CreateParkingPieces();

			CreateSidewalkPieces();

			CreatePolePieces();
		}

		private void DoNetSectionCreation()
		{
			CreateTiledMedian0Section();

			CreateBusOnlySection();

			CreateCarOnlyTiledSection();

			FixMedianSections();

			FixSubwayMedianSections();

			AddParkingNetSections();

			CreateEmptyNetSection("RB Empty Section {0}", "RB Empty Piece", 0.5f, 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f);
			CreateEmptyNetSection("RB Gravel Empty Section {0}", "RB Gravel Empty Piece", 0.5f, 1f, 1.5f, 2f, 2.5f, 3f);
			CreateEmptyNetSection("RB Tiled Empty Section {0}", "RB Tiled Empty Piece", 0.5f, 1f, 1.5f, 2f, 2.5f, 3f);
			CreateEmptyNetSection("RB Train Empty Section {0}", "RB Train Empty Piece", 0.5f, 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f);

			CreateEmptyNetSection("RB Train Pole Section {0}", "RB Train Pole Piece", 1f, 2f);
			CreateEmptyNetSection("RB Tram Pole Section {0}", "RB Tram Pole Piece", 1f, 2f);

			GenerateSidewalkSections();
		}

		private void DoNetGroupsCreation()
		{
			foreach (var type in typeof(RoadBuilderNetSectionsSystem).Assembly.GetTypes())
			{
				if (typeof(BaseLaneGroupPrefab).IsAssignableFrom(type) && !type.IsAbstract)
				{
					var prefab = ScriptableObject.CreateInstance<LaneGroupPrefab>();

					var groupPrefab = Activator.CreateInstance(type) as BaseLaneGroupPrefab;

					groupPrefab.Prefab = prefab;
					groupPrefab.Sections = NetSections;
					groupPrefab.Initialize();

					prefab.name = type.FullName;
					prefab.RoadBuilder = true;

					prefabSystem.AddPrefab(prefab);
				}
			}
		}

		private void CreateCustomMedian5Pieces()
		{
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

		private void CreateTiledMedian0Piece()
		{
			var medianTiledIntersectionPiece0 = NetPieces["Median Intersection Piece 0"].Clone("RB Tiled Median Intersection Piece 0") as NetPiecePrefab;
			var medianTiledIntersectionPiece0Flat = NetPieces["Median Intersection Piece 0 - Flat"].Clone("RB Tiled Median Intersection Piece 0 - Flat") as NetPiecePrefab;

			medianTiledIntersectionPiece0.geometryAsset = AssetDatabase.game.GetAsset<GeometryAsset>("6a5ebd30da0e34986c320b4cdc2014df"); // TiledMedianIntersection2
			medianTiledIntersectionPiece0.surfaceAssets = new[] { AssetDatabase.game.GetAsset<SurfaceAsset>("40dd255c63eb8f65f96c5333d7d28afb") }; // TiledRoad
			medianTiledIntersectionPiece0Flat.geometryAsset = AssetDatabase.game.GetAsset<GeometryAsset>("2549026d2a35f5fce401702b40c90258"); // TiledMedianIntersectionFlat2
			medianTiledIntersectionPiece0Flat.surfaceAssets = new[] { AssetDatabase.game.GetAsset<SurfaceAsset>("40dd255c63eb8f65f96c5333d7d28afb") }; // TiledRoad

			prefabSystem.AddPrefab(NetPieces[medianTiledIntersectionPiece0.name] = medianTiledIntersectionPiece0);
			prefabSystem.AddPrefab(NetPieces[medianTiledIntersectionPiece0Flat.name] = medianTiledIntersectionPiece0Flat);
		}

		private void CreateAllEmptyPieces()
		{
			CreateAndAddEmptyPieces("RB Empty Piece {0}", ("Car Drive Piece 3", new[] { 0.5f, 1f, 1.5f, 2f, 2.5f, 3f }), ("Highway Drive Piece 4", new[] { 3.5f, 4f, 6f }));
			CreateAndAddEmptyPieces("RB Empty Piece Flat {0}", ("Car Drive Piece 3 - Flat", new[] { 0.5f, 1f, 1.5f, 2f, 2.5f, 3f }), ("Highway Drive Piece 4 - Flat", new[] { 3.5f, 4f, 6f }));
			CreateAndAddEmptyPieces("RB Empty Piece Middle {0}", ("Car Middle Piece 3", new[] { 0.5f, 1f, 1.5f, 2f, 2.5f, 3f }), ("Highway Middle Piece 4", new[] { 3.5f, 4f, 6f }));
			CreateAndAddEmptyPieces("RB Empty Piece Middle Flat {0}", ("Car Middle Piece 3 - Flat", new[] { 0.5f, 1f, 1.5f, 2f, 2.5f, 3f }), ("Highway Middle Piece 4 - Flat", new[] { 3.5f, 4f, 6f }));

			CreateAndAddEmptyPieces("RB Gravel Empty Piece {0}", ("Gravel Drive Piece 3", new[] { 0.5f, 1f, 1.5f, 2f, 2.5f, 3f }));
			CreateAndAddEmptyPieces("RB Gravel Empty Piece Flat {0}", ("Gravel Drive Piece 3 - Flat", new[] { 0.5f, 1f, 1.5f, 2f, 2.5f, 3f }));
			CreateAndAddEmptyPieces("RB Gravel Empty Piece Middle {0}", ("Gravel Middle Piece 3", new[] { 0.5f, 1f, 1.5f, 2f, 2.5f, 3f }));
			CreateAndAddEmptyPieces("RB Gravel Empty Piece Middle Flat {0}", ("Gravel Middle Piece 3 - Flat", new[] { 0.5f, 1f, 1.5f, 2f, 2.5f, 3f }));

			CreateAndAddEmptyPieces("RB Tiled Empty Piece {0}", ("Tiled Drive Piece 3", new[] { 0.5f, 1f, 1.5f, 2f, 2.5f, 3f }));
			CreateAndAddEmptyPieces("RB Tiled Empty Piece Flat {0}", ("Tiled Drive Piece 3 - Flat", new[] { 0.5f, 1f, 1.5f, 2f, 2.5f, 3f }));
			CreateAndAddEmptyPieces("RB Tiled Empty Piece Middle {0}", ("Tiled Drive Piece 3", new[] { 0.5f, 1f, 1.5f, 2f, 2.5f, 3f }));
			CreateAndAddEmptyPieces("RB Tiled Empty Piece Middle Flat {0}", ("Tiled Drive Piece 3 - Flat", new[] { 0.5f, 1f, 1.5f, 2f, 2.5f, 3f }));

			CreateAndAddEmptyPieces("RB Train Empty Piece {0}", ("Train Track Piece 4", new[] { 0.5f, 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f }));
			CreateAndAddEmptyPieces("RB Train Empty Piece Flat {0}", ("Train Track Piece 4", new[] { 0.5f, 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f }));
			CreateAndAddEmptyPieces("RB Train Empty Piece Middle {0}", ("Train Track Middle Piece 4", new[] { 0.5f, 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f }));
			CreateAndAddEmptyPieces("RB Train Empty Piece Middle Flat {0}", ("Train Track Middle Piece 4", new[] { 0.5f, 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f }));

			CreateAndAddEmptyTunnelAndBridgePieces("Elevated Bottom Piece 1", "Elevated Bottom Piece {0}", 0.5f);
			CreateAndAddEmptyTunnelAndBridgePieces("Elevated Bottom Piece 1 - Ending", "Elevated Bottom Piece {0} - Ending", 0.5f);
			CreateAndAddEmptyTunnelAndBridgePieces("Elevated Bottom Piece 1 - Intersection Middle", "Elevated Bottom Piece {0} - Intersection Middle", 0.5f);

			CreateAndAddEmptyTunnelAndBridgePieces("Elevated Bottom Piece 2", "Elevated Bottom Piece {0}", 1.5f);
			CreateAndAddEmptyTunnelAndBridgePieces("Elevated Bottom Piece 2 - Ending", "Elevated Bottom Piece {0} - Ending", 1.5f);
			CreateAndAddEmptyTunnelAndBridgePieces("Elevated Bottom Piece 2 - Intersection Middle", "Elevated Bottom Piece {0} - Intersection Middle", 1.5f);

			CreateAndAddEmptyTunnelAndBridgePieces("Elevated Bottom Piece 3", "Elevated Bottom Piece {0}", 2.5f);
			CreateAndAddEmptyTunnelAndBridgePieces("Elevated Bottom Piece 3 - Ending", "Elevated Bottom Piece {0} - Ending", 2.5f);
			CreateAndAddEmptyTunnelAndBridgePieces("Elevated Bottom Piece 3 - Intersection Middle", "Elevated Bottom Piece {0} - Intersection Middle", 2.5f);

			CreateAndAddEmptyTunnelAndBridgePieces("Elevated Bottom Piece 4", "Elevated Bottom Piece {0}", 3.5f);
			CreateAndAddEmptyTunnelAndBridgePieces("Elevated Bottom Piece 4 - Ending", "Elevated Bottom Piece {0} - Ending", 3.5f);
			CreateAndAddEmptyTunnelAndBridgePieces("Elevated Bottom Piece 4 - Intersection Middle", "Elevated Bottom Piece {0} - Intersection Middle", 3.5f);

			CreateAndAddEmptyTunnelAndBridgePieces("Elevated Bottom Piece 4", "Elevated Bottom Piece {0}", 6f);
			CreateAndAddEmptyTunnelAndBridgePieces("Elevated Bottom Piece 4 - Ending", "Elevated Bottom Piece {0} - Ending", 6f);
			CreateAndAddEmptyTunnelAndBridgePieces("Elevated Bottom Piece 4 - Intersection Middle", "Elevated Bottom Piece {0} - Intersection Middle", 6f);

			CreateAndAddEmptyTunnelAndBridgePieces("Elevated Bottom Piece 3.5 - Edge", "Elevated Bottom Piece {0} - Edge", 3f);
			CreateAndAddEmptyTunnelAndBridgePieces("Elevated Bottom Piece 3.5 - Edge Ending", "Elevated Bottom Piece {0} - Edge Ending", 3f);
			CreateAndAddEmptyTunnelAndBridgePieces("Elevated Bottom Piece 3.5 - Edge", "Elevated Bottom Piece {0} - Edge", 2.5f);
			CreateAndAddEmptyTunnelAndBridgePieces("Elevated Bottom Piece 3.5 - Edge Ending", "Elevated Bottom Piece {0} - Edge Ending", 2.5f);
			CreateAndAddEmptyTunnelAndBridgePieces("Elevated Bottom Piece 2 - Edge", "Elevated Bottom Piece {0} - Edge", 1.5f);
			CreateAndAddEmptyTunnelAndBridgePieces("Elevated Bottom Piece 2 - Edge Ending", "Elevated Bottom Piece {0} - Edge Ending", 1.5f);

			CreateAndAddEmptyTunnelAndBridgePieces("Tunnel Top Piece 1", "Tunnel Top Piece {0}", 0.5f);
			CreateAndAddEmptyTunnelAndBridgePieces("Tunnel Top Piece 1 - Ending", "Tunnel Top Piece {0} - Ending", 0.5f);
			CreateAndAddEmptyTunnelAndBridgePieces("Tunnel Top Piece 1 - Intersection Middle", "Tunnel Top Piece {0} - Intersection Middle", 0.5f);

			CreateAndAddEmptyTunnelAndBridgePieces("Tunnel Top Piece 2", "Tunnel Top Piece {0}", 1.5f);
			CreateAndAddEmptyTunnelAndBridgePieces("Tunnel Top Piece 2 - Ending", "Tunnel Top Piece {0} - Ending", 1.5f);
			CreateAndAddEmptyTunnelAndBridgePieces("Tunnel Top Piece 2 - Intersection Middle", "Tunnel Top Piece {0} - Intersection Middle", 1.5f);

			CreateAndAddEmptyTunnelAndBridgePieces("Tunnel Top Piece 3", "Tunnel Top Piece {0}", 2.5f);
			CreateAndAddEmptyTunnelAndBridgePieces("Tunnel Top Piece 3 - Ending", "Tunnel Top Piece {0} - Ending", 2.5f);
			CreateAndAddEmptyTunnelAndBridgePieces("Tunnel Top Piece 3 - Intersection Middle", "Tunnel Top Piece {0} - Intersection Middle", 2.5f);

			//CreateAndAddEmptyTunnelAndBridgePieces("Tunnel Top Piece 4", "Tunnel Top Piece {0}", 3.5f);
			CreateAndAddEmptyTunnelAndBridgePieces("Tunnel Top Piece 4 - Ending", "Tunnel Top Piece {0} - Ending", 3.5f);
			CreateAndAddEmptyTunnelAndBridgePieces("Tunnel Top Piece 4 - Intersection Middle", "Tunnel Top Piece {0} - Intersection Middle", 3.5f);

			CreateAndAddEmptyTunnelAndBridgePieces("Subway Tunnel Top Piece 8", "Tunnel Top Piece {0}", 6f);
			CreateAndAddEmptyTunnelAndBridgePieces("Subway Tunnel Top Piece 8 - Ending", "Tunnel Top Piece {0} - Ending", 6f);
			CreateAndAddEmptyTunnelAndBridgePieces("Subway Tunnel Top Piece 8 - Intersection Middle", "Tunnel Top Piece {0} - Intersection Middle", 6f);

			CreateAndAddEmptyTunnelAndBridgePieces("Tunnel Top Piece 3.5 - Edge Ending", "Tunnel Top Piece {0} - Edge Ending", 3f);
			CreateAndAddEmptyTunnelAndBridgePieces("Tunnel Top Piece 3.5 - Edge Ending", "Tunnel Top Piece {0} - Edge Ending", 2.5f);
			CreateAndAddEmptyTunnelAndBridgePieces("Tunnel Top Piece 2 - Edge Ending", "Tunnel Top Piece {0} - Edge Ending", 1.5f);
		}

		private void CreateTiledTramPiece()
		{
			var newPiece = NetPieces["Tiled Drive Piece 3 - Tram"].Clone("RB Tiled Tram Piece 3") as NetPiecePrefab;

			newPiece.GetComponent<NetPieceLanes>().m_Lanes[0].m_Lane = NetLanes["Oneway Tram Lane 3"];

			prefabSystem.AddPrefab(NetPieces[newPiece.name] = newPiece);
		}

		private void CreateParkingPieces()
		{
			var pieces = new[]
			{
				("RB Parking Piece Parallel", "Parking Lane 2", 3f, "Car Drive Piece 3", "Car Drive Piece 3"),
				("RB Parking Piece Parallel NoMarking", "Parking Lane 2", 2f, "Car Drive Piece 3", "Car Drive Piece 3"),
				("RB Parking Piece Angled", "Invisible Parking Lane - Angled67 2.9x5.9", 6f, "Subway Middle Piece 8", "Highway Drive Piece 4"),
				("RB Parking Piece Angled NoMarking", "Invisible Parking Lane - Angled67 2.9x5.9", 6f, "Subway Middle Piece 8", "Highway Drive Piece 4"),
				("RB Parking Piece Perpendicular", "Invisible Parking Lane - Perpendicular 3x5.5", 6f, "Subway Middle Piece 8", "Highway Drive Piece 4"),
				("RB Parking Piece Perpendicular NoMarking", "Invisible Parking Lane - Perpendicular 4.7x5.9", 6f, "Subway Middle Piece 8", "Highway Drive Piece 4"),
			};

			foreach (var item in pieces)
			{
				var newPiece = NetPieces["Car Drive Piece 3"].Clone(item.Item1) as NetPiecePrefab;
				newPiece.geometryAsset = NetPieces[item.Item4].geometryAsset;
				newPiece.surfaceAssets = NetPieces[item.Item5].surfaceAssets;
				newPiece.m_Width = item.Item3;
				newPiece.m_SurfaceHeights = new(-0.2f);
				newPiece.Remove<NetPieceObjects>();
				newPiece.AddOrGetComponent<NetPieceLanes>().m_Lanes = new[]
				{
					new NetLaneInfo
					{
						m_Lane = NetLanes[item.Item2],
						m_Position = new float3(0, -0.2f, 0)
					}
				};

				if (item.Item1 == "RB Parking Piece Parallel")
				{
					newPiece.AddOrGetComponent<NetPieceObjects>().m_PieceObjects = new[]
					{
						new NetPieceObjectInfo
						{
							m_Object = prefabSystem.TryGetPrefab(new PrefabID(nameof(StaticObjectPrefab), "ParkingLotSidewaysDecal01"), out var prefab)?prefab as ObjectPrefab : default,
							m_RequireAll = new []{NetPieceRequirements.Edge},
							m_RequireAny = new NetPieceRequirements[0],
							m_RequireNone = new NetPieceRequirements[0],
							m_Probability = 100,
							m_FlipWhenInverted = true,
							m_EvenSpacing = true,
							m_Rotation = Quaternion.Euler(0, 180, 0),
							m_Offset = new float3(0, 0, -6f / 2f),
							m_Spacing = new float3(2f, 0f, 6f)
						}
					};
				}

				if (item.Item1 == "RB Parking Piece Angled")
				{
					newPiece.AddOrGetComponent<NetPieceObjects>().m_PieceObjects = new[]
					{
						new NetPieceObjectInfo
						{
							m_Object = prefabSystem.TryGetPrefab(new PrefabID(nameof(StaticObjectPrefab), "ParkingLotDiagonalDecal01"), out var prefab)?prefab as ObjectPrefab : default,
							m_RequireAll = new []{NetPieceRequirements.Edge},
							m_RequireAny = new NetPieceRequirements[0],
							m_RequireNone = new NetPieceRequirements[0],
							m_Probability = 100,
							m_FlipWhenInverted = true,
							m_Rotation = Quaternion.Euler(0, 90, 0),
							m_Offset = new float3(-3.1f / 2f, 0, 0),
							m_Spacing = new float3(5.9f, 0f, 3.1f)
						}
					};
				}

				prefabSystem.AddPrefab(NetPieces[newPiece.name] = newPiece);

				newPiece = NetPieces["Car Drive Piece 3 - Flat"].Clone(item.Item1 + " - Flat") as NetPiecePrefab;
				newPiece.geometryAsset = NetPieces[item.Item4].geometryAsset;
				newPiece.surfaceAssets = NetPieces[item.Item5].surfaceAssets;
				newPiece.m_Width = item.Item3;
				newPiece.m_SurfaceHeights = new(-0.2f);
				newPiece.Remove<NetPieceObjects>();
				newPiece.AddOrGetComponent<NetPieceLanes>().m_Lanes = new[]
				{
					new NetLaneInfo
					{
						m_Lane = NetLanes[item.Item2],
						m_Position = new float3(0, -0.2f, 0)
					}
				};

				if (item.Item1 == "RB Parking Piece Parallel")
				{
					newPiece.AddOrGetComponent<NetPieceObjects>().m_PieceObjects = new[]
					{
						new NetPieceObjectInfo
						{
							m_Object = prefabSystem.TryGetPrefab(new PrefabID(nameof(StaticObjectPrefab), "ParkingLotSidewaysDecal01"), out var prefab)?prefab as ObjectPrefab : default,
							m_RequireAll = new []{NetPieceRequirements.Edge},
							m_RequireAny = new NetPieceRequirements[0],
							m_RequireNone = new NetPieceRequirements[0],
							m_Probability = 100,
							m_FlipWhenInverted = true,
							m_EvenSpacing = true,
							m_Rotation = Quaternion.Euler(0, 180, 0),
							m_Offset = new float3(0, 0, -6f / 2f),
							m_Spacing = new float3(2f, 0f, 6f)
						}
					};
				}

				if (item.Item1 == "RB Parking Piece Angled")
				{
					newPiece.AddOrGetComponent<NetPieceObjects>().m_PieceObjects = new[]
					{
						new NetPieceObjectInfo
						{
							m_Object = prefabSystem.TryGetPrefab(new PrefabID(nameof(StaticObjectPrefab), "ParkingLotDiagonalDecal01"), out var prefab)?prefab as ObjectPrefab : default,
							m_RequireAll = new []{NetPieceRequirements.Edge},
							m_RequireAny = new NetPieceRequirements[0],
							m_RequireNone = new NetPieceRequirements[0],
							m_Probability = 100,
							m_FlipWhenInverted = true,
							m_Rotation = Quaternion.Euler(0, 90, 0),
							m_Offset = new float3(-3.1f / 2f, 0, 0),
							m_Spacing = new float3(5.9f, 0f, 3.1f)
						}
					};
				}

				prefabSystem.AddPrefab(NetPieces[newPiece.name] = newPiece);
			}
		}

		private void CreateSidewalkPieces()
		{
			foreach (var width in new float[] { 3, 2.5f, 2, 1.5f, 1 })
			{
				setUpSidewalk(NetPieces["Sidewalk Piece 3.5"].Clone("Sidewalk Piece " + width.ToString(CultureInfo.InvariantCulture)) as NetPiecePrefab, width);
				setUpSidewalk(NetPieces["Sidewalk Piece 3.5 - Flat"].Clone($"Sidewalk Piece {width.ToString(CultureInfo.InvariantCulture)} - Flat") as NetPiecePrefab, width);
			}

			setUpSidewalk(NetPieces["Sidewalk Piece 4.5"].Clone("Sidewalk Piece 4") as NetPiecePrefab, 4);
			setUpSidewalk(NetPieces["Sidewalk Piece 4.5 - Flat"].Clone($"Sidewalk Piece 4 - Flat") as NetPiecePrefab, 4);

			void setUpSidewalk(NetPiecePrefab sidewalk, float width)
			{
				var originalWidth = sidewalk.m_Width;
				sidewalk.m_Width = width;

				if (sidewalk.TryGet<NetPieceLanes>(out var netPieceLanes))
				{
					netPieceLanes.m_Lanes[0].m_Position = new(sidewalk.m_Width / -2, -0.2f, 0f);
					netPieceLanes.m_Lanes[1].m_Lane = NetLanes[width < 2 ? "Alley Pedestrian Lane 0.5" : "Alley Pedestrian Lane 1"];
					netPieceLanes.m_Lanes[1].m_Position = new((sidewalk.m_Width - 1f) / 2f, 0f, 0f);
				}

				var matchPieceVertices = sidewalk.GetComponent<MatchPieceVertices>();
				//matchPieceVertices.m_Offsets[0] = sidewalk.m_Width / -2;
				//matchPieceVertices.m_Offsets[1] = (sidewalk.m_Width / -2) + 0.4f;
				matchPieceVertices.m_Offsets[2] = sidewalk.m_Width / 2;

				foreach (var item in sidewalk.GetComponent<NetPieceObjects>().m_PieceObjects)
				{
					if (item.m_Position.x > 0)
					{
						item.m_Position = new(math.max(width / -3f, item.m_Position.x - ((originalWidth - sidewalk.m_Width) / 2f)), item.m_Position.y, item.m_Position.z);
					}
					else if (item.m_Position.x < 0)
					{
						item.m_Position = new(math.min(width / 3f, item.m_Position.x + ((originalWidth - sidewalk.m_Width) / 2f)), item.m_Position.y, item.m_Position.z);
					}
				}

				prefabSystem.AddPrefab(sidewalk);
			}
		}

		private void CreatePolePieces()
		{
			var tramPolePiece = NetPieces["Tram Shoulder Piece 1"];

			foreach (var width in new[] { 1f, 2f })
			{
				foreach (var pieceName in new[] { "Piece {0}", "Piece Flat {0}", "Piece Middle {0}", "Piece Middle Flat {0}" })
				{
					var newPiece = NetPieces[string.Format("RB Empty " + pieceName, width.ToString(CultureInfo.InvariantCulture))].Clone(string.Format("RB Tram Pole " + pieceName, width.ToString(CultureInfo.InvariantCulture))) as NetPiecePrefab;

					newPiece.Remove<NetPieceObjects>();
					newPiece.AddComponentFrom(tramPolePiece.GetComponent<NetPieceObjects>());
					newPiece.GetComponent<NetPieceObjects>().m_PieceObjects.ForEach(x => x.m_Position = new float3(x.m_Position.x -= (width - 1) / 2f, x.m_Position.y, x.m_Position.z));

					prefabSystem.AddPrefab(NetPieces[newPiece.name] = newPiece);
				}
			}

			var trainPolePiece = NetPieces["Train Shoulder Piece 1"];

			foreach (var width in new[] { 1f, 2f })
			{
				foreach (var pieceName in new[] { "Piece {0}", "Piece Flat {0}", "Piece Middle {0}", "Piece Middle Flat {0}" })
				{
					var newPiece = NetPieces[string.Format("RB Train Empty " + pieceName, width.ToString(CultureInfo.InvariantCulture))].Clone(string.Format("RB Train Pole " + pieceName, width.ToString(CultureInfo.InvariantCulture))) as NetPiecePrefab;

					newPiece.Remove<NetPieceObjects>();
					newPiece.AddComponentFrom(trainPolePiece.GetComponent<NetPieceObjects>());
					newPiece.GetComponent<NetPieceObjects>().m_PieceObjects.ForEach(x => x.m_Position = new float3(x.m_Position.x -= (width - 1) / 2f, x.m_Position.y, x.m_Position.z));

					prefabSystem.AddPrefab(NetPieces[newPiece.name] = newPiece);
				}
			}
		}

		private void CreateAndAddEmptyPieces(string newName, params (string Name, float[] widths)[] values)
		{
			foreach (var item in values)
			{
				foreach (var width in item.widths)
				{
					var newPiece = NetPieces[item.Name].Clone(string.Format(newName, width.ToString(CultureInfo.InvariantCulture))) as NetPiecePrefab;

					newPiece.m_Width = width;

					newPiece.Remove<NetPieceLanes>();
					newPiece.Remove<PlaceableNetPiece>();
					newPiece.Remove<NetPieceObjects>();

					prefabSystem.AddPrefab(NetPieces[newPiece.name] = newPiece);
				}
			}
		}

		private void CreateAndAddEmptyTunnelAndBridgePieces(string name, string newName, float width)
		{
			var newPiece = NetPieces[name].Clone(string.Format(newName, width.ToString(CultureInfo.InvariantCulture))) as NetPiecePrefab;

			newPiece.m_Width = width;

			newPiece.Remove<PlaceableNetPiece>();

			prefabSystem.AddPrefab(NetPieces[newPiece.name] = newPiece);
		}

		private void CreateTiledMedian0Section()
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
		}

		private void CreateBusOnlySection()
		{
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
		}

		private void CreateCarOnlyTiledSection()
		{
			var newSectionCarOnly = NetSections["Tiled Drive Section 3"].Clone("RB Tiled Drive Section 3 - Car") as NetSectionPrefab;
			var newSectionTramOnly = NetSections["Tiled Drive Section 3"].Clone("RB Tiled Tram Section 3") as NetSectionPrefab;

			var sections1 = new List<NetPieceInfo>();

			foreach (var piece in NetSections["Tiled Drive Section 3"].m_Pieces)
			{
				if (!piece.m_RequireAll.Contains(NetPieceRequirements.TramTrack) && !piece.m_RequireAny.Contains(NetPieceRequirements.TramTrack))
				{
					sections1.Add(new NetPieceInfo
					{
						m_RequireAll = piece.m_RequireAll,
						m_RequireAny = piece.m_RequireAny,
						m_RequireNone = piece.m_RequireNone.Where(x => x != NetPieceRequirements.TramTrack).ToArray(),
						m_Offset = piece.m_Offset,
						m_Piece = piece.m_Piece,
					});
				}
			}

			newSectionCarOnly.m_Pieces = sections1.ToArray();

			var sections2 = new List<NetPieceInfo>();

			foreach (var piece in NetSections["Tiled Drive Section 3"].m_Pieces)
			{
				if (!piece.m_RequireNone.Contains(NetPieceRequirements.TramTrack))
				{
					sections2.Add(new NetPieceInfo
					{
						m_RequireAll = piece.m_RequireAll.Where(x => x != NetPieceRequirements.TramTrack).ToArray(),
						m_RequireAny = piece.m_RequireAny.Where(x => x != NetPieceRequirements.TramTrack).ToArray(),
						m_RequireNone = piece.m_RequireNone,
						m_Offset = piece.m_Offset,
						m_Piece = piece.m_Piece.name == "Tiled Drive Piece 3 - Tram" ? NetPieces["RB Tiled Tram Piece 3"] : piece.m_Piece,
					});
				}
			}

			newSectionTramOnly.m_Pieces = sections2.ToArray();

			prefabSystem.AddPrefab(NetSections[newSectionCarOnly.name] = newSectionCarOnly);
			prefabSystem.AddPrefab(NetSections[newSectionTramOnly.name] = newSectionTramOnly);
		}

		private void FixMedianSections()
		{
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
		}

		private void FixSubwayMedianSections()
		{
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

		private void CreateEmptyNetSection(string sectionName, string pieceBaseName, params float[] widths)
		{
			foreach (var widthVal in widths)
			{
				var width = widthVal.ToString(CultureInfo.InvariantCulture);
				var section = ScriptableObject.CreateInstance<NetSectionPrefab>();

				section.name = string.Format(sectionName, width);
				section.m_SubSections = new NetSubSectionInfo[0];
				section.m_Pieces = new[]
				{
					new NetPieceInfo
					{
						m_Piece = NetPieces[string.Format(pieceBaseName + " {0}", width)],
						m_RequireAll = new NetPieceRequirements[0],
						m_RequireAny = new NetPieceRequirements[0],
						m_RequireNone = new[] { NetPieceRequirements.LevelCrossing, NetPieceRequirements.Median }
					},
					new NetPieceInfo
					{
						m_Piece = NetPieces[string.Format(pieceBaseName + " {0}", width)],
						m_RequireAll = new[] { NetPieceRequirements.Median },
						m_RequireAny = new NetPieceRequirements[0],
						m_RequireNone = new[] { NetPieceRequirements.LevelCrossing, NetPieceRequirements.Intersection, NetPieceRequirements.DeadEnd }
					},
					new NetPieceInfo
					{
						m_Piece = NetPieces[string.Format(pieceBaseName + " Flat {0}", width)],
						m_RequireAll = new[] { NetPieceRequirements.LevelCrossing },
						m_RequireAny = new NetPieceRequirements[0],
						m_RequireNone = new[] { NetPieceRequirements.Median }
					},
					new NetPieceInfo
					{
						m_Piece = NetPieces[string.Format(pieceBaseName + " Flat {0}", width)],
						m_RequireAll = new[] { NetPieceRequirements.LevelCrossing, NetPieceRequirements.Median },
						m_RequireAny = new NetPieceRequirements[0],
						m_RequireNone = new[] { NetPieceRequirements.Intersection, NetPieceRequirements.DeadEnd }
					},
					new NetPieceInfo
					{
						m_Piece = NetPieces[string.Format(pieceBaseName + " Middle {0}", width)],
						m_RequireAll = new[] { NetPieceRequirements.Median },
						m_RequireAny = new[] { NetPieceRequirements.Intersection, NetPieceRequirements.DeadEnd },
						m_RequireNone = new[] { NetPieceRequirements.LevelCrossing }
					},
					new NetPieceInfo
					{
						m_Piece = NetPieces[string.Format(pieceBaseName + " Middle Flat {0}", width)],
						m_RequireAll = new[] { NetPieceRequirements.Median, NetPieceRequirements.LevelCrossing },
						m_RequireAny = new[] { NetPieceRequirements.Intersection, NetPieceRequirements.DeadEnd },
						m_RequireNone = new NetPieceRequirements[0],
					},
					new NetPieceInfo
					{
						m_Piece = NetPieces[string.Format("Elevated Bottom Piece {0}", width)],
						m_RequireAll = new[] { NetPieceRequirements.Elevated },
						m_RequireAny = new NetPieceRequirements[0] ,
						m_RequireNone = new[] { NetPieceRequirements.Intersection, NetPieceRequirements.HighTransition, NetPieceRequirements.LowTransition },
					},
					new NetPieceInfo
					{
						m_Piece = NetPieces[string.Format("Elevated Bottom Piece {0} - Ending", width)],
						m_RequireAll = new[] { NetPieceRequirements.Elevated },
						m_RequireAny = new[] { NetPieceRequirements.HighTransition, NetPieceRequirements.LowTransition },
						m_RequireNone = new NetPieceRequirements[0],
					},
					new NetPieceInfo
					{
						m_Piece = NetPieces[string.Format("Elevated Bottom Piece {0}", width)],
						m_RequireAll = new[] { NetPieceRequirements.Elevated, NetPieceRequirements.Intersection },
						m_RequireAny = new NetPieceRequirements[0] ,
						m_RequireNone = new[] { NetPieceRequirements.Median, NetPieceRequirements.HighTransition, NetPieceRequirements.LowTransition },
					},
					new NetPieceInfo
					{
						m_Piece = NetPieces[string.Format("Elevated Bottom Piece {0} - Intersection Middle", width)],
						m_RequireAll = new[] { NetPieceRequirements.Elevated, NetPieceRequirements.Intersection, NetPieceRequirements.Median },
						m_RequireAny = new NetPieceRequirements[0] ,
						m_RequireNone = new[] { NetPieceRequirements.HighTransition, NetPieceRequirements.LowTransition },
					},
					new NetPieceInfo
					{
						m_Piece = NetPieces[string.Format("Tunnel Top Piece {0}", width)],
						m_RequireAll = new[] { NetPieceRequirements.Tunnel },
						m_RequireAny = new NetPieceRequirements[0] ,
						m_RequireNone = new[] { NetPieceRequirements.Intersection, NetPieceRequirements.HighTransition, NetPieceRequirements.LowTransition },
					},
					new NetPieceInfo
					{
						m_Piece = NetPieces[string.Format("Tunnel Top Piece {0} - Ending", width)],
						m_RequireAll = new[] { NetPieceRequirements.Tunnel },
						m_RequireAny = new[] { NetPieceRequirements.HighTransition, NetPieceRequirements.LowTransition },
						m_RequireNone = new NetPieceRequirements[0],
					},
					new NetPieceInfo
					{
						m_Piece = NetPieces[string.Format("Tunnel Top Piece {0}", width)],
						m_RequireAll = new[] { NetPieceRequirements.Tunnel, NetPieceRequirements.Intersection },
						m_RequireAny = new NetPieceRequirements[0] ,
						m_RequireNone = new[] { NetPieceRequirements.Median, NetPieceRequirements.HighTransition, NetPieceRequirements.LowTransition },
					},
					new NetPieceInfo
					{
						m_Piece = NetPieces[string.Format("Tunnel Top Piece {0} - Intersection Middle", width)],
						m_RequireAll = new[] { NetPieceRequirements.Tunnel, NetPieceRequirements.Intersection, NetPieceRequirements.Median },
						m_RequireAny = new NetPieceRequirements[0] ,
						m_RequireNone = new[] { NetPieceRequirements.HighTransition, NetPieceRequirements.LowTransition },
					},
				};

				prefabSystem.AddPrefab(NetSections[section.name] = section);
			}
		}

		private void GenerateSidewalkSections()
		{
			foreach (var width in new float[] { 4, 3, 2.5f, 2, 1.5f, 1 })
			{
				var originalSize = width > 3 ? "4.5" : "3.5";
				var sidewalk = NetSections["Sidewalk " + originalSize].Clone("Sidewalk " + width.ToString(CultureInfo.InvariantCulture)) as NetSectionPrefab;

				foreach (var item in sidewalk.m_Pieces)
				{
					item.m_Piece = NetPieces[item.m_Piece.name.Replace(originalSize, width.ToString(CultureInfo.InvariantCulture))];
				}

				var offset = width switch
				{
					4 => 0.25f,
					_ => (3.5f - width) / 2f
				};

				var pieces = new List<NetPieceInfo>(sidewalk.m_Pieces);

				sidewalk.m_Pieces[0].m_RequireAll = new[] { NetPieceRequirements.Inverted };
				sidewalk.m_Pieces[1].m_RequireAll = new[] { NetPieceRequirements.Inverted, NetPieceRequirements.LevelCrossing };
				sidewalk.m_Pieces[0].m_Offset = new(-offset, 0f, 0f);
				sidewalk.m_Pieces[1].m_Offset = new(-offset, 0f, 0f);

				pieces.Add(new NetPieceInfo
				{
					m_Piece = sidewalk.m_Pieces[0].m_Piece,
					m_Offset = new(offset, 0f, 0f),
					m_RequireAll = new NetPieceRequirements[0],
					m_RequireAny = new NetPieceRequirements[0],
					m_RequireNone = new[] { NetPieceRequirements.Inverted, NetPieceRequirements.LevelCrossing, NetPieceRequirements.Tunnel, NetPieceRequirements.Elevated },
				});

				pieces.Add(new NetPieceInfo
				{
					m_Piece = sidewalk.m_Pieces[1].m_Piece,
					m_Offset = new(offset, 0f, 0f),
					m_RequireAll = new[] { NetPieceRequirements.LevelCrossing },
					m_RequireAny = new NetPieceRequirements[0],
					m_RequireNone = new[] { NetPieceRequirements.Inverted, NetPieceRequirements.Tunnel, NetPieceRequirements.Elevated },
				});

				sidewalk.m_Pieces = pieces.ToArray();

				prefabSystem.AddPrefab(sidewalk);
			}
		}

		private void AddParkingNetSections()
		{
			var sections = new[]
			{
				("RB Parking Piece Parallel", "RB Parking Section Parallel", "3"),
				("RB Parking Piece Parallel NoMarking", "RB Parking Section Parallel NoMarking", "2"),
				("RB Parking Piece Angled", "RB Parking Section Angled", "6"),
				("RB Parking Piece Angled NoMarking", "RB Parking Section Angled NoMarking", "6"),
				("RB Parking Piece Perpendicular", "RB Parking Section Perpendicular", "6"),
				("RB Parking Piece Perpendicular NoMarking", "RB Parking Section Perpendicular NoMarking", "6"),
			};

			foreach (var item in sections)
			{
				var section = ScriptableObject.CreateInstance<NetSectionPrefab>();
				var width = item.Item3;

				section.name = item.Item2;
				section.m_SubSections = new NetSubSectionInfo[0];
				section.m_Pieces = new[]
				{
					new NetPieceInfo
					{
						m_Piece = NetPieces[item.Item1],
						m_RequireAll = new NetPieceRequirements[0],
						m_RequireAny = new NetPieceRequirements[0],
						m_RequireNone = new[] { NetPieceRequirements.LevelCrossing, NetPieceRequirements.Median }
					},
					new NetPieceInfo
					{
						m_Piece = NetPieces[item.Item1],
						m_RequireAll = new[] { NetPieceRequirements.Median },
						m_RequireAny = new NetPieceRequirements[0],
						m_RequireNone = new[] { NetPieceRequirements.LevelCrossing, NetPieceRequirements.Intersection, NetPieceRequirements.DeadEnd }
					},
					new NetPieceInfo
					{
						m_Piece = NetPieces[item.Item1 + " - Flat"],
						m_RequireAll = new[] { NetPieceRequirements.LevelCrossing },
						m_RequireAny = new NetPieceRequirements[0],
						m_RequireNone = new[] { NetPieceRequirements.Median }
					},
					new NetPieceInfo
					{
						m_Piece = NetPieces[item.Item1 + " - Flat"],
						m_RequireAll = new[] { NetPieceRequirements.LevelCrossing, NetPieceRequirements.Median },
						m_RequireAny = new NetPieceRequirements[0],
						m_RequireNone = new[] { NetPieceRequirements.Intersection, NetPieceRequirements.DeadEnd }
					},
					new NetPieceInfo
					{
						m_Piece = NetPieces[string.Format("RB Empty Piece Middle {0}", width)],
						m_RequireAll = new[] { NetPieceRequirements.Median },
						m_RequireAny = new[] { NetPieceRequirements.Intersection, NetPieceRequirements.DeadEnd },
						m_RequireNone = new[] { NetPieceRequirements.LevelCrossing }
					},
					new NetPieceInfo
					{
						m_Piece = NetPieces[string.Format("RB Empty Piece Middle Flat {0}", width)],
						m_RequireAll = new[] { NetPieceRequirements.Median, NetPieceRequirements.LevelCrossing },
						m_RequireAny = new[] { NetPieceRequirements.Intersection, NetPieceRequirements.DeadEnd },
						m_RequireNone = new NetPieceRequirements[0],
					},
					new NetPieceInfo
					{
						m_Piece = NetPieces[string.Format("Elevated Bottom Piece {0}", width)],
						m_RequireAll = new[] { NetPieceRequirements.Elevated },
						m_RequireAny = new NetPieceRequirements[0] ,
						m_RequireNone = new[] { NetPieceRequirements.Intersection, NetPieceRequirements.HighTransition, NetPieceRequirements.LowTransition },
					},
					new NetPieceInfo
					{
						m_Piece = NetPieces[string.Format("Elevated Bottom Piece {0} - Ending", width)],
						m_RequireAll = new[] { NetPieceRequirements.Elevated },
						m_RequireAny = new[] { NetPieceRequirements.HighTransition, NetPieceRequirements.LowTransition },
						m_RequireNone = new NetPieceRequirements[0],
					},
					new NetPieceInfo
					{
						m_Piece = NetPieces[string.Format("Elevated Bottom Piece {0}", width)],
						m_RequireAll = new[] { NetPieceRequirements.Elevated, NetPieceRequirements.Intersection },
						m_RequireAny = new NetPieceRequirements[0] ,
						m_RequireNone = new[] { NetPieceRequirements.Median, NetPieceRequirements.HighTransition, NetPieceRequirements.LowTransition },
					},
					new NetPieceInfo
					{
						m_Piece = NetPieces[string.Format("Elevated Bottom Piece {0} - Intersection Middle", width)],
						m_RequireAll = new[] { NetPieceRequirements.Elevated, NetPieceRequirements.Intersection, NetPieceRequirements.Median },
						m_RequireAny = new NetPieceRequirements[0] ,
						m_RequireNone = new[] { NetPieceRequirements.HighTransition, NetPieceRequirements.LowTransition },
					},
					new NetPieceInfo
					{
						m_Piece = NetPieces[string.Format("Tunnel Top Piece {0}", width)],
						m_RequireAll = new[] { NetPieceRequirements.Tunnel },
						m_RequireAny = new NetPieceRequirements[0] ,
						m_RequireNone = new[] { NetPieceRequirements.Intersection, NetPieceRequirements.HighTransition, NetPieceRequirements.LowTransition },
					},
					new NetPieceInfo
					{
						m_Piece = NetPieces[string.Format("Tunnel Top Piece {0} - Ending", width)],
						m_RequireAll = new[] { NetPieceRequirements.Tunnel },
						m_RequireAny = new[] { NetPieceRequirements.HighTransition, NetPieceRequirements.LowTransition },
						m_RequireNone = new NetPieceRequirements[0],
					},
					new NetPieceInfo
					{
						m_Piece = NetPieces[string.Format("Tunnel Top Piece {0}", width)],
						m_RequireAll = new[] { NetPieceRequirements.Tunnel, NetPieceRequirements.Intersection },
						m_RequireAny = new NetPieceRequirements[0] ,
						m_RequireNone = new[] { NetPieceRequirements.Median, NetPieceRequirements.HighTransition, NetPieceRequirements.LowTransition },
					},
					new NetPieceInfo
					{
						m_Piece = NetPieces[string.Format("Tunnel Top Piece {0} - Intersection Middle", width)],
						m_RequireAll = new[] { NetPieceRequirements.Tunnel, NetPieceRequirements.Intersection, NetPieceRequirements.Median },
						m_RequireAny = new NetPieceRequirements[0] ,
						m_RequireNone = new[] { NetPieceRequirements.HighTransition, NetPieceRequirements.LowTransition },
					},
				};

				prefabSystem.AddPrefab(NetSections[section.name] = section);
			}
		}

		private void AddCustomPrefabComponents()
		{
			SetUp("Pavement Path Section 3", "coui://roadbuildericons/RB_PedestrianLane.svg").WithRequireAll(RoadCategory.Pathway).AddLaneThumbnail("coui://roadbuildericons/Thumb_PedestrianLaneWide.svg");
			SetUp("Tiled Pedestrian Section 3", "coui://roadbuildericons/RB_PedestrianOnly.svg").WithRequireAll(RoadCategory.Tiled).AddLaneThumbnail("coui://roadbuildericons/Thumb_TiledSmall.svg");
			SetUp("RB Tiled Median 2", "coui://roadbuildericons/RB_TiledMedian_Centered.svg").WithRequireAll(RoadCategory.Tiled).WithThumbnail("coui://roadbuildericons/RB_TiledMedian.svg").AddLaneThumbnail("coui://roadbuildericons/Thumb_PedestrianLaneSmall.svg");
			SetUp("Sound Barrier 1", "coui://roadbuildericons/RB_SoundBarrier.svg").AddLaneThumbnail("coui://roadbuildericons/Thumb_SoundBarrier.svg");

			NetSections["Sound Barrier 1"].AddComponent<RoadBuilderEdgeLaneInfo>().DoNotRequireBeingOnEdge = true;

			var pathEdgeInfo = NetSections["Pavement Path Section 3"].AddComponent<RoadBuilderEdgeLaneInfo>();
			pathEdgeInfo.SidePrefab = NetSections["Pavement Path Side Section 0"];
			pathEdgeInfo.AddSidewalkStateOnNode = true;
			pathEdgeInfo.DoNotRequireBeingOnEdge = true;
		}

		private RoadBuilderLaneInfo SetUp(string prefabName, string thumbnail)
		{
			var prefab = NetSections[prefabName];

			prefab.AddOrGetComponent<UIObject>().m_Icon = thumbnail;

			var info = prefab.AddOrGetComponent<RoadBuilderLaneInfo>();

			info.RoadBuilder = true;

			return info;
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
