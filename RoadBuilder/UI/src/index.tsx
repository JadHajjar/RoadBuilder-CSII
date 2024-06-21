import { ModRegistrar } from "cs2/modding";
import { BottomView } from "mods/BottomView/BottomView";
import { LaneListPanel } from "mods/LaneListPanel/LaneListPanel";
import ToggleModButton from "mods/ToggleModButton";

const register: ModRegistrar = (moduleRegistry) => {

    moduleRegistry.append('GameTopLeft', ToggleModButton)
    moduleRegistry.append('Game', BottomView);    
    moduleRegistry.append('Game', LaneListPanel);    
}

export default register;