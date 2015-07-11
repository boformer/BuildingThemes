using ColossalFramework;
using System.Collections.Generic;
using UnityEngine;

namespace BuildingThemes
{
    public class BuildingVariationManager : Singleton<BuildingVariationManager>
    {
        // TODO store lists of variation names per theme
        // TODO upgrade mapping
        public Dictionary<string, BuildingInfo> createVariations(BuildingInfo prefab) 
        {
            var prefabLevel = (int)prefab.m_class.m_level + 1;
            var maxAllowedLevel = getMaxAllowedLevel(prefab.m_class);

            var prefabVariations = new Dictionary<string, BuildingInfo>();
            
            foreach (var theme in BuildingThemesManager.instance.GetAllThemes()) 
            {
                var building = theme.getBuilding(prefab.name); // TODO add support for service type variation?

                var minLevel = building.minLevel < 1 ? prefabLevel : Mathf.Min(building.minLevel, maxAllowedLevel);
                var maxLevel = Mathf.Clamp(building.maxLevel < 1 ? prefabLevel : building.maxLevel, minLevel, maxAllowedLevel);

                for(int l = minLevel; l <= maxLevel; l++) 
                {
                    var level = (ItemClass.Level)(l - 1);

                    if (level == prefab.m_class.m_level) continue;
                    
                    var variationName = prefab.name + "#" + level;

                    if(prefabVariations.ContainsKey(variationName)) continue;

                    var variationClass = ScriptableObject.CreateInstance<ItemClass>();

                    variationClass.m_layer = prefab.m_class.m_layer;
                    variationClass.m_service = prefab.m_class.m_service;
                    variationClass.m_subService = prefab.m_class.m_subService;
                    variationClass.m_level = level;

                    BuildingInfo prefabVariation = BuildingInfo.Instantiate(prefab);
                    prefabVariation.name = variationName;
                    prefabVariation.m_class = variationClass;

                    prefabVariations.Add(variationName, prefabVariation);
                }
            }

            return prefabVariations;
        }

        private int getMaxAllowedLevel(ItemClass itemClass) 
        {
            switch (itemClass.m_service) 
            { 
                case ItemClass.Service.Residential:
                    return 5;
                case ItemClass.Service.Commercial:
                    return 3;
                case ItemClass.Service.Office:
                    return 3;
                case ItemClass.Service.Industrial:
                    if(itemClass.m_subService == ItemClass.SubService.IndustrialGeneric) return 3;
                    goto default;
                default:
                    return 1;
            }
        }
    }
}
