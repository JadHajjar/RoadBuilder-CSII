import { useValue } from "cs2/api";
import { NetSectionItem } from "domain/NetSectionItem";
import { allNetSections$ } from "mods/bindings";
import { createContext, PropsWithChildren, useMemo } from "react";

export type NetSectionsStore = Record<string, NetSectionItem>;
export const NetSectionsStoreContext = createContext<NetSectionsStore>({});

export const NetSectionsStoreManager = ({ children }: PropsWithChildren) => {
    let allNetSections = useValue(allNetSections$);
    let nStore = useMemo(() => {
        let nStore = allNetSections.reduce<Record<string, NetSectionItem>>((record: Record<string, NetSectionItem>, cVal: NetSectionItem, cIdx) => {
            record[cVal.PrefabName] = cVal;
            return record;
        }, {});
        return nStore;
    }, [allNetSections]);

    return (
        <NetSectionsStoreContext.Provider value={nStore}>
            {children}
        </NetSectionsStoreContext.Provider>
    )
}