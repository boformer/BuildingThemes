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

        private readonly FastList<ushort>[] m_areaBuildings = new FastList<ushort>[2720];
        private bool m_areaBuildingsDirty = true;

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

            for (int i = 0; i < m_areaBuildings.Length; i++)
            {
                m_areaBuildings[i] = null;
            }

            _configuration = null;
            m_areaBuildingsDirty = true;
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
            RefreshAreaBuildings();

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

        public BuildingInfo GetUpgradeBuildingInfo(ushort prefabIndex, byte districtId)
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

	    private void RefreshAreaBuildings()
	    {
		    if (this.m_areaBuildingsDirty)
		    {
			    int num = this.m_areaBuildings.Length;
			    for (int i = 0; i < num; i++)
			    {
				    this.m_areaBuildings[i] = null;
			    }
			    int num2 = PrefabCollection<BuildingInfo>.PrefabCount();
			    for (int j = 0; j < num2; j++)
			    {
				    BuildingInfo prefab = PrefabCollection<BuildingInfo>.GetPrefab((uint)j);
				    if (prefab != null && prefab.m_class.m_service != ItemClass.Service.None && prefab.m_placementStyle == ItemClass.Placement.Automatic && prefab.m_class.m_service <= ItemClass.Service.Office)
				    {
					    if (prefab.m_cellWidth < 1 || prefab.m_cellWidth > 4)
					    {
						    string text = PrefabCollection<BuildingInfo>.PrefabName((uint)j);
						    CODebugBase<LogChannel>.Error(LogChannel.Core, string.Concat(new object[]
						    {
							    "Invalid width (",
							    text,
							    "): ",
							    prefab.m_cellWidth
						    }));
					    }
					    else if (prefab.m_cellLength < 1 || prefab.m_cellLength > 4)
					    {
						    string text2 = PrefabCollection<BuildingInfo>.PrefabName((uint)j);
						    CODebugBase<LogChannel>.Error(LogChannel.Core, string.Concat(new object[]
						    {
							    "Invalid length (",
							    text2,
							    "): ",
							    prefab.m_cellLength
						    }));
					    }
					    else
					    {
						    int areaIndex = GetAreaIndex(prefab.m_class.m_service, prefab.m_class.m_subService, prefab.m_class.m_level, prefab.m_cellWidth, prefab.m_cellLength, prefab.m_zoningMode);
						    if (this.m_areaBuildings[areaIndex] == null)
						    {
							    this.m_areaBuildings[areaIndex] = new FastList<ushort>();
						    }
						    this.m_areaBuildings[areaIndex].Add((ushort)j);
					    }
				    }
			    }
			    int num3 = 17;
			    for (int k = 0; k < num3; k++)
			    {
				    for (int l = 0; l < 5; l++)
				    {
					    for (int m = 0; m < 4; m++)
					    {
						    for (int n = 1; n < 4; n++)
						    {
							    int num4 = k;
							    num4 = num4 * 5 + l;
							    num4 = num4 * 4 + m;
							    num4 = num4 * 4 + n;
							    num4 *= 2;
							    FastList<ushort> fastList = this.m_areaBuildings[num4];
							    FastList<ushort> fastList2 = this.m_areaBuildings[num4 - 2];
							    if (fastList2 != null)
							    {
								    if (fastList == null)
								    {
									    this.m_areaBuildings[num4] = fastList2;
								    }
								    else
								    {
									    for (int num5 = 0; num5 < fastList2.m_size; num5++)
									    {
										    fastList.Add(fastList2.m_buffer[num5]);
									    }
								    }
							    }
						    }
					    }
				    }
			    }
			    for (int num6 = 0; num6 < num2; num6++)
			    {
				    BuildingInfo prefab2 = PrefabCollection<BuildingInfo>.GetPrefab((uint)num6);
				    if (prefab2 != null && prefab2.m_class.m_service != ItemClass.Service.None && prefab2.m_placementStyle == ItemClass.Placement.Automatic && prefab2.m_class.m_service <= ItemClass.Service.Office)
				    {
					    if (prefab2.m_cellWidth >= 1 && prefab2.m_cellWidth <= 4)
					    {
						    if (prefab2.m_cellLength >= 1 && prefab2.m_cellLength <= 4)
						    {
							    ItemClass.Level level = ItemClass.Level.Level1;
							    if (prefab2.m_class.m_service == ItemClass.Service.Residential)
							    {
								    level = ItemClass.Level.Level5;
							    }
							    else if (prefab2.m_class.m_service == ItemClass.Service.Commercial)
							    {
								    level = ItemClass.Level.Level3;
							    }
							    else if (prefab2.m_class.m_service == ItemClass.Service.Industrial)
							    {
								    if (prefab2.m_class.m_subService == ItemClass.SubService.IndustrialGeneric)
								    {
									    level = ItemClass.Level.Level3;
								    }
							    }
							    else if (prefab2.m_class.m_service == ItemClass.Service.Office)
							    {
								    level = ItemClass.Level.Level3;
							    }
							    if (prefab2.m_class.m_level < level)
							    {
								    int areaIndex2 = GetAreaIndex(prefab2.m_class.m_service, prefab2.m_class.m_subService, prefab2.m_class.m_level + 1, prefab2.m_cellWidth, prefab2.m_cellLength, prefab2.m_zoningMode);
								    if (this.m_areaBuildings[areaIndex2] == null)
								    {
									    string str = PrefabCollection<BuildingInfo>.PrefabName((uint)num6);
									    CODebugBase<LogChannel>.Warn(LogChannel.Core, "Building cannot upgrade to next level: " + str);
								    }
							    }
							    if (prefab2.m_class.m_level > ItemClass.Level.Level1)
							    {
								    int areaIndex3 = GetAreaIndex(prefab2.m_class.m_service, prefab2.m_class.m_subService, prefab2.m_class.m_level - 1, prefab2.m_cellWidth, prefab2.m_cellLength, prefab2.m_zoningMode);
								    if (this.m_areaBuildings[areaIndex3] == null)
								    {
									    string str2 = PrefabCollection<BuildingInfo>.PrefabName((uint)num6);
									    CODebugBase<LogChannel>.Warn(LogChannel.Core, "There is no building that would upgrade to: " + str2);
								    }
							    }
						    }
					    }
				    }
			    }
			    this.m_areaBuildingsDirty = false;
		    }
        }

        public static int GetAreaIndex(ItemClass.Service service, ItemClass.SubService subService, ItemClass.Level level, int width, int length, BuildingInfo.ZoningMode zoningMode)
        {
            int areaIndex;
            if (subService != ItemClass.SubService.None)
            {
                areaIndex = 8 + subService - ItemClass.SubService.ResidentialLow;
            }
            else
            {
                areaIndex = service - ItemClass.Service.Residential;
            }
            areaIndex = (int)(areaIndex * 5 + level);
            if (zoningMode == BuildingInfo.ZoningMode.CornerRight)
            {
                areaIndex = areaIndex * 4 + length - 1;
                areaIndex = areaIndex * 4 + width - 1;
                areaIndex = areaIndex * 2 + 1;
            }
            else
            {
                areaIndex = areaIndex * 4 + width - 1;
                areaIndex = areaIndex * 4 + length - 1;
                areaIndex = (int)(areaIndex * 2 + zoningMode);
            }
            return areaIndex;
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