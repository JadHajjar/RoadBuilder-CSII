import { MouseEventHandler, startTransition, useContext, useEffect, useMemo, useRef, useState, useTransition } from "react";
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
import { RoadLane } from "domain/RoadLane";
import { NetSectionsStore, NetSectionsStoreContext } from "mods/Contexts/NetSectionsStore";
import { RoadPropertiesPanel } from "mods/RoadPropertiesPanel/RoadPropertiesPanel";
import { LanePropertiesContext, LanePropertiesContextData } from "mods/Contexts/LanePropertiesContext";
import { EditPropertiesPopup } from "mods/Components/EditPropertiesPopup/EditPropertiesPopup";
import { useLocalization } from "cs2/l10n";

export const ModView = () => {
  const roadBuilderToolMode = useValue(roadBuilderToolMode$);
  let rem = useRem();
  let {translate} = useLocalization();

  let [draggingItem, setDraggingItem] = useState<NetSectionItem | undefined>();
  let [draggingLane, setDraggingLane] = useState<RoadLane | undefined>();
  let [mousePosition, setMousePosition] = useState<Number2>({ x: 0, y: 0 });
  let [mouseReleased, setMouseReleased] = useState<boolean>(false);
  let [fromDragIndex, setFromDragIndex] = useState<number | undefined>(undefined);
  let [actionPopupPosition, setActionPopupPosition] = useState<Number2>({ x: 0, y: 0 });
  let dragItemRef = useRef<HTMLDivElement>(null);
  let allNetSections = useValue(allNetSections$);
  let defaultLanePropCtx = useContext(LanePropertiesContext);
  let [lanePropState, setLanePropState] = useState<LanePropertiesContextData>(defaultLanePropCtx);

  let lanePropCtx = useMemo(
    () => ({
      ...lanePropState,
      open(roadLane: RoadLane, index: number, position: Number2) {
        setLanePropState({
          ...lanePropState,
          position,
          index,
          laneData: roadLane,
          showPopup: true,
        });
      },
      close() {
        setLanePropState({
          ...lanePropState,
          showPopup: false,
        });
      },
    }),
    [lanePropState, setLanePropState]
  );

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
  }, [roadBuilderToolMode]);

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
      content = (
        <>
          <div className={styles.pickerHint}>{translate("Prompt[PickerHint]", "Click On A Road")}</div>
          <LaneListItemDrag ref={dragItemRef} />
          <LaneListPanel />
        </>
      );
      break;
    case RoadBuilderToolModeEnum.ActionSelection:
      content = (
        <>
          <ActionPopup popupPosition={actionPopupPosition}/>
        </>
      );
      break;
    case RoadBuilderToolModeEnum.Editing:
    case RoadBuilderToolModeEnum.EditingSingle:
    case RoadBuilderToolModeEnum.EditingNonExistent:
      content = (
        <>
          <LaneListPanel />
          <BottomView />
          <RoadPropertiesPanel />
          <LaneListItemDrag ref={dragItemRef} />
          <EditPropertiesPopup />
        </>
      );
      break;
    default:
      return <></>;
  }
  return (
    <DragContext.Provider value={dragData}>
      <NetSectionsStoreContext.Provider value={nStore}>
        <LanePropertiesContext.Provider value={lanePropCtx}>
          <div className={styles.view}>{content}</div>
        </LanePropertiesContext.Provider>
      </NetSectionsStoreContext.Provider>
    </DragContext.Provider>
  );
};
