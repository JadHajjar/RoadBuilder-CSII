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
    // Kept in refs so the interval and listener can always be cleaned up, even after re-renders or unmount
    let scrollRoutine = useRef<number>();
    let wheelTarget = useRef<HTMLDivElement | null>(null);
    let [isScrollPosInit, setIsScrollPosInit] = useState(false);
    let scrollRef = useRef<HTMLDivElement>(null);
    let rem = useRem();
    const baseScrollSpeed = 10 * rem;
    const wheelScrollMultiplier = 0.66;
    let [arrowState, setArrowState] = useState<ArrowState>(ArrowState.None);

    let stopScroll = () => {
        if (scrollRoutine.current !== undefined) {
            clearInterval(scrollRoutine.current);
            scrollRoutine.current = undefined;
        }
    }

    let onMouseOver = (dir: 'left' | 'right') => () => {
        if (!scrollRef.current) {
            return;
        }
        let dx = baseScrollSpeed * (dir == 'left'? -1 : 1);
        stopScroll();
        scrollRoutine.current = setInterval(() => {
            scrollController.scrollBy(dx, 0);
        }, Math.round(1000/30));
    }

    let updateArrowState = () => {
        if (scrollRef.current == null) {
            setArrowState(ArrowState.Left | ArrowState.Right);
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

    // A single stable listener, so the one removed on unmount is the same one that was added
    let scrollControllerRef = useRef(scrollController);
    scrollControllerRef.current = scrollController;

    let onScrollWheel = useRef((evt: WheelEvent) => {
        evt.preventDefault();
        scrollControllerRef.current.scrollBy(evt.deltaY * wheelScrollMultiplier, 0);
    }).current;

    // On Mount, on Detach
    useEffect(() => {
        return () => {
            stopScroll();
            wheelTarget.current?.removeEventListener('wheel', onScrollWheel);
            wheelTarget.current = null;
        }
    }, [])

    useEffect(() => {
        if (scrollController && scrollRef.current && !isScrollPosInit) {
            let maxScrollLeft = scrollRef.current.scrollWidth - scrollRef.current.clientWidth;
            if (maxScrollLeft > 0) {
                scrollController.scrollTo(maxScrollLeft/2, 0);
                // The listener added here is the one removed on unmount, the element is remembered since the ref is cleared first
                wheelTarget.current = scrollRef.current;
                wheelTarget.current.addEventListener('wheel', onScrollWheel);
                setIsScrollPosInit(true);
            }
        }
        updateArrowState();
    }, [scrollRef.current, props.children]);

    let onOverflowX = useCallback((overflow: boolean) => {
        updateArrowState();
    }, [scrollRef]);

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