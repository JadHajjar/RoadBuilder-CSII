import { Number2 } from "cs2/ui";
import { NetSectionItem } from "domain/NetSectionItem";
import { RoadLane } from "domain/RoadProperties";
import { createContext } from "react";

export interface DragContextData {
    netSectionItem?: NetSectionItem;
    roadLane?: RoadLane;
    mousePosition: Number2;
    onNetSectionItemChange: (item?: NetSectionItem) => void;
    setRoadLane: (roadLane?: RoadLane) => void;
    mouseReleased: boolean;
    dragElement?: Element | null;
}

export const DragContext = createContext<DragContextData>({
    netSectionItem: undefined,
    roadLane: undefined,
    mousePosition: {x: 0, y: 0},
    onNetSectionItemChange: (item) => {},
    setRoadLane: (lane?: RoadLane) => {},
    mouseReleased: false
});