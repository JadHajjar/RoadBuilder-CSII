import { Number2 } from "cs2/ui";
import { NetSectionItem } from "domain/NetSectionItem";
import { RoadLane } from "domain/RoadLane";
import { createContext } from "react";

export interface DragContextData {
  netSectionItem?: NetSectionItem;
  roadLane?: RoadLane;
  mousePosition: Number2;
  onNetSectionItemChange: (item?: NetSectionItem) => void;
  setRoadLane: (roadLane?: RoadLane, currentIndex?: number) => void;
  mouseReleased: boolean;
  dragElement?: Element | null;
  oldIndex?: number;
}

export const DragContext = createContext<DragContextData>({
  netSectionItem: undefined,
  roadLane: undefined,
  mousePosition: { x: 0, y: 0 },
  onNetSectionItemChange: () => {},
  setRoadLane: () => {},
  mouseReleased: false,
});
