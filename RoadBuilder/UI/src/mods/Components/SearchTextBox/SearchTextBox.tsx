import { Button, FOCUS_DISABLED, FocusKey } from 'cs2/ui';
import styles from './SearchTextBox.module.scss';
import { Theme } from 'cs2/bindings';
import { getModule } from 'cs2/modding';
import { useLocalization } from 'cs2/l10n';
import { useState } from 'react';
import { AssetGridTheme, TextInput, TextInputTheme } from '../TextInput/TextInput';

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
                className={AssetGridTheme.item + ' ' + styles.searchClearButton}
                />
        </div>
    )
}