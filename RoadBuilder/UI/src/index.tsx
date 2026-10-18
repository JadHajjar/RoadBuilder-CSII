import { ModRegistrar } from "cs2/modding";
import { ModView } from "mods/ModView/ModView";
import ModIconButton from "mods/Components/ModIconButton/ModIconButton";
import { VanillaComponentResolver } from "vanillacomponentresolver";
import { RemoveVanillaRightToolbar } from "mods/Components/RemoveVanillaRightToolbar";
import mod from "../mod.json";
import { LaneSetupView } from "mods/LaneSetup/LaneSetupView";
import { LaneSetupBottomBarExtension } from "mods/LaneSetup/LaneSetupBottomBarButton";

const register: ModRegistrar = (moduleRegistry) => {
  console.log(mod.id + " UI module registering...");
  VanillaComponentResolver.setRegistry(moduleRegistry);

  moduleRegistry.append("GameTopLeft", ModIconButton);
  moduleRegistry.append("Game", () => ModView(false));
  moduleRegistry.append("Editor", () => ModView(true));
  // Helper panel for setting up custom lanes on the prefab selected in the inspector
  moduleRegistry.append("Editor", LaneSetupView);
  moduleRegistry.extend("game-ui/editor/components/bottom-bar/bottom-bar.tsx", "BottomBar", LaneSetupBottomBarExtension);
  moduleRegistry.extend("game-ui/game/components/right-menu/right-menu.tsx", "RightMenu", RemoveVanillaRightToolbar);

  console.log(mod.id + " UI module registrations completed.");
};

export default register;
