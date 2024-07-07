import { FormattedText, Tooltip } from 'cs2/ui';
import styles from './DeleteAreaDnD.module.scss';
import { useLocalization } from 'cs2/l10n';
import { useContext, useEffect, useState } from 'react';
import { DragContext } from 'mods/Contexts/DragContext';
import classNames from 'classnames';

export const DeleteAreaDnD = (props: {onRemove: (index: number) => void}) => {
    let dragCtx = useContext(DragContext);
    let localize = useLocalization();
    let [hovered, setHovered] = useState(false);
    let tooltip = localize.translate("Prompt[DragToDelete]", "Drag Here to Remove")!;
    let isDragging = (dragCtx.roadLane || dragCtx.netSectionItem) !== undefined;
    let classes = classNames(styles.area, {[styles.hidden]: !isDragging})

    useEffect(() => {
        console.log(`Released! ${hovered}`);
        if (dragCtx.mouseReleased && hovered) {
            props.onRemove(dragCtx.oldIndex!);
        }
    }, [dragCtx.mouseReleased])
    return (
        <div className={classes}>
            <Tooltip tooltip={tooltip} disabled={!isDragging}>
                <div className={styles.target} onMouseEnter={setHovered.bind(null, true)} onMouseLeave={setHovered.bind(null, true)}>
                    <img src='coui://gameui/Media/Glyphs/Trash.svg'></img>
                </div>            
            </Tooltip>            
        </div>
    )
}