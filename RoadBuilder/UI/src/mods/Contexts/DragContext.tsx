import { Number2 } from "cs2/ui";
import { NetSectionItem } from "domain/NetSectionItem";
import { RoadLane } from "domain/RoadLane";
import { LaneListItemDrag } from "mods/Components/LaneListItem/LaneListItem";
import { MouseButtons } from "mods/util";
import { createContext, PropsWithChildren, Reducer, startTransition, useEffect, useMemo, useReducer, useRef, useState } from "react";

interface DragContextReducerState {
  netSectionItem?: NetSectionItem;
  roadLane?: RoadLane;
  mousePosition: Number2;
  mouseReleased: boolean;
  oldIndex?: number;
}

export interface DragContextData extends DragContextReducerState {
  onNetSectionItemChange: (item?: NetSectionItem) => void;
  setRoadLane: (roadLane?: RoadLane, currentIndex?: number) => void;
  dragElement?: Element | null;
}

const defaultDragCtxReducerState : DragContextReducerState = {
  netSectionItem: undefined,
  roadLane: undefined,
  mousePosition: { x: 0, y: 0 },
  mouseReleased: false,
}

const defaultDragContextData : DragContextData = {
  ...defaultDragCtxReducerState,
  onNetSectionItemChange: () => { },
  setRoadLane: () => { }
};

export const DragContext = createContext<DragContextData>(defaultDragContextData);

enum DragContextActionType {
  MouseReleased,
  MouseMove,
  SetNetSectionItem,
  SetDragRoadLane,
  UpdateFunctions
}

type DragContextAction = 
  | {type: DragContextActionType.MouseReleased, button: number}
  | {type: DragContextActionType.MouseMove, position: Number2}
  | {type: DragContextActionType.SetNetSectionItem, item?: NetSectionItem}
  | {type: DragContextActionType.SetDragRoadLane, lane?: RoadLane, currentIndex?: number}
;

let dragContextReducer : Reducer<DragContextReducerState, DragContextAction>= (prevState, action) => {
  switch(action.type) {
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

export const DragContextManager = ({ children }: PropsWithChildren) => {
  let [reducerState, dispatch] = useReducer(dragContextReducer, defaultDragContextData);
  let dragItemRef = useRef<HTMLDivElement>(null);

  let onMouseMove = (evt: MouseEvent) => {
    let payload : DragContextAction = {type: DragContextActionType.MouseMove, position: { x: evt.clientX, y: evt.clientY }};
    if (reducerState.netSectionItem == undefined || reducerState.roadLane == undefined) {
      startTransition(() => {
        dispatch(payload);
      });
    } else {
      dispatch(payload);
    }
  };

  let onMouseClick = (evt: MouseEvent) => { };

  let onMouseDown = (evt: MouseEvent) => { };

  let onMouseRelease = (evt: MouseEvent) =>
    dispatch({type: DragContextActionType.MouseReleased, button: evt.button});

  let onNetSectionItemChange = (item?: NetSectionItem) =>
    dispatch({type: DragContextActionType.SetNetSectionItem, item});  

  let setDragRoadLane = (lane?: RoadLane, currentIndex?: number) =>
    dispatch({type: DragContextActionType.SetDragRoadLane, currentIndex, lane});        

  useEffect(() => {
    document.addEventListener("mousemove", onMouseMove);
    document.addEventListener("mousedown", onMouseDown);
    document.addEventListener("mouseup", onMouseRelease);
    document.addEventListener("click", onMouseClick);
    console.log("Setup event listener");
    return () => {
      document.removeEventListener("mousemove", onMouseMove);
      document.removeEventListener("mousedown", onMouseDown);
      document.removeEventListener("mouseup", onMouseRelease);
      document.removeEventListener("click", onMouseClick);
    };
  }, [reducerState.netSectionItem, reducerState.roadLane]);
  
  let dragData: DragContextData = useMemo(
    () => ({
      ...reducerState,
      onNetSectionItemChange: onNetSectionItemChange,
      setRoadLane: setDragRoadLane,
      dragElement: dragItemRef.current,
    }),
    [reducerState, dragItemRef.current]
  );

  return (
    <DragContext.Provider value={dragData}>
      <span style={{position: 'absolute', left: '20rem', top: '50%'}}>{dragData.mousePosition.x}, {dragData.mousePosition.y}</span>
      <LaneListItemDrag ref={dragItemRef} />
      {children}
    </DragContext.Provider>
  )

}