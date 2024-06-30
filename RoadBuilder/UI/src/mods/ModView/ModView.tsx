import { MouseEventHandler, startTransition, useEffect, useMemo, useRef, useState, useTransition } from "react";
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
import { allNetSections$, roadBuilderToolMode$, toggleTool } from "mods/bindings";
import ActionPopup from "mods/Components/ActionPopup/ActionPopup";
import { useRem } from "cs2/utils";
import { tool } from "cs2/bindings";
import { RoadLane } from "domain/RoadProperties";
import { NetSectionsStore, NetSectionsStoreContext } from "mods/Contexts/NetSectionsStore";
import { RoadPropertiesPanel } from "mods/RoadPropertiesPanel/RoadPropertiesPanel";

export const ModView = () => {
  const roadBuilderToolMode = useValue(roadBuilderToolMode$);
  let rem = useRem();

  let [draggingItem, setDraggingItem] = useState<NetSectionItem | undefined>();
  let [draggingLane, setDraggingLane] = useState<RoadLane | undefined>();
  let [mousePosition, setMousePosition] = useState<Number2>({ x: 0, y: 0 });
  let [mouseReleased, setMouseReleased] = useState<boolean>(false);
  let [fromDragIndex, setFromDragIndex] = useState<number | undefined>(undefined);
  let [actionPopupPosition, setActionPopupPosition] = useState<Number2>({ x: 0, y: 0 });
  let [netSectionDict, setNetSectionDict] = useState<NetSectionsStore>();
  let dragItemRef = useRef<HTMLDivElement>(null);
  let allNetSections = useValue(allNetSections$);
  // let nStore = allNetSections.reduce<Record<string, NetSectionItem>>(
  //   (record: Record<string, NetSectionItem>, cVal: NetSectionItem, cIdx) => {
  //     record[cVal.PrefabName] = cVal;
  //     return record;
  //   },
  // {});
  let nStore = useMemo(() => {
    let nStore = allNetSections.reduce<Record<string, NetSectionItem>>((record: Record<string, NetSectionItem>, cVal: NetSectionItem, cIdx) => {
      record[cVal.PrefabName] = cVal;
      return record;
    }, {});
    return nStore;
  }, [allNetSections]);

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

  let onMouseClick = (evt: MouseEvent) => {};

  let onMouseDown = (evt: MouseEvent) => {};

  let onMouseRelease = (evt: MouseEvent) => {
    let isDragging = draggingItem != undefined || draggingLane != undefined;
    if (evt.button == MouseButtons.Secondary && roadBuilderToolMode == RoadBuilderToolModeEnum.Picker) {
      toggleTool();
    } else if (evt.button == MouseButtons.Primary && isDragging) {
      setMouseReleased(true);
    } else if (evt.button == MouseButtons.Secondary && isDragging) {
      setDraggingItem(undefined);
      setDragRoadLane(undefined, undefined);
    }
  };

  /*useEffect(() => {
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
  }, [roadBuilderToolMode])*/

  let setDragRoadLane = (lane?: RoadLane, currentIndex?: number) => {
    setFromDragIndex(currentIndex);
    setDraggingLane(lane);
    setMouseReleased(false);
  };

  let dragData: DragContextData = {
    onNetSectionItemChange: onNetSectionItemChange,
    mousePosition: mousePosition,
    netSectionItem: draggingItem,
    roadLane: draggingLane,
    setRoadLane: setDragRoadLane,
    mouseReleased: mouseReleased,
    dragElement: dragItemRef.current,
    oldIndex: fromDragIndex,
  };

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

  let content: JSX.Element | null = null;
  switch (roadBuilderToolMode) {
    case RoadBuilderToolModeEnum.Picker:
      content = <div className={styles.pickerHint}>Select on a Road to edit</div>;
      break;
    /*case RoadBuilderToolModeEnum.ActionSelection:  
      content = (
        <ActionPopup popupPosition={actionPopupPosition} />
      );
      break;*/
    case RoadBuilderToolModeEnum.Editing:
    case RoadBuilderToolModeEnum.EditingSingle:
      content = (
        <>
          <LaneListPanel />
          <BottomView />
          <RoadPropertiesPanel />
          <LaneListItemDrag ref={dragItemRef} />
        </>
      );
      break;
    default:
      return <></>;
  }
  return (
    <DragContext.Provider value={dragData}>
      <NetSectionsStoreContext.Provider value={nStore}>
        <div className={styles.view}>{content}</div>
      </NetSectionsStoreContext.Provider>
    </DragContext.Provider>
  );
};
