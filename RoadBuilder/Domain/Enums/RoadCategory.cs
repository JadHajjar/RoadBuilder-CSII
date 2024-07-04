using System;

namespace RoadBuilder.Domain.Enums
{
	[Flags]
	public enum RoadCategory : uint
	{
		Road = 0,
		Highway = 1,
		PublicTransport = 2,
		Train = 4,
		Tram = 8,
		Subway = 16,
		Gravel = 32,
		Tiled = 64,
	}
}
