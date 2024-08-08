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
    let deltaBottom = bodySize.height - (dragCtx.mousePosition.y + halfPopupHeight + (120 * rem));
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
        {/* Pick Prefab */}
        <Button className={classNames(styles.pickerButton, styles.button)} onSelect={pickPrefab} variant="flat" focusKey={FOCUS_AUTO}>
          <img />
          {translate("Prompt[Picker]", "Place More")}
        </Button>
        {/* Use as Template */}
        <Button className={classNames(styles.templateButton, styles.button)} variant="flat" onSelect={createNewPrefab} focusKey={FOCUS_AUTO}>
          <img />
          {translate("Prompt[UseAsTemplate]", "Use As Template")}
        </Button>
        {/* Edit Prefab */}
        <Button className={classNames(styles.editButton, styles.button)} onSelect={editPrefab} variant="flat" focusKey={FOCUS_AUTO}>
          <img />
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
