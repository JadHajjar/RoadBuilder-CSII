import classNames from "classnames";
import { Button, Tooltip } from "cs2/ui";
import { tool } from "cs2/bindings";
import { bindValue, trigger, useValue } from "cs2/api";
import mod from "mod.json";
import styles from "./ModIconButton.module.scss";
import trafficIcon from "images/mod-icon.svg";
import { RoadBuilderToolModeEnum } from "domain/RoadBuilderToolMode";
import { toggleTool } from "mods/bindings";

const RoadBuilderToolMode$ = bindValue(mod.id, "RoadBuilderToolMode", RoadBuilderToolModeEnum.None);

export default () => {
  const roadBuilderToolMode = useValue(RoadBuilderToolMode$);
  return (
    <Tooltip tooltip="Road Builder">
      <Button
        src={trafficIcon}
        variant="floating"
        className={classNames({ [styles.selected]: roadBuilderToolMode !== RoadBuilderToolModeEnum.None }, styles.toggle)}
        onSelect={toggleTool}
      />
    </Tooltip>
  );
};
