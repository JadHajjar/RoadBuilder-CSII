using Game.Prefabs;

using RoadBuilder.Domain.Enums;
using RoadBuilder.Domain.Prefabs;

using System;
using System.Collections.Generic;
using System.Linq;

using Unity.Entities;

using UnityEngine;

namespace RoadBuilder.Domain.Components.Prefabs
{
	[ComponentMenu("RoadBuilder/", new Type[] { typeof(NetSectionPrefab), typeof(LaneGroupPrefab) })]
	public class RoadBuilderLaneInfo : ComponentBase
	{
		public RoadCategory RequireAll;
		public RoadCategory RequireAny;
		public RoadCategory RequireNone;
		public Color LaneColor;
		public string BackThumbnail;
		public string FrontThumbnail;
		public string[] LaneThumbnails;

		public override void GetArchetypeComponents(HashSet<ComponentType> components)
		{ }

		public override void GetPrefabComponents(HashSet<ComponentType> components)
		{ }

		public RoadBuilderLaneInfo WithRequired(RoadCategory roadCategory)
		{
			RequireAll = roadCategory;
			return this;
		}

		public RoadBuilderLaneInfo WithAny(RoadCategory roadCategory)
		{
			RequireAny = roadCategory;
			return this;
		}

		public RoadBuilderLaneInfo WithExcluded(RoadCategory roadCategory)
		{
			RequireNone = roadCategory;
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

		public RoadBuilderLaneInfo AddLaneThumbnail(string thumbnail)
		{
			if (LaneThumbnails is null)
			{
				LaneThumbnails = new[] { thumbnail };
			}
			else
			{
				LaneThumbnails = LaneThumbnails.Append(thumbnail).ToArray();
			}

			return this;
		}

		public RoadBuilderLaneInfo WithColor(byte r, byte g, byte b, byte a = 255)
		{
			LaneColor = new Color(r / 255f, g / 255f, b / 255f, a / 255f);
			return this;
		}
	}
}
