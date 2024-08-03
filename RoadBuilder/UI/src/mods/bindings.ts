import { bindValue, trigger } from "cs2/api";
import { NetSectionItem } from "domain/NetSectionItem";
import { OptionSection } from "domain/Options";
import { RoadBuilderToolModeEnum } from "domain/RoadBuilderToolMode";
import { RoadConfiguration } from "domain/RoadConfiguration";
import { RoadLane } from "domain/RoadLane";
import mod from "mod.json";

export const allNetSections$ = bindValue<NetSectionItem[]>(mod.id, "NetSections");
export const allRoadConfigurations$ = bindValue<RoadConfiguration[]>(mod.id, "GetRoadConfigurations");
//todo: create a "store" for holding the net sections by their name
export const roadBuilderToolMode$ = bindValue(mod.id, "RoadBuilderToolMode", RoadBuilderToolModeEnum.None);
export const roadLanes$ = bindValue<RoadLane[]>(mod.id, "GetRoadLanes", []);
export const roadOptions$ = bindValue<OptionSection[]>(mod.id, "GetRoadOptions");
export const getRoadName$ = bindValue<string>(mod.id, "GetRoadName");
export const getRoadId$ = bindValue<string>(mod.id, "GetRoadId");
export const isPaused$ = bindValue<boolean>(mod.id, "IsPaused");
export const roadListView$ = bindValue<boolean>(mod.id, "RoadListView");
export const IsCustomRoadSelected$ = bindValue<boolean>(mod.id, "IsCustomRoadSelected", false);

export const toggleTool = trigger.bind(null, mod.id, "ToggleTool");
export const clearTool = trigger.bind(null, mod.id, "ClearTool");
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
export const setRoadLanes = (lanes: RoadLane[]) => {
  trigger(mod.id, "SetRoadLanes", lanes);
};
export const laneOptionClicked = (optionIndex: number, netSectionId: number, optionId: number, value: number) =>
  trigger(mod.id, "OptionClicked", optionIndex, netSectionId, optionId, value);
export const roadOptionClicked = (netSectionId: number, optionId: number, value: number) =>
  trigger(mod.id, "RoadOptionClicked", netSectionId, optionId, value);
