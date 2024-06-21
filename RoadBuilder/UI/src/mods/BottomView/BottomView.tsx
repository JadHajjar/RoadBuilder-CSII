import styles from './BottomView.module.scss';
import { RoadButtonSmall } from '../Components/RoadButtonSmall/RoadButtonSmall';
import { DragAndDropDivider } from 'mods/Components/DragAndDropDivider/DragAndDropDivider';
import { Button } from 'cs2/ui';

export const BottomView = () => {

    return (
        <div className={styles.view}>
            <RoadButtonSmall />
            <DragAndDropDivider />
            <RoadButtonSmall />
            <Button className={styles.closeButton} variant='flat'>X</Button>
        </div>
    )

}