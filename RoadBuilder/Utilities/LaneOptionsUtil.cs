using Game.Prefabs;

using RoadBuilder.Domain.Components;
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

		public static List<OptionSectionUIEntry> GenerateOptions(INetworkConfig config, LaneConfig lane)
		{
			var options = new List<OptionSectionUIEntry>();

			if (true) // if lane supports invert
			{
				options.Add(GetInvertOption(lane));
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

				var entries = new OptionItemUIEntry[option.IsDecoration ? 2 : option.IsValue ? 1 : option.Options.Length];
				var value = GetSelectedOptionValue(config, lane, option);

				if (option.IsDecoration)
				{
					entries[0] = new()
					{
						Id = 1,
						Icon = "Media/Game/Icons/Grass.svg",
						Selected = value is "GT" or "G",
					};
					entries[1] = new()
					{
						Id = 2,
						Icon = "Media/Game/Icons/Trees.svg",
						Selected = value is "GT" or "T",
					};
				}
				else if (option.IsValue)
				{
					entries[0] = new()
					{
						IsValue = option.IsValue,
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

		private static bool MatchesOptionValue(NetSectionPrefab section, RoadBuilderLaneOptionInfo option, string currentValue)
		{
			var value = section.GetComponent<RoadBuilderLaneGroupItem>().Combination.FirstOrDefault(x => x.OptionName == option.Name)?.Value;

			if (option.IsDecoration)
			{
				return currentValue is null || value is not null;
			}

			return value is not null && value == currentValue;
		}

		private static OptionSectionUIEntry GetInvertOption(LaneConfig lane)
		{
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
						Selected = lane.Invert,
						Id = 0,
					},
					new()
					{
						Name = "Forward",
						Icon = "coui://roadbuildericons/RB_Arrow.svg",
						Selected = !lane.Invert,
						Id = 1,
					},
				}
			};
		}

		public static void OptionClicked(INetworkConfig config, LaneConfig lane, int optionId, int id, int value)
		{
			if (optionId < 0)
			{
				GroupOptionClicked(config, lane, -optionId - 1, id, value);
				return;
			}

			switch (optionId)
			{
				case (int)ActionType.Invert:
					lane.Invert = id == 0;
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
				if (option.IsDecoration)
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

				if (!option.IsValue)
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

				var value = GetSelectedOptionValue(config, lane, option);

				if (!remainingSections.Any(x => MatchesOptionValue(x, option, value)))
				{
					lane.GroupOptions[option.Name] = value = remainingSections[0].GetComponent<RoadBuilderLaneGroupItem>().Combination.FirstOrDefault(x => x.OptionName == option.Name)?.Value;
				}

				remainingSections.RemoveAll(x => !MatchesOptionValue(x, option, value));
			}
		}

		private static string GetSelectedOptionValue(INetworkConfig config, LaneConfig lane, RoadBuilderLaneOptionInfo option)
		{
			var value = lane.GroupOptions.TryGetValue(option.Name ?? string.Empty, out var val) ? val : option.DefaultValue;

			if (option.IsDecoration)
			{
				var laneIndex = config.Lanes.IndexOf(lane);
				var addGrass = config.Addons.HasFlag(laneIndex == 0 ? RoadAddons.GrassLeft : laneIndex == config.Lanes.Count - 1 ? RoadAddons.GrassRight : RoadAddons.GrassCenter);
				var addTrees = config.Addons.HasFlag(laneIndex == 0 ? RoadAddons.TreesLeft : laneIndex == config.Lanes.Count - 1 ? RoadAddons.TreesRight : RoadAddons.TreesCenter);

				value = addGrass && addTrees ? "GT" : addGrass ? "G" : addTrees ? "T" : null;

				Mod.Log.Info($"Deco: {value}");
			}

			return value;
		}
	}
}
