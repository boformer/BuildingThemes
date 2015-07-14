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
        private UIBuildingFilter m_filter;
        private UIFastList m_themeSelection;
        private UIFastList m_buildingSelection;
        private UITextureSprite m_preview;
        private PreviewRenderer m_previewRenderer;

        #region Constant values
        private const float LEFT_WIDTH = 250;
        private const float MIDDLE_WIDTH = 400;
        private const float RIGHT_WIDTH = 250;
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
            width = SPACING + LEFT_WIDTH + SPACING + MIDDLE_WIDTH + SPACING + RIGHT_WIDTH + SPACING;
            height = TITLE_HEIGHT + HEIGHT + SPACING;
            relativePosition = new Vector3(Mathf.Floor((GetUIView().fixedWidth - width + 450) / 2), Mathf.Floor((GetUIView().fixedHeight - height) / 2));

            SetupControls();
        }

        private void SetupControls()
        {
            // Title Bar
            m_title = AddUIComponent<UITitleBar>();
            m_title.title = "Building Themes Manager";

            // Filter
            m_filter = AddUIComponent<UIBuildingFilter>();
            m_filter.width = width - SPACING * 2;
            m_filter.height = 40;
            m_filter.relativePosition = new Vector3(SPACING, TITLE_HEIGHT);

            m_filter.eventFilteringChanged += (c, i) =>
            {
                if (m_themeSelection != null && m_themeSelection.selectedIndex != -1)
                {
                    Configuration.Theme theme = m_themeSelection.rowsData[m_themeSelection.selectedIndex] as Configuration.Theme;
                    m_buildingSelection.rowsData = GetBuildingItemList(theme);
                }
            };

            // Panels
            UIPanel left = AddUIComponent<UIPanel>();
            left.width = LEFT_WIDTH;
            left.height = HEIGHT - SPACING - m_filter.height;
            left.relativePosition = new Vector3(SPACING, TITLE_HEIGHT + m_filter.height);

            UIPanel middle = AddUIComponent<UIPanel>();
            middle.width = MIDDLE_WIDTH;
            middle.height = HEIGHT - SPACING - m_filter.height;
            middle.relativePosition = new Vector3(LEFT_WIDTH + SPACING * 2, TITLE_HEIGHT + m_filter.height);

            UIPanel right = AddUIComponent<UIPanel>();
            right.width = RIGHT_WIDTH;
            right.height = HEIGHT - SPACING - m_filter.height;
            right.relativePosition = new Vector3(LEFT_WIDTH + MIDDLE_WIDTH + SPACING * 3, TITLE_HEIGHT + m_filter.height);

            // Theme selection
            m_themeSelection = UIFastList.Create<UIThemeItem>(left);

            m_themeSelection.backgroundSprite = "UnlockingPanel";
            m_themeSelection.width = left.width;
            m_themeSelection.height = left.height;
            m_themeSelection.canSelect = true;
            m_themeSelection.rowHeight = 40;
            m_themeSelection.relativePosition = Vector3.zero;

            m_themeSelection.rowsData.m_buffer = Singleton<BuildingThemesManager>.instance.GetAllThemes().ToArray();
            m_themeSelection.rowsData.m_size = m_themeSelection.rowsData.m_buffer.Length;
            Array.Sort(m_themeSelection.rowsData.m_buffer as Configuration.Theme[], ThemeCompare);

            m_themeSelection.DisplayAt(0);

            m_themeSelection.eventSelectedIndexChanged += (c, i) =>
            {
                Configuration.Theme theme = m_themeSelection.rowsData[i] as Configuration.Theme;

                m_buildingSelection.rowsData = GetBuildingItemList(theme);
                m_buildingSelection.DisplayAt(0);
            };

            // Building selection
            m_buildingSelection = UIFastList.Create<UIBuildingItem>(middle);

            m_buildingSelection.backgroundSprite = "UnlockingPanel";
            m_buildingSelection.width = middle.width;
            m_buildingSelection.height = middle.height;
            m_buildingSelection.canSelect = false;
            m_buildingSelection.rowHeight = 40;
            m_buildingSelection.relativePosition = Vector3.zero;

            m_buildingSelection.rowsData = new FastList<object>();

            // Preview
            UIPanel previewPanel = right.AddUIComponent<UIPanel>();
            previewPanel.backgroundSprite = "GenericPanel";
            previewPanel.width = right.width;
            previewPanel.height = previewPanel.width;
            previewPanel.relativePosition = Vector3.zero;

            m_preview = previewPanel.AddUIComponent<UITextureSprite>();
            m_preview.size = previewPanel.size;
            m_preview.relativePosition = Vector3.zero;

            m_previewRenderer = gameObject.AddComponent<PreviewRenderer>();
            m_previewRenderer.size = m_preview.size * 2; // Twice the size for anti-aliasing

            previewPanel.eventMouseDown += (c, p) =>
            {
                eventMouseMove += RotateCamera;
            };

            previewPanel.eventMouseUp += (c, p) =>
            {
                eventMouseMove -= RotateCamera;
            };

            previewPanel.eventMouseWheel += (c, p) =>
            {
                m_previewRenderer.zoom -= Mathf.Sign(p.wheelDelta) * 0.25f;
                m_previewRenderer.Render();
            };
        }

        public void UpdatePreview(BuildingInfo prefab)
        {
            if (prefab != null && prefab.m_mesh != null)
            {
                m_previewRenderer.cameraRotation = 120f;
                m_previewRenderer.zoom = 4f;
                m_previewRenderer.mesh = prefab.m_mesh;
                m_previewRenderer.material = prefab.m_material;
                m_previewRenderer.Render();
                m_preview.texture = m_previewRenderer.texture;
            }
            else
            {
                m_preview.texture = null;
            }
        }

        private Dictionary<string, BuildingItem> m_buildingDictionary;

        private FastList<object> GetBuildingItemList(Configuration.Theme theme)
        {
            if (theme == null) return null;

            // List of all growables prefabs
            if (m_buildingDictionary == null)
            {
                m_buildingDictionary = new Dictionary<string, BuildingItem>();
                for (uint i = 0; i < PrefabCollection<BuildingInfo>.PrefabCount(); i++)
                {
                    BuildingInfo prefab = PrefabCollection<BuildingInfo>.GetPrefab(i);
                    if (prefab != null && prefab.m_placementStyle == ItemClass.Placement.Automatic)
                    {
                        BuildingItem item = new BuildingItem();
                        item.prefab = PrefabCollection<BuildingInfo>.GetPrefab(i);
                        m_buildingDictionary.Add(item.name, item);
                    }
                }
            }
            else
            {
                // Reset building association
                foreach(BuildingItem item in m_buildingDictionary.Values)
                    item.building = null;
            }

            // Combine growables with buildings in configuration
            FastList<object> list = new FastList<object>();
            list.m_buffer = m_buildingDictionary.Values.ToArray();
            list.m_size = list.m_buffer.Length;

            Configuration.Building[] buildings = theme.buildings.ToArray();
            for(int i=0; i< buildings.Length; i++)
            {
                if(m_buildingDictionary.ContainsKey(buildings[i].name))
                {
                    // Associate building with prefab
                    BuildingItem item = m_buildingDictionary[buildings[i].name];
                    item.building = buildings[i];
                }
                else
                {
                    // Prefab not found, adding building without prefab
                    BuildingItem item = new BuildingItem();
                    item.building = buildings[i];
                    list.Add(item);
                }
            }

            // Filtering
            if (m_filter.buildingLevel == ItemClass.Level.None && m_filter.buildingSize == Vector2.zero && m_filter.buildingName.IsNullOrWhiteSpace() && m_filter.IsAllZoneSelected())
            {
                /*list.Trim();
                Array.Sort(list.m_buffer as BuildingItem[], BuildingCompare);*/
                return list;
            }

            FastList<object> filtered = new FastList<object>();
            for (int i = 0; i < list.m_size; i++)
            {
                BuildingItem item = (BuildingItem)list.m_buffer[i];
                bool prefabExists = item.prefab != null;

                // Level
                if (m_filter.buildingLevel != ItemClass.Level.None && !(prefabExists && item.prefab.m_class.m_level == m_filter.buildingLevel)) continue;

                // size
                Vector2 buildingSize = m_filter.buildingSize;
                if (m_filter.buildingSize != Vector2.zero && !(prefabExists && item.prefab.m_cellWidth == buildingSize.x && item.prefab.m_cellLength == buildingSize.y)) continue;

                // zone
                bool inZone = false;
                if(prefabExists)
                {
                    ItemClass itemClass = item.prefab.m_class;
                    if (m_filter.IsZoneSelected(UIBuildingFilter.Zone.ResidentialLow) && itemClass.m_subService == ItemClass.SubService.ResidentialLow) inZone = true;
                    if (m_filter.IsZoneSelected(UIBuildingFilter.Zone.ResidentialHigh) && itemClass.m_subService == ItemClass.SubService.ResidentialHigh) inZone = true;
                    if (m_filter.IsZoneSelected(UIBuildingFilter.Zone.CommercialLow) && itemClass.m_subService == ItemClass.SubService.CommercialLow) inZone = true;
                    if (m_filter.IsZoneSelected(UIBuildingFilter.Zone.CommercialHigh) && itemClass.m_subService == ItemClass.SubService.CommercialHigh) inZone = true;
                    if (m_filter.IsZoneSelected(UIBuildingFilter.Zone.Industrial) && itemClass.m_subService == ItemClass.SubService.IndustrialGeneric) inZone = true;
                    if (m_filter.IsZoneSelected(UIBuildingFilter.Zone.Farming) && itemClass.m_subService == ItemClass.SubService.IndustrialFarming) inZone = true;
                    if (m_filter.IsZoneSelected(UIBuildingFilter.Zone.Forestry) && itemClass.m_subService == ItemClass.SubService.IndustrialForestry) inZone = true;
                    if (m_filter.IsZoneSelected(UIBuildingFilter.Zone.Oil) && itemClass.m_subService == ItemClass.SubService.IndustrialOil) inZone = true;
                    if (m_filter.IsZoneSelected(UIBuildingFilter.Zone.Ore) && itemClass.m_subService == ItemClass.SubService.IndustrialOre) inZone = true;
                    if (m_filter.IsZoneSelected(UIBuildingFilter.Zone.Office) && itemClass.m_service == ItemClass.Service.Office) inZone = true;
                }

                if (!inZone && !m_filter.IsAllZoneSelected()) continue;

                // Name
                if (!m_filter.buildingName.IsNullOrWhiteSpace() && !item.name.ToLower().Contains(m_filter.buildingName.ToLower())) continue;

                filtered.Add(item);
            }

            /*filtered.Trim();
            Array.Sort(filtered.m_buffer as BuildingItem[], BuildingCompare);*/
            return filtered;
        }

        private void RotateCamera(UIComponent c, UIMouseEventParameter p)
        {
            m_previewRenderer.cameraRotation -= p.moveDelta.x / m_preview.width * 360f;
            m_previewRenderer.Render();
        }

        #region Sorting
        private static int ThemeCompare(Configuration.Theme a, Configuration.Theme b)
        {
            // Sort by name
            return a.name.CompareTo(b.name);
        }

        private static int BuildingCompare(BuildingItem a, BuildingItem b)
        {
            // Sort by name
            return a.name.CompareTo(b.name);
        }
        #endregion
    }
}
