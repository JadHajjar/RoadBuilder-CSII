import { MouseEventHandler, useEffect, useState } from "react";
import { BottomView } from "../BottomView/BottomView"
import { LaneListPanel } from "../LaneListPanel/LaneListPanel"
import { DragContext, DragContextData } from 'mods/Contexts/DragContext';
import { NetSectionItem } from "domain/NetSectionItem";
import { Number2 } from "cs2/ui";
import { LaneListItemDrag } from "../Components/LaneListItem/LaneListItem";

import styles from './ModView.module.scss';

export const ModView = () => {    
    let [draggingItem, setDraggingItem] = useState<NetSectionItem | undefined>();
    let [mousePosition, setMousePosition] = useState<Number2>({x: 0, y: 0});
    
    let onNetSectionItemChange = (item?: NetSectionItem) => {
        setDraggingItem(item);       
    }

    let onMouseMove = (evt: MouseEvent) => {        
        if (draggingItem) {
            return;
        }        
        setMousePosition({x: evt.clientX, y: evt.clientY});
    }

    useEffect(() => {
        document.addEventListener('mousemove', onMouseMove);
        console.log("Setup event listener");
    }, []);

    let dragData : DragContextData = {
        onNetSectionItemChange: onNetSectionItemChange,
        mousePosition: mousePosition,
        netSectionItem: draggingItem
    };

    return (
        <DragContext.Provider value={dragData}>
            <div className={styles.view}>            
                    <LaneListPanel />
                    <BottomView />
                    <LaneListItemDrag />            
            </div>                      
        </DragContext.Provider>      
    )
}