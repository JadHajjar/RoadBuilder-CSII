import { useValue } from 'cs2/api';
import styles from './RoadPropertiesPanel.module.scss';
import { roadProperties$, setRoadProperties } from 'mods/bindings';
import { VanillaComponentResolver } from 'vanillacomponentresolver';
import { TextInput, TextInputTheme } from 'mods/Components/TextInput/TextInput';
import { Dropdown, DropdownItem, DropdownToggle, FOCUS_AUTO, FOCUS_DISABLED } from 'cs2/ui';
import { ChangeEvent, FormEvent, useEffect, useState } from 'react';
import { RoadCategory, RoadProperties } from 'domain/RoadProperties';
import { Theme } from 'cs2/bindings';
import { getModule } from 'cs2/modding';

const DropdownStyle: Theme | any = getModule("game-ui/menu/themes/dropdown.module.scss", "classes");

export const RoadPropertiesPanel = () => {
    let roadProperties = useValue(roadProperties$);    
    useEffect(() => {            
        console.log(roadProperties);
    }, [roadProperties]);

    let roadCategories = Object.entries(RoadCategory)
        .filter((value, idx, arr) => idx < arr.length/2 && Number(value[0]) != 0).map(([k,v]) => [v,k]);        

    let onTextChange = (field: keyof RoadProperties) => (evt: ChangeEvent<HTMLInputElement>) => {
        //TODO: Add a delay to setting the properties or change to save "onFocusEnd"
        setRoadProperties({
            ...roadProperties,
            [field]: evt.target.value
        });
    }
    let onIntChange = (field: keyof RoadProperties) => (text: string) => {
        setRoadProperties({
            ...roadProperties,
            [field]: Number(text)
        });
    }
    let onToggle = (field: keyof RoadProperties) => () => {
        setRoadProperties({
            ...roadProperties,
            [field]: !roadProperties[field]
        });
    };   
    
    let onDropdownChange = <T,>(field: keyof RoadProperties) => (value: string) => {           
        setRoadProperties({
            ...roadProperties,
            [field]: Number(value) as T
        });
    }

    let roadCategoryOptions = roadCategories.map(([key, val]) => (
        <>
            <DropdownItem onChange={onDropdownChange<RoadCategory>('Category')} value={String(val)} key={val}>{key}</DropdownItem>
        </>
    ));

    return (
        <div className={styles.panel}>          
            <VanillaComponentResolver.instance.Section title={"Name"}>                             
                <TextInput 
                    onChange={onTextChange('Name')} 
                    placeholder={'Road Name'} 
                    value={roadProperties.Name} 
                    multiline={1} 
                    type='text' 
                    className={styles.textInput}
                    focusKey={FOCUS_DISABLED} />
            </VanillaComponentResolver.instance.Section>

            <VanillaComponentResolver.instance.Section title={"Speed Limit"}>
                <VanillaComponentResolver.instance.IntInput 
                    value={roadProperties.SpeedLimit}
                    className={styles.textInput}
                    min={0}
                    onChange={onIntChange('SpeedLimit')} />
            </VanillaComponentResolver.instance.Section>

            <VanillaComponentResolver.instance.Section title={"Traffic Lights"}>
                <VanillaComponentResolver.instance.Checkbox 
                    checked={roadProperties.GeneratesTrafficLights}
                    theme={VanillaComponentResolver.instance.checkboxTheme}
                    onChange={onToggle('GeneratesTrafficLights')}
                    />
            </VanillaComponentResolver.instance.Section>

            <VanillaComponentResolver.instance.Section title={"Zonable"}>
                <VanillaComponentResolver.instance.Checkbox 
                    checked={roadProperties.GeneratesZoningBlocks} 
                    theme={VanillaComponentResolver.instance.checkboxTheme}
                    onChange={onToggle('GeneratesZoningBlocks')}
                    />
            </VanillaComponentResolver.instance.Section>      

            <VanillaComponentResolver.instance.Section title="Category">
                <Dropdown focusKey={FOCUS_AUTO} theme={DropdownStyle} content={roadCategoryOptions}>
                    <DropdownToggle>
                        {RoadCategory[roadProperties.Category]}
                    </DropdownToggle>
                </Dropdown>
            </VanillaComponentResolver.instance.Section>

            {/* <VanillaComponentResolver.instance.Section title={"Max Slope"}>
                <VanillaComponentResolver.instance.IntInput />
            </VanillaComponentResolver.instance.Section>
            <VanillaComponentResolver.instance.Section title={"Type"}>
                <TextInput multiline={1} type='text' focusKey={FOCUS_DISABLED} value={roadProperties.AggregateType} />
            </VanillaComponentResolver.instance.Section> */}

        </div>
    )
}