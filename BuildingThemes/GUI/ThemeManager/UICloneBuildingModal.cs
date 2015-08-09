using ColossalFramework;
using ColossalFramework.UI;
using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace BuildingThemes.GUI
{
    public class UICloneBuildingModal : UIPanel
    {
        private UITitleBar m_title;
        private UITextField m_name;
        private UIDropDown m_level;
        private UIButton m_ok;
        private UIButton m_cancel;

        private BuildingItem m_item;
        private string m_cloneName;
        private int m_selectedLevel;

        private static UICloneBuildingModal _instance;

        public static UICloneBuildingModal instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = UIView.GetAView().AddUIComponent(typeof(UICloneBuildingModal)) as UICloneBuildingModal;
                }
                return _instance;
            }
        }

        public override void Start()
        {
            base.Start();

            backgroundSprite = "UnlockingPanel2";
            isVisible = false;
            canFocus = true;
            isInteractive = true;
            width = 350;

            // Title Bar
            m_title = AddUIComponent<UITitleBar>();
            m_title.title = "Clone Building";
            m_title.iconSprite = "ToolbarIconZoomOutCity";
            m_title.isModal = true;

            // Name
            UILabel name = AddUIComponent<UILabel>();
            name.height = 30;
            name.text = "Building name:";
            name.relativePosition = new Vector3(5, m_title.height);

            m_name = UIUtils.CreateTextField(this);
            m_name.width = width - 115;
            m_name.height = 30;
            m_name.padding = new RectOffset(6, 6, 6, 6);
            m_name.relativePosition = new Vector3(5, name.relativePosition.y + name.height + 5);

            m_name.Focus();
            m_name.eventTextChanged += (c, s) => CheckValidity();

            // Level
            m_level = UIUtils.CreateDropDown(this);
            m_level.width = 100;
            m_level.height = 30;
            (m_level.triggerButton as UIButton).textPadding = new RectOffset(6, 6, 6, 0);
            m_level.relativePosition = new Vector3(m_name.relativePosition.x + m_name.width + 5, m_name.relativePosition.y);

            m_level.eventSelectedIndexChanged += (c, i) => CheckValidity();

            // Ok
            m_ok = UIUtils.CreateButton(this);
            m_ok.text = "Clone";
            m_ok.isEnabled = false;
            m_ok.relativePosition = new Vector3(5, m_name.relativePosition.y + m_name.height + 5);

            m_ok.eventClick += (c, p) =>
            {
                UIThemeManager.instance.CloneBuilding(m_item, m_cloneName, m_selectedLevel);
                UIView.PopModal();
                Hide();
            };

            // Cancel
            m_cancel = UIUtils.CreateButton(this);
            m_cancel.text = "Cancel";
            m_cancel.relativePosition = new Vector3(width - m_cancel.width - 5, m_ok.relativePosition.y);

            m_cancel.eventClick += (c, p) =>
            {
                UIView.PopModal();
                Hide();
            };

            height = m_cancel.relativePosition.y + m_cancel.height + 5;
            relativePosition = new Vector3(Mathf.Floor((GetUIView().fixedWidth - width) / 2), Mathf.Floor((GetUIView().fixedHeight - height) / 2));

            isVisible = true;
        }

        private void CheckValidity()
        {
            if (!m_name.text.IsNullOrWhiteSpace())
            {
                int.TryParse(m_level.selectedValue.Replace("Level ", ""), out m_selectedLevel);

                string prefix = (m_item.isCloned) ? prefix = "{{" + m_item.building.baseName + "}}." : "{{" + m_item.name + "}}.";
                string suffix = " L" + m_selectedLevel + " " + UIThemeManager.instance.selectedBuilding.sizeAsString;

                m_cloneName = prefix + BuildingItem.CleanName(m_name.text) + suffix;

                m_ok.isEnabled = !UIThemeManager.instance.selectedTheme.containsBuilding(m_cloneName) && m_selectedLevel != m_item.level;

                if (m_ok.isEnabled && m_item.isCloned)
                {
                    BuildingItem baseItem = UIThemeManager.instance.GetBuildingItem(m_item.building.baseName);
                    m_ok.isEnabled = baseItem != null && baseItem.level != m_selectedLevel;
                }

                if (m_ok.isEnabled)
                    m_ok.tooltip = null;
                else
                    m_ok.tooltip = "Building already exists with that level";
            }
            else
            {
                m_ok.isEnabled = false;
                m_ok.tooltip = "Please enter a name";
            }

        }

        protected override void OnVisibilityChanged()
        {
            base.OnVisibilityChanged();

            UIComponent modalEffect = GetUIView().panelsLibraryModalEffect;

            if (isVisible)
            {
                if (UIThemeManager.instance.selectedBuilding == null)
                {
                    UIView.PopModal();
                    Hide();
                    return;
                }

                m_item = UIThemeManager.instance.selectedBuilding;

                // Name
                m_name.text = m_item.displayName;

                // Level
                int maxLevel = m_item.maxLevel;
                m_level.items = new string[0];
                for (int i = 1; i <= maxLevel; i++ ) m_level.AddItem("Level " + i);
                m_level.selectedIndex = (m_item.level < maxLevel) ? m_item.level : 0;

                CheckValidity();

                if (modalEffect != null)
                {
                    modalEffect.Show(false);
                    ValueAnimator.Animate("NewThemeModalEffect", delegate(float val)
                    {
                        modalEffect.opacity = val;
                    }, new AnimatedFloat(0f, 1f, 0.7f, EasingType.CubicEaseOut));
                }
            }
            else if (modalEffect != null)
            {
                ValueAnimator.Animate("NewThemeModalEffect", delegate(float val)
                {
                    modalEffect.opacity = val;
                }, new AnimatedFloat(1f, 0f, 0.7f, EasingType.CubicEaseOut), delegate
                {
                    modalEffect.Hide();
                });
            }
        }

        protected override void OnKeyDown(UIKeyEventParameter p)
        {
            if (Input.GetKey(KeyCode.Escape))
            {
                p.Use();
                UIView.PopModal();
                Hide();
            }

            base.OnKeyDown(p);
        }
    }
}
