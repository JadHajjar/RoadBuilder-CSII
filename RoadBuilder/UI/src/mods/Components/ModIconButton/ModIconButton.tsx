import classNames from "classnames";
import { Button, Tooltip } from "cs2/ui";
import { tool } from "cs2/bindings";
import { bindValue, trigger, useValue } from "cs2/api";
import mod from "mod.json";
import styles from "./ModIconButton.module.scss";
import trafficIcon from "images/RB_ModIcon.svg";
import trafficIconActive from "images/RB_ModIconActive.svg";
import { RoadBuilderToolModeEnum } from "domain/RoadBuilderToolMode";
import { roadBuilderToolMode$, toggleTool } from "mods/bindings";

export default () => {
  const roadBuilderToolMode = useValue(roadBuilderToolMode$);
  return (
    <Tooltip tooltip="Road Builder">
      <Button
        variant="floating"
        className={classNames({ [styles.selected]: roadBuilderToolMode !== RoadBuilderToolModeEnum.None }, styles.toggle)}
        onSelect={toggleTool}
      >
        <img style={{ maskImage: `url(${roadBuilderToolMode !== RoadBuilderToolModeEnum.None ? trafficIconActive : trafficIcon})` }} />
      </Button>
    </Tooltip>
  );
};
