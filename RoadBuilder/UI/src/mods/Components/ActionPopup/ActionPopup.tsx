import classNames from "classnames";
import { Button, Tooltip, FOCUS_DISABLED, FOCUS_AUTO, Number2 } from "cs2/ui";
import { trigger, useValue, bindValue } from "cs2/api";
import mod from "mod.json";
import styles from "./ActionPopup.module.scss";
import { useLocalization } from "cs2/l10n";
import { CSSProperties } from "react";
import { useRem } from "cs2/utils";
import { cancelActionPopup, createNewPrefab, editPrefab, IsCustomRoadSelected$ } from "mods/bindings";

export default (props: {popupPosition: Number2}) => {
  const { translate } = useLocalization();  
  let isCustomRoadSelected = useValue(IsCustomRoadSelected$);  

  let positionStyle : CSSProperties = {
    transform:  `translate(${props.popupPosition?.x}px, ${props.popupPosition?.y}px)`
  };
  
  return (
    <div className={styles.container}>
      <div className={styles.relContainer} style={positionStyle}>        
        {/* Use as Template */}        
        <Button className={classNames(styles.templateButton, styles.button)} variant="flat" onSelect={createNewPrefab} focusKey={FOCUS_AUTO}>
          {translate("Prompt[UseAsTemplate]", "Use As Template")}
        </Button>                
        {/* Edit Prefab */}        
        <Button className={classNames(styles.editButton, styles.button, {[styles.hidden]: !isCustomRoadSelected})} onSelect={editPrefab} variant="flat" focusKey={FOCUS_AUTO}>
          {translate("Prompt[EditAllInstances]", "Edit All Instances")}
        </Button>
        {/* Cancel Button */}        
        <Button className={classNames(styles.cancelButton, styles.button)} onSelect={cancelActionPopup} variant="flat" focusKey={FOCUS_DISABLED}>
          {translate("Common.ACTION[Cancel]", "Cancel")}
        </Button>        
      </div>
    </div>
  );
};
