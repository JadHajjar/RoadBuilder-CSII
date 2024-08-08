import { Number2 } from "cs2/ui";
import { RoadLane } from "domain/RoadLane";
import { createContext, PropsWithChildren, useContext, useMemo, useState } from "react";

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
  open: () => { },
  close: () => { },
});

export const LanePropertiesContextManager = ({ children }: PropsWithChildren) => {
  let defaultLanePropCtx = useContext(LanePropertiesContext);
  let [lanePropState, setLanePropState] = useState<LanePropertiesContextData>(defaultLanePropCtx);

  let lanePropCtx = useMemo(
    () => ({
      ...lanePropState,
      open(roadLane: RoadLane, index: number, position: Number2) {
        setLanePropState({
          ...lanePropState,
          position,
          index,
          laneData: roadLane,
          showPopup: true,
        });
      },
      close() {
        setLanePropState({
          ...lanePropState,
          showPopup: false,
        });
      },
    }),
    [lanePropState, setLanePropState]
  );

  return (
    <LanePropertiesContext.Provider value={lanePropCtx}>
      {children}
    </LanePropertiesContext.Provider>
  )

}