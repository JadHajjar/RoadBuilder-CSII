import { NetSectionItem } from "domain/NetSectionItem";
import styles from "./EditPropertiesPopup.module.scss";
import { Button, Number2 } from "cs2/ui";
import { CSSProperties, useContext } from "react";
import { useRem } from "cs2/utils";
import { RoadLane } from "domain/RoadProperties";
import { OptionsPanelComponent } from "../OptionsPanel/OptionsPanel";
import { LanePropertiesContext } from "mods/Contexts/LanePropertiesContext";
import { NetSectionsStoreContext } from "mods/Contexts/NetSectionsStore";
import { useValue } from "cs2/api";
import { roadLanes$, setRoadLanes } from "mods/bindings";
import { removeAt } from "mods/util";

export const EditPropertiesPopup = () => {
  let rem = useRem();
  let laneCtx = useContext(LanePropertiesContext);  
  let netSectionsCtx = useContext(NetSectionsStoreContext);
  let roadLanes = useValue(roadLanes$);
  
  if (!laneCtx.showPopup || laneCtx.laneData == undefined) {
    return (<></>);
  }
  
  let netSectionItem = netSectionsCtx[laneCtx.laneData!.SectionPrefabName];
  let inlineStyle : CSSProperties = {
    left: laneCtx.position.x + 'px'
  };

  let onDelete = () => {        
    laneCtx.close();
    let nList = removeAt(roadLanes, laneCtx.index);
    setRoadLanes(nList);
  }

  return (
    <div className={styles.view} style={inlineStyle} onMouseLeave={laneCtx.close}>
      <div className={styles.topBar}>
        <div className={styles.title}>{netSectionItem.DisplayName}</div>
        <Button className={styles.deleteButton} onSelect={onDelete} variant="icon" />
      </div>
      <div className={styles.content}>
        <div className={styles.options}>
          {laneCtx.laneData.Options && laneCtx.laneData.Options.length !== 0 ? (
            <OptionsPanelComponent Index={laneCtx.laneData.Index} options={laneCtx.laneData.Options}></OptionsPanelComponent>
          ) : (
            <span> No Options Available</span>
          )}
        </div>        
        <div className={styles.caret}></div>
        <div className={styles.hoverDetector}></div>
      </div>
    </div>
  );
};
