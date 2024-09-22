export enum RoadCategory {
  Road = 0,
  Highway = 1,
  PublicTransport = 2,
  Train = 4,
  Tram = 8,
  Subway = 16,
  Gravel = 32,
  Tiled = 64,
  Fence = 256,
  Pathway = 512,
}

export function GetCategoryIcon(cat: RoadCategory): string | undefined {
  switch (cat) {
    case RoadCategory.Road:
      return "Media/Game/Icons/Roads.svg";
    case RoadCategory.Highway:
      return "Media/Game/Icons/Highways.svg";
    case RoadCategory.PublicTransport:
      return "Media/Game/Icons/Bus.svg";
    case RoadCategory.Train:
      return "Media/Game/Icons/Train.svg";
    case RoadCategory.Subway:
      return "Media/Game/Icons/Subway.svg";
    case RoadCategory.Tram:
      return "Media/Game/Icons/Tram.svg";
    case RoadCategory.Gravel:
      return "Media/Game/Icons/GravelRoad.svg";
    case RoadCategory.Tiled:
      return "Media/Game/Icons/PedestrianStreet.svg";
    case RoadCategory.Pathway:
      return "Media/Game/Icons/Pathways.svg";
  }
}

export function GetCategoryName(cat: RoadCategory): string {
  return `RoadBuilder.RoadCategory[${RoadCategory[cat]}]`;
}
