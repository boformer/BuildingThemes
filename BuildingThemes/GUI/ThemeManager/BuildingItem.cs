using System.Text.RegularExpressions;
using System.Globalization;

using UnityEngine;
using ColossalFramework;
using ColossalFramework.Globalization;

namespace BuildingThemes.GUI
{
    public class BuildingItem
    {
        private string m_name;
        private string m_displayName;
        private string m_steamID;
        private int m_level = -1;
        private Vector2 m_size;

        public BuildingInfo prefab;
        public Configuration.Building building;

        public bool included
        {
            get { return building != null && building.include; }
        }

        public string name
        {
            get
            {
                if (m_name == null)
                {
                    if (prefab != null) m_name = prefab.name;
                    else if (building != null) m_name = building.name;
                    else m_name = string.Empty;
                }
                return m_name;
            }
        }

        public string displayName
        {
            get
            {
                if (Debugger.Enabled) return name; // display the real names in debug mode

                if (m_displayName != null) return m_displayName;

                m_displayName = Locale.GetUnchecked("BUILDING_TITLE", name);
                if (m_displayName.StartsWith("BUILDING_TITLE"))
                {
                    m_displayName = name.Substring(name.IndexOf('.') + 1).Replace("_Data", "");
                }
                m_displayName = CleanName(m_displayName, !name.Contains("."));

                return m_displayName;
            }
        }

        public Category category
        {
            get
            {
                if (prefab == null) return Category.None;

                ItemClass itemClass = prefab.m_class;
                if (itemClass.m_subService == ItemClass.SubService.ResidentialLow) return Category.ResidentialLow;
                if (itemClass.m_subService == ItemClass.SubService.ResidentialHigh) return Category.ResidentialHigh;
                if (itemClass.m_subService == ItemClass.SubService.CommercialLow) return Category.CommercialLow;
                if (itemClass.m_subService == ItemClass.SubService.CommercialHigh) return Category.CommercialHigh;
                if (itemClass.m_subService == ItemClass.SubService.CommercialLeisure) return Category.CommercialLeisure;
                if (itemClass.m_subService == ItemClass.SubService.CommercialTourist) return Category.CommercialTourism;
                if (itemClass.m_subService == ItemClass.SubService.IndustrialGeneric) return Category.Industrial;
                if (itemClass.m_subService == ItemClass.SubService.IndustrialFarming) return Category.Farming;
                if (itemClass.m_subService == ItemClass.SubService.IndustrialForestry) return Category.Forestry;
                if (itemClass.m_subService == ItemClass.SubService.IndustrialOil) return Category.Oil;
                if (itemClass.m_subService == ItemClass.SubService.IndustrialOre) return Category.Ore;
                if (itemClass.m_service == ItemClass.Service.Office) return Category.Office;

                return Category.None;
            }
        }

        public int level
        {
            get
            {
                if(m_level == -1)
                {
                    m_level = 0;
                    if (prefab != null)
                    {
                        m_level = (int)prefab.m_class.m_level + 1;
                    }
                    else
                    {
                        string cleanName = Regex.Replace(name, @"^{{.*?}}\.", "");
                        int.TryParse(Regex.Match(cleanName, @"(?<=[HL])\d").Value, out m_level);
                    }
                }
                return m_level;
            }
        }

        public int maxLevel
        {
            get
            {
                switch (category)
                {
                    case Category.None:
                    case Category.ResidentialHigh:
                    case Category.ResidentialLow:
                        return 5;
                    case Category.Farming:
                    case Category.Forestry:
                    case Category.Oil:
                    case Category.Ore:
                    case Category.CommercialLeisure:
                    case Category.CommercialTourism:
                        return 1;
                }

                return 3;
            }
        }

        public Vector2 size
        {
            get
            {
                if (m_size == Vector2.zero)
                {
                    if (prefab != null)
                    {
                        m_size = new Vector2(prefab.m_cellWidth, prefab.m_cellLength);
                    }
                    else
                    {
                        string cleanName = Regex.Replace(name, @"^{{.*?}}\.", "");
                        string size = Regex.Match(cleanName, @"\d[xX]\d").Value.ToLower();
                        if(!size.IsNullOrWhiteSpace())
                        {
                            string[] splitSize = size.Split('x');

                            int x, y;
                            int.TryParse(splitSize[0], out x);
                            int.TryParse(splitSize[0], out y);
                            m_size = new Vector2(x, y);
                        }
                    }
                }
                return m_size;
            }
        }

        public string sizeAsString
        {
            get
            {
                if (size == Vector2.zero) return "";
                return size.x + "x" + size.y;
            }
        }

        public string steamID
        {
            get
            {
                if (m_steamID != null) return m_steamID;

                if (isCustomAsset)
                {
                    m_steamID = name.Substring(0, name.IndexOf("."));

                    ulong result;
                    if (!ulong.TryParse(m_steamID, out result) || result == 0)
                        m_steamID = null;
                }

                return m_steamID;
            }
        }

        public bool isCloned
        {
            get
            {
                if (building == null) return false;

                return building.baseName != null;
            }
        }

        public bool isCustomAsset
        {
            get
            {
                string cleanName = Regex.Replace(name, @"^{{.*?}}\.", "");
                return cleanName.Contains(".");
            }
        }

        public Color32 GetStatusColor()
        {
            if (prefab == null && building != null && !isCustomAsset)
                return new Color32(128, 128, 128, 255);
            if (prefab == null)
                return new Color32(255, 255, 0, 255);
            if (building != null && building.baseName != null)
                return new Color32(50, 230, 255, 255);


            return new Color32(255, 255, 255, 255);
        }

        public static string CleanName(string name, bool cleanNumbers = false)
        {
            name = Regex.Replace(name, @"^{{.*?}}\.", "");
            name = Regex.Replace(name, @"[_+\.]", " ");
            name = Regex.Replace(name, @"(\d[xX]\d)|([HL]\d)", "");
            if (cleanNumbers)
            {
                name = Regex.Replace(name, @"(\d+[\da-z])", "");
                name = Regex.Replace(name, @"\s\d+", " ");
            }
            name = Regex.Replace(name, @"\s+", " ").Trim();

            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(name);
        }
    }
}
