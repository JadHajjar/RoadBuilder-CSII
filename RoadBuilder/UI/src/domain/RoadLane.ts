import { NetSectionItem } from "./NetSectionItem";
import { OptionSection } from "./Options";

export interface RoadLane {
  SectionPrefabName: string;
  Index: number;
  Invert?: boolean;
  IsGroup: boolean;
  NetSection?: NetSectionItem;
  Options?: OptionSection[];
}
