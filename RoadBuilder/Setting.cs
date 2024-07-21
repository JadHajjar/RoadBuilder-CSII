using Colossal.IO.AssetDatabase;

using Game.Input;
using Game.Modding;
using Game.Settings;

namespace RoadBuilder
{
	[FileLocation("ModsSettings/" + nameof(RoadBuilder) + "/" + nameof(RoadBuilder))]
	[SettingsUIGroupOrder(kButtonGroup, MAIN_GROUP, kSliderGroup, kDropdownGroup)]
	[SettingsUIShowGroupName(kButtonGroup, MAIN_GROUP, kSliderGroup, kDropdownGroup)]
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

		[SettingsUISection(MAIN_SECTION, MAIN_GROUP)]
		public bool SaveUsedRoadsOnly { get; set; }

		[SettingsUISection(MAIN_SECTION, MAIN_GROUP)]
		public bool AdvancedUserMode { get; set; }

		//[SettingsUISection(kSection, kButtonGroup)]
		//public bool Button { set => Mod.Log.Info("Button clicked"); }

		//[SettingsUIButton]
		//[SettingsUIConfirmation]
		//[SettingsUISection(kSection, kButtonGroup)]
		//public bool ButtonWithConfirmation { set => Mod.Log.Info("ButtonWithConfirmation clicked"); }

		//[SettingsUISection(kSection, kToggleGroup)]
		//public bool Toggle { get; set; }

		//[SettingsUISlider(min = 0, max = 100, step = 1, scalarMultiplier = 1, unit = Unit.kDataMegabytes)]
		//[SettingsUISection(kSection, kSliderGroup)]
		//public int IntSlider { get; set; }

		//[SettingsUIDropdown(typeof(Setting), nameof(GetIntDropdownItems))]
		//[SettingsUISection(kSection, kDropdownGroup)]
		//public int IntDropdown { get; set; }

		//[SettingsUISection(kSection, kDropdownGroup)]
		//public SomeEnum EnumDropdown { get; set; } = SomeEnum.Value1;

		//public DropdownItem<int>[] GetIntDropdownItems()
		//{
		//	var items = new List<DropdownItem<int>>();

		//	for (var i = 0; i < 3; i += 1)
		//	{
		//		items.Add(new DropdownItem<int>()
		//		{
		//			value = i,
		//			displayName = i.ToString(),
		//		});
		//	}

		//	return items.ToArray();
		//}

		public override void SetDefaults()
		{
		}

		public enum SomeEnum
		{
			Value1,
			Value2,
			Value3,
		}
	}
}
