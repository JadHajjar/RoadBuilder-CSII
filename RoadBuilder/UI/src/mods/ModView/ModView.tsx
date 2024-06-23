import { MouseEventHandler, startTransition, useEffect, useRef, useState, useTransition } from "react";
import { BottomView } from "../BottomView/BottomView";
import { LaneListPanel } from "../LaneListPanel/LaneListPanel";
import { DragContext, DragContextData } from "mods/Contexts/DragContext";
import { NetSectionItem } from "domain/NetSectionItem";
import { Number2 } from "cs2/ui";
import mod from "mod.json";
import { LaneListItemDrag } from "../Components/LaneListItem/LaneListItem";

import styles from "./ModView.module.scss";
import { MouseButtons } from "mods/util";
import { bindValue, useValue } from "cs2/api";
import { RoadBuilderToolModeEnum } from "domain/RoadBuilderToolMode";

const RoadBuilderToolMode$ = bindValue(mod.id, "RoadBuilderToolMode", RoadBuilderToolModeEnum.None);

export const ModView = () => {
  const RoadBuilderToolMode = useValue(RoadBuilderToolMode$);

  let [draggingItem, setDraggingItem] = useState<NetSectionItem | undefined>();
  let [mousePosition, setMousePosition] = useState<Number2>({ x: 0, y: 0 });
  let [mouseReleased, setMouseReleased] = useState<boolean>(false);
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

  if (RoadBuilderToolMode !== RoadBuilderToolModeEnum.Editing) return null;

  return (
    <DragContext.Provider value={dragData}>
      <div className={styles.view}>
        <LaneListPanel />
        <BottomView />
        <LaneListItemDrag ref={dragItemRef} />
      </div>
    </DragContext.Provider>
  );
};
