import { MouseEventHandler, startTransition, useEffect, useRef, useState, useTransition } from "react";
import { BottomView } from "../BottomView/BottomView";
import { LaneListPanel } from "../LaneListPanel/LaneListPanel";
import { DragContext, DragContextData } from "mods/Contexts/DragContext";
import { NetSectionItem } from "domain/NetSectionItem";
import { Number2 } from "cs2/ui";
import { LaneListItemDrag } from "../Components/LaneListItem/LaneListItem";

import styles from "./ModView.module.scss";
import { MouseButtons } from "mods/util";
import { bindValue, useValue } from "cs2/api";
import { RoadBuilderToolModeEnum } from "domain/RoadBuilderToolMode";
import { roadBuilderToolMode$ } from "mods/bindings";
import ActionPopup from "mods/Components/ActionPopup/ActionPopup";

export const ModView = () => {
  const roadBuilderToolMode = useValue(roadBuilderToolMode$);

  let [draggingItem, setDraggingItem] = useState<NetSectionItem | undefined>();
  let [mousePosition, setMousePosition] = useState<Number2>({ x: 0, y: 0 });
  let [mouseReleased, setMouseReleased] = useState<boolean>(false);
  let [actionPopupPosition, setActionPopupPosition] = useState<Number2>({x: 0, y: 0});
  let dragItemRef = useRef<HTMLDivElement>(null);

  let onNetSectionItemChange = (item?: NetSectionItem) => {
    setDraggingItem(item);
    setMouseReleased(false);
  };

  let onMouseMove = (evt: MouseEvent) => {
    if (draggingItem == undefined) {
      startTransition(() => {
        setMousePosition({ x: evt.clientX, y: evt.clientY });
      });
    } else {
      setMousePosition({ x: evt.clientX, y: evt.clientY });
    }
  };

  let onMouseDown = (evt: MouseEvent) => {};

  let onMouseRelease = (evt: MouseEvent) => {
    if (evt.button == MouseButtons.Primary && draggingItem) {
      //TODO: send message to bottom view that a new lane has been added
      setMouseReleased(true);
    }
    if (evt.button == MouseButtons.Secondary) {
      setDraggingItem(undefined);
    }
  };

  useEffect(() => {
    switch(roadBuilderToolMode) {
        case RoadBuilderToolModeEnum.ActionSelection:
          setActionPopupPosition(mousePosition);
          break;        
        default:          
          break;
    }
  }, [roadBuilderToolMode])

  let dragData: DragContextData = {
    onNetSectionItemChange: onNetSectionItemChange,
    mousePosition: mousePosition,
    netSectionItem: draggingItem,
    mouseReleased: mouseReleased,
    dragElement: dragItemRef.current,
  };

  useEffect(() => {
    document.addEventListener("mousemove", onMouseMove);
    document.addEventListener("mousedown", onMouseDown);
    document.addEventListener("mouseup", onMouseRelease);
    console.log("Setup event listener");
    return () => {
      document.removeEventListener("mousemove", onMouseMove);
      document.removeEventListener("mousedown", onMouseDown);
      document.removeEventListener("mouseup", onMouseRelease);
    };
  }, [draggingItem]);
  
  let content : JSX.Element | null = null;
  switch (roadBuilderToolMode) {
    case RoadBuilderToolModeEnum.ActionSelection:  
      content = (
        <ActionPopup popupPosition={actionPopupPosition} />
      );
      break;
    case RoadBuilderToolModeEnum.Editing:
      content = (
        <>
        <LaneListPanel />
        <BottomView />
        <LaneListItemDrag ref={dragItemRef} />
        </>
      )
      break;
    default:
      return (<></>);      
  }
  return (
    <DragContext.Provider value={dragData}>
      <div className={styles.view}>
        {content}              
      </div>
    </DragContext.Provider>
  );
};
