import { ModRegistrar } from "cs2/modding";
import { BottomView } from "mods/BottomView/BottomView";
import { LaneListPanel } from "mods/LaneListPanel/LaneListPanel";
import { ModView } from "mods/ModView/ModView";
import ModIconButton from "mods/Components/ModIconButton/ModIconButton";
import { VanillaComponentResolver } from "vanillacomponentresolver";
import ActionPopup from "mods/Components/ActionPopup/ActionPopup";
import { RemoveVanillaRightToolbar } from "mods/Components/RemoveVanillaRightToolbar";

const register: ModRegistrar = (moduleRegistry) => {
  VanillaComponentResolver.setRegistry(moduleRegistry);

  moduleRegistry.append("GameTopLeft", ModIconButton);
  moduleRegistry.append("Game", ModView);
  moduleRegistry.extend("game-ui/game/components/right-menu/right-menu.tsx", "RightMenu", RemoveVanillaRightToolbar);
};

export default register;
