using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
