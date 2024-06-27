import { Button, FOCUS_DISABLED, FocusKey, Scrollable } from 'cs2/ui';
import { LaneListItem } from '../Components/LaneListItem/LaneListItem';
import styles from './LaneListPanel.module.scss';
import { useValue } from 'cs2/api';
import { allNetSections$ } from 'mods/bindings';
import { getModule } from 'cs2/modding';
import { MutableRefObject, MouseEventHandler, useState } from 'react';
import { VanillaComponentResolver } from 'vanillacomponentresolver';
import { Theme } from 'cs2/bindings';
import { useLocalization } from 'cs2/l10n'; 
import { SearchTextBox } from 'mods/Components/SearchTextBox/SearchTextBox';
  

export const LaneListPanel = () => {
    const {translate} = useLocalization();
    let [searchQuery, setSearchQuery] = useState<string>();
    

    let allNetSections = useValue(allNetSections$);                
    let items = allNetSections
        .filter((val, idx) => val.PrefabName)
        .filter((val, idx) => searchQuery == undefined || searchQuery == '' || val.DisplayName.indexOf(searchQuery) >= 0)                
        .map((val, idx) => <LaneListItem key={idx} netSection={val}/>);        

    return (
        <div className={styles.panel}>
            <SearchTextBox onChange={setSearchQuery} />
            <Scrollable className={styles.list} vertical smooth trackVisibility='scrollable'>
                {items}
            </Scrollable>
            {/* <div className={styles.list}>
                
            </div>             */}
        </div>
    )
}