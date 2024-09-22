import { Button, Tooltip } from "cs2/ui";
import styles from "./ManageRoadConfigListItem.module.scss";
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

export const ManageRoadConfigListItem = ({
  road,
  selected,
  selectRoad,
}: {
  road: RoadConfiguration;
  selected: boolean;
  selectRoad: (road: RoadConfiguration) => void;
}) => {
  const getRoadId = useValue(getRoadId$);
  const { translate } = useLocalization();

  if (road.Locked) {
    return (
      <div className={classNames(styles.gridItemLocked, VanillaComponentResolver.instance.assetGridTheme.item)}>
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
      className={classNames(VanillaComponentResolver.instance.assetGridTheme.item, styles.gridItem, selected && styles.active)}
      onDoubleClick={() => activateRoad(road.ID)}
      onClick={() => selectRoad(road)}
    >
      <div className={styles.itemInfo}>
        <img className={classNames(styles.gridThumbnail)} src={road.Thumbnail ?? "coui://roadbuildericons/RB_Unknown.svg"} />

        <div className={classNames(styles.gridItemText)}>
          <p>{road.Name}</p>
        </div>

        <div className={styles.sideIcons}>
          {road.Used && (
            <img
              className={styles.masked}
              style={{
                maskImage: "url(coui://roadbuildericons/RB_Location.svg)",
                backgroundColor: "rgba(69, 215, 91)",
              }}
            />
          )}
          {road.IsNotInPlayset && (
            <img
              className={styles.masked}
              style={{
                maskImage: "url(coui://roadbuildericons/RB_Playset.svg)",
                backgroundColor: "rgba(253, 43, 77, 0.9)",
              }}
            />
          )}
        </div>
      </div>
    </div>
  );
};
