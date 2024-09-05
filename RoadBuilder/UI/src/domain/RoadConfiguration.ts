import { RoadCategory } from "./RoadCategory";

export interface RoadConfiguration {
  ID: string;
  Name: string;
  Author: string;
  Thumbnail: string;
  Locked: boolean;
  Used: boolean;
  IsNotInPlayset: boolean;
  Category: RoadCategory;
  Tags: string[];
}
