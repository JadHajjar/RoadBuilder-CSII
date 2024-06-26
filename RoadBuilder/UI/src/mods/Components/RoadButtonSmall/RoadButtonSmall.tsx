import { NetSectionItem } from "domain/NetSectionItem";
import styles from "./RoadButtonSmall.module.scss";
import { Button, Tooltip } from "cs2/ui";
import { EditPropertiesPopup } from "../EditPropertiesPopup/EditPropertiesPopup";
import { MouseEvent, MouseEventHandler, forwardRef, useContext, useState } from "react";
import { DragContext } from "mods/Contexts/DragContext";
import classNames from "classnames";
import { RoadLane } from "domain/RoadProperties";
import { NetSectionsStoreContext } from "mods/Contexts/NetSectionsStore";
import { MouseButtons } from "mods/util";

type _Props = {
  roadLane: RoadLane;
  onDelete: (index: number) => void;
  onClick?: (index: number, evt: MouseEvent<HTMLDivElement>) => void;
  index: number;
};
export const RoadButtonSmall = (props: _Props) => {
  let [showProperties, setShowProperties] = useState(false);
  let dragState = useContext(DragContext);
  let netSectionStore = useContext(NetSectionsStoreContext);
  let item = netSectionStore[props.roadLane.SectionPrefabName];

  let dragging = dragState.oldIndex == props.index;
  let onMouseEnter = () => {
    setShowProperties(true);
  };

  let onMouseLeave = () => {
    setShowProperties(false);
  };

  let updateModDragItem = () => {
    if (!dragging) {
      dragState.setRoadLane(props.roadLane, props.index);
    } else {
      dragState.setRoadLane(undefined, undefined);
      console.log("Release");
    }
  };

  let onMouseDown: MouseEventHandler<HTMLDivElement> = (evt) => {
    if (evt.button == MouseButtons.Primary) {
      updateModDragItem();
    }
  };

  let popup =
    showProperties && !dragState.dragElement ? (
      <EditPropertiesPopup onDelete={props.onDelete} index={props.index} item={item} lane={props.roadLane} />
    ) : (
      <></>
    );

  return (
    <div className={styles.container} onMouseEnter={onMouseEnter} onMouseLeave={onMouseLeave}>
      <div className={classNames(styles.button, { [styles.dragging]: dragging })} onMouseDown={onMouseDown}>
        <img src="Media/Placeholder.svg" />
      </div>
      <div className={styles.informationBar}>
        <div className={styles.laneDesign}>
          <img className={classNames(styles.arrow, props.roadLane.Invert ? styles.down : styles.up)} />
        </div>
        <div className={styles.laneName}>{props.roadLane.Width! > 0 && props.roadLane.Width + " m"}</div>
      </div>
      {popup}
    </div>
  );
};
