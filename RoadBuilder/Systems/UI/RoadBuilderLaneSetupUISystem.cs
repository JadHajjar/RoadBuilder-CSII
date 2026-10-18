using Colossal;
using Colossal.IO.AssetDatabase;
using Colossal.UI;

using Game;
using Game.Prefabs;
using Game.Reflection;
using Game.SceneFlow;
using Game.UI.Editor;

using RoadBuilder.Domain.Components.Prefabs;
using RoadBuilder.Domain.Configurations;
using RoadBuilder.Domain.Enums;
using RoadBuilder.Domain.Prefabs;
using RoadBuilder.Domain.UI;
using RoadBuilder.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEngine;

namespace RoadBuilder.Systems.UI
{
	// Backs the asset editor panel that sets up Road Builder components on the net section or lane group selected in the inspector
	public partial class RoadBuilderLaneSetupUISystem : ExtendedUISystemBase
	{
		private static readonly PropertyInfo? _activeSubPanel = typeof(EditorPanelSystemBase).GetProperty("activeSubPanel", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		private static readonly MethodInfo? _refreshInspectorContent = typeof(InspectorPanelSystem).GetMethod("RefreshContent", BindingFlags.Instance | BindingFlags.NonPublic);
		private static readonly MethodInfo? _loadImageFromFile = typeof(UIIconField).GetMethod("OnLoadImage", BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(string), typeof(CastAccessor<string>) }, null);

#nullable disable
		private PrefabSystem prefabSystem;
		private InspectorPanelSystem inspectorPanelSystem;
		private EditorPanelUISystem editorPanelUISystem;
		private RoadBuilderNetSectionsSystem netSectionsSystem;
		private RoadBuilderUISystem roadBuilderUISystem;
		private ValueBindingHelper<LaneSetupState> State;
		private ValueBindingHelper<bool> Open;
		private ValueBindingHelper<string[]> Sections;
		private ValueBindingHelper<LaneSetupGroupSummary[]> Groups;
#nullable enable

		private object? lastSelectedObject;
		private PrefabBase? target;
		private bool stateDirty = true;
		private bool listsDirty = true;
		private bool imagePickerOpen;

		protected override void OnCreate()
		{
			base.OnCreate();

			prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
			netSectionsSystem = World.GetOrCreateSystemManaged<RoadBuilderNetSectionsSystem>();
			roadBuilderUISystem = World.GetOrCreateSystemManaged<RoadBuilderUISystem>();

			State = CreateBinding("LaneSetup.State", new LaneSetupState());
			Open = CreateBinding("LaneSetup.Open", false);
			Sections = CreateBinding("LaneSetup.Sections", new string[0]);
			Groups = CreateBinding("LaneSetup.Groups", new LaneSetupGroupSummary[0]);

			CreateBinding("LaneSetup.PieceRequirements", Enum.GetValues(typeof(NetPieceRequirements))
				.Cast<NetPieceRequirements>()
				.Distinct()
				.Select(x => new LaneSetupEnumValue { Name = x.ToString(), Value = (int)x })
				.OrderBy(x => x.Name)
				.ToArray());

			CreateTrigger<bool>("LaneSetup.SetOpen", SetOpen);
			CreateTrigger<string>("LaneSetup.SetRole", SetRole);
			CreateTrigger<string, bool>("LaneSetup.SetEnabled", SetComponentEnabled);
			CreateTrigger<string, string>("LaneSetup.SetText", SetText);
			CreateTrigger<string, bool>("LaneSetup.SetFlag", SetFlag);
			CreateTrigger<string, int>("LaneSetup.SetNumber", SetNumber);
			CreateTrigger<string, string[]>("LaneSetup.SetList", SetList);
			CreateTrigger<string, int[]>("LaneSetup.SetRequirements", SetRequirements);
			CreateTrigger<string, string>("LaneSetup.SetCombination", SetCombination);
			CreateTrigger<string>("LaneSetup.AddAggregateSection", AddAggregateSection);
			CreateTrigger<string, int>("LaneSetup.RemoveAggregateSection", RemoveAggregateSection);
			CreateTrigger<string, int, string, string>("LaneSetup.SetAggregateSection", SetAggregateSection);
			CreateTrigger("LaneSetup.AddOption", AddOption);
			CreateTrigger<int>("LaneSetup.RemoveOption", RemoveOption);
			CreateTrigger<int, int>("LaneSetup.MoveOption", MoveOption);
			CreateTrigger<int, string, string>("LaneSetup.SetOption", SetOption);
			CreateTrigger<int>("LaneSetup.AddOptionValue", AddOptionValue);
			CreateTrigger<int, int>("LaneSetup.RemoveOptionValue", RemoveOptionValue);
			CreateTrigger<int, int, string, string>("LaneSetup.SetOptionValue", SetOptionValue);
			CreateTrigger<string, int>("LaneSetup.PickImage", PickImage);
			CreateTrigger<string>("LaneSetup.SelectPrefab", SelectPrefab);
			CreateTrigger<string>("LaneSetup.CreateGroup", CreateGroup);
			CreateTrigger<string, string, string>("LaneSetup.CreateFromSection", CreateFromSection);

			netSectionsSystem.SectionsAdded += () =>
			{
				listsDirty = true;
				stateDirty = true;
			};
		}

