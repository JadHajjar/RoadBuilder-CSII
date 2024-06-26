using RoadBuilder.Domain.Enums;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoadBuilder.Domain.UI
{
	public class RoadPropertiesUIBinder
	{
		public string Name;
		public float SpeedLimit;
		public bool GeneratesTrafficLights;
		public bool GeneratesZoningBlocks;
		public float MaxSlopeSteepness;
		public string AggregateType;
		public RoadCategory Category;
	}
}
