using Game.Prefabs;

using RoadBuilder.Domain;
using RoadBuilder.Domain.Components.Prefabs;
using RoadBuilder.Domain.Configurations;
using RoadBuilder.Domain.Enums;
using RoadBuilder.Domain.Prefabs;
using RoadBuilder.Domain.UI;
using RoadBuilder.Systems;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using Unity.Entities;

namespace RoadBuilder.Utilities
{
	public static class LaneOptionsUtil
	{
		private enum ActionType
		{
			Invert
		}

		private static readonly RoadBuilderNetSectionsSystem _netSectionsSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<RoadBuilderNetSectionsSystem>();

		#region Combination rules

		// These rules are shared by road generation, the lane options UI and road imports, so a variant is matched the same way everywhere.
		// Option names and values are matched without case, and a variant that leaves an option out reads as an empty value.

		public static bool TryGetCombinationValue(NetSectionPrefab section, string? optionName, out string? value)
		{
			if (section.TryGet<RoadBuilderLaneGroup>(out var laneGroup) && laneGroup.Combination is not null)
			{
				foreach (var item in laneGroup.Combination)
				{
					if (item is not null && string.Equals(item.OptionName, optionName, StringComparison.InvariantCultureIgnoreCase))
					{
						value = item.Value;
						return true;
					}
				}
			}

			value = null;
			return false;
		}

		public static string? GetCombinationValue(NetSectionPrefab section, string? optionName)
		{
			return TryGetCombinationValue(section, optionName, out var value) ? value : null;
		}

		public static bool SameValue(string? a, string? b)
		{
			return string.Equals(a ?? string.Empty, b ?? string.Empty, StringComparison.InvariantCultureIgnoreCase);
		}

		// Decoration values: no value means no decorations, "G" grass only, "T" trees only, anything else both
		public static bool SupportsGrass(string? value)
		{
			return value is not null && !SameValue(value, "T");
		}

		public static bool SupportsTrees(string? value)
		{
			return value is not null && !SameValue(value, "G");
		}

		private static bool SupportsDecoration(string? sectionValue, string? selectedValue)
		{
			var needsGrass = selectedValue?.IndexOf('G') >= 0;
			var needsTrees = selectedValue?.IndexOf('T') >= 0;

			return (!needsGrass || SupportsGrass(sectionValue)) && (!needsTrees || SupportsTrees(sectionValue));
		}

		// The two-way variant is the one that lists the TwoWay option, with any value other than "0"
		public static bool IsTwoWayVariant(NetSectionPrefab section, string? optionName)
		{
			return TryGetCombinationValue(section, optionName, out var value) && value != "0";
		}

		public static bool IsTwoWaySelected(string? selectedValue)
		{
			return selectedValue == "1";
		}

		public static bool MatchesOptionValue(NetSectionPrefab section, RoadBuilderLaneOption? option, string? selectedValue)
		{
			if (option is null)
			{
				return false;
			}

			switch (option.Type)
			{
				case LaneOptionType.Checkbox:
					return string.IsNullOrEmpty(selectedValue) == string.IsNullOrEmpty(GetCombinationValue(section, option.Name));
				case LaneOptionType.Decoration:
					return SupportsDecoration(GetCombinationValue(section, option.Name), selectedValue);
				case LaneOptionType.TwoWay:
					return IsTwoWayVariant(section, option.Name) == IsTwoWaySelected(selectedValue);
				default:
					return SameValue(GetCombinationValue(section, option.Name), selectedValue);
			}
		}

		#endregion

		public static List<OptionSectionUIEntry> GenerateOptions(RoadGenerationData? roadGenerationData, INetworkConfig config, LaneConfig lane)
		{
			var options = new List<OptionSectionUIEntry>();

			if (NetworkPrefabGenerationUtil.GetNetSection(roadGenerationData, config, lane, out var section, out var group)) // if lane supports invert
			{
				if ((group is not null || !(section?.SupportsTwoWay() ?? false)) && !(section?.TryGet<RoadBuilderLaneInfo>(out var laneInfo) == true && laneInfo.NoDirection) && !((group?.TryGet<RoadBuilderLaneInfo>(out var groupInfo) ?? false) && groupInfo.NoDirection))
				{
					options.Add(GetInvertOption(config, lane, group?.Options?.FirstOrDefault(x => x.Type is LaneOptionType.TwoWay)));
				}
			}

			if (!string.IsNullOrEmpty(lane.GroupPrefabName))
			{
				options.AddRange(GenerateGroupOptions(config, lane));
			}

			return options;
		}

