import { Tooltip } from 'cs2/ui';
import styles from './LaneListItem.module.scss';
import { NetSectionItem } from 'domain/NetSectionItem';

export const LaneListItem = ({netSection} : {netSection: NetSectionItem}) => {
    return (
        <Tooltip tooltip={netSection.DisplayName}>
            <div className={styles.item}>            
                <img className={styles.image} src={netSection.Thumbnail}/>
                <div className={styles.label}>
                    {netSection.DisplayName}
                </div>
            </div>
        </Tooltip>
        
    )
}