using UnityEngine;
using ColossalFramework.UI;

namespace BuildingThemes.GUI
{
    public class UIThemeItem : UIPanel, IUIFastListRow
    {
        private UILabel m_name;
        private UIPanel m_background;

        private Configuration.Theme m_theme;

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

        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();

            background.width = width;
        }

        #region IUIFastListRow implementation
        public void Display(object data, bool isRowOdd)
        {
            if (m_name == null)
            {
                m_name = AddUIComponent<UILabel>();
                m_name.textScale = 0.9f;
                m_name.relativePosition = new Vector3(5, 13);
            }

            m_theme = data as Configuration.Theme;
            m_name.text = m_theme.name;
            UIUtils.TruncateLabel(m_name, parent.width - 30);

            string validityError = UIThemeManager.instance.ThemeValidityError(m_theme);

            m_name.textColor = (validityError == null) ? new Color32(255, 255, 255, 255) : new Color32(255, 255, 0, 255); ;
            tooltip = validityError;

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
            if (m_theme == null) return;

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
