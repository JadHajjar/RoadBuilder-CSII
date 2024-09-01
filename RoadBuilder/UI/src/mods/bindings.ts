import { bindValue, trigger } from "cs2/api";
import { LaneSectionType } from "domain/LaneSectionType";
import { NetSectionGroup } from "domain/NetSectionGroup";
import { NetSectionItem } from "domain/NetSectionItem";
import { OptionSection } from "domain/Options";
import { RoadBuilderToolModeEnum } from "domain/RoadBuilderToolMode";
import { RoadConfiguration } from "domain/RoadConfiguration";
import { RoadLane } from "domain/RoadLane";
import mod from "mod.json";

export const allNetSections$ = bindValue<NetSectionGroup[]>(mod.id, "NetSections");
export const allRoadConfigurations$ = bindValue<RoadConfiguration[]>(mod.id, "GetRoadConfigurations");
export const roadBuilderToolMode$ = bindValue(mod.id, "RoadBuilderToolMode", RoadBuilderToolModeEnum.None);
export const roadLanes$ = bindValue<RoadLane[]>(mod.id, "GetRoadLanes", []);
export const roadOptions$ = bindValue<OptionSection[]>(mod.id, "GetRoadOptions");
export const getRoadName$ = bindValue<string>(mod.id, "GetRoadName");
export const getRoadSize$ = bindValue<string>(mod.id, "GetRoadSize");
export const getRoadTypeName$ = bindValue<string>(mod.id, "GetRoadTypeName");
export const getRoadId$ = bindValue<string>(mod.id, "GetRoadId");
export const isPaused$ = bindValue<boolean>(mod.id, "IsPaused");
export const fpsMeterLevel$ = bindValue<number>(mod.id, "FpsMeterLevel");
export const roadListView$ = bindValue<boolean>(mod.id, "RoadListView");
export const IsCustomRoadSelected$ = bindValue<boolean>(mod.id, "IsCustomRoadSelected", false);
export const DiscoverLoading$ = bindValue<boolean>(mod.id, "Discover.Loading", true);
export const DiscoverCurrentPage$ = bindValue<number>(mod.id, "Discover.CurrentPage", 1);
export const DiscoverMaxPages$ = bindValue<number>(mod.id, "Discover.MaxPages", 1);
export const DiscoverItems$ = bindValue<RoadConfiguration[]>(mod.id, "Discover.Items");

export const toggleTool = trigger.bind(null, mod.id, "ToggleTool");
export const clearTool = trigger.bind(null, mod.id, "ClearTool");
export const manageRoads = trigger.bind(null, mod.id, "ManageRoads");
export const createNewPrefab = trigger.bind(null, mod.id, "CreateNewPrefab"); // create a new prefab from the selected one
export const pickPrefab = trigger.bind(null, mod.id, "PickPrefab"); // create a new prefab from the selected one
export const editPrefab = trigger.bind(null, mod.id, "EditPrefab"); // edit the selected prefab
export const cancelActionPopup = trigger.bind(null, mod.id, "CancelActionPopup");
export const duplicateLane = (index: number) => trigger(mod.id, "DuplicateLane", index);
export const setRoadName = (name: string) => trigger(mod.id, "SetRoadName", name);
export const setRoadListView = (active: boolean) => trigger(mod.id, "SetRoadListView", active);
export const activateRoad = (id: string) => trigger(mod.id, "ActivateRoad", id);
export const editRoad = (id: string) => trigger(mod.id, "EditRoad", id);
export const findRoad = (id: string) => trigger(mod.id, "FindRoad", id);
export const deleteRoad = (id: string) => trigger(mod.id, "DeleteRoad", id);
export const setIsUIDragging = (isDragging: boolean) => trigger(mod.id, "SetDragging", isDragging);
export const setSearchBinder = (q: string) => trigger(mod.id, "SetSearchQuery", q);
export const setRoadLanes = (lanes: RoadLane[]) => {
  trigger(
    mod.id,
    "SetRoadLanes",
    lanes.filter((x) => !x.IsEdgePlaceholder)
  );
};
export const laneOptionClicked = (optionIndex: number, netSectionId: number, optionId: number, value: number) =>
  trigger(mod.id, "OptionClicked", optionIndex, netSectionId, optionId, value);
export const roadOptionClicked = (netSectionId: number, optionId: number, value: number) =>
  trigger(mod.id, "RoadOptionClicked", netSectionId, optionId, value);
export const setDiscoverPage = (p: number) => trigger(mod.id, "Discover.SetPage", p);
export const setManagementSearchBinder = (q: string) => trigger(mod.id, "Management.SetSearchQuery", q);
export const setManagementSetCategory = (s: number) => trigger(mod.id, "Management.SetCategory", s);
export const setDiscoverSorting = (s: number) => trigger(mod.id, "Discover.SetSorting", s);
export const downloadConfig = (id: string) => trigger(mod.id, "Discover.Download", id);