		private static IEnumerable<OptionSectionUIEntry> GenerateGroupOptions(INetworkConfig config, LaneConfig lane)
		{
			if (!_netSectionsSystem.LaneGroups.TryGetValue(lane.GroupPrefabName ?? string.Empty, out var group) || group.Options is null)
			{
				yield break;
			}

			var index = 0;
			var remainingSections = group.LinkedSections.Where(x => x.MatchCategories(config)).ToList();

			foreach (var option in group.Options)
			{
				// Ids map back to group.Options in GroupOptionClicked, so they follow the option's position even when some are skipped
				index--;

				if (remainingSections.Count == 0)
				{
					yield break;
				}

				if (option is null || option.Type is LaneOptionType.TwoWay)
				{
					continue;
				}

				var optionValues = option.Options?.Where(x => x is not null).ToArray() ?? new RoadBuilderLaneOptionValue[0];
				var entries = new OptionItemUIEntry[option.Type is LaneOptionType.Decoration or LaneOptionType.TwoWay ? 2 : option.Type is LaneOptionType.ValueUpDown or LaneOptionType.LaneWidth or LaneOptionType.Checkbox ? 1 : optionValues.Length];
				var value = GetSelectedOptionValue(config, lane, option);

				if (option.Type is LaneOptionType.Decoration)
				{
					var grassAvailable = remainingSections.Any(x => SupportsGrass(GetCombinationValue(x, option.Name)));
					var treeAvailable = remainingSections.Any(x => SupportsTrees(GetCombinationValue(x, option.Name)));

					entries[0] = new()
					{
						Id = 1,
						Name = "RoadBuilder.Grass",
						Icon = "coui://roadbuildericons/RB_GrassWhite.svg",
						Selected = value is "GT" or "G",
						Disabled = !grassAvailable,
					};
					entries[1] = new()
					{
						Id = 2,
						Name = "RoadBuilder.Trees",
						Icon = "coui://roadbuildericons/RB_TreeWhite.svg",
						Selected = value is "GT" or "T",
						Disabled = !treeAvailable,
					};
				}
				else if (option.Type is LaneOptionType.Checkbox)
				{
					// A variant that leaves the option out counts as the unchecked one
					var available = remainingSections.Any(x => string.IsNullOrEmpty(GetCombinationValue(x, option.Name)) != string.IsNullOrEmpty(value));

					entries[0] = new()
					{
						Id = string.IsNullOrEmpty(value) ? 0 : 1,
						Selected = !string.IsNullOrEmpty(value),
						Disabled = !available,
					};
				}
				else if (option.Type is LaneOptionType.ValueUpDown or LaneOptionType.LaneWidth)
				{
					if (remainingSections.Count <= 1)
					{
						remainingSections.RemoveAll(x => !MatchesOptionValue(x, option, value));
						continue;
					}

					entries[0] = new()
					{
						IsValue = option.Type is LaneOptionType.ValueUpDown or LaneOptionType.LaneWidth,
						Value = option.Type is LaneOptionType.LaneWidth ? ConvertWidth(value) : LocaleHelper.Translate($"{group.name}.Options[{option.Name}][{value}]", value),
					};
				}
				else
				{
					for (var i = 0; i < entries.Length; i++)
					{
						var optionValue = optionValues[i].Value;

						entries[i] = new OptionItemUIEntry
						{
							// The id is the value's position in option.Options, which GroupOptionClicked indexes into
							Id = Array.IndexOf(option.Options!, optionValues[i]),
							Name = LocaleHelper.Translate($"{group.name}.Options[{option.Name}][{optionValue}]", optionValue),
							Icon = optionValues[i].ThumbnailUrl,
							Selected = SameValue(optionValue, value),
							Disabled = !remainingSections.Any(x => SameValue(GetCombinationValue(x, option.Name), optionValue))
						};
					}
				}

				yield return new OptionSectionUIEntry
				{
					Id = index,
					Options = entries,
					IsToggle = option.Type is LaneOptionType.Toggle,
					IsCheckbox = option.Type is LaneOptionType.Checkbox,
					Name = option.Type is LaneOptionType.Decoration
					? LocaleHelper.Translate("RoadBuilder.Decoration", "Decoration")
					: LocaleHelper.Translate($"{group.name}.Options[{option.Name}]", option.Name)
				};

				remainingSections.RemoveAll(x => !MatchesOptionValue(x, option, value));
			}
		}

