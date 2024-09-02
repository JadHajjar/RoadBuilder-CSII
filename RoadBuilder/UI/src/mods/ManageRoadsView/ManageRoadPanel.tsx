import { Button, Tooltip } from "cs2/ui";
import styles from "./ManageRoadPanel.module.scss";
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

export const ManageRoadPanel = ({ road }: { road: RoadConfiguration }) => {
  const getRoadId = useValue(getRoadId$);
  const { translate } = useLocalization();

  return (
      <img className={styles.thumbnail} src={road.Thumbnail ?? "coui://roadbuildericons/RB_Unknown.svg"} />
        <div className={styles.itemInfo}>
          <div className={classNames(styles.gridItemText)}>
            <p>{road.Name}</p>
          </div>
      </div>
  );
};
