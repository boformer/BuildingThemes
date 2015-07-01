﻿using UnityEngine;
using ColossalFramework.UI;

namespace BuildingThemes.GUI
{
    public class UITitleBar : UIPanel
    {
        private UILabel m_title;
        private UIButton m_close;
        private UIDragHandle m_drag;

        public UIButton closeButton
        {
            get { return m_close; }
        }

        public string title
        {
            get { return m_title.text; }
            set { m_title.text = value; }
        }

        public override void Awake()
        {
            base.Awake();

            m_title = AddUIComponent<UILabel>();
            m_close = AddUIComponent<UIButton>();
            m_drag = AddUIComponent<UIDragHandle>();

            height = 40;
            width = 450;
            title = "(None)";
        }

        public override void Start()
        {
            base.Start();

            width = parent.width;
            relativePosition = Vector3.zero;
            isVisible = true;
            canFocus = true;
            isInteractive = true;

            m_drag.width = width - 50;
            m_drag.height = height;
            m_drag.relativePosition = Vector3.zero;
            m_drag.target = parent;


            m_title.relativePosition = new Vector3(50, 13);
            m_title.text = title;

            m_close.relativePosition = new Vector3(width - 35, 2);
            m_close.normalBgSprite = "buttonclose";
            m_close.hoveredBgSprite = "buttonclosehover";
            m_close.pressedBgSprite = "buttonclosepressed";
            m_close.eventClick += (component, param) => parent.Hide();
        }
    }
}
