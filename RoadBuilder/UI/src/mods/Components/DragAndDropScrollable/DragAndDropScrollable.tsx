import { Scrollable, ScrollableProps } from "cs2/ui"
import React from "react";
import { VanillaComponentResolver } from "vanillacomponentresolver"

import styles from './DragAndDropScrollable.module.scss'
import classNames from "classnames";

type _Props = React.PropsWithChildren<ScrollableProps> & {    
};

export const DragAndDropScrollable = (props: _Props) => {
    let scrollController = VanillaComponentResolver.instance.useScrollController();    

    let onMouseOver = (dir: 'left' | 'right') => () => {
        scrollController.scrollTo
    }

    return (
        <>
            <Scrollable {...props} className={classNames(props.className, styles.view)}>                
                {props.children}                
            </Scrollable>
            <div onMouseOver={onMouseOver('right')} className={styles.scrollButton + ' ' + styles.scrollRight}>{'>'}</div>
            <div onMouseOver={onMouseOver('left')} className={styles.scrollButton + ' ' + styles.scrollLeft}>{'<'}</div>
        </>        
    )
}