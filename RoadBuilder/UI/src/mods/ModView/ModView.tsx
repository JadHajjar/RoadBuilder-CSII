import { MouseEventHandler, useEffect, useState } from "react";
import { BottomView } from "../BottomView/BottomView"
import { LaneListPanel } from "../LaneListPanel/LaneListPanel"
import { DragContext, DragContextData } from 'mods/Contexts/DragContext';
import { NetSectionItem } from "domain/NetSectionItem";
import { Number2 } from "cs2/ui";
import { LaneListItemDrag } from "../Components/LaneListItem/LaneListItem";

import styles from './ModView.module.scss';
import { MouseButtons } from "mods/util";

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

    let onMouseDown = (evt: MouseEvent) => {
        
    }

    let onMouseRelease = (evt: MouseEvent) => {
        if (evt.button == MouseButtons.Primary) {
            //TODO: send message to bottom view that a new lane has been added
            setDraggingItem(undefined);            
        }
        if (evt.button == MouseButtons.Secondary) {
            setDraggingItem(undefined);
        }
    }

    useEffect(() => {
        document.addEventListener('mousemove', onMouseMove);
        document.addEventListener('mousedown', onMouseDown);
        document.addEventListener('mouseup', onMouseRelease);
        console.log("Setup event listener");
        return () => {
            document.removeEventListener('mousemove', onMouseMove);
            document.removeEventListener('mousedown', onMouseDown);
            document.removeEventListener('mouseup', onMouseRelease);
        }
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