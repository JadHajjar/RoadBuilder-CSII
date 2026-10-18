import { useValue } from "cs2/api";
import { laneSetupOpen$, laneSetupState$ } from "./laneSetupBindings";
import { LaneSetupPanel } from "./LaneSetupPanel";

// Opened from the editor's bottom bar. With a net section or lane group selected it edits that prefab, otherwise it shows the getting started view
export const LaneSetupView = () => {
  const state = useValue(laneSetupState$);
  const open = useValue(laneSetupOpen$);

  if (!state.InEditor || !open) {
    return null;
  }

  return <LaneSetupPanel state={state} />;
};
