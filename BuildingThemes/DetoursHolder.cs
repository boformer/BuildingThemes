using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using ColossalFramework;
using ColossalFramework.Math;
using UnityEngine;

namespace BuildingThemes
{
 
    class DetoursHolder
    {

        //we'll use this variable to pass block to GetRandomBuildingInfo method
        private static ushort blockId; //TODO(earalov): maybe ZoneBlock
        

        //this is detoured version of BuildingManger#GetRandomBuildingInfo method. Note, that it's an instance method. It's better because this way all registers will be expected to have the same values
        //as in original methods
        public BuildingInfo GetRandomBuildingInfo(ref Randomizer r, ItemClass.Service service, ItemClass.SubService subService, ItemClass.Level level, int width, int length, BuildingInfo.ZoningMode zoningMode)
        {
            Debug.Log("Building Themes: Detoured GetRandomBuildingInfo was called.");
            //this part is the same as in original method
            var buildingManager = (BuildingManager)Convert.ChangeType(this, typeof(BuildingManager)); //I love this hack :)
            var refreshAreaBuidlings = typeof(BuildingManager).GetMethod("RefreshAreaBuildings", BindingFlags.NonPublic|BindingFlags.Instance);
            refreshAreaBuidlings.Invoke(buildingManager, new object[]{});
            var areaBuildings = (FastList<ushort>[])GetInstanceField(typeof(BuildingManager), buildingManager,
                "m_areaBuildings");
            var getAreaIndex = typeof(BuildingManager).GetMethod("GetAreaIndex", BindingFlags.NonPublic | BindingFlags.Static);

            FastList<ushort> fastList = areaBuildings[(int)getAreaIndex.Invoke(null, new object[] { service, subService, level, width, length, zoningMode })];
            if (fastList == null)
                return (BuildingInfo)null;
            if (fastList.m_size == 0)
                return (BuildingInfo)null;
           
            //TODO(earalov): here fastList variable should be filtered. All buildings that  don't belong to block's district should be removed
            int index = r.Int32((uint)fastList.m_size);
            return PrefabCollection<BuildingInfo>.GetPrefab((uint)fastList.m_buffer[index]);
        }

        internal static object GetInstanceField(Type type, object instance, string fieldName)
        {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                | BindingFlags.Static;
            FieldInfo field = type.GetField(fieldName, bindFlags);
            return field.GetValue(instance);
        }
    }
}
