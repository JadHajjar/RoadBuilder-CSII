import { NetSectionItem } from "domain/NetSectionItem";
import { createContext } from "react";

export type NetSectionsStore = Record<string, NetSectionItem>;
export const NetSectionsStoreContext = createContext<NetSectionsStore>({});