		protected override void OnUpdate()
		{
			if (GameManager.instance.gameMode != GameMode.Editor)
			{
				if (target is not null || State.Value?.InEditor == true)
				{
					target = null;
					lastSelectedObject = null;
					State.Value = new LaneSetupState();
				}

				base.OnUpdate();
				return;
			}

			// Only looked up in the editor, the inspector is an editor system
			inspectorPanelSystem ??= World.GetExistingSystemManaged<InspectorPanelSystem>();

			if (inspectorPanelSystem is null)
			{
				base.OnUpdate();
				return;
			}

			editorPanelUISystem ??= World.GetExistingSystemManaged<EditorPanelUISystem>();

			// The inspector has no selection event, the selected object is polled instead.
			// It keeps its last selection after it closes, so it only counts while the inspector is the open panel.
			var inspectorOpen = editorPanelUISystem is null || ReferenceEquals(editorPanelUISystem.activePanel, inspectorPanelSystem);

			// The image picker may replace the inspector while it's open, the prefab being edited is kept until the pick ends
			// The picker is the inspector's sub-panel from the moment it opens, so this only clears once it has closed
			if (inspectorOpen && _activeSubPanel?.GetValue(inspectorPanelSystem) is null)
			{
				imagePickerOpen = false;
			}

			var selected = inspectorOpen ? inspectorPanelSystem.selectedObject : imagePickerOpen ? lastSelectedObject : null;

			if (!ReferenceEquals(selected, lastSelectedObject))
			{
				lastSelectedObject = selected;
				target = selected is NetSectionPrefab or LaneGroupPrefab ? (PrefabBase)selected : null;
				stateDirty = true;
			}

			if (listsDirty && Open.Value)
			{
				listsDirty = false;
				UpdateLists();
			}

			if (stateDirty)
			{
				stateDirty = false;

				try
				{
					State.Value = BuildState();
				}
				catch (Exception ex)
				{
					Mod.Log.Error(ex, "Failed to build the lane setup state");
				}
			}

			base.OnUpdate();
		}

		private void SetOpen(bool open)
		{
			Open.Value = open;

			if (open)
			{
				listsDirty = true;
				stateDirty = true;
			}
		}

		private void UpdateLists()
		{
			Sections.Value = netSectionsSystem.NetSections.Keys.OrderBy(x => x).ToArray();
			Groups.Value = netSectionsSystem.LaneGroups.Values
				.OrderBy(x => x.name)
				.Select(x => new LaneSetupGroupSummary
				{
					Name = x.name,
					DisplayName = roadBuilderUISystem.GetAssetName(x),
					BuiltIn = netSectionsSystem.IsBuiltIn(x),
					Options = ToOptions(x.Options),
				})
				.ToArray();
		}

		#region State

		private LaneSetupState BuildState()
		{
			// Without a selection the panel shows its getting started view
			if (target is null)
			{
				return new LaneSetupState { InEditor = true };
			}

			var state = new LaneSetupState
			{
				Available = true,
				InEditor = true,
				Name = target.name,
				DisplayName = roadBuilderUISystem.GetAssetName(target),
				ListIcon = target.TryGet<UIObject>(out var uiObject) ? uiObject.m_Icon : null,
				IsGroup = target is LaneGroupPrefab,
				ReadOnly = IsReadOnly(target, out var reason),
				ReadOnlyReason = reason,
				Role = GetRole(target),
				Ignore = target.Has<RoadBuilderIgnoreSection>(),
				Issues = LaneSetupValidator.Validate(target, netSectionsSystem).ToArray(),
			};

			if (target.TryGetExactly<RoadBuilderLaneInfo>(out var laneInfo))
			{
				state.LaneInfo = new LaneSetupLaneInfo
				{
					RequireAll = (int)laneInfo.RequireAll,
					RequireAny = (int)laneInfo.RequireAny,
					RequireNone = (int)laneInfo.RequireNone,
					GroundTexture = (int)laneInfo.GroundTexture,
					Color = laneInfo.LaneColor == default ? null : "#" + ColorUtility.ToHtmlStringRGBA(laneInfo.LaneColor),
					NoDirection = laneInfo.NoDirection,
					Median = laneInfo.Median,
					FrontThumbnail = laneInfo.FrontThumbnail,
					BackThumbnail = laneInfo.BackThumbnail,
					LaneThumbnails = laneInfo.LaneThumbnails ?? new string[0],
					PieceRequireAll = ToInts(laneInfo.PieceRequireAll),
					PieceRequireAny = ToInts(laneInfo.PieceRequireAny),
					PieceRequireNone = ToInts(laneInfo.PieceRequireNone),
				};
			}

			if (target.TryGetExactly<RoadBuilderEdgeLaneInfo>(out var edgeInfo))
			{
				state.Edge = new LaneSetupEdge
				{
					LeftSide = edgeInfo.LeftSidePrefab?.name,
					RightSide = edgeInfo.RightSidePrefab?.name,
					AddSidewalkStateOnNode = edgeInfo.AddSidewalkStateOnNode,
					DoNotRequireBeingOnEdge = edgeInfo.DoNotRequireBeingOnEdge,
				};
			}

			if (target.TryGetExactly<RoadBuilderLaneDecorationInfo>(out var decorationInfo))
			{
				state.Decoration = new LaneSetupDecoration
				{
					GrassThumbnail = decorationInfo.GrassThumbnail,
					TreeThumbnail = decorationInfo.TreeThumbnail,
					GrassAndTreeThumbnail = decorationInfo.GrassAndTreeThumbnail,
					LaneGrassThumbnail = decorationInfo.LaneGrassThumbnail ?? new string[0],
					LaneTreeThumbnail = decorationInfo.LaneTreeThumbnail ?? new string[0],
					LaneGrassAndTreeThumbnail = decorationInfo.LaneGrassAndTreeThumbnail ?? new string[0],
				};
			}

			if (target.TryGetExactly<RoadBuilderLaneAggregate>(out var aggregate))
			{
				state.Aggregate = new LaneSetupAggregate
				{
					Left = ToAggregateSections(aggregate.LeftSections),
					Right = ToAggregateSections(aggregate.RightSections),
				};
			}

			if (target.TryGet<RoadBuilderLaneGroup>(out var laneGroup))
			{
				state.Variant = new LaneSetupVariant
				{
					Group = laneGroup.GroupPrefab?.name,
					Combination = ToCombination(laneGroup.Combination),
				};
			}

			if (target is LaneGroupPrefab groupPrefab)
			{
				state.Group = new LaneSetupGroup
				{
					Options = ToOptions(groupPrefab.Options),
					LinkedSections = groupPrefab.LinkedSections
						.Select(x => new LaneSetupLinkedSection
						{
							Name = x.name,
							DisplayName = roadBuilderUISystem.GetAssetName(x),
							Combination = ToCombination(x.TryGet<RoadBuilderLaneGroup>(out var linkedGroup) ? linkedGroup.Combination : null),
						})
						.ToArray(),
				};
			}

			state.Preview = CreatePreview(target, state.LaneInfo);

			return state;
		}

