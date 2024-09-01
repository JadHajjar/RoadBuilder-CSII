import { Button, Tooltip } from "cs2/ui";
import styles from "./LaneListItem.module.scss";
import { NetSectionItem } from "domain/NetSectionItem";
import { CSSProperties, MouseEventHandler, forwardRef, useContext } from "react";
import classNames from "classnames";
import { DragContext } from "mods/Contexts/DragContext";
import { MouseButtons } from "mods/util";
import { VanillaComponentResolver } from "vanillacomponentresolver";
import { useLocalization } from "cs2/l10n";

export const LaneListItem = ({ netSection, small }: { netSection: NetSectionItem; small: boolean }) => {
  // let [dragging, setDragging] = useState(false);
  let dragContext = useContext(DragContext);
  let { translate } = useLocalization();

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
    <div
      onMouseDown={onMouseDown}
      className={classNames(VanillaComponentResolver.instance.assetGridTheme.item, styles.gridItem, dragging && styles.moving, small && styles.small)}
    >
      <div className={classNames(styles.gridThumbnail)}>
        <img src={netSection.Thumbnail ?? "coui://roadbuildericons/RB_Unknown.svg"} />
      </div>

      <div className={classNames(styles.gridItemText)}>
        <p>{netSection.DisplayName}</p>
      </div>

      <div className={styles.rightSideContainer}>
        {netSection.IsRestricted && (
          <Tooltip tooltip={translate("RoadBuilder.RestrictedLane")}>
            <img src="coui://gameui/Media/Game/Notifications/BuildingOnFire.svg"></img>
          </Tooltip>
        )}

        {netSection.IsCustom && (
          <Tooltip tooltip={translate("RoadBuilder.CustomLane")}>
            <img src="coui://gameui/Media/Glyphs/ParadoxModsCloud.svg"></img>
          </Tooltip>
        )}
      </div>
    </div>
  );
};

enum DragType {
  None,
  Order,
  Add,
}

export const LaneListItemDrag = forwardRef<HTMLDivElement>((props, ref) => {
  let dragData = useContext(DragContext);

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
  let netSection = dragType == DragType.Add ? dragData.netSectionItem! : dragData.roadLane?.NetSection!;

  let offsetStyle: CSSProperties = {
    left: `calc( ${dragData.mousePosition.x}px - 40rem)`,
    top: `calc( ${dragData.mousePosition.y}px - 40rem)`,
  };
  let containerClasses = classNames(VanillaComponentResolver.instance.assetGridTheme.item, styles.gridItem, styles.dragRepresentation, {
    [styles.bottomRow]: dragType == DragType.Order,
  });

  return (
    <div style={offsetStyle} className={containerClasses} ref={ref}>
      <div
        className={classNames(
          VanillaComponentResolver.instance.assetGridTheme.thumbnail,
          styles.gridThumbnail,
          dragData.roadLane?.InvertImage && styles.inverted
        )}
      >
        <img src={netSection.Thumbnail ?? "coui://roadbuildericons/RB_Unknown.svg"} />
      </div>
    </div>
  );
});
