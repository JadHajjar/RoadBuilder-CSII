import classNames from "classnames";
import { Button, Tooltip, FOCUS_DISABLED, FOCUS_AUTO, Number2 } from "cs2/ui";
import { trigger, useValue, bindValue } from "cs2/api";
import mod from "mod.json";
import styles from "./ActionPopup.module.scss";
import { useLocalization } from "cs2/l10n";
import { CSSProperties } from "react";
import { cancelPickerAction, createFromScratch, createFromTemplate } from "mods/bindings";

export default (props: {popupPosition: Number2}) => {
  const { translate } = useLocalization();  

  let positionStyle : CSSProperties = {
    transform:  `translate(${props.popupPosition?.x}px, ${props.popupPosition?.y}px)`
  };

  return (
    <div className={styles.container}>
      <div className={styles.relContainer} style={positionStyle}>
        
        <Tooltip tooltip={translate(`Tooltip.LABEL[${mod.id}.ClosePanel]`, "Close Panel")}>
          <Button className={styles.closeIcon} variant="icon" onSelect={cancelPickerAction} focusKey={FOCUS_DISABLED}>
            <img style={{ maskImage: "url(Media/Glyphs/Close.svg)" }}></img>
          </Button>
        </Tooltip>        

        {/* Use as Template */}
        <Tooltip tooltip="">
          <Button className={classNames(styles.templateButton, styles.button)} variant="flat" onSelect={createFromTemplate} focusKey={FOCUS_AUTO}>
            Use as Template
          </Button>        
        </Tooltip>          
        {/* Create from Scratch */}
        <Tooltip tooltip="">      
          <Button className={classNames(styles.createButton, styles.button)} onSelect={createFromScratch} variant="flat" focusKey={FOCUS_AUTO}>
            Create New Prefab
          </Button>
        </Tooltip>         
        {/* Cancel Button */}
        <Tooltip tooltip="">  
          <Button className={classNames(styles.cancelButton, styles.button)} onSelect={cancelPickerAction} variant="flat" focusKey={FOCUS_DISABLED}>
            Cancel
          </Button>
        </Tooltip>         
      </div>
    </div>
  );
};
