﻿using UnityEngine;
using ColossalFramework.UI;

namespace BuildingThemes.GUI
{
    public class UIUtils
    {
        // Figuring all this was a pain (no documentation whatsoever)
        // So if your are using it for your mod consider thanking me (SamsamTS)
        // Extended Public Transport UI's code helped me a lot so thanks a lot AcidFire

        public static UIButton CreateButton(UIComponent parent)
        {
            UIButton button = (UIButton)parent.AddUIComponent<UIButton>();

            button.size = new Vector2(90f, 30f);
            button.textScale = 0.9f;
            button.normalBgSprite = "ButtonMenu";
            button.hoveredBgSprite = "ButtonMenuHovered";
            button.pressedBgSprite = "ButtonMenuPressed";
            button.canFocus = false;

            return button;
        }

        public static UICheckBox CreateCheckBox(UIComponent parent)
        {
            UICheckBox checkBox = (UICheckBox)parent.AddUIComponent<UICheckBox>();

            checkBox.width = 300f;
            checkBox.height = 20f;
            checkBox.clipChildren = true;

            UISprite sprite = checkBox.AddUIComponent<UISprite>();
            sprite.spriteName = "ToggleBase";
            sprite.size = new Vector2(16f, 16f);
            sprite.relativePosition = Vector3.zero;

            checkBox.checkedBoxObject = sprite.AddUIComponent<UISprite>();
            ((UISprite)checkBox.checkedBoxObject).spriteName = "ToggleBaseFocused";
            checkBox.checkedBoxObject.size = new Vector2(16f, 16f);
            checkBox.checkedBoxObject.relativePosition = Vector3.zero;

            checkBox.label = checkBox.AddUIComponent<UILabel>();
            checkBox.label.text = " ";
            checkBox.label.textScale = 0.9f;
            checkBox.label.relativePosition = new Vector3(22f, 2f);

            return checkBox;
        }

        public static UITextField CreateTextField(UIComponent parent)
        {
            UITextField textField = parent.AddUIComponent<UITextField>();

            textField.size = new Vector2(90f, 20f);
            textField.padding = new RectOffset(6, 6, 3, 3);
            textField.builtinKeyNavigation = true;
            textField.isInteractive = true;
            textField.readOnly = false;
            textField.horizontalAlignment = UIHorizontalAlignment.Center;
            textField.selectionSprite = "EmptySprite";
            textField.selectionBackgroundColor = new Color32(0, 172, 234, 255);
            textField.normalBgSprite = "TextFieldPanelHovered";
            textField.textColor = new Color32(0, 0, 0, 255);
            textField.disabledTextColor = new Color32(0, 0, 0, 128);
            textField.color = new Color32(255, 255, 255, 255);

            return textField;
        }

        public static void ResizeIcon(UISprite icon, Vector2 maxSize)
        {
            if (icon.height == 0) return;

            float ratio = icon.width / icon.height;

            if (icon.width > maxSize.x)
            {
                icon.width = maxSize.x;
                icon.height = maxSize.x / ratio;
            }

            if (icon.height > maxSize.y)
            {
                icon.height = maxSize.y;
                icon.width = maxSize.y * ratio;
            }
        }

        public static void DestroyDeeply(UIComponent component)
        {
            if (component == null) return;

            UIComponent[] children = component.GetComponentsInChildren<UIComponent>();

            if (children != null && children.Length > 0)
            {
                for (int i = 0; i < children.Length; i++)
                {
                    if (children[i].parent == component)
                        DestroyDeeply(children[i]);
                }
            }

            GameObject.Destroy(component);
        }
    }
}

