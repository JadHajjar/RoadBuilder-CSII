using Game.Prefabs;

using RoadBuilder.Domain.Components.Prefabs;
using RoadBuilder.Domain.Configurations;
using RoadBuilder.Domain.Enums;
using RoadBuilder.Domain.Prefabs;
using RoadBuilder.Domain.UI;
using RoadBuilder.Systems;

using System;
using System.Collections.Generic;
using System.Linq;

namespace RoadBuilder.Utilities
{
	// Checks a net section or lane group's Road Builder setup, shared by the asset editor panel and the load-time log
	public static class LaneSetupValidator
	{
		// Single road types plus a few common combinations, enough to tell whether category rules ever allow a lane
		private static readonly RoadCategory[] _categories = new[]
		{
			RoadCategory.Road, RoadCategory.Highway, RoadCategory.PublicTransport, RoadCategory.Train, RoadCategory.Tram, RoadCategory.Subway,
			RoadCategory.Gravel, RoadCategory.Tiled, RoadCategory.Fence, RoadCategory.Pathway,
			RoadCategory.Tiled | RoadCategory.Tram, RoadCategory.Gravel | RoadCategory.Train, RoadCategory.Highway | RoadCategory.PublicTransport,
		};

		// Logs the issues of third-party content once, so creators can find problems without opening the asset editor
		public static void LogIssues(IEnumerable<PrefabBase> prefabs, RoadBuilderNetSectionsSystem netSectionsSystem, HashSet<string> logged)
		{
			foreach (var prefab in prefabs)
			{
				if (!logged.Add(prefab.name))
				{
					continue;
				}

				foreach (var issue in Validate(prefab, netSectionsSystem))
				{
					// Logged as warnings even for errors, Mod.Log shows errors as popups and players can't fix another mod's lanes
					if (issue.Severity is LaneSetupSeverity.Error or LaneSetupSeverity.Warning)
					{
						Mod.Log.Warn($"[Custom lane] {issue.Severity}: {prefab.name}: {issue.Message}");
					}
				}
			}
		}

		public static List<LaneSetupIssue> Validate(PrefabBase prefab, RoadBuilderNetSectionsSystem netSectionsSystem)
		{
			var issues = new List<LaneSetupIssue>();

			if (prefab is LaneGroupPrefab group)
			{
				ValidateGroup(group, issues);
			}
			else if (prefab is NetSectionPrefab section)
			{
				ValidateSection(section, netSectionsSystem, issues);
			}

			if (prefab.TryGetExactly<RoadBuilderLaneInfo>(out var laneInfo))
			{
				ValidateLaneInfo(prefab, laneInfo, issues);
			}

			if (prefab.TryGetExactly<RoadBuilderLaneAggregate>(out var aggregate))
			{
				ValidateAggregate(aggregate, issues);
			}

			if (prefab.TryGetExactly<RoadBuilderEdgeLaneInfo>(out var edgeInfo))
			{
				if (edgeInfo.LeftSidePrefab is null || edgeInfo.RightSidePrefab is null)
				{
					Add(issues, LaneSetupSeverity.Info, "EdgeDefaultSide", "Edge sides without a section use the road's default side.");
				}
			}

			if (prefab.TryGetExactly<RoadBuilderLaneDecorationInfo>(out var decorationInfo))
			{
				if (!HasDecorationOption(prefab))
				{
					Add(issues, LaneSetupSeverity.Warning, "DecorationUnused", "Decoration thumbnails are only used by lane groups that have a Decoration option.");
				}

				ValidateLaneThumbnails(decorationInfo.LaneGrassThumbnail, issues);
				ValidateLaneThumbnails(decorationInfo.LaneTreeThumbnail, issues);
				ValidateLaneThumbnails(decorationInfo.LaneGrassAndTreeThumbnail, issues);
			}

			return issues;
		}