		private RoadLaneUIBinder[]? CreatePreview(PrefabBase prefab, LaneSetupLaneInfo? laneInfo)
		{
			// Only lanes that show up in Road Builder's list can be previewed
			if (prefab is NetSectionPrefab && !prefab.Has<RoadBuilderLaneInfo>())
			{
				return null;
			}

			if (prefab is NetSectionPrefab && prefab.Has<RoadBuilderLaneGroup>())
			{
				return null;
			}

			try
			{
				return roadBuilderUISystem.CreatePreviewLanes(prefab, new RoadConfig { Category = GetPreviewCategory(prefab) });
			}
			catch (Exception ex)
			{
				Mod.Log.Warn(ex, "Failed to create the lane setup preview for " + prefab.name);

				return null;
			}
		}

		// The preview uses a road type the lane is allowed in, a lane restricted to tracks wouldn't resolve on a plain road
		private static RoadCategory GetPreviewCategory(PrefabBase prefab)
		{
			var candidates = new[] { RoadCategory.Road, RoadCategory.Highway, RoadCategory.PublicTransport, RoadCategory.Tram, RoadCategory.Train, RoadCategory.Subway, RoadCategory.Gravel, RoadCategory.Tiled, RoadCategory.Pathway, RoadCategory.Fence };

			foreach (var category in candidates)
			{
				if (prefab.MatchCategories(new RoadConfig { Category = category }))
				{
					return category;
				}
			}

			return RoadCategory.Road;
		}

		private static string GetRole(PrefabBase prefab)
		{
			if (prefab is LaneGroupPrefab)
			{
				return LaneSetupRoles.Group;
			}

			if (prefab.Has<RoadBuilderLaneGroup>())
			{
				return LaneSetupRoles.Variant;
			}

			if (prefab.Has<RoadBuilderEdgeLaneInfo>())
			{
				return LaneSetupRoles.Edge;
			}

			if (prefab.Has<RoadBuilderLaneInfo>())
			{
				return LaneSetupRoles.Lane;
			}

			return LaneSetupRoles.None;
		}

		private bool IsReadOnly(PrefabBase prefab, out string? reason)
		{
			if (netSectionsSystem.IsBuiltIn(prefab))
			{
				reason = "BuiltIn";
				return true;
			}

			if (prefab.isReadOnly)
			{
				reason = "ReadOnly";
				return true;
			}

			reason = null;
			return false;
		}

		private static LaneSetupOption[] ToOptions(RoadBuilderLaneOption[]? options)
		{
			return options?
				.Where(x => x is not null)
				.Select(x => new LaneSetupOption
				{
					Name = x.Name,
					Type = (int)x.Type,
					DefaultValue = x.DefaultValue,
					IgnoreForSimilarDuplicate = x.IgnoreForSimilarDuplicate,
					Values = x.Options?
						.Where(v => v is not null)
						.Select(v => new LaneSetupOptionValue { Value = v.Value, Thumbnail = v.ThumbnailUrl })
						.ToArray() ?? new LaneSetupOptionValue[0],
				})
				.ToArray() ?? new LaneSetupOption[0];
		}

