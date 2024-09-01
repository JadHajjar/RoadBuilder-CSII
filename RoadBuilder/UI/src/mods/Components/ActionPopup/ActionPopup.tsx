import classNames from "classnames";
import { Button, FOCUS_DISABLED, FOCUS_AUTO } from "cs2/ui";
import styles from "./ActionPopup.module.scss";
import { useLocalization } from "cs2/l10n";
import { CSSProperties, useContext, useMemo } from "react";
import { useRem } from "cs2/utils";
import { cancelActionPopup, createNewPrefab, editPrefab, pickPrefab } from "mods/bindings";
import { DragContext } from "mods/Contexts/DragContext";

export default () => {
  const { translate } = useLocalization();
  let rem = useRem();
  let dragCtx = useContext(DragContext);

  let popupPosition = useMemo(() => {
    let halfPopupWidth = (rem * 500) / 2;
    let halfPopupHeight = (rem * 200) / 2;
    let bodySize = document.body.getBoundingClientRect();
    let deltaRight = bodySize.width - (dragCtx.mousePosition.x + halfPopupWidth);
    let deltaLeft = dragCtx.mousePosition.x - halfPopupWidth;
    let deltaTop = dragCtx.mousePosition.y - halfPopupHeight;
    let deltaBottom = bodySize.height - (dragCtx.mousePosition.y + halfPopupHeight + 120 * rem);
    let nPos = dragCtx.mousePosition;
    if (deltaRight < 0 || deltaLeft < 0) {
      nPos = { ...nPos, x: nPos.x + Math.min(deltaRight, deltaLeft < 0 ? -deltaLeft : 0) };
    }
    if (deltaBottom < 0 || deltaTop < 0) {
      nPos = { ...nPos, y: nPos.y + Math.min(deltaBottom, deltaTop < 0 ? -deltaTop : 0) };
    }
    return nPos;
  }, []);

  let positionStyle: CSSProperties = {
    transform: `translate(${popupPosition.x}px, ${popupPosition.y}px)`,
  };

  return (
    <div className={styles.container}>
      <div className={styles.relContainer} style={positionStyle}>
        <div className={styles.pickerButton}>
          <Button className={styles.button} onSelect={pickPrefab} variant="flat" focusKey={FOCUS_AUTO}>
            <img />
            {translate("RoadBuilder.Picker", "Place More")}
          </Button>
        </div>
        <div className={styles.templateButton}>
          <Button className={styles.button} variant="flat" onSelect={createNewPrefab} focusKey={FOCUS_AUTO}>
            <img />
            {translate("RoadBuilder.UseAsTemplate", "Use As Template")}
          </Button>
        </div>
        <div className={styles.editButton}>
          <Button className={styles.button} onSelect={editPrefab} variant="flat" focusKey={FOCUS_AUTO}>
            <img />
            {translate("RoadBuilder.EditAllInstances", "Edit All Instances")}
          </Button>
        </div>
        <div className={styles.cancelButton}>
          <Button className={styles.button} onSelect={cancelActionPopup} variant="flat" focusKey={FOCUS_DISABLED}>
            {translate("RoadBuilder.Cancel", "Cancel")}
          </Button>
        </div>
      </div>
    </div>
  );
};
