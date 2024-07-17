import { Button, Number2, Tooltip } from "cs2/ui";
import styles from "./LaneListItem.module.scss";
import { NetSectionItem } from "domain/NetSectionItem";
import { CSSProperties, MouseEventHandler, forwardRef, useContext, useEffect, useRef, useState } from "react";
import classNames from "classnames";
import { DragContext } from "mods/Contexts/DragContext";
import { MouseButtons } from "mods/util";
import { NetSectionsStoreContext } from "mods/Contexts/NetSectionsStore";
import { VanillaComponentResolver } from "vanillacomponentresolver";

export const LaneListItem = ({ netSection }: { netSection: NetSectionItem }) => {
  // let [dragging, setDragging] = useState(false);
  let dragContext = useContext(DragContext);

  let dragging = dragContext.netSectionItem?.PrefabName == netSection.PrefabName;
  let containerStyles: Record<string, boolean> = {};
  containerStyles[styles.moving as string] = dragging;

  let updateModDragItem = () => {
    if (!dragging) {
      dragContext.onNetSectionItemChange(netSection);
    } else {
      dragContext.onNetSectionItemChange(undefined);
    }
  };

  let onMouseDown: MouseEventHandler<HTMLDivElement> = (evt) => {
    if (evt.button == MouseButtons.Primary) {
      updateModDragItem();
    }
  };

  return (
    <Button
      onMouseDown={onMouseDown}
      className={classNames(VanillaComponentResolver.instance.assetGridTheme.item, styles.gridItem, dragging && styles.moving)}
      variant="icon"
      focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
    >
      <img
        src={netSection.Thumbnail ?? "Media/Placeholder.svg"}
        className={classNames(VanillaComponentResolver.instance.assetGridTheme.thumbnail, styles.gridThumbnail)}
      />

      <div className={classNames(styles.gridItemText)}>
        <p>{netSection.DisplayName}</p>
      </div>

      {/* THIS IS FOR LATER IN CASE WE WANT CATEGORY ICONS NEXT TO THE LANES
        <div className={styles.rightSideContainer}>
          {props.showCategory && <img src={props.prefab.categoryThumbnail}></img>}

          {props.prefab.dlcThumbnail && <img src={props.prefab.dlcThumbnail}></img>}

          {props.prefab.random && <img src="coui://uil/Colored/Dice.svg"></img>}
        </div>*/}
    </Button>
  );
};

enum DragType {
  None,
  Order,
  Add,
}

export const LaneListItemDrag = forwardRef<HTMLDivElement>((props, ref) => {
  let dragData = useContext(DragContext);
  let sectionsStore = useContext(NetSectionsStoreContext);
  let [position, setPosition] = useState<Number2>({ x: 0, y: 0 });

  let dragType = DragType.None;
  if (dragData.netSectionItem) {
    dragType = DragType.Add;
  }
  if (dragData.roadLane) {
    dragType = DragType.Order;
  }
  if (dragType == DragType.None) {
    return <></>;
  }
  let netSection = dragType == DragType.Add ? dragData.netSectionItem! : sectionsStore[dragData.roadLane!.SectionPrefabName];

  let offsetStyle: CSSProperties = {
    left: `calc( ${dragData.mousePosition.x}px - 40rem)`,
    top: `calc( ${dragData.mousePosition.y}px - 40rem)`,
  };
  let containerClasses = classNames(
    VanillaComponentResolver.instance.assetGridTheme.item,
    styles.gridItem,
    styles.dragged,
    styles.dragRepresentation,
    { [styles.bottomRow]: dragType == DragType.Order }
  );

  return (
    <div style={offsetStyle} className={containerClasses} ref={ref}>
      <img
        src={netSection.Thumbnail ?? "Media/Placeholder.svg"}
        className={classNames(VanillaComponentResolver.instance.assetGridTheme.thumbnail, styles.gridThumbnail)}
      />
    </div>
  );
});
