using UnityEngine;
using ColossalFramework.UI;
using ColossalFramework.Steamworks;

namespace BuildingThemes.GUI
{
    public class UIBuildingItem : UIPanel, IUIFastListRow
    {
        private UICheckBox m_name;
        private UISprite m_steamIcon;
        private UISprite m_category;
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

        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();

            if (m_name == null) return;

            background.width = width;
            m_size.relativePosition = new Vector3(width - 35f, 15);
            m_level.relativePosition = new Vector3(width - 65f, 15);
            m_category.relativePosition = new Vector3(width - 95f, 10);
        }

        protected override void OnMouseEnter(UIMouseEventParameter p)
        {
            base.OnMouseEnter(p);
            if (enabled) GetUIView().FindUIComponent<UIThemeManager>("BuildingThemes").UpdateBuildingInfo(m_building);
        }

        protected override void OnMouseWheel(UIMouseEventParameter p)
        {
            base.OnMouseWheel(p);
            if (enabled) GetUIView().FindUIComponent<UIThemeManager>("BuildingThemes").UpdateBuildingInfo(m_building);
        }

        private void SetupControls()
        {
            if (m_name != null) return;

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
            m_size.width = 30;
            m_size.textAlignment = UIHorizontalAlignment.Center;

            m_level = AddUIComponent<UILabel>();
            m_level.width = 30;
            m_level.textAlignment = UIHorizontalAlignment.Center;

            m_category = AddUIComponent<UISprite>();
            m_category.size = new Vector2(20, 20);
        }

        #region IUIFastListRow implementation
        public void Display(object data, bool isRowOdd)
        {
            SetupControls();

            float maxLabelWidth = width - 120;

            m_building = data as BuildingItem;
            m_name.text = m_building.displayName;
            if (m_building.prefab == null) m_name.text += " (Not Loaded)";
            m_name.label.textColor = m_building.GetStatusColor();
            m_name.label.isInteractive = false;
            m_name.isChecked = m_building.included;

            m_level.text = m_building.level;
            m_size.text = m_building.size;

            if (m_building.category != Category.None)
            {
                m_category.atlas = UIUtils.GetAtlas(CategoryIcons.atlases[(int)m_building.category]);
                m_category.spriteName = CategoryIcons.spriteNames[(int)m_building.category];
                m_category.tooltip = CategoryIcons.tooltips[(int)m_building.category];
                m_category.isVisible = true;
            }
            else
                m_category.isVisible = false;

            if(m_building.steamID != null)
            {
                m_steamIcon.tooltip = m_building.steamID;
                m_steamIcon.isVisible = true;

                maxLabelWidth -= 30;

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


            UIUtils.TruncateLabel(m_name.label, maxLabelWidth);
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
