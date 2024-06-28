import { ForwardedRef, MouseEventHandler, forwardRef, useCallback, useContext, useEffect, useImperativeHandle, useRef } from 'react';
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

export interface DragAndDropDividerRef {
    intersects: (other: DOMRect) => boolean;
    listIdx: number;
}

export const DragAndDropDivider = forwardRef<DragAndDropDividerRef, _Props>((props: _Props, ref) => {    
    let dragContext = useContext(DragContext);    
    let containerRef = useRef<HTMLDivElement>(null);      
    let containerClasses = classNames(
        styles.container,
        {
            [styles.hidden]: !dragContext.netSectionItem && !dragContext.roadLane
        }
    );

    useImperativeHandle(ref, () => {
        let _intersects = (other: DOMRect) => {
            let isDragging = dragContext.netSectionItem || dragContext.roadLane;  
            if (containerRef.current != undefined) {
                let bounds = other;
                let bounds2 = containerRef.current.getBoundingClientRect();            
                return intersects(bounds, bounds2);                                       
            }
            return false;
        };
        return {intersects: _intersects, listIdx: props.listIdx};
    }, [containerRef, props.listIdx]);

    // useEffect(() => {
    //     let isDragging = dragContext.netSectionItem || dragContext.roadLane;  
    //     if (dragContext.mouseReleased && dragContext.dragElement != undefined && isDragging && containerRef.current != undefined) {
    //         let bounds = dragContext.dragElement.getBoundingClientRect();
    //         let bounds2 = containerRef.current.getBoundingClientRect();            
    //         if (intersects(bounds, bounds2)) {                
    //             if (dragContext.roadLane) {
    //                 props.onMoveLane(
    //                     dragContext.roadLane, 
    //                     dragContext.oldIndex!, 
    //                     props.listIdx
    //                 );
    //             } else if (dragContext.netSectionItem) {
    //                 props.onAddItem(dragContext.netSectionItem, props.listIdx);
    //             }                
    //             dragContext.onNetSectionItemChange(undefined);
    //             dragContext.setRoadLane(undefined, undefined);
    //         }                     
    //         props.onEvaluateDragAndDrop(props.listIdx);
    //     }
    // }, [dragContext.mouseReleased]);

    return (
        <div className={containerClasses} ref={containerRef}>
            <div className={styles.divider}>
            </div>
        </div>        
    )
});