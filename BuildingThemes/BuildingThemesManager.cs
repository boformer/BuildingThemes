using System.Collections.Generic;
using System.Linq;
using ColossalFramework;
using System;
using ColossalFramework.Plugins;
using System.IO;
using System.Reflection;

namespace BuildingThemes
{
    public class BuildingThemesManager : Singleton<BuildingThemesManager>
    {
        private readonly DistrictThemeInfo[] districtThemeInfos = new DistrictThemeInfo[128];

        private MethodInfo _BuildingManager_RefreshAreaBuildings;
        private FieldInfo _BuildingManager_m_areaBuildings;

        private class DistrictThemeInfo
        {
            public readonly HashSet<Configuration.Theme> themes = new HashSet<Configuration.Theme>();

            // similar to BuildingManager.m_areaBuildings, but separate for every district
            public readonly FastList<ushort>[] areaBuildings = new FastList<ushort>[2720];

            // building upgrade mapping (prefabLevel1 --> prefabLevel2) for realistic building upgrades
            public readonly Dictionary<ushort, ushort> upgradeBuildings = new Dictionary<ushort, ushort>();
        }
        
        private const string userConfigPath = "BuildingThemes.xml";
        private Configuration _configuration;
        internal Configuration Configuration
        {
            get
            {
                if (_configuration == null)
                {
                    _configuration = Configuration.Deserialize(userConfigPath);

                    if (Debugger.Enabled)
                    {
                        Debugger.Log("Building Themes: User Configuration loaded.");
                    }

                    if (_configuration == null)
                    {
                        _configuration = new Configuration();
                        SaveConfig();
                    }
                }

                return _configuration;
            }
        }

        internal void SaveConfig()
        {
            if(_configuration != null) Configuration.Serialize(userConfigPath, _configuration);
        }

