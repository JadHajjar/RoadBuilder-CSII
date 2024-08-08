import { useMemo } from "react";
import { BottomView } from "../BottomView/BottomView";
import { SidePanel } from "mods/SidePanel/SidePanel";
import { DragContextManager } from "mods/Contexts/DragContext";
import { NetSectionItem } from "domain/NetSectionItem";
import RB_ClickOnRoad from "images/RB_ClickOnRoad.svg";

import styles from "./ModView.module.scss";
import { useValue } from "cs2/api";
import { RoadBuilderToolModeEnum } from "domain/RoadBuilderToolMode";
import { allNetSections$, roadBuilderToolMode$ } from "mods/bindings";
import ActionPopup from "mods/Components/ActionPopup/ActionPopup";
import { NetSectionsStoreContext } from "mods/Contexts/NetSectionsStore";
import { RoadPropertiesPanel } from "mods/RoadPropertiesPanel/RoadPropertiesPanel";
import { LanePropertiesContextManager } from "mods/Contexts/LanePropertiesContext";
import { useLocalization } from "cs2/l10n";

export const ModView = () => {
  const roadBuilderToolMode = useValue(roadBuilderToolMode$);
  let { translate } = useLocalization();
  let allNetSections = useValue(allNetSections$);    

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
        <LanePropertiesContextManager>
          <div className={styles.view}>{content}</div>
        </LanePropertiesContextManager>
      </NetSectionsStoreContext.Provider>    
    </DragContextManager>
  );
};
