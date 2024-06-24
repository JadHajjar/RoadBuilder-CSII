import classNames from "classnames";
import { Button, Tooltip, FOCUS_DISABLED, FOCUS_AUTO, Number2 } from "cs2/ui";
import { trigger, useValue, bindValue } from "cs2/api";
import mod from "mod.json";
import styles from "./ActionPopup.module.scss";
import { useLocalization } from "cs2/l10n";
import { CSSProperties } from "react";
import { cancelPickerAction, createFromScratch, createFromTemplate, toggleTool } from "mods/bindings";
import { useRem } from "cs2/utils";

export default (props: {popupPosition: Number2}) => {
  const { translate } = useLocalization();    

  let positionStyle : CSSProperties = {
    transform:  `translate(${props.popupPosition?.x}px, ${props.popupPosition?.y}px)`
  };

  return (
    <div className={styles.container}>
      <div className={styles.relContainer} style={positionStyle}>        
        {/* Use as Template */}        
        <Button className={classNames(styles.templateButton, styles.button)} variant="flat" onSelect={createFromTemplate} focusKey={FOCUS_AUTO}>
          Use as Template
        </Button>                
        {/* Create from Scratch */}        
        <Button className={classNames(styles.createButton, styles.button)} onSelect={createFromScratch} variant="flat" focusKey={FOCUS_AUTO}>
          Create New Prefab
        </Button>      
        {/* Cancel Button */}        
        <Button className={classNames(styles.cancelButton, styles.button)} onSelect={() => {cancelPickerAction(); toggleTool();}} variant="flat" focusKey={FOCUS_DISABLED}>
          Cancel
        </Button>        
      </div>
    </div>
  );
};
