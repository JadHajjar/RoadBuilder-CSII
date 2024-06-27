using RoadBuilder.Domain.Configuration;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoadBuilder.Utilities
{
	public static class RoadConfigExtensionsUtil
	{
		public static bool IsOneWay(this RoadConfig roadConfig)
		{
			return false;
		}
	}
}
