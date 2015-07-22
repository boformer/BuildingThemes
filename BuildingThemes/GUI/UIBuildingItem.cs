using System.Text.RegularExpressions;
using System.Globalization;

using UnityEngine;
using ColossalFramework.UI;
using ColossalFramework.Globalization;
using ColossalFramework.Steamworks;

namespace BuildingThemes.GUI
{
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

            if (m_displayName.Length > 30) m_displayName = m_displayName.Substring(0, 27) + "...";
        }
    }
    public class UIBuildingItem : UIPanel, IUIFastListRow
    {
        private UICheckBox m_name;
        private UISprite m_steamIcon;
        private UILabel m_level;
        private UILabel m_size;
        private UIPanel m_background;

        private BuildingItem m_building;

        public UIPanel background
        {
            get
            {
                if (m_background == null)
                {
                    m_background = AddUIComponent<UIPanel>();
                    m_background.width = width;
                    m_background.height = 40;
                    m_background.relativePosition = Vector2.zero;

                    m_background.zOrder = 0;
                }

                return m_background;
            }
        }

        protected override void OnMouseEnter(UIMouseEventParameter p)
        {
            base.OnMouseEnter(p);
            if (enabled) GetUIView().FindUIComponent<UIThemeManager>("BuildingThemes").UpdatePreview(m_building.prefab);
        }

        protected override void OnMouseWheel(UIMouseEventParameter p)
        {
            base.OnMouseWheel(p);
            if (enabled) GetUIView().FindUIComponent<UIThemeManager>("BuildingThemes").UpdatePreview(m_building.prefab);
        }

        private void SetupControls()
        {
            isVisible = true;
            canFocus = true;
            isInteractive = true;
            width = parent.width;
            height = 40;

            m_name = UIUtils.CreateCheckBox(this);
            m_name.width = 20;
            m_name.clipChildren = false;
            m_name.relativePosition = new Vector3(5, 13);

            m_name.eventCheckChanged += (c, state) =>
            {
                if (m_building != null)
                {
                    UIThemeManager.instance.ChangeBuildingStatus(m_building, state);
                }
            };

            m_steamIcon = m_name.AddUIComponent<UISprite>();
            m_steamIcon.spriteName = "SteamWorkshop";
            m_steamIcon.isVisible = false;
            m_steamIcon.relativePosition = new Vector3(22, 0);

            UIUtils.ResizeIcon(m_steamIcon, new Vector2(25, 25));

            if (Steam.IsOverlayEnabled())
            {
                m_steamIcon.eventClick += (c, p) =>
                {
                    p.Use();
                    Steam.ActivateGameOverlayToWorkshopItem(new PublishedFileId(ulong.Parse(m_building.steamID)));
                };
            }

            m_size = AddUIComponent<UILabel>();
            m_size.width = 50;
            m_size.textAlignment = UIHorizontalAlignment.Center;
            m_size.relativePosition = new Vector3(width - m_size.width, 15);

            m_level = AddUIComponent<UILabel>();
            m_level.width = 30;
            m_level.textAlignment = UIHorizontalAlignment.Center;
            m_level.relativePosition = new Vector3(width - m_level.width - m_size.width, 15);
        }

        #region IUIFastListRow implementation
        public void Display(object data, bool isRowOdd)
        {
            if (m_name == null) SetupControls();

            m_building = data as BuildingItem;
            m_name.text = m_building.displayName;
            m_name.label.textColor = m_building.GetStatusColor();
            m_name.label.isInteractive = false;
            m_name.isChecked = m_building.included;

            m_level.text = m_building.level;
            m_size.text = m_building.size;

            if(m_building.steamID != null)
            {
                m_steamIcon.tooltip = m_building.steamID;
                m_steamIcon.isVisible = true;

                m_name.label.relativePosition = new Vector3(52, 2);
            }
            else
            {
                m_steamIcon.isVisible = false;

                m_name.label.relativePosition = new Vector3(22, 2);
            }

            if (isRowOdd)
            {
                background.backgroundSprite = "UnlockingItemBackground";
                background.color = new Color32(0, 0, 0, 128);
            }
            else
            {
                background.backgroundSprite = null;
            }
        }

        public void Select(bool isRowOdd)
        {
            background.backgroundSprite = "ListItemHighlight";
            background.color = new Color32(255, 255, 255, 255);
        }

        public void Deselect(bool isRowOdd)
        {
            if (m_building == null) return;

            if (isRowOdd)
            {
                background.backgroundSprite = "UnlockingItemBackground";
                background.color = new Color32(0, 0, 0, 128);
            }
            else
            {
                background.backgroundSprite = null;
            }
        }
        #endregion
    }
}
