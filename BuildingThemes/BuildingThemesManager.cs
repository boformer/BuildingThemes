using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ColossalFramework;

namespace BuildingThemes
{
    public class BuildingThemesManager : Singleton<BuildingThemesManager>
    {

        private readonly Dictionary<uint, HashSet<Configuration.Theme>> districtsThemes =
            new Dictionary<uint, HashSet<Configuration.Theme>>(128);
        private readonly Dictionary<uint, HashSet<string>> mergedThemes =
            new Dictionary<uint, HashSet<string>>(128);

        private Configuration configuration;


        public BuildingThemesManager()
        {
            UnityEngine.Debug.LogFormat("Building Themes: Constructing BuildingThemesManager", Thread.CurrentThread.ManagedThreadId);
            configuration = Configuration.Deserialize("BuildingThemes.xml");
            if (configuration == null)
            {
                UnityEngine.Debug.LogFormat("Building Themes: No theme config file discovered. Generating default config");
                configuration = Configuration.GenerateDefaultConfig();
            }
            for (uint i = 0; i < 128; ++i)
            {
                districtsThemes.Add(i, new HashSet<Configuration.Theme>());
                mergedThemes.Add(i, new HashSet<string>());
                foreach (var theme in configuration.themes)
                {
                    EnableTheme(i, theme, false);
                }
                MergeDistrictThemes(i);
            }
        }


        public void EnableTheme(uint districtIdx, Configuration.Theme theme, bool autoMerge)
        {
            UnityEngine.Debug.LogFormat("Building Themes: BuildingThemesManager. Enabling theme {0} for district {1}. auto merge: {2}",
                theme.name, districtIdx, autoMerge);
            var themes = districtsThemes[districtIdx];
            if (!themes.Add(theme))
            {
                UnityEngine.Debug.LogFormat("Building Themes: BuildingThemesManager. Theme {0} was already enabled for district {1}.",
                    theme.name, districtIdx);
                return;
            }
            districtsThemes[districtIdx] = themes;
            if (autoMerge)
            {
                MergeDistrictThemes(districtIdx);
            }
        }

        public void DisableTheme(uint districtIdx, string themeName, bool autoMerge)
        {
            UnityEngine.Debug.LogFormat("Building Themes: BuildingThemesManager. Disabling theme {0} for district {1}. auto merge: {2}",
                themeName, districtIdx, autoMerge);
            var themes = districtsThemes[districtIdx];
            if (themes.RemoveWhere(theme => theme.name.Equals(themeName)) <= 0)
            {
                UnityEngine.Debug.LogFormat("Building Themes: BuildingThemesManager. Theme {0} was already disabled for district {1}.",
                    themeName, districtIdx);
                return;
            }
            districtsThemes[districtIdx] = themes;
            if (autoMerge)
            {
                MergeDistrictThemes(districtIdx);
            }
        }



        public void MergeDistrictThemes(uint districtIdx)
        {
            UnityEngine.Debug.LogFormat("Building Themes: BuildingThemesManager. Merging themes for district {0}.", districtIdx);
            var themes = districtsThemes[districtIdx];
            var mergedTheme = MergeThemes(themes);
            mergedThemes[districtIdx] = mergedTheme;

        }

        private static HashSet<string> MergeThemes(HashSet<Configuration.Theme> themes)
        {
            var mergedTheme = new HashSet<string>();
            foreach (var building in themes.SelectMany(theme => theme.buildings))
            {
                if (mergedTheme.Add(building.name))
                {
                    UnityEngine.Debug.LogFormat("Building Themes: BuildingThemesManager. Adding building {0} to merged theme.", building.name);
                }
            }
            return mergedTheme;
        }


        public bool DoesBuildingBelongToDistrict(string buildingName, uint districtIdx)
        {
            return mergedThemes[districtIdx].Contains(buildingName);
        }

        public HashSet<Configuration.Theme> GetDistrictThemes(uint districtIdx)
        {
            return districtsThemes[districtIdx];
        }

        public List<Configuration.Theme> GetAllThemes()
        {
            return configuration.themes;
        }
    }
}