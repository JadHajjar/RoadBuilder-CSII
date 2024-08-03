import styles from "./EditPropertiesPopup.module.scss";
import { Button } from "cs2/ui";
import { CSSProperties, MouseEvent, MouseEventHandler, useContext } from "react";
import { useRem } from "cs2/utils";
import { OptionsPanelComponent } from "../OptionsPanel/OptionsPanel";
import { LanePropertiesContext } from "mods/Contexts/LanePropertiesContext";
import { useValue } from "cs2/api";
import { duplicateLane, roadLanes$, setRoadLanes } from "mods/bindings";
import { removeAt } from "mods/util";
import { laneOptionClicked } from "mods/bindings";
import { DragContext } from "mods/Contexts/DragContext";
import { RoadLane } from "domain/RoadLane";

export const EditPropertiesPopup = () => {
  let rem = useRem();
  let dragCtx = useContext(DragContext);
  let laneCtx = useContext(LanePropertiesContext);
  let roadLanes = useValue(roadLanes$);

  if (!laneCtx.showPopup || laneCtx.laneData == undefined) {
    return <></>;
  }

  let inlineStyle: CSSProperties = {
    left: laneCtx.position.x + "px",
  };

  let onCopy = () => {
    duplicateLane(laneCtx.index);
  };

  let onDelete = () => {
    laneCtx.close();
    let nList = removeAt(roadLanes, laneCtx.index);
    setRoadLanes(nList);
  };

  let currentLane = roadLanes[laneCtx.index];
  let isDragging = dragCtx.roadLane != null || dragCtx.netSectionItem != null;

  if (!currentLane || isDragging) return <></>;

  return (
    <div className={styles.viewContainer} style={inlineStyle}  onMouseLeave={laneCtx.close}>
      <div className={styles.view}>
        <div className={styles.topBar}>
          <div className={styles.title}>{laneCtx.laneData.NetSection?.DisplayName}</div>
          <div className={styles.copyButton} onClick={onCopy}>
            <img />
          </div>
          <div className={styles.deleteButton} onClick={onDelete}>
            <img />
          </div>
        </div>
        <div className={styles.content}>
          <div className={styles.options}>
            {currentLane?.Options != undefined && currentLane?.Options?.length !== 0 ? (
              <OptionsPanelComponent
                OnChange={(x, y, z) =>{console.log("CLICKED LANE OPT");laneOptionClicked(currentLane.Index, x, y, z)}}
                options={currentLane?.Options ?? new Array()}
              ></OptionsPanelComponent>
            ) : (
              <span> No Options Available</span>
            )}
          </div>
          <div className={styles.caret}></div>
        </div>
      </div>      
    </div>
  );
};
