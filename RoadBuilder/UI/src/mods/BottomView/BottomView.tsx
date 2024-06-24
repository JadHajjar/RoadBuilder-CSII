import styles from './BottomView.module.scss';
import { RoadButtonSmall } from '../Components/RoadButtonSmall/RoadButtonSmall';
import { DragAndDropDivider } from 'mods/Components/DragAndDropDivider/DragAndDropDivider';
import { Button, Number2 } from 'cs2/ui';
import { NetSectionItem } from 'domain/NetSectionItem';
import { range } from 'mods/util';
import { MouseEvent, useContext, useRef, useState } from 'react';
import { DragContext } from 'mods/Contexts/DragContext';
import { EditPropertiesPopup } from 'mods/Components/EditPropertiesPopup/EditPropertiesPopup';

export const BottomView = () => {

    let dragContext = useContext(DragContext);
    let [evaluationCount, setEvaluationCount] = useState(0);
    let [itemList, setItemList] = useState<NetSectionItem[]>([]);    
    let [selectedItem, setSelectedItem] = useState<number>(-1);
    let [propertiesPosition, setPropertiesPosition] = useState<Number2 | undefined>(); 

    let onAddItem = (item: NetSectionItem, index: number) => {        
        let nList = [
            ...itemList.slice(0, index),
            item,
            ...itemList.slice(index)
        ];
        setItemList(nList);        
    }
    let onEvaluateDragAndDrop = () => {        
        if (evaluationCount + 1 == itemList.length + 1) {
            dragContext.onNetSectionItemChange(undefined);
            setEvaluationCount(0);
        } else {
            setEvaluationCount(evaluationCount + 1);
        }
    }
    let onClickItem = (index: number, evt: any) => {
        setSelectedItem(index);
        let boundRect = evt.target.getBoundingClientRect();                
        let propertyPos = {
            x: evt.target.offsetLeft, 
            y: evt.currentTarget.clientTop
        };
        console.log(propertyPos);
        setPropertiesPosition(propertyPos);
    }

    let onDeleteItem = (idx: number) => {
        setItemList([
            ...itemList.slice(0, idx),
            ...itemList.slice(Math.min(idx+1, itemList.length))
        ]);
    }

    let items = itemList.flatMap((val, idx) => {        
        return [
            <DragAndDropDivider onAddItem={onAddItem} key={idx*2} listIdx={idx} onEvaluateDragAndDrop={onEvaluateDragAndDrop}/>,
            <RoadButtonSmall index={idx} onClick={onClickItem} onDelete={onDeleteItem} item={val} key={idx * 2 + 1} />
        ]
    });
    items.push(
        <DragAndDropDivider 
            onAddItem={onAddItem} 
            key={items.length} listIdx={itemList.length}
            onEvaluateDragAndDrop={onEvaluateDragAndDrop} />
    )
    
    return (
        <div className={styles.viewContainer}>
            <div className={styles.view}>
            {items}
            {itemList.length == 0 && !dragContext.netSectionItem? <div className={styles.hint}>Drag Lanes Here</div> : <></>}
            {/* <Button className={styles.closeButton} variant='icon' src='Media/Glyphs/Close.svg' />             */}
        </div>
        </div>        
    )

}