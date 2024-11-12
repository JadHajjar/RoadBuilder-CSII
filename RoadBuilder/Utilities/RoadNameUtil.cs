using Colossal;

using Game.SceneFlow;
using Game.UI.InGame;

using RoadBuilder.Domain.Prefabs;
using RoadBuilder.Systems;
using RoadBuilder.Systems.UI;

using System.Collections.Generic;
using System.Linq;

namespace RoadBuilder.Utilities
{
	public class RoadNameUtil : IDictionarySource
	{
		private readonly RoadBuilderSystem _roadBuilderSystem;
		private readonly RoadBuilderUISystem _roadBuilderUISystem;
		private readonly RoadBuilderNetSectionsSystem _netSectionsSystem;

		public RoadNameUtil(RoadBuilderSystem roadBuilderSystem, RoadBuilderUISystem roadBuilderUISystem, RoadBuilderNetSectionsSystem netSectionsSystem)
		{
			_roadBuilderSystem = roadBuilderSystem;
			_roadBuilderUISystem = roadBuilderUISystem;
			_netSectionsSystem = netSectionsSystem;

			foreach (var localeId in GameManager.instance.localizationManager.GetSupportedLocales())
			{
				GameManager.instance.localizationManager.AddSource(localeId, this);
			}
		}

		public IEnumerable<KeyValuePair<string, string>> ReadEntries(IList<IDictionaryEntryError> errors, Dictionary<string, int> indexCounts)
		{
			foreach (var item in _roadBuilderSystem.Configurations.Values)
			{
				_roadBuilderUISystem.GetTitleAndDescription(item.Prefab, out var titleId, out var descriptionId);

				yield return new(titleId, item.Config.Name);
				yield return new(descriptionId, GetRoadDescription(item));
			}
		}

		private string GetRoadDescription(INetworkBuilderPrefab item)
		{
			var width = item.Prefab.m_Sections.Sum(x => x.m_Section.CalculateWidth());
			var units = width / 8f;

			return $"{(item.Config.IsOneWay() ? "One-Way" : "Two-Way")} {GetRoadTypeName(item)} - {width}m / {units:0.#}U\r\n" +
				$"{string.Join(", ", EnumerateLaneNames(item))}" +
				$"\r\nMade with Road Builder";
		}

		private IEnumerable<string> EnumerateLaneNames(INetworkBuilderPrefab item)
		{
			var lastName = string.Empty;
			var lastDirection = 0;
			var count = 0;

			foreach (var lane in item.Config.Lanes)
			{
				var name = _roadBuilderUISystem.GetLaneName(item.Config, lane);
				var direction = lane.Invert ? -1 : 1;

				if (lastName != string.Empty && (lastName != name || lastDirection != direction))
				{
					yield return $"{count}{(lastDirection == 1 ? "F" : "B")} {lastName}";

					count = 0;
				}

				lastName = name;
				lastDirection = direction;
				count++;
			}

			yield return $"{count}{(lastDirection == 1 ? "F" : "B")} {lastName}";
		}

		private string GetRoadTypeName(INetworkBuilderPrefab item)
		{
			return item.Config.Category.ToString();
		}

		public void Unload()
		{ }
	}
}