		private static LaneSetupCombination[] ToCombination(LaneOptionCombination[]? combination)
		{
			return combination?
				.Where(x => x is not null)
				.Select(x => new LaneSetupCombination { Option = x.OptionName, Value = x.Value })
				.ToArray() ?? new LaneSetupCombination[0];
		}

		private static LaneSetupAggregateSection[] ToAggregateSections(RoadBuilderAggregateSection[]? sections)
		{
			return sections?
				.Select(x => new LaneSetupAggregateSection
				{
					Section = x?.Section?.name,
					Invert = x?.Invert ?? false,
					PieceRequireAll = ToInts(x?.PieceRequireAll),
					PieceRequireAny = ToInts(x?.PieceRequireAny),
					PieceRequireNone = ToInts(x?.PieceRequireNone),
				})
				.ToArray() ?? new LaneSetupAggregateSection[0];
		}

		private static int[] ToInts(NetPieceRequirements[]? requirements)
		{
			return requirements?.Select(x => (int)x).ToArray() ?? new int[0];
		}

		private static NetPieceRequirements[] ToRequirements(int[]? values)
		{
			return values?.Distinct().Select(x => (NetPieceRequirements)x).ToArray() ?? new NetPieceRequirements[0];
		}

		#endregion

		#region Editing

		// Every change goes through here, so read-only prefabs are never touched and Road Builder refreshes afterwards
		private void Edit(Action<PrefabBase> action)
		{
			if (target is null || IsReadOnly(target, out _))
			{
				return;
			}

			try
			{
				action(target);

				target.isDirty = true;
			}
			catch (Exception ex)
			{
				Mod.Log.Error(ex, "Failed to apply a lane setup change to " + target.name);
			}

			netSectionsSystem.RefreshCustomContent();

			RefreshInspector();

			stateDirty = true;
		}

		private void RefreshInspector()
		{
			try
			{
				if (_refreshInspectorContent is not null)
				{
					_refreshInspectorContent.Invoke(inspectorPanelSystem, null);
				}
				else if (target is not null)
				{
					inspectorPanelSystem?.SelectPrefab(target);
				}
			}
			catch (Exception ex)
			{
				Mod.Log.Warn(ex, "Failed to refresh the inspector");
			}
		}

		private static T GetOrAdd<T>(PrefabBase prefab) where T : ComponentBase
		{
			var component = prefab.TryGetExactly<T>(out var existing) ? existing : (T)prefab.AddComponent(typeof(T));

			component.active = true;

			return component;
		}

		private static void RemoveExactly<T>(PrefabBase prefab) where T : ComponentBase
		{
			if (prefab.TryGetExactly<T>(out _))
			{
				prefab.Remove(typeof(T));
			}
		}

		private void SetRole(string role)
		{
			if (target is LaneGroupPrefab)
			{
				return;
			}

			Edit(prefab =>
			{
				switch (role)
				{
					case LaneSetupRoles.None:
						RemoveExactly<RoadBuilderLaneGroup>(prefab);
						RemoveExactly<RoadBuilderEdgeLaneInfo>(prefab);
						RemoveExactly<RoadBuilderLaneInfo>(prefab);
						RemoveExactly<RoadBuilderLaneDecorationInfo>(prefab);
						RemoveExactly<RoadBuilderLaneAggregate>(prefab);
						break;

					case LaneSetupRoles.Lane:
						RemoveExactly<RoadBuilderLaneGroup>(prefab);
						RemoveExactly<RoadBuilderEdgeLaneInfo>(prefab);
						GetOrAdd<RoadBuilderLaneInfo>(prefab);
						break;

					case LaneSetupRoles.Edge:
						RemoveExactly<RoadBuilderLaneGroup>(prefab);
						GetOrAdd<RoadBuilderLaneInfo>(prefab);
						GetOrAdd<RoadBuilderEdgeLaneInfo>(prefab);
						break;

					case LaneSetupRoles.Variant:
						// Variants are listed through their group, the lane info stays optional for per-variant thumbnails and categories
						RemoveExactly<RoadBuilderEdgeLaneInfo>(prefab);
						var laneGroup = GetOrAdd<RoadBuilderLaneGroup>(prefab);
						laneGroup.Combination ??= new LaneOptionCombination[0];
						break;
				}
			});
		}

		private void SetComponentEnabled(string component, bool enabled)
		{
			Edit(prefab =>
			{
				switch (component)
				{
					case "LaneInfo":
						Toggle<RoadBuilderLaneInfo>(prefab, enabled);
						break;
					case "Edge":
						Toggle<RoadBuilderEdgeLaneInfo>(prefab, enabled);
						break;
					case "Decoration":
						Toggle<RoadBuilderLaneDecorationInfo>(prefab, enabled);
						break;
					case "Aggregate":
						Toggle<RoadBuilderLaneAggregate>(prefab, enabled);
						break;
					case "Ignore":
						if (prefab is NetSectionPrefab)
						{
							Toggle<RoadBuilderIgnoreSection>(prefab, enabled);
						}

						break;
				}
			});
		}

