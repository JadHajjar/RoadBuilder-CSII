import { FormattedText, Tooltip } from "cs2/ui";
import styles from "./DeleteAreaDnD.module.scss";
import { useLocalization } from "cs2/l10n";
import { useContext, useEffect, useState } from "react";
import { DragContext } from "mods/Contexts/DragContext";
import classNames from "classnames";
import { useDropArea } from "domain/DragAndDrop/DropArea";

export const DeleteAreaDnD = (props: { onRemove: (index: number) => void }) => {
  let dragCtx = useContext(DragContext);
  let localize = useLocalization();
  let tooltip = localize.translate("Prompt[DragToDelete]", "Drag Here to Remove")!;
  let isDragging = dragCtx.roadLane !== undefined;
  let classes = classNames(styles.area, { [styles.hidden]: !isDragging });

  let onDrop = () => {
    props.onRemove(dragCtx.oldIndex!);
  };
  let [areaRef, _] = useDropArea<HTMLDivElement>({ onDrop });
  return (
    <div className={classes}>
      <Tooltip tooltip={tooltip} disabled={!isDragging}>
        <div className={styles.target} ref={areaRef}>
          <img src="Media/Glyphs/Trash.svg"></img>
        </div>
      </Tooltip>
    </div>
  );
};