		private static void ValidateSection(NetSectionPrefab section, RoadBuilderNetSectionsSystem netSectionsSystem, List<LaneSetupIssue> issues)
		{
			var hasLaneGroup = section.TryGet<RoadBuilderLaneGroup>(out var laneGroup);

			if (!hasLaneGroup && !section.Has<RoadBuilderLaneInfo>())
			{
				Add(issues, LaneSetupSeverity.Info, "NotSetUp", "This section is not set up for Road Builder and won't appear in its lane list.");
				return;
			}

			if (!hasLaneGroup)
			{
				return;
			}

			var group = laneGroup!.GroupPrefab;

			if (group is null)
			{
				Add(issues, LaneSetupSeverity.Error, "VariantNoGroup", "Pick the lane group this section is a variant of.");
				return;
			}

			if (laneGroup.GetType() != typeof(RoadBuilderLaneGroup))
			{
				Add(issues, LaneSetupSeverity.Warning, "VariantWrongType", "Only the RoadBuilderLaneGroup component links a section to a group, other variants of it are ignored.");
			}

			if (!netSectionsSystem.LaneGroups.ContainsKey(group.name))
			{
				Add(issues, LaneSetupSeverity.Error, "VariantGroupMissing", $"The lane group '{group.name}' isn't loaded.");
			}

			var options = group.Options ?? new RoadBuilderLaneOption[0];
			var combination = laneGroup.Combination ?? new LaneOptionCombination[0];

			foreach (var item in combination)
			{
				var option = options.FirstOrDefault(x => string.Equals(x?.Name, item?.OptionName, StringComparison.InvariantCultureIgnoreCase));

				if (option is null)
				{
					Add(issues, LaneSetupSeverity.Error, "VariantUnknownOption", $"'{item?.OptionName}' is not an option of the group.");
					continue;
				}

				if (option.Type is LaneOptionType.SingleSelectionButtons or LaneOptionType.MultiSelectionButtons or LaneOptionType.Toggle or LaneOptionType.ValueUpDown or LaneOptionType.LaneWidth
					&& !(option.Options ?? new RoadBuilderLaneOptionValue[0]).Any(x => string.Equals(x?.Value ?? string.Empty, item?.Value ?? string.Empty, StringComparison.InvariantCultureIgnoreCase)))
				{
					Add(issues, LaneSetupSeverity.Error, "VariantUnknownValue", $"'{item?.Value}' is not a value of the '{option.Name}' option.");
				}
			}

			foreach (var option in options)
			{
				if (option is null || option.Type is LaneOptionType.Decoration or LaneOptionType.Checkbox or LaneOptionType.TwoWay)
				{
					continue;
				}

				if (!combination.Any(x => string.Equals(x?.OptionName, option.Name, StringComparison.InvariantCultureIgnoreCase)))
				{
					Add(issues, LaneSetupSeverity.Warning, "VariantMissingValue", $"No value is set for the '{option.Name}' option, the variant can only be picked when that option is empty.");
				}
			}

			var duplicate = group.LinkedSections.FirstOrDefault(x => x != section && x.TryGet<RoadBuilderLaneGroup>(out var other) && SameCombination(other.Combination, combination));

			if (duplicate is not null)
			{
				Add(issues, LaneSetupSeverity.Warning, "VariantDuplicate", $"'{duplicate.name}' has the same option values, only one of them can ever be picked.");
			}
		}

		private static void ValidateGroup(LaneGroupPrefab group, List<LaneSetupIssue> issues)
		{
			var options = group.Options ?? new RoadBuilderLaneOption[0];

			if (options.Length == 0)
			{
				Add(issues, LaneSetupSeverity.Warning, "GroupNoOptions", "The group has no options, only its first variant can ever be used.");
			}

			foreach (var option in options)
			{
				if (option is null)
				{
					continue;
				}

				if (string.IsNullOrWhiteSpace(option.Name))
				{
					Add(issues, LaneSetupSeverity.Error, "OptionNoName", "Every option needs a name, variants match options by name.");
				}

				if (options.Count(x => string.Equals(x?.Name, option.Name, StringComparison.InvariantCultureIgnoreCase)) > 1)
				{
					Add(issues, LaneSetupSeverity.Error, "OptionDuplicateName", $"The option name '{option.Name}' is used more than once.");
				}

				var values = option.Options ?? new RoadBuilderLaneOptionValue[0];

				if (option.Type is LaneOptionType.SingleSelectionButtons or LaneOptionType.MultiSelectionButtons or LaneOptionType.Toggle or LaneOptionType.ValueUpDown or LaneOptionType.LaneWidth)
				{
					if (values.Length == 0)
					{
						Add(issues, LaneSetupSeverity.Error, "OptionNoValues", $"The '{option.Name}' option has no values.");
					}
					else if (!values.Any(x => string.Equals(x?.Value ?? string.Empty, option.DefaultValue ?? string.Empty, StringComparison.InvariantCultureIgnoreCase)))
					{
						Add(issues, LaneSetupSeverity.Warning, "OptionBadDefault", $"The default of the '{option.Name}' option is not one of its values.");
					}
				}
			}

			if (group.LinkedSections.Count == 0)
			{
				Add(issues, LaneSetupSeverity.Warning, "GroupNoSections", "No section is a variant of this group yet, it won't appear in Road Builder.");
			}
			else if (!_categories.Any(category => group.MatchCategories(new RoadConfig { Category = category }) && group.LinkedSections.Any(x => x.MatchCategories(new RoadConfig { Category = category }))))
			{
				Add(issues, LaneSetupSeverity.Error, "GroupNoAllowedVariant", "No variant is allowed in any road type the group allows, the group never appears in Road Builder.");
			}
		}