		private static void Toggle<T>(PrefabBase prefab, bool enabled) where T : ComponentBase
		{
			if (enabled)
			{
				GetOrAdd<T>(prefab);
			}
			else
			{
				RemoveExactly<T>(prefab);
			}
		}

		private void SetText(string field, string value)
		{
			var text = string.IsNullOrWhiteSpace(value) ? null : value.Trim();

			Edit(prefab =>
			{
				switch (field)
				{
					case "LaneInfo.FrontThumbnail":
						GetOrAdd<RoadBuilderLaneInfo>(prefab).FrontThumbnail = text;
						break;
					case "LaneInfo.BackThumbnail":
						GetOrAdd<RoadBuilderLaneInfo>(prefab).BackThumbnail = text;
						break;
					case "LaneInfo.Color":
						GetOrAdd<RoadBuilderLaneInfo>(prefab).LaneColor = text is not null && ColorUtility.TryParseHtmlString(text, out var color) ? color : default;
						break;
					case "Prefab.ListIcon":
						// The icon of the prefab itself, shown for the lane in Road Builder's lane list
						GetOrAdd<UIObject>(prefab).m_Icon = text;
						break;
					case "Edge.LeftSide":
						GetOrAdd<RoadBuilderEdgeLaneInfo>(prefab).LeftSidePrefab = FindSection(text);
						break;
					case "Edge.RightSide":
						GetOrAdd<RoadBuilderEdgeLaneInfo>(prefab).RightSidePrefab = FindSection(text);
						break;
					case "Decoration.GrassThumbnail":
						GetOrAdd<RoadBuilderLaneDecorationInfo>(prefab).GrassThumbnail = text;
						break;
					case "Decoration.TreeThumbnail":
						GetOrAdd<RoadBuilderLaneDecorationInfo>(prefab).TreeThumbnail = text;
						break;
					case "Decoration.GrassAndTreeThumbnail":
						GetOrAdd<RoadBuilderLaneDecorationInfo>(prefab).GrassAndTreeThumbnail = text;
						break;
					case "Variant.Group":
						var laneGroup = GetOrAdd<RoadBuilderLaneGroup>(prefab);
						var group = text is null ? null : netSectionsSystem.LaneGroups.TryGetValue(text, out var found) ? found : null;

						if (laneGroup.GroupPrefab != group)
						{
							laneGroup.GroupPrefab = group;
							// Option names differ between groups, a combination from the previous group would never match
							laneGroup.Combination = new LaneOptionCombination[0];
						}

						break;
				}
			});
		}

		private NetSectionPrefab? FindSection(string? name)
		{
			return name is not null && netSectionsSystem.NetSections.TryGetValue(name, out var section) ? section : null;
		}

		private void SetFlag(string field, bool value)
		{
			Edit(prefab =>
			{
				switch (field)
				{
					case "LaneInfo.NoDirection":
						GetOrAdd<RoadBuilderLaneInfo>(prefab).NoDirection = value;
						break;
					case "LaneInfo.Median":
						GetOrAdd<RoadBuilderLaneInfo>(prefab).Median = value;
						break;
					case "Edge.AddSidewalkStateOnNode":
						GetOrAdd<RoadBuilderEdgeLaneInfo>(prefab).AddSidewalkStateOnNode = value;
						break;
					case "Edge.DoNotRequireBeingOnEdge":
						GetOrAdd<RoadBuilderEdgeLaneInfo>(prefab).DoNotRequireBeingOnEdge = value;
						break;
				}
			});
		}

		private void SetNumber(string field, int value)
		{
			Edit(prefab =>
			{
				switch (field)
				{
					case "LaneInfo.RequireAll":
						GetOrAdd<RoadBuilderLaneInfo>(prefab).RequireAll = (RoadCategory)(ulong)value;
						break;
					case "LaneInfo.RequireAny":
						GetOrAdd<RoadBuilderLaneInfo>(prefab).RequireAny = (RoadCategory)(ulong)value;
						break;
					case "LaneInfo.RequireNone":
						GetOrAdd<RoadBuilderLaneInfo>(prefab).RequireNone = (RoadCategory)(ulong)value;
						break;
					case "LaneInfo.GroundTexture":
						GetOrAdd<RoadBuilderLaneInfo>(prefab).GroundTexture = (LaneGroundType)value;
						break;
				}
			});
		}

		private void SetList(string field, string[] values)
		{
			var list = values?.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToArray() ?? new string[0];

			Edit(prefab =>
			{
				switch (field)
				{
					case "LaneInfo.LaneThumbnails":
						GetOrAdd<RoadBuilderLaneInfo>(prefab).LaneThumbnails = list;
						break;
					case "Decoration.LaneGrassThumbnail":
						GetOrAdd<RoadBuilderLaneDecorationInfo>(prefab).LaneGrassThumbnail = list;
						break;
					case "Decoration.LaneTreeThumbnail":
						GetOrAdd<RoadBuilderLaneDecorationInfo>(prefab).LaneTreeThumbnail = list;
						break;
					case "Decoration.LaneGrassAndTreeThumbnail":
						GetOrAdd<RoadBuilderLaneDecorationInfo>(prefab).LaneGrassAndTreeThumbnail = list;
						break;
				}
			});
		}

