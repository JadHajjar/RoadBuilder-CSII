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
				if (remainingSections.Count == 0)
				{
					yield break;
				}

				if (option.Type is LaneOptionType.TwoWay)
				{
					continue;
				}

				var entries = new OptionItemUIEntry[option.Type is LaneOptionType.Decoration or LaneOptionType.TwoWay ? 2 : option.Type is LaneOptionType.ValueUpDown or LaneOptionType.LaneWidth or LaneOptionType.Checkbox ? 1 : option.Options?.Length ?? 0];
				var value = GetSelectedOptionValue(config, lane, option);

				if (option.Type is LaneOptionType.Decoration)
				{
					var grassAvailable = remainingSections.Any(x => x.GetComponent<RoadBuilderLaneGroup>().Combination
						.Any(x => x.OptionName == option.Name && x.Value is not null and not "T"));
					var treeAvailable = remainingSections.Any(x => x.GetComponent<RoadBuilderLaneGroup>().Combination
						.Any(x => x.OptionName == option.Name && x.Value is not null and not "G"));

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
					var available = remainingSections.Any(x => x.GetComponent<RoadBuilderLaneGroup>().Combination.Any(x => x.OptionName == option.Name && string.IsNullOrEmpty(x.Value) != string.IsNullOrEmpty(value)));

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
						entries[i] = new OptionItemUIEntry
						{
							Id = i,
							Name = LocaleHelper.Translate($"{group.name}.Options[{option.Name}][{option.Options?[i].Value}]", option.Options?[i].Value),
							Icon = option.Options?[i].ThumbnailUrl,
							Selected = option.Options?[i].Value == value,
							Disabled = !remainingSections.Any(x => x.GetComponent<RoadBuilderLaneGroup>().Combination.Any(x => x.OptionName == option.Name && x.Value == option.Options?[i].Value))
						};
					}
				}

				yield return new OptionSectionUIEntry
				{
					Id = --index,
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

		private static bool MatchesOptionValue(NetSectionPrefab section, RoadBuilderLaneOption? option, string? currentValue)
		{
			if (option is null)
			{
				return false;
			}

			var value = section.GetComponent<RoadBuilderLaneGroup>().Combination.FirstOrDefault(x => x.OptionName == option.Name)?.Value;

			if (option.Type is LaneOptionType.Checkbox)
			{
				return string.IsNullOrEmpty(currentValue) == string.IsNullOrEmpty(value);
			}

			if (option.Type is LaneOptionType.Decoration or LaneOptionType.TwoWay)
			{
				return currentValue is null or "" || value is not null and not "";
			}

			return value is not null && value == currentValue;
		}

		private static OptionSectionUIEntry GetInvertOption(INetworkConfig config, LaneConfig lane, RoadBuilderLaneOption? twoWayOption)
		{
			var isTwoWaySelected = twoWayOption is not null && GetSelectedOptionValue(config, lane, twoWayOption) == "1";

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
						var option = group?.Options.FirstOrDefault(x => x.Type is LaneOptionType.TwoWay);

						if (option?.Name is not null)
						{
							lane.GroupOptions[option.Name] = id != 2 || GetSelectedOptionValue(config, lane, option) == "1" ? null : "1";
						}
					}

					break;
			}
		}

		public static void GroupOptionClicked(INetworkConfig config, LaneConfig lane, int optionId, int id, int value)
		{
			var group = _netSectionsSystem.LaneGroups[lane.GroupPrefabName ?? string.Empty];
			var option = group.Options?[optionId];

			try
			{
				if (option?.Type is LaneOptionType.Decoration)
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

				if (option?.Type is LaneOptionType.Checkbox)
				{
					lane.GroupOptions[option?.Name ?? string.Empty] = id == 0 ? "checked" : "";

					return;
				}

				if (option?.Type is not LaneOptionType.ValueUpDown and not LaneOptionType.LaneWidth)
				{
					lane.GroupOptions[option?.Name ?? string.Empty] = option?.Options?[id].Value;

					return;
				}

				var remainingSections = group.LinkedSections.Where(x => x.MatchCategories(config)).ToList();

				foreach (var item in group.Options ?? Enumerable.Empty<RoadBuilderLaneOption>())
				{
					if (remainingSections.Count == 0 || item == option)
					{
						break;
					}

					if (option.Type is LaneOptionType.TwoWay)
					{
						continue;
					}

					var value_ = GetSelectedOptionValue(config, lane, item);

					remainingSections.RemoveAll(x => !MatchesOptionValue(x, item, value_));
				}

				var currentValue = GetSelectedOptionValue(config, lane, option);
				var currentOption = option.Options.FirstOrDefault(x => x.Value == currentValue);
				var validOptions = option.Options.Where(x => remainingSections.Any(s => MatchesOptionValue(s, option, x.Value))).ToList();

				if (value > 0)
				{
					lane.GroupOptions[option?.Name ?? string.Empty] = validOptions.Next(currentOption)?.Value ?? currentValue;
				}
				else
				{
					lane.GroupOptions[option?.Name ?? string.Empty] = validOptions.Previous(currentOption)?.Value ?? currentValue;
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
					lane.GroupOptions[option?.Name ?? string.Empty] = value = remainingSections[0].GetComponent<RoadBuilderLaneGroup>().Combination.FirstOrDefault(x => x.OptionName == option?.Name)?.Value;
				}

				remainingSections.RemoveAll(x => !MatchesOptionValue(x, option, value));
			}
		}

		public static string? GetSelectedOptionValue(INetworkConfig? config, LaneConfig lane, RoadBuilderLaneOption option)
		{
			var value = lane.GroupOptions.TryGetValue(option.Name ?? string.Empty, out var val) ? val : option.DefaultValue;

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
