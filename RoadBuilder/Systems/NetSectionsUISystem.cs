using Colossal.Serialization.Entities;

using Game;
using Game.Prefabs;
using Game.SceneFlow;
using Game.Tools;
using Game.UI;
using Game.UI.InGame;

using RoadBuilder.Domain.UI;
using RoadBuilder.Utilities;

using System.Collections.Generic;

using Unity.Collections;
using Unity.Entities;

namespace RoadBuilder.Systems
{
	public partial class NetSectionsUISystem : ExtendedUISystemBase
	{
		private PrefabSystem prefabSystem;
		private PrefabUISystem prefabUISystem;
		private ValueBindingHelper<NetSectionItem[]> _NetSections;

		protected override void OnCreate()
		{
			base.OnCreate();

			prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
			prefabUISystem = World.GetOrCreateSystemManaged<PrefabUISystem>();

			_NetSections = CreateBinding("NetSections", new NetSectionItem[0]);
		}

		protected override void OnGameLoadingComplete(Purpose purpose, GameMode mode)
		{
			base.OnGameLoadingComplete(purpose, mode);

			if (mode != GameMode.Game)
			{
				return;
			}

			var entityQuery = SystemAPI.QueryBuilder()
				.WithAll<NetSectionData>()
				.WithOptions(EntityQueryOptions.IncludePrefab)
				.Build();

			var entities = entityQuery.ToEntityArray(Allocator.Temp);
			var sections = new List<NetSectionItem>();

			for (var i = 0; i < entities.Length; i++)
			{
				if (!prefabSystem.TryGetPrefab<PrefabBase>(entities[i], out var prefab) || prefab is not NetSectionPrefab)
				{
					continue;
				}

				sections.Add(new NetSectionItem
				{
					PrefabName = prefab.name,
					DisplayName = GetAssetName(prefab),
					Thumbnail = ImageSystem.GetThumbnail(prefab)
				});
			}

			_NetSections.Value = sections.ToArray();
		}

		private string GetAssetName(PrefabBase prefab)
		{
			prefabUISystem.GetTitleAndDescription(prefab, out var titleId, out var _);

			if (GameManager.instance.localizationManager.activeDictionary.TryGetValue(titleId, out var name))
			{
				return name;
			}

			return prefab.name.Replace('_', ' ').FormatWords();
		}
	}
}
