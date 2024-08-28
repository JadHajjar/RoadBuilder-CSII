import styles from "./RoadButtonSmall.module.scss";
import { Number2, Tooltip } from "cs2/ui";
import { MouseEvent, MouseEventHandler, useCallback, useContext, useRef } from "react";
import { DragContext } from "mods/Contexts/DragContext";
import classNames from "classnames";
import { RoadLane } from "domain/RoadLane";
import { NetSectionsStoreContext } from "mods/Contexts/NetSectionsStore";
import { MouseButtons } from "mods/util";
import { LanePropertiesContext } from "mods/Contexts/LanePropertiesContext";
import { useRem } from "cs2/utils";
import { useLocalization } from "cs2/l10n";

type _Props = {
  roadLane: RoadLane;
  onDelete: (index: number) => void;
  onClick?: (index: number, evt: MouseEvent<HTMLDivElement>) => void;
  index: number;
};
export const RoadButtonSmall = (props: _Props) => {
  let laneCtx = useContext(LanePropertiesContext);
  let dragState = useContext(DragContext);
  let containerRef = useRef<HTMLDivElement>(null);
  let rem = useRem();
  let { translate } = useLocalization();

  let dragging = dragState.oldIndex == props.index;
  let onMouseEnter = useCallback(() => {
    if (containerRef.current == null) {
      return;
    }
    let bounds = containerRef.current.getBoundingClientRect();
    let position: Number2 = {
      x: bounds.x + bounds.width / 2,
      y: bounds.top - 20 * rem,
    };
    laneCtx.open(props.roadLane, props.index, position);
  }, [containerRef.current, laneCtx.open]);

  let updateModDragItem = () => {
    if (!dragging) {
      dragState.setRoadLane(props.roadLane, props.index);
    } else {
      dragState.setRoadLane(undefined, undefined);
    }
  };

  let onMouseDown: MouseEventHandler<HTMLDivElement> = (evt) => {
    if (evt.button == MouseButtons.Primary) {
      updateModDragItem();
    }
  };

  if (props.roadLane.IsEdgePlaceholder) {
    return (
      <Tooltip tooltip={translate("RoadBuilder.Warning[MissingEdgeLane]")}>
        <div
          ref={containerRef}
          className={classNames(
            styles.container,
            styles.edgePlaceholder,
            (dragState.netSectionItem?.IsEdge || dragState.roadLane?.NetSection?.IsEdge) && styles.highlighted
          )}
        >
          <div className={styles.button}>
            <div className={styles.imageContainer}>
              <img className={props.roadLane.InvertImage && styles.inverted} src="coui://roadbuildericons/RB_Edge.svg" />
            </div>
          </div>
          <div className={styles.informationBar}></div>
        </div>
      </Tooltip>
    );
  }

  return (
    <div ref={containerRef} className={styles.container} onMouseEnter={onMouseEnter}>
      <div className={classNames(styles.button, { [styles.dragging]: dragging })} onMouseDown={onMouseDown}>
        <div className={styles.imageContainer}>
          <img
            src={props.roadLane.NetSection?.Thumbnail ?? "coui://roadbuildericons/RB_Unknown.svg"}
            className={props.roadLane.InvertImage && styles.inverted}
          />
        </div>
      </div>
      <div className={styles.informationBar}>
        <div className={styles.laneDesign}>
          <div
            className={styles[props.roadLane.Texture ?? "asphalt"]}
            style={{
              backgroundColor: props.roadLane.Color,
            }}
          >
            {!(props.roadLane.NoDirection ?? false) && (
              <img className={classNames(styles.arrow, props.roadLane.TwoWay ? styles.twoway : props.roadLane.Invert ? styles.down : styles.up)} />
            )}
          </div>
        </div>
        <div className={styles.laneName}>{props.roadLane.NetSection?.WidthText}</div>
      </div>
    </div>
  );
};
