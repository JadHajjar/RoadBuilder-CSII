import { useValue } from "cs2/api";
import styles from "./RoadPropertiesPanel.module.scss";
import { roadOptions$, getRoadName$, setRoadName, roadLanes$, getRoadTypeName$, getRoadSize$ } from "mods/bindings";
import { TextInput, TextInputTheme } from "mods/Components/TextInput/TextInput";
import { FOCUS_DISABLED, Tooltip } from "cs2/ui";
import { Theme } from "cs2/bindings";
import { getModule } from "cs2/modding";
import { OptionsSection } from "mods/Components/OptionsPanel/OptionsSection";
import { OptionsPanelComponent } from "mods/Components/OptionsPanel/OptionsPanel";
import { roadOptionClicked } from "mods/bindings";
import { KeyboardEvent, useState } from "react";
import { VanillaComponentResolver } from "vanillacomponentresolver";
import { useLocalization } from "cs2/l10n";
import classNames from "classnames";

const DropdownStyle: Theme | any = getModule("game-ui/menu/themes/dropdown.module.scss", "classes");

export const RoadPropertiesPanel = (props: { editor: boolean }) => {
  const { translate } = useLocalization();
  let roadOptions = useValue(roadOptions$);
  let getRoadTypeName = useValue(getRoadTypeName$);
  let roadName = useValue(getRoadName$);
  let roadSize = useValue(getRoadSize$);

  let [newRoadName, setNewRoadName] = useState(roadName);
  let [isEditingName, setIsEditingName] = useState(false);

  let onFinishEditRoadName = () => {
    setRoadName(newRoadName);
    setIsEditingName(false);
  };

  let onStartEditRoadName = () => {
    setNewRoadName(roadName);
    setIsEditingName(true);
  };

  return (
    <div className={classNames(styles.panel, props.editor ? styles.editor : styles.game)}>
      <div className={styles.header}>
        <div className={styles.title}>{translate(getRoadTypeName)}</div>
        <Tooltip tooltip={translate("RoadBuilder.RoadWidth", "Road Width")}>
          <div className={styles.roadWidth}>{roadSize}</div>
        </Tooltip>
      </div>

      <OptionsSection name={translate("RoadBuilder.Name", "Name")!}>
        <VanillaComponentResolver.instance.EllipsisTextInput
          onChange={({ target }) => setNewRoadName(target.value)}
          placeholder={translate("RoadBuilder.RoadName", "Road Name")!}
          value={isEditingName ? newRoadName : roadName}
          className={styles.textInput}
          onBlur={onFinishEditRoadName}
          onFocus={onStartEditRoadName}
        />
      </OptionsSection>

      <OptionsPanelComponent OnChange={roadOptionClicked} options={roadOptions}></OptionsPanelComponent>
    </div>
  );
};
