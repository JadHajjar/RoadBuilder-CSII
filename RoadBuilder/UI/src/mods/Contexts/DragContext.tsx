import { Number2 } from "cs2/ui";
import { NetSectionItem } from "domain/NetSectionItem";
import { createContext } from "react";

export interface DragContextData {
    netSectionItem?: NetSectionItem;
    mousePosition: Number2;
    onNetSectionItemChange: (item?: NetSectionItem) => void;
}

export const DragContext = createContext<DragContextData>({
    netSectionItem: undefined,
    mousePosition: {x: 0, y: 0},
    onNetSectionItemChange: (item) => {}
});