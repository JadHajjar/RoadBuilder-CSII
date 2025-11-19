using Game.Prefabs;
using Game.UI.Editor;
using Game.UI.Widgets;

using RoadBuilder.Domain.Prefabs;

using System;
using System.Collections.Generic;

using Unity.Entities;

namespace RoadBuilder.Domain.Components.Prefabs
{
	[ComponentMenu("RoadBuilder/", new Type[] { typeof(NetSectionPrefab), typeof(LaneGroupPrefab) })]
	public class RoadBuilderLaneDecorationInfo : ComponentBase
	{
		[CustomField(typeof(UIIconField))]
		public string? GrassThumbnail;
		[CustomField(typeof(UIIconField))]
		public string? TreeThumbnail;
		[CustomField(typeof(UIIconField))]
		public string? GrassAndTreeThumbnail;
		public string[]? LaneGrassThumbnail;
		public string[]? LaneTreeThumbnail;
		public string[]? LaneGrassAndTreeThumbnail;
		
		public override void GetArchetypeComponents(HashSet<ComponentType> components)
		{ }

		public override void GetPrefabComponents(HashSet<ComponentType> components)
		{ }
	}
}
