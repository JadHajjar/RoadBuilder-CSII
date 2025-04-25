import { Button, Tooltip } from "cs2/ui";
import styles from "./RoadConfigListItem.module.scss";
import { NetSectionItem } from "domain/NetSectionItem";
import { CSSProperties, MouseEventHandler, forwardRef, useContext } from "react";
import classNames from "classnames";
import { DragContext } from "mods/Contexts/DragContext";
import { MouseButtons } from "mods/util";
import { VanillaComponentResolver } from "vanillacomponentresolver";
import { RoadConfiguration } from "domain/RoadConfiguration";
import { deleteRoad, editRoad, findRoad, activateRoad, getRoadId$, roadBuilderToolMode$ } from "mods/bindings";
import { useValue } from "cs2/api";
import { useLocalization } from "cs2/l10n";
import { RoadBuilderToolModeEnum } from "domain/RoadBuilderToolMode";

export const RoadConfigListItem = ({
  road,
  index,
  selected,
  onSelect,
}: {
  road: RoadConfiguration;
  index: number;
  selected: boolean;
  onSelect: (idx: number | undefined) => void;
}) => {
  const getRoadId = useValue(getRoadId$);
  const toolMode = useValue(roadBuilderToolMode$);
  const { translate } = useLocalization();

  return (
    <div
      className={classNames(
        styles.container,
        styles[`i${index % 3}`],
        toolMode == RoadBuilderToolModeEnum.Picker && styles.large,
        selected && styles.expanded
      )}
    >
      <div
        className={classNames(
          VanillaComponentResolver.instance.assetGridTheme.item,
          styles.gridItem,
          getRoadId == road.ID && styles.active,
          road.Locked && styles.locked
        )}
        onClick={
          road.Locked
            ? undefined
            : () => {
                if (selected) activateRoad(road.ID);
                else onSelect(index);
              }
        }
        onDoubleClick={road.Locked ? undefined : () => activateRoad(road.ID)}
        onMouseLeave={road.Locked ? undefined : () => onSelect(undefined)}
      >
        <div className={styles.itemInfo}>
          <div className={styles.thumbnailContainer}>
            {road.Locked && <img className={styles.lockIcon} />}
            <img className={classNames(styles.gridThumbnail)} src={road.Thumbnail ?? "coui://roadbuildericons/RB_Unknown.svg"} />
          </div>

          <div className={classNames(styles.gridItemText)}>
            <p>{road.Name}</p>
          </div>
        </div>

        <div className={styles.buttons}>
          <Tooltip tooltip={translate("RoadBuilder.PlaceTooltip")}>
            <Button variant="flat" onSelect={() => activateRoad(road.ID)}>
              <img style={{ maskImage: "url(coui://roadbuildericons/RB_PlaceMore.svg)", width: "24rem", height: "24rem", marginRight: "6rem" }} />{" "}
              {translate("RoadBuilder.Place")}
            </Button>
          </Tooltip>
          <Tooltip tooltip={translate("RoadBuilder.EditTooltip")}>
            <Button variant="flat" onSelect={() => editRoad(road.ID)}>
              <img style={{ maskImage: "url(coui://roadbuildericons/RB_Edit.svg)" }} /> {translate("RoadBuilder.Edit")}
            </Button>
          </Tooltip>
          <Tooltip tooltip={translate("RoadBuilder.FindTooltip")}>
            <Button disabled={!road.Used} variant="flat" onSelect={() => findRoad(road.ID)} className={!road.Used && styles.disabled}>
              <img style={{ maskImage: "url(coui://roadbuildericons/RB_Location.svg)" }} /> {translate("RoadBuilder.Find")}
            </Button>
          </Tooltip>
          <Tooltip tooltip={translate("RoadBuilder.DeleteTooltip")}>
            <Button variant="flat" onSelect={() => deleteRoad([road.ID])} className={styles.danger}>
              <img style={{ maskImage: "url(coui://roadbuildericons/RB_Trash.svg)" }} /> {translate("RoadBuilder.Delete")}
            </Button>
          </Tooltip>
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
