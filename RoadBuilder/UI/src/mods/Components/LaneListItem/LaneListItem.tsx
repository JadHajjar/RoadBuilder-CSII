import { Number2, Tooltip } from 'cs2/ui';
import styles from './LaneListItem.module.scss';
import { NetSectionItem } from 'domain/NetSectionItem';
import { CSSProperties, useContext, useState } from 'react';
import classNames from 'classnames';
import { DragContext } from 'mods/Contexts/DragContext';

export const LaneListItem = ({netSection} : {netSection: NetSectionItem}) => {
    // let [dragging, setDragging] = useState(false);
    let dragContext = useContext(DragContext);

    let dragging = dragContext.netSectionItem?.PrefabName == netSection.PrefabName;
    let containerStyles: Record<string, boolean> = {};
    containerStyles[(styles.moving as string)] = dragging;    

    let onClick = () => {        
        if (!dragging) {
            dragContext.onNetSectionItemChange(netSection);
        } else {
            dragContext.onNetSectionItemChange(undefined);
        }        
    }

    return (
        <Tooltip tooltip={netSection.DisplayName}>
            <div onClick={onClick} className={classNames(styles.container, containerStyles)}>
                <div className={styles.item  + ' ' + (dragging? styles.moving : '')}>
                    <img className={styles.image} src={netSection.Thumbnail}/>
                    <div className={styles.label}>
                        {netSection.DisplayName}
                    </div>
                </div>
            </div>
        </Tooltip>
        
    )
}

export const LaneListItemDrag = () => {
    let dragData = useContext(DragContext);        
    let [position, setPosition] = useState<Number2>({x: 0, y: 0});

    if (dragData.netSectionItem == undefined) {
        return (<></>)
    }

    let netSection = dragData.netSectionItem;
    let offsetStyle : CSSProperties = {
        left: `calc( ${dragData.mousePosition.x}px - 66rem)`,
        top: `calc( ${dragData.mousePosition.y}px - 40rem)`
    }

    return (
        <div style={offsetStyle} className={classNames(styles.item, styles.dragRepresentation)}>
            <img className={styles.image} src={netSection.Thumbnail}/>
            <div className={styles.label}>
                {netSection.DisplayName}
            </div>
        </div>
    )

}