		private static string? ConvertWidth(string? value)
		{
			if (!double.TryParse(value?.TrimEnd('m').Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var width))
			{
				return value;
			}

			if (RoadOptionsUtil.IsMetric())
			{
				return $"{width:0.##} m";
			}

			return $"{Math.Round(width * 3.28084 * 4, MidpointRounding.AwayFromZero) / 4:0.##} ft";
		}

		private static OptionSectionUIEntry GetInvertOption(INetworkConfig config, LaneConfig lane, RoadBuilderLaneOption? twoWayOption)
		{
			var isTwoWaySelected = twoWayOption is not null && IsTwoWaySelected(GetSelectedOptionValue(config, lane, twoWayOption));

			return new OptionSectionUIEntry
			{
				Id = (int)ActionType.Invert,
				Name = LocaleHelper.Translate("RoadBuilder.Direction", "Direction"),
				Options = new OptionItemUIEntry[]
				{
					new()
					{
						Name = "RoadBuilder.Backward",
						Icon = "coui://roadbuildericons/RB_ArrowDown.svg",
						Selected = !isTwoWaySelected && lane.Invert,
						Id = 0,
					},
					new()
					{
						Name = "RoadBuilder.Forward",
						Icon = "coui://roadbuildericons/RB_Arrow.svg",
						Selected = !isTwoWaySelected && !lane.Invert,
						Id = 1,
					},
					new()
					{
						Name = "RoadBuilder.TwoWay",
						Icon = "coui://roadbuildericons/RB_ArrowBoth.svg",
						Selected = isTwoWaySelected,
						Id = 2,
						Hidden = twoWayOption is null
					},
				}
			};
		}

		public static void OptionClicked(RoadGenerationData? roadGenerationData, INetworkConfig config, LaneConfig lane, int optionId, int id, int value)
		{
			if (optionId < 0)
			{
				GroupOptionClicked(config, lane, -optionId - 1, id, value);
				return;
			}

			switch (optionId)
			{
				case (int)ActionType.Invert:
					if (id is 0 or 1)
					{
						lane.Invert = id == 0;
					}

					if (NetworkPrefabGenerationUtil.GetNetSection(roadGenerationData, config, lane, out var section, out var group))
					{
						var option = group?.Options?.FirstOrDefault(x => x?.Type is LaneOptionType.TwoWay);

						if (option?.Name is not null)
						{
							lane.GroupOptions[option.Name] = id != 2 || IsTwoWaySelected(GetSelectedOptionValue(config, lane, option)) ? null : "1";
						}
					}

					break;
			}
		}

