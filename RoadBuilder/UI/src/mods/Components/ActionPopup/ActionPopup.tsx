import classNames from "classnames";
import { Button, Tooltip, FOCUS_DISABLED, FOCUS_AUTO } from "cs2/ui";
import { tool } from "cs2/bindings";
import { trigger, useValue, bindValue } from "cs2/api";
import mod from "mod.json";
import styles from "./ActionPopup.module.scss";
import { RoadBuilderToolModeEnum } from "domain/RoadBuilderToolMode";
import { useLocalization } from "cs2/l10n";

const RoadBuilderToolMode$ = bindValue(mod.id, "RoadBuilderToolMode", RoadBuilderToolModeEnum.None);

export default () => {
  const { translate } = useLocalization();

  const RoadBuilderToolMode = useValue(RoadBuilderToolMode$);

  if (RoadBuilderToolMode !== RoadBuilderToolModeEnum.ActionSelection) return null;

  return (
    <div className={styles.container}>
      <div>
        <Tooltip tooltip={translate(`Tooltip.LABEL[${mod.id}.ClosePanel]`, "Close Panel")}>
          <Button className={styles.closeIcon} variant="icon" onSelect={() => trigger(mod.id, "ActionPopup.Cancel")} focusKey={FOCUS_DISABLED}>
            <img style={{ maskImage: "url(Media/Glyphs/Close.svg)" }}></img>
          </Button>
        </Tooltip>

        <Button variant="flat" onSelect={() => trigger(mod.id, "ActionPopup.Edit")} focusKey={FOCUS_AUTO}>
          Edit This Prefab
        </Button>

        <Button variant="flat" onSelect={() => trigger(mod.id, "ActionPopup.New")} focusKey={FOCUS_AUTO}>
          Create New Prefab
        </Button>
      </div>
    </div>
  );
};
