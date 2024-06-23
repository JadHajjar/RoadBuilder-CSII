using Colossal.IO.AssetDatabase;
using Colossal.Logging;

using Game;
using Game.Input;
using Game.Modding;
using Game.Prefabs;
using Game.SceneFlow;
using HarmonyLib;
using RoadBuilder.Systems;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Mathematics;

namespace RoadBuilder
{
	public class Mod : IMod
	{
		public const string Id = nameof(RoadBuilder);
		public static ILog Log { get; } = LogManager.GetLogger(nameof(RoadBuilder)).SetShowsErrorsInUI(false);
		public static Setting Settings { get; private set; }

		public void OnLoad(UpdateSystem updateSystem)
		{
			Log.Info(nameof(OnLoad));

			//Settings = new Setting(this);
			//Settings.RegisterInOptionsUI();
			//GameManager.instance.localizationManager.AddSource("en-US", new LocaleEN(Settings));

			//AssetDatabase.global.LoadSettings(nameof(RoadBuilder), Settings, new Setting(this));

			updateSystem.UpdateAt<NetSectionsUISystem>(SystemUpdatePhase.UIUpdate);
		}

        void LoadPrefab()
        {
            var prefabSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<PrefabSystem>();
            var prefabs = Traverse.Create(prefabSystem).Field<List<PrefabBase>>("m_Prefabs").Value;
            RoadPrefab basePrefab = (RoadPrefab)prefabs.FirstOrDefault(p => p.name == "Small Road");
            NetSectionPrefab roadLanePrefab = (NetSectionPrefab)prefabs.FirstOrDefault(p => p.name == "Car Drive Section 3");
            NetSectionPrefab sidewalk5Prefab = (NetSectionPrefab)prefabs.FirstOrDefault(p => p.name == "Sidewalk 5");

            ComponentBase[] baseComponents = new ComponentBase[basePrefab.components.Count];
            basePrefab.components.CopyTo(baseComponents);

            RoadPrefab prefab = new RoadPrefab()
            {
                m_SpeedLimit = 80,
                m_RoadType = RoadType.Normal,
                m_TrafficLights = false,
                m_HighwayRules = false,
                m_MaxSlopeSteepness = 0.2f,
                m_InvertMode = CompositionInvertMode.FlipLefthandTraffic,
                isDirty = true,
                active = true,
                m_Sections = new NetSectionInfo[basePrefab.m_Sections.Length],
                m_ZoneBlock = basePrefab.m_ZoneBlock,
                m_AggregateType = basePrefab.m_AggregateType,
                m_NodeStates = new NetNodeStateInfo[basePrefab.m_NodeStates.Length],
                m_EdgeStates = new NetEdgeStateInfo[basePrefab.m_EdgeStates.Length]
            };
            prefab.components.AddRange(baseComponents);
            basePrefab.m_Sections.CopyTo(prefab.m_Sections, 0);
            basePrefab.m_NodeStates.CopyTo(prefab.m_NodeStates, 0);
            basePrefab.m_EdgeStates.CopyTo(prefab.m_EdgeStates, 0);
            prefab.name = "Test Road Prefab";
            prefab.m_Sections[1].m_Section = sidewalk5Prefab;
            prefab.m_Sections[5].m_Section = sidewalk5Prefab;
            prefab.m_Sections = new NetSectionInfo[]
            {
                prefab.m_Sections[0],
                prefab.m_Sections[1],
                new NetSectionInfo()
                {
                    m_Section = roadLanePrefab,
                    m_Flip = false,
                    m_Invert = true,
                    m_RequireAll = new NetPieceRequirements[0],
                    m_RequireAny = new NetPieceRequirements[0],
                    m_RequireNone = new NetPieceRequirements[0],
                    m_Median = false,
                    m_Offset = float3.zero
                },
                prefab.m_Sections[2],
                prefab.m_Sections[3],
                prefab.m_Sections[4],
                new NetSectionInfo()
                {
                    m_Section = roadLanePrefab,
                    m_Flip = false,
                    m_Invert = false,
                    m_RequireAll = new NetPieceRequirements[0],
                    m_RequireAny = new NetPieceRequirements[0],
                    m_RequireNone = new NetPieceRequirements[0],
                    m_Median = false,
                    m_Offset = float3.zero
                },
                prefab.m_Sections[5],
                prefab.m_Sections[6]
            };            
            if (prefabSystem.AddPrefab(prefab))
            {
                Log.Info("Generated Prefab!");
            }
        }

        public void OnDispose()
		{
			Log.Info(nameof(OnDispose));

			Settings?.UnregisterInOptionsUI();
		}
	}
}      