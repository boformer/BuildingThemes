using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using ColossalFramework;
using ColossalFramework.Math;
using UnityEngine;

namespace BuildingThemes
{
    class CustomBuildingManager : BuildingManager
    {
        private static string[] privateFields = {
                                         "m_areaBuildingsDirty", 
                                         "m_serviceBuildings", 
                                         "m_outsideConnections", 
                                         "m_highlightMesh", 
                                         "m_highlightMaterial", 
                                         "m_buildingLayer", 
                                         "m_colorUpdateMin", 
                                         "m_colorUpdateMax", 
                                         "m_colorUpdateLock"};

        protected override void SimulationStepImpl(int subStep) 
        {
            for (int i = 0; i <  this.m_updatedBuildings.Length; i++)
            {
                ulong num2 = this.m_updatedBuildings[i];
                if (num2 != 0uL)
                {
                    for (int j = 0; j < 64; j++)
                    {
                        if ((num2 & 1uL << j) != 0uL)
                        {
                            ushort num3 = (ushort)(i << 6 | j);
                            Building b = this.m_buildings.m_buffer[(int)num3];

                            byte district = Singleton<DistrictManager>.instance.GetDistrict(b.m_position);
                            string districtName = Singleton<DistrictManager>.instance.GetDistrictName((int)district);

                            // TODO find out when a building is updated. Only on creation?
                            Debug.Log("Building updated: " + b.Info.name + ", district: " + districtName);
                        }
                    }
                }
            }
            base.SimulationStepImpl(subStep);
        }

        public void SetOriginalValues(BuildingManager originalManager) 
        {
	        // Copy public/protected fields
            this.m_buildingCount = originalManager.m_buildingCount;
	        this.m_infoCount = originalManager.m_infoCount;
	        this.m_abandonmentDisabled = originalManager.m_abandonmentDisabled;
	        this.m_firesDisabled = originalManager.m_firesDisabled;
	        this.m_buildings = originalManager.m_buildings;
	        this.m_updatedBuildings = originalManager.m_updatedBuildings;
	        this.m_buildingsUpdated = originalManager.m_buildingsUpdated;
	        this.m_buildingGrid = originalManager.m_buildingGrid;
	        this.m_buildingGrid2 = originalManager.m_buildingGrid2;
	        this.m_materialBlock = originalManager.m_materialBlock;
            this.m_audioGroup = originalManager.m_audioGroup;
	        this.m_common = originalManager.m_common;
	        this.m_buildingAbandoned1 = originalManager.m_buildingAbandoned1;
	        this.m_buildingAbandoned2 = originalManager.m_buildingAbandoned2;
            this.m_buildingOnFire = originalManager.m_buildingOnFire;
	        this.m_buildingBurned = originalManager.m_buildingBurned;
	        this.m_buildingLevelUp = originalManager.m_buildingLevelUp;
	        this.m_buildNextToRoad = originalManager.m_buildNextToRoad;
	        this.m_buildNextToWater = originalManager.m_buildNextToWater;
	        this.m_landfillSiteFull = originalManager.m_landfillSiteFull;
	        this.m_cemeteryFull = originalManager.m_cemeteryFull;
	        this.m_landfillSiteEmpty = originalManager.m_landfillSiteEmpty;
	        this.m_cemeteryEmpty = originalManager.m_cemeteryEmpty;
	        this.m_windTurbinePlacement = originalManager.m_windTurbinePlacement;
	        this.m_harborPlacement = originalManager.m_harborPlacement;
	        this.m_lastBuildingProblems = originalManager.m_lastBuildingProblems;
	        this.m_currentBuildingProblems = originalManager.m_currentBuildingProblems;
	        this.m_cityNameGroups = originalManager.m_cityNameGroups;
	        this.m_LevelUpWrapper = originalManager.m_LevelUpWrapper;
	        this.m_lodRgbAtlas = originalManager.m_lodRgbAtlas;
	        this.m_lodXysAtlas = originalManager.m_lodXysAtlas;
	        this.m_lodAciAtlas = originalManager.m_lodAciAtlas;

            this.m_properties = originalManager.m_properties;
            this.m_drawCallData = originalManager.m_drawCallData;

            // Copy private fields
            foreach(string field in privateFields) 
            {
                Type type = typeof(BuildingManager);

                FieldInfo info = type.GetField(field, BindingFlags.NonPublic | BindingFlags.Instance);

                info.SetValue(this, info.GetValue(originalManager));
            }
        }
    }
}
