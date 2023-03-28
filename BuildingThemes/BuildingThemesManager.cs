﻿using System.Collections.Generic;
using System.Linq;
using ColossalFramework;
using System;
using ColossalFramework.Plugins;
using System.IO;
using UnityEngine;

namespace BuildingThemes
{
    public class BuildingThemesManager : Singleton<BuildingThemesManager>
    {
        // From BuildingManager.
        private const int AreaBuildingsLength = 4480;

        private const string ModConfigPath = "BuildingThemes.xml";
        private readonly DistrictThemeInfo[] districtThemeInfos = new DistrictThemeInfo[128];
        private readonly FastList<ushort>[] m_areaBuildings = new FastList<ushort>[AreaBuildingsLength];
        private bool m_areaBuildingsDirty = true;
        private bool importedModThemes = false;
        private bool importedStyles = false;

        private class DistrictThemeInfo
        {
            public bool blacklistMode = false;

            public readonly HashSet<Configuration.Theme> themes = new HashSet<Configuration.Theme>();

            // similar to BuildingManager.m_areaBuildings, but separate for every district
            public readonly FastList<ushort>[] areaBuildings = new FastList<ushort>[AreaBuildingsLength];

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
                    try
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

                        Debugger.xmlCorrupt = false;
                    }
                    catch
                    {
                        Debugger.xmlCorrupt = true;
                    }
                }

