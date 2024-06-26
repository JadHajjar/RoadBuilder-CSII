export interface RoadProperties {
    name: string;
    speedLimit: number;
    generatesTrafficLights: boolean;
    generatesZoningBlocks: boolean;
    maxSlopeSteepness: number; // ignore
    aggregateType: string; // ignore
    category: RoadCategory;
}

export interface RoadLane {
    SectionPrefabName: string;
}

// Flag Enum
export enum RoadCategory {
    Road = 0,
    Highway = 1,
    PublicTransport = 2,
}