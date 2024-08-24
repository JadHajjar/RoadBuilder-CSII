import { NetSectionItem } from "./NetSectionItem";
import { OptionSection } from "./Options";

export interface RoadLane {
  SectionPrefabName: string;
  Index: number;
  NoDirection?: boolean;
  Invert?: boolean;
  InvertImage?: boolean;
  TwoWay?: boolean;
  IsGroup: boolean;
  IsEdgePlaceholder?: boolean;
  Color?: string;
  Texture?: string;
  NetSection?: NetSectionItem;
  Options?: OptionSection[];
}
