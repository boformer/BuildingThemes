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
        private UITextField m_maxSpeed;
        private UIColorField m_color0;
        private UIColorField m_color1;
        private UIColorField m_color2;
        private UIColorField m_color3;
        private UITextField m_color0_hex;
        private UITextField m_color1_hex;
        private UITextField m_color2_hex;
        private UITextField m_color3_hex;
        private UICheckBox m_enabled;
        private UICheckBox m_addBackEngine;
        private UIButton m_clearVehicles;
        private UIButton m_clearParked;
        
        public override void Start()
        {
            base.Start();
            backgroundSprite = "UnlockingPanel2";
            isVisible = false;
            canFocus = true;
            isInteractive = true;
            width = 315;
            height = 330;
            relativePosition = new Vector3(Mathf.Floor((GetUIView().fixedWidth - width + 450) / 2), Mathf.Floor((GetUIView().fixedHeight - height) / 2));

            SetupControls();
        }

        private void SetupControls()
        {
            float offset = 40f;

            // Title Bar
            m_title = AddUIComponent<UITitleBar>();
            m_title.title = "Building Themes Manager";

            UIPanel panel = AddUIComponent<UIPanel>();
            panel.gameObject.AddComponent<UICustomControl>();

            panel.backgroundSprite = "UnlockingPanel";
            panel.width = width - 10;
            panel.height = height - offset - 75;
            panel.zOrder = 0;
            panel.relativePosition = new Vector3(5, offset);
        }
    }
}
