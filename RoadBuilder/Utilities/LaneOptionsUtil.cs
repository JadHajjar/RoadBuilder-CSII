using Game.Prefabs;

using RoadBuilder.Domain;
using RoadBuilder.Domain.Components.Prefabs;
using RoadBuilder.Domain.Configurations;
using RoadBuilder.Domain.Enums;
using RoadBuilder.Domain.Prefabs;
using RoadBuilder.Domain.UI;
using RoadBuilder.Systems;

using System.Collections.Generic;
using System.Linq;

using Unity.Entities;

namespace RoadBuilder.Utilities
{
	public static class LaneOptionsUtil
	{
		private enum ActionType
		{
			Invert,
		}

		private static readonly NetSectionsSystem _netSectionsSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<NetSectionsSystem>();

		public static List<OptionSectionUIEntry> GenerateOptions(RoadGenerationData roadGenerationData, INetworkConfig config, LaneConfig lane)
		{
			var options = new List<OptionSectionUIEntry>();

			if (NetworkPrefabGenerationUtil.GetNetSection(roadGenerationData, config, lane, out var section, out var group)) // if lane supports invert
			{
				options.Add(GetInvertOption(config, lane, group?.Options?.FirstOrDefault(x => x.Type is LaneOptionType.TwoWay)));
			}

			if (!string.IsNullOrEmpty(lane.GroupPrefabName))
			{
				options.AddRange(GenerateGroupOptions(config, lane));
			}

			return options;
		}

		private static IEnumerable<OptionSectionUIEntry> GenerateGroupOptions(INetworkConfig config, LaneConfig lane)
		{
			if (!_netSectionsSystem.LaneGroups.TryGetValue(lane.GroupPrefabName ?? string.Empty, out var group))
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

				var entries = new OptionItemUIEntry[option.Type is LaneOptionType.Decoration or LaneOptionType.TwoWay ? 2 : option.Type is LaneOptionType.ValueUpDown ? 1 : option.Options.Length];
				var value = GetSelectedOptionValue(config, lane, option);

				if (option.Type is LaneOptionType.Decoration)
				{
					entries[0] = new()
					{
						Id = 1,
						Icon = "coui://roadbuildericons/RB_GrassWhite.svg",
						Selected = value is "GT" or "G",
					};
					entries[1] = new()
					{
						Id = 2,
						Icon = "coui://roadbuildericons/RB_TreeWhite.svg",
						Selected = value is "GT" or "T",
					};
				}
				else if (option.Type is LaneOptionType.ValueUpDown)
				{
					entries[0] = new()
					{
						IsValue = option.Type is LaneOptionType.ValueUpDown,
						Value = value,
					};
				}
				else
				{
					for (var i = 0; i < entries.Length; i++)
					{
						entries[i] = new OptionItemUIEntry
						{
							Id = i,
							Name = option.Options[i].Value,
							Icon = option.Options[i].ThumbnailUrl,
							Selected = option.Options[i].Value == value,
							Disabled = !remainingSections.Any(x => MatchesOptionValue(x, option, value))
						};
					}
				}

				yield return new OptionSectionUIEntry
				{
					Id = --index,
					Name = option.Name,
					Options = entries
				};

				remainingSections.RemoveAll(x => !MatchesOptionValue(x, option, value));
			}
		}

		private static bool MatchesOptionValue(NetSectionPrefab section, RoadBuilderLaneOption option, string currentValue)
		{
			var value = section.GetComponent<RoadBuilderLaneGroup>().Combination.FirstOrDefault(x => x.OptionName == option.Name)?.Value;

			if (option.Type is LaneOptionType.Decoration or LaneOptionType.TwoWay)
			{
				return currentValue is null || value is not null;
			}

			return value is not null && value == currentValue;
		}

		private static OptionSectionUIEntry GetInvertOption(INetworkConfig config, LaneConfig lane, RoadBuilderLaneOption twoWayOption)
		{
			var isTwoWaySelected = twoWayOption is not null && GetSelectedOptionValue(config, lane, twoWayOption) == "1";
			return new OptionSectionUIEntry
			{
				Id = (int)ActionType.Invert,
				Name = "Direction",
				Options = new OptionItemUIEntry[]
				{
					new()
					{
						Name = "Backward",
						Icon = "coui://roadbuildericons/RB_ArrowDown.svg",
						Selected = !isTwoWaySelected && lane.Invert,
						Id = 0,
					},
					new()
					{
						Name = "Forward",
						Icon = "coui://roadbuildericons/RB_Arrow.svg",
						Selected = !isTwoWaySelected && !lane.Invert,
						Id = 1,
					},
					new()
					{
						Name = "Two-way",
						Icon = "coui://roadbuildericons/RB_ArrowBoth.svg",
						Selected = isTwoWaySelected,
						Id = 2,
						Hidden = twoWayOption is null
					},
				}
			};
		}

		public static void OptionClicked(RoadGenerationData roadGenerationData, INetworkConfig config, LaneConfig lane, int optionId, int id, int value)
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

						if (option is not null)
						{
							lane.GroupOptions[option.Name] = id != 2 || GetSelectedOptionValue(config, lane, option) == "1" ? null : "1";
						}
					}

					break;
				default:
					break;
			}
		}

		public static void GroupOptionClicked(INetworkConfig config, LaneConfig lane, int optionId, int id, int value)
		{
			var group = _netSectionsSystem.LaneGroups[lane.GroupPrefabName];
			var option = group.Options[optionId];

			try
			{
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

				if (option.Type is not LaneOptionType.ValueUpDown)
				{
					lane.GroupOptions[option.Name] = option.Options[id].Value;

					return;
				}

				var remainingSections = group.LinkedSections.ToList();

				foreach (var item in group.Options)
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
					lane.GroupOptions[option.Name] = validOptions.Next(currentOption)?.Value ?? currentValue;
				}
				else
				{
					lane.GroupOptions[option.Name] = validOptions.Previous(currentOption)?.Value ?? currentValue;
				}
			}
			finally
			{
				FixGroupOptions(config, lane, group);
			}
		}

		private static void FixGroupOptions(INetworkConfig config, LaneConfig lane, LaneGroupPrefab group)
		{
			var remainingSections = group.LinkedSections.ToList();

			foreach (var option in group.Options)
			{
				if (remainingSections.Count == 0)
				{
					Mod.Log.WarnFormat("Lane Fix Failed: no remaining sections left at option '{0}'", option.Name);

					return;
				}

				if (option.Type is LaneOptionType.TwoWay)
				{
					continue;
				}

				var value = GetSelectedOptionValue(config, lane, option);

				if (!remainingSections.Any(x => MatchesOptionValue(x, option, value)))
				{
					lane.GroupOptions[option.Name] = value = remainingSections[0].GetComponent<RoadBuilderLaneGroup>().Combination.FirstOrDefault(x => x.OptionName == option.Name)?.Value;
				}

				remainingSections.RemoveAll(x => !MatchesOptionValue(x, option, value));
			}
		}

		public static string GetSelectedOptionValue(INetworkConfig config, LaneConfig lane, RoadBuilderLaneOption option)
		{
			var value = lane.GroupOptions.TryGetValue(option.Name ?? string.Empty, out var val) ? val : option.DefaultValue;

			if (option.Type is LaneOptionType.Decoration)
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
