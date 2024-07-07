import { NetSectionItem } from "domain/NetSectionItem";
import styles from "./RoadButtonSmall.module.scss";
import { Button, Number2, Tooltip } from "cs2/ui";
import { EditPropertiesPopup } from "../EditPropertiesPopup/EditPropertiesPopup";
import { MouseEvent, MouseEventHandler, forwardRef, useCallback, useContext, useRef, useState } from "react";
import { DragContext } from "mods/Contexts/DragContext";
import classNames from "classnames";
import { RoadLane } from "domain/RoadProperties";
import { NetSectionsStoreContext } from "mods/Contexts/NetSectionsStore";
import { MouseButtons } from "mods/util";
import { LanePropertiesContext } from "mods/Contexts/LanePropertiesContext";
import { useRem } from "cs2/utils";

type _Props = {
  roadLane: RoadLane;
  onDelete: (index: number) => void;
  onClick?: (index: number, evt: MouseEvent<HTMLDivElement>) => void;
  index: number;
};
export const RoadButtonSmall = (props: _Props) => {
  let laneCtx = useContext(LanePropertiesContext);
  let dragState = useContext(DragContext);
  let netSectionStore = useContext(NetSectionsStoreContext);
  let containerRef = useRef<HTMLDivElement>(null);
  let rem = useRem();

  let dragging = dragState.oldIndex == props.index;
  let onMouseEnter = useCallback(() => {
    if (containerRef.current == null) {      
      return;
    }
    let bounds = containerRef.current.getBoundingClientRect();
    let position : Number2 = {
      x: bounds.x + (bounds.width / 2),
      y: bounds.top - (20 * rem)
    };
    laneCtx.open(props.roadLane, props.index, position);    
  }, [containerRef.current, laneCtx.open]);

  let updateModDragItem = () => {
    if (!dragging) {
      dragState.setRoadLane(props.roadLane, props.index);
    } else {
      dragState.setRoadLane(undefined, undefined);
    }
  };

  let onMouseDown: MouseEventHandler<HTMLDivElement> = (evt) => {
    if (evt.button == MouseButtons.Primary) {
      updateModDragItem();
    }
  };

  return (
    <div ref={containerRef} className={styles.container} onMouseEnter={onMouseEnter}>
      <div className={classNames(styles.button, { [styles.dragging]: dragging })} onMouseDown={onMouseDown}>
        <img src="Media/Placeholder.svg" />
      </div>
      <div className={styles.informationBar}>
        <div className={styles.laneDesign}>
          <img className={classNames(styles.arrow, props.roadLane.Invert ? styles.down : styles.up)} />
        </div>
        <div className={styles.laneName}>{props.roadLane.Width! > 0 && props.roadLane.Width + " m"}</div>
      </div>
    </div>
  );
};
