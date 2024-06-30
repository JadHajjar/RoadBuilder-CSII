export interface RoadProperties {
    Name: string;
    SpeedLimit: number;
    GeneratesTrafficLights: boolean;
    GeneratesZoningBlocks: boolean;
    MaxSlopeSteepness: number; // ignore
    AggregateType: string; // ignore
    Category: RoadCategory;
}

export interface RoadLane {
    SectionPrefabName: string;
    Invert: boolean;
}

// Flag Enum
export enum RoadCategory {
    Road = 0,
    Highway = 1,
    PublicTransport = 2,
}