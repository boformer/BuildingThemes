using ColossalFramework;
using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BuildingThemes.GUI
{
    public class UIThemeManager : UIPanel
    {
        private UITitleBar m_title;
        private UITextField m_themeFilter;
        private UIFastList m_themeSelection;
        private UIPanel m_buildingSelection;

        #region Constant values
        private const float LEFT_WIDTH = 300;
        private const float RIGHT_WIDTH = 640;
        private const float HEIGHT = 500;
        private const float SPACING = 5;
        private const float TITLE_HEIGHT = 40;
        #endregion

        public override void Start()
        {
            base.Start();
            backgroundSprite = "UnlockingPanel2";
            isVisible = true;
            canFocus = true;
            isInteractive = true;
            width = SPACING + LEFT_WIDTH + SPACING + RIGHT_WIDTH + SPACING;
            height = TITLE_HEIGHT + HEIGHT + SPACING;
            relativePosition = new Vector3(Mathf.Floor((GetUIView().fixedWidth - width + 450) / 2), Mathf.Floor((GetUIView().fixedHeight - height) / 2));

            SetupControls();
        }

        private void SetupControls()
        {
            // Title Bar
            m_title = AddUIComponent<UITitleBar>();
            m_title.title = "Building Themes Manager";

            UIPanel left = AddUIComponent<UIPanel>();
            left.width = LEFT_WIDTH;
            left.height = HEIGHT;
            left.relativePosition = new Vector3(SPACING, TITLE_HEIGHT);

            // Theme selection
            m_themeFilter = UIUtils.CreateTextField(left);
            m_themeFilter.width = LEFT_WIDTH;
            m_themeFilter.height = 30;
            m_themeFilter.relativePosition = Vector3.zero;

            m_themeSelection = UIFastList.Create<UIThemeItem>(left);

            m_themeSelection.backgroundSprite = "UnlockingPanel";
            m_themeSelection.width = LEFT_WIDTH;
            m_themeSelection.height = HEIGHT - m_themeFilter.height - SPACING;
            m_themeSelection.canSelect = true;
            m_themeSelection.relativePosition = new Vector3(0, m_themeFilter.height + SPACING);

            m_themeSelection.rowsData.m_buffer = Singleton<BuildingThemesManager>.instance.GetAllThemes().ToArray();
            m_themeSelection.rowsData.m_size = m_themeSelection.rowsData.m_buffer.Length;
            m_themeSelection.rowHeight = 40;

            m_themeSelection.DisplayAt(0);
        }
    }
}
