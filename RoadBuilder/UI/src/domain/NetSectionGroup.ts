import { LaneSectionType } from "./LaneSectionType";
import { NetSectionItem } from "./NetSectionItem";

export interface NetSectionGroup {
  Type: LaneSectionType;
  Sections: NetSectionItem[];
}
