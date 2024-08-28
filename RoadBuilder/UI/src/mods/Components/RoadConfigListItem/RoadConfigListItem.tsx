import { Button, Tooltip } from "cs2/ui";
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
import { useLocalization } from "cs2/l10n";

export const RoadConfigListItem = ({ road }: { road: RoadConfiguration }) => {
  const getRoadId = useValue(getRoadId$);
  const { translate } = useLocalization();

  if (road.Locked) {
    return (
      <div
        className={classNames(styles.gridItemLocked, VanillaComponentResolver.instance.assetGridTheme.item, getRoadId == road.ID && styles.active)}
      >
        <div className={styles.itemInfo}>
          <div className={styles.thumbnailContainer}>
            <img className={styles.lockIcon} />
            <img className={classNames(styles.gridThumbnail)} src={road.Thumbnail ?? "coui://roadbuildericons/RB_Unknown.svg"} />
          </div>

          <div className={classNames(styles.gridItemText)}>
            <p>{road.Name}</p>
          </div>
        </div>
      </div>
    );
  }

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
        <Tooltip tooltip={translate("RoadBuilder.EditTooltip")}>
          <Button variant="flat" onSelect={() => editRoad(road.ID)}>
            <img style={{ maskImage: "url(coui://gameui/Media/Glyphs/Pen.svg)" }} /> {translate("RoadBuilder.Edit")}
          </Button>
        </Tooltip>
        <Tooltip tooltip={translate("RoadBuilder.FindTooltip")}>
          <Button variant="flat" onSelect={() => findRoad(road.ID)}>
            <img style={{ maskImage: "url(coui://gameui/Media/Radio/MapMarker.svg)" }} /> {translate("RoadBuilder.Find")}
          </Button>
        </Tooltip>
        <Tooltip tooltip={translate("RoadBuilder.DeleteTooltip")}>
          <Button variant="flat" onSelect={() => deleteRoad(road.ID)} className={styles.danger}>
            <img style={{ maskImage: "url(coui://gameui/Media/Glyphs/Trash.svg)" }} /> {translate("RoadBuilder.Delete")}
          </Button>
        </Tooltip>
      </div>
    </div>
  );
};
