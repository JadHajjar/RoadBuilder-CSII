import { Tooltip } from 'cs2/ui';
import styles from './LaneListItem.module.scss';

export const LaneListItem = () => {
    return (
        <Tooltip tooltip={"Test Description"}>
            <div className={styles.item}>            
                <img className={styles.image}/>
                <div className={styles.label}>
                    Test Name
                </div>
            </div>
        </Tooltip>
        
    )
}