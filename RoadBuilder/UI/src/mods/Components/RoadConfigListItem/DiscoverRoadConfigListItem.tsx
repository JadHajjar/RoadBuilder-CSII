import { Button, Tooltip } from "cs2/ui";
import styles from "./DiscoverRoadConfigListItem.module.scss";
import { NetSectionItem } from "domain/NetSectionItem";
import { CSSProperties, MouseEventHandler, forwardRef, useContext, useState } from "react";
import classNames from "classnames";
import { DragContext } from "mods/Contexts/DragContext";
import { MouseButtons } from "mods/util";
import { VanillaComponentResolver } from "vanillacomponentresolver";
import { RoadConfiguration } from "domain/RoadConfiguration";
import { activateRoad, downloadConfig } from "mods/bindings";
import { useValue } from "cs2/api";
import { useLocalization } from "cs2/l10n";
import { GetCategoryIcon } from "domain/RoadCategory";

export const DiscoverRoadConfigListItem = ({ road }: { road: RoadConfiguration }) => {
  let [downloaded, setDownloaded] = useState<boolean>(false);
  const { translate } = useLocalization();

  return (
    <div className={classNames(VanillaComponentResolver.instance.assetGridTheme.item, styles.gridItem)} onDoubleClick={() => activateRoad(road.ID)}>
      <div className={styles.itemInfo}>
        <img className={classNames(styles.gridThumbnail)} src={road.Thumbnail ?? "coui://roadbuildericons/RB_Unknown.svg"} />

        <div className={classNames(styles.gridItemText)}>
          <p>{road.Name}</p>
        </div>
      </div>

      <div className={styles.bottom}>
        <div className={styles.info}>
          <img className={styles.category} src={GetCategoryIcon(road.Category)}></img>
          <div className={styles.author}>
            <img style={{ maskImage: "url(coui://roadbuildericons/RB_User.svg)" }} />
            <span>{road.Author}</span>
          </div>
        </div>
        <div className={classNames(styles.buttons, !road.Available && !downloaded && styles.downloaded)}>
          <Button
            variant="flat"
            onSelect={
              road.Available || downloaded
                ? () => activateRoad(road.ID)
                : () => {
                    setDownloaded(true);
                    downloadConfig(road.ID);
                  }
            }
          >
            <img
              style={{
                maskImage:
                  road.Available || downloaded ? "url(coui://roadbuildericons/RB_PlaceMore.svg)" : "url(coui://roadbuildericons/RB_Package.svg)",
              }}
            />
            {translate(road.Available || downloaded ? "RoadBuilder.Place" : "RoadBuilder.Download")}
          </Button>
        </div>
      </div>
    </div>
  );
};
