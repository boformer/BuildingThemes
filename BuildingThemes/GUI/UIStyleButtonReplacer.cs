using ColossalFramework.UI;
using UnityEngine;

namespace BuildingThemes.GUI
{
    public static class UIStyleButtonReplacer
    {
        public static void ReplaceStyleButton()
        {
            if (UIView.Find<UIDropDown>("ReplacementThemesButton") != null)
            {
                return;
            }
            var uiDropDown = UIView.Find<UIDropDown>("StyleDropdown");
            uiDropDown.Hide();
            var policiesButton = uiDropDown.parent.Find<UIButton>("PoliciesButton");
            var themesButton = policiesButton.parent.AddUIComponent<UIButton>();
            themesButton.name = "ReplacementThemesButton";
            themesButton.size = policiesButton.size;
            themesButton.text = "THEMES";
            themesButton.relativePosition = uiDropDown.relativePosition;
            themesButton.pressedBgSprite = policiesButton.pressedBgSprite;
            themesButton.disabledBgSprite = policiesButton.disabledBgSprite;
            themesButton.focusedBgSprite = policiesButton.focusedBgSprite;
            themesButton.hoveredBgSprite = policiesButton.hoveredBgSprite;
            themesButton.normalBgSprite = policiesButton.normalBgSprite;
            themesButton.clickSound = policiesButton.clickSound;
            themesButton.playAudioEvents = policiesButton.playAudioEvents;
            themesButton.eventClicked += (comp, param) =>
            {
                GameObject.Find("(Library) DistrictWorldInfoPanel").GetComponent<DistrictWorldInfoPanel>().OnPoliciesClick();
                UIView.Find<UIPanel>("PoliciesPanel").Find<UITabstrip>("Tabstrip").selectedIndex = 4;
            };
        }
    }
}