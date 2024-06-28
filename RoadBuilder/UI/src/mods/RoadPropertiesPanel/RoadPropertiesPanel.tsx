import { useValue } from 'cs2/api';
import styles from './RoadPropertiesPanel.module.scss';
import { roadProperties$ } from 'mods/bindings';
import { VanillaComponentResolver } from 'vanillacomponentresolver';
import { TextInput } from 'mods/Components/TextInput/TextInput';
import { Dropdown, DropdownToggle, FOCUS_DISABLED } from 'cs2/ui';
import { useEffect } from 'react';

export const RoadPropertiesPanel = () => {
    let roadProperties = useValue(roadProperties$);
    useEffect(() => {
        console.log(roadProperties);
    }, [roadProperties])
    return (
        <div className={styles.panel}>          
            <VanillaComponentResolver.instance.Section title={"Name"}>                
                <TextInput value={roadProperties.name} multiline={1} type='text' focusKey={FOCUS_DISABLED} />
            </VanillaComponentResolver.instance.Section>
            <VanillaComponentResolver.instance.Section title={"Speed Limit"}>
                <VanillaComponentResolver.instance.IntInput />
            </VanillaComponentResolver.instance.Section>
            <VanillaComponentResolver.instance.Section title={"Traffic Lights"}>
                <VanillaComponentResolver.instance.Checkbox checked={roadProperties.generatesTrafficLights} />
            </VanillaComponentResolver.instance.Section>
            <VanillaComponentResolver.instance.Section title={"Zonable"}>
                <VanillaComponentResolver.instance.Checkbox checked={roadProperties.generatesZoningBlocks} />
            </VanillaComponentResolver.instance.Section>            
            <VanillaComponentResolver.instance.Section title="Category">
                <Dropdown content={[]}>
                    <DropdownToggle />
                </Dropdown>
            </VanillaComponentResolver.instance.Section>

            {/* <VanillaComponentResolver.instance.Section title={"Max Slope"}>
                <VanillaComponentResolver.instance.IntInput />
            </VanillaComponentResolver.instance.Section>
            <VanillaComponentResolver.instance.Section title={"Type"}>
                <TextInput multiline={1} type='text' focusKey={FOCUS_DISABLED} value={roadProperties.aggregateType} />
            </VanillaComponentResolver.instance.Section> */}

        </div>
    )
}