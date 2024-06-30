import { ForwardedRef, MouseEventHandler, forwardRef, useCallback, useContext, useEffect, useImperativeHandle, useRef } from 'react';
import styles from './DragAndDropDivider.module.scss';
import { DragContext } from 'mods/Contexts/DragContext';
import classNames from 'classnames';
import { NetSectionItem } from 'domain/NetSectionItem';
import { MouseButtons, intersects } from 'mods/util';
import { RoadLane } from 'domain/RoadProperties';

type _Props = {
    listIdx: number;
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
            if (containerRef.current != undefined) {
                let bounds = containerRef.current.getBoundingClientRect();            
                return intersects(bounds, other);                                       
            }
            return false;
        };
        return {intersects: _intersects, listIdx: props.listIdx};
    }, [containerRef, props.listIdx]);

    return (
        <div className={containerClasses} ref={containerRef}>
            <div className={styles.divider}>
            </div>
        </div>        
    )
});