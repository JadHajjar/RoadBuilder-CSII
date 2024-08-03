import { useValue } from "cs2/api";
import styles from "./RoadPropertiesPanel.module.scss";
import { roadOptions$, getRoadName$, setRoadName, roadLanes$ } from "mods/bindings";
import { TextInput, TextInputTheme } from "mods/Components/TextInput/TextInput";
import { FOCUS_DISABLED } from "cs2/ui";
import { Theme } from "cs2/bindings";
import { getModule } from "cs2/modding";
import { OptionsSection } from "mods/Components/OptionsPanel/OptionsSection";
import { OptionsPanelComponent } from "mods/Components/OptionsPanel/OptionsPanel";
import { roadOptionClicked } from "mods/bindings";
import { KeyboardEvent, useState } from "react";
import { VanillaComponentResolver } from "vanillacomponentresolver";

const DropdownStyle: Theme | any = getModule("game-ui/menu/themes/dropdown.module.scss", "classes");

export const RoadPropertiesPanel = () => {
  let roadOptions = useValue(roadOptions$);
  let roadName = useValue(getRoadName$);
  let roadLanes = useValue(roadLanes$);
  let roadWidth = 0;

  for (let index = 0; index < roadLanes.length; index++) {
    roadWidth += roadLanes[index].NetSection?.Width ?? 0;
  }

  let roadUnits = roadWidth % 8 == 0 ? (roadWidth / 8).toString() : (roadWidth / 8).toFixed(1);

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
    <div className={styles.panel}>
      <div className={styles.header}>
        <div className={styles.title}>Road Properties</div>
        <div className={styles.roadWidth}>{`${roadWidth}m / ${roadUnits}U`}</div>
      </div>

      <OptionsSection name="Name">
        <VanillaComponentResolver.instance.EllipsisTextInput
          onChange={({ target }) => setNewRoadName(target.value)}
          placeholder={"Road Name"}
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
