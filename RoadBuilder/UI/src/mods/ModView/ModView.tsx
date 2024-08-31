import { DragContextManager } from "mods/Contexts/DragContext";

import { LanePropertiesContextManager } from "mods/Contexts/LanePropertiesContext";
import { Router } from "./ModViewRouter";

export const ModView = (editor: boolean) => {
  return (
    <DragContextManager>
      <LanePropertiesContextManager>
        <Router editor={editor} />
      </LanePropertiesContextManager>
    </DragContextManager>
  );
};
