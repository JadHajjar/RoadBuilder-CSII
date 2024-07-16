using Game.Prefabs;

using RoadBuilder.Domain.Enums;
using RoadBuilder.Domain.Prefabs;

using System;
using System.Collections.Generic;

using Unity.Entities;

namespace RoadBuilder.Domain.Components
{
	[ComponentMenu("RoadBuilder/", new Type[] { typeof(NetSectionPrefab), typeof(LaneGroupPrefab) })]
	public class RoadBuilderLaneInfoItem : ComponentBase
	{
		public RoadCategory RequiredCategories;
		public RoadCategory ExcludedCategories;
		public string BackThumbnail;
		public string FrontThumbnail;

		public override void GetArchetypeComponents(HashSet<ComponentType> components)
		{ }

		public override void GetPrefabComponents(HashSet<ComponentType> components)
		{ }

		public RoadBuilderLaneInfoItem WithRequired(RoadCategory roadCategory)
		{
			RequiredCategories = roadCategory;
			return this;
		}

		public RoadBuilderLaneInfoItem WithExcluded(RoadCategory roadCategory)
		{
			ExcludedCategories = roadCategory;
			return this;
		}

		public RoadBuilderLaneInfoItem WithFrontThumbnail(string thumbnail)
		{
			FrontThumbnail = thumbnail;
			return this;
		}

		public RoadBuilderLaneInfoItem WithBackThumbnail(string thumbnail)
		{
			BackThumbnail = thumbnail;
			return this;
		}
	}
}
