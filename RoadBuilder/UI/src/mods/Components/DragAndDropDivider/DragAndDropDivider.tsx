import { ForwardedRef, MouseEventHandler, forwardRef, useContext, useEffect, useRef } from 'react';
import styles from './DragAndDropDivider.module.scss';
import { DragContext } from 'mods/Contexts/DragContext';
import classNames from 'classnames';
import { NetSectionItem } from 'domain/NetSectionItem';
import { MouseButtons, intersects } from 'mods/util';

type _Props = {
    listIdx: number;
    onAddItem: (item: NetSectionItem, index: number) => void;
    onEvaluateDragAndDrop: () => void;
    first?: boolean;
    last?: boolean;
}
export const DragAndDropDivider = (props: _Props) => {    
    let dragContext = useContext(DragContext);    
    let containerRef = useRef<HTMLDivElement>(null);

    let containerClasses = classNames(
        styles.container,
        {
            [styles.hidden]: dragContext.netSectionItem === undefined
        }
    );

    useEffect(() => {
        if (dragContext.mouseReleased && dragContext.netSectionItem && dragContext.dragElement && containerRef.current) {
            let bounds = dragContext.dragElement.getBoundingClientRect();
            let bounds2 = containerRef.current.getBoundingClientRect();            
            if (intersects(bounds, bounds2)) {                
                props.onAddItem(dragContext.netSectionItem, props.listIdx);
                dragContext.onNetSectionItemChange(undefined);
            }            
            props.onEvaluateDragAndDrop();
        }
    }, [dragContext.mouseReleased]);

    return (
        <div className={containerClasses} ref={containerRef}>
            <div className={styles.divider}>
            </div>
        </div>        
    )
};