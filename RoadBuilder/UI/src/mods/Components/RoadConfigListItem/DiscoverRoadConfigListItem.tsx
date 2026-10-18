import { Button, Tooltip } from "cs2/ui";
import styles from "./DiscoverRoadConfigListItem.module.scss";
import { NetSectionItem } from "domain/NetSectionItem";
import classNames from "classnames";
import { VanillaComponentResolver } from "vanillacomponentresolver";
import { RoadConfiguration } from "domain/RoadConfiguration";
import { activateRoad, DiscoverDownloading$, downloadConfig } from "mods/bindings";
import { useValue } from "cs2/api";
import { useLocalization } from "cs2/l10n";
import { GetCategoryIcon } from "domain/RoadCategory";

export const DiscoverRoadConfigListItem = ({ road }: { road: RoadConfiguration }) => {
  // Download state comes from C#, the road only becomes placeable once it was actually added
  const downloading = useValue(DiscoverDownloading$).includes(road.ID);
  const { translate } = useLocalization();

  return (
    <div
      className={classNames(VanillaComponentResolver.instance.assetGridTheme.item, styles.gridItem)}
      onDoubleClick={() => road.Available && activateRoad(road.ID)}
    >
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
        <div className={classNames(styles.buttons, !road.Available && styles.downloaded)}>
          <Button
            variant="flat"
            disabled={downloading}
            onSelect={road.Available ? () => activateRoad(road.ID) : () => downloadConfig(road.ID)}
          >
            <img
              style={{
                maskImage: road.Available ? "url(coui://roadbuildericons/RB_PlaceMore.svg)" : "url(coui://roadbuildericons/RB_Package.svg)",
              }}
            />
            {translate(road.Available ? "RoadBuilder.Place" : "RoadBuilder.Download")}
          </Button>
        </div>
      </div>
    </div>
  );
};
