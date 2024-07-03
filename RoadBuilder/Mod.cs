using Colossal.IO.AssetDatabase;
using Colossal.Logging;

using Game;
using Game.Input;
using Game.Modding;
using Game.Prefabs;
using Game.SceneFlow;
using Game.Serialization;
using Game.Tools;

using HarmonyLib;
using RoadBuilder.Systems;
using RoadBuilder.Systems.UI;
using RoadBuilder.Utilities;

using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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

			Settings = new Setting(this);
			Settings.RegisterKeyBindings();
			Settings.RegisterInOptionsUI();

			foreach (var item in new LocaleHelper("RoadBuilder.Locale.json").GetAvailableLanguages())
			{
				GameManager.instance.localizationManager.AddSource(item.LocaleId, item);
			}

			AssetDatabase.global.LoadSettings(nameof(RoadBuilder), Settings, new Setting(this));

			updateSystem.UpdateBefore<RoadBuilderSerializeSystem>(SystemUpdatePhase.Serialize);
			updateSystem.UpdateAt<RoadBuilderSystem>(SystemUpdatePhase.Modification1);
			updateSystem.UpdateAt<RoadBuilderApplyTagSystem>(SystemUpdatePhase.Modification1);
			updateSystem.UpdateAt<RoadBuilderToolSystem>(SystemUpdatePhase.ToolUpdate);
			updateSystem.UpdateAt<RoadBuilderUISystem>(SystemUpdatePhase.UIUpdate);
			updateSystem.UpdateAt<NetSectionsUISystem>(SystemUpdatePhase.UIUpdate);
		}

		public void OnDispose()
		{
			Log.Info(nameof(OnDispose));

			Settings?.UnregisterInOptionsUI();
		}
	}
}      