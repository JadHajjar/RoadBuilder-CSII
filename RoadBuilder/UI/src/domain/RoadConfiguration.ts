import { RoadCategory } from "./RoadCategory";

export interface RoadConfiguration {
  ID: string;
  Name: string;
  Thumbnail: string;
  Locked: boolean;
  Used: boolean;
  Category: RoadCategory;
  Tags: string[];
}
