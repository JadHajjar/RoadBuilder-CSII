using Game.Prefabs;

using RoadBuilder.Domain.Components;
using RoadBuilder.Domain.Configurations;
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

		public static List<OptionSectionUIEntry> GenerateOptions(LaneConfig lane)
		{
			var options = new List<OptionSectionUIEntry>();

			if (true) // if lane supports invert
			{
				options.Add(GetInvertOption(lane));
			}

			if (!string.IsNullOrEmpty(lane.GroupPrefabName))
			{
				options.AddRange(GenerateGroupOptions(lane));
			}

			return options;
		}

		private static IEnumerable<OptionSectionUIEntry> GenerateGroupOptions(LaneConfig lane)
		{
			if (!_netSectionsSystem.LaneGroups.TryGetValue(lane.GroupPrefabName ?? string.Empty, out var group))
			{
				yield break;
			}

			var index = 0;
			var remainingSections = group.LinkedSections.ToList();

			foreach (var option in group.Options)
			{
				if (remainingSections.Count == 0)
				{
					yield break;
				}

				var entries = new OptionItemUIEntry[option.IsValue ? 1 : option.Options.Length];
				var value = lane.GroupOptions.TryGetValue(option.Name ?? string.Empty, out var val) ? val : option.DefaultValue;

				if (option.IsValue)
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

		public static void OptionClicked(LaneConfig lane, int optionId, int id, int value)
		{
			if (optionId < 0)
			{
				GroupOptionClicked(lane, -optionId - 1, id, value);
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

		public static void GroupOptionClicked(LaneConfig lane, int optionId, int id, int value)
		{
			var group = _netSectionsSystem.LaneGroups[lane.GroupPrefabName];
			var option = group.Options[optionId];

			try
			{
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

					var value_ = lane.GroupOptions.TryGetValue(item.Name ?? string.Empty, out var val_) ? val_ : option.DefaultValue;

					remainingSections.RemoveAll(x => !MatchesOptionValue(x, item, value_));
				}

				var currentValue = lane.GroupOptions.TryGetValue(option.Name ?? string.Empty, out var val) ? val : option.DefaultValue;
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
				FixGroupOptions(lane, group);
			}
		}

		private static void FixGroupOptions(LaneConfig lane, LaneGroupPrefab group)
		{
			var remainingSections = group.LinkedSections.ToList();

			foreach (var option in group.Options)
			{
				if (remainingSections.Count == 0)
				{
					Mod.Log.WarnFormat("Lane Fix Failed: no remaining sections left at option '{0}'", option.Name);

					return;
				}

				var value = lane.GroupOptions.TryGetValue(option.Name ?? string.Empty, out var val) ? val : option.DefaultValue;

				if (!remainingSections.Any(x => MatchesOptionValue(x, option, value)))
				{
					lane.GroupOptions[option.Name] = value = remainingSections[0].GetComponent<RoadBuilderLaneGroupItem>().Combination.FirstOrDefault(x => x.OptionName == option.Name)?.Value;
				}

				remainingSections.RemoveAll(x => !MatchesOptionValue(x, option, value));
			}
		}
	}
}
