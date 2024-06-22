import { bindValue } from "cs2/api";
import { NetSectionItem } from "domain/NetSectionItem";

export const allNetSections$ = bindValue<NetSectionItem[]>("RoadBuilder", "NetSections");
