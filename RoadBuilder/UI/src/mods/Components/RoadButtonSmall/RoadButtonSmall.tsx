import { NetSectionItem } from 'domain/NetSectionItem';
import styles from './RoadButtonSmall.module.scss';
import { Tooltip } from 'cs2/ui';

export const RoadButtonSmall = (props: {item: NetSectionItem}) => {        
    return (
        <Tooltip tooltip={props.item.DisplayName}>
            <div className={styles.button}>
                <div className={styles.frame}>                    
                </div>
                <img src='Media/Placeholder.svg'/>
            </div>
        </Tooltip>        
    )
}