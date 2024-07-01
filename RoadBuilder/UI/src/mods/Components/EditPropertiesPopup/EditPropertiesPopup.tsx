import { NetSectionItem } from "domain/NetSectionItem";
import styles from "./EditPropertiesPopup.module.scss";
import { Button, Number2 } from "cs2/ui";
import { CSSProperties } from "react";
import { useRem } from "cs2/utils";
import { RoadLane } from "domain/RoadProperties";
import { OptionsPanelComponent } from "../OptionsPanel/OptionsPanel";

interface _Props {
  item?: NetSectionItem;
  lane: RoadLane;
  index: number;
  onDelete: (index: number) => void;
  position?: Number2;
}

export const EditPropertiesPopup = (props: _Props) => {
  let rem = useRem();

  return (
    <div className={styles.view}>
      <div className={styles.topBar}>
        <div className={styles.title}>{props.item?.DisplayName}</div>
        <Button className={styles.deleteButton} onSelect={props.onDelete.bind(null, props.index)} variant="icon" />
      </div>
      <div className={styles.content}>
        {props.lane.Options && props.lane.Options.length !== 0 ? (
          <OptionsPanelComponent Index={props.lane.Index} options={props.lane.Options}></OptionsPanelComponent>
        ) : (
          <span> No Options Available</span>
        )}
        <div className={styles.caret}></div>
      </div>
    </div>
  );
};