        public void Reset()
        {
            for (int d = 0; d < districtThemeInfos.Length; d++) 
            {
                districtThemeInfos[d] = null;
            }
            _configuration = null;

            _BuildingManager_RefreshAreaBuildings = typeof(BuildingManager).GetMethod("RefreshAreaBuildings", BindingFlags.NonPublic | BindingFlags.Instance);
            _BuildingManager_m_areaBuildings = typeof(BuildingManager).GetField("m_areaBuildings", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        private const string modConfigPath = "BuildingThemes.xml";
        public void searchBuildingThemeMods()
        {
            foreach (var pluginInfo in Singleton<PluginManager>.instance.GetPluginsInfo())
            {
                if (!pluginInfo.isEnabled) continue;

                try
                {
                    Configuration config = Configuration.Deserialize(Path.Combine(pluginInfo.modPath, modConfigPath));

                    if (config == null)
                    {
                        continue;
                    }

                    foreach (var theme in config.themes)
                    {
                        AddModTheme(theme, pluginInfo.name);
                    }
                }
                catch (Exception e)
                {
                    Debugger.Log("Error while parsing BuildingThemes.xml of mod " + pluginInfo.name);
                    Debugger.Log(e.ToString());
                }
            }
        }

        private void AddModTheme(Configuration.Theme theme, string modName)
        {
            if (theme == null)
            {
                return;
            }

            foreach (var building in theme.buildings)
            {
                building.isBuiltIn = true;
            }

            var existingTheme = Configuration.getTheme(theme.name);

            if (existingTheme != null)
            {
                foreach (var building in theme.buildings)
                {
                    if (!existingTheme.containsBuilding(building.name))
                    {
                        existingTheme.buildings.Add(building);
                    }
                }
            }
            else
            {
                theme.isBuiltIn = true;
                Configuration.themes.Add(theme);
            }

            SaveConfig();

            Debugger.LogFormat("Building Themes: Theme {0} added by mod {1}", theme.name, modName);
        }


        public void EnableTheme(byte districtId, Configuration.Theme theme)
        {
            if (!IsThemeManagementEnabled(districtId)) return;
            
            if (Debugger.Enabled)
            {
                Debugger.LogFormat("Building Themes: BuildingThemesManager. Enabling theme {0} for district {1}.", theme.name, districtId);
            }
            var themes = GetDistrictThemes(districtId, true);

            if (!districtThemeInfos[districtId].themes.Add(theme))
            {
                if (Debugger.Enabled)
                {
                    Debugger.LogFormat("Building Themes: BuildingThemesManager. Theme {0} was already enabled for district {1}.", theme.name, districtId);
                }
                return;
            }

            CompileDistrictThemes(districtId);
        }

        public void DisableTheme(byte districtId, Configuration.Theme theme)
        {
            if (!IsThemeManagementEnabled(districtId)) return;
            
            if (Debugger.Enabled)
            {
                Debugger.LogFormat("Building Themes: BuildingThemesManager. Disabling theme {0} for district {1}.", theme.name, districtId);
            }

            if (!districtThemeInfos[districtId].themes.Remove(theme))
            {
                if (Debugger.Enabled)
                {
                    Debugger.LogFormat("Building Themes: BuildingThemesManager. Theme {0} was already disabled for district {1}.", theme.name, districtId);
                }
                return;
            }

            CompileDistrictThemes(districtId);
        }

        public void SetThemes(byte districtId, HashSet<Configuration.Theme> themes)
        {
            var info = districtThemeInfos[districtId];

            if (info == null)
            {
                info = new DistrictThemeInfo();
                districtThemeInfos[districtId] = info;
            }
            else
            {
                info.themes.Clear();
            }

            // Add the themes to the district theme info
            info.themes.UnionWith(themes);

            CompileDistrictThemes(districtId);
        }

        //TODO move this method to DistrictThemeInfo class?
        public void CompileDistrictThemes(byte districtId) 
        {
            var info = districtThemeInfos[districtId];

            if (info == null) return;
            
            // Refresh and get BuildingManager.m_areaBuildings
            var buildingManager = BuildingManager.instance;
            _BuildingManager_RefreshAreaBuildings.Invoke(buildingManager, new object[] { });
            var m_areaBuildings = (FastList<ushort>[])_BuildingManager_m_areaBuildings.GetValue(buildingManager);

            // Now filter the list for this district
            for (int i = 0; i < 2720; i++)
            {
                info.areaBuildings[i] = FilterList(districtId, m_areaBuildings[i], GetDistrictThemes(districtId, true));
            }

            // TODO compile upgrade mapping!
        }

        private FastList<ushort> FilterList(uint districtIdx, FastList<ushort> fastList, HashSet<Configuration.Theme> themes)
        {
            if (fastList == null || fastList.m_size == 0) return null;

            // no theme enabled?
            if (themes.Count == 0) return fastList;

            var filteredList = new FastList<ushort>();

            for (int i = 0; i < fastList.m_size; i++)
            {
                ushort prefabId = fastList.m_buffer[i];

                var prefab = PrefabCollection<BuildingInfo>.GetPrefab(prefabId);

                if (prefab == null) continue;

                foreach (var theme in themes)
                {
                    if (theme.containsBuilding(prefab.name))
                    {
                        filteredList.Add(prefabId);
                        break;
                    }
                }
            }

            if (filteredList.m_size == 0) return null;

            return filteredList;
        }

        public void ToggleThemeManagement(byte districtId, bool enabled)
        {
            if (enabled == IsThemeManagementEnabled(districtId)) return;

            if (enabled)
            {
                SetThemes(districtId, getDefaultThemes(districtId));
            }
            else
            {
                districtThemeInfos[districtId] = null;
            }
        }

        public bool IsThemeManagementEnabled(byte districtId)
        {
            return districtThemeInfos[districtId] != null;
        }

        public BuildingInfo GetConfiguredUpgradedBuildingInfo(ushort prefabIndex, byte districtId)
        {
            var info = districtThemeInfos[districtId];

            if (info == null)
            {
                return null;
            }

            ushort upgradePrefabIndex;
            if (!info.upgradeBuildings.TryGetValue(prefabIndex, out upgradePrefabIndex))
            {
                return null;
            }
            
            return PrefabCollection<BuildingInfo>.GetPrefab(upgradePrefabIndex);
        }

        public FastList<ushort> GetAreaBuildings(byte districtId, int areaIndex) 
        {
            var info = districtThemeInfos[districtId];
            
            // Theme management enabled in district? return custom fastlist for district
            if (info != null)
            {
                return info.areaBuildings[areaIndex];
            }
            // Theme management not enabled in district? return fastlist for city-wide "district"
            else if (districtId != 0)
            {
                return GetAreaBuildings(0, areaIndex);
            }
            // Theme management not enabled in city-wide district? return fastlist of the game
            else
            {
                var m_areaBuildings = (FastList<ushort>[])_BuildingManager_m_areaBuildings.GetValue(BuildingManager.instance);
                return m_areaBuildings[areaIndex];
            }
        }

        private HashSet<Configuration.Theme> getDefaultThemes(uint districtIdx)
        {
            var theme = new HashSet<Configuration.Theme>();

            if (districtIdx == 0)
            {
                /*
                // city-wide default derived from environment (european, sunny, boreal, tropical)

                var env = Singleton<SimulationManager>.instance.m_metaData.m_environment;

                if (env == "Europe")
                {
                    theme.Add(GetThemeByName("European"));
                }
                else
                {
                    theme.Add(GetThemeByName("International"));
                }

                if (Debugger.Enabled)
                {
                    Debugger.LogFormat("Building Themes: Environment is {0}. Selected default builtin theme.", env);
                }
                */

                // By default no theme is enabled, so custom buildings grow
            }
            else
            {
                // district theme derived from city-wide theme

                theme.UnionWith(GetDistrictThemes(0, true));

                if (Debugger.Enabled)
                {
                    Debugger.LogFormat("Building Themes: Deriving theme for district {0} from city-wide theme.", districtIdx);
                }
            }

            return theme;
        }

        public HashSet<Configuration.Theme> GetDistrictThemes(byte districtId, bool initializeIfNull)
        {
            if (IsThemeManagementEnabled(districtId))
            {
                return districtThemeInfos[districtId].themes;
            }
            else
            {
                return initializeIfNull ? getDefaultThemes(districtId) : null;
            }
        }

        public List<Configuration.Theme> GetAllThemes()
        {
            return Configuration.themes;
        }

        public Configuration.Theme GetThemeByName(string themeName)
        {
            var themes = Configuration.themes.Where(theme => theme.name == themeName).ToList();
            return themes.Count == 0 ? null : themes[0];
        }
    }
}