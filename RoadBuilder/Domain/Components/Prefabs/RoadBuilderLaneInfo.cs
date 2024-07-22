using Game.Prefabs;

using RoadBuilder.Domain.Enums;
using RoadBuilder.Domain.Prefabs;

using System;
using System.Collections.Generic;

using Unity.Entities;

using UnityEngine;

namespace RoadBuilder.Domain.Components.Prefabs
{
	[ComponentMenu("RoadBuilder/", new Type[] { typeof(NetSectionPrefab), typeof(LaneGroupPrefab) })]
	public class RoadBuilderLaneInfo : ComponentBase
	{
		public RoadCategory RequiredCategories;
		public RoadCategory AnyCategories;
		public RoadCategory ExcludedCategories;
		public string BackThumbnail;
		public string FrontThumbnail;
		public Color LaneColor;

		public override void GetArchetypeComponents(HashSet<ComponentType> components)
		{ }

		public override void GetPrefabComponents(HashSet<ComponentType> components)
		{ }

		public RoadBuilderLaneInfo WithRequired(RoadCategory roadCategory)
		{
			RequiredCategories = roadCategory;
			return this;
		}

		public RoadBuilderLaneInfo WithAny(RoadCategory roadCategory)
		{
			AnyCategories = roadCategory;
			return this;
		}

		public RoadBuilderLaneInfo WithExcluded(RoadCategory roadCategory)
		{
			ExcludedCategories = roadCategory;
			return this;
		}

		public RoadBuilderLaneInfo WithFrontThumbnail(string thumbnail)
		{
			FrontThumbnail = thumbnail;
			return this;
		}

		public RoadBuilderLaneInfo WithBackThumbnail(string thumbnail)
		{
			BackThumbnail = thumbnail;
			return this;
		}

		public RoadBuilderLaneInfo WithThumbnail(string thumbnail)
		{
			FrontThumbnail = BackThumbnail = thumbnail;
			return this;
		}

		public RoadBuilderLaneInfo WithColor(byte r, byte g, byte b, byte a = 255)
		{
			LaneColor = new Color(r / 255f, g / 255f, b / 255f, a / 255f);
			return this;
		}
	}
}
