import { Button } from "cs2/ui";
import styles from "./RoadConfigListItem.module.scss";
import { NetSectionItem } from "domain/NetSectionItem";
import { CSSProperties, MouseEventHandler, forwardRef, useContext } from "react";
import classNames from "classnames";
import { DragContext } from "mods/Contexts/DragContext";
import { MouseButtons } from "mods/util";
import { VanillaComponentResolver } from "vanillacomponentresolver";
import { RoadConfiguration } from "domain/RoadConfiguration";
import { deleteRoad, editRoad, findRoad, activateRoad, getRoadId$ } from "mods/bindings";
import { useValue } from "cs2/api";

export const RoadConfigListItem = ({ road }: { road: RoadConfiguration }) => {
  const getRoadId = useValue(getRoadId$);

  return (
    <div
      className={classNames(VanillaComponentResolver.instance.assetGridTheme.item, styles.gridItem, getRoadId == road.ID && styles.active)}
      onClick={() => activateRoad(road.ID)}
    >
      <img className={classNames(styles.gridThumbnail)} src={road.Thumbnail ?? "Media/Placeholder.svg"} />

      <div className={classNames(styles.gridItemText)}>
        <p>{road.Name}</p>
      </div>

      <div className={styles.rightButtons}>
        <Button variant="flat" onSelect={() => editRoad(road.ID)}>
          <img src="Media/Glyphs/Pen.svg" />
        </Button>
        <Button variant="flat" onSelect={() => findRoad(road.ID)}>
          <img src="Media/Radio/MapMarker.svg" />
        </Button>
        <Button variant="flat" onSelect={() => deleteRoad(road.ID)}>
          <img src="Media/Glyphs/Trash.svg" />
        </Button>
      </div>
    </div>
  );
};
