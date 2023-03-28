using System.Collections.Generic;

namespace BuildingThemes.Data
{
    internal static class ConfigurationHelper
    {
        public static void ApplyConfiguration(DistrictsConfiguration configuration) 
        {
            var buildingThemesManager = BuildingThemesManager.instance;
            buildingThemesManager.ImportThemes();

            foreach (var district in configuration.Districts)
            {
                //skip districts which do not exist
                if (DistrictManager.instance.m_districts.m_buffer[district.id].m_flags == District.Flags.None)
                {
                    continue;
                }

                var themes = new HashSet<Configuration.Theme>();

                foreach (var themeName in district.themes)
                {
                    var theme = buildingThemesManager.GetThemeByName(themeName);
                    if (theme == null)
                    {
                        Debugger.LogFormat("Theme {0} that was enabled in district {1} could not be found!", themeName, district.id);
                        continue;
                    }
                    themes.Add(theme);
                }

                if (Debugger.Enabled)
                {
                    Debugger.LogFormat("Building Themes: Loading: {0} themes enabled for district {1}", themes.Count, district.id);
                }

                buildingThemesManager.setThemeInfo(district.id, themes, district.blacklistMode);
            }
        }
    }
}