		// field is "LaneInfo.PieceRequireAll|Any|None" or "Aggregate.{Left|Right}.{index}.PieceRequireAll|Any|None"
		private void SetRequirements(string field, int[] values)
		{
			var requirements = ToRequirements(values);
			var parts = field.Split('.');

			Edit(prefab =>
			{
				if (parts.Length == 2 && parts[0] == "LaneInfo")
				{
					var laneInfo = GetOrAdd<RoadBuilderLaneInfo>(prefab);

					switch (parts[1])
					{
						case "PieceRequireAll": laneInfo.PieceRequireAll = requirements; break;
						case "PieceRequireAny": laneInfo.PieceRequireAny = requirements; break;
						case "PieceRequireNone": laneInfo.PieceRequireNone = requirements; break;
					}
				}
				else if (parts.Length == 4 && parts[0] == "Aggregate" && int.TryParse(parts[2], out var index))
				{
					var section = GetAggregateSections(prefab, parts[1]).ElementAtOrDefault(index);

					if (section is null)
					{
						return;
					}

					switch (parts[3])
					{
						case "PieceRequireAll": section.PieceRequireAll = requirements; break;
						case "PieceRequireAny": section.PieceRequireAny = requirements; break;
						case "PieceRequireNone": section.PieceRequireNone = requirements; break;
					}
				}
			});
		}

		private void SetCombination(string option, string value)
		{
			Edit(prefab =>
			{
				var laneGroup = GetOrAdd<RoadBuilderLaneGroup>(prefab);
				var combination = (laneGroup.Combination ?? new LaneOptionCombination[0]).Where(x => x is not null).ToList();

				combination.RemoveAll(x => string.Equals(x.OptionName, option, StringComparison.InvariantCultureIgnoreCase));

				// An empty value removes the entry, which is also how an unchecked checkbox or a one-way variant is stored
				if (!string.IsNullOrEmpty(value))
				{
					combination.Add(new LaneOptionCombination { OptionName = option, Value = value });
				}

				laneGroup.Combination = combination.ToArray();
			});
		}

		private static RoadBuilderAggregateSection[] GetAggregateSections(PrefabBase prefab, string side)
		{
			var aggregate = GetOrAdd<RoadBuilderLaneAggregate>(prefab);

			return (side == "Left" ? aggregate.LeftSections : aggregate.RightSections) ?? new RoadBuilderAggregateSection[0];
		}

		private static void SetAggregateSections(PrefabBase prefab, string side, RoadBuilderAggregateSection[] sections)
		{
			var aggregate = GetOrAdd<RoadBuilderLaneAggregate>(prefab);

			if (side == "Left")
			{
				aggregate.LeftSections = sections;
			}
			else
			{
				aggregate.RightSections = sections;
			}
		}

		private void AddAggregateSection(string side)
		{
			Edit(prefab => SetAggregateSections(prefab, side, GetAggregateSections(prefab, side).Append(new RoadBuilderAggregateSection()).ToArray()));
		}

		private void RemoveAggregateSection(string side, int index)
		{
			Edit(prefab => SetAggregateSections(prefab, side, GetAggregateSections(prefab, side).Where((_, i) => i != index).ToArray()));
		}

		// field is "Section" or "Invert"
		private void SetAggregateSection(string side, int index, string field, string value)
		{
			Edit(prefab =>
			{
				var section = GetAggregateSections(prefab, side).ElementAtOrDefault(index);

				if (section is null)
				{
					return;
				}

				switch (field)
				{
					case "Section":
						section.Section = FindSection(value);
						break;
					case "Invert":
						section.Invert = value == "true";
						break;
				}
			});
		}

		private void EditGroup(Action<LaneGroupPrefab> action)
		{
			Edit(prefab =>
			{
				if (prefab is LaneGroupPrefab group)
				{
					action(group);
				}
			});
		}

		private void AddOption()
		{
			EditGroup(group =>
			{
				var options = (group.Options ?? new RoadBuilderLaneOption[0]).ToList();

				options.Add(new RoadBuilderLaneOption
				{
					Name = $"Option {options.Count + 1}",
					Type = LaneOptionType.SingleSelectionButtons,
					Options = new RoadBuilderLaneOptionValue[0],
				});

				group.Options = options.ToArray();
			});
		}

		private void RemoveOption(int index)
		{
			EditGroup(group => group.Options = (group.Options ?? new RoadBuilderLaneOption[0]).Where((_, i) => i != index).ToArray());
		}

		private void MoveOption(int index, int direction)
		{
			EditGroup(group =>
			{
				var options = (group.Options ?? new RoadBuilderLaneOption[0]).ToList();
				var newIndex = index + Math.Sign(direction);

				if (index < 0 || index >= options.Count || newIndex < 0 || newIndex >= options.Count)
				{
					return;
				}

				(options[index], options[newIndex]) = (options[newIndex], options[index]);

				group.Options = options.ToArray();
			});
		}

