using Colossal;

using Game.SceneFlow;
using Game.UI.InGame;

using RoadBuilder.Domain.Configurations;
using RoadBuilder.Systems;
using RoadBuilder.Systems.UI;

using System;
using System.Collections.Generic;
using System.Linq;

namespace RoadBuilder.Utilities
{
	public class RoadNameUtil : IDictionarySource
	{
		private readonly RoadBuilderSystem _roadBuilderSystem;
		private readonly RoadBuilderUISystem _roadBuilderUISystem;
		private readonly PrefabUISystem _prefabUISystem;
		private readonly NetSectionsSystem _netSectionsSystem;

		public RoadNameUtil(RoadBuilderSystem roadBuilderSystem, RoadBuilderUISystem roadBuilderUISystem, PrefabUISystem prefabUISystem, NetSectionsSystem netSectionsSystem)
		{
			_roadBuilderSystem = roadBuilderSystem;
			_roadBuilderUISystem = roadBuilderUISystem;
			_prefabUISystem = prefabUISystem;
			_netSectionsSystem = netSectionsSystem;

			foreach (var localeId in GameManager.instance.localizationManager.GetSupportedLocales())
			{
				GameManager.instance.localizationManager.AddSource(localeId, this);
			}
		}

		public IEnumerable<KeyValuePair<string, string>> ReadEntries(IList<IDictionaryEntryError> errors, Dictionary<string, int> indexCounts)
		{
			foreach (var item in _roadBuilderSystem.Configurations)
			{
				_prefabUISystem.GetTitleAndDescription(item.Prefab, out var titleId, out var descriptionId);

				yield return new(titleId, item.Config.Name);
				yield return new(descriptionId, string.Join(", ", item.Config.Lanes.Select(x => _roadBuilderUISystem.GetLaneName(item.Config, x))));
			}

			foreach (var item in _netSectionsSystem.LaneGroups.Where(x => x.Value.DisplayName is not null))
			{
				_prefabUISystem.GetTitleAndDescription(item.Value, out var titleId, out var descriptionId);

				yield return new(titleId, item.Value.DisplayName);
			}
		}

		public void Unload()
		{ }
	}
}
