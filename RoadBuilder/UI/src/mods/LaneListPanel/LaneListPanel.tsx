import { Scrollable } from 'cs2/ui';
import { LaneListItem } from '../Components/LaneListItem/LaneListItem';
import styles from './LaneListPanel.module.scss';

export const LaneListPanel = () => {

    const range = (i: number, n: number) => ({
        [Symbol.iterator]: () => ({ 
            next: () => ({ done: i > n, value: i++  }) 
        })
    })
    let items = [...range(0,15)].map((val, idx) => <LaneListItem key={idx} />)

    return (
        <div className={styles.panel}>
            <Scrollable className={styles.list} vertical smooth trackVisibility='scrollable'>
                {items}
            </Scrollable>
            {/* <div className={styles.list}>
                
            </div>             */}
        </div>
    )
}