		// field is "Name", "Type", "DefaultValue" or "IgnoreForSimilarDuplicate"
		private void SetOption(int index, string field, string value)
		{
			EditGroup(group =>
			{
				var option = group.Options?.ElementAtOrDefault(index);

				if (option is null)
				{
					return;
				}

				switch (field)
				{
					case "Name":
						var previousName = option.Name;
						option.Name = string.IsNullOrWhiteSpace(value) ? option.Name : value.Trim();
						RenameCombinationOption(group, previousName, option.Name);
						break;
					case "Type":
						if (int.TryParse(value, out var type) && Enum.IsDefined(typeof(LaneOptionType), type))
						{
							option.Type = (LaneOptionType)type;
						}

						break;
					case "DefaultValue":
						option.DefaultValue = string.IsNullOrEmpty(value) ? null : value;
						break;
					case "IgnoreForSimilarDuplicate":
						option.IgnoreForSimilarDuplicate = value == "true";
						break;
				}
			});
		}

		// Linked sections reference options by name, renaming keeps them matched
		private void RenameCombinationOption(LaneGroupPrefab group, string? previousName, string? newName)
		{
			if (previousName is null || newName is null || previousName == newName)
			{
				return;
			}

			foreach (var section in group.LinkedSections)
			{
				if (IsReadOnly(section, out _) || !section.TryGet<RoadBuilderLaneGroup>(out var laneGroup) || laneGroup.Combination is null)
				{
					continue;
				}

				foreach (var item in laneGroup.Combination)
				{
					if (item is not null && string.Equals(item.OptionName, previousName, StringComparison.InvariantCultureIgnoreCase))
					{
						item.OptionName = newName;
						section.isDirty = true;
					}
				}
			}
		}

		private void AddOptionValue(int index)
		{
			EditGroup(group =>
			{
				var option = group.Options?.ElementAtOrDefault(index);

				if (option is not null)
				{
					option.Options = (option.Options ?? new RoadBuilderLaneOptionValue[0]).Append(new RoadBuilderLaneOptionValue { Value = string.Empty }).ToArray();
				}
			});
		}

		private void RemoveOptionValue(int index, int valueIndex)
		{
			EditGroup(group =>
			{
				var option = group.Options?.ElementAtOrDefault(index);

				if (option is not null)
				{
					option.Options = (option.Options ?? new RoadBuilderLaneOptionValue[0]).Where((_, i) => i != valueIndex).ToArray();
				}
			});
		}

		// field is "Value" or "Thumbnail"
		private void SetOptionValue(int index, int valueIndex, string field, string value)
		{
			EditGroup(group =>
			{
				var optionValue = group.Options?.ElementAtOrDefault(index)?.Options?.ElementAtOrDefault(valueIndex);

				if (optionValue is null)
				{
					return;
				}

				switch (field)
				{
					case "Value":
						optionValue.Value = value ?? string.Empty;
						break;
					case "Thumbnail":
						optionValue.ThumbnailUrl = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
						break;
				}
			});
		}

		private void SelectPrefab(string name)
		{
			PrefabBase? prefab = netSectionsSystem.NetSections.TryGetValue(name, out var section) ? section
				: netSectionsSystem.LaneGroups.TryGetValue(name, out var group) ? group
				: null;

			if (prefab is not null)
			{
				ShowInInspector(prefab);
			}
		}

		#endregion

		#region Creating prefabs

		// A new, empty lane group, selected in the inspector so it can be set up and saved like any other asset.
		// The name is required and must be new: lanes are saved with their group's name, so it can't be numbered automatically.
		private void CreateGroup(string name)
		{
			var groupName = name?.Trim();

			if (string.IsNullOrEmpty(groupName))
			{
				Mod.Log.Warn("A lane group needs a name");
				return;
			}

			if (LaneGroupExists(groupName!))
			{
				Mod.Log.Warn($"A lane group named '{groupName}' already exists");
				return;
			}

			try
			{
				var group = ScriptableObject.CreateInstance<LaneGroupPrefab>();

				group.name = groupName;
				group.Options = new RoadBuilderLaneOption[0];
				group.AddOrGetComponent<UIObject>().m_Icon = "coui://roadbuildericons/RB_Unknown.svg";
				group.AddOrGetComponent<RoadBuilderLaneInfo>();

				AddAndSelect(group);
			}
			catch (Exception ex)
			{
				Mod.Log.Error(ex, "Failed to create a lane group");
			}
		}

