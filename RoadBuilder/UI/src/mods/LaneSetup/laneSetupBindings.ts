import { bindValue, trigger } from "cs2/api";
import { LaneSetupEnumValue, LaneSetupGroupSummary, LaneSetupRole, LaneSetupState } from "domain/LaneSetup";
import mod from "mod.json";

const emptyState: LaneSetupState = { Available: false, InEditor: false, IsGroup: false, ReadOnly: false, Role: "none", Ignore: false, Issues: [] };

export const laneSetupState$ = bindValue<LaneSetupState>(mod.id, "LaneSetup.State", emptyState);
export const laneSetupOpen$ = bindValue<boolean>(mod.id, "LaneSetup.Open", false);
export const laneSetupSections$ = bindValue<string[]>(mod.id, "LaneSetup.Sections", []);
export const laneSetupGroups$ = bindValue<LaneSetupGroupSummary[]>(mod.id, "LaneSetup.Groups", []);
export const laneSetupPieceRequirements$ = bindValue<LaneSetupEnumValue[]>(mod.id, "LaneSetup.PieceRequirements", []);

export const setLaneSetupOpen = (open: boolean) => trigger(mod.id, "LaneSetup.SetOpen", open);
export const setLaneSetupRole = (role: LaneSetupRole) => trigger(mod.id, "LaneSetup.SetRole", role);
export const setLaneSetupEnabled = (component: "LaneInfo" | "Edge" | "Decoration" | "Aggregate" | "Ignore", enabled: boolean) =>
  trigger(mod.id, "LaneSetup.SetEnabled", component, enabled);
export const setLaneSetupText = (field: string, value: string) => trigger(mod.id, "LaneSetup.SetText", field, value ?? "");
export const setLaneSetupFlag = (field: string, value: boolean) => trigger(mod.id, "LaneSetup.SetFlag", field, value);
export const setLaneSetupNumber = (field: string, value: number) => trigger(mod.id, "LaneSetup.SetNumber", field, value);
export const setLaneSetupList = (field: string, values: string[]) => trigger(mod.id, "LaneSetup.SetList", field, values);
export const setLaneSetupRequirements = (field: string, values: number[]) => trigger(mod.id, "LaneSetup.SetRequirements", field, values);
// An empty value removes the option from the variant's combination
export const setLaneSetupCombination = (option: string, value: string) => trigger(mod.id, "LaneSetup.SetCombination", option, value ?? "");
export const addLaneSetupAggregateSection = (side: "Left" | "Right") => trigger(mod.id, "LaneSetup.AddAggregateSection", side);
export const removeLaneSetupAggregateSection = (side: "Left" | "Right", index: number) => trigger(mod.id, "LaneSetup.RemoveAggregateSection", side, index);
export const setLaneSetupAggregateSection = (side: "Left" | "Right", index: number, field: "Section" | "Invert", value: string) =>
  trigger(mod.id, "LaneSetup.SetAggregateSection", side, index, field, value);
export const addLaneSetupOption = () => trigger(mod.id, "LaneSetup.AddOption");
export const removeLaneSetupOption = (index: number) => trigger(mod.id, "LaneSetup.RemoveOption", index);
export const moveLaneSetupOption = (index: number, direction: number) => trigger(mod.id, "LaneSetup.MoveOption", index, direction);
export const setLaneSetupOption = (index: number, field: "Name" | "Type" | "DefaultValue" | "IgnoreForSimilarDuplicate", value: string) =>
  trigger(mod.id, "LaneSetup.SetOption", index, field, value);
export const addLaneSetupOptionValue = (index: number) => trigger(mod.id, "LaneSetup.AddOptionValue", index);
export const removeLaneSetupOptionValue = (index: number, valueIndex: number) => trigger(mod.id, "LaneSetup.RemoveOptionValue", index, valueIndex);
export const setLaneSetupOptionValue = (index: number, valueIndex: number, field: "Value" | "Thumbnail", value: string) =>
  trigger(mod.id, "LaneSetup.SetOptionValue", index, valueIndex, field, value ?? "");
// index is the position in a list field, or -1 for a single image field
export const pickLaneSetupImage = (field: string, index: number) => trigger(mod.id, "LaneSetup.PickImage", field, index);
export const selectLaneSetupPrefab = (name: string) => trigger(mod.id, "LaneSetup.SelectPrefab", name);
// Creating new prefabs, both are selected in the inspector afterwards
export const createLaneSetupGroup = (name: string) => trigger(mod.id, "LaneSetup.CreateGroup", name ?? "");
export const createLaneSetupFromSection = (sourceName: string, role: "lane" | "edge" | "variant", groupName?: string) =>
  trigger(mod.id, "LaneSetup.CreateFromSection", sourceName, role, groupName ?? "");
