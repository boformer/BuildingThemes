using System.Text.RegularExpressions;
using System.Globalization;

using UnityEngine;
using ColossalFramework.Globalization;

namespace BuildingThemes.GUI
{
    public enum Category
    {
        None = -1,
        ResidentialLow = 0,
        ResidentialHigh,
        CommercialLow,
        CommercialHigh,
        Industrial,
        Farming,
        Forestry,
        Oil,
        Ore,
        Office
    }

    public class BuildingItem
    {
        private string m_displayName;
        private string m_steamID;
        private string m_level = "-";
        private string m_size = "-";

        public BuildingInfo prefab;
        public Configuration.Building building;

        public bool included
        {
            get { return building != null && (!building.isBuiltIn || building.include); }
        }

        public string name
        {
            get
            {
                if (prefab != null) return prefab.name;
                if (building != null) return building.name;
                return string.Empty;
            }
        }

        public string displayName
        {
            get
            {
                if (m_displayName != null) return m_displayName;

                m_displayName = Locale.GetUnchecked("BUILDING_TITLE", name);
                if (m_displayName.StartsWith("BUILDING_TITLE"))
                {
                    m_displayName = name.Substring(m_displayName.IndexOf('.') + 1).Replace("_Data", "");
                }

                if (prefab == null) m_displayName += " (not loaded)";

                CleanDisplayName();

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
                if (itemClass.m_subService == ItemClass.SubService.IndustrialGeneric) return Category.Industrial;
                if (itemClass.m_subService == ItemClass.SubService.IndustrialFarming) return Category.Farming;
                if (itemClass.m_subService == ItemClass.SubService.IndustrialForestry) return Category.Forestry;
                if (itemClass.m_subService == ItemClass.SubService.IndustrialOil) return Category.Oil;
                if (itemClass.m_subService == ItemClass.SubService.IndustrialOre) return Category.Ore;
                if (itemClass.m_service == ItemClass.Service.Office) return Category.Office;

                return Category.None;
            }
        }

        public string level
        {
            get { return m_level; }
        }

        public string size
        {
            get { return m_size; }
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

        public bool isCustomAsset
        {
            get { return name.Contains("."); }
        }

        public Color32 GetStatusColor()
        {
            if (prefab == null && building != null && !isCustomAsset)
                return new Color32(128, 128, 128, 255);
            if (prefab == null)
                return new Color32(255, 255, 0, 255);

            return new Color32(255, 255, 255, 255);
        }

        private void CleanDisplayName()
        {
            if (prefab != null)
            {
                if (prefab.m_class.m_subService >= ItemClass.SubService.IndustrialForestry && prefab.m_class.m_subService <= ItemClass.SubService.IndustrialOre)
                    m_level = "L1";
                else
                    m_level = "L" + ((int)prefab.m_class.m_level + 1);
                m_size = prefab.m_cellWidth + "x" + prefab.m_cellLength;
            }
            else
            {
                m_level = Regex.Match(m_displayName, @"[HL]\d").Value.Replace("H", "L");
                m_size = Regex.Match(m_displayName, @"\d[xX]\d").Value.ToLower();
            }

            m_displayName = Regex.Replace(m_displayName, @"_+", " ");
            m_displayName = Regex.Replace(m_displayName, @"(\d[xX]\d)|([HL]\d)|(\d+[\da-z])", "");
            m_displayName = Regex.Replace(m_displayName, @"\s\d+", " ");
            m_displayName = Regex.Replace(m_displayName, @"\s+", " ").Trim();

            m_displayName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(m_displayName);
        }
    }
}
