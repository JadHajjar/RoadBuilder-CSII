import { startTransition, useContext, useEffect, useMemo, useRef, useState } from "react";
import { BottomView } from "../BottomView/BottomView";
import { SidePanel } from "mods/SidePanel/SidePanel";
import { DragContext, DragContextData, DragContextManager } from "mods/Contexts/DragContext";
import { NetSectionItem } from "domain/NetSectionItem";
import { Number2 } from "cs2/ui";
import { LaneListItemDrag } from "../Components/LaneListItem/LaneListItem";
import RB_ClickOnRoad from "images/RB_ClickOnRoad.svg";

import styles from "./ModView.module.scss";
import { MouseButtons } from "mods/util";
import { useValue } from "cs2/api";
import { RoadBuilderToolModeEnum } from "domain/RoadBuilderToolMode";
import { allNetSections$, roadBuilderToolMode$ } from "mods/bindings";
import ActionPopup from "mods/Components/ActionPopup/ActionPopup";
import { useRem } from "cs2/utils";
import { RoadLane } from "domain/RoadLane";
import { NetSectionsStoreContext } from "mods/Contexts/NetSectionsStore";
import { RoadPropertiesPanel } from "mods/RoadPropertiesPanel/RoadPropertiesPanel";
import { LanePropertiesContext, LanePropertiesContextData } from "mods/Contexts/LanePropertiesContext";
import { useLocalization } from "cs2/l10n";

export const ModView = () => {
  const roadBuilderToolMode = useValue(roadBuilderToolMode$);
  let { translate } = useLocalization();
  let allNetSections = useValue(allNetSections$);
  let defaultLanePropCtx = useContext(LanePropertiesContext);
  let [lanePropState, setLanePropState] = useState<LanePropertiesContextData>(defaultLanePropCtx);

  let lanePropCtx = useMemo(
    () => ({
      ...lanePropState,
      open(roadLane: RoadLane, index: number, position: Number2) {
        setLanePropState({
          ...lanePropState,
          position,
          index,
          laneData: roadLane,
          showPopup: true,
        });
      },
      close() {
        setLanePropState({
          ...lanePropState,
          showPopup: false,
        });
      },
    }),
    [lanePropState, setLanePropState]
  );

  let nStore = useMemo(() => {
    let nStore = allNetSections.reduce<Record<string, NetSectionItem>>((record: Record<string, NetSectionItem>, cVal: NetSectionItem, cIdx) => {
      record[cVal.PrefabName] = cVal;
      return record;
    }, {});
    return nStore;
  }, [allNetSections]);  

  let content: JSX.Element | null = null;
  switch (roadBuilderToolMode) {
    case RoadBuilderToolModeEnum.Picker:
      content = (
        <>
          <div className={styles.pickerHint}>
            <span>
              <img src={RB_ClickOnRoad} />
              {translate("Prompt[PickerHint]", "Click On A Road")}
            </span>
          </div>
          <SidePanel />
        </>
      );
      break;
    case RoadBuilderToolModeEnum.ActionSelection:
      content = (
        <>
          <ActionPopup />
        </>
      );
      break;
    case RoadBuilderToolModeEnum.Editing:
    case RoadBuilderToolModeEnum.EditingSingle:
    case RoadBuilderToolModeEnum.EditingNonExistent:
      content = (
        <>
          <SidePanel />
          <BottomView />
          <RoadPropertiesPanel />                    
        </>
      );
      break;
    default:
      return <></>;
  }
  return (
    <DragContextManager>    
      <NetSectionsStoreContext.Provider value={nStore}>
        <LanePropertiesContext.Provider value={lanePropCtx}>
          <div className={styles.view}>{content}</div>
        </LanePropertiesContext.Provider>
      </NetSectionsStoreContext.Provider>    
    </DragContextManager>
  );
};
