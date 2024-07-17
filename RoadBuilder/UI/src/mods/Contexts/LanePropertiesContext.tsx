import { Number2 } from "cs2/ui";
import { RoadLane } from "domain/RoadLane";
import { createContext } from "react";

export interface LanePropertiesContextData {
  laneData?: RoadLane;
  index: number;
  position: Number2;
  showPopup: boolean;
  open: (roadLane: RoadLane, index: number, position: Number2) => void;
  close: () => void;
}

export const LanePropertiesContext = createContext<LanePropertiesContextData>({
  laneData: undefined,
  index: -1,
  showPopup: false,
  position: { x: 0, y: 0 },
  open: () => {},
  close: () => {},
});
