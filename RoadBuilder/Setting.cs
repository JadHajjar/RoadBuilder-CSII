using Colossal.IO.AssetDatabase;

using Game.Input;
using Game.Modding;
using Game.Settings;

namespace RoadBuilder
{
	[FileLocation("ModsSettings/" + nameof(RoadBuilder) + "/" + nameof(RoadBuilder))]
	[SettingsUIGroupOrder(MAIN_GROUP)]
	[SettingsUIShowGroupName(MAIN_GROUP)]
	[SettingsUIMouseAction(nameof(RoadBuilder) + "Apply", "CustomUsage")]
	[SettingsUIMouseAction(nameof(RoadBuilder) + "Cancel", "CustomUsage")]
	public class Setting : ModSetting
	{
		public const string MAIN_SECTION = "Main";

		public const string MAIN_GROUP = "Main";

		public Setting(IMod mod) : base(mod)
		{

		}

		[SettingsUIMouseBinding(nameof(RoadBuilder) + "Apply"), SettingsUIHidden]
		public ProxyBinding ApplyMimic { get; set; }

		[SettingsUIMouseBinding(nameof(RoadBuilder) + "Cancel"), SettingsUIHidden]
		public ProxyBinding CancelMimic { get; set; }

		[SettingsUISection(MAIN_SECTION, MAIN_GROUP), SettingsUIKeyboardBinding()]
		public ProxyBinding ToolToggle { get; set; }

		[SettingsUISection(MAIN_SECTION, MAIN_GROUP)]
		public bool SaveUsedRoadsOnly { get; set; }

		[SettingsUISection(MAIN_SECTION, MAIN_GROUP)]
		public bool NoImitateLaneOptionsOnPlace { get; set; }

		[SettingsUISection(MAIN_SECTION, MAIN_GROUP)]
		public bool AdvancedUserMode { get; set; }
		[SettingsUISection(MAIN_SECTION, MAIN_GROUP)]
		public bool HideArrowsOnThumbnails { get; set; }
		[SettingsUISection(MAIN_SECTION, MAIN_GROUP)]
		public bool DoNotAddSides { get; set; }

		public override void SetDefaults()
		{
		}
	}
}