		// Copies an existing section and sets it up as a lane, an edge lane or a variant of a group.
		// The game's own and Road Builder's sections can't be edited, a copy is how a custom lane starts from them.
		private void CreateFromSection(string sourceName, string role, string groupName)
		{
			if (!netSectionsSystem.NetSections.TryGetValue(sourceName ?? string.Empty, out var source))
			{
				return;
			}

			try
			{
				var section = (NetSectionPrefab)source.Clone(GetUniqueName(nameof(NetSectionPrefab), $"{source.name} Custom"));

				switch (role)
				{
					case LaneSetupRoles.Variant when netSectionsSystem.LaneGroups.TryGetValue(groupName ?? string.Empty, out var group):
						RemoveExactly<RoadBuilderEdgeLaneInfo>(section);
						var laneGroup = GetOrAdd<RoadBuilderLaneGroup>(section);
						laneGroup.GroupPrefab = group;
						laneGroup.Combination = new LaneOptionCombination[0];
						break;

					case LaneSetupRoles.Edge:
						RemoveExactly<RoadBuilderLaneGroup>(section);
						GetOrAdd<RoadBuilderLaneInfo>(section);
						GetOrAdd<RoadBuilderEdgeLaneInfo>(section);
						break;

					default:
						// A copy of a variant would otherwise stay linked to the original's group
						RemoveExactly<RoadBuilderLaneGroup>(section);
						RemoveExactly<RoadBuilderEdgeLaneInfo>(section);
						GetOrAdd<RoadBuilderLaneInfo>(section);
						break;
				}

				AddAndSelect(section);
			}
			catch (Exception ex)
			{
				Mod.Log.Error(ex, "Failed to create a lane from " + sourceName);
			}
		}

		private void AddAndSelect(PrefabBase prefab)
		{
			prefab.isDirty = true;

			if (!prefabSystem.AddPrefab(prefab))
			{
				Mod.Log.Warn("Failed to add the new prefab " + prefab.name);
				return;
			}

			Open.Value = true;
			listsDirty = true;

			ShowInInspector(prefab);
		}

		// Selects the prefab and opens the inspector if another panel is showing, since the setup panel follows the open inspector
		private void ShowInInspector(PrefabBase prefab)
		{
			if (inspectorPanelSystem is null)
			{
				return;
			}

			inspectorPanelSystem.SelectPrefab(prefab);

			if (editorPanelUISystem is not null && !ReferenceEquals(editorPanelUISystem.activePanel, inspectorPanelSystem))
			{
				editorPanelUISystem.activePanel = inspectorPanelSystem;
			}
		}

		// Matched without case, two groups differing only in case would be confusing in configs and logs
		private bool LaneGroupExists(string name)
		{
			return prefabSystem.TryGetPrefab(new PrefabID(nameof(LaneGroupPrefab), name), out _)
				|| netSectionsSystem.LaneGroups.Keys.Any(x => string.Equals(x, name, StringComparison.InvariantCultureIgnoreCase));
		}

		private string GetUniqueName(string type, string baseName)
		{
			var name = baseName;

			for (var i = 2; prefabSystem.TryGetPrefab(new PrefabID(type, name), out _); i++)
			{
				name = $"{baseName} {i}";
			}

			return name;
		}

		#endregion

		#region Images

		// Opens the game's own image picker; field follows SetText/SetList names, index is the list position or -1 for single fields
		private void PickImage(string field, int index)
		{
			if (target is null || inspectorPanelSystem is null || IsReadOnly(target, out _))
			{
				return;
			}

			var picked = target;

			imagePickerOpen = true;

			inspectorPanelSystem.ShowThumbnailPicker(
				hash =>
				{
					if (AssetDatabase.global.TryGetAsset(hash, out IAssetData asset) && asset is AssetData assetData)
					{
						ApplyImage(picked, field, index, assetData.ToGlobalUri());
					}
				},
				path => ImportImage(path, url => ApplyImage(picked, field, index, url)));
		}

		// Imports a file the same way the inspector's icon fields do, so the image is saved with the asset
		private static void ImportImage(string path, Action<string> onImported)
		{
			if (_loadImageFromFile is null)
			{
				Mod.Log.Warn("Importing images is not available in this game version");
				return;
			}

			try
			{
				var url = string.Empty;
				var accessor = new CastAccessor<string>(new DelegateAccessor<string>(() => url, value => url = value));

				_loadImageFromFile.Invoke(new UIIconField(), new object[] { path, accessor });

				if (!string.IsNullOrEmpty(url))
				{
					onImported(url);
				}
			}
			catch (Exception ex)
			{
				Mod.Log.Error(ex, "Failed to import image " + path);
			}
		}

		private void ApplyImage(PrefabBase prefab, string field, int index, string url)
		{
			// The picker answers later, the inspector may have moved on to another prefab
			if (!ReferenceEquals(prefab, target))
			{
				return;
			}

			if (field.StartsWith("Option."))
			{
				// "Option.{index}" with index being the value's position
				if (int.TryParse(field.Substring("Option.".Length), out var optionIndex))
				{
					SetOptionValue(optionIndex, index, "Thumbnail", url);
				}

				return;
			}

			if (index < 0)
			{
				SetText(field, url);
				return;
			}

			var current = State.Value;
			var list = field switch
			{
				"LaneInfo.LaneThumbnails" => current?.LaneInfo?.LaneThumbnails,
				"Decoration.LaneGrassThumbnail" => current?.Decoration?.LaneGrassThumbnail,
				"Decoration.LaneTreeThumbnail" => current?.Decoration?.LaneTreeThumbnail,
				"Decoration.LaneGrassAndTreeThumbnail" => current?.Decoration?.LaneGrassAndTreeThumbnail,
				_ => null
			};

			var values = (list ?? new string[0]).ToList();

			if (index >= values.Count)
			{
				values.Add(url);
			}
			else
			{
				values[index] = url;
			}

			SetList(field, values.ToArray());
		}

		#endregion
	}
}
