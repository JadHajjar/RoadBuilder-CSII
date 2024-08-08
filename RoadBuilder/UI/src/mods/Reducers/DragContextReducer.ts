import { Number2 } from "cs2/ui";
import { NetSectionItem } from "domain/NetSectionItem";
import { RoadLane } from "domain/RoadLane";
import { MouseButtons } from "mods/util";
import { Reducer } from "react";

export enum DragContextActionType {
    MouseReleased,
    MouseMove,
    SetNetSectionItem,
    SetDragRoadLane,
    UpdateFunctions
}

export type DragContextAction =
    | { type: DragContextActionType.MouseReleased, button: number }
    | { type: DragContextActionType.MouseMove, position: Number2 }
    | { type: DragContextActionType.SetNetSectionItem, item?: NetSectionItem }
    | { type: DragContextActionType.SetDragRoadLane, lane?: RoadLane, currentIndex?: number }
    ;

export interface DragContextReducerState {
    netSectionItem?: NetSectionItem;
    roadLane?: RoadLane;
    mousePosition: Number2;
    mouseReleased: boolean;
    oldIndex?: number;
}

export const defaultDragCtxReducerState: DragContextReducerState = {
    netSectionItem: undefined,
    roadLane: undefined,
    mousePosition: { x: 0, y: 0 },
    mouseReleased: false,
}

export const dragContextReducer: Reducer<DragContextReducerState, DragContextAction> = (prevState, action) => {
    switch (action.type) {
        case DragContextActionType.MouseMove: {
            return {
                ...prevState,
                mousePosition: action.position
            };
        }
        case DragContextActionType.MouseReleased: {
            let isDragging = prevState.netSectionItem != undefined || prevState.roadLane != undefined;
            if (action.button == MouseButtons.Primary && isDragging) {
                return {
                    ...prevState,
                    mouseReleased: true
                };
            } else if (action.button == MouseButtons.Secondary && isDragging) {
                return {
                    ...prevState,
                    netSectionItem: undefined,
                    roadLane: undefined,
                    oldIndex: undefined,
                    mouseReleased: false
                };
            }
            break;
        }
        case DragContextActionType.SetNetSectionItem: {
            return {
                ...prevState,
                netSectionItem: action.item,
                mouseReleased: false
            };
        }
        case DragContextActionType.SetDragRoadLane: {
            return {
                ...prevState,
                oldIndex: action.currentIndex,
                roadLane: action.lane,
                mouseReleased: false
            }
        }
        default:
            throw new Error("Unknown action type provided to dragContextReducer!");
    }
    return prevState;
}