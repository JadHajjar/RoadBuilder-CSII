import { bindValue, trigger } from "cs2/api";
import { NetSectionItem } from "domain/NetSectionItem";
import { RoadBuilderToolModeEnum } from "domain/RoadBuilderToolMode";
import { RoadLane, RoadProperties } from "domain/RoadProperties";
import mod from "mod.json";

export const allNetSections$ = bindValue<NetSectionItem[]>("RoadBuilder", "NetSections");
//todo: create a "store" for holding the net sections by their name
export const roadBuilderToolMode$ = bindValue(mod.id, "RoadBuilderToolMode", RoadBuilderToolModeEnum.None);
export const roadLanes$ = bindValue<RoadLane[]>(mod.id, "GetRoadLanes", []);
export const roadProperties$ = bindValue<RoadProperties>(mod.id, "GetRoadProperties");
export const isPaused$ = bindValue<boolean>(mod.id, "IsPaused");

// export const editPrefab = trigger.bind(null, mod.id, "ActionPopup.Edit");
// export const createFromTemplate = trigger.bind(null, mod.id, "ActionPopup.New");
// export const cancelPickerAction = trigger.bind(null, mod.id, "ActionPopup.Cancel");
export const toggleTool = trigger.bind(null, mod.id, "ToggleTool");
export const clearTool = trigger.bind(null, mod.id, "ClearTool");
export const createNewPrefab = trigger.bind(null, mod.id, "CreateNewPrefab");
export const setRoadProperties = (properties: RoadProperties) => trigger(mod.id, "SetRoadProperties", properties);
export const setRoadLanes = (lanes: RoadLane[]) => { console.log(lanes); trigger(mod.id, "SetRoadLanes", lanes);}