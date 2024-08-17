using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoadBuilder.Domain.Enums
{
	public enum NetPrefabGenerationPhase
	{
		None,
		NetLaneCreation,
		NetPieceCreation,
		NetSectionCreation,
		NetGroupsCreation,
		Complete
	}
}
