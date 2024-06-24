import { NetSectionItem } from 'domain/NetSectionItem';
import styles from './RoadButtonSmall.module.scss';
import { Button, Tooltip } from 'cs2/ui';
import { EditPropertiesPopup } from '../EditPropertiesPopup/EditPropertiesPopup';
import { MouseEvent, MouseEventHandler, useState } from 'react';

type _Props = {
    item: NetSectionItem;
    onDelete: (index: number) => void;
    onClick?: (index: number, evt: MouseEvent<HTMLDivElement>) => void;
    index: number;
};
export const RoadButtonSmall = (props: _Props) => {        
    let [showProperties, setShowProperties] = useState(false);

    let onMouseEnter = () => {
        setShowProperties(true);
    }

    let onMouseLeave = () => {
        setShowProperties(false);
    }

    let popup = showProperties? (
        <EditPropertiesPopup 
            onDelete={props.onDelete} 
            index={props.index} 
            item={props.item}                 
            />
    ) : (<></>);

    return (
        <div className={styles.container} onMouseEnter={onMouseEnter} onMouseLeave={onMouseLeave}>            
            <div className={styles.button}>
                <div className={styles.frame}>                    
                </div>
                <img src='Media/Placeholder.svg'/>                    
            </div>            
            {popup}
        </div>            
    )
}