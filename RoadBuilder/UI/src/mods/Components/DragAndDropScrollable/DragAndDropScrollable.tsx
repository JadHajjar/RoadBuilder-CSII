import { Scrollable, ScrollableProps } from "cs2/ui"
import React, { useCallback, useEffect, useRef, useState } from "react";
import { VanillaComponentResolver } from "vanillacomponentresolver"

import styles from './DragAndDropScrollable.module.scss'
import classNames from "classnames";
import { useRem } from "cs2/utils";

type _Props = React.PropsWithChildren<ScrollableProps> & {    
};

enum ArrowState {
    None = 0,
    Left = 1,
    Right = 2
}

export const DragAndDropScrollable = (props: _Props) => {
    let scrollController = VanillaComponentResolver.instance.useScrollController();    
    let [scrollRoutine, setScrollRoutine] = useState<number>();
    let [isScrollPosInit, setIsScrollPosInit] = useState(false);
    let scrollRef = useRef<HTMLDivElement>(null);
    let rem = useRem();
    const baseScrollSpeed = 10 * rem;
    const wheelScrollMultiplier = 0.66;
    let [arrowState, setArrowState] = useState<ArrowState>(ArrowState.None);

    let onMouseOver = (dir: 'left' | 'right') => () => {        
        if (!scrollRef.current) {
            return;
        }
        let dx = baseScrollSpeed * (dir == 'left'? -1 : 1);        
        setScrollRoutine(setInterval(() => {         
            scrollController.scrollBy(dx, 0);            
        }, Math.round(1000/30)));                
    }

    let updateArrowState = () => {
        if (scrollRef.current == null) {            
            arrowState = ArrowState.Left | ArrowState.Right;
            return;       
        }   
        let maxScrollLeft = scrollRef.current.scrollWidth - scrollRef.current.clientWidth;
        let scrollLeft = scrollRef.current?.scrollLeft;
        let nState = ArrowState.None;
        if (scrollLeft > 0) {             
            nState |= ArrowState.Left;
        }            
        if (scrollLeft < maxScrollLeft - 1) {
            nState |= ArrowState.Right;
        }
        setArrowState(nState);
    }

    let onScrollWheel = (evt: WheelEvent) => {
        evt.preventDefault();
        scrollController.scrollBy(evt.deltaY * wheelScrollMultiplier, 0);
    }

    // On Mount, on Detach
    useEffect(() => {
        return () => {
            if(scrollRef.current) {
                scrollRef.current.removeEventListener('wheel', onScrollWheel);
            }
        }
    }, [])

    useEffect(() => {
        if (scrollController && scrollRef.current && !isScrollPosInit) {
            let maxScrollLeft = scrollRef.current.scrollWidth - scrollRef.current.clientWidth;            
            if (maxScrollLeft > 0) {
                scrollController.scrollTo(maxScrollLeft/2, 0);            
                scrollRef.current.addEventListener('wheel', onScrollWheel);
                setIsScrollPosInit(true);
            }            
        }        
        updateArrowState();
    }, [scrollRef.current, props.children]);

    let onOverflowX = useCallback((overflow: boolean) => {             
        updateArrowState();
    }, [scrollRef]);

    let stopScroll = () => {
        if (scrollRoutine) {
            clearInterval(scrollRoutine);
            setScrollRoutine(undefined);
        }
    }

    return (
        <>
            <Scrollable {...props} onScroll={updateArrowState} ref={scrollRef} onOverflowX={onOverflowX} smooth={true} className={classNames(props.className, styles.view)} controller={scrollController}>                
                {props.children}                
            </Scrollable>
            <div data-hidden={(arrowState & ArrowState.Right) == 0} onMouseEnter={onMouseOver('right')} onMouseLeave={stopScroll} className={classNames(styles.scrollButton, styles.scrollRight)}></div>
            <div data-hidden={(arrowState & ArrowState.Left) == 0} onMouseEnter={onMouseOver('left')} onMouseLeave={stopScroll} className={styles.scrollButton + ' ' + styles.scrollLeft}></div>
        </>        
    )
}