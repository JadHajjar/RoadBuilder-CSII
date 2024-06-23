import styles from './BottomView.module.scss';
import { RoadButtonSmall } from '../Components/RoadButtonSmall/RoadButtonSmall';
import { DragAndDropDivider } from 'mods/Components/DragAndDropDivider/DragAndDropDivider';
import { Button } from 'cs2/ui';
import { NetSectionItem } from 'domain/NetSectionItem';
import { range } from 'mods/util';
import { useContext, useRef, useState } from 'react';
import { DragContext } from 'mods/Contexts/DragContext';

export const BottomView = () => {

    let dragContext = useContext(DragContext);
    let [evaluationCount, setEvaluationCount] = useState(0);
    let [itemList, setItemList] = useState<NetSectionItem[]>([]);    

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

    let items = itemList.flatMap((val, idx) => {        
        return [
            <DragAndDropDivider onAddItem={onAddItem} key={idx*2} listIdx={idx} onEvaluateDragAndDrop={onEvaluateDragAndDrop}/>,
            <RoadButtonSmall item={val} key={idx * 2 + 1} />
        ]
    });
    items.push(
        <DragAndDropDivider 
            onAddItem={onAddItem} 
            key={items.length} listIdx={itemList.length}
            onEvaluateDragAndDrop={onEvaluateDragAndDrop} />
    )
    
    return (
        <div className={styles.view}>
            {items}
            {/* <RoadButtonSmall />
            <DragAndDropDivider onAddItem={onAddItem} ref={}/>
            <RoadButtonSmall /> */}
            <Button className={styles.closeButton} variant='flat'>X</Button>
        </div>
    )

}