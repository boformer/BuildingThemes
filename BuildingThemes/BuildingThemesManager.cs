using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ColossalFramework;

namespace BuildingThemes
{
    public class BuildingThemesManager : Singleton<BuildingThemesManager>
    {

        private readonly Dictionary<uint, HashSet<Configuration.Theme>> _districtsThemes =
            new Dictionary<uint, HashSet<Configuration.Theme>>(128);
        private readonly Dictionary<uint, HashSet<string>> _mergedThemes =
            new Dictionary<uint, HashSet<string>>(128);

        private readonly Configuration configuration;


        public BuildingThemesManager()
        {
            UnityEngine.Debug.LogFormat("Building Themes: Constructing BuildingThemesManager", Thread.CurrentThread.ManagedThreadId);
            configuration = Configuration.Deserialize("BuildingThemes.xml");
            if (configuration == null)
            {
                UnityEngine.Debug.LogFormat("Building Themes: No theme config file discovered. Generating default config");
                configuration = new Configuration();
            }

            foreach (var theme in Configuration.GetBuitInThemes())
            {
                configuration.themes.Add(theme);
            }
        }


        public void EnableTheme(uint districtIdx, Configuration.Theme theme, bool autoMerge)
        {
            UnityEngine.Debug.LogFormat("Building Themes: BuildingThemesManager. Enabling theme {0} for district {1}. auto merge: {2}",
                theme.name, districtIdx, autoMerge);
            HashSet<Configuration.Theme> themes;
            themes = GetDistrictThemes(districtIdx, true);

            if (!themes.Add(theme))
            {
                UnityEngine.Debug.LogFormat("Building Themes: BuildingThemesManager. Theme {0} was already enabled for district {1}.",
                    theme.name, districtIdx);
                return;
            }
            _districtsThemes[districtIdx] = themes;
            if (autoMerge)
            {
                MergeDistrictThemes(districtIdx);
            }
        }

        public void DisableTheme(uint districtIdx, string themeName, bool autoMerge)
        {
            UnityEngine.Debug.LogFormat("Building Themes: BuildingThemesManager. Disabling theme {0} for district {1}. auto merge: {2}",
                themeName, districtIdx, autoMerge);
            var themes = GetDistrictThemes(districtIdx, true);
            if (themes.RemoveWhere(theme => theme.name.Equals(themeName)) <= 0)
            {
                UnityEngine.Debug.LogFormat("Building Themes: BuildingThemesManager. Theme {0} was already disabled for district {1}.",
                    themeName, districtIdx);
                return;
            }
            _districtsThemes[districtIdx] = themes;
            if (autoMerge)
            {
                MergeDistrictThemes(districtIdx);
            }
        }



        public HashSet<string> MergeDistrictThemes(uint districtIdx)
        {
            UnityEngine.Debug.LogFormat("Building Themes: BuildingThemesManager. Merging themes for district {0}.", districtIdx);
            var themes = GetDistrictThemes(districtIdx, true);
            var mergedTheme = MergeThemes(themes);
            _mergedThemes[districtIdx] = mergedTheme;
            return mergedTheme;

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
            return GetDistrictThemes(districtIdx, true).Count == 0 || GetMergedThemes(districtIdx).Contains(buildingName);
        }

        private HashSet<string> GetMergedThemes(uint districtIdx)
        {
            HashSet<string> theme;
            _mergedThemes.TryGetValue(districtIdx, out theme);
            return theme ?? (_mergedThemes[districtIdx] = MergeDistrictThemes(districtIdx));
        }


        public HashSet<Configuration.Theme> GetDistrictThemes(uint districtIdx, bool initializeIfNull)
        {
            HashSet<Configuration.Theme> themes;
            _districtsThemes.TryGetValue(districtIdx, out themes);
            return themes ?? (initializeIfNull ? _districtsThemes[districtIdx] = new HashSet<Configuration.Theme>() : null);
        }

        public List<Configuration.Theme> GetAllThemes()
        {
            return configuration.themes;
        }

        public Configuration.Theme GetThemeNyName(string themeName)
        {
            var themes = configuration.themes.Where(theme => theme.name == themeName).ToList();
            return themes.Count == 0 ? null : themes[0];
        }
    }
}