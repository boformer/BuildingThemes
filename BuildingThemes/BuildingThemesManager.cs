using System.Collections.Generic;
using System.Linq;
using ColossalFramework;

namespace BuildingThemes
{
    public class BuildingThemesManager : Singleton<BuildingThemesManager>
    {

        private readonly Dictionary<int, HashSet<Configuration.Theme>> districtsThemes =
            new Dictionary<int, HashSet<Configuration.Theme>>(128);
        private readonly Dictionary<int, HashSet<string>> mergedThemes =
            new Dictionary<int, HashSet<string>>(128);

        private Configuration configuration;


        public BuildingThemesManager()
        {
            configuration = Configuration.Deserialize("BuildingThemes.xml");
            if (configuration == null)
            {
                configuration = Configuration.GenerateDefaultConfig();
            }
            for (int i = 0; i < 128; ++i)
            {
                foreach (var theme in configuration.themes)
                {
                    EnableTheme(i, theme, false);
                }
                MergeDistrictThemes(i);
            }
        }


        public void EnableTheme(int districtIdx, Configuration.Theme theme, bool autoMerge)
        {
            if (!districtsThemes[districtIdx].Add(theme))
            {
                return;
            }
            if (autoMerge)
            {
                MergeDistrictThemes(districtIdx);
            }
        }

        public void DisableTheme(int districtIdx, string themeName, bool autoMerge)
        {
            var themes = districtsThemes[districtIdx];
            if (themes.RemoveWhere(theme => theme.name.Equals(themeName)) <= 0)
            {
                return;
            }
            if (autoMerge)
            {
                MergeDistrictThemes(districtIdx);      
            }
        }



        public void MergeDistrictThemes(int districtIdx)
        {
            var themes = districtsThemes[districtIdx];
            var mergedTheme = MergeThemes(themes);
            mergedThemes[districtIdx] = mergedTheme;

        }

        private static HashSet<string> MergeThemes(HashSet<Configuration.Theme> themes)
        {
            var mergedTheme = new HashSet<string>();
            foreach (var building in themes.SelectMany(theme => theme.buildings))
            {
                mergedTheme.Add(building.name);
            }
            return mergedTheme;
        }


        public HashSet<string> GetAvailableBuidlings(int districtIdx)
        {
            return mergedThemes[districtIdx];
        }

        public HashSet<Configuration.Theme> GetDistrictThemes(int districtIdx)
        {
            return districtsThemes[districtIdx];
        }

    }
}