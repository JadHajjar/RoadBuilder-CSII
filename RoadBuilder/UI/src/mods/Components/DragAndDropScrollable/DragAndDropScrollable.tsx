import { Scrollable, ScrollableProps } from "cs2/ui"
import React, { useState } from "react";
import { VanillaComponentResolver } from "vanillacomponentresolver"

import styles from './DragAndDropScrollable.module.scss'
import classNames from "classnames";
import { useRem } from "cs2/utils";

type _Props = React.PropsWithChildren<ScrollableProps> & {    
};

export const DragAndDropScrollable = (props: _Props) => {
    let scrollController = VanillaComponentResolver.instance.useScrollController();    
    let [scrollRoutine, setScrollRoutine] = useState<number>();
    let rem = useRem();

    let onMouseOver = (dir: 'left' | 'right') => () => {        
        let dx = 8 * rem * (dir == 'left'? -1 : 1);
        setScrollRoutine(setInterval(() => {
            scrollController.scrollBy(dx, 0);
        }, 1000/30));                
    }

    let onMouseExit = () => {
        if (scrollRoutine) {
            clearInterval(scrollRoutine);
            setScrollRoutine(undefined);
        }
    }

    return (
        <>
            <Scrollable {...props} className={classNames(props.className, styles.view)} controller={scrollController}>                
                {props.children}                
            </Scrollable>
            <div onMouseEnter={onMouseOver('right')} onMouseLeave={onMouseExit} className={styles.scrollButton + ' ' + styles.scrollRight}></div>
            <div onMouseEnter={onMouseOver('left')} onMouseLeave={onMouseExit} className={styles.scrollButton + ' ' + styles.scrollLeft}></div>
        </>        
    )
}