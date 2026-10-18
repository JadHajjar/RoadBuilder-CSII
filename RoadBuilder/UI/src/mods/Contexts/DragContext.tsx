import { Number2 } from "cs2/ui";
import { NetSectionItem } from "domain/NetSectionItem";
import { RoadLane } from "domain/RoadLane";
import { LaneListItemDrag } from "mods/Components/LaneListItem/LaneListItem";
import { createContext, PropsWithChildren, useEffect, useMemo, useReducer, useRef } from "react";
import { defaultDragCtxReducerState, DragContextActionType, dragContextReducer, DragContextReducerState } from "../Reducers/DragContextReducer";
import { setIsUIDragging } from "mods/bindings";

export interface DragContextData extends DragContextReducerState {
  onNetSectionItemChange: (item?: NetSectionItem) => void;
  setRoadLane: (roadLane?: RoadLane, currentIndex?: number) => void;
  getMousePosition: () => Number2;
  dragElement?: Element | null;
}

const defaultDragContextData : DragContextData = {
  ...defaultDragCtxReducerState,
  onNetSectionItemChange: () => { },
  setRoadLane: () => { },
  getMousePosition: () => defaultDragCtxReducerState.mousePosition,
};

export const DragContext = createContext<DragContextData>(defaultDragContextData);

export const DragContextManager = ({ children }: PropsWithChildren) => {
  let [reducerState, dispatch] = useReducer(dragContextReducer, defaultDragContextData);
  let dragItemRef = useRef<HTMLDivElement>(null);
  let mousePositionRef = useRef<Number2>(defaultDragCtxReducerState.mousePosition);
  let isDragging = reducerState.netSectionItem != undefined || reducerState.roadLane != undefined;

  // The position is always tracked, but only pushed into the context while dragging,
  // otherwise every mouse move re-renders all context consumers
  let onMouseMove = (evt: MouseEvent) => {
    mousePositionRef.current = { x: evt.clientX, y: evt.clientY };

    if (isDragging) {
      dispatch({type: DragContextActionType.MouseMove, position: mousePositionRef.current});
    }
  };

  let onMouseRelease = (evt: MouseEvent) =>
    dispatch({type: DragContextActionType.MouseReleased, button: evt.button});

  // Starting a drag syncs the position, so the dragged item doesn't render at a stale spot
  let onNetSectionItemChange = (item?: NetSectionItem) => {
    dispatch({type: DragContextActionType.MouseMove, position: mousePositionRef.current});
    dispatch({type: DragContextActionType.SetNetSectionItem, item});
  };

  let setDragRoadLane = (lane?: RoadLane, currentIndex?: number) => {
    dispatch({type: DragContextActionType.MouseMove, position: mousePositionRef.current});
    dispatch({type: DragContextActionType.SetDragRoadLane, currentIndex, lane});
  };

  useEffect(() => {
    document.addEventListener("mousemove", onMouseMove);
    document.addEventListener("mouseup", onMouseRelease);

    setIsUIDragging(isDragging);
    return () => {
      document.removeEventListener("mousemove", onMouseMove);
      document.removeEventListener("mouseup", onMouseRelease);
    };
  }, [reducerState.netSectionItem, reducerState.roadLane]);

  let dragData: DragContextData = useMemo(
    () => ({
      ...reducerState,
      onNetSectionItemChange: onNetSectionItemChange,
      setRoadLane: setDragRoadLane,
      getMousePosition: () => mousePositionRef.current,
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
