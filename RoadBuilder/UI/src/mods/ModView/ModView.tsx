import { DragContextManager } from "mods/Contexts/DragContext";

import { NetSectionsStoreManager } from "mods/Contexts/NetSectionsStore";
import { LanePropertiesContextManager } from "mods/Contexts/LanePropertiesContext";
import { Router } from "./ModViewRouter";

export const ModView = () => {  
  return (
    <DragContextManager>
      <NetSectionsStoreManager>
        <LanePropertiesContextManager>
          <Router />
        </LanePropertiesContextManager>
      </NetSectionsStoreManager>
    </DragContextManager>
  );
};
