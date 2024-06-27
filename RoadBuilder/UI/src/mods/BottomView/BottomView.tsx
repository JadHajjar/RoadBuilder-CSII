import styles from './BottomView.module.scss';
import { RoadButtonSmall } from '../Components/RoadButtonSmall/RoadButtonSmall';
import { DragAndDropDivider } from 'mods/Components/DragAndDropDivider/DragAndDropDivider';
import { Button, Number2 } from 'cs2/ui';
import { NetSectionItem } from 'domain/NetSectionItem';
import { range } from 'mods/util';
import { CSSProperties, MouseEvent, useCallback, useContext, useEffect, useRef, useState } from 'react';
import { DragContext } from 'mods/Contexts/DragContext';
import { tool } from 'cs2/bindings';
import { useValue } from 'cs2/api';
import { clearTool, createNewPrefab, setRoadLanes, roadBuilderToolMode$, roadLanes$, allNetSections$ } from 'mods/bindings';
import { RoadBuilderToolModeEnum } from 'domain/RoadBuilderToolMode';
import { RoadLane } from 'domain/RoadProperties';
import { NetSectionsStoreContext } from 'mods/Contexts/NetSectionsStore';

export const BottomView = () => {
    let dragContext = useContext(DragContext);
    let toolMode = useValue(roadBuilderToolMode$);
    let roadLanes = useValue(roadLanes$);
    let [foo, setFoo] = useState<number[]>([]);
    let allNetSections = useValue(allNetSections$);
    let [evaluationCount, setEvaluationCount] = useState(0);            
    let netSectionData = useContext(NetSectionsStoreContext);

    let onMoveLane = (lane: RoadLane, oldIndex: number, newIndex: number) => {
        let nList = [
            ...roadLanes.slice(0, oldIndex),
            ...roadLanes.slice(Math.min(oldIndex+1, roadLanes.length))
        ];
        let insertIndex = oldIndex < newIndex? newIndex - 1 : newIndex;
        insertIndex = Math.min(insertIndex, nList.length);
        insertIndex = Math.max(insertIndex, 0);
        nList = [
            ...nList.slice(0, insertIndex),
            lane,
            ...nList.slice(insertIndex)
        ];
    }

    let onAddItem = (item: NetSectionItem, index: number) => { 
        let rLane : RoadLane = {
            SectionPrefabName: item.PrefabName,
            Invert: false            
        };
        let nList = [
            ...roadLanes.slice(0, index),
            rLane,
            ...roadLanes.slice(index)
        ];
        setRoadLanes(nList);        
    }
    console.log(foo);
    let onEvaluateDragAndDrop = (idx: number) => {                        
        if (evaluationCount + 1 == roadLanes.length + 1) {
            dragContext.onNetSectionItemChange(undefined);
            dragContext.setRoadLane(undefined, undefined);            
            console.log("EVAL CLEAR");
            setEvaluationCount(0);
        } else {
            console.log("EVAL", idx, evaluationCount, roadLanes.length);
            setEvaluationCount(idx);
            setFoo([...foo, idx]);
        }        
    };
    // let onClickItem = (index: number, evt: any) => {
    //     setSelectedItem(index);
    //     let boundRect = evt.target.getBoundingClientRect();                
    //     let propertyPos = {
    //         x: evt.target.offsetLeft, 
    //         y: evt.currentTarget.clientTop
    //     };
    //     console.log(propertyPos);
    //     setPropertiesPosition(propertyPos);
    // }

    let onDeleteItem = (idx: number) => {
        setRoadLanes([
            ...roadLanes.slice(0, idx),
            ...roadLanes.slice(Math.min(idx+1, roadLanes.length))
        ]);
    }

    let items = roadLanes.filter((val) => val.SectionPrefabName).flatMap((val, idx) => {              
        return [
            <DragAndDropDivider onMoveLane={onMoveLane} onAddItem={onAddItem} key={idx*2} listIdx={idx} onEvaluateDragAndDrop={onEvaluateDragAndDrop}/>,
            <RoadButtonSmall index={idx} onDelete={onDeleteItem} roadLane={val} key={idx * 2 + 1} />
        ]
    });
    items.push(
        <DragAndDropDivider 
            onMoveLane={onMoveLane}
            onAddItem={onAddItem} 
            key={items.length} listIdx={roadLanes.length}
            onEvaluateDragAndDrop={onEvaluateDragAndDrop} />
    )
    
    let copyButtonStyle : CSSProperties = {
        visibility: toolMode == RoadBuilderToolModeEnum.EditingSingle? 'hidden' : 'initial'
    };

    return (
        <div className={styles.viewContainer}>
            <div className={styles.view}>                
                {items}                
                {roadLanes.length == 0 && !dragContext.netSectionItem? <div className={styles.hint}>Drag Lanes Here</div> : <></>}                
            </div>            
            <div className={styles.bottomBG}>
                <Button style={copyButtonStyle} className={styles.copyButton} variant='flat' onSelect={createNewPrefab}>Copy to New Prefab</Button>
                <Button className={styles.closeButton} src='Media/Glyphs/Close.svg' variant='icon' onSelect={clearTool} />
            </div>
        </div>        
    )

}