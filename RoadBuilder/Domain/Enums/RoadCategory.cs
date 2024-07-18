using System;

namespace RoadBuilder.Domain.Enums
{
	[Flags]
	public enum RoadCategory : ulong
	{
		Road = 0,
		Highway = 1,
		PublicTransport = 2,
		Train = 4,
		Tram = 8,
		Subway = 16,
		Gravel = 32,
		Tiled = 64,
		RaisedSidewalk = 128,
		Fence = 256,
		Pathway = 512,
		NonAsphalt = Train | Subway | Gravel | Tiled | Fence | Pathway,
		NonRoad = Highway | PublicTransport | Train | Tram | Subway | Gravel | Tiled | Fence | Pathway,
		NoRaisedSidewalkSupport = Train | Subway | Gravel | Fence | Pathway,
	}
}
