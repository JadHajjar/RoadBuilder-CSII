import { ModRegistrar } from "cs2/modding";
import { BottomView } from "mods/BottomView/BottomView";
import { LaneListPanel } from "mods/LaneListPanel/LaneListPanel";
import { ModView } from "mods/ModView/ModView";
import ToggleModButton from "mods/ToggleModButton";
import { VanillaComponentResolver } from "vanillacomponentresolver";

const register: ModRegistrar = (moduleRegistry) => {
    VanillaComponentResolver.setRegistry(moduleRegistry);

    moduleRegistry.append('GameTopLeft', ToggleModButton)
    moduleRegistry.append('Game', ModView);
}

export default register;