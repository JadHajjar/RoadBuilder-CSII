import { Button, FOCUS_DISABLED, FocusKey } from "cs2/ui";
import styles from "./SearchTextBox.module.scss";
import { Theme } from "cs2/bindings";
import { getModule } from "cs2/modding";
import { useLocalization } from "cs2/l10n";
import { useRef, useState } from "react";
import { TextInput, TextInputTheme } from "../TextInput/TextInput";
import { VanillaComponentResolver } from "vanillacomponentresolver";
import magnifierIcon from "images/RB_Magnifier.svg";
import arrowLeftClear from "images/RB_ArrowLeftClear.svg";
import classNames from "classnames";

export const SearchTextBox = (props: { onChange?: (val: string) => void; value?: string }) => {
  const { translate } = useLocalization();
  let [searchQuery, setSearchQuery] = useState<string>(props.value == undefined ? "" : props.value);

  const onChange: React.ChangeEventHandler<HTMLInputElement> = ({ target }) => {
    setSearchQuery(target.value);
    props.onChange && props.onChange(target.value);
  };

  // A value passed by the parent wins over the local state, it can be reset from outside
  const currentValue = props.value === undefined ? searchQuery : props.value;

  const clearText = () => {
    setSearchQuery("");
    props.onChange && props.onChange("");
  };

  return (
    <div className={styles.container}>
      <div className={styles.searchArea}>
        <input
          value={currentValue}
          disabled={false}
          type="text"
          className={classNames(TextInputTheme.input, styles.textBox)}
          onChange={onChange}
        />

        {currentValue === "" && (
          <span className={styles.placeholder}>{translate("Editor.SEARCH_PLACEHOLDER", "Search...")!}</span>
        )}

        {currentValue.trim() !== "" ? (
          <Button
            className={styles.clearIcon}
            variant="icon"
            onSelect={clearText}
            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
          >
            <img style={{ maskImage: `url(${arrowLeftClear})` }} />
          </Button>
        ) : (
          // Not themed as an asset grid item, that theme draws its own background behind the icon
          <div className={styles.searchIcon}>
            <img style={{ maskImage: `url(${magnifierIcon})` }} />
          </div>
        )}
      </div>
    </div>
  );
};
