using ColossalFramework;
using System.Collections.Generic;
using UnityEngine;

namespace BuildingThemes
{
    public class BuildingVariationManager : Singleton<BuildingVariationManager>
    {
        private readonly Dictionary<string, BuildingInfo> variationToBase = new Dictionary<string, BuildingInfo>();

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
            foreach (Transform child in transform)
            {
                BuildingInfo info = child.gameObject.GetComponent<BuildingInfo>();

                if (info != null) info.DestroyPrefab();

                GameObject.Destroy(child.gameObject);
            }

            variationToBase.Clear();
        }
        
        public bool IsVariation(string prefabName) 
        {
            return variationToBase.ContainsKey(prefabName);
        }

        public string GetBasePrefabName(string prefabName)
        {
            if(variationToBase.ContainsKey(prefabName))
            {
                return variationToBase[prefabName].name;
            }

            return null;
        }
        
        public Dictionary<string, BuildingInfo> CreateVariations(BuildingInfo prefab)
        {
            var maxAllowedLevel = getMaxAllowedLevel(prefab.m_class);

            var prefabVariations = new Dictionary<string, BuildingInfo>();

            if (Enabled)
            {
                foreach (var theme in BuildingThemesManager.instance.GetAllThemes())
                {
                    foreach (var variation in theme.getVariations(prefab.name)) 
                    {
                        if (prefabVariations.ContainsKey(variation.name)) continue;

                        if (variation.level < 1 || variation.level > maxAllowedLevel) continue;

                        var variationClass = ScriptableObject.CreateInstance<ItemClass>();

                        variationClass.m_layer = prefab.m_class.m_layer;
                        variationClass.m_service = prefab.m_class.m_service;
                        variationClass.m_subService = prefab.m_class.m_subService;
                        variationClass.m_level = (ItemClass.Level)(variation.level - 1);

                        var prefabVariation = BuildingInfo.Instantiate(prefab);
                        prefabVariation.name = variation.name;
                        prefabVariation.m_class = variationClass;

                        // also clone generatedInfo (else this would be shared between original and clone = problem)
                        prefabVariation.m_generatedInfo = BuildingInfoGen.Instantiate(prefab.m_generatedInfo);
                        prefabVariation.m_generatedInfo.name = variation.name + " (GeneratedInfo)";
                        prefabVariation.m_generatedInfo.m_buildingInfo = null;

                        // disable rendering of the buildinginfo
                        prefabVariation.gameObject.SetActive(false);

                        // This line is evil and removing it is killing the game's performances
                        prefabVariation.transform.parent = prefab.transform;

                        variationToBase.Remove(variation.name);
                        variationToBase.Add(variation.name, prefab);
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
