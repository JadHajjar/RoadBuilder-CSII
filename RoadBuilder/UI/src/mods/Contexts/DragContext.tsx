import { Number2 } from "cs2/ui";
import { NetSectionItem } from "domain/NetSectionItem";
import { RoadLane } from "domain/RoadLane";
import { LaneListItemDrag } from "mods/Components/LaneListItem/LaneListItem";
import { MouseButtons } from "mods/util";
import { createContext, PropsWithChildren, Reducer, startTransition, useEffect, useMemo, useReducer, useRef, useState } from "react";
import { defaultDragCtxReducerState, DragContextAction, DragContextActionType, dragContextReducer, DragContextReducerState } from "../Reducers/DragContextReducer";

export interface DragContextData extends DragContextReducerState {
  onNetSectionItemChange: (item?: NetSectionItem) => void;
  setRoadLane: (roadLane?: RoadLane, currentIndex?: number) => void;
  dragElement?: Element | null;
}

const defaultDragContextData : DragContextData = {
  ...defaultDragCtxReducerState,
  onNetSectionItemChange: () => { },
  setRoadLane: () => { }
};

export const DragContext = createContext<DragContextData>(defaultDragContextData);

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
      <LaneListItemDrag ref={dragItemRef} />
      {children}
    </DragContext.Provider>
  )

}