                return _configuration;
            }
        }

        internal void SaveConfig()
        {
            if (_configuration != null) Configuration.Serialize(userConfigPath, _configuration);
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
            importedModThemes = false;
            importedStyles = false;

        }

        public void ImportThemes() 
        {
            if (!importedModThemes) ImportThemesFromThemeMods();
            if(!importedStyles) ImportStylesAsThemes();
        }


        public void ImportThemesFromThemeMods()
        {
            foreach (var pluginInfo in Singleton<PluginManager>.instance.GetPluginsInfo().Where(pluginInfo => pluginInfo.isEnabled))
            {
                try
                {
                    var config = Configuration.Deserialize(Path.Combine(pluginInfo.modPath, ModConfigPath));
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
                    Debugger.LogException(e);
                }
            }

            importedModThemes = true;
        }

        public void ImportStylesAsThemes()
        {
            var styles = DistrictManager.instance.m_Styles;
            if (styles == null)
            {
                return;
            }
            foreach (var style in styles)
            {
                try
                {
                    AddStyleTheme(style);
                }
                catch (Exception e)
                {
                    Debugger.Log("Error while importing style " + style.FullName);
                    Debugger.LogException(e);
                }
            }
            for (byte districtId = 0; districtId < DistrictManager.instance.m_districts.m_buffer.Length; ++districtId)
            {
                var district = DistrictManager.instance.m_districts.m_buffer[districtId];
                if (district.m_flags == District.Flags.None || district.m_Style <= 0)
                {
                    continue; // 0 is the default style
                }
                var style = DistrictManager.instance.m_Styles[district.m_Style - 1];
                var stylePackage = style.PackageName;
                Singleton<DistrictManager>.instance.m_districts.m_buffer[districtId].m_Style = 0;
                ToggleThemeManagement(districtId, true);
                var theme = GetThemeByStylePackage(stylePackage);
                EnableTheme(districtId, theme);
                Debugger.LogFormat("Theme \"{0}\" was enabled for districtId={1} instead of style \"{2}\" (packageName={3})",
                    theme.name, districtId, style.Name, style.PackageName);
            }

            importedStyles = true;
        }

        private void AddModTheme(Configuration.Theme modTheme, string modName)
        {
            if (modTheme == null)
            {
                return;
            }
            var theme = AddImportedTheme(modTheme.buildings, modTheme.name, null);

            Debugger.LogFormat(
                "Imported theme from mod \"{0}\" as theme \"{1}\". Buildings in mod: {2}. Buildings in theme: {3} ",
                modTheme.buildings.Count,
                modName, theme.name, theme.buildings.Count);
        }


        private void AddStyleTheme(DistrictStyle style)
        {
            if (style.Name == DistrictStyle.kEuropeanStyleName) return; //skip builtin style

            var buildingInfos = style.GetBuildingInfos();
            List<Configuration.Building> buildings = null;
            if (buildingInfos != null)
            {
                buildings = buildingInfos.Select(buildingInfo => new Configuration.Building
                {
                    name = buildingInfo.name, fromStyle = true
                }).ToList();
            }
            var theme = AddImportedTheme(buildings, FormatStyleName(style), style.PackageName);


            Debugger.LogFormat(
                "Imported style \"{0}\" as theme \"{1}\". Buildings in style: {2}. Buildings in theme: {3} ",
                style.FullName, theme.name, buildingInfos?.Length ?? 0,
                theme.buildings.Count);
        }

        private static string FormatStyleName(DistrictStyle style)
        {
            if (style.BuiltIn)
            {
                if (style.Name == DistrictStyle.kEuropeanStyleName) return "European";
                if (style.Name == DistrictStyle.kEuropeanSuburbiaStyleName) return "European Suburbia";
            }

            return style.Name;
        }

        private Configuration.Theme AddImportedTheme(List<Configuration.Building> builtInBuildings, string themeName, string stylePackage)
        {
            var theme = Configuration.getTheme(themeName);
            if (theme == null)
            {
                theme = new Configuration.Theme
                {
                    name = themeName,
                    stylePackage = stylePackage
                };
                Configuration.themes.Add(theme);
            }
            theme.isBuiltIn = true;
            if (builtInBuildings != null)
            {
                foreach (var builtInBuilding in builtInBuildings)
                {
                    var building = theme.getBuilding(builtInBuilding.name);
                    if (building == null)
                    {
                        building = new Configuration.Building(builtInBuilding);
                        theme.buildings.Add(building);
                    }
                    else if (building.builtInBuilding == null)
                    {
                        building.builtInBuilding = builtInBuilding;
                    }
                    building.fromStyle = builtInBuilding.fromStyle;
                }
            }

            return theme;
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

        public void RefreshDistrictThemeInfos()
        {
            for (byte d = 0; d < districtThemeInfos.Length; d++)
            {
                var info = districtThemeInfos[d];
                if (info == null) continue;

                // Remove themes which are no longer listed in the configuration
                info.themes.RemoveWhere(theme => !Configuration.themes.Contains(theme));

                CompileDistrictThemes(d);
            }
        }

        public void setThemeInfo(byte districtId, HashSet<Configuration.Theme> themes, bool blacklistMode)
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
                info.upgradeBuildings.Clear();
            }

            info.blacklistMode = blacklistMode;

            // Add the themes to the district theme info
            info.themes.UnionWith(themes);

            CompileDistrictThemes(districtId);
        }

        //TODO move this method to DistrictThemeInfo class?
        public void CompileDistrictThemes(byte districtId)
        {
            var info = districtThemeInfos[districtId];

            if (info == null) return;

            HashSet<Configuration.Theme> enabledThemes = info.themes;
            HashSet<Configuration.Theme> blacklistedThemes = null;

            if (info.blacklistMode)
            {
                blacklistedThemes = new HashSet<Configuration.Theme>(GetAllThemes());
                blacklistedThemes.ExceptWith(info.themes);
            }

            if (Debugger.Enabled)
            {
                Debugger.LogFormat("Compiling theme data for district {0}. Enabled Themes: {1}, Blacklist Themes: {2}",
                    districtId, enabledThemes.Count, blacklistedThemes == null ? 0 : blacklistedThemes.Count);
            }

            // Create custom areaBuildings fastlist array for this district
            RefreshAreaBuildings(info.areaBuildings, enabledThemes, blacklistedThemes, true);

            // Create upgrade mapping
            info.upgradeBuildings.Clear();
            foreach (var theme in enabledThemes)
            {
                foreach (var building in theme.buildings)
                {
                    if (building.upgradeName == null) continue;

                    var fromPrefab = PrefabCollection<BuildingInfo>.FindLoaded(building.name);
                    var toPrefab = PrefabCollection<BuildingInfo>.FindLoaded(building.upgradeName);

                    if (fromPrefab != null && toPrefab != null && !info.upgradeBuildings.ContainsKey((ushort)fromPrefab.m_prefabDataIndex))
                    {
                        info.upgradeBuildings.Add((ushort)fromPrefab.m_prefabDataIndex, (ushort)toPrefab.m_prefabDataIndex);
                    }
                }
            }

            if (Debugger.Enabled)
            {
                Debugger.LogFormat("Upgrade Mappings in district {0}: {1}", districtId, info.upgradeBuildings.Count);
            }
        }

        public void ToggleThemeManagement(byte districtId, bool enabled)
        {
            if (enabled == IsThemeManagementEnabled(districtId)) return;

            if (enabled)
            {
                setThemeInfo(districtId, getDefaultThemes(districtId), IsBlacklistModeEnabled(0));
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

        public void ToggleBlacklistMode(byte districtId, bool enabled)
        {
            if (!IsThemeManagementEnabled(districtId) || enabled == IsBlacklistModeEnabled(districtId)) return;

            districtThemeInfos[districtId].blacklistMode = enabled;
            CompileDistrictThemes(districtId);
        }

        public bool IsBlacklistModeEnabled(byte districtId)
        {
            var info = districtThemeInfos[districtId];

            if (info != null)
            {
                return info.blacklistMode;
            }
            else if (districtId != 0)
            {
                return IsBlacklistModeEnabled(0);
            }
            else
            {
                return false;
            }
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

        private void RefreshAreaBuildings(FastList<ushort>[] m_areaBuildings, HashSet<Configuration.Theme> enabledThemes, HashSet<Configuration.Theme> blacklistedThemes, bool includeVariations)
        {
            int areaBuildingsLength = m_areaBuildings.Length;
            for (int i = 0; i < areaBuildingsLength; i++)
            {
                m_areaBuildings[i] = null;
            }
            int prefabCount = PrefabCollection<BuildingInfo>.PrefabCount();
            for (int j = 0; j < prefabCount; j++)
            {
                BuildingInfo prefab = PrefabCollection<BuildingInfo>.GetPrefab((uint)j);
                if (prefab != null && prefab.m_class.m_service != ItemClass.Service.None && prefab.m_placementStyle == ItemClass.Placement.Automatic && prefab.m_class.m_service <= ItemClass.Service.Office)
                {
                    int privateServiceIndex = ItemClass.GetPrivateServiceIndex(prefab.m_class.m_service);

                    if (privateServiceIndex != -1) {
                        if (prefab.m_cellWidth < 1 || prefab.m_cellWidth > 4 || prefab.m_cellLength < 1 || prefab.m_cellLength > 4)
                        {
                            continue;
                        }
                        else
                        {
                            // mod begin
                            if (!includeVariations && BuildingVariationManager.instance.IsVariation(prefab.name)) continue;

                            int spawnRateSum = 0;
                            int hits = 0;

                            if (enabledThemes != null && enabledThemes.Count > 0)
                            {
                                foreach (var theme in enabledThemes)
                                {
                                    var building = theme.getBuilding(prefab.name);

                                    if (building != null && building.include)
                                    {
                                        hits++;
                                        // limit spawn rate to 50
                                        spawnRateSum += Mathf.Clamp(building.spawnRate, 0, 100);
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                spawnRateSum = 1;
                                hits = 1;
                            }

                            if (hits == 0 && blacklistedThemes != null)
                            {
                                bool onBlacklist = false;

                                foreach (var theme in blacklistedThemes)
                                {
                                    var building = theme.getBuilding(prefab.name);

                                    if (building != null && building.include)
                                    {
                                        onBlacklist = true;
                                        break;
                                    }
                                }

                                if (onBlacklist)
                                {
                                    continue;
                                }
                                else
                                {
                                    spawnRateSum = 10;
                                    hits = 1;
                                }
                            }

                            if (hits == 0 || spawnRateSum == 0)
                            {
                                continue;
                            }

                            // mod end

                            int areaIndex = GetAreaIndex(prefab.m_class.m_service, prefab.m_class.m_subService, prefab.m_class.m_level, prefab.m_cellWidth, prefab.m_cellLength, prefab.m_zoningMode);
                            if (m_areaBuildings[areaIndex] == null)
                            {
                                m_areaBuildings[areaIndex] = new FastList<ushort>();
                            }

                            // mod begin
                            int spawnRate = spawnRateSum / hits;
                            for (uint s = 0; s < spawnRate; s++)
                            {
                                // mod end
                                m_areaBuildings[areaIndex].Add((ushort)j);
                                // mod begin
                            }
                            // mod end
                        }
                    }
                }
            }
            int num3 = 24;
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
                            FastList<ushort> fastList = m_areaBuildings[num4];
                            FastList<ushort> fastList2 = m_areaBuildings[num4 - 2];
                            if (fastList2 != null)
                            {
                                if (fastList == null)
                                {
                                    m_areaBuildings[num4] = fastList2;
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
        }

        public static int GetAreaIndex(ItemClass.Service service, ItemClass.SubService subService, ItemClass.Level level, int width, int length, BuildingInfo.ZoningMode zoningMode)
        {
            int privateSubServiceIndex = ItemClass.GetPrivateSubServiceIndex(subService);
            int num = (int)((privateSubServiceIndex == -1 ? ItemClass.GetPrivateServiceIndex(service) : 8 + privateSubServiceIndex) * 5 + level);
            return zoningMode != BuildingInfo.ZoningMode.CornerRight ? (int)(((num * 4 + width - 1) * 4 + length - 1) * 2 + zoningMode) : ((num * 4 + length - 1) * 4 + width - 1) * 2 + 1;

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
                if (m_areaBuildingsDirty)
                {
                    RefreshAreaBuildings(m_areaBuildings, null, null, false);
                    m_areaBuildingsDirty = false;
                }
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
            return Configuration.themes.FirstOrDefault(theme => theme.name == themeName);
        }

        private Configuration.Theme GetThemeByStylePackage(string stylePackage)
        {
            return Configuration.themes.FirstOrDefault(theme => (theme.stylePackage == stylePackage || (stylePackage == DistrictStyle.kEuropeanStyleName && theme.name == "European")));
        }
    }
}
