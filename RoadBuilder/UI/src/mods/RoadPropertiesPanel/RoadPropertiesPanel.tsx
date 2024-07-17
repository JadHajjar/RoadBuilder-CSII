import { useValue } from "cs2/api";
import styles from "./RoadPropertiesPanel.module.scss";
import { roadOptions$, getRoadname$, setRoadName } from "mods/bindings";
import { TextInput, TextInputTheme } from "mods/Components/TextInput/TextInput";
import { FOCUS_DISABLED } from "cs2/ui";
import { Theme } from "cs2/bindings";
import { getModule } from "cs2/modding";
import { OptionsSection } from "mods/Components/OptionsPanel/OptionsSection";
import { OptionsPanelComponent } from "mods/Components/OptionsPanel/OptionsPanel";
import { roadOptionClicked } from "mods/bindings";

const DropdownStyle: Theme | any = getModule("game-ui/menu/themes/dropdown.module.scss", "classes");

export const RoadPropertiesPanel = () => {
  let roadOptions = useValue(roadOptions$);
  let getRoadname = useValue(getRoadname$);

  return (
    <div className={styles.panel}>
      <div className={styles.title}>Road Properties</div>

      <OptionsSection name="Name">
        <TextInput
          onChange={(x) => setRoadName(x.target.value)}
          placeholder={"Road Name"}
          value={getRoadname}
          multiline={1}
          type="text"
          className={TextInputTheme.input + " " + styles.textInput}
          focusKey={FOCUS_DISABLED}
        />
      </OptionsSection>

      <OptionsPanelComponent OnChange={roadOptionClicked} options={roadOptions}></OptionsPanelComponent>
    </div>
  );
};
