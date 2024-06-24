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
import { useRem } from "cs2/utils";

export const ModView = () => {
  const roadBuilderToolMode = useValue(roadBuilderToolMode$);
  let rem = useRem();

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
          let halfPopupWidth = rem * 500/2;
          let halfPopupHeight = rem * 120/2;
          let bodySize = document.body.getBoundingClientRect();
          let deltaRight = bodySize.width - (mousePosition.x + halfPopupWidth);
          let deltaLeft = mousePosition.x - halfPopupWidth;
          let deltaTop = mousePosition.y - halfPopupHeight;
          let deltaBottom = bodySize.height - (mousePosition.y + halfPopupHeight);          
          let nPos = mousePosition;          
          if (deltaRight <  0 || deltaLeft < 0) {
            nPos = {...nPos, x: nPos.x + Math.min(deltaRight, deltaLeft < 0? -deltaLeft : 0)}
          }          
          if (deltaBottom < 0 || deltaTop < 0) {
            nPos = {...nPos, y: nPos.y + Math.min(deltaBottom, deltaTop < 0? -deltaTop : 0)}
          }
          setActionPopupPosition(nPos);
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
    case RoadBuilderToolModeEnum.Picker:
      content = (
        <div className={styles.pickerHint}>Click on a Road</div>
      )
      break;
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
