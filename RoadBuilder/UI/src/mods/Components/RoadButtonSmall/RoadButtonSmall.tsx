import { NetSectionItem } from 'domain/NetSectionItem';
import styles from './RoadButtonSmall.module.scss';
import { Button, Tooltip } from 'cs2/ui';

type _Props = {
    item: NetSectionItem;
    onDelete: (index: number) => void;
    index: number;
};
export const RoadButtonSmall = (props: _Props) => {        
    return (
        <Tooltip tooltip={props.item.DisplayName}>
            <div className={styles.button}>
                <div className={styles.frame}>                    
                </div>
                <img src='Media/Placeholder.svg'/>
                <Button 
                    variant='icon' 
                    onSelect={props.onDelete.bind(null, props.index)} 
                    className={styles.closeButton} 
                    src='Media/Glyphs/Trash.svg'/>
            </div>
        </Tooltip>        
    )
}