import { ForwardedRef, MouseEventHandler, forwardRef, useContext, useEffect, useRef } from 'react';
import styles from './DragAndDropDivider.module.scss';
import { DragContext } from 'mods/Contexts/DragContext';
import classNames from 'classnames';
import { NetSectionItem } from 'domain/NetSectionItem';
import { MouseButtons, intersects } from 'mods/util';
import { RoadLane } from 'domain/RoadProperties';

type _Props = {
    listIdx: number;
    onAddItem: (item: NetSectionItem, index: number) => void;
    onEvaluateDragAndDrop: (idx: number) => void;
    onMoveLane: (lane: RoadLane, oldIndex: number, newIndex: number) => void;
    first?: boolean;
    last?: boolean;
}
export const DragAndDropDivider = (props: _Props) => {    
    let dragContext = useContext(DragContext);    
    let containerRef = useRef<HTMLDivElement>(null);

    let containerClasses = classNames(
        styles.container,
        {
            [styles.hidden]: !dragContext.netSectionItem && !dragContext.roadLane
        }
    );

    useEffect(() => {
        let isDragging = dragContext.netSectionItem || dragContext.roadLane;  
        if (dragContext.mouseReleased && dragContext.dragElement != undefined && isDragging && containerRef.current != undefined) {
            let bounds = dragContext.dragElement.getBoundingClientRect();
            let bounds2 = containerRef.current.getBoundingClientRect();            
            if (intersects(bounds, bounds2)) {                
                if (dragContext.roadLane) {
                    props.onMoveLane(
                        dragContext.roadLane, 
                        dragContext.oldIndex!, 
                        props.listIdx
                    );
                } else if (dragContext.netSectionItem) {
                    props.onAddItem(dragContext.netSectionItem, props.listIdx);
                }                
                dragContext.onNetSectionItemChange(undefined);
                dragContext.setRoadLane(undefined, undefined);
            }                     
            props.onEvaluateDragAndDrop(props.listIdx);
        }
    }, [dragContext.mouseReleased]);

    return (
        <div className={containerClasses} ref={containerRef}>
            <div className={styles.divider}>
            </div>
        </div>        
    )
};