		private static void ValidateLaneInfo(PrefabBase prefab, RoadBuilderLaneInfo laneInfo, List<LaneSetupIssue> issues)
		{
			var isVariant = prefab.Has<RoadBuilderLaneGroup>();

			if (!isVariant && string.IsNullOrEmpty(laneInfo.FrontThumbnail) && string.IsNullOrEmpty(laneInfo.BackThumbnail))
			{
				Add(issues, LaneSetupSeverity.Warning, "NoThumbnail", "No lane icon is set, the lane bar falls back to the prefab's own icon.");
			}

			if (!isVariant && (laneInfo.LaneThumbnails?.Length ?? 0) == 0)
			{
				Add(issues, LaneSetupSeverity.Info, "NoLaneThumbnails", "No road thumbnail images are set, generated road thumbnails use a generic car lane.");
			}

			ValidateLaneThumbnails(laneInfo.LaneThumbnails, issues);

			if ((laneInfo.RequireAll & laneInfo.RequireNone) != 0)
			{
				Add(issues, LaneSetupSeverity.Error, "CategoryConflict", "A road type is both required and excluded, the lane can never be used.");
			}
			else if (laneInfo.RequireAny != 0 && (laneInfo.RequireAny & ~laneInfo.RequireNone) == 0)
			{
				Add(issues, LaneSetupSeverity.Error, "CategoryConflict", "Every allowed road type is also excluded, the lane can never be used.");
			}
		}

		private static void ValidateLaneThumbnails(string[]? thumbnails, List<LaneSetupIssue> issues)
		{
			foreach (var thumbnail in thumbnails ?? new string[0])
			{
				if (!string.IsNullOrEmpty(thumbnail) && !ThumbnailGenerationUtil.IsValidLaneThumbnail(thumbnail))
				{
					Add(issues, LaneSetupSeverity.Warning, "BadLaneThumbnail", $"'{thumbnail}' can't be used in road thumbnails, it must be an SVG with rectangles named Position and Extents.");
				}
			}
		}

		private static void ValidateAggregate(RoadBuilderLaneAggregate aggregate, List<LaneSetupIssue> issues)
		{
			var sections = (aggregate.LeftSections ?? new RoadBuilderAggregateSection[0]).Concat(aggregate.RightSections ?? new RoadBuilderAggregateSection[0]);

			if (sections.Any(x => x?.Section is null))
			{
				Add(issues, LaneSetupSeverity.Error, "AggregateEmpty", "An extra section has no section picked, it is skipped when generating roads.");
			}
		}

		private static bool HasDecorationOption(PrefabBase prefab)
		{
			if (prefab is LaneGroupPrefab group)
			{
				return group.Options?.Any(x => x?.Type is LaneOptionType.Decoration) ?? false;
			}

			return prefab.TryGet<RoadBuilderLaneGroup>(out var laneGroup) && (laneGroup.GroupPrefab?.Options?.Any(x => x?.Type is LaneOptionType.Decoration) ?? false);
		}

		private static bool SameCombination(LaneOptionCombination[]? a, LaneOptionCombination[]? b)
		{
			static Dictionary<string, string> ToDictionary(LaneOptionCombination[]? combination)
			{
				var dictionary = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

				foreach (var item in combination ?? new LaneOptionCombination[0])
				{
					if (item?.OptionName is not null)
					{
						dictionary[item.OptionName] = item.Value?.ToLowerInvariant() ?? string.Empty;
					}
				}

				return dictionary;
			}

			var first = ToDictionary(a);
			var second = ToDictionary(b);

			return first.Count == second.Count && first.All(x => second.TryGetValue(x.Key, out var value) && value == x.Value);
		}

		private static void Add(List<LaneSetupIssue> issues, string severity, string key, string fallback)
		{
			if (issues.Any(x => x.Message == fallback))
			{
				return;
			}

			issues.Add(new LaneSetupIssue { Severity = severity, Key = key, Message = fallback });
		}
	}
}
