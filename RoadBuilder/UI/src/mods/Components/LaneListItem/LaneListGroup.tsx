import { Button, RefReactElement, Tooltip } from "cs2/ui";
import styles from "./LaneListGroup.module.scss";
import { NetSectionItem } from "domain/NetSectionItem";
import { CSSProperties, MouseEventHandler, ReactNode, forwardRef, useContext, useState } from "react";
import classNames from "classnames";
import { DragContext } from "mods/Contexts/DragContext";
import { MouseButtons } from "mods/util";
import { VanillaComponentResolver } from "vanillacomponentresolver";
import { useLocalization } from "cs2/l10n";
import { LaneSectionType } from "domain/LaneSectionType";

export const LaneListGroup = (props: { type: LaneSectionType; small: boolean; children: ReactNode | undefined }) => {
  let { translate } = useLocalization();
  let [isFolded, setFolded] = useState(false);

  let icon: string = "";

  switch (props.type) {
    case LaneSectionType.Tracks:
      icon = "Media/Game/Icons/TwoWayTrainTrack.svg";
      break;
    case LaneSectionType.Edges:
      icon = "coui://roadbuildericons/RB_Edge.svg";
      break;
  }

  return (
    <div className={classNames(styles.group, isFolded && styles.folded, props.small && styles.small)}>
      <div className={styles.header} onClick={() => setFolded(!isFolded)}>
        <div className={styles.name}>
          <img src={icon} />
          <span>{translate(`RoadBuilder.LaneSectionType[${LaneSectionType[props.type]}]`, LaneSectionType[props.type])}</span>
        </div>
        <Tooltip tooltip={translate("RoadBuilder.FoldCategory")}>
          <div className={styles.foldArrow}>
            <img />
          </div>
        </Tooltip>
      </div>
      <div className={styles.container}>{props.children}</div>
    </div>
  );
};
