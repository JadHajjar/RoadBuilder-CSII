using System;

namespace RoadBuilder.Domain.Enums
{
	[Flags]
	public enum RoadCategory
	{
		Road = 0,
		Highway = 1,
		PublicTransport = 2,
	}
}
