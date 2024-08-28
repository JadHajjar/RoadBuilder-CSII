import { Color } from "cs2/bindings";

export interface NetSectionItem {
  PrefabName: string;
  DisplayName: string;
  Thumbnail: string;
  IsGroup: boolean;
  IsEdge: boolean;
  IsRestricted: boolean;
  IsCustom: boolean;
  Width?: number;
  WidthText?: string;
}
