using ColossalFramework;
using System.Collections.Generic;
using UnityEngine;

namespace BuildingThemes
{
    public class BuildingVariationManager : Singleton<BuildingVariationManager>
    {
        private readonly HashSet<string> variations = new HashSet<string>();

        public static bool Enabled
        {
            get
            {
                return BuildingThemesManager.instance.Configuration.CreateBuildingDuplicates;
            }
            set
            {
                BuildingThemesManager.instance.Configuration.CreateBuildingDuplicates = value;
                BuildingThemesManager.instance.SaveConfig();
            }
        }

        public void Reset() 
        {
            variations.Clear();
        }
        
        public bool IsVariation(string prefabName) 
        {
            return variations.Contains(prefabName);
        }
        
        public Dictionary<string, BuildingInfo> CreateVariations(BuildingInfo prefab)
        {
            var prefabLevel = (int)prefab.m_class.m_level + 1;

            var maxAllowedLevel = getMaxAllowedLevel(prefab.m_class);

            var prefabVariations = new Dictionary<string, BuildingInfo>();

            if (Enabled)
            {
                foreach (var theme in BuildingThemesManager.instance.GetAllThemes())
                {
                    var building = theme.getBuilding(prefab.name); // TODO add support for service type variation?

                    if (building == null || !building.include) continue;

                    var minLevel = building.minLevel < 1 ? prefabLevel : Mathf.Min(building.minLevel, maxAllowedLevel);
                    var maxLevel = Mathf.Clamp(building.maxLevel < 1 ? prefabLevel : building.maxLevel, minLevel, maxAllowedLevel);

                    // all variations sorted by level
                    var prefabsByLevel = new string[5];

                    // check if the original prefab is in the specified level range
                    int originalLevel = (int)prefab.m_class.m_level + 1;
                    if (originalLevel >= minLevel && originalLevel <= maxLevel)
                    {
                        // yes? add it to the list of variations sorted by level
                        prefabsByLevel[(int)prefab.m_class.m_level] = prefab.name;
                    }
                    else
                    {
                        // no? prevent building from spawning
                        building.notInLevelRange = true;
                    }

                    if (building.upgrade != null && maxLevel < maxAllowedLevel) 
                    {
                        prefabsByLevel[maxLevel] = building.upgrade;
                    }


                    for (int l = minLevel; l <= maxLevel; l++)
                    {
                        var level = (ItemClass.Level)(l - 1);

                        if (level == prefab.m_class.m_level) continue;

                        var variationName = prefab.name + "#" + level;

                        prefabsByLevel[(int)level] = variationName;
                        theme.buildings.Add(new Configuration.Building { name = variationName, isBuiltIn = true });

                        variations.Add(variationName);

                        if (prefabVariations.ContainsKey(variationName)) continue;

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

                    for (int l = 0; l < 4; l++)
                    {
                        if (prefabsByLevel[l] != null && prefabsByLevel[l + 1] != null)
                        {
                            theme.upgrades.Add(prefabsByLevel[l], prefabsByLevel[l + 1]);
                        }
                    }
                }
            }

            if (Debugger.Enabled && prefabVariations.Count > 0) 
            {
                Debugger.LogFormat("Created {0} variations for prefab {1}", prefabVariations.Count, prefab.name);
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
                    if (itemClass.m_subService == ItemClass.SubService.IndustrialGeneric) return 3;
                    goto default;
                default:
                    return 1;
            }
        }
    }
}
