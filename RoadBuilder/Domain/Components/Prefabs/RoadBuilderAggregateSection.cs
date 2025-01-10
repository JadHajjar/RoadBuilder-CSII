using Game.Prefabs;

using System;

namespace RoadBuilder.Domain.Components.Prefabs
{
	[Serializable]
	public class RoadBuilderAggregateSection
	{
		public NetSectionPrefab? Section;
		public bool Invert;
		public NetPieceRequirements[] PieceRequireAll = new NetPieceRequirements[0];
		public NetPieceRequirements[] PieceRequireAny = new NetPieceRequirements[0];
		public NetPieceRequirements[] PieceRequireNone = new NetPieceRequirements[0];
	}
}
