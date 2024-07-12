import styles from "./BottomView.module.scss";
import { RoadButtonSmall } from "../Components/RoadButtonSmall/RoadButtonSmall";
import { DragAndDropDivider, DragAndDropDividerRef } from "mods/Components/DragAndDropDivider/DragAndDropDivider";
import { Button, Scrollable, Tooltip } from "cs2/ui";
import { NetSectionItem } from "domain/NetSectionItem";
import { CSSProperties, useContext, useEffect, useRef } from "react";
import { DragContext } from "mods/Contexts/DragContext";
import { useValue } from "cs2/api";
import { clearTool, createNewPrefab, setRoadLanes, roadBuilderToolMode$, roadLanes$, isPaused$, toggleTool } from "mods/bindings";
import { RoadBuilderToolModeEnum } from "domain/RoadBuilderToolMode";
import { RoadLane } from "domain/RoadProperties";
import { VanillaComponentResolver } from "vanillacomponentresolver";
import { DragAndDropScrollable } from "mods/Components/DragAndDropScrollable/DragAndDropScrollable";
import { DeleteAreaDnD } from "mods/Components/DeleteAreaDnD/DeleteAreaDnD";

export const BottomView = () => {
  let dragContext = useContext(DragContext);
  let toolMode = useValue(roadBuilderToolMode$);
  let roadLanes = useValue(roadLanes$);
  let isPaused = useValue(isPaused$);
  let dividersRef = useRef<DragAndDropDividerRef[]>([]);
  let scrollController = VanillaComponentResolver.instance.useScrollController();

  useEffect(() => {}, [scrollController]);

  useEffect(() => {
    dividersRef.current = dividersRef.current.slice(0, roadLanes.length + 1);
  }, [roadLanes]);

  useEffect(() => {
    let isDragging = dragContext.netSectionItem || dragContext.roadLane;
    if (dragContext.mouseReleased && dragContext.dragElement != undefined && isDragging) {
      let dragBounds = dragContext.dragElement.getBoundingClientRect();
      for (let i = 0; i < dividersRef.current.length; i++) {
        let divider = dividersRef.current[i];
        if (divider.intersects(dragBounds)) {
          if (dragContext.roadLane) {
            moveLane(dragContext.roadLane, dragContext.oldIndex!, divider.listIdx);
          } else if (dragContext.netSectionItem) {
            addItem(dragContext.netSectionItem, divider.listIdx);
          }
          break;
        }
      }
      dragContext.onNetSectionItemChange(undefined);
      dragContext.setRoadLane(undefined, undefined);
    }
  }, [dragContext.mouseReleased]);

  let moveLane = (lane: RoadLane, oldIndex: number, newIndex: number) => {
    let nList = [...roadLanes.slice(0, oldIndex), ...roadLanes.slice(Math.min(oldIndex + 1, roadLanes.length))];
    let insertIndex = oldIndex < newIndex ? newIndex - 1 : newIndex;
    insertIndex = Math.min(insertIndex, nList.length);
    insertIndex = Math.max(insertIndex, 0);
    nList = [...nList.slice(0, insertIndex), lane, ...nList.slice(insertIndex)];
    setRoadLanes(nList);
  };

  let addItem = (item: NetSectionItem, index: number) => {
    let rLane: RoadLane = {
      SectionPrefabName: item.PrefabName,
      IsGroup: item.IsGroup,
      Index: -1,
    };
    let nList = [...roadLanes.slice(0, index), rLane, ...roadLanes.slice(index)];
    setRoadLanes(nList);
  };

  let deleteLane = (idx: number) => {
    setRoadLanes([...roadLanes.slice(0, idx), ...roadLanes.slice(Math.min(idx + 1, roadLanes.length))]);
  };

  let items = roadLanes
    .filter((val) => val.SectionPrefabName)
    .flatMap((val, idx) => {
      return [
        <DragAndDropDivider ref={(elem) => (dividersRef.current[idx] = elem!)} key={idx * 2} listIdx={idx} />,
        <RoadButtonSmall index={idx} onDelete={deleteLane} roadLane={val} key={idx * 2 + 1} />,
      ];
    });
  items.push(<DragAndDropDivider ref={(elem) => (dividersRef.current[roadLanes.length] = elem!)} key={items.length} listIdx={roadLanes.length} />);

  let copyButtonStyle: CSSProperties = {
    visibility: toolMode == RoadBuilderToolModeEnum.EditingSingle ? "hidden" : "initial",
  };

  let isDragging = dragContext.netSectionItem || dragContext.roadLane;
  return (
    <div className={styles.viewContainer}>
      <DragAndDropScrollable className={styles.view} trackVisibility="always" horizontal controller={scrollController}>
        {items}
        {roadLanes.length == 0 && !dragContext.netSectionItem ? <div className={styles.hint}>Drag Lanes Here</div> : <></>}
      </DragAndDropScrollable>
      <div className={styles.bottomBG + " " + (isPaused && styles.paused)}>
        {isDragging ? (
          <></>
        ) : (
          <>
            <div className={styles.bottomLeftButtonBar}>
              <Tooltip tooltip={"Pick a different road"}>
                <Button
                  className={styles.backButton}
                  variant="flat"
                  onSelect={() => {
                    toggleTool();
                    toggleTool();
                  }}
                >
                  <img src="coui://gameui/Media/Glyphs/ArrowLeft.svg" />
                </Button>
              </Tooltip>
              <Button style={copyButtonStyle} className={styles.copyButton} variant="flat" onSelect={createNewPrefab}>
                Copy to New Prefab
              </Button>
            </div>
            <Button className={styles.closeButton} src="Media/Glyphs/Close.svg" variant="icon" onSelect={clearTool} />
          </>
        )}
      </div>
      <DeleteAreaDnD onRemove={deleteLane} />
    </div>
  );
};
