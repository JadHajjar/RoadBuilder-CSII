import { NetSectionItem } from "domain/NetSectionItem";
import styles from './EditPropertiesPopup.module.scss';
import { Button } from "cs2/ui";

export const EditPropertiesPopup = (props: {item: NetSectionItem, target?: Element}) => {

    return (
        <div className={styles.view}>
            <div className={styles.topBar}>
                <Button src="Media/Glyphs/Close.svg" variant="icon" />
                <Button src="Media/Glyphs/Trash.svg" variant="icon" />
            </div>
            <div className={styles.content}>
                <div className={styles.caret}></div>
            </div>
        </div>
    )

}