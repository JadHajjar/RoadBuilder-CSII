using Colossal.IO.AssetDatabase;

using Game.Input;
using Game.Modding;
using Game.Settings;

namespace RoadBuilder
{
	[FileLocation("ModsSettings/" + nameof(RoadBuilder) + "/" + nameof(RoadBuilder))]
	[SettingsUIGroupOrder(HOTKEY_GROUP, MAIN_GROUP, ADVANCED_GROUP)]
	[SettingsUIShowGroupName(HOTKEY_GROUP, MAIN_GROUP, ADVANCED_GROUP)]
	[SettingsUIMouseAction(nameof(RoadBuilder) + "Apply", "CustomUsage")]
	[SettingsUIMouseAction(nameof(RoadBuilder) + "Cancel", "CustomUsage")]
	public class Setting : ModSetting
	{
		public const string MAIN_SECTION = "Main";

		public const string HOTKEY_GROUP = "Hotkey";
		public const string MAIN_GROUP = "Main";
		public const string ADVANCED_GROUP = "Advanced";

		public Setting(IMod mod) : base(mod)
		{

		}

		[SettingsUIMouseBinding(nameof(RoadBuilder) + "Apply"), SettingsUIHidden]
		public ProxyBinding ApplyMimic { get; set; }

		[SettingsUIMouseBinding(nameof(RoadBuilder) + "Cancel"), SettingsUIHidden]
		public ProxyBinding CancelMimic { get; set; }

		[SettingsUISection(MAIN_SECTION, HOTKEY_GROUP), SettingsUIKeyboardBinding()]
		public ProxyBinding ToolToggle { get; set; }

		[SettingsUISection(MAIN_SECTION, HOTKEY_GROUP), SettingsUIKeyboardBinding()]
		public ProxyBinding PlaceToggle { get; set; }

		[SettingsUISection(MAIN_SECTION, MAIN_GROUP)]
		public bool SaveUsedRoadsOnly { get; set; }

		[SettingsUISection(MAIN_SECTION, MAIN_GROUP)]
		public bool HideArrowsOnThumbnails { get; set; }

		[SettingsUISection(MAIN_SECTION, MAIN_GROUP)]
		public bool NoImitateLaneOptionsOnPlace { get; set; }

		[SettingsUIHidden]
		public bool AdvancedUserMode { set => UnrestrictedLanes = value; }

		[SettingsUISection(MAIN_SECTION, ADVANCED_GROUP)]
		public bool RemoveLockRequirements { get; set; }

		[SettingsUISection(MAIN_SECTION, ADVANCED_GROUP)]
		public bool UnrestrictedLanes { get; set; }

		[SettingsUISection(MAIN_SECTION, ADVANCED_GROUP)]
		public bool RemoveSafetyMeasures { get; set; }

		[SettingsUISection(MAIN_SECTION, ADVANCED_GROUP)]
		public bool DoNotAddSides { get; set; }

		public override void SetDefaults()
		{
		}
	}
}
