import { Button, FOCUS_DISABLED, FocusKey } from 'cs2/ui';
import styles from './SearchTextBox.module.scss';
import { Theme } from 'cs2/bindings';
import { getModule } from 'cs2/modding';
import { useLocalization } from 'cs2/l10n';
import { useState } from 'react';

interface PropsTextInput {
    focusKey?: FocusKey;
    debugName?: string;
    type?: "text" | "password";
    value?: string;
    selectAllOnFocus?: boolean;
    placeholder?: string;
    vkTitle?: string;
    vkDescription?: string;
    disabled?: boolean;
    className?: string;
    multiline: number;
    ref?: any;
    onFocus?: (value: Event) => void;
    onBlur?: (value: Event) => void;
    onKeyDown?: (value: Event) => void;
    onChange?: (value: React.ChangeEvent<HTMLInputElement>) => void;
    onMouseUp?: (value: Event) => void;
} 

const TextInput : (props: PropsTextInput) => JSX.Element = getModule(
    "game-ui/common/input/text/text-input.tsx",
    "TextInput"
  );
  
  const TextInputTheme: Theme | any = getModule(
    "game-ui/editor/widgets/item/editor-item.module.scss",
    "classes"
  );

  const assetGridTheme: Theme | any = getModule(
    "game-ui/game/components/asset-menu/asset-grid/asset-grid.module.scss",
    "classes"
  );

export const SearchTextBox = (props: {onChange?: (val: string) => void}) => {
    const {translate} = useLocalization();
    let [searchQuery, setSearchQuery] = useState<string>();    

    const onChange : React.ChangeEventHandler<HTMLInputElement> = ({target}) => {
        setSearchQuery(target.value);
        props.onChange?.call(null, target.value);
    }

    const clearText = () => {
        setSearchQuery('');
        props.onChange?.call(null, '');
    }

    return (
        <div className={styles.searchContainer}>
            <TextInput 
                multiline={1}                
                value={searchQuery} 
                className={TextInputTheme.input + " " + styles.searchTextBox}
                placeholder={translate("Editor.SEARCH_PLACEHOLDER", "Search...")!}
                onChange={onChange}
                type='text'
                focusKey={FOCUS_DISABLED}                
                />
            <Button 
                variant='icon'
                onSelect={clearText}
                focusKey={FOCUS_DISABLED}
                src="coui://uil/Standard/ArrowLeftClear.svg"
                className={assetGridTheme.item + ' ' + styles.searchClearButton}
                />
        </div>
    )
}