using RoadBuilder.Domain.Configurations;
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
			if (!_netSectionsSystem.LaneGroups.TryGetValue(lane.GroupPrefabName, out var group))
			{
				yield break;
			}

			var index = 0;

			foreach (var option in group.Options)
			{
				var entries = new OptionItemUIEntry[option.IsValue ? 1 : option.Options.Length];

				if (option.IsValue)
				{
					entries[0] = new()
					{
						IsValue = option.IsValue,
						Value = lane.GroupOptions.TryGetValue(option.Name, out var val) ? val : option.DefaultValue,
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
							Selected = option.Options[i].Value == (lane.GroupOptions.TryGetValue(option.Name, out var val) ? val : option.DefaultValue)
						};
					}
				}

				yield return new OptionSectionUIEntry
				{
					Id = --index,
					Name = option.Name,
					Options = entries
				};
			}
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
						Icon = "coui://uil/Standard/ArrowDown.svg",
						Selected = lane.Invert,
						Id = 0,
					},
					new()
					{
						Name = "Forward",
						Icon = "coui://uil/Standard/ArrowUp.svg",
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

			if (!option.IsValue)
			{
				lane.GroupOptions[option.Name] = option.Options[id].Value;

				return;
			}

			var currentValue = lane.GroupOptions.TryGetValue(option.Name, out var val) ? val : option.DefaultValue;
			var currentOption = option.Options.FirstOrDefault(x => x.Value == currentValue);

			if (value > 0)
			{
				lane.GroupOptions[option.Name] = option.Options.Next(currentOption)?.Value ?? currentValue;
			}
			else
			{
				lane.GroupOptions[option.Name] = option.Options.Previous(currentOption)?.Value ?? currentValue;
			}
		}
	}
}
