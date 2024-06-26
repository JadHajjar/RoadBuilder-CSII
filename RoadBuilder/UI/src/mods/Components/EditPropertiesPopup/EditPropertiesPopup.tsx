import { NetSectionItem } from "domain/NetSectionItem";
import styles from './EditPropertiesPopup.module.scss';
import { Button, Number2 } from "cs2/ui";
import { CSSProperties } from "react";
import { useRem } from "cs2/utils";

interface _Props {
    item?: NetSectionItem;
    index: number;
    onDelete: (index: number) => void;
    position?: Number2;
};

export const EditPropertiesPopup = (props: _Props) => {
    let rem = useRem();

    return (
        <div className={styles.view}>
            <div className={styles.topBar}>
                <Button className={styles.deleteButton} onSelect={props.onDelete.bind(null, props.index)} variant="icon" />                
            </div>
            <div className={styles.content}>
                <div>
                    {props.item?.DisplayName}
                </div>                
                <div className={styles.caret}></div>
            </div>
        </div>
    )

}