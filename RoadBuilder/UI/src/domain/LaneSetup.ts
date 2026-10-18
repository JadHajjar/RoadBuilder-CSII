import { RoadLane } from "./RoadLane";

// Mirrors RoadBuilder.Domain.UI.LaneSetupState on the C# side
export type LaneSetupRole = "none" | "lane" | "edge" | "variant" | "group";

export interface LaneSetupState {
  Available: boolean;
  InEditor: boolean;
  ListIcon?: string;
  Name?: string;
  DisplayName?: string;
  IsGroup: boolean;
  ReadOnly: boolean;
  ReadOnlyReason?: "BuiltIn" | "ReadOnly";
  Role: LaneSetupRole;
  LaneInfo?: LaneSetupLaneInfo;
  Edge?: LaneSetupEdge;
  Decoration?: LaneSetupDecoration;
  Aggregate?: LaneSetupAggregate;
  Variant?: LaneSetupVariant;
  Group?: LaneSetupGroup;
  Ignore: boolean;
  Preview?: RoadLane[];
  Issues: LaneSetupIssue[];
}

export interface LaneSetupLaneInfo {
  RequireAll: number;
  RequireAny: number;
  RequireNone: number;
  GroundTexture: number;
  Color?: string;
  NoDirection: boolean;
  Median: boolean;
  FrontThumbnail?: string;
  BackThumbnail?: string;
  LaneThumbnails: string[];
  PieceRequireAll: number[];
  PieceRequireAny: number[];
  PieceRequireNone: number[];
}

export interface LaneSetupEdge {
  LeftSide?: string;
  RightSide?: string;
  AddSidewalkStateOnNode: boolean;
  DoNotRequireBeingOnEdge: boolean;
}

export interface LaneSetupDecoration {
  GrassThumbnail?: string;
  TreeThumbnail?: string;
  GrassAndTreeThumbnail?: string;
  LaneGrassThumbnail: string[];
  LaneTreeThumbnail: string[];
  LaneGrassAndTreeThumbnail: string[];
}

export interface LaneSetupAggregate {
  Left: LaneSetupAggregateSection[];
  Right: LaneSetupAggregateSection[];
}

export interface LaneSetupAggregateSection {
  Section?: string;
  Invert: boolean;
  PieceRequireAll: number[];
  PieceRequireAny: number[];
  PieceRequireNone: number[];
}

export interface LaneSetupVariant {
  Group?: string;
  Combination: LaneSetupCombination[];
}

export interface LaneSetupCombination {
  Option?: string;
  Value?: string;
}

export interface LaneSetupGroup {
  Options: LaneSetupOption[];
  LinkedSections: LaneSetupLinkedSection[];
}

export interface LaneSetupOption {
  Name?: string;
  Type: LaneOptionType;
  DefaultValue?: string;
  IgnoreForSimilarDuplicate: boolean;
  Values: LaneSetupOptionValue[];
}

export interface LaneSetupOptionValue {
  Value?: string;
  Thumbnail?: string;
}

export interface LaneSetupLinkedSection {
  Name?: string;
  DisplayName?: string;
  Combination: LaneSetupCombination[];
}

export interface LaneSetupGroupSummary {
  Name: string;
  DisplayName: string;
  BuiltIn: boolean;
  Options: LaneSetupOption[];
}

export interface LaneSetupEnumValue {
  Name: string;
  Value: number;
}

export interface LaneSetupIssue {
  Severity: "error" | "warning" | "info";
  Key?: string;
  Message?: string;
}

// Mirrors RoadBuilder.Domain.Enums.LaneOptionType
export enum LaneOptionType {
  SingleSelectionButtons = 0,
  MultiSelectionButtons = 10,
  Toggle = 20,
  Checkbox = 21,
  ValueUpDown = 30,
  LaneWidth = 31,
  Decoration = 40,
  TwoWay = 50,
}

// Mirrors RoadBuilder.Domain.Enums.LaneGroundType
export enum LaneGroundType {
  Unset = 0,
  Asphalt = 1,
  PaintedAsphalt = 2,
  Gravel = 10,
  Tiled = 20,
  Train = 30,
}

// Option types whose variants pick one of the option's listed values
export const valueOptionTypes = [
  LaneOptionType.SingleSelectionButtons,
  LaneOptionType.MultiSelectionButtons,
  LaneOptionType.Toggle,
  LaneOptionType.ValueUpDown,
  LaneOptionType.LaneWidth,
];
