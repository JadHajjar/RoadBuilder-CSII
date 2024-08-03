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
      <div className={styles.itemInfo}>
        <img className={classNames(styles.gridThumbnail)} src={road.Thumbnail ?? "coui://roadbuildericons/RB_Unknown.svg"} />

        <div className={classNames(styles.gridItemText)}>
          <p>{road.Name}</p>
        </div>
      </div>

      <div className={styles.buttons}>
        <Button variant="flat" onSelect={() => editRoad(road.ID)}>
          <img style={{ maskImage: "url(coui://gameui/Media/Glyphs/Pen.svg)" }} /> Edit
        </Button>
        <Button variant="flat" onSelect={() => findRoad(road.ID)}>
          <img style={{ maskImage: "url(coui://gameui/Media/Radio/MapMarker.svg)" }} /> Find
        </Button>
        <Button variant="flat" onSelect={() => deleteRoad(road.ID)}>
          <img style={{ maskImage: "url(coui://gameui/Media/Glyphs/Trash.svg)" }} /> Delete
        </Button>
      </div>
    </div>
  );
};
