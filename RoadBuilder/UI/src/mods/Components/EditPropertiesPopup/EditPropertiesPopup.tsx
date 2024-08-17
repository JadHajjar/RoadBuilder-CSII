import styles from "./EditPropertiesPopup.module.scss";
import { CSSProperties, useContext } from "react";
import { useRem } from "cs2/utils";
import { OptionsPanelComponent } from "../OptionsPanel/OptionsPanel";
import { LanePropertiesContext } from "mods/Contexts/LanePropertiesContext";
import { useValue } from "cs2/api";
import { duplicateLane, roadLanes$, setRoadLanes } from "mods/bindings";
import { removeAt } from "mods/util";
import { laneOptionClicked } from "mods/bindings";
import { DragContext } from "mods/Contexts/DragContext";
import { useLocalization } from "cs2/l10n";
import { Tooltip } from "cs2/ui";

export const EditPropertiesPopup = () => {
  let rem = useRem();
  let dragCtx = useContext(DragContext);
  let laneCtx = useContext(LanePropertiesContext);
  let roadLanes = useValue(roadLanes$);
  const { translate } = useLocalization();

  if (!laneCtx.showPopup || laneCtx.laneData == undefined) {
    return <></>;
  }

  let inlineStyle: CSSProperties = {
    left: laneCtx.position.x + "px",
  };

  let onCopy = () => {
    duplicateLane(laneCtx.laneData?.Index ?? -1);
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
    <div className={styles.viewContainer} style={inlineStyle} onMouseLeave={laneCtx.close}>
      <div className={styles.view}>
        <div className={styles.topBar}>
          <div className={styles.title}>{laneCtx.laneData.NetSection?.DisplayName}</div>
          <Tooltip tooltip={translate("RoadBuilder.CopyLane")}>
            <div className={styles.copyButton} onClick={onCopy}>
              <img />
            </div>
          </Tooltip>
          <Tooltip tooltip={translate("RoadBuilder.DeleteLane")}>
            <div className={styles.deleteButton} onClick={onDelete}>
              <img />
            </div>
          </Tooltip>
        </div>
        <div className={styles.content}>
          <div className={styles.options}>
            {currentLane?.Options != undefined && currentLane?.Options?.length !== 0 ? (
              <OptionsPanelComponent
                OnChange={(x, y, z) => laneOptionClicked(currentLane.Index, x, y, z)}
                options={currentLane?.Options ?? new Array()}
              ></OptionsPanelComponent>
            ) : (
              <span>{translate("RoadBuilder.NoOptions")}</span>
            )}
          </div>
          <div className={styles.caret}></div>
        </div>
      </div>
    </div>
  );
};
