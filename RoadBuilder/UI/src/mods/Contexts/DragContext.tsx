import { Number2 } from "cs2/ui";
import { NetSectionItem } from "domain/NetSectionItem";
import { RoadLane } from "domain/RoadLane";
import { LaneListItemDrag } from "mods/Components/LaneListItem/LaneListItem";
import { MouseButtons } from "mods/util";
import { createContext, PropsWithChildren, startTransition, useEffect, useMemo, useRef, useState } from "react";

export interface DragContextData {
  netSectionItem?: NetSectionItem;
  roadLane?: RoadLane;
  mousePosition: Number2;
  onNetSectionItemChange: (item?: NetSectionItem) => void;
  setRoadLane: (roadLane?: RoadLane, currentIndex?: number) => void;
  mouseReleased: boolean;
  dragElement?: Element | null;
  oldIndex?: number;
}

export const DragContext = createContext<DragContextData>({
  netSectionItem: undefined,
  roadLane: undefined,
  mousePosition: { x: 0, y: 0 },
  onNetSectionItemChange: () => { },
  setRoadLane: () => { },
  mouseReleased: false,
});

export const DragContextManager = ({ children }: PropsWithChildren) => {
  let [draggingItem, setDraggingItem] = useState<NetSectionItem | undefined>();
  let [draggingLane, setDraggingLane] = useState<RoadLane | undefined>();
  let [mousePosition, setMousePosition] = useState<Number2>({ x: 0, y: 0 });
  let [mouseReleased, setMouseReleased] = useState<boolean>(false);
  let [fromDragIndex, setFromDragIndex] = useState<number | undefined>(undefined);
  let dragItemRef = useRef<HTMLDivElement>(null);

  let onMouseMove = (evt: MouseEvent) => {
    if (draggingItem == undefined) {
      startTransition(() => {
        setMousePosition({ x: evt.clientX, y: evt.clientY });
      });
    } else {
      setMousePosition({ x: evt.clientX, y: evt.clientY });
    }
  };

  let onMouseClick = (evt: MouseEvent) => { };

  let onMouseDown = (evt: MouseEvent) => { };

  let onMouseRelease = (evt: MouseEvent) => {
    let isDragging = draggingItem != undefined || draggingLane != undefined;
    if (evt.button == MouseButtons.Primary && isDragging) {
      setMouseReleased(true);
    } else if (evt.button == MouseButtons.Secondary && isDragging) {
      setDraggingItem(undefined);
      setDragRoadLane(undefined, undefined);
    }
  };

  let onNetSectionItemChange = (item?: NetSectionItem) => {
    setDraggingItem(item);
    setMouseReleased(false);
  };

  let setDragRoadLane = (lane?: RoadLane, currentIndex?: number) => {
    setFromDragIndex(currentIndex);
    setDraggingLane(lane);
    setMouseReleased(false);
  };

  let dragData: DragContextData = useMemo(
    () => ({
      onNetSectionItemChange: onNetSectionItemChange,
      mousePosition: mousePosition,
      netSectionItem: draggingItem,
      roadLane: draggingLane,
      setRoadLane: setDragRoadLane,
      mouseReleased: mouseReleased,
      dragElement: dragItemRef.current,
      oldIndex: fromDragIndex,
    }),
    [mousePosition, draggingItem, draggingLane, mouseReleased, dragItemRef.current, fromDragIndex]
  );

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
  }, [draggingItem, draggingLane]);

  return (
    <DragContext.Provider value={dragData}>
      <span style={{position: 'absolute', left: '20rem', top: '50%'}}>{mousePosition.x}, {mousePosition.y}</span>
      <LaneListItemDrag ref={dragItemRef} />
      {children}
    </DragContext.Provider>
  )

}