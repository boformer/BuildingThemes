using UnityEngine;
using ColossalFramework.UI;

using System.Text;

namespace BuildingThemes.GUI
{
    public class UIThemePolicyItem : UIPanel, IUIFastListRow
    {
        private UIButton m_policyButton;
        private UICheckBox m_policyCheckBox;
        private UISprite m_sprite;

        private Configuration.Theme m_theme;

        public static bool showWarning
        {
            get
            {
                return BuildingThemesManager.instance.Configuration.ThemeValidityWarning;
            }
            set
            {
                if (BuildingThemesManager.instance.Configuration.ThemeValidityWarning == value) return;

                BuildingThemesManager.instance.Configuration.ThemeValidityWarning = value;
                BuildingThemesManager.instance.SaveConfig();
            }
        }

        private void SetupControls()
        {
            if (m_policyButton != null) return;

            isVisible = true;
            canFocus = true;
            isInteractive = true;
            backgroundSprite = "GenericPanel";
            size = new Vector2(364f, 44f);
            objectUserData = ToolsModifierControl.policiesPanel;

            m_policyButton = AddUIComponent<UIButton>();
            m_policyButton.name = "PolicyButton";
            m_policyButton.text = "Theme Name";
            m_policyButton.size = new Vector2(324f, 40f);
            m_policyButton.focusedBgSprite = "PolicyBarBackActive";
            m_policyButton.normalBgSprite = "PolicyBarBack";
            m_policyButton.relativePosition = new Vector3(2f, 2f, 0f);
            m_policyButton.textPadding.left = 50;
            m_policyButton.textColor = new Color32(0, 0, 0, 255);
            m_policyButton.disabledTextColor = new Color32(0, 0, 0, 255);
            m_policyButton.hoveredTextColor = new Color32(0, 0, 0, 255);
            m_policyButton.pressedTextColor = new Color32(0, 0, 0, 255);
            m_policyButton.focusedTextColor = new Color32(0, 0, 0, 255);
            m_policyButton.disabledColor = new Color32(124, 124, 124, 255);
            m_policyButton.dropShadowColor = new Color32(103, 103, 103, 255);
            m_policyButton.dropShadowOffset = new Vector2(1f, 1f);
            m_policyButton.textHorizontalAlignment = UIHorizontalAlignment.Left;
            m_policyButton.useDropShadow = false;
            m_policyButton.textScale = 0.875f;

            // This helper component updates the checkbox state every game tick
            m_policyButton.gameObject.AddComponent<ThemePolicyContainer>();

            m_policyCheckBox = m_policyButton.AddUIComponent<UICheckBox>();
            m_policyCheckBox.name = "Checkbox";
            m_policyCheckBox.size = new Vector2(363f, 44f);
            m_policyCheckBox.relativePosition = new Vector3(0f, -2f, 0f);
            m_policyCheckBox.clipChildren = true;

            // Connect the checkbox with our theme manager
            m_policyCheckBox.eventCheckChanged += delegate(UIComponent component, bool isChecked)
            {
                lock (component)
                {
                    var districtId = ToolsModifierControl.policiesPanel.targetDistrict;
                    var districtThemes = BuildingThemesManager.instance.GetDistrictThemes(districtId, true);

                    if (isChecked == districtThemes.Contains(m_theme)) return;

                    if (isChecked)
                    {
                        BuildingThemesManager.instance.EnableTheme(districtId, m_theme);
                    }
                    else
                    {
                        BuildingThemesManager.instance.DisableTheme(districtId, m_theme);
                    }
                }

            };

            // Checkbox-related UI components
            m_sprite = m_policyCheckBox.AddUIComponent<UISprite>();
            m_sprite.name = "Unchecked";
            m_sprite.spriteName = "ToggleBase";
            m_sprite.size = new Vector2(16f, 16f);
            m_sprite.relativePosition = new Vector3(336.6984f, 14, 0f);

            m_policyCheckBox.checkedBoxObject = m_sprite.AddUIComponent<UISprite>();
            m_policyCheckBox.checkedBoxObject.name = "Checked";
            ((UISprite)m_policyCheckBox.checkedBoxObject).spriteName = "ToggleBaseFocused";
            m_policyCheckBox.checkedBoxObject.size = new Vector2(16f, 16f);
            m_policyCheckBox.checkedBoxObject.relativePosition = Vector3.zero;
        }

        protected override void OnClick(UIMouseEventParameter p)
        {
            if(showWarning && m_policyCheckBox.isChecked && tooltip != null)
            {
                UIWarningModal.instance.message = "This theme might not work like expected:\n\n" + tooltip;
                UIView.PushModal(UIWarningModal.instance);
                UIWarningModal.instance.Show(true);
            }

            base.OnClick(p);
        }

        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();

            if (m_policyButton == null) return;

            m_policyButton.width = width - 39f;
            m_policyCheckBox.width = width - 1f;
            m_sprite.relativePosition = new Vector3(width - 27.3f, 14, 0f);
        }

        #region IUIFastListRow implementation
        public void Display(object data, bool isRowOdd)
        {
            SetupControls();

            m_theme = data as Configuration.Theme;
            m_policyButton.text = m_theme.name;
            m_policyCheckBox.objectUserData = m_theme;

            var districtId = ToolsModifierControl.policiesPanel.targetDistrict;
            var districtThemes = BuildingThemesManager.instance.GetDistrictThemes(districtId, true);
            m_policyCheckBox.isChecked = districtThemes.Contains(m_theme);

            if (UIThemeManager.instance != null)
            {
                string validityError = UIThemeManager.instance.ThemeValidityError(m_theme);
                tooltip = validityError;
            }
        }

        public void Select(bool isRowOdd) { }

        public void Deselect(bool isRowOdd) { }
        #endregion
    }
}
