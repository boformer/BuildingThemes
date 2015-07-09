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

        private readonly Dictionary<uint, HashSet<Configuration.Theme>> _districtsThemes =
            new Dictionary<uint, HashSet<Configuration.Theme>>(128);

        // similar to BuildingManager.m_areaBuildings, but separate for every district

        private readonly bool[] _districtThemesEnabled = new bool[128];
        private readonly FastList<ushort>[,] _districtAreaBuildings = new FastList<ushort>[128, 2720];
        
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

        public void Reset()
        {
            _districtsThemes.Clear();
            _configuration = null;

            for (int i = 0; i < _districtAreaBuildings.GetLength(0); i++)
            {
                for (int j = 0; j < _districtAreaBuildings.GetLength(1); j++)
                {
                    _districtAreaBuildings[i, j] = null;
                }
            }

            for (int k = 0; k < _districtThemesEnabled.Length; k++)
            {
                _districtThemesEnabled[k] = false;
            }
        }

        public void EnableTheme(uint districtIdx, Configuration.Theme theme, bool autoMerge)
        {
            if (!_districtThemesEnabled[districtIdx]) return;
            
            if (Debugger.Enabled)
            {
                Debugger.LogFormat("Building Themes: BuildingThemesManager. Enabling theme {0} for district {1}. auto merge: {2}",
                    theme.name, districtIdx, autoMerge);
            }
            HashSet<Configuration.Theme> themes;
            themes = GetDistrictThemes(districtIdx, true);

            if (!themes.Add(theme))
            {
                if (Debugger.Enabled)
                {
                    Debugger.LogFormat("Building Themes: BuildingThemesManager. Theme {0} was already enabled for district {1}.",
                        theme.name, districtIdx);
                }
                return;
            }
            SetThemes(districtIdx, themes, autoMerge);
        }

        public void DisableTheme(uint districtIdx, string themeName, bool autoMerge)
        {
            if (!_districtThemesEnabled[districtIdx]) return;
            
            if (Debugger.Enabled)
            {
                Debugger.LogFormat("Building Themes: BuildingThemesManager. Disabling theme {0} for district {1}. auto merge: {2}",
                themeName, districtIdx, autoMerge);
            }
            var themes = GetDistrictThemes(districtIdx, true);
            if (themes.RemoveWhere(theme => theme.name.Equals(themeName)) <= 0)
            {
                if (Debugger.Enabled)
                {
                    Debugger.LogFormat("Building Themes: BuildingThemesManager. Theme {0} was already disabled for district {1}.",
                        themeName, districtIdx);
                }
                return;
            }
            SetThemes(districtIdx, themes, autoMerge);
        }

        public void SetThemes(uint districtIdx, HashSet<Configuration.Theme> themes, bool autoMerge)
        {
            _districtsThemes[districtIdx] = new HashSet<Configuration.Theme>(themes);
            _districtThemesEnabled[districtIdx] = true;
            if (autoMerge)
            {
                MergeDistrictThemes(districtIdx);
            }
        }

        public void MergeDistrictThemes(uint districtIdx)
        {
            var buildingManager = Singleton<BuildingManager>.instance;
            typeof(BuildingManager).GetMethod("RefreshAreaBuildings", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(buildingManager, new object[] { });
            var AreaBuildings = (FastList<ushort>[])typeof(BuildingManager).GetField("m_areaBuildings", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(buildingManager);
            
            for (int i = 0; i < 2720; i++)
            {
                
                _districtAreaBuildings[districtIdx, i] = FilterList(districtIdx, AreaBuildings[i], GetDistrictThemes(districtIdx, true));
            }
        }

        public void ToggleThemeManagement(uint districtIdx, bool enabled)
        {
            if (enabled == _districtThemesEnabled[districtIdx]) return;

            if (enabled)
            {
                SetThemes(districtIdx, GetDistrictThemes(0, true), true);
            }
            else
            {
                _districtThemesEnabled[districtIdx] = false;
                _districtsThemes[districtIdx] = null;

                for (int i = 0; i < 2720; i++)
                {
                    _districtAreaBuildings[districtIdx, i] = null;
                }
            }
        }

        public bool IsThemeManagementEnabled(uint districtIdx)
        {
            return _districtThemesEnabled[districtIdx];
        }

        public FastList<ushort> GetAreaBuildings(int districtId, int areaIndex) 
        {
            // Theme management enabled in district? return custom fastlist for district
            if (_districtThemesEnabled[districtId])
            {
                return _districtAreaBuildings[districtId, areaIndex];
            }
            // Theme management not enabled in district? return fastlist for city-wide "district"
            else if (districtId != 0)
            {
                return GetAreaBuildings(0, areaIndex);
            }
            // Theme management not enabled in city-wide district? return fastlist of the game
            else
            {
                var buildingManager = Singleton<BuildingManager>.instance;
                var AreaBuildings = (FastList<ushort>[])typeof(BuildingManager).GetField("m_areaBuildings", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(buildingManager);
                
                return AreaBuildings[areaIndex];
            }
        }

        private FastList<ushort> FilterList(uint districtIdx, FastList<ushort> fastList, HashSet<Configuration.Theme> themes)
        {
            if (fastList == null || fastList.m_size == 0) return null;

            // no theme enabled?
            if (themes.Count == 0) return fastList;

            FastList<ushort> filteredList = new FastList<ushort>();

            for (int i = 0; i < fastList.m_size; i++) 
            {
                ushort prefabId = fastList.m_buffer[i];

                var prefab = PrefabCollection<BuildingInfo>.GetPrefab(prefabId);

                if(prefab == null) continue;

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

        private HashSet<Configuration.Theme> getDefaultDistrictThemes(uint districtIdx)
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

        public HashSet<Configuration.Theme> GetDistrictThemes(uint districtIdx, bool initializeIfNull)
        {
            if (_districtThemesEnabled[districtIdx])
            {
                HashSet<Configuration.Theme> themes;
                _districtsThemes.TryGetValue(districtIdx, out themes);
                return themes ?? (initializeIfNull ? _districtsThemes[districtIdx] = getDefaultDistrictThemes(districtIdx) : null);
            }
            else
            {
                return getDefaultDistrictThemes(districtIdx);
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