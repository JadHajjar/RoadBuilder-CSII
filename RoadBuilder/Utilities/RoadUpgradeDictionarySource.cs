using Colossal;
using Game.Prefabs;
using Game.SceneFlow;
using Game.UI.InGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoadBuilder.Utilities
{
    public class RoadUpgradeDictionarySource : IDictionarySource
    {

        private readonly PrefabUISystem _prefabUISystem;
        private PrefabBase[] _upgrades;


        public RoadUpgradeDictionarySource(PrefabUISystem prefabUISystem, PrefabBase[] upgrades)
        {
            _prefabUISystem = prefabUISystem;
            _upgrades = upgrades;
            foreach (var localeId in GameManager.instance.localizationManager.GetSupportedLocales())
            {
                GameManager.instance.localizationManager.AddSource(localeId, this);
            }
        }

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(IList<IDictionaryEntryError> errors, Dictionary<string, int> indexCounts)
        {
            foreach (var prefab in _upgrades)
            {
                _prefabUISystem.GetTitleAndDescription(prefab, out var titleId, out var descriptionId);
                yield return new(titleId, "Median Platforms");
                yield return new(descriptionId, "Marks each 5m median on the road as a platform, allowing citizens to walk on and board transit from it when a stop is placed.");
            }            
        }

        public void Unload()
        {}
    }
}
