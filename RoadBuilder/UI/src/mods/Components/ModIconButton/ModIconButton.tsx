import classNames from "classnames";
import { Button, Tooltip } from "cs2/ui";
import { tool } from "cs2/bindings";
import { bindValue, trigger, useValue } from "cs2/api";
import mod from "mod.json";
import styles from "./ModIconButton.module.scss";
import trafficIcon from "images/mod-icon.svg";
import { RoadBuilderToolModeEnum } from "domain/RoadBuilderToolMode";

const RoadBuilderToolMode$ = bindValue(mod.id, "RoadBuilderToolMode", RoadBuilderToolModeEnum.None);

export default () => {
  const RoadBuilderToolMode = useValue(RoadBuilderToolMode$);

  const toggleTool = () => trigger(mod.id, "ToggleTool");

  return (
    <Tooltip tooltip="Road Builder">
      <Button
        src={trafficIcon}
        variant="floating"
        className={classNames({ [styles.selected]: RoadBuilderToolMode !== RoadBuilderToolModeEnum.None }, styles.toggle)}
        onSelect={toggleTool}
      />
    </Tooltip>
  );
};
