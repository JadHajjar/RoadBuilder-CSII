import styles from "./DropDown.module.scss";
import { Theme } from "cs2/bindings";
import { getModule } from "cs2/modding";
import { Dropdown, DropdownItem, DropdownToggle, FOCUS_AUTO } from "cs2/ui";

export interface DropdownItems<T> {
  SelectedItem: T;
  Items: T[];
  Icon?: string;
  OnItemSelected: (item: T) => void;
  ToString: (item: T) => string | null;
}

const DropdownStyle: Theme | any = getModule("game-ui/menu/themes/dropdown.module.scss", "classes");

export const CustomDropdown = <T,>(props: DropdownItems<T>) => {
  const dropDownItems = props.Items.map((item, index) => (
    <DropdownItem<Number>
      theme={DropdownStyle}
      focusKey={FOCUS_AUTO}
      value={index}
      closeOnSelect={true}
      onToggleSelected={() => props.OnItemSelected(item)}
      selected={true}
      sounds={{ select: "select-item" }}
    >
      {props.ToString(item)}
    </DropdownItem>
  ));

  return (
    <div style={{ padding: "5rem" }}>
      <Dropdown focusKey={FOCUS_AUTO} theme={DropdownStyle} content={dropDownItems}>
        <DropdownToggle>
          <span>
            {props.Icon && <img className={styles.icon} style={{ maskImage: `url(${props.Icon})` }} />}
            {props.ToString(props.SelectedItem)}
          </span>
        </DropdownToggle>
      </Dropdown>
    </div>
  );
};