		public static void GroupOptionClicked(INetworkConfig config, LaneConfig lane, int optionId, int id, int value)
		{
			if (!_netSectionsSystem.LaneGroups.TryGetValue(lane.GroupPrefabName ?? string.Empty, out var group))
			{
				return;
			}

			var option = group.Options?.ElementAtOrDefault(optionId);

			try
			{
				if (option is null)
				{
					return;
				}

				if (option.Type is LaneOptionType.Decoration)
				{
					var laneIndex = config.Lanes.IndexOf(lane);
					RoadAddons addon = default;

					if (id == 1)
					{
						addon = laneIndex == 0 ? RoadAddons.GrassLeft : laneIndex == config.Lanes.Count - 1 ? RoadAddons.GrassRight : RoadAddons.GrassCenter;
					}
					else if (id == 2)
					{
						addon = laneIndex == 0 ? RoadAddons.TreesLeft : laneIndex == config.Lanes.Count - 1 ? RoadAddons.TreesRight : RoadAddons.TreesCenter;
					}

					if (config.Addons.HasFlag(addon))
					{
						config.Addons &= ~addon;
					}
					else
					{
						config.Addons |= addon;
					}

					return;
				}

				if (option.Type is LaneOptionType.Checkbox)
				{
					lane.GroupOptions[option.Name ?? string.Empty] = id == 0 ? "checked" : "";

					return;
				}

				if (option.Type is not LaneOptionType.ValueUpDown and not LaneOptionType.LaneWidth)
				{
					var clickedValue = option.Options?.ElementAtOrDefault(id);

					if (clickedValue is not null)
					{
						lane.GroupOptions[option.Name ?? string.Empty] = clickedValue.Value;
					}

					return;
				}

				if (option.Options is null)
				{
					return;
				}

				var remainingSections = group.LinkedSections.Where(x => x.MatchCategories(config)).ToList();

				foreach (var item in group.Options ?? Enumerable.Empty<RoadBuilderLaneOption>())
				{
					if (remainingSections.Count == 0 || item == option)
					{
						break;
					}

					if (item is null || item.Type is LaneOptionType.TwoWay)
					{
						continue;
					}

					var value_ = GetSelectedOptionValue(config, lane, item);

					remainingSections.RemoveAll(x => !MatchesOptionValue(x, item, value_));
				}

				var currentValue = GetSelectedOptionValue(config, lane, option);
				var currentOption = option.Options.FirstOrDefault(x => x is not null && SameValue(x.Value, currentValue));
				var validOptions = option.Options.Where(x => x is not null && remainingSections.Any(s => MatchesOptionValue(s, option, x.Value))).ToList();

				if (value > 0)
				{
					lane.GroupOptions[option.Name ?? string.Empty] = validOptions.Next(currentOption)?.Value ?? currentValue;
				}
				else
				{
					lane.GroupOptions[option.Name ?? string.Empty] = validOptions.Previous(currentOption)?.Value ?? currentValue;
				}
			}
			finally
			{
				FixGroupOptions(config, lane, group);
			}
		}

		public static void FixGroupOptions(INetworkConfig config, LaneConfig lane, LaneGroupPrefab group)
		{
			var remainingSections = group.LinkedSections.Where(x => x.MatchCategories(config)).ToList();

			foreach (var option in group.Options ?? Enumerable.Empty<RoadBuilderLaneOption>())
			{
				if (option is null)
				{
					continue;
				}

				if (remainingSections.Count == 0)
				{
					Mod.Log.WarnFormat("Lane Fix Failed: no remaining sections left at option '{0}'", option.Name);

					lane.GroupOptions = new();

					return;
				}

				if (option.Type is LaneOptionType.TwoWay)
				{
					continue;
				}

				var value = GetSelectedOptionValue(config, lane, option);

				if (!remainingSections.Any(x => MatchesOptionValue(x, option, value)))
				{
					lane.GroupOptions[option.Name ?? string.Empty] = value = GetCombinationValue(remainingSections[0], option.Name);
				}

				remainingSections.RemoveAll(x => !MatchesOptionValue(x, option, value));
			}
		}

		public static string? GetSelectedOptionValue(INetworkConfig? config, LaneConfig lane, RoadBuilderLaneOption option)
		{
			string? value;

			if (lane.GroupOptions.TryGetValue(option.Name ?? string.Empty, out var val))
			{
				value = val;
			}
			else
			{
				// Option names are matched without case, a stored value can differ in case from the current option name
				var stored = lane.GroupOptions.FirstOrDefault(x => string.Equals(x.Key, option.Name, StringComparison.InvariantCultureIgnoreCase));

				value = stored.Key is not null ? stored.Value : option.DefaultValue;
			}

			if (option.Type is LaneOptionType.Decoration && config is not null)
			{
				var laneIndex = config.Lanes.IndexOf(lane);
				var addGrass = config.Addons.HasFlag(laneIndex == 0 ? RoadAddons.GrassLeft : laneIndex == config.Lanes.Count - 1 ? RoadAddons.GrassRight : RoadAddons.GrassCenter);
				var addTrees = config.Addons.HasFlag(laneIndex == 0 ? RoadAddons.TreesLeft : laneIndex == config.Lanes.Count - 1 ? RoadAddons.TreesRight : RoadAddons.TreesCenter);

				value = addGrass && addTrees ? "GT" : addGrass ? "G" : addTrees ? "T" : null;
			}

			return value;
		}
	}
}
