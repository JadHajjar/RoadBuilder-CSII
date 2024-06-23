import { ModRegistrar } from "cs2/modding";
import { BottomView } from "mods/BottomView/BottomView";
import { LaneListPanel } from "mods/LaneListPanel/LaneListPanel";
import { ModView } from "mods/ModView/ModView";
import ModIconButton from "mods/Components/ModIconButton/ModIconButton";
import { VanillaComponentResolver } from "vanillacomponentresolver";
import ActionPopup from "mods/Components/ActionPopup/ActionPopup";

const register: ModRegistrar = (moduleRegistry) => {
  VanillaComponentResolver.setRegistry(moduleRegistry);

  moduleRegistry.append("GameTopLeft", ModIconButton);
  moduleRegistry.append("Game", ActionPopup);
  moduleRegistry.append("Game", ModView);
};

export default register;
