import { useValue } from "cs2/api";
import styles from "./RoadPropertiesPanel.module.scss";
import { roadOptions$, getRoadName$, setRoadName } from "mods/bindings";
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

  let [newRoadName, setNewRoadName] = useState(roadName);
  let [isEditingName, setIsEditingName] = useState(false);

  let onFinishEditRoadName = () => {
    console.log("FIN");
    setRoadName(newRoadName);
    setIsEditingName(false);
  }

  let onStartEditRoadName = () => {
    setNewRoadName(roadName); 
    setIsEditingName(true);       
  }

  let onRoadNameKeyDown = (baseEvt: KeyboardEvent<HTMLInputElement>) => {
    let evt : KeyboardEvent<HTMLInputElement> = baseEvt as any;        
    if (evt && evt.key == 'Enter') {
      evt.preventDefault();
      evt.currentTarget.selectionEnd = 0;
      evt.currentTarget.blur(); // auto triggers "onFinishEditRoadName"
      return false;
    } else {
      return true;
    }
  }


  return (
    <div className={styles.panel}>
      <div className={styles.title}>Road Properties</div>

      <OptionsSection name="Name">
        <input
          onChange={({target}) => setNewRoadName(target.value)}
          placeholder={"Road Name"}
          value={isEditingName? newRoadName : roadName}          
          onKeyDown={onRoadNameKeyDown}
          className={ styles.textInput}
          onBlur={onFinishEditRoadName}
          onFocus={onStartEditRoadName}
          type="text"
        />
      </OptionsSection>

      <OptionsPanelComponent OnChange={roadOptionClicked} options={roadOptions}></OptionsPanelComponent>
    </div>
  );
};
