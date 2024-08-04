using Colossal.IO.AssetDatabase;
using Colossal.Logging;
using Colossal.UI;

using Game;
using Game.Modding;
using Game.Prefabs;
using Game.SceneFlow;

using HarmonyLib;

using RoadBuilder.Systems;
using RoadBuilder.Systems.UI;
using RoadBuilder.Utilities;

using System.IO;

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

#if DEBUG
			Log.SetEffectiveness(Level.Debug);
#endif

			UIManager.defaultUISystem.AddHostLocation("roadbuilderthumbnails", FoldersUtil.TempFolder, true);

			if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
			{
				UIManager.defaultUISystem.AddHostLocation($"roadbuildericons", Path.Combine(Path.GetDirectoryName(asset.path), "PrefabIcons"), false);
			}
			else
			{
				Log.Error("Load Failed, could not get executable path");
			}

			Settings = new Setting(this);
			Settings.RegisterKeyBindings();
			Settings.RegisterInOptionsUI();

			foreach (var item in new LocaleHelper("RoadBuilder.Locale.json").GetAvailableLanguages())
			{
				GameManager.instance.localizationManager.AddSource(item.LocaleId, item);
			}

			AssetDatabase.global.LoadSettings(nameof(RoadBuilder), Settings, new Setting(this));

			updateSystem.UpdateAfter<NetSectionsSystem, PrefabInitializeSystem>(SystemUpdatePhase.PrefabUpdate);
			updateSystem.UpdateAfter<RoadBuilderPrefabUpdateSystem, PrefabInitializeSystem>(SystemUpdatePhase.PrefabUpdate);
			updateSystem.UpdateAt<RoadInitializerSystem>(SystemUpdatePhase.PrefabUpdate);
			updateSystem.UpdateAt<RoadBuilderInfoViewFixSystem>(SystemUpdatePhase.PrefabUpdate);
			updateSystem.UpdateAt<RoadGenerationDataSystem>(SystemUpdatePhase.PrefabReferences);
			updateSystem.UpdateBefore<RoadBuilderSerializeSystem>(SystemUpdatePhase.Serialize);
			updateSystem.UpdateAt<RoadBuilderSystem>(SystemUpdatePhase.Modification1);
			updateSystem.UpdateAt<RoadBuilderApplyTagSystem>(SystemUpdatePhase.Modification2);
			updateSystem.UpdateAt<RoadBuilderClearTagSystem>(SystemUpdatePhase.Modification2);
			updateSystem.UpdateAt<RoadBuilderToolSystem>(SystemUpdatePhase.ToolUpdate);
			updateSystem.UpdateAt<RoadBuilderConfigCleanupSystem>(SystemUpdatePhase.MainLoop);
			updateSystem.UpdateAt<RoadBuilderUISystem>(SystemUpdatePhase.UIUpdate);
			updateSystem.UpdateAt<NetSectionsUISystem>(SystemUpdatePhase.UIUpdate);
			updateSystem.UpdateAt<RoadBuilderConfigurationsUISystem>(SystemUpdatePhase.UIUpdate);

			updateSystem.UpdateBefore<MedianPlatformSystem, NetCompositionSystem>(SystemUpdatePhase.Modification4);
		}

		public void OnDispose()
		{
			Log.Info(nameof(OnDispose));

			Settings?.UnregisterInOptionsUI();

			UIManager.defaultUISystem.RemoveHostLocation("roadbuilderthumbnails");

			new DirectoryInfo(FoldersUtil.TempFolder).Delete(true);
		}
	}
}