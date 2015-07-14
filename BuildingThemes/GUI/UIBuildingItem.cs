using UnityEngine;
using ColossalFramework.UI;

namespace BuildingThemes.GUI
{
    public class BuildingItem
    {
        public BuildingInfo prefab;
        public Configuration.Building building;
        public bool included
        {
            get { return building != null; }
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

        public Color32 GetStatusColor()
        {
            if (prefab == null && building != null && building.isBuiltIn)
                return new Color32(255, 255, 0, 255);
            if (prefab == null)
                return new Color32(255, 0, 0, 255);

            return new Color32(255, 255, 255, 255);
        }
    }
    public class UIBuildingItem : UIPanel, IUIFastListRow
    {
        private UICheckBox m_name;
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

        public override void Start()
        {
            base.Start();

            isVisible = true;
            canFocus = true;
            isInteractive = true;
            width = parent.width;
            height = 40;
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

        #region IUIFastListRow implementation
        public void Display(object data, bool isRowOdd)
        {
            if (m_name == null)
            {
                m_name = UIUtils.CreateCheckBox(this);
                m_name.relativePosition = new Vector3(5, 13);
            }

            m_building = data as BuildingItem;
            m_name.text = m_building.name;
            m_name.label.textColor = m_building.GetStatusColor();
            m_name.isChecked = m_building.included;

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
