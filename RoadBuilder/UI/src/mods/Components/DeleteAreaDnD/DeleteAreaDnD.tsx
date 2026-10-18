import styles from "./DeleteAreaDnD.module.scss";
import { useLocalization } from "cs2/l10n";
import { useContext } from "react";
import { DragContext } from "mods/Contexts/DragContext";
import classNames from "classnames";
import { useDropArea } from "domain/DragAndDrop/DropArea";

export const DeleteAreaDnD = (props: { onRemove: (index: number) => void; editor: boolean }) => {
  let dragCtx = useContext(DragContext);
  let localize = useLocalization();
  let label = localize.translate("Prompt[DragToDelete]", "Drag Here to Remove")!;
  let isDragging = dragCtx.roadLane !== undefined;

  let onDrop = () => {
    props.onRemove(dragCtx.oldIndex!);
  };
  let [areaRef, _] = useDropArea<HTMLDivElement>({ onDrop });
  return (
    <div className={classNames(styles.area, { [styles.hidden]: !isDragging }, props.editor ? styles.editor : styles.game)}>
      {/* The hint is shown inside the drop zone, a tooltip would cover the lane bar above */}
      <div className={styles.target} ref={areaRef}>
        <div className={styles.icon} style={{ maskImage: "url(Media/Glyphs/Trash.svg)" }}></div>
        <span className={styles.label}>{label}</span>
      </div>
    </div>
  );
};
