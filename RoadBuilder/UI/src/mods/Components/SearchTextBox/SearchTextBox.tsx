import { Button, FOCUS_DISABLED, FocusKey } from "cs2/ui";
import styles from "./SearchTextBox.module.scss";
import { Theme } from "cs2/bindings";
import { getModule } from "cs2/modding";
import { useLocalization } from "cs2/l10n";
import { useRef, useState } from "react";
import { TextInput, TextInputTheme } from "../TextInput/TextInput";
import { VanillaComponentResolver } from "vanillacomponentresolver";
import magnifierIcon from "images/magnifier.svg";
import arrowLeftClear from "images/arrowLeftClear.svg";
import classNames from "classnames";

const AssetGridTheme: Theme | any = getModule("game-ui/game/components/asset-menu/asset-grid/asset-grid.module.scss", "classes");

export const SearchTextBox = (props: { onChange?: (val: string) => void }) => {
  const { translate } = useLocalization();
  const searchRef = useRef(null);
  let [searchQuery, setSearchQuery] = useState<string>("");

  const onChange: React.ChangeEventHandler<HTMLInputElement> = ({ target }) => {
    setSearchQuery(target.value);
    props.onChange?.call(null, target.value);
  };

  const clearText = () => {
    setSearchQuery("");
    props.onChange?.call(null, "");
  };

  return (
    <div className={styles.container}>
      <div className={styles.searchArea}>
        <TextInput
          ref={searchRef}
          multiline={1}
          value={searchQuery}
          disabled={false}
          type="text"
          className={classNames(TextInputTheme.input, styles.textBox)}
          focusKey={FOCUS_DISABLED}
          onChange={onChange}
          placeholder={translate("Editor.SEARCH_PLACEHOLDER", "Search...")!}
        ></TextInput>

        {searchQuery.trim() !== "" ? (
          <Button
            className={classNames(VanillaComponentResolver.instance.assetGridTheme.item, styles.clearIcon)}
            variant="icon"
            onSelect={clearText}
            focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
          >
            <img src={arrowLeftClear}></img>
          </Button>
        ) : (
          <div className={classNames(VanillaComponentResolver.instance.assetGridTheme.item, styles.clearIcon)}>
            <img src={magnifierIcon}></img>
          </div>
        )}
      </div>
    </div>
  );
};
